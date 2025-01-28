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
    @"You love object abstraction and are a big time JSON user. You will follow this exact format : 
{
  ""UML"": {
    ""Name"": ""ObjectName"",
    ""Attributes"": [
      {
        ""Name"": ""AttributeName1"",
        ""Type"": ""AttributeType1"",
        ""DefaultValue"": ""DefaultValue1""
      },
      {
        ""Name"": ""AttributeName2"",
        ""Type"": ""AttributeType2"",
        ""DefaultValue"": ""null""
      }
    ],
    ""Methods"": [
      {
        ""Name"": ""MethodName1"",
        ""ReturnType"": ""ReturnType1"",
        ""Parameters"": [
          {
            ""Name"": ""ParamName1"",
            ""Type"": ""ParamType1"",
            ""DefaultValue"": ""ParamDefaultValue1""
          }
        ]
      },
      {
        ""Name"": ""MethodName2"",
        ""ReturnType"": ""ReturnType2"",
        ""Parameters"": []
      }
    ],
    ""ComposedClasses"": [
      {
        ""Name"": ""ComposedClassName1"",
        ""Attributes"": [
          {
            ""Name"": ""AttributeName1"",
            ""Type"": ""AttributeType1"",
            ""DefaultValue"": ""null""
          }
        ],
        ""Methods"": [],
        ""ComposedClasses"": [],
        ""ParentClass"": ""null""
      }
    ],
    ""ParentClass"": ""null""
  }
}
";

    private void GenerateUML(string input)
    {
      
        input = umlJsonStructure + input;
        BaseObject root;
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
            root = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["UML"]);
            if (umlDiagramWindow == null)
            {
                Debug.LogError("umlDiagramWindow is null when calling ReloadDiagram");
                return;
            }
            umlDiagramWindow.ReloadDiagram(root); 

        });
    }

    //Pour l'instant generateScripts est ici mais on pourra le bouger si besoin. De même pour BaseObjectList
    //GenerateScripts est lié à l'event UI generateScripts for selected baseObject
    private void GenerateScript(BaseObject root){
        Debug.Log("Script was generated");
        root.GenerateScript();
    }

    private string prefabJsonStructure = 
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
    }" + "\n";

    private string inputToCreatePrefabs = 
    "Make a list of the game objects that are needed to execute the scripts presented in the UML below. Remember that the script names must be coherent with the UML scripts. \n";
    private string generalInputForPrefabs;
    private void GenerateGameObjects(string jsoninput){
      generalInputForPrefabs = prefabJsonStructure + inputToCreatePrefabs + jsoninput;

      if (gptGenerator == null){
          Debug.Log("No instance of gptGenerator");
          return;
      }
      gptGenerator.GenerateFromText(generalInputForPrefabs, (response) =>
      {
          string jsonString = response;
          Debug.Log("Generated Prefabs JSON: \n" + jsonString);
          GameObjectCreator.JsonToDictionary(jsonString);


          // Dictionary<string, object> parsedObject = (Dictionary<string, object>) Parse(jsonString);

          // GameObjectCreator.CreateEveryGameObjects(parsedObject);
      });
    }



    public void ReloadUI(BaseObject root){
      if (umlDiagramWindow == null)
        {
          Debug.LogError("umlDiagramWindow is null when calling ReloadDiagram");
          return;
        }
      umlDiagramWindow.ReloadDiagram(root);
    }
}
