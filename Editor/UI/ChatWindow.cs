using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatClass
{

    public class ChatWindow : EditorWindow
    {
        private VisualTreeAsset chatVisualTree;
        private StyleSheet chatStyleSheet;

        private VisualElement chatCanvas;

        public VisualElement CreateChatView()
        {
            chatVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uxml");
            chatStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jin.protochill/Editor/UI/ChatWindow.uss");

            chatCanvas = chatVisualTree.CloneTree();
            chatCanvas.style.flexGrow = 1;
            chatCanvas.styleSheets.Add(chatStyleSheet);

            return chatCanvas;
        }
    }

}