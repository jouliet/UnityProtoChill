using UnityEngine;
using static JsonParser;
using System.Collections.Generic;
using UMLClassDiag;
using static SaverLoader;
using UnityPusher;
using ChatClass;
using System.IO;
using ChatGPTWrapper;
using static PromptEngineeringUtilities;
using static ObjectResearch;
using System;
using System.Globalization;
using UnityEditor;
using NUnit.Framework;
public class LevelDesignCreator : GenerativeProcess
{
    private static string LevelDesignJsonPath = "Packages/com.jin.protochill/Editor/GeneratedContent/LevelDesign.json";
    private static string LevelDesignStructureJsonPath = "Packages/com.jin.protochill/Editor/JsonStructures/LevelDesignStructure.json";
    private static string directivePrompt = "You make a json to describe a level with the position, scale and rotation of the game objects. \n" +
    "The game and the game objects are enumerated in the following json. The prefab_name must be equal with a gameObject Name of the following json. \n";
    private static string jsonClassesAndGos;
    private static string formatPrompt = 
    "The json must follow this format: \n" +
    AssetDatabase.LoadAssetAtPath<TextAsset>(LevelDesignStructureJsonPath).text + "\n";
    private static string jsonLD;
    public static void GenerateLevelDesign(string input){
        if (File.Exists(UMLFilePath)){
            jsonClassesAndGos = AssetDatabase.LoadAssetAtPath<TextAsset>(UMLFilePath).text;
        }else{
            throw new Exception("Cannot push GameObjects without a the UMLandGOs json.");
        }
        input = directivePrompt + jsonClassesAndGos + formatPrompt + input;
        GPTGenerator.Instance.GenerateFromText(input, (response) =>
        {
            jsonLD = response;

            if (!Directory.Exists(generatedContentFolder))
            {
                Directory.CreateDirectory(generatedContentFolder);
            }
            File.WriteAllText(LevelDesignJsonPath, jsonLD);

            //Le cast est nécessaire pour parse
            Dictionary<string, object> LDDictionary = (Dictionary<string, object>) Parse(jsonLD);

            PushGOsOnScene(LDDictionary);
        });
    }

    public static void PushGOsOnScene(Dictionary<string, object> LDDictionary){
        try{
            List<object> LD_GO_List = (List<object>)LDDictionary["gameObjects"];
            foreach (Dictionary<string, object> GODict in LD_GO_List)
            {
                string prefabName = GODict["prefab_name"].ToString();
                string prefabPath = GameObjectCreator.prefabPath + prefabName + ".prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null){
                    throw new Exception("Prefab non trouvée: " + prefabPath);
                }
                Dictionary<string, object> LD_GOtransform = (Dictionary<string, object>)GODict["transform"];
                Vector3 position = ParseVector3((List<object>)LD_GOtransform["position"]);
                Vector3 rotation = ParseVector3((List<object>)LD_GOtransform["rotation"]);
                Vector3 scale = ParseVector3((List<object>)LD_GOtransform["scale"]);
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = position;
                go.transform.localScale = scale;
                go.transform.eulerAngles = rotation;

                //ndo.RegisterCreatedObjectUndo(go, "Instancier " + prefabName);

                //Assert.IsNotNull(go, "Le prefab n'a pas été instancié correctement.");
            }
        }catch(Exception ex){
            Debug.LogError("Error during the mapping of the LevelDesign json:\n" + ex);
        }
    }

    private static Vector3 ParseVector3(List<object> vector3ListJson){
        List<float> vector3List = new List<float>();
        foreach(object number in vector3ListJson){
            vector3List.Add(Convert.ToSingle(number, CultureInfo.InvariantCulture));
        }
        return new Vector3(vector3List[0], vector3List[1], vector3List[2]);
    }
}