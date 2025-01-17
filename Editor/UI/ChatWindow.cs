using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ChatGPTWrapper;

namespace ChatClass
{

    public class ChatWindow : EditorWindow
    {
        private VisualTreeAsset chatVisualTree;
        private StyleSheet chatStyleSheet;

        private VisualElement chatCanvas;

        public static event System.Action<string> OnSubmitText;
        private TextField inputField;
        private string userInput = "";
        private Button submitButton;

        private ScrollView chatContainer;

        public VisualElement CreateChatView()
        {
            chatVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uxml");
            chatStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uss");

            chatCanvas = chatVisualTree.CloneTree();
            chatCanvas.style.flexGrow = 1;
            chatCanvas.styleSheets.Add(chatStyleSheet);

            inputField = chatCanvas.Q<TextField>("chat-input-field");
            submitButton = chatCanvas.Q<Button>("chat-submit-button");
            chatContainer = chatCanvas.Q<ScrollView>("chat-messages-container");

            submitButton.clicked += OnSubmitButtonClick;

            return chatCanvas;
        }

        private void OnSubmitButtonClick()
        {
            userInput = inputField.value;
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                // Actuellement le seul abonné est l'instance de UMLDiag de main.
                OnSubmitText?.Invoke("Make a UML for the system :" + userInput);

                VisualElement bubble = new VisualElement();
                bubble.AddToClassList("chat-user-bubble");
                TextField content = new TextField();
                content.AddToClassList("chat-user-text");
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
    }

}