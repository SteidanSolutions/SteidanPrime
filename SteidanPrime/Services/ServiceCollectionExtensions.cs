using System;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteidanPrime.Models;
using SteidanPrime.Repositories.Saveboard;
using SteidanPrime.Services.Admin;
using SteidanPrime.Services.Common;
using SteidanPrime.Services.Gambling;
using SteidanPrime.Services.Markov;
using SteidanPrime.Services.Saveboard;
using SteidanPrime.Services.Sokoban;

namespace SteidanPrime.Services;

public static class ServiceCollectionExtensions
{
    public static void AddSteidanPrime(this IServiceCollection services,
        DiscordSocketConfig socketSettings,
        Action<Settings> settings)
    {
        services.Configure<Settings>(settings.Invoke);
        
        services
            .AddSingleton(socketSettings)
            .AddSingleton<ISaveboardRepository, SaveboardRepository>()
            .AddSingleton<IMarkovService, MarkovService>()
            .AddSingleton<ISaveboardService, SaveboardService>()
            .AddSingleton<ISokobanService, SokobanService>()
            .AddSingleton<IGamblingService, GamblingService>()
            .AddSingleton<IAdminService, AdminService>()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<IInteractionHandler, InteractionHandler>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<IDiscordBotClient, DiscordBotClient>()
            .AddSystemd()
            .BuildServiceProvider();
    }
}