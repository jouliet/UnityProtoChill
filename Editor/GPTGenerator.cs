//This GPTGenerator uses the GPT Wrapper originally used in hello world project ... So no langchain and use of events with some consideration 
//for execution/data transfer time and API Key. The InitChatGPTConversation and GenerateFromText are called in UMLClass
using UnityEditor;
using UnityEngine;
using System;
using SettingsClass;
using ChatClass;
using System.Linq;
using static JsonParser;

namespace ChatGPTWrapper
{
    public class GPTGenerator
    {
    private static GPTGenerator _instance;

    public static GPTGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GPTGenerator();
            }
            return _instance;
        }
    }

    public UIManager uIManager;
        private GPTGenerator()
        {
            SettingsWindow.OnInitializeGPTInformation += InitChatGPTConversation;
        }

        ~GPTGenerator()
        {
            SettingsWindow.OnInitializeGPTInformation -= InitChatGPTConversation;
        }
        public CustomChatGPTConversation _chatGPTConversation;
        
    
        // Init est Appelée par l'event de l'editor window auquel on s'est abonné

        public void InitChatGptConversationAsynchroneFriendly(
            bool useProxy, 
            string proxyUri, 
            string apiKey, 
            CustomChatGPTConversation.Model model, 
            string initialPrompt, 
            float temperature, 
            Action onInitializedCallback)
            {
                if (_chatGPTConversation ==null){
                    _chatGPTConversation = new CustomChatGPTConversation(useProxy, proxyUri, apiKey, model, initialPrompt, temperature);

                    if (_chatGPTConversation != null)
                    {
                        onInitializedCallback?.Invoke();
                    
                    }
                    else {
                        Debug.LogError("GPT Settings have big troubles loading");
                    }
                }
            }
 

            public void InitChatGPTConversation(
                bool useProxy, 
                string proxyUri, 
                string apiKey, 
                CustomChatGPTConversation.Model model, 
                string initialPrompt, 
                float temperature)
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    Debug.LogError("API key is required to initialize ChatGPTConversation.");
                    return; 
                }  

                // Perform initialization of the chat conversation
                _chatGPTConversation = new CustomChatGPTConversation(
                    useProxy, proxyUri, apiKey, model, initialPrompt, temperature);

                Debug.Log("ChatGPTConversation initialized successfully. This does not mean the key is correct");
            }

        // Appelé par UML Diag
        public void GenerateFromText(string text, Action<string> onResponse)
        {
            if (_chatGPTConversation == null)
            {
                Debug.LogError("ChatGPTConversation is not initialized. Trying to load user data...");
                
                GPTSettingsManager.LoadGPTSettings(() =>
                {
                    
                    if (_chatGPTConversation != null){
                        Debug.Log("Settings Loaded, now proceeding with GenerateFromText...");
                        SendTextToChatGPT(text, onResponse);
                    }
                    else {
                        Debug.LogWarning("Settings loading failed, you might wanna try manually entering your settings. En sommes, nul ici malgré la réussite des étapes précédentes");
                    }
                        
                }); 
            }
            else {
                SendTextToChatGPT(text, onResponse);
            }
        }

        private void SendTextToChatGPT(string text, Action<string> onResponse)
        {
            


            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogError("Input text cannot be null or empty.");
                return;
            }

            text += ".  Use json and user markers : ```user ... ``` to explain simply what is your games and how can it evolve and ```json ... ``` for formatted content. Include only one of each, your answer should have 1 user summary and 1 json content only";

            _chatGPTConversation.SendToChatGPT(text, (response) =>
            {
                Debug.Log($"Message Sent: {text}");
                Debug.Log($"Message Received: {response}");

                //Triste mais pas trouvé mieux, le setter marche po for some reason (surement le lifecycle sus des editor windows)
                uIManager = Resources.FindObjectsOfTypeAll<UIManager>().FirstOrDefault();

                if (uIManager != null){
                    uIManager.AddChatResponse(GetUserResponse(response));
                }
                else{
                    Debug.Log("No uIManager");
                }
                onResponse?.Invoke(response);
            });
        }


        public void setUIManager(UIManager manager){
            uIManager = manager;
        }

        public bool isGPTInitialized(){
            return _chatGPTConversation!=null;
        }
        
    }
}
