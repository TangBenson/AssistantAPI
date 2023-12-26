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
        //constructor
        public AIWeatherController(IConfiguration configuration)
        {
            _configuration = configuration;
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