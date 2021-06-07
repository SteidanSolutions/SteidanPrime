using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SteidanPrime.Services.Markov;
using SteidanPrime.Services.Sokoban;

namespace SteidanPrime.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly string _prefix;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, string prefix)
        {
            _client = client;
            _commands = commands;
            _prefix = prefix;
            _services = services;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            //_client.MessageReceived += HandleCommandAsync;
            _client.MessageReceived += HandleMessageAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync; 

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: _services);
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync($"You do not have sufficient permissions to use ``!{command.Value.Name}``. Use ``!help {command.Value.Name}`` to find out more.");
                        break;
                    case CommandError.Unsuccessful:
                        break;
                    case CommandError.UnknownCommand:
                        break;
                    case CommandError.ParseFailed:
                        break;
                    case CommandError.BadArgCount:
                        break;
                    case CommandError.ObjectNotFound:
                        break;
                    case CommandError.MultipleMatches:
                        break;
                    case CommandError.Exception:
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private Task HandleMessageAsync(SocketMessage messageParam)
        {
            _ = Task.Run(async () =>
            {
                var message = messageParam as SocketUserMessage;
                int argPos = 0;

                if (message.Author.IsBot)
                    return;

                if (message.HasStringPrefix(_prefix, ref argPos) ||
                      message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                    await HandleCommandAsync(message, argPos);
                else
                    await HandleTextAsync(message, argPos);
            });
            return Task.CompletedTask;
        }

        private Task HandleTextAsync(SocketUserMessage message, int argPos = 0)
        {
            return Task.Run(async () =>
            {
                if (message == null) return;

                var channel = message.Channel as SocketGuildChannel;
                var guild = channel.Guild;

                if (message.Author.IsBot)
                    return;

                if (message.Content.ToLower().Contains("madge"))
                    await new SocketCommandContext(_client, message).Message.AddReactionAsync(
                    Emote.Parse("<:Madge:788536698098810881>"));

                if (_services.GetRequiredService<SokobanService>().SokobanGameDictionary.ContainsKey(guild.Id))
                    await _services.GetRequiredService<SokobanService>().ParseInput(new SocketCommandContext(_client, message));

                if (!(message.HasStringPrefix(_prefix, ref argPos) ||
                      message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                {
                    string msg = message.ToString().Trim().ToLower();

                    msg = Regex.Replace(msg,
                        @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?", " ");
                    msg = Regex.Replace(msg, "[*\",_&^*\\-+.?;[\\]'/|\\\\`~{}]+", " ");
                    msg = Regex.Replace(msg, @"\s+", " ");
                    await _services.GetRequiredService<MarkovService>().ParseMarkovWords(msg.Split(' ', StringSplitOptions.RemoveEmptyEntries), guild.Id);
                }
            });
            //return Task.CompletedTask;
        }
        private async Task HandleCommandAsync(SocketUserMessage message, int argPos = 0)
        {
            // Don't process the command if it was a system message
            if (message == null) return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }
    }
}
