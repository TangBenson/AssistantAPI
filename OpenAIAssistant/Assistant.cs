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
        private readonly string? _apiKey;

        public Assistant(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration.GetSection("OpenAIServiceOptions")["ApiKey"].ToString();
        }

        internal async Task<(string, JsonElement)> CreateAssistant(
            string instructions,
            string name,
            List<object> tools,
            string model,
            List<string> file_ids)
        {
            var url = "https://api.openai.com/v1/assistants";
            var jsonStr = JsonSerializer.Serialize(
                new
                {
                    model,
                    name,
                    instructions,
                    tools,
                    // file_ids
                });
            var responseRoot = await CallAPI(url, jsonStr, "POST");
            return responseRoot.ValueKind == JsonValueKind.Undefined ?
                    ("", default) :
                    (responseRoot.GetProperty("id").GetString()!, responseRoot);
        }

        //要查看有那些 Assistants 可以呼叫 ListAssistants()
        internal async Task<string> ListAssistants()
        {
            var url = "https://api.openai.com/v1/assistants?order=desc&limit=20";

            var responseRoot = await CallAPI(url, null, "GET");
            return (
                responseRoot.ValueKind == JsonValueKind.Undefined ?
                "" :
                responseRoot.GetProperty("data").ToString()!);
        }

        // 建立 Thread
        internal async Task<string> CreateThread()
        {
            var url = "https://api.openai.com/v1/threads";

            var responseRoot = await CallAPI(url, null, "POST");
            return responseRoot.ValueKind == JsonValueKind.Undefined ?
                    "" :
                    responseRoot.GetProperty("id").GetString()!;
        }

        //發送使用者 Message
        internal async Task CreateMessage(string question, string threadId)
        {
            Console.WriteLine($"發送msg給LLM");
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            var jsonStr = JsonSerializer.Serialize(new
            {
                role = "user",
                content = question
            });

            _ = await CallAPI(url, jsonStr, "POST");
            // Console.WriteLine(
            //     responseRoot.ValueKind == JsonValueKind.Undefined ?
            //     "" :
            //     responseRoot.GetString()!);
        }

        // 取回 Message
        internal async Task ListMessages(string threadId)
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
        internal async Task CreateRun(string threadId, string assistantId)
        {
            Console.WriteLine($"執行Run");
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs";
            var jsonStr = JsonSerializer.Serialize(new
            {
                assistant_id = assistantId
            });
            var responseRoot = await CallAPI(url, jsonStr, "POST");

            if (responseRoot.ValueKind != JsonValueKind.Undefined)
            {
                string runId = responseRoot.GetProperty("id").GetString()!;
                while (true)
                {
                    var status = await RetrieveRun(threadId, runId);
                    // await ListRunSteps(threadId, runId);
                    
                    while (status == "queued" || status == "in_progress")
                    {
                        await Task.Delay(2000);
                        status = await RetrieveRun(threadId, runId);
                    }
                    if (status == "requires_action")
                    {
                        //呼叫function來處理
                        break;

                    }
                    else if (status == "error" || status == "failed")
                    {
                        break;
                    }
                    else if (status == "complete")
                    {
                        await ListMessages(threadId);
                    }
                    else
                    {
                        Console.WriteLine($"未知的status:{status}");
                        break;
                    }
                }
            }
        }

        internal async Task<string> RetrieveRun(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}";
            var responseRoot = await CallAPI(url, null, "get");
            string status = responseRoot.GetProperty("status").GetString()!;
            Console.WriteLine($"status資訊:{status}");
            return status;
        }

        internal async Task ListRunSteps(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}/steps";
            var responseRoot = await CallAPI(url, null, "get");
            Console.WriteLine(
                responseRoot.ValueKind == JsonValueKind.Undefined ?
                default :
                responseRoot!);
        }

        //將 Thread 刪除掉
        internal async Task DeleteThread(string threadId)
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

        private async Task<JsonElement> CallAPI(
            string url,
            string jsonStr,
            string method)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            var response = method == "POST" ?
                await client.PostAsync(url, string.IsNullOrEmpty(jsonStr) ? null : new StringContent(jsonStr, Encoding.UTF8, "application/json")) :
                await client.GetAsync(url);
            Console.WriteLine($"FYI ~ {response.Content.ReadAsStringAsync().Result}");
            if (response.IsSuccessStatusCode)
            {
                //從HTTP響應中讀取內容並轉換為字符串
                var responseBody = await response.Content.ReadAsStringAsync();
                //將JSON字符串解析為 JsonDocument
                JsonDocument doc = JsonDocument.Parse(responseBody);
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
        internal static string GetRumor(string personName)
        {
            switch (personName)
            {
                case "周杰倫":
                    return "周杰倫的緋聞對象是昆凌";
                case "昆凌":
                    return "昆凌的緋聞對象是周杰倫";
                case "蔡依林":
                    return "蔡依林的緋聞對象是周杰倫";
                case "周渝民":
                    return "周渝民的緋聞對象是阿尼亞";
                case "川普":
                    return "川普的緋聞對象是....數不完";
                case "唐瑋祁":
                    return "他太神祕了，沒有緋聞對象";
                default:
                    return "這難倒我了，我不知道";
            }
        }
    }
}