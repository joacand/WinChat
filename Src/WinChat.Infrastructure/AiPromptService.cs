using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WinChat.Infrastructure.Repository;

namespace WinChat.Infrastructure;

public class AiPromptService
{
    private static readonly HttpClient _httpClient = new();
    private string ApiKey = string.Empty;
    private readonly AppDbContext _appDbContext;

    public AiPromptService(AppDbContext appDbContext)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _appDbContext = appDbContext;
    }

    public async Task<string> TestApi()
    {
        var apiKey = await GetApiKey();
        if (apiKey == null)
        {
            return "No API key specified";
        }

        var address = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={ApiKey}";

        var request = new HttpRequestMessage(HttpMethod.Post, address)
        {
            Content = new StringContent(JsonSerializer.Serialize(
            new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new []
                        {
                            new
                            {
                                text = "Explain how AI works"
                            }
                        }
                    }
                }
            }), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task AddApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return;
        }
        _appDbContext.ApplicationData.Add(new ApplicationData
        {
            Id = "ApiKey",
            Name = "ApiKey",
            Value = apiKey
        });
        await _appDbContext.SaveChangesAsync();
    }

    private async Task<string?> GetApiKey()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            var dbResponse = await _appDbContext.ApplicationData.FindAsync("ApiKey");
            if (string.IsNullOrWhiteSpace(dbResponse?.Value))
            {
                return null;
            }
            ApiKey = dbResponse.Value;
        }
        return ApiKey;
    }
}
