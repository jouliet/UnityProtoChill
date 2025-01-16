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
        private string userInput = "";

        public VisualElement CreateChatView()
        {
            chatVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uxml");
            chatStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uss");

            chatCanvas = chatVisualTree.CloneTree();
            chatCanvas.style.flexGrow = 1;
            chatCanvas.styleSheets.Add(chatStyleSheet);

            return chatCanvas;
        }

        private void SubmitText()
        {
            // Actuellement le seul abonné est l'instance de UMLDiag de main.
            OnSubmitText?.Invoke("Make a UML for the system :" + userInput);
        }
    }

}