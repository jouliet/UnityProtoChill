using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;


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
        private static string DialoguePath = "Packages/com.jin.protochill/Editor/GeneratedContent/Dialogue.json"; 
        private static string DialogueModelPath = "Packages/com.jin.protochill/Editor/GeneratedContent/DialogueModel.json"; 
        List<DialogueMessage> dialogueMemory = new List<DialogueMessage>();


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


            ReloadChat();
            return chatCanvas;
        }

        private void OnSubmitButtonClick()
        {
            userInput = inputField.value;
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                // Actuellement le seul abonn� est l'instance de UMLDiag de main.
                OnSubmitText?.Invoke(userInput);

                AddUserMessage(userInput);
                
                userInput = "";
                inputField.value = "";
            }
        }
        
        private void AddUserMessage(string message){
            VisualElement bubble = new VisualElement();
            bubble.AddToClassList("chat-user-bubble");
            TextField content = new TextField();
            content.AddToClassList("chat-text");
            content.multiline = true;
            content.value = message;
            content.isReadOnly = true;
            bubble.Add(content);

            chatContainer.Add(bubble);
            chatContainer.ScrollTo(bubble);
            
            dialogueMemory.Add(new DialogueMessage { Message = message, TypeMessage = "user" });
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

            dialogueMemory.Add(new DialogueMessage { Message = message, TypeMessage = "user" });
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

            dialogueMemory.Add(new DialogueMessage { Message = responseText, TypeMessage = "llm" });
            ConvertToJSON(dialogueMemory);
        }


        public void ConvertToJSON(List<DialogueMessage> dialogueMemory)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\n\"Dialogue\":[");

            for (int i = 0; i < dialogueMemory.Count; i++)
            {
                var entry = dialogueMemory[i];
                jsonBuilder.Append("{\n\"message\":\"").Append(entry.Message.Replace("\"", "")).Append("\",");
                jsonBuilder.Append("\"typeMessage\":\"").Append(entry.TypeMessage.Replace("\"", "")).Append("\"").Append("}");
                
                if (i < dialogueMemory.Count - 1)

                {
                    jsonBuilder.Append(",");
                }
            }
            jsonBuilder.Append("]\n}");
            File.WriteAllText(DialoguePath, jsonBuilder.ToString());
        }

        public void ReloadChat(){
            try{
                if (!File.Exists(DialoguePath)){
                    return;
                }
                string jsonString = File.ReadAllText(DialoguePath);
                Dictionary<string, object> jsonDict = (Dictionary<string, object>)JsonParser.Parse(jsonString);
                List<object> jsonMessages = (List<object>)jsonDict["Dialogue"];
                foreach(Dictionary<string, object> jsonMessage in jsonMessages)
                {
                    if (jsonMessage["typeMessage"].ToString() == "user"){
                        AddUserMessage(jsonMessage["message"].ToString());
                    }else if (jsonMessage["typeMessage"].ToString() == "llm"){
                        AddChatResponse(jsonMessage["message"].ToString());
                    }
                }
            }catch (Exception ex){
                Debug.LogError("Il y a une erreur dans le reload du chat. \n Exception: \n" + ex );
            }
        }

        public void DeleteMessages(){
            if (File.Exists(DialoguePath)){
                File.Delete(DialoguePath);
            }
        }
    }

    public class DialogueMessage{
        public string Message { get; set; }
        public string TypeMessage { get; set; }
    }
}