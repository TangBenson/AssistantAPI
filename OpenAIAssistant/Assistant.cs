using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAIAssistant
{
    public class Assistant
    {
        private readonly IConfiguration _configuration;
        private static string _apiKey = "xxxxxxxxx";

        public Assistant(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["OpenAIServiceOptions:ApiKey"];
        }

        public async Task CreateAssistant(string instructions, string name, List<object> tools, string model, List<string> file_ids)
        {
            var url = "https://api.openai.com/v1/assistants";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var jsonStr = JsonSerializer.Serialize(new
            {
                instructions,
                name,
                tools,
                model,
                file_ids
            });

            using var jsonContent = new StringContent(jsonStr,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

        //要查看有那些 Assistants 可以呼叫 ListAssistants()
        public async Task ListAssistants()
        {
            var url = "https://api.openai.com/v1/assistants?order=desc&limit=20";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"錯誤：{response.StatusCode} - {response.ReasonPhrase}");
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }
    }
}