using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OpenAIAssistant.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AIWeatherController : ControllerBase
    {
        // private readonly string file1Id = "file-9xpwQr4rni98mUcrQtlkDkvc";
        private readonly string instructions = @"這是一個透過api取得天氣資料並回答使用者的GPT，使用者會問各縣市的天氣，
                                                藉由呼叫GetCurrentWeather action取得縣市天氣預報，並同時呼叫GetSpecialWeather取得縣市天氣警報。
                                                若使用者問的縣市不在台灣，請回答'您所問的地區不在偉大的台灣國喔'，不需要多餘廢話。
                                                若使用者輸入的縣市是簡體字，請自動轉為繁體字。
                                                若使用者沒有輸入縣市，例如只輸入臺北，請自動帶入縣市變為臺北市。
                                                非天氣問題一律回答'老子只他媽懂天氣'";
        private readonly string assistantName = "天氣之子";
        //code_interpreter, retrieval, or function
        private readonly List<object> tools = new() {
            new { type = "retrieval" },
            new { type = "code_interpreter" },
            new {
                type = "function",
                function = new {
                    name= "get_city_for_date",
                    description= "根据传入的日期获取对应的城市.",
                    parameters = new {
                        type="object",
                        properties=new {
                            date_str=new {
                                type= "string",
                                description= "用于获取对应城市的日期，格式为YYYY-MM-DD."
                            }
                        }
                    }
                }
            }
        };
        private readonly string model = "gpt-3.5-turbo-1106";
        // private readonly List<string> file_ids = new List<string> { file1Id };
        // private readonly Assistant _assistant;
        private static string _assistantId = "asst_yRXtzWuhFOPv1xqZrcHj5rrV";
        public AIWeatherController()
        {
            // _assistant = assistant;
        }

        // 這樣會404，why?
        // [HttpGet]
        // public async Task<string> GetAssistantAsync() => 
        //     _assistantId == "" ? await Assistant.CreateAssistant(instructions, assistantName, tools, model, null) : "";

        // 已經取過了就不用再取了
        // [HttpGet]
        // public async Task GetAssistant()
        // {
        //     if (_assistantId == "")
        //     {
        //         _assistantId = await Assistant.CreateAssistant(instructions, assistantName, tools, model, new List<string>());
        //         Console.WriteLine($"建立assistantId成功");
        //     }
        //     else
        //     {
        //         Console.WriteLine($"已經有assistantId了");
        //     }
        // }

        [HttpGet]
        public async Task<ActionResult> GetAssistantsList() =>
            Ok(await Assistant.ListAssistants());

        //前端要存著threadId
        [HttpGet]
        public async Task<ActionResult> CreateThreadEndpoint() =>
            Ok(await Assistant.CreateThread());

        [HttpPost]
        public async Task<(string rmsg, string img)> ChatEndpoint(
            string msg,
            string threadId)
        {
            await Assistant.CreateMessage(msg, threadId);
            await Assistant.CreateRun(threadId, _assistantId);
            await Assistant.ListMessages(threadId);
            return ("", "");
        }
    }
}