using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OpenAIAssistant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        private readonly object tools = new List<object> { new { type = "retrieval" } };
        private readonly string model = "gpt-3.5-turbo-1106";
        // private readonly List<string> file_ids = new List<string> { file1Id };
        public AIWeatherController()
        {
        }

        [HttpGet]
        public async Task CreateThreadEndpoint()
        {
            
        }

        [HttpPost]
        public async Task ChatEndpoint()
        {
            
        }
    }
}