using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;
using UMLClassDiag;
using ChatClass;
using SettingsClass;
using System.IO;
using System.Collections.Generic;
using static JsonParser;
using static SaverLoader;
using UnityPusher;
using System;
using static ObjectResearch;

using ChatGPTWrapper;
public class UIManager : EditorWindow
{
    private VisualElement mainContainer;
    private VisualElement umlContainer;
    private VisualElement chatContainer;
    private VisualElement settingsContainer;

    public ChatWindow chatWindow;
    private UMLDiagramWindow umlDiagramWindow;
    private ObjectPopUp objectPopUp;

    private Button testButton;
    private Button settingsButton;
    private Button objectSelectorButton;
    private Button generateButton;
    //GO = Game Object
    private Button generateGOListButton;
    private Button gameObjectSelectorButton;
    private Button generateGOButton;
    private GameObjectPopUp GOPopUp;

    public static event System.Action<BaseObject> OnGenerateScriptEvent;
    public static event System.Action OnGenerateGameObjectListEvent;
    public static event System.Action<string> OnGenerateGameObjectEvent;
    public event System.Action<string> OnMessageToChat;

    [MenuItem("Window/ProtoChill")]
    public static void ShowWindow()
    {
        if (HasOpenInstances<UIManager>())
        {
            CloseAllInstances();
        }
        GetWindow<UIManager>("ProtoCHILL");
    }

    private static void CloseAllInstances()
    {
        var windows = Resources.FindObjectsOfTypeAll<UIManager>();
        foreach (var window in windows)
        {
            window.Close();
        }
    }

    // Temporaire ? histoire de pas avoir de trucs sus en attendant que les choses soient bien faites

    private void OnGUI()
    {   
        if (ObjectResearch.AllBaseObjects.Count == 0){
            LoadUML();
            LoadGOJson();
        }
    }


    private void OnEnable()
    {
        CreateLayout();
        Main.Instance.Init(umlDiagramWindow);
        //Initialise GPTGenerator pour qu'il puisse envoyer des réponses quel que soit la source de l'appel ... On fait le tri après
        GPTGenerator.Instance.uIManager =this;
       
    }

    private void OnDisable()
    {
        AssetDatabase.Refresh();
        // Debug.Log("UIManager disabled");
    }
    private void CreateLayout()
    {
        var rootContainer = new VisualElement();

        // Set up settings Container
        rootContainer.style.flexDirection = FlexDirection.Column;
        rootContainer.style.flexGrow = 1;
        settingsContainer = new VisualElement { name = "settings-bar" };
        settingsContainer.style.height = 30;
        settingsContainer.style.backgroundColor = Color.gray;
        settingsContainer.style.flexDirection = FlexDirection.Row;
        settingsContainer.style.alignItems = Align.Center;

        InitializeTestButton();
        InitializeGPTButton();
        InitializeGenerateScriptPopUp();
        InitializeGenerateScriptButton();
        InitializeGenerateGOListButton();
        InitializeGOSelector();
        InitializeGenerateGOButton();
        
        rootContainer.Add(settingsContainer);

        // Set up main content
        mainContainer = new VisualElement();
        mainContainer.style.flexDirection = FlexDirection.Row;
        mainContainer.style.flexGrow = 1;

        // Set up UML Diagram View
        umlContainer = new VisualElement { name = "uml-container" };
        umlContainer.style.flexGrow = 7;
        umlContainer.style.borderRightWidth = 3;
        umlContainer.style.borderRightColor = Color.gray;
        umlContainer.style.overflow = Overflow.Hidden;
        InitializeUMLView();
        mainContainer.Add(umlContainer);

        // Set up Chat View
        chatContainer = new VisualElement { name = "chat-window" };
        chatContainer.style.flexGrow = 3;
        chatContainer.style.flexDirection = FlexDirection.Column;
        chatContainer.style.maxWidth = new Length(30, LengthUnit.Percent);
        chatContainer.style.width = new Length(30, LengthUnit.Percent);
        InitializeChatView();
        mainContainer.Add(chatContainer);

        rootContainer.Add(mainContainer);

        // Add the root container to the window
        rootVisualElement.Clear();
        rootVisualElement.Add(rootContainer);
    }

    //
    // Views Initialization
    //

    private void InitializeUMLView()
    {
        umlDiagramWindow = ScriptableObject.CreateInstance<UMLClassDiag.UMLDiagramWindow>();
        var umlCanvas = umlDiagramWindow.CreateDiagramView(this);
        umlCanvas.style.flexGrow = 1;
        umlContainer.Add(umlCanvas);
    }

    private void InitializeChatView()
    {
        chatWindow = ScriptableObject.CreateInstance<ChatClass.ChatWindow>();
        var chatCanvas = chatWindow.CreateChatView(this);
        chatContainer.Add(chatCanvas);
    }

    //
    // Options Initialization
    //

    private void InitializeTestButton()
    {
        testButton = new Button() { text = "Test" };
        testButton.clicked += OnTestButtonClick;
        settingsContainer.Add(testButton);
    }

    private void InitializeGPTButton()
    {
        settingsButton = new Button() { text = "Settings" };
        settingsButton.clicked += OnSettingsButtonClick;
        settingsContainer.Add(settingsButton);
    }

    private void InitializeGenerateScriptPopUp()
    {
        objectSelectorButton = new Button() { text = "Object Selector" };
        objectPopUp = new ObjectPopUp();
        objectSelectorButton.clicked += () => PopupWindow.Show(objectSelectorButton.worldBound, objectPopUp);
        settingsContainer.Add(objectSelectorButton);
    }

    private void InitializeGenerateScriptButton()
    {
        generateButton = new Button() { text = "Generate Script(s)" };
        generateButton.clicked += OnGenerateScriptButtonClick;
        settingsContainer.Add(generateButton);
    }

    private void InitializeGenerateGOListButton(){
        generateGOListButton = new Button() { text = "Generate List of Game Objects" };
        generateGOListButton.clicked += OnGenerateGameObjectListButton;
        settingsContainer.Add(generateGOListButton);
    }

    private void InitializeGOSelector()
    {
        gameObjectSelectorButton = new Button() { text = "GameObject Selector" };
        GOPopUp = new GameObjectPopUp();
        gameObjectSelectorButton.clicked += () => PopupWindow.Show(gameObjectSelectorButton.worldBound, GOPopUp);
        settingsContainer.Add(gameObjectSelectorButton);
    }

    private void InitializeGenerateGOButton(){
        generateGOButton = new Button() { text = "Generate GameObject(s)" };
        generateGOButton.clicked += OnGenerateGameObjectButton;
        settingsContainer.Add(generateGOButton);
    }

    //
    //
    // On Click Events
    //

    private void OnTestButtonClick()
    {
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Tests/JsonMockUpProblem.json");
        if (jsonFile != null)
        {
            string jsonString = jsonFile.text;
            Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
            BaseObject baseObject = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["UML"]);
            GenerativeProcess.SetJsonScripts(jsonString);
            umlDiagramWindow.ReloadDiagram(baseObject);
        }
    }

    private void OnLoadUMLButtonClick(){

        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(UMLFilePath);
        if (jsonFile != null)
        {
            string jsonString = jsonFile.text;
            Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
            ObjectResearch.CleanUp();
            BaseObject baseObject = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["UML"]);
            GenerativeProcess.SetJsonScripts(jsonString);
            umlDiagramWindow.ReloadDiagram(baseObject);
        }
    }


    private void OnSettingsButtonClick()
    {
        //Debug.Log("Custom OnClick Event: Settings button clicked!");

        SettingsWindow.ShowWindow();
    }

    private void OnGenerateScriptButtonClick()
    {
        List<BaseObject> objects = objectPopUp.GetSelectedObjects();
        if (objects.Count > 0)
        {
            foreach(var baseObject in objects)
            {
                OnGenerateScriptEvent?.Invoke(baseObject);
            }
        }
        else
        {
            Debug.LogWarning("There is no selected script object to be generated.");
        }
    }

    private void OnGenerateGameObjectListButton(){
        Debug.Log("Wait for GOList generation.");
        OnGenerateGameObjectListEvent?.Invoke();
        //Debug.LogWarning("Bouton toujours pas implementé!!!");  
    }

    private void OnTestGOButtonClick()
    {
        Debug.Log("Custom OnClick Event: GO Test button clicked!");

        var GOJsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Editor/GeneratedContent/gameObjects_model.json");
        //C:\Users\simon\Documents\PFE\PFEUnityProject\Packages\UnityProtoChill\Editor\GeneratedContent\gameObjects_model.json
        if (GOJsonFile != null)
        {
            string jsonString = GOJsonFile.text;
            GenerativeProcess.SetJsonGOs(jsonString);
            GameObjectCreator.JsonToDictionary("");
            GameObjectCreator.StockEveryGOsInList();
        }
    }

    private void OnGenerateGameObjectButton(){
        List<string> gameObjectNames = GOPopUp.GetSelectedGameObjects();
        if (gameObjectNames.Count > 0)
        {
            foreach(var goName in gameObjectNames)
            {
                OnGenerateGameObjectEvent?.Invoke(goName);
            }
        }
        else
        {
            Debug.LogWarning("There is no selected game object to be generated.");
        }
        //Debug.LogWarning("Bouton toujours pas implementé");
    }

    public void SendMessageToChatWindow(string message)
    {
        OnMessageToChat?.Invoke(message);
    }
    public void AddChatResponse(string response){
        chatWindow.AddChatResponse(response);
    }


}

public class ObjectPopUp : PopupWindowContent
{
    private ScrollView scrollView;
    private List<BaseObject> selectedObjects = new List<BaseObject>();
    private bool allSelected = false;

    public override void OnOpen()
    {
        //Debug.Log("Popup opened: " + this);
    }

    public override VisualElement CreateGUI()
    {
        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/ObjectPopUp.uxml");
        var root = visualTreeAsset.CloneTree();
        scrollView = root.Q<ScrollView>("base-object-list");
        UpdateBaseObjectList();
        return root;
    }

    public override void OnClose()
    {
        //Debug.Log("Popup closed: " + this);
    }

    public void UpdateBaseObjectList()
    {
        Debug.Log("update obj list");
        scrollView.Clear();
        selectedObjects.Clear();

        if (ObjectResearch.AllBaseObjects.Count > 0)
        {
            foreach (var baseObject in ObjectResearch.AllBaseObjects)
            {
                var toggle = new Toggle(baseObject.Name)
                {
                    value = false
                };

                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        if (!selectedObjects.Contains(baseObject))
                        {
                            selectedObjects.Add(baseObject);
                            //Debug.Log($"{baseObject.Name} added to selection.");
                        }
                    }
                    else
                    {
                        if (selectedObjects.Contains(baseObject))
                        {
                            selectedObjects.Remove(baseObject);
                            //Debug.Log($"{baseObject.Name} removed from selection.");
                        }
                    }
                });
                scrollView.Add(toggle);
            }

            var allToggle = new Toggle("All");
            allToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    allSelected = true;
                }
                else
                {
                    allSelected = false;
                }
            });
            scrollView.Add(allToggle);
        }
        else
        {
            var emptyList = new Label("No base objects available");
            scrollView.Add(emptyList);
        }
    }

    public List<BaseObject> GetSelectedObjects()
    {
        if (allSelected)
        {
            return ObjectResearch.AllBaseObjects;
        }
        else
        {
            return selectedObjects;
        }
    }
}


public class GameObjectPopUp : PopupWindowContent
{
    private ScrollView scrollView;
    private List<string> selectedGameObjectNames = new List<string>();
    private bool allSelected = false;

    public override void OnOpen()
    {
        //Debug.Log("Popup opened: " + this);
    }

    public override VisualElement CreateGUI()
    {
        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/GameObjectPopUp.uxml");
        
        var root = visualTreeAsset.CloneTree();
        scrollView = root.Q<ScrollView>("game-object-list");
        if (scrollView == null){
            throw new Exception("Don't find scrollView");
        }
        UpdateGameObjectList();
        return root;
    }

    public override void OnClose()
    {
        //Debug.Log("Popup closed: " + this);
    }

    
    public void UpdateGameObjectList()
    {
        scrollView.Clear();
        selectedGameObjectNames.Clear();

        if (GameObjectCreator.GameObjectNameList != null && GameObjectCreator.GameObjectNameList.Count > 0)
        {
            foreach (var GOName in GameObjectCreator.GameObjectNameList)
            {
                var toggle = new Toggle(GOName)
                {
                    value = false
                };

                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        if (!selectedGameObjectNames.Contains(GOName))
                        {
                            selectedGameObjectNames.Add(GOName);
                            //Debug.Log($"{baseObject.Name} added to selection.");
                        }
                    }
                    else
                    {
                        if (selectedGameObjectNames.Contains(GOName))
                        {
                            selectedGameObjectNames.Remove(GOName);
                            //Debug.Log($"{baseObject.Name} removed from selection.");
                        }
                    }
                });
                scrollView.Add(toggle);
            }

            var allToggle = new Toggle("All");
            allToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    allSelected = true;
                }
                else
                {
                    allSelected = false;
                }
            });
            scrollView.Add(allToggle);
        }
        else
        {
            var emptyList = new Label("No game objects available");
            scrollView.Add(emptyList);
        }
    }

    public List<string> GetSelectedGameObjects()
    {
        if (allSelected)
        {
            return GameObjectCreator.GameObjectNameList;
            
        }
        else
        {
            return selectedGameObjectNames;
        }
    }
}