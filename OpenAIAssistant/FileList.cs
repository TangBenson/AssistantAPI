using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAIAssistant
{
    public static class FileList
    {
        static readonly string jsonContentType = "application/json";
        static readonly string leaveDoc = @"C:\OpenAI\docs\請假規則.docx";
        private static IConfiguration? _configuration;
        private static string apiKey = "xxxxxxxxx";

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            apiKey = _configuration["OpenAIServiceOptions:ApiKey"];
        }

        public static async Task UploadFile(string filePath)
        {
            var url = "https://api.openai.com/v1/files";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            using var content = new MultipartFormDataContent();
            using var fileStream = new FileStream(filePath, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
            var purpose = new StringContent("assistants");
            content.Add(purpose, "purpose");
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"錯誤：{response.StatusCode} - {response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

        public static async Task ListUploadFiles()
        {
            var url = "https://api.openai.com/v1/files";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"錯誤：{response.StatusCode} - {response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }
    }
}