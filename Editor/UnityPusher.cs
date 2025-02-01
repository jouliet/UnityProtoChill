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
using System.Globalization;
using System.Security.Permissions;
using UnityEngine.UI;
using System.Numerics;



namespace UnityPusher
{

public class GameObjectCreator : GenerativeProcess {
    public static string prefabPath = "Assets/Prefabs";
    public static List<string> GameObjectNameList;
    private static Dictionary<string, object> jsonDictGOs;

    public static void CreateAllGameObjects(){
        foreach(string goName in GameObjectNameList){
            CreateGameObjectWithName(goName);
        }
    }

    public static void JsonToDictionary(string json){
        jsonGOs = json;
        if (string.IsNullOrEmpty(jsonGOs)){
            throw new Exception("Cannot create GO Dictionnary without GO Json: GOJson is Empty");
        }
        jsonDictGOs = (Dictionary<string, object>) Parse(jsonGOs);
    }

    public static void StockEveryGOsInList(){
        if (jsonDictGOs == null){
            throw new Exception("Dictionary of GameObjects no defined: Unable to make GameObjectList");
        }

        if (jsonDictGOs.ContainsKey("GameObjects") && jsonDictGOs["GameObjects"] is List<object> gameObjects)
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

        if (jsonDictGOs.ContainsKey("GameObjects") && jsonDictGOs["GameObjects"] is List<object> gameObjects)
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
        if (jsonDict.ContainsKey("GameObjects") && jsonDict["GameObjects"] is List<object> gameObjects)
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
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath + "/" + _name + ".prefab");
        GameObject.DestroyImmediate(go);
        PrefabUtility.InstantiatePrefab(prefab);
    }

    public static void AddComponentsToGO(List<object> components, GameObject go){

        foreach (object _component in components)
        {
            AddComponentToGO(_component, go);
        }
    }

    public static void AddComponentToGO(object _component, GameObject go){
        if (_component is Dictionary<string, object> componentDict)
            {
                string type = componentDict.ContainsKey("type") ? componentDict["type"].ToString() : string.Empty;

                Type componentType = Type.GetType($"UnityEngine.{type}, UnityEngine") ?? Type.GetType($"{type}, Assembly-CSharp");
                if (type == "Script"){
                    if  (componentDict.ContainsKey("properties") && componentDict["properties"] is Dictionary<string, object> propertiesDict){
                       if (propertiesDict.ContainsKey("Name") && propertiesDict["Name"] is string name){
                            componentType = Type.GetType($"UnityEngine.{name}, UnityEngine") ?? Type.GetType($"{name}, Assembly-CSharp");
                            // Debug.Log("Ajout du gameObject " + go.name + " à la liste GameObjectAttachedTo au baseObject " + name);
                            // BaseObject bo = JsonMapper.TrackBaseObjectByName(name);
                            // bo.GameObjectAttachedTo.Add(go.name);
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
                        component = go.transform;
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
                }else if (type != "Script")
                {
                    Debug.LogWarning($"Composant non reconnu : {type}");
                }
            }
    }

    public static void AddPropertiesToComponent(Dictionary<string, object> jsonDict, Component component, Type componentType){
        foreach (var kvp in jsonDict)
        {
            var propertyInfo = componentType.GetProperty(kvp.Key);
            try 
            {
                if (propertyInfo == null){
                    throw new Exception("Cette property n'est pas reconnu: " + kvp.Key + " : " + kvp.Value);
                }

                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    // Ecrit les properties dans le component
                    //Debug.Log("type de " + kvp.Value + " : " + kvp.Value.GetType());
        
                    object value = kvp.Value;
                    Type propertyType = propertyInfo.PropertyType;

                    // Gestion spécifique des floats
                    if (propertyType == typeof(float) || propertyType == typeof(Single))
                    {
                        value = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                        propertyInfo.SetValue(component, value);
                    }else if (kvp.Key == "mesh"){
                        SetMeshFilterFromString((string)kvp.Value, (MeshFilter)component);
                    }else if (kvp.Key == "material"){
                        // Ici la kvp.Value ne change rien car on applique un material par default
                        SetMeshRendererFromString((MeshRenderer)component);
                    }else if (propertyType == typeof(UnityEngine.Vector3)){
                        
                        List<object> vector3ListJson = (List<object>) kvp.Value;
                        List<float> vector3List = new List<float>();
                        foreach(object number in vector3ListJson){
                            vector3List.Add(Convert.ToSingle(number, CultureInfo.InvariantCulture));
                        }
                        UnityEngine.Vector3 vector3 = new UnityEngine.Vector3(vector3List[0], vector3List[1], vector3List[2]);
                        propertyInfo.SetValue(component, vector3);
                    }
                    else
                    {
                        value = Convert.ChangeType(value, propertyType);
                        propertyInfo.SetValue(component, value);
                    }
                    
                    //Debug.LogException(ex);
                }else{
                    Debug.LogWarning($"Property déjà définit ou non éditable : {kvp.Key}");
                }
            }catch (Exception ex){
                    Debug.LogWarning("Exeption for property value : " + kvp.Value  + "\n Exception : " + ex);
            }
            
        }
    }

    public static void SetMeshFilterFromString(string primitiveTypeName, MeshFilter meshFilter)
    {
        if (Enum.TryParse(primitiveTypeName, out PrimitiveType primitiveType))
        {
            GameObject temp = GameObject.CreatePrimitive(primitiveType);
            meshFilter.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(temp);
        }
        else
        {
            Debug.LogError($"PrimitiveType '{primitiveTypeName}' invalide.");
        }
    }


    public static void SetMeshRendererFromString(MeshRenderer meshRenderer)
    {
      
        //Material material = Resources.GetBuiltinResource<Material>(materialName + ".mat");
        Material DefaultMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");

        DefaultMaterial.SetInt("_Smoothness", 0);
        meshRenderer.material = DefaultMaterial;
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
                try{
                    object value = kvp.Value;
                    Type fieldType = fieldInfo.FieldType;

                    // Gestion spécifique des floats
                    if (fieldType == typeof(float) || fieldType == typeof(Single))
                    {
                        value = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                    }else if (fieldType == typeof(GameObject)){
                        value = Resources.Load<GameObject>("Prefabs/" + kvp.Value);
                        if (value == null){
                            throw new Exception("Il n'existe pas de prefab " + kvp.Value + "." );
                        }
                    }
                    else
                    {
                        value = Convert.ChangeType(value, fieldType);
                    }
                    fieldInfo.SetValue(component, value);
                }catch( Exception ex){
                    Debug.LogWarning("Exeption for field value : " + kvp.Value + " With asked type: " + fieldInfo.FieldType + "\nException : " +  ex);
                } 
                
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
            //Debug.Log($"Tag '{tag}' existe déjà.");
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
            //Debug.Log($"Layer '{layerName}' existe déjà.");
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