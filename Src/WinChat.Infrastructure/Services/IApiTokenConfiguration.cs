namespace WinChat.Infrastructure.Services;

public interface IApiTokenConfiguration
{
    Task SetApiToken(string apiToken);
}
