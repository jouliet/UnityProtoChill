using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static PromptEngineeringUtilities;

namespace ChatGPTWrapper
{
    public class CustomChatGPTConversation
    {
        private bool _useProxy;
        private string _proxyUri;
        private string _apiKey;
        public enum Model
        {
            ChatGPT4,
            Fine_Tuned_GPT4,
            GPT4
        }
        public Model _model = Model.ChatGPT4;
        private string _selectedModel = null;
        private int _maxTokens = 3000;
        private float _temperature = 0;
        private string _uri;
        private List<(string, string)> _reqHeaders;
        private Prompt _prompt;
        private Chat _chat;
        private string _lastUserMsg;
        private string _lastChatGPTMsg;

        // private string _chatbotName = "ChatGPT";
        private string _initialPrompt;

        public CustomChatGPTConversation(bool useProxy, string proxyUri, string apiKey, CustomChatGPTConversation.Model model, string initialPrompt, float temperature)
        {
            _temperature = temperature;
            _useProxy = useProxy;
            _proxyUri = proxyUri;
            _apiKey = apiKey;
            _model = model;
            //voili voilou
            _initialPrompt = staticOverridenInitialPrompt;

            _reqHeaders = new List<(string, string)>
            {
                ("Authorization", $"Bearer {_apiKey}"),
                ("Content-Type", "application/json")
            };

            SetModel(model);
        }

        public void SetModel(Model model)
        {
            _model = model;
            switch (_model)
            {
                case Model.ChatGPT4:
                    _chat = new Chat(_initialPrompt);
                    _uri = "https://api.openai.com/v1/chat/completions";
                    _selectedModel = "chatgpt-4o-latest";
                    break;
                case Model.Fine_Tuned_GPT4:
                    _chat = new Chat(_initialPrompt);
                    _uri = "https://api.openai.com/v1/chat/completions";
                    _selectedModel = "ft:gpt-4o-2024-08-06:personal::AwSP1AwC";
                    break;
                case Model.GPT4:
                    _chat = new Chat( _initialPrompt);
                    _uri = "https://api.openai.com/v1/chat/completions";
                    _selectedModel = "gpt-4o-mini";
                    break;
            }
        }

        public void SendToChatGPT(string message, System.Action<string> callback)
        {
            _lastUserMsg = message;

            // if (_model == Model.ChatGPT4)
            // {
                ChatGPTReq chatGPTReq = new ChatGPTReq
                {
                    model = _selectedModel,
                    max_tokens = _maxTokens,
                    temperature = _temperature,
                    messages = _chat.CurrentChat
                };
                chatGPTReq.messages.Add(new Message("user", message));

                string jsonPayload = JsonUtility.ToJson(chatGPTReq);

                PostRequest(_uri, jsonPayload, response =>
                {
                    var chatGPTRes = JsonUtility.FromJson<ChatGPTRes>(response);
                    _lastChatGPTMsg = chatGPTRes.choices[0].message.content;

                    //Gestion de la mémoire ici (actuellement 2)
                    _chat.AppendMessage(Chat.Speaker.User, _lastUserMsg);
                    _chat.AppendMessage(Chat.Speaker.ChatGPT, _lastChatGPTMsg);
                    callback?.Invoke(_lastChatGPTMsg);
                });
            // }

        }

        private void PostRequest(string uri, string jsonPayload, System.Action<string> callback)
        {
            UnityWebRequest request = new UnityWebRequest(uri, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            foreach (var header in _reqHeaders)
            {
                request.SetRequestHeader(header.Item1, header.Item2);
            }

            request.SendWebRequest().completed += operation =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"Request failed: {request.error}");
                }
            };
        }
    }
}
