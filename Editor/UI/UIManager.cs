using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UMLClassDiag;
using ChatClass;

using System.IO;
using System.Collections.Generic;
using static JsonParser;

public class UIManager : EditorWindow
{
    private VisualElement mainContainer;
    private VisualElement umlContainer;
    private VisualElement chatContainer;
    private VisualElement settingsContainer;

    private ChatWindow chatWindow;
    private UMLDiagramWindow umlDiagramWindow;

    private Button testButton;

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

        testButton = new Button() { text = "Test" };
        testButton.clicked += OnTestButtonClick;
        settingsContainer.Add(testButton);

        rootContainer.Add(settingsContainer);

        // Set up main content
        mainContainer = new VisualElement();
        mainContainer.style.flexDirection = FlexDirection.Row;
        mainContainer.style.flexGrow = 1;

        // Set up UML Diagram View
        umlContainer = new VisualElement { name = "uml-container" };
        umlContainer.style.flexGrow = 7;
        umlContainer.style.borderRightWidth = 2;
        umlContainer.style.borderRightColor = Color.gray;
        umlContainer.style.paddingRight = 5;
        umlContainer.style.overflow = Overflow.Hidden;
        InitializeUMLView();
        mainContainer.Add(umlContainer);

        // Set up Chat View
        chatContainer = new VisualElement { name = "chat-container" };
        chatContainer.style.flexGrow = 3;
        chatContainer.style.paddingLeft = 3;
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


}