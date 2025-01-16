using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UMLClassDiag;
using ChatClass;
using SettingsClass;
using System.IO;
using System.Collections.Generic;
using static JsonParser;
using static SaverLoader;

public class UIManager : EditorWindow
{
    private VisualElement mainContainer;
    private VisualElement umlContainer;
    private VisualElement chatContainer;
    private VisualElement settingsContainer;

    private ChatWindow chatWindow;
    private UMLDiagramWindow umlDiagramWindow;

    private Button testButton;
    private Button settingsButton;

    [MenuItem("Window/ProtoChill")]
    public static void ShowWindow()
    {
        GetWindow<UIManager>("ProtoCHILL");
    }

    private void OnEnable()
    {
        CreateLayout();
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

    private void InitializeUMLView()
    {
        umlDiagramWindow = ScriptableObject.CreateInstance<UMLClassDiag.UMLDiagramWindow>();
        var umlCanvas = umlDiagramWindow.CreateDiagramView();
        umlContainer.Add(umlCanvas);
    }

    private void InitializeChatView()
    {
        chatWindow = ScriptableObject.CreateInstance<ChatClass.ChatWindow>();
        var chatCanvas = chatWindow.CreateChatView();
        chatContainer.Add(chatCanvas);
    }

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

    private void OnTestButtonClick()
    {
        Debug.Log("Custom OnClick Event: Test button clicked!");

        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.jin.protochill/Tests/JsonMockUp.json");
        if (jsonFile != null)
        {
            string jsonString = jsonFile.text;
            Dictionary<string, object> parsedObject = (Dictionary<string, object>)Parse(jsonString);
            BaseObject baseObject = JSONMapper.MapToBaseObject((Dictionary<string, object>)parsedObject["Root"]);
            umlDiagramWindow.ReloadDiagram(baseObject);
        }
    }

    private void OnSettingsButtonClick()
    {
        Debug.Log("Custom OnClick Event: Settings button clicked!");

        SettingsWindow.ShowWindow();
    }
}