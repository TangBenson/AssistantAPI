using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAIAssistant
{
    public class Assistant
    {
        private readonly IConfiguration _configuration;
        //constructor
        public Assistant(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}