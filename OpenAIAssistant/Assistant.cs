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

        internal static async Task<string> CreateAssistant(
            string instructions,
            string name,
            List<object> tools,
            string model,
            List<string> file_ids)
        {
            var url = "https://api.openai.com/v1/assistants";
            var jsonStr = JsonSerializer.Serialize(new
            {
                instructions,
                name,
                tools,
                model,
                file_ids
            });
            var responseRoot = await CallAPI(url, jsonStr, "POST");
            return responseRoot.ValueKind == JsonValueKind.Undefined ?
                    "" :
                    responseRoot.GetProperty("id").GetString()!;
        }

        //要查看有那些 Assistants 可以呼叫 ListAssistants()
        internal static async Task<string> ListAssistants()
        {
            var url = "https://api.openai.com/v1/assistants?order=desc&limit=20";

            var responseRoot = await CallAPI(url, null, "GET");
            return(
                responseRoot.ValueKind == JsonValueKind.Undefined ?
                "" :
                responseRoot.GetProperty("data").ToString()!);
        }

        // 建立 Thread
        internal static async Task<string> CreateThread()
        {
            var url = "https://api.openai.com/v1/threads";

            var responseRoot = await CallAPI(url, null, "GET");
            return responseRoot.ValueKind == JsonValueKind.Undefined ?
                    "" :
                    responseRoot.GetProperty("id").GetString()!;
        }

        //發送使用者 Message
        internal static async Task CreateMessage(string threadId, string question)
        {
            Console.WriteLine($"發送msg給LLM");
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            var jsonStr = JsonSerializer.Serialize(new
            {
                role = "user",
                content = question
            });

            var responseRoot = await CallAPI(url, jsonStr, "POST");
            Console.WriteLine(
                responseRoot.ValueKind == JsonValueKind.Undefined ?
                "" :
                responseRoot.GetString()!);
        }

        // 取回 Message
        internal static async Task ListMessages(string threadId)
        {
            Console.WriteLine($"取得LLM回傳的msg");
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages?order=desc&limit=2";

            var responseRoot = await CallAPI(url, null, "GET");

            Console.WriteLine(
                responseRoot.ValueKind == JsonValueKind.Undefined ?
                "" :
                responseRoot.GetString()!);
        }

        //發送 Run 來讓 Assistant 處理使用者詢問的問題
        internal static async Task CreateRun(string threadId, string assistantId)
        {
            Console.WriteLine($"執行Run");
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs";
            var jsonStr = JsonSerializer.Serialize(new
            {
                thread_id = threadId,
                assistant_id = assistantId
            });
            var responseRoot = await CallAPI(url, jsonStr, "POST");

            if (responseRoot.ValueKind != JsonValueKind.Undefined)
            {
                string runId = responseRoot.GetProperty("id").GetString()!;
                while (true)
                {
                    var status = await RetrieveRun(threadId, runId);
                    await ListRunSteps(threadId, runId);

                    while (status == "queued" || status == "in_progress")
                    {
                        await Task.Delay(2000);
                        status = await RetrieveRun(threadId, runId);
                    }
                    if (status == "requires_action")
                    {
                        //呼叫function來處理

                    }
                    else if (status == "error" || status == "failed")
                    {
                        break;
                    }
                }
            }
        }

        internal static async Task<string> RetrieveRun(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}";
            var responseRoot = await CallAPI(url, null, "get");
            string status = responseRoot.GetProperty("status").GetString()!;
            return status;
        }

        internal static async Task ListRunSteps(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}/steps";
            var responseRoot = await CallAPI(url, null, "get");
            Console.WriteLine(
                responseRoot.ValueKind == JsonValueKind.Undefined ?
                default :
                responseRoot!);
        }

        //將 Thread 刪除掉
        internal static async Task DeleteThread(string threadId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "Authorization",
                $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add(
                "OpenAI-Beta",
                "assistants=v1");

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

        private static async Task<JsonElement> CallAPI(
            string url,
            string jsonStr,
            string method)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            using var jsonContent = new StringContent(jsonStr,
                Encoding.UTF8,
                "application/json");

            var response = method == "POST" ? 
                await client.PostAsync(url, jsonContent) : 
                await client.GetAsync(url);
            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {
                //從HTTP響應中讀取內容並轉換為字符串
                var responseBody = await response.Content.ReadAsStringAsync();
                //將JSON字符串解析為 JsonDocument
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                //獲取解析後的JSON文檔的根元素
                JsonElement root = doc.RootElement;
                return root;
            }
            else
            {
                Console.WriteLine($"OpenAI API回傳錯誤:{response.StatusCode}-{response.ReasonPhrase}");
                return default;
            }
        }
    }
}