using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaDriver.NET.Infrastructure;
using OllamaDriver.NET.Settings;
using Sample.SemanticKernelApp.Planners;
using Sample.SemanticKernelApp.Plugins;
using Sample.SemanticKernelApp.Services;

var configs = new OllamaConnectionConfigs
{
    ModelName = "mistral:latest",
    BaseUrl = "http://localhost:11434/"
};

var logFactory = LoggerFactory.Create(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace));
var builder = Kernel.CreateBuilder();

builder.Services.AddHttpClient(nameof(OllamaAccessor))
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(configs.BaseUrl);
    });
builder.Services.AddSingleton<IAiAccessor, OllamaAccessor>();
builder.Services.AddSingleton<IChatCompletionService, OllamaChatCompletionService>();
builder.Services.AddSingleton(configs);
//builder.Services.AddSingleton(logFactory);
builder.Plugins.AddFromType<AuthorEmailPlanner>();
builder.Plugins.AddFromType<EmailPlugin>();

var kernel = builder.Build();

// Retrieve the chat completion service from the kernel
var chat = kernel.GetRequiredService<IChatCompletionService>();

// Create the chat history
ChatHistory history = [];

history.AddSystemMessage("""
    You are a friendly assistant who likes to follow the rules. 
    You will complete required steps and request approval before taking any consequential actions. 
    If the user doesn't provide enough information for you to complete a task, you will keep asking questions until you have enough information to complete the task.
""");

// Start the conversation
while (true)
{
    // Get user input
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("User > ");
    history.AddUserMessage(Console.ReadLine()!);

    // Get the chat completions
    var promptExecutionSettings = new PromptExecutionSettings
    {
        ModelId = configs.ModelName,
    };
    /*var promptExecutionSettings = new OpenAIPromptExecutionSettings
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };*/
    var result = chat.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: promptExecutionSettings,
        kernel: kernel);

    // Stream the results
    var fullMessage = string.Empty;

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Assistant > ");
    await foreach (var content in result)
    {
        Console.Write(content.Content);
        fullMessage += content.Content;
    }

    Console.WriteLine();

    // Add the message from the agent to the chat history
    history.AddAssistantMessage(fullMessage);
}
