using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;   // <-- for NavigationManager
using AppServer.Models;

namespace AppServer.Services;

public class AuthUiService
{
    private readonly HttpClient _http;

    public AuthUiService(IHttpClientFactory factory, NavigationManager nav)
    {
        _http = factory.CreateClient();
        // Sæt base address til den kørende apps base-URL (virker i dev uden at hardcode porte)
        _http.BaseAddress = new Uri(nav.BaseUri);
    }

    public async Task<(bool ok, string? code)> RegisterAsync(RegisterRequest req)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/register", req);
        if (resp.IsSuccessStatusCode) return (true, null);
        var err = await TryReadError(resp);
        return (false, err);
    }

    public async Task<(bool ok, string? code)> LoginAsync(LoginRequest req)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/login", req);
        if (resp.IsSuccessStatusCode) return (true, null);
        var err = await TryReadError(resp);
        return (false, err);
    }

    private static async Task<string?> TryReadError(HttpResponseMessage resp)
    {
        try
        {
            var anon = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (anon != null && anon.TryGetValue("code", out var code)) return code;
        }
        catch { }
        return null;
    }
}