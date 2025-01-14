using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OcentraAI.LLMGames.LLMServices
{
    public abstract class BaseLLMService<T> : ScriptableSingletonBase<T>, ILLMService where T : GlobalConfig<T>, new()
    {
        public abstract ILLMProvider Provider { get; protected set; }
        [ShowInInspector,ReadOnly] public ILLMConfig LLMConfig { get; set; } = new LLMConfig();

        public async UniTask InitializeAsync(ILLMConfig config)
        {
            if (Equals(config.Provider, Provider))
            {
                LLMConfig = config;
            }
            
            await base.InitializeAsync();
            await UniTask.Yield();
        }

        public async UniTask<string> GetResponseAsync(string systemMessage, string userPrompt)
        {
            try
            {
                object requestContent = GenerateRequestContent(systemMessage, userPrompt);
                string jsonData = JsonConvert.SerializeObject(requestContent);
                UnityWebRequest webRequest = new UnityWebRequest(LLMConfig.Endpoint + LLMConfig.ApiUrl, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", "Bearer " + LLMConfig.ApiKey);

                UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    return ProcessResponse(webRequest.downloadHandler.text);
                }

                string errorContent = webRequest.downloadHandler.text;
                Debug.LogError(
                    $"Error calling LLM API: {webRequest.responseCode} - {webRequest.error} - {errorContent}");
                return "Error";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred: {ex.Message}");
                return "Error";
            }
        }

        protected abstract string ProcessResponse(string jsonResponse);

        protected virtual object GenerateRequestContent(string systemMessage, string userPrompt)
        {
            return new
            {
                messages = new[]
                {
                    new {role = "system", content = systemMessage}, new {role = "user", content = userPrompt}
                },
                temperature = LLMConfig.Temperature,
                max_tokens = LLMConfig.MaxTokens,
                stream = LLMConfig.Stream
            };
        }
    }
}