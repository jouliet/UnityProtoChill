using UnityEngine;
using static JsonParser;
using System.Collections.Generic;
using UMLClassDiag;
using static SaverLoader;
using UnityPusher;
using ChatClass;
using System;
using System.IO;
using ChatGPTWrapper;
using static PromptEngineeringUtilities;
using static ObjectResearch;
public class UMLDiag : GenerativeProcess
{
    private static UMLDiag _instance;
    
    public static UMLDiag Instance 
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UMLDiag instance not initialized. Call Initialize() first.");
            }
            return _instance;
        }
    }

    private UMLDiagramWindow umlDiagramWindow;

    public BaseObject selectedObject;
    private UMLDiag(UMLDiagramWindow umlDiagramWindowInstance)
    {
        umlDiagramWindow = umlDiagramWindowInstance;
        gPTGenerator = GPTGenerator.Instance;

        ChatWindow.OnSubmitText += OnSubmit;
        //UIManager.OnGenerateScriptEvent += OnGenerateScript;
    }


    public static void Initialize(UMLDiagramWindow umlDiagramWindowInstance)
    {
        if (_instance != null)
        {
            return;
        }

        _instance = new UMLDiag(umlDiagramWindowInstance);
    }



    // Méthode de nettoyage pour désabonner les événements
    private void Cleanup()
    {
        ChatWindow.OnSubmitText -= OnSubmit;
        //UIManager.OnGenerateScriptEvent -= OnGenerateScript;
        Debug.Log("UMLDiag events unsubscribed.");
    }

    public void OnSubmit(string input)
    {
        
        if (selectedObject == null){
            umlDiagramWindow.OnLoadingUML(true);
            Debug.Log("Submit received in UMLDiag. Generating UML..." + input);
            GenerateClassesandGOs(input);
        }
        else {
            Debug.Log("Submit received in UMLDiag, Updating specific Object "+selectedObject.Name+ ". Generating UML..." + input);
            //GenerateSingleClass(input,selectedObject);
            GenerateScript(selectedObject, input);
        }
    }

    public void OnGenerateScript(BaseObject root){
        Debug.Log("generating script for" + root.ToString() + "...");
        GenerateScript(root, "");
    }


    
    private void GenerateClassesandGOs(string input)
    {
        input = UMLAndGOPrompt(input);
        //BaseObject root;
        List<BaseObject> baseObjects = new List<BaseObject>();
        List<BaseGameObject> gameObjects = new List<BaseGameObject>();
        ObjectResearch.CleanUp();
        if (GPTGenerator.Instance == null){
            Debug.Log("No instance of gptGenerator");
            return;
        } 
        GPTGenerator.Instance.GenerateFromText(input, (response) =>
        {
            jsonScripts = response;  

            Debug.Log("Generated UML & GameObjects JSON: " + jsonScripts);

            try
            {
                //Le cast est nécessaire pour parse
                Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonScripts);

                //Mapping vers structure objet maison
                //root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["UML"]);
                baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
                gameObjects = JsonMapper.MapAllBaseGOAndLinksToBO(parsedObject);

                if (umlDiagramWindow == null)
                {
                    Debug.LogError("umlDiagramWindow is null when calling ReloadDiagram");
                    return;
                }
                umlDiagramWindow.OnLoadingUML(false);
                umlDiagramWindow.ReloadDiagram(baseObjects);
                Debug.Log("finished generating !");
                SaveDataToCurrentUML();


            }
            catch (Exception e) 
            {
                Debug.LogError($"Wrong JSON format: {e.Message}");
                umlDiagramWindow.OnLoadingUML(false);
            }
        });
    }

    //Pour l'instant generateScripts est ici mais on pourra le bouger si besoin. De même pour BaseObjectList
    //GenerateScripts est lié à l'event UI generateScripts for selected baseObject
    private void GenerateScript(BaseObject root, string input){
        Debug.Log("Script was generated");
        root.GenerateScript(input);
    }

    public void ReloadUI(List<BaseObject> baseObjects){
      if (umlDiagramWindow == null)
        {
          Debug.LogError("umlDiagramWindow is null when calling ReloadDiagram");
          return;
        }
      umlDiagramWindow.ReloadDiagram(baseObjects);
    }

    public static void SaveDataToCurrentUML()
    {
        string updatedJson = "{\n\t\"Classes\": [\n";
        
        for (int i = 0; i < AllBaseObjects.Count; i++)
        {
            
            updatedJson += "\t\t" + AllBaseObjects[i].ToJson();
            if (i < AllBaseObjects.Count - 1)
            {
                updatedJson += ",";
            }

            updatedJson += "\n";
        }

        updatedJson += "\t],\n\t\"GameObjects\": [\n";
        for (int i = 0; i < AllBaseGameObjects.Count; i++)
        {
         
            updatedJson += "\t\t" + AllBaseGameObjects[i].ToJson();
            if (i < AllBaseGameObjects.Count - 1)
            {
                updatedJson += ",";
            }

            updatedJson += "\n";
        }

        updatedJson += "\t]\n}";
        SaveUML(updatedJson);
    }
}
