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
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private bool stopBot = false;
        public CommandHandler commandHandler;
        public LoggingService loggingService;
        public static Settings settings { get; set; }
        public static Markov markov { get; set; }
        public static Sokoban.Game Sokoban { get; set; }

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            settings = new Settings();

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
            });

            commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false              
            });

            client.Ready += ClientReady;
            client.JoinedGuild += JoinedGuild;

            if (!File.Exists("Resources/config.json"))
            {
                Console.WriteLine("Config file not found. Initializing bot settings.");

                switch (settings.ApplicationRunningMethod)
                {
                    case ApplicationRunningMethod.Console:
                        Console.WriteLine("Please input your bot token: ");
                        settings.Token = Console.ReadLine();

                        Console.WriteLine("Please choose your bot prefix: ");
                        settings.CommandPrefix = Console.ReadLine();
                        break;

                    case ApplicationRunningMethod.Service:
                        Console.WriteLine("Application is running as a service, please change config.");
                        break;
                }

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                System.IO.File.WriteAllText("Resources/config.json", json);
            }
        }

        private async Task ClientReady()
        {
            await client.SetGameAsync("Hello there");
            markov = new Markov(client);
            Sokoban = new Sokoban.Game();
        }

        private static void AutoSave(object source, ElapsedEventArgs e)
        {
            markov.SerializeDict();
            Console.WriteLine("Dictionaries auto-saved.");
        }

        private async Task JoinedGuild(SocketGuild Guild)
        {
            if (!markov.MarkovDict.ContainsKey(Guild.Id))
                markov.MarkovDict[Guild.Id] = new Dictionary<string, List<string>>();
        }

        public async Task MainAsync()
        {
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Resources/config.json"));

            commandHandler = new CommandHandler(client, commands, settings.CommandPrefix);
            await commandHandler.InstallCommandsAsync();

            loggingService = new LoggingService(client, commands);

            try
            {
                await client.LoginAsync(TokenType.Bot, settings.Token);
                await client.StartAsync();
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

            while (!stopBot)
            {
                if (settings.ApplicationRunningMethod != ApplicationRunningMethod.Service)
                    await ProcessConsoleInput();
            }
        }

        public async Task ProcessConsoleInput()
        {
            var consoleInput = (await GetInputAsync()).ToLower();

            switch (consoleInput)
            {
                case "stop":
                    markov.SerializeDict();
                    stopBot = true;
                    break;

                case "hello there":
                    Console.WriteLine("General Kenobi");
                    break;

                default:
                    Console.WriteLine("Command not recognized.");
                    break;
            }
        }

        private async Task<string> GetInputAsync()
        {
            return await Task.Run(() => Console.ReadLine());
        }      
    }
}
