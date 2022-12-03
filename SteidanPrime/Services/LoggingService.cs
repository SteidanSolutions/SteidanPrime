using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace SteidanPrime.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _client;
        public LoggingService(DiscordSocketClient client, InteractionService commands)
        {
            _client = client;
            client.Log += LogAsync;
            commands.Log += LogAsync;
        }

        private Task LogAsync(LogMessage message)
        {
            Console.ForegroundColor = message.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.Red,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Verbose => ConsoleColor.DarkGray,
                LogSeverity.Debug => ConsoleColor.DarkGray,
                _ => Console.ForegroundColor
            };

            //using StreamWriter file = new("log.txt", append: true);
            //file.WriteLineAsync($"{DateTime.Now,-19} [{message.Severity}] {message.Source}: {message.Message} {message.Exception}");
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }

    }
}
