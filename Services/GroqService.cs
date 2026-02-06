using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace MeuProjetoIA.Services;

public class GroqService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GroqService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["Groq:ApiKey"]
        ?? throw new InvalidOperationException("Groq:ApiKey não configurado em appsettings.");

        _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 512
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Groq API erro {response.StatusCode}: {responseBody}");
        }

        // Log raw (mantenha por enquanto, depois pode remover)
        Console.WriteLine($"Groq Raw Response for prompt '{prompt}': {responseBody}");

        // Parsing correto - use classes com nomes que batem exatamente com o JSON
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };  // tolera case
        var result = JsonSerializer.Deserialize<GroqCompletionResponse>(responseBody, options);

        var contentFromIA = result?.Choices?
            .FirstOrDefault()?
            .Message?
            .Content;

        if (string.IsNullOrWhiteSpace(contentFromIA))
        {
            return $"Falha no parsing. Conteúdo não encontrado. Raw: {responseBody}";
        }

        return contentFromIA.Trim();
    }

    // Classes de resposta EXATAS para o formato Groq/OpenAI
    private class GroqCompletionResponse
    {
        public List<GroqChoice>? Choices { get; set; }
    }

    private class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }

    private class GroqMessage
    {
        public string? Content { get; set; }
    }
}