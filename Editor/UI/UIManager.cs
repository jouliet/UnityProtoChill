using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UMLClassDiag;
using ChatClass;
using SettingsClass;
using static SaverLoader;

using ChatGPTWrapper;
public class UIManager : EditorWindow
{
    private VisualElement mainContainer;
    private VisualElement umlContainer;
    private VisualElement chatContainer;
    private VisualElement settingsContainer;

    public ChatWindow chatWindow;
    private UMLDiagramWindow umlDiagramWindow;
    private Button settingsButton;

    private Button resetButton;

    //public static event System.Action<BaseObject> OnGenerateScriptEvent;
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
            //LoadUML();
        }
        if (GPTGenerator.Instance._chatGPTConversation == null){
            GPTSettingsManager.LoadGPTSettings(() => {});
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

        InitializeGPTButton();
        InitializeResetUMLDataButton();

        rootContainer.Add(settingsContainer);

        // Set up main content
        mainContainer = new VisualElement();
        mainContainer.style.flexDirection = FlexDirection.Row;
        mainContainer.style.flexGrow = 1;

        // Set up UML Diagram View
        umlContainer = new VisualElement { name = "uml-container" };
        umlContainer.style.flexGrow = 7;
        umlContainer.style.width = new Length(70, LengthUnit.Percent);
        umlContainer.style.borderRightWidth = 3;
        umlContainer.style.borderRightColor = Color.gray;
        umlContainer.style.overflow = Overflow.Hidden;
        InitializeUMLView();
        mainContainer.Add(umlContainer);

        // Set up Chat View
        chatContainer = new VisualElement { name = "chat-window" };
        chatContainer.style.flexGrow = 3;
        chatContainer.style.flexDirection = FlexDirection.Column;
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
        umlCanvas.style.width = new Length(100, LengthUnit.Percent);
        umlContainer.Add(umlCanvas);
    }

    private void InitializeChatView()
    {
        chatWindow = ScriptableObject.CreateInstance<ChatClass.ChatWindow>();
        var chatCanvas = chatWindow.CreateChatView(this);
        chatCanvas.style.flexGrow = 1;
        chatCanvas.style.width = new Length(100, LengthUnit.Percent);
        chatContainer.Add(chatCanvas);
    }

    //
    // Options Initialization
    //

    private void InitializeGPTButton()
    {
        settingsButton = new Button() { text = "Settings" };
        settingsButton.clicked += OnSettingsButtonClick;
        settingsContainer.Add(settingsButton);
    }

    private void InitializeResetUMLDataButton(){
        resetButton = new Button() { text = "Reset" };
        resetButton.clicked += OnResetButtonClick;
        settingsContainer.Add(resetButton);
    }
    private void OnSettingsButtonClick()
    {
        //Debug.Log("Custom OnClick Event: Settings button clicked!");

        SettingsWindow.ShowWindow();
    }

    private void OnResetButtonClick(){
        RemoveJsonFiles();
    }

    public void SendMessageToChatWindow(string message)
    {
        OnMessageToChat?.Invoke(message);
    }
    public void AddChatResponse(string response){
        chatWindow.AddChatResponse(response);
    }


}