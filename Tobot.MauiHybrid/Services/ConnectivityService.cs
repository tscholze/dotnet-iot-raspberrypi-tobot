namespace Tobot.MauiHybrid.Services;

public interface IConnectivityService
{
    Task<bool> IsServerReachableAsync(string url, CancellationToken cancellationToken = default);
}

public class ConnectivityService : IConnectivityService
{
    private readonly HttpClient _httpClient;

    public ConnectivityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsServerReachableAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
