using AppServer.Components;
using AppServer.Utils;
using AppServer.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

// Configure HttpClient to share cookies with Blazor Server
builder.Services.AddHttpClient();

// Registrer services
builder.Services.AddScoped<AuthUiService>();
builder.Services.AddSingleton<UserGrpcClient>();
builder.Services.AddSingleton<QuizGrpcClient>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

// SIMPLE AUTH - Clean approach (NO Circuit ID tracking needed!)
builder.Services.AddSingleton<SimpleAuthService>();
builder.Services.AddScoped<SimpleCircuitHandler>();
builder.Services.AddScoped<CircuitHandler>(sp => sp.GetRequiredService<SimpleCircuitHandler>());

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AppServer.Session";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Allow cookie to be sent with API requests
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseSession(); // IMPORTANT: Session must come BEFORE MapControllers

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
