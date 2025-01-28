using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ChatGPTWrapper;
using static UIManager;

namespace ChatClass
{

    public class ChatWindow : EditorWindow
    {
        private UIManager uiManager;

        private VisualTreeAsset chatVisualTree;
        private StyleSheet chatStyleSheet;

        private VisualElement chatCanvas;

        public static event System.Action<string> OnSubmitText;
        private TextField inputField;
        private string userInput = "";
        private Button submitButton;

        private ScrollView chatContainer;

        public VisualElement CreateChatView(UIManager manager)
        {
            uiManager = manager;

            chatVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uxml");
            chatStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uss");

            chatCanvas = chatVisualTree.CloneTree();
            chatCanvas.style.flexGrow = 1;
            chatCanvas.styleSheets.Add(chatStyleSheet);

            inputField = chatCanvas.Q<TextField>("chat-input-field");
            submitButton = chatCanvas.Q<Button>("chat-submit-button");
            chatContainer = chatCanvas.Q<ScrollView>("chat-messages-container");

            submitButton.clicked += OnSubmitButtonClick;
            uiManager.OnMessageToChat += PushProtochillMessage;

            return chatCanvas;
        }

        private void OnSubmitButtonClick()
        {
            userInput = inputField.value;
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                // Actuellement le seul abonn� est l'instance de UMLDiag de main.
                OnSubmitText?.Invoke("Make a UML for the system :" + userInput);

                VisualElement bubble = new VisualElement();
                bubble.AddToClassList("chat-user-bubble");
                TextField content = new TextField();
                content.AddToClassList("chat-text");
                content.multiline = true;
                content.value = userInput;
                content.isReadOnly = true;
                bubble.Add(content);

                chatContainer.Add(bubble);
                chatContainer.ScrollTo(bubble);

                userInput = "";
                inputField.value = "";
            }
        }

        private void PushProtochillMessage(string message)
        {
            VisualElement bubble = new VisualElement();
            bubble.AddToClassList("chat-protochill-bubble");
            TextField content = new TextField();
            content.AddToClassList("chat-text");
            content.multiline = true;
            content.value = message;
            content.isReadOnly = true;
            bubble.Add(content);

            chatContainer.Add(bubble);
            chatContainer.ScrollTo(bubble);
        }
        
        public void AddChatResponse(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
                return;


            VisualElement bubble = new VisualElement();
            bubble.AddToClassList("chat-response-bubble");
            TextField content = new TextField();
            content.AddToClassList("chat-response-text");

            content.multiline = true;
            content.value = responseText;
            content.isReadOnly = true;
            content.style.flexGrow = 1;
            content.style.whiteSpace = WhiteSpace.Normal;
            bubble.Add(content);

            chatContainer.Add(bubble);
            chatContainer.ScrollTo(bubble);
        }
    }

}