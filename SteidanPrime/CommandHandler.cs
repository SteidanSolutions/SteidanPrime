using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;

namespace SteidanPrime
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly string _prefix;

        public CommandHandler(DiscordSocketClient client, CommandService commands, string prefix)
        {
            _client = client;
            _commands = commands;
            _prefix = prefix;
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
                                            services: null);
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
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
            _ = Task.Run(async () =>
            {
                if (message == null) return;

                var channel = message.Channel as SocketGuildChannel;
                var guild = channel.Guild;

                if (message.Author.IsBot)
                    return;

                if (message.Content.ToLower().Contains("madge"))
                    await new SocketCommandContext(_client, message).Channel.SendMessageAsync("<:Madge:788536698098810881>");

                if (Program.Sokoban.GameActive)
                {
                    var msg = message.Content;
                    var game = Program.Sokoban;

                    var moved = true;

                    switch (msg.Trim().ToLower())
                    {
                        case "w":
                            await game.MovePlayerAsync(Sokoban.Movement.UP);
                            break;

                        case "d":
                            await game.MovePlayerAsync(Sokoban.Movement.RIGHT);
                            break;

                        case "s":
                            await game.MovePlayerAsync(Sokoban.Movement.DOWN);
                            break;

                        case "a":
                            await game.MovePlayerAsync(Sokoban.Movement.LEFT);
                            break;

                        case "r":
                            await game.MovePlayerAsync(Sokoban.Movement.RESET);
                            break;
                        default:
                            moved = false;
                            break;
                    }

                    if (moved)
                    {
                        await message.DeleteAsync();
                        return;
                    }
                }

                if (!(message.HasStringPrefix(_prefix, ref argPos) ||
                      message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                {
                    string msg = message.ToString().Trim().ToLower();

                    msg = Regex.Replace(msg,
                        @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?", " ");
                    msg = Regex.Replace(msg, "[*\",_&^*\\-+.?;[\\]'/|\\\\`~{}]+", " ");
                    msg = Regex.Replace(msg, @"\s+", " ");

                    await ParseMarkovWords(msg.Split(' ', System.StringSplitOptions.RemoveEmptyEntries), guild.Id);
                }
            });
            return Task.CompletedTask;
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
                services: null);
        }

        private static Task ParseMarkovWords(string[] words, ulong guildId)
        {
            for (int i = 0; i < words.Length - 2; i++)
            {
                string key = words[i] + ' ' + words[i + 1];

                if (Program.Markov.MarkovDict[guildId].ContainsKey(key))
                {
                    Program.Markov.MarkovDict[guildId][key].Add(words[i + 2]);
                }
                else
                {
                    var v = new List<string> {words[i + 2]};
                    Program.Markov.MarkovDict[guildId][key] = v;
                }
            }
            return Task.CompletedTask;
        }
    }
}
