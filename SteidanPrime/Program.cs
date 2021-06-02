using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SteidanPrime.MarkovChain;
using SteidanPrime.Sokoban;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace SteidanPrime
{
    class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private bool _stopBot = false;
        public CommandHandler CommandHandler;
        public LoggingService LoggingService;
        public static Settings Settings { get; set; }
        public static Markov Markov { get; set; }
        public static Sokoban.Game Sokoban { get; set; }

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

            _client.Ready += ClientReady;
            _client.JoinedGuild += JoinedGuild;

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
            Markov = new Markov(_client);
            Sokoban = new Sokoban.Game();
        }

        private static void AutoSave(object source, ElapsedEventArgs e)
        {
            Markov.SerializeDict();
            Console.WriteLine("Dictionaries auto-saved.");
        }

        private async Task JoinedGuild(SocketGuild Guild)
        {
            if (!Markov.MarkovDict.ContainsKey(Guild.Id))
                Markov.MarkovDict[Guild.Id] = new Dictionary<string, List<string>>();
        }

        public async Task MainAsync()
        {
            Settings = JsonConvert.DeserializeObject<Settings>(await File.ReadAllTextAsync("Resources/config.json"));

            CommandHandler = new CommandHandler(_client, _commands, Settings.CommandPrefix);
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
                    Markov.SerializeDict();
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
