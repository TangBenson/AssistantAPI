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
        // private readonly string file1Id = "file-9xpwQr4rni98mUcrQtlkDkvc";
        // private readonly string instructions = @"你是GSS的請假助手，你的任務是基於上傳的檔案內容，回答使用者的問題。
        //                                         任務說明:
        //                                         - 當使用者提問時，仔細分析問題並基於上傳的檔案內容給出回答。
        //                                         - 如果上傳的檔案內容沒有能回答使用者問題的參考內容，請直接回答'我不知道'";
        // private readonly string assistantName = "GSS的請假助手";
        // //code_interpreter, retrieval, or function
        // private readonly object tools = new List<object> { new { type = "retrieval" } };
        // private readonly string model = "gpt-3.5-turbo-1106";
        // private readonly List<string> file_ids = new List<string> { file1Id };
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