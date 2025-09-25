using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PalmHilsSemanticKernelBot.BusinessLogic.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<ConversationState>();
builder.Services.AddSingleton<UserState>(); 

builder.Services.AddSingleton<Kernel>(serviceProvider =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    // Add Azure OpenAI chat completion service
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o-mini",
        endpoint: "https://angieasset.openai.azure.com/",
        apiKey: "6YYqlKbFEB9KWusOP53AbBA2jQ8oGWK6l004LOsuRznVBbal3YzoJQQJ99BIACYeBjFXJ3w3AAABACOGIuAI");

    var kernel = kernelBuilder.Build();

    // Add your plugins here if you have them
    /*kernel.Plugins.AddFromType<WeatherPlugin>();
    kernel.Plugins.AddFromType<MathPlugin>();*/

    return kernel;
});

// Register IChatCompletionService
builder.Services.AddSingleton<IChatCompletionService>(serviceProvider =>
{
    var kernel = serviceProvider.GetRequiredService<Kernel>();
    return kernel.GetRequiredService<IChatCompletionService>();
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
