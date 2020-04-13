using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteidanPrime
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly string prefix;

        public CommandHandler(DiscordSocketClient client, CommandService commands, string prefix)
        {
            this.client = client;
            this.commands = commands;
            this.prefix = prefix;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (message.Author.IsBot)
                return;

            // If it's not a command, parse the message for Markov chains
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            {
                string msg = message.ToString().Trim().ToLower();

                Regex reg = new Regex("[*\",_&#^@?!*-+\\.]+");
                msg = reg.Replace(msg, " ");

                msg = Regex.Replace(msg, @"\s+", " ");
                string[] words = msg.Split(' ');


                for (int i = 0; i < words.Length - 2; i++)
                {
                    string key = words[i] + ' ' + words[i + 1];

                    List<string> value = new List<string>();
                    if (Program.MarkovDict.TryGetValue(key, out value))
                    {
                        Program.MarkovDict[key].Add(words[i + 2]);
                    }
                    else
                    {
                        List<string> v = new List<string>();
                        v.Add(words[i + 2]);
                        Program.MarkovDict[key] = v;
                    }
                }

                return;
            }



            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }
    }
}
