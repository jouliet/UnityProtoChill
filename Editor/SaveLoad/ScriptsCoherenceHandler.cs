using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UMLClassDiag;
using System.Text.RegularExpressions;
using static UMLDiag;
using static ObjectResearch;
using System.Threading.Tasks;
using System.Collections.Generic;   
public static class ScriptsCoherenceHandler 
{
    public static void UpdateBaseObjectsToMatchProject()
    {
        string folderPath = "Assets/Scripts";  
        
        if (!Directory.Exists(folderPath))
        { 
            Debug.LogWarning("No Scripts folder detected");
            return;
        }

        string[] scriptFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (string scriptFile in scriptFiles)
        {
            string scriptContent =scriptFile;

            Dictionary<string, string> classData = ExtractClassNamesAndContent(scriptFile);

            foreach (var entry in classData)
            {
                string className = entry.Key;     
                string classContent = entry.Value;   

                BaseObject newBaseObject = new BaseObject(className);
                newBaseObject.hasBeenGenerated = true;
                
                ExtractAttributesAndMethods(classContent, newBaseObject);

                newBaseObject.scriptFile = classContent;
            }
        }
    }
private static string ExtractClassContent(string scriptContent, int classStartIndex)
{
    int openBraces = 0;
    int endIndex = classStartIndex;
    bool firstAccoladeEncountered = false;

    // Start scanning from the given classStartIndex, which is the position of the class declaration
    for (int i = classStartIndex; i < scriptContent.Length; i++)
    {
        if (scriptContent[i] == '{')
        {
            firstAccoladeEncountered = true;
            openBraces++;  // Increment when encountering an opening brace
        }
        else if (scriptContent[i] == '}')
        {
            openBraces--; // Decrement when encountering a closing brace
        }

        // When openBraces reaches zero, we've closed the class block
        if (openBraces == 0 && i > classStartIndex && firstAccoladeEncountered)
        {
            endIndex = i;  // Record the position where the class ends
            break;  // Exit loop when we've found the end of the class
        }
    }

    // Ensure that the class was properly closed
    if (openBraces != 0)
    {
        Debug.LogError("Class block was not properly closed.");
        return string.Empty; // Return an empty string if the class is malformed
    }

    // Extract content from classStartIndex to the closing brace
    return scriptContent.Substring(classStartIndex, endIndex - classStartIndex + 1);
}

private static Dictionary<string, string> ExtractClassNamesAndContent(string scriptFile)
{
    string scriptContent = File.ReadAllText(scriptFile);
    Dictionary<string, string> classData = new Dictionary<string, string>();

    MatchCollection matches = Regex.Matches(scriptContent, 
        @"\b(class|struct)\s+(\w+)\s*(?::\s*(\w+))?\s*{", RegexOptions.Singleline);

    foreach (Match match in matches)
    {
        string className = match.Groups[2].Value;      // The class or struct name
        string parentClass = match.Groups[3].Success ? match.Groups[3].Value : null;  // The parent class (if any)

        int classStartIndex = match.Index;
        
        string classContent = ExtractClassContent(scriptContent, classStartIndex);
        
        classData[className] = classContent;
    }

    return classData;
}

    private static void ExtractAttributesAndMethods(string scriptContent, BaseObject baseObject)
{

    // Regular expression to match attributes like: public float speed;
    var attributeMatches = Regex.Matches(scriptContent, @"\s*(public|private|protected|internal|)\s*(\w+)\s+(\w+);");

    foreach (Match match in attributeMatches)
    {
        if (match.Groups.Count == 4)
        {
            string accessModifier = match.Groups[1].Value;
            string attributeType = match.Groups[2].Value;
            string attributeName = match.Groups[3].Value;

            baseObject.Attributes.Add(new UMLClassDiag.Attribute
            {
                Name = attributeName,
                Type = attributeType
            });
        }
    }

    // Regular expression to match methods like: void Start(), public void Move()
    var methodMatches = Regex.Matches(scriptContent, @"\s*(public|private|protected|internal|)\s*(\w+)\s+(\w+)\s*\(([^)]*)\)\s*{");

    foreach (Match match in methodMatches)
    {
        if (match.Groups.Count >= 5)
        {
            string accessModifier = match.Groups[1].Value;
            string returnType = match.Groups[2].Value;
            string methodName = match.Groups[3].Value;
            string parameters = match.Groups[4].Value;

            var method = new Method
            {
                Name = methodName,
                ReturnType = returnType
            };

            // Extract parameters (e.g., (int param1, string param2))
            if (!string.IsNullOrEmpty(parameters))
            {
                var parameterMatches = Regex.Matches(parameters, @"\s*(\w+)\s+(\w+)\s*");
                foreach (Match paramMatch in parameterMatches)
                {
                    if (paramMatch.Groups.Count == 3)
                    {
                        string paramType = paramMatch.Groups[1].Value;
                        string paramName = paramMatch.Groups[2].Value;

                        method.Parameters.Add(new UMLClassDiag.Attribute
                        {
                            Name = paramName,
                            Type = paramType
                        });
                    }
                }
            }

            baseObject.Methods.Add(method);
        }
    }
}

}
