using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ChatGPTWrapper;

namespace SettingsClass
{

    public class SettingsWindow : EditorWindow
    {
        private VisualTreeAsset settingsVisualTree;
        private StyleSheet settingsStyleSheet;

        private VisualElement settingsCanvas;

        public static event System.Action<bool, string, string, CustomChatGPTConversation.Model, string> OnInitializeGPTInformation;
        private bool useProxy;
        private string proxyUri = "";
        private string apiKey = "";
        private CustomChatGPTConversation.Model selectedModel = CustomChatGPTConversation.Model.ChatGPT4;
        private string initialPrompt = @"
You are both an oriented object and gameobject oriented beast. You only use functions defined in the json or native to Unity.
Never assume a method, class or function exists without explicitly seeing it in the UML diagram you are presented. When building said diagram always be exhaustive with the links between classes and always prefer making them go both ways";

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
            selectedModel = (CustomChatGPTConversation.Model)EditorGUILayout.EnumPopup("Model", selectedModel);

            GUILayout.Label("Initial Prompt", EditorStyles.label);
            topScrollPosition = GUILayout.BeginScrollView(topScrollPosition, GUILayout.Height(90));
            initialPrompt = EditorGUILayout.TextArea(initialPrompt, GUILayout.Height(70));
            GUILayout.EndScrollView();

            GUILayout.Space(15);

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
            // Le seul abonné est l'instance de GPTGenerator, elle même instanciée dans le constructeur de Generative process. On 
            // instancie donc Un GPTGenerator et un CustomChatGPTConversation par generative process. 
            OnInitializeGPTInformation?.Invoke(useProxy, proxyUri, apiKey, selectedModel, initialPrompt);
        }
    }

}