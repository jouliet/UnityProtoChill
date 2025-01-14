using System;
using System.Collections;
using System.Collections.Generic;
using UMLClassDiag;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityPusher
{

public static class GameObjectCreator{
    static string prefabPath = "Assets/Prefabs";


    public static void MapEveryGameObjects(Dictionary<string, object> jsonDict){
        if (jsonDict.ContainsKey("gameObjects") && jsonDict["gameObjects"] is List<object> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject is Dictionary<string, object> gameObjectDict)
                {
                    MapToGameObject(gameObjectDict);
                }
            }
        }else{
            Debug.LogError("Le json est mal écrit à la racine. Le parser ne fonctionne pas!");
        }
    }


    public static void MapToGameObject(Dictionary<string, object> jsonDict){         
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
            MapToComponent(componentsList, go);
        } 

        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
            Debug.Log("Dossier créé : " + prefabPath);
        }
        // Sauvegarder le GameObject en tant que prefab
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath + "/" + _name + ".prefab");
    }

    public static void MapToComponent(List<object> components, GameObject go){

        foreach (object _component in components)
        {
            if (_component is Dictionary<string, object> componentDict)
            {
                string type = componentDict.ContainsKey("type") ? componentDict["type"].ToString() : string.Empty;
                Type componentType = Type.GetType($"UnityEngine.{type}, UnityEngine") ?? Type.GetType($"{type}, Assembly-CSharp");
                
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
                        MapToProperties(propertiesDict, component, componentType);
                    }
                }else
                {
                    Debug.LogWarning($"Composant non reconnu : {type}");
                }
            }
        }
    }

    public static void MapToProperties(Dictionary<string, object> jsonDict, Component component, Type componentType){
        foreach (var kvp in jsonDict)
        {
            var propertyInfo = componentType.GetProperty(kvp.Key);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                // Ecrit les properties dans le component
                propertyInfo.SetValue(component, Convert.ChangeType(kvp.Value, propertyInfo.PropertyType));
            }else{
                Debug.LogWarning($"Property non reconnu ou non éditable : {kvp.Key}");
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