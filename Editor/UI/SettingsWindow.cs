using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ChatGPTWrapper;
using System;

namespace SettingsClass
{

    public class SettingsWindow : EditorWindow
    {
        public static event System.Action<bool, string, string, CustomChatGPTConversation.Model, string, float> OnInitializeGPTInformation;
        private bool useProxy;
        private string proxyUri = "";
        private string apiKey = "";
        private float temperature;
        private CustomChatGPTConversation.Model selectedModel = CustomChatGPTConversation.Model.ChatGPT4;
        private string initialPrompt;
        private Vector2 topScrollPosition;

        public static void ShowWindow()
        {
            GetWindow<SettingsWindow>("GPT Settings");
        }

        private void OnGUI()
        {
            // SECTION INIT
            GUILayout.BeginVertical("box");
            GUILayout.Label("Initialize GPT Settings", EditorStyles.boldLabel);

            apiKey = EditorGUILayout.PasswordField("API Key", apiKey);
            if (apiKey == ""){
                apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                {
                    Debug.LogWarning("Variable d'environnement OPENAI_API_KEY introuvable ou vide.");
                }
                else
                {
                    Debug.Log("Variable d'environnement trouvée");
                    //Debug.Log($"Variable d'environnement trouvée : {apiKey}");
                }
            }
            selectedModel = (CustomChatGPTConversation.Model)EditorGUILayout.EnumPopup("Model", selectedModel);

            // GUILayout.Label("Initial Prompt", EditorStyles.label);
            // topScrollPosition = GUILayout.BeginScrollView(topScrollPosition, GUILayout.Height(90));
            // initialPrompt = EditorGUILayout.TextArea(initialPrompt, GUILayout.Height(70));
            // GUILayout.EndScrollView();

            //GUILayout.Space(15);
            //temperature = EditorGUILayout.Slider("Temperature", temperature, 0f, 2f);

            // Bouton INIT GPT
            if (GUILayout.Button("Initialize GPT", GUILayout.Height(40)))
            {
                InitializeGPTInformation();
                Debug.Log("GPT Initialized with user parameters.");
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);
        }

        private void InitializeGPTInformation()
        {
            // Le seul abonn� est l'instance de GPTGenerator, elle m�me instanci�e dans le constructeur de Generative process. On 
            // instancie donc Un GPTGenerator et un CustomChatGPTConversation par generative process. 
            OnInitializeGPTInformation?.Invoke(useProxy, proxyUri, apiKey, selectedModel, initialPrompt, temperature);
            
            // La sauvegarde se fait dans l'UI coming soon l'option pour ne pas le faire lolelol (infâme mais Michel posera la question 100u)
            GPTSettingsManager.SaveGPTSettings(useProxy, proxyUri, apiKey, selectedModel, initialPrompt, temperature);
        }
    }

}