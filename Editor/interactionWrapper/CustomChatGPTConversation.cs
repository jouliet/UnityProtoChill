using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // For HTTP requests in Unity

namespace ChatGPTWrapper
{
    public class CustomChatGPTConversation
    {
        private bool _useProxy;
        private string _proxyUri;
        private string _apiKey;

        public enum Model
        {
            ChatGPT,
            Davinci,
            Curie
        }
        public Model _model = Model.ChatGPT;
        private string _selectedModel = null;
        private int _maxTokens = 1500;
        private float _temperature = 0;
        private string _uri;
        private List<(string, string)> _reqHeaders;

        private Prompt _prompt;
        private Chat _chat;
        private string _lastUserMsg;
        private string _lastChatGPTMsg;

        private string _chatbotName = "ChatGPT";
        private string _initialPrompt = "You are ChatGPT, a large language model trained by OpenAI.";

        public CustomChatGPTConversation(bool useProxy, string proxyUri, string apiKey, CustomChatGPTConversation.Model model, string initialPrompt)
        {
            _useProxy = useProxy;
            _proxyUri = proxyUri;
            _apiKey = apiKey;
            _model = model;
            _initialPrompt = initialPrompt;

            _reqHeaders = new List<(string, string)>
            {
                ("Authorization", $"Bearer {_apiKey}"),
                ("Content-Type", "application/json")
            };

            SetModel(_model);
        }

        public void SetModel(Model model)
        {
            _model = model;
            switch (_model)
            {
                case Model.ChatGPT:
                    _chat = new Chat(_initialPrompt);
                    _uri = "https://api.openai.com/v1/chat/completions";
                    _selectedModel = "chatgpt-4o-latest";
                    break;
                case Model.Davinci:
                    _prompt = new Prompt(_chatbotName, _initialPrompt);
                    _uri = "https://api.openai.com/v1/completions";
                    _selectedModel = "text-davinci-003";
                    break;
                case Model.Curie:
                    _prompt = new Prompt(_chatbotName, _initialPrompt);
                    _uri = "https://api.openai.com/v1/completions";
                    _selectedModel = "text-curie-001";
                    break;
            }
        }

        public void SendToChatGPT(string message, System.Action<string> callback)
        {
            _lastUserMsg = message;

            if (_model == Model.ChatGPT)
            {
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
                    _chat.AppendMessage(Chat.Speaker.User, _lastUserMsg);
                    _chat.AppendMessage(Chat.Speaker.ChatGPT, _lastChatGPTMsg);
                    callback?.Invoke(_lastChatGPTMsg);
                });
            }
            // Similar handling for other models...
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
