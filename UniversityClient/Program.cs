using System.Net.Http.Headers;
using UniversityClient.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Адрес API appsettings.json-нан алынады — кодта қатаң жазылмаған
var baseUrl = builder.Configuration["UniversityApi:BaseUrl"]
    ?? throw new InvalidOperationException("UniversityApi:BaseUrl is not configured");
var timeoutSeconds = builder.Configuration.GetValue("UniversityApi:TimeoutSeconds", 10);

// IHttpClientFactory: аталған клиент
builder.Services.AddHttpClient(UniversityApiClient.ClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<IUniversityApiClient, UniversityApiClient>();

var app = builder.Build();

app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Courses}/{action=Index}/{id?}");

app.Run();
