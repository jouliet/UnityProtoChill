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
            Debug.Log("Submit received in UMLDiag. Generating UML..." + input);
            GenerateClassesandGOs(input);
        }
        else {
            Debug.Log("Submit received in UMLDiag, Updating specific Object "+selectedObject.Name+ ". Generating UML..." + input);
            GenerateSingleClass(input,selectedObject);
        }
        umlDiagramWindow.OnLoadingUML(true);
    }

    public void OnGenerateScript(BaseObject root){
        Debug.Log("generating script for" + root.ToString() + "...");
        GenerateScript(root);
    }


    private void GenerateSingleClass(string input, BaseObject bo){
        input = UpdateSingleClassPrompt(bo, input);

        GPTGenerator.Instance.GenerateFromText(input, (response) =>
        {
            jsonScripts = response;
            Debug.Log("Generated UML & GameObjects JSON: " + jsonScripts);

            //Le cast est nécessaire pour parse
            Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonScripts);

            if (umlDiagramWindow == null)
            {
                Debug.LogError("umlDiagramWindow is null when calling ReloadDiagram");
                return;
            }
            umlDiagramWindow.ReloadDiagram(AllBaseObjects); 
        });
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

            //Le cast est nécessaire pour parse
            Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonScripts);

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
            // if (GameObjectCreator.GameObjectNameList != null){
            //     GameObjectCreator.GameObjectNameList.Clear();
            // }

            // GameObjectCreator.JsonToDictionary(jsonScripts);
            // GameObjectCreator.StockEveryGOsInList();
            // GameObjectCreator.CreateAllGameObjects();
        });
    }

    //Pour l'instant generateScripts est ici mais on pourra le bouger si besoin. De même pour BaseObjectList
    //GenerateScripts est lié à l'event UI generateScripts for selected baseObject
    private void GenerateScript(BaseObject root){
        Debug.Log("Script was generated");
        root.GenerateScript();
    }

    public void ReloadUI(List<BaseObject> baseObjects){
      if (umlDiagramWindow == null)
        {
          Debug.LogError("umlDiagramWindow is null when calling ReloadDiagram");
          return;
        }
      umlDiagramWindow.ReloadDiagram(baseObjects);
    }

    public static void SaveDataToCurrentUML(){
        
        string updatedJson = "{ \"Classes\": [";

        // Ajouter les classes
        foreach (BaseObject bo in AllBaseObjects){
            // Ici, on suppose que bo.ToJson() renvoie une chaîne représentant un objet de classe JSON
            updatedJson += bo.ToJson() + ",";
        }

        // Ajouter les objets de jeu
        updatedJson += "], \"GameObjects\": [";
        foreach (BaseGameObject bgo in AllBaseGameObjects){
            // Ici aussi, on suppose que bgo.ToJson() renvoie une chaîne représentant un objet de jeu en JSON
            updatedJson += bgo.ToJson() + ",";
        }

        // Si des classes ou objets de jeu ont été ajoutés, on enlève la dernière virgule
        if (AllBaseObjects.Count > 0) {
            updatedJson = updatedJson.TrimEnd(',');
        }
        updatedJson += "]}";

        SaveUML(updatedJson);
    }
}
