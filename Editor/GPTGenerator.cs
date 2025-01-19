//This GPTGenerator uses the GPT Wrapper originally used in hello world project ... So no langchain and use of events with some consideration 
//for execution/data transfer time and API Key. The InitChatGPTConversation and GenerateFromText are called in UMLClass

using UnityEngine;
using System;
using SettingsClass;

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
        public GPTGenerator()
        {
            SettingsWindow.OnInitializeGPTInformation += InitChatGPTConversation;
            MyEditorWindow.OnInitializeGPTInformation += InitChatGPTConversation;
        }

        ~GPTGenerator()
        {
            SettingsWindow.OnInitializeGPTInformation -= InitChatGPTConversation;
            MyEditorWindow.OnInitializeGPTInformation -= InitChatGPTConversation;
        }
        private CustomChatGPTConversation _chatGPTConversation;

    
        // Init est Appelée par l'event de l'editor window auquel on s'est abonné
        public void InitChatGPTConversation(bool useProxy, string proxyUri, string apiKey, CustomChatGPTConversation.Model model, string initialPrompt)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Debug.LogError("API key is required to initialize ChatGPTConversation.");
                return;
            }

            _chatGPTConversation = new CustomChatGPTConversation(useProxy, proxyUri, apiKey, model, initialPrompt);

            Debug.Log("ChatGPTConversation initialized successfully.");
        }

        // Appelé par UML Diag
        public void GenerateFromText(string text, Action<string> onResponse)
        {
            if (_chatGPTConversation == null)
            {
                Debug.LogError("ChatGPTConversation is not initialized. Call InitChatGPTConversation first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogError("Input text cannot be null or empty.");
                return;
            }

            _chatGPTConversation.SendToChatGPT(text, (response) =>
            {
                Debug.Log($"Message Sent: {text}");
                Debug.Log($"Message Received: {response}");
                onResponse?.Invoke(response);
            });
        
        }
    }
}
