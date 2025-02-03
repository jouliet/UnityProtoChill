using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UMLClassDiag;
using System.Text.RegularExpressions;
using static UMLDiag;
using static ObjectResearch;
using System.Threading.Tasks;
public static class ScriptsCoherenceHandler 
{
    public static void UpdateBaseObjectsToMatchProject()
    {
        string folderPath = "Assets/Scripts";  // Dossier contenant les scripts C#
        
        // Vérifie que le dossier existe
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Le dossier " + folderPath + " n'existe pas.");
            return;
        }

        // Parcours tous les fichiers .cs dans le dossier Assets/Scripts
        string[] scriptFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (string scriptFile in scriptFiles)
        {
            // Extraire le nom du fichier sans l'extension ".cs"
            string fileName = Path.GetFileNameWithoutExtension(scriptFile);
            string scriptContent =scriptFile;
            // Vérifie si le fichier contient une classe avec le même nom que le fichier
            if (ClassExistsInScript(fileName, scriptContent))
            {
                // Crée un objet BaseObject avec le nom de la classe
                BaseObject newBaseObject = new BaseObject(fileName);
                newBaseObject.hasBeenGenerated = true;
                Debug.Log("BaseObject créé pour la classe : " + fileName);
                ExtractAttributesAndMethods(scriptContent, newBaseObject);
                // Vous pouvez maintenant utiliser newBaseObject comme vous le souhaitez
            }
        }

    }

    private static bool ClassExistsInScript(string className, string scriptFile)
    {
        // Ouvre le fichier et recherche la classe par son nom
        string scriptContent = File.ReadAllText(scriptFile);
        
        // Recherche une ligne contenant "class NomDeClasse"
        return scriptContent.Contains($"class {className}");
    }

    private static void ExtractAttributesAndMethods(string scriptFile, BaseObject baseObject)
    {
        string scriptContent = File.ReadAllText(scriptFile);

        // Extraction des attributs (en recherche des déclarations comme "public string Name;")
        var attributeMatches = Regex.Matches(scriptContent, @"(public|private|protected)\s+(\w+)\s+(\w+);");

        foreach (Match match in attributeMatches)
        {
            if (match.Groups.Count == 4)
            {
                string attributeName = match.Groups[3].Value;
                string attributeType = match.Groups[2].Value;
                baseObject.Attributes.Add(new UMLClassDiag.Attribute
                {
                    Name = attributeName,
                    Type = attributeType
                });
                //Classes composées ici
                BaseObject potentialComposedObject = ObjectResearch.FindBaseObjectByName(attributeType);
                if (potentialComposedObject != null)
                {
                    baseObject.ComposedClasses.Add(potentialComposedObject);
                }
            }
        }

        // Extraction des méthodes (en recherche des déclarations comme "public void MethodName()")
        var methodMatches = Regex.Matches(scriptContent, @"(public|private|protected)\s+(\w+)\s+(\w+)\s*\(([^)]*)\)");

        foreach (Match match in methodMatches)
        {
            if (match.Groups.Count >= 5)
            {
                var method = new Method
                {
                    Name = match.Groups[3].Value,
                    ReturnType = match.Groups[2].Value
                };

                // Extraction des paramètres de méthode (ex: (int param1, string param2))
                string parameters = match.Groups[4].Value;
                if (!string.IsNullOrEmpty(parameters))
                {
                    var parameterMatches = Regex.Matches(parameters, @"(\w+)\s+(\w+)");
                    foreach (Match paramMatch in parameterMatches)
                    {
                        if (paramMatch.Groups.Count == 3)
                        {
                            method.Parameters.Add(new UMLClassDiag.Attribute
                            {
                                Name = paramMatch.Groups[2].Value,
                                Type = paramMatch.Groups[1].Value
                            });
                        }
                    }
                }

                baseObject.Methods.Add(method);
            }
        }
    }
}
