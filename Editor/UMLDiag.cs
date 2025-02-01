using UnityEngine;
using static JsonParser;
using System.Collections.Generic;
using UMLClassDiag;
using static SaverLoader;
using UnityPusher;
using ChatClass;
using System.IO;
using ChatGPTWrapper;
public class UMLDiag : GenerativeProcess
{
    private static UMLDiag _instance;
    private static string classesAndGOJsonStructurePath = "Packages/com.jin.protochill/Editor/JsonStructures/Classes&GOStructure.json";
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
        umlDiagramWindow.OnLoadingUML(true);
        Debug.Log("Submit received in UMLDiag. Generating UML..." + input);
        GenerateClassesandGOs(input);
    }

    public void OnGenerateScript(BaseObject root){
        Debug.Log("generating script for" + root.ToString() + "...");
        GenerateScript(root);
    }

    private string classesAndGOJsonStructure = 
    "You are a Json Writer. You will follow this exact format with every value in between quotes : \n" +
    File.ReadAllText(classesAndGOJsonStructurePath);
    private string separationRequest = "The UML part is on the 'Classes' node and the 'GameObjects' part is on the GameObjects node.";
    private string classesRequest = "For the Classes part: \n" +
    "Composed Classes are the classes used by the classe in question. \n" +
    "Classes with the most composed classes should be at the top of the list.";
    private string goRequests = 
    "For the GameObject part : \n" +
    "Float values format exemple : 10.5 \n" +
    "For type = Script, there is always a properties Name who must be an existing script name" + "\n" +
    "Don't hesitate to add boxCollider or rigidbody components if necessary. Also don't hesisate to scale the gameobject with the Transform localScale.\n" +
    "You must add MeshFilter (with MeshRenderer) component on almost all game objects who are not UI or Managers.\n" +
    "Ground must be black. You choose for the other game objects.";

    private static string inputToCreatePrefabs = 
    "Remember that the script names must be coherent with the UML scripts. \n";

    private void GenerateClassesandGOs(string input)
    {
        input = input + classesAndGOJsonStructure + separationRequest + classesRequest +  goRequests + inputToCreatePrefabs;
        //BaseObject root;
        List<BaseObject> baseObjects = new List<BaseObject>();
        if (GPTGenerator.Instance == null){
            Debug.Log("No instance of gptGenerator");
            return;
        }
        GPTGenerator.Instance.GenerateFromText(input, (response) =>
        {
            jsonScripts = response;
            SaveUML(jsonScripts);
            umlDiagramWindow.OnLoadingUML(false);
            Debug.Log("Generated UML & GameObjects JSON: " + jsonScripts);

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

            if (GameObjectCreator.GameObjectNameList != null){
                GameObjectCreator.GameObjectNameList.Clear();
            }

            GameObjectCreator.JsonToDictionary(jsonScripts);
            GameObjectCreator.StockEveryGOsInList();
            GameObjectCreator.CreateAllGameObjects();
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
