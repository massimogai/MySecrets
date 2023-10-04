using Microsoft.AspNetCore.Components.Authorization;
using MySecrets.Services;
using ScuolaRegionale.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<SecretsDbContext>();
builder.Services.AddSingleton<DatabaseInitializatorService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddScoped<WebsiteAuthenticator>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<WebsiteAuthenticator>());
builder.Services.AddSingleton<RsaService>();
builder.Services.AddSingleton<AesService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
 
SecretsDbContext? context = (SecretsDbContext) app.Services.GetService(typeof(SecretsDbContext))!;
context.Initialize();

DatabaseInitializatorService? databaseInitializatorService = (DatabaseInitializatorService) app.Services.GetService(typeof(DatabaseInitializatorService))!;
databaseInitializatorService.Initialize();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers().AllowAnonymous();
app.Run();