using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ChatGPTWrapper;
using System.Text.RegularExpressions;
using static SaverLoader;
using static ObjectResearch;
using System.Linq;
using System;
using Unity.VisualScripting.YamlDotNet.Core.Events;
using static JsonParser;
using UnityPusher;
using Unity.VisualScripting;

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
    }

    ~BaseObject(){
        AllBaseObjects.Remove(this);
    }
 
    public override bool Equals(object obj)
    {
        if (obj is BaseObject other)
        {
            return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    // Implémenter GetHashCode pour garantir que l'égalité fonctionne dans les collections
    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }

    public string Name { get; set; }
    public List<Attribute> Attributes { get; set; } = new List<Attribute>();
    public List<Method> Methods { get; set; } = new List<Method>();
    public List<BaseObject> ComposedClasses { get; set; } = new List<BaseObject>();
    public BaseObject ParentClass { get; set; }
    public List<string> GameObjectAttachedTo { get; set; } = new List<string>();

    // Appelé par UMLDiag ligne 110
    public void GenerateScript(){
        GenerateScriptbis();
        // Des trucs s'exécutent en parallèle voir le moment ou "oui" arrive (bien plus tôt que les debug de generate bis)

        //Debug.Log("oui");
    }
      
    // Bordel à refact dans une classe composée scriptGenerator ou dans unityPusher. Gaffe alors au timing de l'exécution de RefreshDatabase
    private void GenerateScriptbis()
    {
        //Peut etre précisé que la classe doit au moins indirectement hériter de mono behaviour 
        string input = 
        "You are in Unity, write this c# class :" + this.Name + "as described in this json : " + this.ToString() + @"You only use functions defined in the uml or native to Unity (Start and Update should be used to initialize and update objects over time).
Never assume a method, class or function exists unless specified in the uml. Relevant gameObjects and prefabs will always be named ObjectNameGO. Use ```csharp marker. ";
        if (UMLDiag.gptGenerator == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }
        UMLDiag.gptGenerator.GenerateFromText(input, (response) =>
        {
            Debug.Log("Generated class: " + response);
            WriteScriptFile(GetScript(response));
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
        //LoadUML();
        #endif
    }


    public static void AddScriptToGameObject(string prefabPath, string scriptName)
    {
        // Charger le prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            //Debug.LogError("Prefab introuvable : " + prefabPath);
            return;
        }

        // Obtenir le type du script
        Type scriptType = Type.GetType($"{scriptName}, Assembly-CSharp");
        if (scriptType == null)
        {
            //Debug.LogError("Script introuvable : " + scriptName);
            return;
        }

        // Instancier temporairement le prefab pour modifier ses composants
        GameObject tempInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (tempInstance.GetComponent(scriptType) == null) // Éviter les doublons
        {
            tempInstance.AddComponent(scriptType);
        }


        // Appliquer les modifications au prefab
        PrefabUtility.SaveAsPrefabAsset(tempInstance, prefabPath);
        GameObject.DestroyImmediate(tempInstance); // Nettoyage

        Debug.Log($"Script {scriptName} ajouté au prefab {prefabPath}.");
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

public class JsonMapper{
    // List of Dictionary object of classes
    public static List<object> Classes;

    // List of baseobjects classes
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
        if (className != "null")
        {
            Debug.LogError("La classe " + className  + " n'existe pas.");
        }
        
        return null;
    }

    public static List<BaseObject> MapAllBaseObjects(Dictionary<string, object> jsonDict){
        BaseObjects = new List<BaseObject>();
        
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
                    bo = MapToBaseObject(classDict, jsonDict);

                    foreach (string goName in bo.GameObjectAttachedTo)
                    {
                        // Debug.Log("Le script " + bo.Name + "s'ajoute au gameobject " + goName);
                        BaseObject.AddScriptToGameObject(GameObjectCreator.prefabPath + "/" + goName + ".prefab", bo.Name);
                    }
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
                    throw new Exception("La classe " + className + " n'existe pas." );
                }
                bo.ComposedClasses = MapComposedClasses(_classDict);
            }
        }
    }

    public static BaseObject MapToBaseObject(Dictionary<string, object> classDict, Dictionary<string,object> jsonDict)
    {
        var baseObject = new BaseObject
        {
            Name = classDict.ContainsKey("Name") ? classDict["Name"].ToString() : string.Empty,
            Attributes = MapAttributes(classDict),
            Methods = MapMethods(classDict),
            // Les classes composées seront ajoutées après. Si on le fait tout de suite, 
            // ca fait une boucle infini.
        };
        baseObject.GameObjectAttachedTo = MapGameObjectAttachedTo(jsonDict, baseObject.Name);

        BaseObjects.Add(baseObject);
    
        return baseObject;
    }

    public static List<string> MapGameObjectAttachedTo(Dictionary<string, object> jsonDict, string boname){
        List<string> list = new List<string>();
        if (jsonDict.ContainsKey("GameObjects") && jsonDict["GameObjects"] is List<object> classes)
        {
            foreach(object obj in classes)
            {
                if (obj is Dictionary<string,object> gameObjectDict){
                    if (gameObjectDict.ContainsKey("components") && gameObjectDict["components"] is List<object> componentList){
                        foreach (object component in componentList){
                            if (component is Dictionary<string,object> componentDict){
                                if (componentDict.ContainsKey("properties") && componentDict["properties"] is Dictionary<string,object> propertiesDict)
                                {
                                    if (propertiesDict.ContainsKey("Name") && componentDict["type"] is string type)
                                    {
                                        string scriptName = propertiesDict["Name"].ToString();
                                        if (scriptName == boname && type == "Script")
                                        {
                                            string goname = gameObjectDict["name"].ToString();
                                            //Debug.Log("Le gameobject " + goname + " est ajouté au baseobject " + boname);
                                            list.Add(goname);
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return list;
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
                    if (composedClassObject == null){
                        continue;
                    }
                    composedClassesList.Add(composedClassObject);
                }
            }
        }

        return composedClassesList;
    }
}

}