using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UMLClassDiag;
using ChatClass;

public class UIManager : EditorWindow
{
    private VisualElement umlContainer;
    private VisualElement chatContainer;

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
        rootContainer.style.flexDirection = FlexDirection.Row;
        rootContainer.style.flexGrow = 1;

        // Set up UML Diagram View
        umlContainer = new VisualElement { name = "uml-container" };
        umlContainer.style.flexGrow = 6; // Relative size: 6 parts out of 10
        umlContainer.style.borderRightWidth = 2;
        umlContainer.style.borderRightColor = Color.white;
        umlContainer.style.paddingRight = 5;
        InitializeUMLView();
        rootContainer.Add(umlContainer);

        // Set up Chat View
        chatContainer = new VisualElement { name = "chat-container" };
        chatContainer.style.flexGrow = 4;
        chatContainer.style.paddingRight = 5;
        InitializeChatView();
        rootContainer.Add(chatContainer);

        // Add the root container to the window
        rootVisualElement.Clear();
        rootVisualElement.Add(rootContainer);
    }

    private void InitializeUMLView()
    {
        var umlDiagramWindow = ScriptableObject.CreateInstance<UMLClassDiag.UMLDiagramWindow>();
        var umlCanvas = umlDiagramWindow.CreateDiagramView();
        umlContainer.Add(umlCanvas);
    }

    private void InitializeChatView()
    {
        var chatWindow = ScriptableObject.CreateInstance<ChatClass.ChatWindow>();
        var chatCanvas = chatWindow.CreateChatView();
        chatContainer.Add(chatCanvas);
    }

}