using System;
using System.Collections;
using System.Collections.Generic;
using UMLClassDiag;
using UnityEngine;
using UnityEditor;
using System.IO;
using static JsonParser;
using System.Threading.Tasks;
using static SaverLoader;
using Unity.VisualScripting;



namespace UnityPusher
{

public class GameObjectCreator : GenerativeProcess{
    private static string prefabPath = "Assets/Prefabs";
    public static List<string> GameObjectNameList;
    private static Dictionary<string, object> jsonDictGOs;
    private static string prefabJsonStructure = 
    @"You are a Json Writer. You will follow this exact format with every value in between quotes :
    {
      ""gameObjects"": [
        {
          ""name"": ""string (nom du GameObject)"",
          ""tag"": ""string (tag du GameObject ou 'Untagged')"",
          ""layer"": ""string (layer du GameObject ou 'Default')"",
          ""components"": [
            {
              ""type"": ""string (nom du composant, ex: Transform, Rigidbody, Script)"",
              ""properties"": {
                ""property1"": ""value1 (clé-valeurs spécifiques au composant)"",
                ""property2"": ""value2""
              }
            }
            
          ]
        }
      ]
    }
    For type = Script, there is always a properties ""Name"" who must be an existing script name" + "\n";
    
    private static string inputToCreatePrefabs = 
    "Make a list of the game objects that are needed to execute the scripts presented in the UML below. Remember that the script names must be coherent with the UML scripts. \n";
    private static string generalInputForPrefabs;

    public GameObjectCreator(){
        MyEditorWindow.OnGenerateGameObjectEvent += OnGenerateGameObject;
        MyEditorWindow.OnGenerateGameObjectListEvent += GenerateGOs;
        Debug.Log("GameObject process initialized.");
    }

    ~GameObjectCreator(){
        MyEditorWindow.OnGenerateGameObjectEvent -= OnGenerateGameObject;
        MyEditorWindow.OnGenerateGameObjectListEvent -= GenerateGOs;
    }


    public static void GenerateGOs(){
        //Abonné à son event

        if (string.IsNullOrEmpty(jsonScripts)){
            throw new Exception("Generate UML for scripts before generate GO list.");
        }

        generalInputForPrefabs = prefabJsonStructure + inputToCreatePrefabs + jsonScripts;

        if (gptGenerator == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }

        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

        gptGenerator.GenerateFromText(generalInputForPrefabs, (response) =>
        {
            jsonGOs = response;
            SaveGOJson(jsonGOs);
            Debug.Log("Generated Prefabs JSON: \n" + jsonGOs);
            JsonToDictionary(jsonGOs);
            StockEveryGOsInList();
        });
    }

    public void OnGenerateGameObject(string GOname){
        //Abonnée à son event
        CreateGameObjectWithName(GOname);
        Debug.Log("Generation GameObject : " + GOname);
    }

    public static void JsonToDictionary(string json){
        if (string.IsNullOrEmpty(jsonGOs)){
            throw new Exception("Cannot create GO Dictionnary without GO Json: GOJson is Empty");
        }
        jsonDictGOs = (Dictionary<string, object>) Parse(jsonGOs);
    }

    public static void StockEveryGOsInList(){
        if (jsonDictGOs == null){
            throw new Exception("Dictionary of GameObjects no defined: Unable to make GameObjectList");
        }

        if (jsonDictGOs.ContainsKey("gameObjects") && jsonDictGOs["gameObjects"] is List<object> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject is Dictionary<string, object> gameObjectDict)
                {
                    if (gameObjectDict.ContainsKey("name") && gameObjectDict["name"] is string GameObjectName){
                        if (GameObjectNameList == null){
                            GameObjectNameList = new List<string>();
                        }
                        GameObjectNameList.Add(GameObjectName);
                    }
                }
            }
        }else{
            Debug.LogError("Le json est mal écrit à la racine. Le parser ne fonctionne pas!");
        }
    }

    public static void CreateGameObjectWithName(string GOName){
        if (jsonDictGOs == null){
            throw new Exception("Dictionary of GameObjects no defined: Unable to create GameObject");
        }

        if (jsonDictGOs.ContainsKey("gameObjects") && jsonDictGOs["gameObjects"] is List<object> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject is Dictionary<string, object> gameObjectDict)
                {
                    if (gameObjectDict.ContainsKey("name") && gameObjectDict["name"].ToString() == GOName){
                        CreateGameObject(gameObjectDict);
                    }
                }
            }
        }else{
            Debug.LogError("Le json est mal écrit à la racine. Le parser ne fonctionne pas!");
        }
    }

    public static void CreateEveryGameObjects(Dictionary<string, object> jsonDict){
        if (jsonDict.ContainsKey("gameObjects") && jsonDict["gameObjects"] is List<object> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject is Dictionary<string, object> gameObjectDict)
                {
                    CreateGameObject(gameObjectDict);
                }
            }
        }else{
            Debug.LogError("Le json est mal écrit à la racine. Le parser ne fonctionne pas!");
        }
    }


    public static void CreateGameObject(Dictionary<string, object> jsonDict){         
        string _name = jsonDict.ContainsKey("name") ? jsonDict["name"].ToString() : string.Empty;
        string _tag = jsonDict.ContainsKey("tag") ? jsonDict["tag"].ToString() : string.Empty;
        UsefulFunctions.EnsureTagExists(_tag);
        string _layer = jsonDict.ContainsKey("layer") ? jsonDict["layer"].ToString() : string.Empty;
        UsefulFunctions.EnsureLayerExists(_layer);
        GameObject go = new GameObject(_name)
        {
            tag = _tag,
            layer = LayerMask.NameToLayer(_layer)
        };

        if (jsonDict.ContainsKey("components") && jsonDict["components"] is List<object> componentsList)
        {
            AddComponentsToGO(componentsList, go);
        } 

        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
            Debug.Log("Dossier créé : " + prefabPath);
        }
        // Sauvegarder le GameObject en tant que prefab
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath + "/" + _name + ".prefab");

    }

    public static void AddComponentsToGO(List<object> components, GameObject go){

        foreach (object _component in components)
        {
            if (_component is Dictionary<string, object> componentDict)
            {
                string type = componentDict.ContainsKey("type") ? componentDict["type"].ToString() : string.Empty;

                Type componentType = Type.GetType($"UnityEngine.{type}, UnityEngine") ?? Type.GetType($"{type}, Assembly-CSharp");
                if (type == "Script"){
                    if  (componentDict.ContainsKey("properties") && componentDict["properties"] is Dictionary<string, object> propertiesDict){
                       if (propertiesDict.ContainsKey("Name") && propertiesDict["Name"] is string name){
                            type = name;
                            componentType = Type.GetType($"UnityEngine.{name}, UnityEngine") ?? Type.GetType($"{name}, Assembly-CSharp");
                       }
                    }
                }
                
                if (componentType != null)
                {
                    Component component;
                    // Ajoute le component au gameObject si ce n'est pas un transform (parce qu'il est deja dessus par défault).
                    if (componentType != typeof(Transform)){
                        component = go.AddComponent(componentType);
                    }else{
                        continue;
                    }

                    if (componentDict.ContainsKey("properties") && componentDict["properties"] is Dictionary<string, object> propertiesDict)
                    {
                        if (type != "Script")
                        {
                            AddPropertiesToComponent(propertiesDict, component, componentType);

                        }else{
                            AddFieldsToComponent(propertiesDict, component, componentType);
                        }
                    }
                }else
                {
                    Debug.LogWarning($"Composant non reconnu : {type}");
                }
            }
        }
    }

    public static void AddPropertiesToComponent(Dictionary<string, object> jsonDict, Component component, Type componentType){
        foreach (var kvp in jsonDict)
        {
            var propertyInfo = componentType.GetProperty(kvp.Key);
            if (propertyInfo == null){
                throw new Exception("Cette property n'est pas reconnu: " + kvp.Key);
            }

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                // Ecrit les properties dans le component
                propertyInfo.SetValue(component, Convert.ChangeType(kvp.Value, propertyInfo.PropertyType));
            }else{
                Debug.LogWarning($"Property déjà définit ou non éditable : {kvp.Key}");
            }
        }
    }

    public static void AddFieldsToComponent(Dictionary<string, object> jsonDict, Component component, Type componentType){
        foreach (var kvp in jsonDict)
        {
            if (kvp.Key == "Name"){
                continue;
            }

            var fieldInfo = componentType.GetField(kvp.Key);
            if (fieldInfo == null){
                throw new Exception("Ce field n'est pas reconnu: " + kvp.Key);
            }

            if (fieldInfo != null && fieldInfo.CanWrite())
            {
                // Ecrit les properties dans le component
                fieldInfo.SetValue(component, Convert.ChangeType(kvp.Value, fieldInfo.FieldType));
            }else{
                Debug.LogWarning($"Field déjà définit ou non éditable : {kvp.Key}");
            }
        }
    }
}

public static class UsefulFunctions{
    public static void EnsureTagExists(string tag)
    {
        #if UNITY_EDITOR
        // Vérifier si le tag existe déjà
        if (!TagExists(tag))
        {
            // Ajouter le tag dans la configuration de Unity
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProperty = tagManager.FindProperty("tags");

            // Trouver une entrée vide ou ajouter une nouvelle
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                SerializedProperty tagProperty = tagsProperty.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(tagProperty.stringValue))
                {
                    tagProperty.stringValue = tag;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"Tag '{tag}' ajouté dans une entrée vide.");
                    return;
                }
            }

            // Si aucune entrée vide, ajouter une nouvelle entrée
            tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
            tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"Tag '{tag}' ajouté comme nouvelle entrée.");
        }
        else
        {
            Debug.Log($"Tag '{tag}' existe déjà.");
        }
        #else
        Debug.LogError("Cette méthode fonctionne uniquement dans l'éditeur Unity.");
        #endif
    }

    private static bool TagExists(string tag)
    {
        return !string.IsNullOrEmpty(tag) && ArrayUtility.Contains(UnityEditorInternal.InternalEditorUtility.tags, tag);
    }

    public static void EnsureLayerExists(string layerName)
    {
        #if UNITY_EDITOR
        // Vérifier si le layer existe déjà
        if (!LayerExists(layerName))
        {
            // Ajouter le layer dans la configuration de Unity
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProperty = tagManager.FindProperty("layers");

            // Chercher une entrée vide pour le nouveau layer
            for (int i = 8; i < layersProperty.arraySize; i++) // Les couches 0-7 sont réservées
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerProperty.stringValue))
                {
                    layerProperty.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"Layer '{layerName}' ajouté dans une entrée vide.");
                    return;
                }
            }

            Debug.LogError("Aucune entrée vide trouvée pour ajouter un nouveau layer.");
        }
        else
        {
            Debug.Log($"Layer '{layerName}' existe déjà.");
        }
        #else
        Debug.LogError("Cette méthode fonctionne uniquement dans l'éditeur Unity.");
        #endif
    }

    private static bool LayerExists(string layerName)
    {
        for (int i = 0; i < 32; i++) // Unity supporte 32 layers (0-31)
        {
            if (layerName == LayerMask.LayerToName(i))
            {
                return true;
            }
        }
        return false;
    }
}

}