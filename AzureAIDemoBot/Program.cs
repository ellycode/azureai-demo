var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpClient()
    .AddControllers()
    .AddNewtonsoftJson();

// Create services to handle Conversation and User state
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<ConversationState>();

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddSingleton<MainDialog>();
builder.Services.AddTransient<IBot, DemoBot<MainDialog>>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();