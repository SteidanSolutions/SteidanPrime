using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using Newtonsoft.Json;

namespace SteidanPrime
{
    class Program
    {
        private readonly DiscordSocketClient Client;
        private readonly CommandService Commands;
        private Settings settings;

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false
            });

            Client.Log += Log;
            Commands.Log += Log;

            Settings settings = new Settings();

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
                settings.Prefix = Console.ReadLine();

                string json = JsonConvert.SerializeObject(settings);
                System.IO.File.WriteAllText("config.json", json);
            }
        }

        public async Task MainAsync()
        {
            settings.Token = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.json")).Token;
            await Client.LoginAsync(TokenType.Bot, settings.Token);

            //var token = "NTkyODE2NjU0NDg5MDkyMTIy.XRE1pA.CDZrfEeuauTXStfVEPUS1_7GPaE";
            //JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.json")).Token;

            await Client.StartAsync();

            //await Task.Delay(-1);
            await Task.Delay(Timeout.Infinite);
        }

        private Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
