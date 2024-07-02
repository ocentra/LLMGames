using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ThreeCardBrag.LLMService
{
    public abstract class BaseLLMService : ILLMService
    {
        protected string Endpoint { get; set; }
        protected string ApiKey { get; set; }
        protected string ApiUrl { get; set; }
        protected string Model { get; set; }
        protected int MaxTokens { get; set; }
        protected Double Temperature { get; set; }
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

        public async Task<string> GetResponseAsync(string prompt)
        {
            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Endpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            object requestContent = GenerateRequestContent(prompt);

            StringContent content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return ProcessResponse(responseContent);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error calling LLM API: {response.StatusCode} - {errorContent}");
            }
        }

        protected abstract string ProcessResponse(string jsonResponse);
        protected virtual object GenerateRequestContent(string prompt)
        {
            return new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                },
                temperature = Temperature,
                max_tokens = MaxTokens,
                stream = Stream
            };
        }
    }
}