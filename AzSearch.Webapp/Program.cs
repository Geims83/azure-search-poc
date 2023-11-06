using AzSearch.App.Data;
using AzSearch.App.Services;
using Microsoft.Fast.Components.FluentUI;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();

builder.Services.AddFluentUIComponents();

var sqlConnection = builder.Configuration.GetConnectionString("SqlDb") ?? throw new InvalidOperationException("Connection string 'SqlDb' not found.");
var azureStorage = builder.Configuration.GetConnectionString("AzureStorage") ?? throw new InvalidOperationException("Connection string 'AzureStorage' not found.");
var searchEndpoint = builder.Configuration.GetConnectionString("SearchEndpoint") ?? throw new InvalidOperationException("Connection string 'SearchEndpoint' not found.");
var searchKey = builder.Configuration.GetConnectionString("SearchKey") ?? throw new InvalidOperationException("Connection string 'SearchKey' not found.");

builder.Services.AddTransient<BlobStorageService>((az) =>
{
    return new BlobStorageService(azureStorage);
});

builder.Services.AddTransient<IDbFactory, DbFactory>((db) =>
{
    return new DbFactory(sqlConnection);
});

builder.Services.AddTransient<DocumentoRepo>();

builder.Services.AddTransient<SearchService>((search) =>
{
    return new SearchService(searchEndpoint, searchKey);
});

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
