using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

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

        public async Task<string> CreateAssistant(
            string instructions,
            string name,
            List<object> tools,
            string model,
            List<string> file_ids)
        {
            Console.WriteLine("CreateAssistant");
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
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;
                string id = root.GetProperty("id").GetString()!;
                return id;
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                return "";
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

        // 建立 Thread
        public async Task<string> CreateThread()
        {
            Console.WriteLine("CreateThread");
            var url = "https://api.openai.com/v1/threads";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            var response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;
                string id = root.GetProperty("id").GetString()!;
                return id;
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                return "";
            }
        }

        //發送使用者 Message
        public async Task CreateMessage(string threadId, string question)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var jsonStr = JsonSerializer.Serialize(new
            {
                role = "user",
                content = question
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

        //發送 Run 來讓 Assistant 處理使用者詢問的問題


        // 取回 Message
        public async Task ListMessages(string threadId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages?order=desc&limit=2";
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
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

        public async Task CreateRun(string threadId, string assistantId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var jsonStr = JsonSerializer.Serialize(new
            {
                assistant_id = assistantId
            });
            using var jsonContent = new StringContent(jsonStr,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;
                string runId = root.GetProperty("id").GetString()!;
                await RetrieveRun(threadId, runId);
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

        public async Task<string> RetrieveRun(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;
                string status = root.GetProperty("status").GetString()!;
                await RetrieveRun(threadId, runId);
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

        //將 Thread 刪除掉
        public async Task DeleteThread(string threadId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            var response = await client.DeleteAsync(url);
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
    }
}