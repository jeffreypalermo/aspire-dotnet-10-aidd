using System.Net.Http.Json;

namespace AspireTest.Web;

public class CacheApiClient(HttpClient httpClient)
{
    public async Task<CacheItemResponse?> SetCacheAsync(string key, string data, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var item = new CacheItemRequest(data, metadata);
        var response = await httpClient.PostAsJsonAsync($"/cache/{key}", item, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CacheItemResponse>(cancellationToken);
    }

    public async Task<CacheEntryResponse?> GetCacheAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/cache/{key}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CacheEntryResponse>(cancellationToken);
    }

    public async Task<List<CacheEntryResponse>> GetAllCacheKeysAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<CacheEntryResponse>>("/cache", cancellationToken) ?? [];
    }

    public async Task<bool> DeleteCacheAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/cache/{key}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

public record CacheItemRequest(string Data, string? Metadata = null);

public record CacheItemValue(string Data, string? Metadata, DateTime CreatedAt);

public record CacheItemResponse(string Key, CacheItemValue Value, string ExpiresIn);

public record CacheEntryResponse(string Key, CacheItemValue Value);
