using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteidanPrime;
using SteidanPrime.Services;

var builder = Host.CreateApplicationBuilder();

var config = builder.Configuration.GetSection("Config");
var discordSocketConfig = new DiscordSocketConfig()
{
    LogLevel = LogSeverity.Info,
    GatewayIntents = GatewayIntents.All,
    AlwaysDownloadUsers = true,
    LogGatewayIntentWarnings = false,
    HandlerTimeout = 10000
};

builder.Services.AddSteidanPrime(discordSocketConfig, s =>
    {
        s.Token = config["token"];
        s.CommandPrefix = config["prefix"];
    });

var host = builder.Build();

await host.StartAsync();

var discordBot = host.Services.GetService<IDiscordBotClient>();
await discordBot.RunAsync();


