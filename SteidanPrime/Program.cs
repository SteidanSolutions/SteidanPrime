using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
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
