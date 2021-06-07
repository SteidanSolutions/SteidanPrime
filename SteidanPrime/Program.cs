using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Discord.Addons.Interactive;
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
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private bool _stopBot = false;
        public CommandHandler CommandHandler;
        public LoggingService LoggingService;

        public static Settings Settings { get; set; }
        //public static MarkovService MarkovService { get; set; }

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            Settings = new Settings();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false              
            });

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<InteractiveService>()
                .AddSingleton(_commands)
                .AddSingleton<CommandHandler>()
                .AddSingleton<MarkovService>()
                .AddSingleton<SaveboardService>()
                .AddSingleton<SokobanService>()
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
            await _client.SetGameAsync("Hello there");
            _services.GetRequiredService<SaveboardService>().DeserializeSaveboard();
            _services.GetRequiredService<MarkovService>().DeserializeDict();
            _services.GetRequiredService<SokobanService>();
        }

        private void AutoSave(object source, ElapsedEventArgs e)
        {
            _services.GetRequiredService<MarkovService>().SerializeDict();
            _services.GetRequiredService<SaveboardService>().SerializeSaveboard();
            Console.WriteLine("Dictionaries auto-saved.");
        }

        public async Task MainAsync()
        {
            Settings = JsonConvert.DeserializeObject<Settings>(await File.ReadAllTextAsync("Resources/config.json"));

            CommandHandler = new CommandHandler(_client, _commands, _services, Settings.CommandPrefix);
            await CommandHandler.InstallCommandsAsync();

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
