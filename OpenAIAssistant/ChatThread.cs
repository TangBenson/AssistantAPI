using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAIAssistant
{
    public class ChatThread
    {
        private static string _apiKey = "xxxxxxxxx";
        public ChatThread(string apiKey)
        {
            _apiKey = apiKey;
        }

        // 建立 Thread
        public async Task CreateThread()
        {
            var url = "https://api.openai.com/v1/threads";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            var response = await client.PostAsync(url, null);
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