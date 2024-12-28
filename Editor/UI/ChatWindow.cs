using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatClass
{

    public class ChatWindow : EditorWindow
    {
        private VisualElement chatCanvas;
        private TextField chatInputField;
        private string userInput = "type prompt here...";

        public VisualElement CreateChatView()
        {
            chatCanvas = new VisualElement();
            chatCanvas.style.position = Position.Absolute;
            chatCanvas.style.flexGrow = 1;

            chatInputField = new TextField();
            chatInputField.value = userInput;
            chatInputField.multiline = true;
            chatInputField.style.flexGrow = 1;

            chatCanvas.Add(chatInputField);

            return chatCanvas;
        }
    }

}