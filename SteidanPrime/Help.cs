using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace SteidanPrime
{
    public class Help : InteractiveBase
    {
        private readonly CommandService _commands;

        public Help(CommandService commands)
        {
            _commands = commands;
        }

        [Summary("Displays this message.")]
        [Command("help")]
        public async Task HelpCommand()
        { 
            var stringBuilder = new StringBuilder("**Steidan Prime** available commands:\n");
            var pages = new List<string>();
            foreach (var module in _commands.Modules)
            {
                stringBuilder.Append($"\n__{module.Name}__\n\n");
                foreach (var command in module.Commands)
                {
                    var parameters = string.Empty;
                    if (command.Parameters.Count > 0)
                        parameters = " ";

                    parameters = command.Parameters.Aggregate(parameters, (current, param) => current + param.Summary);

                    stringBuilder.Append(command.Module.Group == null
                        ? $"``{Program.Settings.CommandPrefix}{command.Name}{parameters}`` - {command.Summary ?? "No description available."}\n"
                        : $"``{Program.Settings.CommandPrefix}{command.Module.Group.ToLower()} {command.Name}{parameters}`` - {command.Summary ?? "No description available."}\n");
                }


                pages.Add(stringBuilder.ToString());
                stringBuilder = new StringBuilder();
            }

            await PagedReplyAsync(pages);
            //await Context.Channel.SendMessageAsync(stringBuilder.ToString());
        }

        [Remarks("You think you're funny?")]
        [Summary("Displays additional information about a specific command.")]
        [Command("help")]
        public async Task HelpCommand([Summary("<command>")] string param)
        {
            if (param == "help")
            {
                await Context.Channel.SendMessageAsync("You think you're funny?");
                return;
            }

            var commands = _commands.Commands.Where(x => string.Equals(x.Name, param, StringComparison.CurrentCultureIgnoreCase)).ToList();
            if (!commands.Any())
                await Context.Channel.SendMessageAsync($"Command ``{param}`` does not exist.");

            foreach (var command in commands)
            {
                var parameters = string.Empty;
                if (command.Parameters.Count > 0)
                    parameters = " ";

                parameters = command.Parameters.Aggregate(parameters, (current, param) => current + param.Summary);
                var message = $"``{Program.Settings.CommandPrefix}{param.ToLower()}{parameters}`` - {command.Remarks ?? "No description provided."}";
                await Context.Channel.SendMessageAsync(message);
            }
        }

        [Remarks("You think you're funny?")]
        [Summary("Displays additional information about a specific command.")]
        [Command("help")]
        public async Task HelpCommand([Summary("<multiple parameters>")]params string[] stringArray)
        {
            foreach (var module in _commands.Modules.Where(x => string.Equals(x.Name, stringArray[0], StringComparison.CurrentCultureIgnoreCase)).ToList())
            {
                foreach (var command in _commands.Commands.Where(c => string.Equals(c.Name, stringArray[1], StringComparison.CurrentCultureIgnoreCase)).ToList())
                {
                    var parameters = string.Empty;
                    if (command.Parameters.Count > 0)
                        parameters = " ";

                    parameters = command.Parameters.Aggregate(parameters, (current, param) => current + param.Summary);
                    await Context.Channel.SendMessageAsync($"``{Program.Settings.CommandPrefix}{module.Name.ToLower()} {command.Name.ToLower()}{parameters}`` - {command.Remarks ?? "No description provided."}");
                }
            }
        }
    }
}
