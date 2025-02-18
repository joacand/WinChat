using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WinChat.Infrastructure.Repository;
using WinChat.Infrastructure.Services.Contracts;

namespace WinChat.Infrastructure.Services.Gemini;

internal sealed class GeminiService(IServiceScopeFactory scopeFactory) : IGenerateTextService
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://generativelanguage.googleapis.com/"),
        DefaultRequestHeaders =
        {
            Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
        }
    };

    private string ApiKey = string.Empty;
    private const string Model = "gemini-2.0-flash";
    private static string GenerateContentEndpoint(string apiKey) => $"v1beta/models/{Model}:generateContent?key={apiKey}";

    public async Task<TextGenerationResponse> GenerateText(TextGenerationRequest textGenerationRequest)
    {
        var address = GenerateContentEndpoint(
            await GetApiKey() ?? throw new Exception("No API key specified"));

        var request = new HttpRequestMessage(HttpMethod.Post, address)
        {
            Content = new StringContent(JsonSerializer.Serialize(
                textGenerationRequest, Constants.DefaultSerializerOptions), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var textGenerationResponse = await Deserialize<TextGenerationResponse>(response);

        return textGenerationResponse ?? throw new Exception("Received null deserialized response");
    }

    public async Task SetApiToken(string apiToken)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            return;
        }

        ApiKey = apiToken;

        using var scope = scopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!appDbContext.ApplicationData.Any(x => x.SettingKey == Constants.ApplicationDataKeys.ApiKey))
        {
            appDbContext.ApplicationData.Add(new ApplicationData
            {
                SettingKey = Constants.ApplicationDataKeys.ApiKey,
                SettingValue = apiToken
            });
            await appDbContext.SaveChangesAsync();
            return;
        }

        var dbResponse = await appDbContext.ApplicationData.FindAsync(Constants.ApplicationDataKeys.ApiKey);
        if (dbResponse != null)
        {
            dbResponse.SettingValue = apiToken;
            await appDbContext.SaveChangesAsync();
        }
    }

    private static async Task<T> Deserialize<T>(HttpResponseMessage response)
    {
        var stringContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(stringContent, Constants.DefaultSerializerOptions);
        return result ?? throw new Exception("Null result after deserialization");
    }

    private async Task<string?> GetApiKey()
    {
        if (!string.IsNullOrWhiteSpace(ApiKey)) { return ApiKey; }

        using var scope = scopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbResponse = await appDbContext.ApplicationData.FindAsync(Constants.ApplicationDataKeys.ApiKey);
        if (string.IsNullOrWhiteSpace(dbResponse?.SettingValue))
        {
            return null;
        }
        ApiKey = dbResponse.SettingValue;

        return ApiKey;
    }
}
