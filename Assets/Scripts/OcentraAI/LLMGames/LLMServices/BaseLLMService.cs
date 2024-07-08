using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OcentraAI.LLMGames.LLMServices
{
    public abstract class BaseLLMService : ILLMService
    {
        protected string Endpoint { get; set; }
        protected string ApiKey { get; set; }
        protected string ApiUrl { get; set; }
        protected string Model { get; set; }
        protected int MaxTokens { get; set; }
        protected double Temperature { get; set; }
        protected bool Stream { get; set; }

        protected BaseLLMService(LLMConfig config)
        {
            Endpoint = config.Endpoint;
            ApiKey = config.ApiKey;
            ApiUrl = config.ApiUrl;
            Model = config.Model;
            MaxTokens = config.MaxTokens;
            Temperature = config.Temperature;
            Stream = config.Stream;
        }

        public async Task<string> GetResponseAsync(string systemMessage, string userPrompt)
        {
            try
            {
                var requestContent = GenerateRequestContent(systemMessage, userPrompt);
                var jsonData = JsonConvert.SerializeObject(requestContent);
                var webRequest = new UnityWebRequest(Endpoint + ApiUrl, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", "Bearer " + ApiKey);

                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    return ProcessResponse(webRequest.downloadHandler.text);
                }
                else
                {
                    string errorContent = webRequest.downloadHandler.text;
                    Debug.LogError($"Error calling LLM API: {webRequest.responseCode} - {webRequest.error} - {errorContent}");
                    return $"Error";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception occurred: {ex.Message}");
                return $"Error";
            }
        }

        protected abstract string ProcessResponse(string jsonResponse);

        protected virtual object GenerateRequestContent(string systemMessage, string userPrompt)
        {
            return new
            {
                messages = new[]
                {
                    new { role = "system", content = systemMessage},
                    new { role = "user", content = userPrompt }
                },
                temperature = Temperature,
                max_tokens = MaxTokens,
                stream = Stream
            };
        }

  
    }
}
