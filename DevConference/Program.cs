using DevConference.Client.Pages;
using DevConference.Components;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DevConferenceAPI.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextFactory<AplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AplicationDbContext") ?? throw new InvalidOperationException("Connection string 'AplicationDbContext' not found.")));

builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();


// OR with base address:
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7241/");
});

//builder.Services.AddHttpClient("API", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7241/");
//});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(DevConference.Client._Imports).Assembly);

app.Run();
