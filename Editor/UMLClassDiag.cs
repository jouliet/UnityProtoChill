using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ChatGPTWrapper;
using System.Text.RegularExpressions;
using static SaverLoader;
using System;
using Unity.VisualScripting.YamlDotNet.Core.Events;

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
    // On constitue la liste localement
    public BaseObject(){
        ObjectResearch.Add(this);
        //Debug.Log("Research system base objects list updated : " + this.Name);
    }


    public string Name { get; set; }
    public List<Attribute> Attributes { get; set; } = new List<Attribute>();
    public List<Method> Methods { get; set; } = new List<Method>();
    public List<BaseObject> ComposedClasses { get; set; } = new List<BaseObject>();
    public BaseObject ParentClass { get; set; }

    private FastUnityPusher _fastUnityPusher = new FastUnityPusher();

    // Appelé par UMLDiag ligne 110
    public void GenerateScript(GPTGenerator gptGenerator){

        
        GenerateScriptbis(gptGenerator);
        // Des trucs s'exécutent en parallèle voir le moment ou "oui" arrive (bien plus tôt que les debug de generate bis)

        //Debug.Log("oui");
    }
    
    public void Push(){
        // C'est sale mais ça marche
        _fastUnityPusher.CreateGameObjectAtZerosFromBaseObject(this);
    }
    
    
    
    // Bordel à refact dans une classe composée scriptGenerator ou dans unityPusher. Gaffe alors au timing de l'exécution de RefreshDatabase
    private void GenerateScriptbis(GPTGenerator gptGenerator)
    {
        //Peut etre précisé que la classe doit au moins indirectement hériter de mono behaviour 
        string input = 
        "You are in Unity, write this c# class :" + this.Name + "as described in this json : " + this.ToString() + @"You only use functions defined in the uml or native to Unity (Start and Update should be used to initialize and update objects over time).
Never assume a method, class or function exists unless specified in the uml. Relevant gameObjects and prefabs will always be named ObjectNameGO";
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
        string filename = this.Name + ".cs";
        string folderPath = "Assets/Scripts";
        string filePath = Path.Combine(folderPath, filename);
        // Vérifie si le dossier existe, sinon le crée
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Dossier créé : " + folderPath);
        }

        File.WriteAllText(filePath, ExtractCSharpCode(content));
        Debug.Log("Fichier créé : " + filePath);
        
        // REFRESH DATABASE ICI
        // Important que ce soit à la fin comme ça !!!! (pour des raisons obscures peut être multithread unity bizarre)
         #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    private string ExtractCSharpCode(string input)
    {
        // Expression régulière pour capturer le contenu entre ```csharp et ```
        var match = Regex.Match(input, @"```csharp\s*(.*?)\s*```", RegexOptions.Singleline);

        // Retourne le contenu correspondant ou un message vide
        return match.Success ? match.Groups[1].Value : "//Aucun script C# trouvé.";
    }

    public string RelanvantInfo(){
        string result = "";
        return result;
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

// Mapper auquel on accède seulement avec le namespace UMLClassDiag pr le moment mais à quoi bon vaudrait très probablement mieux le staticifier dans son coin comme pr le parser
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

public class NewJsonMapper{
    public static List<object> Classes;
    public static List<BaseObject> BaseObjects;
    public static BaseObject TrackBaseObjectByName(string className){
        foreach (BaseObject bo in BaseObjects){
            if (bo.Name == className){
                return bo;
            }
        }
        return null;
    }
    public static BaseObject MapPreciseBaseObject(List<object> jsonList, string className){
        var baseObject = TrackBaseObjectByName(className);
        if (baseObject != null){
            return baseObject;
        }
        throw new Exception("La classe " + className  + " n'existe pas.");
    }

    public static List<BaseObject> MapAllBaseObjects(Dictionary<string, object> jsonDict){
        if (BaseObjects == null){
            BaseObjects = new List<BaseObject>();
        }
        
        BaseObject bo;
        if (jsonDict == null){
            throw new Exception("Dictionary of classes no defined: Unable to create baseObjects");
        }

        if (jsonDict.ContainsKey("Classes") && jsonDict["Classes"] is List<object> classes)
        {
            if (Classes == null){
                Classes = (List<object>)jsonDict["Classes"];
            }
            foreach (var _class in classes) 
            {
                if (_class is Dictionary<string, object> classDict)
                {
                    bo = MapToBaseObject(classDict);
                }
            }
            AddComposedClasses(classes);
            return BaseObjects;
        }else{
            Debug.LogError("Le json est mal écrit à la racine. Le parser ne fonctionne pas!");
            return null;
        }
    }

    public static void AddComposedClasses(List<object> classes){
        foreach (var _class in classes) 
        {
            if (_class is Dictionary<string, object> _classDict)
            {
                string className = _classDict.ContainsKey("Name") ? _classDict["Name"].ToString() : string.Empty;
                BaseObject bo = TrackBaseObjectByName(className);
                if (bo == null){
                    throw new Exception("la classe " + className + " n'existe pas." );
                }
                bo.ComposedClasses = MapComposedClasses(_classDict);
            }
        }
    }

    public static BaseObject MapToBaseObject(Dictionary<string, object> jsonDict)
    {
        var baseObject = new BaseObject
        {
            Name = jsonDict.ContainsKey("Name") ? jsonDict["Name"].ToString() : string.Empty,
            Attributes = MapAttributes(jsonDict),
            Methods = MapMethods(jsonDict),
            // Les classes composées seront ajoutées après. Si on le fait tout de suite, 
            // ca fait une boucle infini.
        };
        BaseObjects.Add(baseObject);
    
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
            foreach (var composedClass in composedClasses)
            {
                if (composedClass is string composedClassName)
                {
                    var composedClassObject = MapPreciseBaseObject(Classes, composedClassName);
                    composedClassesList.Add(composedClassObject);
                }
            }
        }

        return composedClassesList;
    }
}

}