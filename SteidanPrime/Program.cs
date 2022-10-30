using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using SteidanPrime.Services;
using SteidanPrime.Services.Markov;
using SteidanPrime.Services.Saveboard;
using SteidanPrime.Services.Sokoban;

namespace SteidanPrime
{
    class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private InteractionService _commands;
        private bool _stopBot = false;
        public LoggingService LoggingService;
        public static Settings Settings { get; set; }

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            Settings = new Settings();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                GatewayIntents = Discord.GatewayIntents.All,
                AlwaysDownloadUsers = true,
                LogGatewayIntentWarnings = false
            });

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<MarkovService>()
                .AddSingleton<SaveboardService>()
                .AddSingleton<SokobanService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .BuildServiceProvider();

            _client.Ready += ClientReady;

            if (!Directory.Exists("Resources"))
                Directory.CreateDirectory("Resources");

            if (!Directory.Exists("Resources/Dictionaries"))
                Directory.CreateDirectory("Resources/Dictionaries");

            if (File.Exists("Resources/config.json")) return;

            Console.WriteLine("Config file not found. Initializing bot settings.");
            switch (Settings.ApplicationRunningMethod)
            {
                case ApplicationRunningMethod.CONSOLE:
                    Console.WriteLine("Please input your bot token: ");
                    Settings.Token = Console.ReadLine();

                    Console.WriteLine("Please choose your bot prefix: ");
                    Settings.CommandPrefix = Console.ReadLine();
                    break;

                case ApplicationRunningMethod.SERVICE:
                    Console.WriteLine("Application is running as a service, please change config.");
                    break;
            }

            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            System.IO.File.WriteAllText("Resources/config.json", json);
        }

        private async Task ClientReady()
        {
            _services.GetRequiredService<SaveboardService>().DeserializeSaveboard();
            _services.GetRequiredService<MarkovService>().DeserializeDict();
            _services.GetRequiredService<SokobanService>();
            await _client.SetGameAsync(
                $"with {_services.GetRequiredService<MarkovService>().GetTotalWords()} words for Markov chains | !help");

            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            await _commands.RegisterCommandsGloballyAsync(true);
            //await _commands.RegisterCommandsToGuildAsync(149871097452429312, true);
            //await _commands.RegisterCommandsToGuildAsync(102375147515699200, true);
            //await _commands.RegisterCommandsToGuildAsync(1035712010836516925, true);
        }

        private async void AutoSave(object source, ElapsedEventArgs e)
        {
            _services.GetRequiredService<MarkovService>().SerializeDict();
            _services.GetRequiredService<SaveboardService>().SerializeSaveboard();
            await _client.SetGameAsync(
                $"with {_services.GetRequiredService<MarkovService>().GetTotalWords()} words for Markov chains | !help");
            Console.WriteLine("Dictionaries auto-saved.");
        }

        public async Task MainAsync()
        {
            _commands = _services.GetRequiredService<InteractionService>();
            Settings = JsonConvert.DeserializeObject<Settings>(await File.ReadAllTextAsync("Resources/config.json"));
            LoggingService = new LoggingService(_client, _commands);

            try
            {
                await _client.LoginAsync(TokenType.Bot, Settings.Token);
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // Initialize auto-save timer
            Timer timer = new Timer
            {
                Interval = 10 * 60 * 1000,
                AutoReset = true    
            };

            timer.Elapsed += AutoSave;
            timer.Enabled = true;

            while (!_stopBot)
            {
                if (Settings.ApplicationRunningMethod != ApplicationRunningMethod.SERVICE)
                    await ProcessConsoleInput();
            }
        }

        public async Task ProcessConsoleInput()
        {
            var consoleInput = (await GetInputAsync()).ToLower();

            switch (consoleInput)
            {
                case "stop":
                    _services.GetRequiredService<MarkovService>().SerializeDict();
                    _services.GetRequiredService<SaveboardService>().SerializeSaveboard();
                    _stopBot = true;
                    break;

                case "hello there":
                    Console.WriteLine("General Kenobi");
                    break;

                default:
                    Console.WriteLine("Command not recognized.");
                    break;
            }
        }

        private static async Task<string> GetInputAsync()
        {
            return await Task.Run(Console.ReadLine);
        }      
    }
}
