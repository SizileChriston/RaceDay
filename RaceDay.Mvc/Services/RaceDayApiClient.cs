using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

public sealed class RaceDayApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    private void AddToken()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        var token = _accessor.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        AddToken();
        using var response = await _http.GetAsync(path);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<(bool Success, T? Data, string? Error, int Status)> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        AddToken();
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonOptions);
        using var response = await _http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        T? data = default;
        if (!string.IsNullOrWhiteSpace(text))
        {
            try { data = JsonSerializer.Deserialize<T>(text, JsonOptions); }
            catch (JsonException) { }
        }
        string? error = null;
        if (!response.IsSuccessStatusCode)
        {
            try { error = JsonDocument.Parse(text).RootElement.GetProperty("message").GetString(); }
            catch { error = $"The API returned status {(int)response.StatusCode}."; }
        }
        return (response.IsSuccessStatusCode, data, error, (int)response.StatusCode);
    }

    public async Task<(bool Success, string? Error, int Status)> UploadAsync(string path, IFormFile file)
    {
        AddToken();
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(content, "file", Path.GetFileName(file.FileName));
        using var response = await _http.PostAsync(path, form);
        var text = await response.Content.ReadAsStringAsync();
        string? error = null;
        if (!response.IsSuccessStatusCode)
        {
            try { error = JsonDocument.Parse(text).RootElement.GetProperty("message").GetString(); }
            catch { error = $"The API returned status {(int)response.StatusCode}."; }
        }
        return (response.IsSuccessStatusCode, error, (int)response.StatusCode);
    }
}
