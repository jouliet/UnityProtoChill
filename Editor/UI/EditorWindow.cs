using UnityEditor;
using UnityEngine;
using ChatGPTWrapper;
using UMLClassDiag;
using System.Collections.Generic;
using System.IO;
using static JsonParser;
using static SaverLoader;
using UnityPusher;
using System;

public class MyEditorWindow : EditorWindow
{
    // Submit prompt to GPT Event
    public static event System.Action<string> OnSubmitText;
    public static event System.Action<BaseObject> OnGenerateScriptEvent;
    public static event System.Action OnGenerateGameObjectListEvent;
    public static event System.Action<string> OnGenerateGameObjectEvent;

    //public static event System.Action
    private string userInput = "A Player that may move with arrows and shoot bullets with space bar"; 
    private BaseObject selectedObject;
    private BaseObject  rootObject;
    private int selectedObjectIndex = 0; // Indice de l'objet sélectionné
    private string selectedGameObject = null;
    private int selectedGameObjectIndex;
    // Init GPT Event
    public static event System.Action<bool, string, string, CustomChatGPTConversation.Model, string> OnInitializeGPTInformation;
    private bool useProxy;
    private string proxyUri = "";
    private string apiKey = "";
    private CustomChatGPTConversation.Model selectedModel = CustomChatGPTConversation.Model.ChatGPT4;
    private string initialPrompt = @"
You are both an oriented object and gameobject oriented beast. You only use functions defined in the json or native to Unity.
Never assume a method, class or function exists without explicitly seeing it in the UML diagram you are presented. When building said diagram always be exhaustive with the links between classes and always prefer making them go both ways";


    [MenuItem("Window/ProtoChillAITool")]
    public static void ShowWindow()
    {
        GetWindow<MyEditorWindow>("ProtoChill Tool");
    }

    private Vector2 scrollPosition; 
    private Vector2 topScrollPosition;
    private Vector2 globalScrollPosition;
    private void OnGUI()
{   
    // Ajout d'un conteneur ScrollView global
    globalScrollPosition = GUILayout.BeginScrollView(globalScrollPosition);

    if (ObjectResearch.AllBaseObjects.Count == 0){
        LoadUML();
    }

    // Titre
    GUILayout.Space(10);
    GUILayout.Label("GPT Object Structure Generator", EditorStyles.boldLabel);
    GUILayout.Space(10);

    // SECTION INIT
    GUILayout.BeginVertical("box");
    GUILayout.Label("Initialize GPT Settings", EditorStyles.boldLabel);

    // useProxy = EditorGUILayout.Toggle("Use Proxy", useProxy);
    // proxyUri = EditorGUILayout.TextField("Proxy URI", proxyUri);
    apiKey = EditorGUILayout.PasswordField("API Key", apiKey);
    if (apiKey == ""){
        apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogWarning("Variable d'environnement OPENAI_API_KEY introuvable ou vide.");
        }
        else
        {
            Debug.Log($"Variable d'environnement trouvée : {apiKey}");
        }
    }
    selectedModel = (CustomChatGPTConversation.Model)EditorGUILayout.EnumPopup("Model", selectedModel);

    GUILayout.Label("Initial Prompt", EditorStyles.label);
    topScrollPosition = GUILayout.BeginScrollView(topScrollPosition, GUILayout.Height(90));
    initialPrompt = EditorGUILayout.TextArea(initialPrompt, GUILayout.Height(70));
    GUILayout.EndScrollView();

    GUILayout.Space(15);
 
    // Bouton INIT GPT
    if (GUILayout.Button("Initialize GPT", GUILayout.Height(40)))
    {
        InitializeGPTInformation();
        Debug.Log("GPT Initialized with user parameters.");
    }
    GUILayout.EndVertical();


    // SECTION DESCRIPTION
    GUILayout.BeginVertical("box");
    GUILayout.Label("Describe Your Object Structure", EditorStyles.boldLabel);

    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
    userInput = EditorGUILayout.TextArea(userInput, GUILayout.Height(100));
    GUILayout.EndScrollView();
    GUILayout.EndVertical();

    GUILayout.Space(20);

    // Bouton SUBMIT
    GUILayout.BeginHorizontal();
    if (GUILayout.Button("Submit", GUILayout.Height(40)))
    {
        RemoveJsonFiles();
        SubmitText();
        AssetDatabase.Refresh();
        Debug.Log("Text submitted: " + userInput);
    }
    GUILayout.EndHorizontal();

    // Bouton UML
    GUILayout.BeginHorizontal();
    if (GUILayout.Button("UML", GUILayout.Height(40)))
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("L'objet root n'est toujours pas généré.");
        }
        else
        {
            ObjectResearch.AllBaseObjects.Clear();
            Debug.Log("Text submitted: " + userInput);
            //UMLDiagramWindow.ShowDiagram(selectedObject);
        }
    }
    GUILayout.EndHorizontal();

    GUILayout.Space(10);

    // Sélecteur d'objet de base
    GUILayout.BeginVertical("box");
    GUILayout.Label("Base Object Selector", EditorStyles.boldLabel);

    if (ObjectResearch.AllBaseObjects.Count > 0)
    {
        string[] options = new string[ObjectResearch.AllBaseObjects.Count];
        for (int i = 0; i < ObjectResearch.AllBaseObjects.Count; i++)
        {
            options[i] = ObjectResearch.AllBaseObjects[i].Name;
        }

        selectedObjectIndex = EditorGUILayout.Popup("Select BaseObject", selectedObjectIndex, options);

        // Assurez-vous de ne pas dépasser les limites du tableau
        if (selectedObjectIndex >= 0 && selectedObjectIndex < ObjectResearch.AllBaseObjects.Count)
        {
            selectedObject = ObjectResearch.AllBaseObjects[selectedObjectIndex];
            EditorGUILayout.LabelField("Selected Object:", selectedObject.Name);
        }
    }
    else
    {
        GUILayout.Label("No base objects available.", EditorStyles.helpBox);
    }
    GUILayout.EndVertical();

    // Bouton GENERATE SCRIPT
    GUILayout.BeginVertical("box");
    GUILayout.Label("Script Generation", EditorStyles.boldLabel);

    if (selectedObject != null)
    {
        if (GUILayout.Button("Generate Script", GUILayout.Height(40)))
        {
            GenerateScript();
            Debug.Log("Script generation triggered for: " + selectedObject.Name);
        }
    }
    else
    {
        GUILayout.Label("Please select a base object before generating a script.", EditorStyles.helpBox);
    }

    GUILayout.EndVertical();
    
    GUILayout.Space(20);

    // //Bouton De génération de la list des GOs
    GUILayout.BeginVertical("box");
    GUILayout.Label("GameObject List Generation", EditorStyles.boldLabel);
    if (GUILayout.Button("Generate GO List", GUILayout.Height(40)))
    {
        GenerateGameObjectList();
        Debug.Log("Generation of GameObjects List");
    }
    GUILayout.EndVertical();
    
    //Selecteur de gameObjects
    GUILayout.BeginVertical("box");
    GUILayout.Label("GameObject Selector", EditorStyles.boldLabel);

    if (GameObjectCreator.GameObjectNameList != null && GameObjectCreator.GameObjectNameList.Count > 0)
    {
        string[] options = new string[GameObjectCreator.GameObjectNameList.Count];
        for (int i = 0; i < GameObjectCreator.GameObjectNameList.Count; i++)
        {
            options[i] = GameObjectCreator.GameObjectNameList[i];
        }

        selectedGameObjectIndex = EditorGUILayout.Popup("Select GameObject", selectedGameObjectIndex, options);

        // Assurez-vous de ne pas dépasser les limites du tableau
        if (selectedGameObjectIndex >= 0 && selectedGameObjectIndex < GameObjectCreator.GameObjectNameList.Count)
        {
            selectedGameObject = GameObjectCreator.GameObjectNameList[selectedGameObjectIndex];
            EditorGUILayout.LabelField("Selected Object:", selectedGameObject);
        }
    }
    else
    {
        GUILayout.Label("No GameObjects available.", EditorStyles.helpBox);
    }
    GUILayout.EndVertical();

    // Bouton GenerateGameObject

    GUILayout.BeginVertical("box");
    GUILayout.Label("GameObject Generation", EditorStyles.boldLabel);
    if (selectedGameObject != null)
    {
        if (GUILayout.Button("Generate GameObject", GUILayout.Height(40)))
        {
            GenerateGameObject();
            Debug.Log("GameObject generation triggered for: " + selectedGameObject);
        }
    }
    else
    {
        GUILayout.Label("Please select a Game object before generating a script.", EditorStyles.helpBox);
    }

    GUILayout.EndVertical();



    // // Bouton PushObject
    // GUILayout.BeginVertical("box");
    // GUILayout.Label("Push EXISTING script to same name GO", EditorStyles.boldLabel);

    // if (selectedObject != null)
    // {
    //     if (GUILayout.Button("Push", GUILayout.Height(40)))
    //     {
    //         selectedObject.Push();
    //         Debug.Log(selectedObject.Name + "was created");
    //     }
    // }
    // else
    // {
    //     GUILayout.Label("Please select a base object and generate it's script before pushing", EditorStyles.helpBox);
    // }

    // GUILayout.EndVertical(); 

    //Fin du conteneur ScrollView global
    GUILayout.EndScrollView();
}


    private void GenerateScript(){
      //L'abonnée est toujours UMLDiag
      OnGenerateScriptEvent?.Invoke(selectedObject);
    }

    private void SubmitText()
    {
        // Actuellement le seul abonné est l'instance de UMLDiag de main.
        OnSubmitText?.Invoke("Make a UML for the system :" +userInput); 
    }

    private void GenerateGameObjectList(){
        OnGenerateGameObjectListEvent?.Invoke();
    }
    private void GenerateGameObject(){
        OnGenerateGameObjectEvent?.Invoke(selectedGameObject);
    }

    private void InitializeGPTInformation()
    {
        // Le seul abonné est l'instance de GPTGenerator, elle même instanciée dans le constructeur de Generative process. On 
        // instancie donc Un GPTGenerator et un CustomChatGPTConversation par generative process. 
        OnInitializeGPTInformation?.Invoke(useProxy, proxyUri, apiKey, selectedModel, initialPrompt);
    }

}
