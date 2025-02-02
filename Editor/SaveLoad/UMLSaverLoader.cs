using System;
using System.Collections.Generic;
using System.IO;
using static JsonParser;
using UMLClassDiag;
using UnityPusher;
using UnityEngine;
using UnityEditor;
using static UMLDiag;
using static ObjectResearch;
using ChatClass;
public static class SaverLoader
{
    private static string generatedContentFolder = "Packages/com.jin.protochill/Editor/GeneratedContent"; 
    public static string UMLFilePath = Path.Combine(generatedContentFolder, "currentUML.json");

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
            //Debug.Log("UML saved at :" + UMLFilePath);
            Debug.Log("finished saving UML");
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
            
            // string jsonString = File.ReadAllText(UMLFilePath);
            // GenerativeProcess.SetJsonScripts(jsonString);
            // Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);
            // List<BaseObject> baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
            // List<BaseGameObject> gameObjects = JsonMapper.MapAllBaseGOAndLinksToBO(parsedObject);
            Debug.Log("UML Loaded");

            //Reload UI (cas relous...) géré par uml diag
            if (AllBaseObjects != null)
            {
                UMLDiag.Instance.ReloadUI(AllBaseObjects);
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

    public static void RemoveJsonFiles(){
        try{
            if (File.Exists(UMLFilePath))
            {
                AssetDatabase.DeleteAsset(UMLFilePath);
            }else if (File.Exists(ChatWindow.DialoguePath)){
                AssetDatabase.DeleteAsset(ChatWindow.DialoguePath); 
            }
        }catch(Exception ex){
            Debug.Log("Failed to delete json files: " + ex);
        }     
        AssetDatabase.Refresh();
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(); // Force la recompilation des scripts
    }
}