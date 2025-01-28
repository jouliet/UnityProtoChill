using System;
using System.Collections.Generic;
using System.IO;
using static JsonParser;
using UMLClassDiag;
using UnityPusher;
using UnityEngine;
using UnityEditor;
using static UMLDiag;

public static class SaverLoader
{
    private static string generatedContentFolder = "Packages/com.jin.protochill/Editor/GeneratedContent"; 
    public static string UMLFilePath = Path.Combine(generatedContentFolder, "currentUML.json");
    private static string GOJsonFilePath = Path.Combine(generatedContentFolder, "currentGameObjects.json");

    public static void SaveUML(string input)
    {
        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(generatedContentFolder))
            {
                Directory.CreateDirectory(generatedContentFolder);
            }

            File.WriteAllText(UMLFilePath, input);
            Debug.Log("UML saved at :" + UMLFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving UML: " + ex.Message);
        }
    }

    public static void SaveGOJson(string input)
    {
        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(generatedContentFolder))
            {
                Directory.CreateDirectory(generatedContentFolder);
            }

            File.WriteAllText(GOJsonFilePath, input);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving UML: " + ex.Message);
        }
    }

    public static void LoadUML() //Si t'utilises ça depuis l'UI il se passe des choses suspectes (ça marche pas en gros, faut faire depuis là bas)
    {
        try
        {
            
            if (!File.Exists(UMLFilePath))
            {
                //Debug.Log("fichier n'existe po");
                return;
            }
            
            string jsonString = File.ReadAllText(UMLFilePath);
            GenerativeProcess.SetJsonScripts(jsonString);
            Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
            List<BaseObject> baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
            Debug.Log("UML Loaded");

            //Reload UI (cas relous...) géré par uml diag
            if (baseObjects != null)
            {
                UMLDiag.Instance.ReloadUI(baseObjects);
            }
            else {
                Debug.Log("TRES SUS NE PAS IGNORER CE DEBUG");
            }

        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading UML: " + ex.Message);
        }
    }

    public static void LoadGOJson()
    {
        try
        {
            
            if (!File.Exists(GOJsonFilePath))
            {
                //Debug.Log("There is no GOJsonfile to load at this path:  " + GOJsonFilePath);
                return;
            }
            

            string jsonString = File.ReadAllText(GOJsonFilePath);
            GenerativeProcess.SetJsonGOs(jsonString);
            GameObjectCreator.JsonToDictionary(jsonString);
            GameObjectCreator.StockEveryGOsInList();
            Debug.Log("GOJson Loaded");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading GOJson: " + ex.Message);
        }
    }

    public static void RemoveJsonFiles(){
        try{
            if (File.Exists(GOJsonFilePath))
            {
                File.Delete(GOJsonFilePath);
            }

            if (File.Exists(UMLFilePath))
            {
                File.Delete(UMLFilePath);
            }
        }catch(Exception ex){
            Debug.Log("Failed to delete json files: " + ex);
        }       
    }
}