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
        private readonly string instructions = "沒加入function功能的基礎模型測試";
        private readonly string assistantName = "基礎LLM模型";
        private readonly List<object> tools = new()
        {
            new { type = "retrieval" },
            // new { type = "code_interpreter" },
            // new {
            //     type = "function",
            //     function = new {
            //         name = "GetRumor",
            //         description = "根據傳入的人名，取得他的緋聞對象名字",
            //         parameters = new {
            //             type = "object",
            //             properties = new {
            //                 personName = new {
            //                     type = "string",
            //                     description = "取得人名"
            //                 }
            //             },
            //             required = new List<string> { "personName" }
            //         }
            //     }
            // }
        };
        private readonly string model = "gpt-3.5-turbo-1106"; //gpt-4-preview-1106
        private static string _assistantId = "asst_7Vg3qRPX3SIvrDUXWtBqlWla";
        // private readonly List<string> file_ids = new List<string> { file1Id };
        private readonly Assistant _assistant;
        public AIWeatherController(Assistant assistant)
        {
            _assistant = assistant;
        }



        // 這樣會404，why?
        // [HttpGet]
        // public async Task<string> GetAssistantAsync() => 
        //     _assistantId == "" ? await Assistant.CreateAssistant(instructions, assistantName, tools, model, null) : "";

        // 已經取過了就不用再取了
        [HttpGet]
        public async Task<string> GetAssistant()
        {
            if (_assistantId == "")
            {
                (_assistantId, var jsonStr) = await _assistant.CreateAssistant(
                    instructions,
                    assistantName,
                    tools,
                    model,
                    new List<string>());
                return @$"建立assistantId成功 - {_assistantId}
                .
                .
                .
                {jsonStr}";
            }
            else
            {
                return $"已經有assistantId了";
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAssistantsList() =>
            Ok(await _assistant.ListAssistants());

        [HttpGet]
        public ActionResult ModifyMyAIAssistant()
        {
            return Ok(_assistant.ModifyAssistant(_assistantId));
        }

        //前端要存著threadId - thread_5KE7QDTrGktZxsJfFQkW0zYi
        [HttpGet]
        public async Task<ActionResult> CreateThreadEndpoint() =>
            Ok(await _assistant.CreateThread());


        [HttpDelete]
        public async Task<ActionResult> DeleteThreadEndpoint([FromQuery] string threadId)
        {
            await _assistant.DeleteThread(threadId);
            return Ok();
        }

        [HttpPost]
        public async Task<string> ChatEndpoint(
            [FromBody] ChatRequest chatRequest)
        {
            //傳入參數若寫成[FromBody]string msg,string threadId會錯....
            await _assistant.CreateMessage(chatRequest.Msg!, chatRequest.ThreadId!);
            var rspContent = await _assistant.CreateRun(chatRequest.ThreadId!, _assistantId);
            return rspContent;
        }
    }
}