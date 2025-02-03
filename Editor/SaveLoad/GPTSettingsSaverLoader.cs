using System;
using System.IO;
using UnityEngine;
using ChatGPTWrapper;

public class GPTSettingsManager
{
    private static string settingsFilePath = Path.Combine(Application.persistentDataPath, "GPTSettings.json");

    public static void SaveGPTSettings(bool useProxy, string proxyUri, string apiKey, CustomChatGPTConversation.Model model, string initialPrompt, float temperature)
    {
        try
        {

            string directory = Path.GetDirectoryName(settingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GPTSettings settings = new GPTSettings
            {
                UseProxy = useProxy,
                ProxyUri = proxyUri,
                ApiKey = apiKey,
                Model = model,
                InitialPrompt = initialPrompt,
                Temperature = temperature
            };

            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(settingsFilePath, json);
            Debug.Log("GPT settings saved at: " + settingsFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving GPT settings: " + ex.Message);
        }
    }
 
    public static GPTSettings LoadGPTSettings(Action onSettingsLoaded, GPTGenerator gptGenerator)
    {
        try
        {
            if (!File.Exists(settingsFilePath))
            {
                return null;
            }

            string json = File.ReadAllText(settingsFilePath);
            GPTSettings settings = JsonUtility.FromJson<GPTSettings>(json);
            //Debug.Log(settings.Model);
            if (settings != null) 
            {
                gptGenerator.InitChatGptConversationAsynchroneFriendly(settings.UseProxy, settings.ProxyUri, settings.ApiKey, settings.Model, settings.InitialPrompt, settings.Temperature, () =>
                {
                    
                    if (gptGenerator._chatGPTConversation != null) {
                        onSettingsLoaded?.Invoke();
                    }
                    else {
                        Debug.LogError("GPT seems to have initialization issues");
                    }
                    
                });
            }
            else
            {
                Debug.LogError("Failed to parse GPT settings.");
            }
            return settings;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading GPT settings: " + ex.Message);
            return null;
        }
    }


    [Serializable]
    public class GPTSettings
    {
        public bool UseProxy;
        public string ProxyUri;
        public string ApiKey;
        public CustomChatGPTConversation.Model Model;
        public string InitialPrompt;
        public float Temperature;
    }
}