using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PalmHilsSemanticKernelBot.BusinessLogic.Bot;
using PalmHilsSemanticKernelBot.BusinessLogic.SemanticKernelPlugins;
using PalmHilsSemanticKernelBot.Helpers;
using System.Data;
using Serilog;
using Serilog.Events;
using Serilog.Settings.Configuration;
using PalmHilsSemanticKernelBot.BusinessLogic.Bot.ConversationMemory;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();


// Register IDbConnection for Dapper
builder.Services.AddSingleton<IDbConnection>(provider =>
    new SqliteConnection(builder.Configuration.GetConnectionString("SQLiteConnection")));

builder.Services.AddSingleton<IDataBaseSchemaReaderService, DataBaseSchemaReaderService>();

//Bot Framework
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IStorage, ResilientMemoryStorage>();
builder.Services.AddSingleton<ConversationState>(serviceProvider =>
{
    var storage = serviceProvider.GetService<IStorage>();
    return new ConversationState(storage);
});
builder.Services.AddSingleton<UserState>();

// Register plugins as services
builder.Services.AddSingleton<TextToSqlitePlugin>();

builder.Services.AddSingleton<Kernel>(serviceProvider =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    // Add Azure OpenAI chat completion service
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: builder.Configuration.GetValue<string>("AzureOpenAI:DeploymentName"),
        endpoint: builder.Configuration.GetValue<string>("AzureOpenAI:Endpoint"),
        apiKey: builder.Configuration.GetValue<string>("AzureOpenAI:ApiKey")
        );
  
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    kernelBuilder.Services.AddSingleton(loggerFactory);
    kernelBuilder.Services.AddSingleton(serviceProvider.GetRequiredService<IDataBaseSchemaReaderService>());

    var kernel = kernelBuilder.Build();

    // Add your plugins here if you have them

    var textToSqlitePlugin = serviceProvider.GetRequiredService<TextToSqlitePlugin>();
    kernel.Plugins.AddFromObject(textToSqlitePlugin, "TextToSqlitePlugin");

    kernel.Plugins.AddFromType<BookingPlugin>();
    /*kernel.Plugins.AddFromType<CustomerDetailsPlugin>();*/
    kernel.Plugins.AddFromType<DocumentLegalPlugin>();

    return kernel;
});

builder.Services.AddTransient<IBot, SemanticKernelWithBot>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
