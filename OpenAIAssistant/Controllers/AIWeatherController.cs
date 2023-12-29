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
        private readonly IConfiguration _configuration;
        private static string _apiKey = "xxxxxxxxx";
        //constructor
        public AIWeatherController(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["OpenAIServiceOptions:ApiKey"];
        }

        [HttpGet]
        public async Task CreateThreadEndpoint(string city)
        {
            
        }

        [HttpPost]
        public async Task ChatEndpoint(string city)
        {
            
        }
    }
}