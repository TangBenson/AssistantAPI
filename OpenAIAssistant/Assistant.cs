using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace OpenAIAssistant
{
    public class Assistant
    {
        private readonly IConfiguration _configuration;
        private readonly string? _apiKey;
        private delegate string MyDelegate(string input);

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
            var responseRoot = await CallAPI("CreateAssistant", url, jsonStr, "POST");
            return responseRoot.ValueKind == JsonValueKind.Undefined ?
                    ("", default) :
                    (responseRoot.GetProperty("id").GetString()!, responseRoot);
        }

        //要查看有那些 Assistants 可以呼叫 ListAssistants()
        internal async Task<string> ListAssistants()
        {
            var url = "https://api.openai.com/v1/assistants?order=desc&limit=20";

            var responseRoot = await CallAPI("ListAssistants", url, null, "GET");
            return (
                responseRoot.ValueKind == JsonValueKind.Undefined ?
                "" :
                responseRoot.GetString()!);
        }

        // 建立 Thread
        internal async Task<string> CreateThread()
        {
            var url = "https://api.openai.com/v1/threads";

            var responseRoot = await CallAPI("CreateThread", url, null, "POST");
            return responseRoot.ValueKind == JsonValueKind.Undefined ?
                    "" :
                    responseRoot.GetProperty("id").GetString()!;
        }

        //發送使用者 Message
        internal async Task CreateMessage(string question, string threadId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            var jsonStr = JsonSerializer.Serialize(new
            {
                role = "user",
                content = question
            });

            _ = await CallAPI("CreateMessage", url, jsonStr, "POST");
            // Console.WriteLine(
            //     responseRoot.ValueKind == JsonValueKind.Undefined ?
            //     "" :
            //     responseRoot.GetString()!);
        }

        // 取回 Message
        internal async Task<string> ListMessages(string threadId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages?order=desc&limit=2";

            var responseRoot = await CallAPI("ListMessages", url, null, "GET");

            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine(responseRoot.GetProperty("data")[0]
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetProperty("value").ToString()!);

            return (
                // responseRoot.ValueKind == JsonValueKind.Undefined ?
                // "" :
                responseRoot.GetProperty("data")[0]
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetProperty("value").ToString()!);
        }

        //發送 Run 來讓 Assistant 處理使用者詢問的問題
        internal async Task<string> CreateRun(string threadId, string assistantId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs";
            var jsonStr = JsonSerializer.Serialize(new
            {
                assistant_id = assistantId
            });
            var responseRoot = await CallAPI("CreateRun", url, jsonStr, "POST");
            var rspContent = "";

            if (responseRoot.ValueKind != JsonValueKind.Undefined)
            {
                string runId = responseRoot.GetProperty("id").GetString()!;
                responseRoot = await RetrieveRun(threadId, runId);
                var status = responseRoot.GetProperty("status").GetString()!;
                while (true)
                {
                    while (status == "queued" || status == "in_progress")
                    {
                        await Task.Delay(2000);
                        responseRoot = await RetrieveRun(threadId, runId);
                        status = responseRoot.GetProperty("status").GetString()!;
                    }
                    if (status == "requires_action")
                    {
                        //呼叫function來處理
                        // var funcName = "GetRumor";
                        var funcName = responseRoot.GetProperty("required_action")
                        .GetProperty("submit_tool_outputs")
                        .GetProperty("tool_calls")[0]
                        .GetProperty("function")
                        .GetProperty("name")
                        .GetString()!;

                        var personName = responseRoot.GetProperty("required_action")
                        .GetProperty("submit_tool_outputs")
                        .GetProperty("tool_calls")[0]
                        .GetProperty("function")
                        .GetProperty("arguments")
                        .GetString()!;

                        var id = responseRoot.GetProperty("required_action")
                        .GetProperty("submit_tool_outputs")
                        .GetProperty("tool_calls")[0]
                        .GetProperty("id")
                        .GetString()!;

                        Answer personInfo = JsonSerializer.Deserialize<Answer>(personName)!;

                        //用反射來呼叫function
                        Type type = typeof(Assistant); // 請替換成包含 GetAbc 方法的類型
                        MethodInfo methodInfo = type.GetMethod(funcName)!;
                        object[] parameters = new object[] { personInfo.personName! };
                        object result = methodInfo.Invoke(null, parameters)!;
                        string funcOutput = (string)result; // 將結果轉換為 string
                        Console.WriteLine($"func回傳的答案:{funcOutput}");

                        //將結果回傳給OpenAI
                        var jsonStr2 = JsonSerializer.Serialize(new
                        {
                            tool_outputs = new List<object> {
                                new {
                                    tool_call_id = id,
                                    output = funcOutput
                                }
                            }
                        });
                        responseRoot = await CallAPI(
                            "submit_tool_outputs",
                            $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}/submit_tool_outputs",
                            jsonStr2,
                            "POST");
                        status = responseRoot.GetProperty("status").GetString()!;

                        // rspContent = await ListMessages(threadId);
                        // break;

                    }
                    else if (status == "error" || status == "failed")
                    {
                        rspContent = $"錯錯錯:{status}";
                        break;
                    }
                    else if (status == "completed")
                    {
                        rspContent = await ListMessages(threadId);
                        break;
                    }
                    else
                    {
                        rspContent = $"未知的status:{status}";
                        break;
                    }
                }
                return rspContent;
            }
            else
            {
                return "沒有取得run結果";
            }
        }

        internal async Task<JsonElement> RetrieveRun(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}";
            var responseRoot = await CallAPI("RetrieveRun", url, null, "get");
            string status = responseRoot.GetProperty("status").GetString()!;
            Console.WriteLine($"status資訊:{status}");
            return responseRoot;
        }

        internal async Task ListRunSteps(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}/steps";
            var responseRoot = await CallAPI("ListRunSteps", url, null, "get");
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
            string methodName,
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
            Console.WriteLine($"FYI:{methodName} ~ {response.Content.ReadAsStringAsync().Result}");
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
        public static string GetRumor(string personName)
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
                case "吳孟其":
                    return "他已婚了，你別想了";
                case "電鋸男":
                    return "他每天都在鉅東西，沒交對象";
                case "飛天大俠":
                    return "他已經飛到火星了，找不到對象";
                default:
                    return "這難倒我了，我不知道";
            }
        }
    }
}