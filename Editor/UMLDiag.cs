using UnityEditor;
using UnityEngine;
using static JsonParser;
using System.Collections.Generic;
using ChatGPTWrapper;
using UMLClassDiag;
using static SaverLoader;
using UnityPusher;
using ChatClass;
using static MyEditorWindow;
using System.IO;
public class UMLDiag : GenerativeProcess
{
    private static UMLDiag _instance;
    private static string umlJsonStructurePath = "Packages/com.jin.protochill/Editor/JsonStructures/UMLStructure.json";
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

    private UMLDiag(UMLDiagramWindow umlDiagramWindowInstance)
    {
        umlDiagramWindow = umlDiagramWindowInstance;


        ChatWindow.OnSubmitText += OnSubmit;
        UIManager.OnGenerateScriptEvent += OnGenerateScript;
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
        UIManager.OnGenerateScriptEvent -= OnGenerateScript;
        Debug.Log("UMLDiag events unsubscribed.");
    }

    public void OnSubmit(string input)
    {
        Debug.Log("Submit received in UMLDiag. Generating UML..." + input);
        GenerateUML(input);
    }

    public void OnGenerateScript(BaseObject root){
        Debug.Log("generating script for" + root.ToString() + "...");
        GenerateScript(root);
    }


    private string umlJsonStructure = 
    "You love object abstraction and are a big time JSON user. You will follow this exact format : \n" + File.ReadAllText(umlJsonStructurePath);


    private void GenerateUML(string input)
    {
      
        input = umlJsonStructure + input;
        //BaseObject root;
        List<BaseObject> baseObjects;
        if (gptGenerator == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }
        gptGenerator.GenerateFromText(input, (response) =>
        {
            jsonScripts = response;
            SaveUML(jsonScripts);
            Debug.Log("Generated UML JSON: " + jsonScripts);

            //Le cast est nécessaire pour parse
            Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonScripts);

            //Mapping vers structure objet maison
            //root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["UML"]);
            baseObjects = JsonMapper.MapAllBaseObjects(parsedObject);
            if (umlDiagramWindow == null)
            {
                Debug.LogError("umlDiagramWindow is null when calling ReloadDiagram");
                return;
            }
            umlDiagramWindow.ReloadDiagram(baseObjects); 

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
}
