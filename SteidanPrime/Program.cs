using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SteidanPrime
{
    class Program
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        public CommandHandler commandHandler;
        public LoggingService loggingService;
        private Settings settings;
        public static Markov markov { get; set; }

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

            client.Ready += clientReady;
            client.JoinedGuild += joinedGuild;

            try
            {
                StreamReader sr = new StreamReader("config.json");
            }
            catch
            {
                Console.WriteLine("Config file not found. Initializing bot settings.");
                Console.WriteLine("Please input your bot token: ");
                settings.Token = Console.ReadLine();

                Console.WriteLine("Please choose your bot prefix: ");
                settings.CommandPrefix = Console.ReadLine();

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                System.IO.File.WriteAllText("config.json", json);
            }
        }

        private async Task clientReady()
        {
            await client.SetGameAsync("Hello there");
            markov = new Markov(client);
            foreach (var Guild in client.Guilds)
            {
                Console.WriteLine(Guild.Id.ToString());
            }
        }

        private async Task joinedGuild(SocketGuild Guild)
        {
            if (!markov.MarkovDict.ContainsKey(Guild.Id))
                markov.MarkovDict[Guild.Id] = new Dictionary<string, List<string>>();
        }

        public async Task MainAsync()
        {
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.json"));

            commandHandler = new CommandHandler(client, commands, settings.CommandPrefix);
            await commandHandler.InstallCommandsAsync();

            loggingService = new LoggingService(client, commands);

            await client.LoginAsync(TokenType.Bot, settings.Token);
            await client.StartAsync();

            bool stopBot = false;

            while (!stopBot)
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
        }

        private async Task<string> GetInputAsync()
        {
            return await Task.Run(() => Console.ReadLine());
        }      
    }
}
