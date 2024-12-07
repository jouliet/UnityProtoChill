using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ChatGPTWrapper;
namespace UMLClassDiag
{


public class Attribute
{
    public string Name { get; set; } 
    public string Type { get; set; } 
    public string DefaultValue { get; set; } 

    public override string ToString()
    {
        return $"Attribute Name: {Name}, Type: {Type}, Default Value: {DefaultValue ?? "None"}";
    }
}

public class Method
{
    public string Name { get; set; } 
    public string ReturnType { get; set; } 
    public List<Attribute> Parameters { get; set; } = new List<Attribute>(); 

    public override string ToString()
    {
        string parametersStr = Parameters.Count > 0 ? string.Join(", ", Parameters) : "None";
        return $"Method Name: {Name}, Return Type: {ReturnType ?? "None"}, Parameters: [{parametersStr}]";
    }
}

public class BaseObject
{
    public string Name { get; set; }
    public List<Attribute> Attributes { get; set; } = new List<Attribute>();
    public List<Method> Methods { get; set; } = new List<Method>();
    public List<BaseObject> ComposedClasses { get; set; } = new List<BaseObject>();
    public BaseObject ParentClass { get; set; }

    public void GenerateScript(GPTGenerator gptGenerator)
    {
        //Peut etre précisé que la classe doit surement hérité de mono behaviour 
        string input = 
        "Tu es dans Unity. Tu dois écrire la classe en c# selon ses composants comme décrit ici: " + this.ToString();

        Debug.Log(this.Name);

        if (gptGenerator == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }

        gptGenerator.GenerateFromText(input, (response) =>
        {
            Debug.Log("Generated class: " + response);
            WriteScriptFile(response);
            
        });
    }
    private void WriteScriptFile(string content)
    {
        // Chemin du dossier et du fichier
        string filename = "cac.cs";
        Debug.Log(filename);
        string folderPath = "Assets/Scripts";
        string filePath = Path.Combine(folderPath, filename);
        // Vérifie si le dossier existe, sinon le crée
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Dossier créé : " + folderPath);
        }

        // Vérifie si le fichier existe, sinon le crée
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, content);
            Debug.Log("Fichier créé : " + filePath);
        }
        // else
        // {
        //     // Si le fichier existe, écrase son contenu
        //     File.WriteAllText(filePath, content);
        //     Debug.Log("Fichier existant écrasé : " + filePath);
        // }

        // Rafraîchit l'éditeur Unity pour inclure le nouveau fichier
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    public override string ToString()
    {
        //string result = $"BaseObject : {Name}\n";
        string result = $"Name  - {Name}\n";
        result += "Attributes:\n";
        foreach (var attribute in Attributes)
        {
            result += $"  - {attribute}\n";
        }
        result += "Methods:\n";
        foreach (var method in Methods)
        {
            result += $"  - {method}\n";
        }
        if (ComposedClasses.Count > 0)
        {
            result += "Composed Classes:\n";
            foreach (var composedClass in ComposedClasses)
            {
                result += $"  - {composedClass.Name}\n";
            }
        }
        if (ParentClass != null)
        {
            result += $"Parent Class: {ParentClass.Name}\n";
        }
        return result;
    }
}

public class ObjectResearch
{
    public static BaseObject BaseObjectResearch(BaseObject root, string className)
    {
        string baseObjectName = "";
        BaseObject baseObject = null;
        if (baseObject.Name != baseObjectName){
            if (baseObject.ComposedClasses.Count > 0){
                foreach (var composedClass in baseObject.ComposedClasses){
                    baseObject = BaseObjectResearch(composedClass, className);
                    if (baseObject != null){
                        return baseObject;
                    }
                }
            }else{
                baseObject = null;
            }
        }else{
            return baseObject;
        }
        return baseObject;
    }

    public static List<BaseObject> BaseObjectList(BaseObject root){
        var baseObjectList = new List<BaseObject>{root};

        if (root.ComposedClasses.Count > 0)
        {
            foreach (var composedClass in root.ComposedClasses)
            {
                baseObjectList.AddRange(BaseObjectList(composedClass));
            }
            
        }
        return baseObjectList;
    }
}

public class JSONMapper
{
    public static BaseObject MapPreciseBaseObject(Dictionary<string, object> jsonDict, string className){
        var baseObject = new BaseObject
        {
            Name = jsonDict.ContainsKey(className) ? jsonDict[className].ToString() : string.Empty,
            Attributes = MapAttributes(jsonDict),
            Methods = MapMethods(jsonDict),
            ComposedClasses = MapComposedClasses(jsonDict)
        };
        return baseObject;
    }


    public static BaseObject MapToBaseObject(Dictionary<string, object> jsonDict)
    {
        var baseObject = new BaseObject
        {
            Name = jsonDict.ContainsKey("Name") ? jsonDict["Name"].ToString() : string.Empty,
            Attributes = MapAttributes(jsonDict),
            Methods = MapMethods(jsonDict),
            ComposedClasses = MapComposedClasses(jsonDict)
        };

        return baseObject;
    }

    private static List<Attribute> MapAttributes(Dictionary<string, object> jsonDict)
    {
        var attributesList = new List<Attribute>();

        if (jsonDict.ContainsKey("Attributes") && jsonDict["Attributes"] is List<object> attributes)
        {
            foreach (var attributeObj in attributes)
            {
                if (attributeObj is Dictionary<string, object> attributeDict)
                {
                    var attribute = new Attribute
                    {
                        Name = attributeDict.ContainsKey("Name") ? attributeDict["Name"].ToString() : string.Empty,
                        Type = attributeDict.ContainsKey("Type") ? attributeDict["Type"].ToString() : string.Empty,
                        DefaultValue = attributeDict.ContainsKey("DefaultValue") ? attributeDict["DefaultValue"]?.ToString() : null
                    };

                    attributesList.Add(attribute);
                }
            }
        }

        return attributesList;
    }

    private static List<Method> MapMethods(Dictionary<string, object> jsonDict)
    {
        var methodsList = new List<Method>();

        if (jsonDict.ContainsKey("Methods") && jsonDict["Methods"] is List<object> methods)
        {
            foreach (var methodObj in methods)
            {
                if (methodObj is Dictionary<string, object> methodDict)
                {
                    var method = new Method
                    {
                        Name = methodDict.ContainsKey("Name") ? methodDict["Name"].ToString() : string.Empty,
                        ReturnType = methodDict.ContainsKey("ReturnType") ? methodDict["ReturnType"].ToString() : string.Empty,
                        Parameters = MapAttributes(methodDict)
                    };

                    methodsList.Add(method);
                }
            }
        }

        return methodsList;
    }

    private static List<BaseObject> MapComposedClasses(Dictionary<string, object> jsonDict)
    {
        var composedClassesList = new List<BaseObject>();

        if (jsonDict.ContainsKey("ComposedClasses") && jsonDict["ComposedClasses"] is List<object> composedClasses)
        {
            foreach (var composedClassObj in composedClasses)
            {
                if (composedClassObj is Dictionary<string, object> composedClassDict)
                {
                    var composedClass = MapToBaseObject(composedClassDict);
                    composedClassesList.Add(composedClass);
                }
            }
        }

        return composedClassesList;
    }
}

}