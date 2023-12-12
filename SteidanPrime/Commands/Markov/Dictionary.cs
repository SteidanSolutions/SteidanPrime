using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using SteidanPrime.Services.Markov;

namespace SteidanPrime.Commands.Markov
{
    [Group("dictionary", "Contains all the text data used for chain commands. Requires ``Administrator`` to use.")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class Dictionary : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private readonly IMarkovService _markovService;

        public Dictionary(IMarkovService markovService)
        {
            _markovService = markovService;
        }

        [SlashCommand("reset", "Deletes everything from the Markov dictionary for this server.")]
        public async Task ResetDictionary()
        {
            var guildId = Context.Guild.Id;
            var markovDictionary = new Dictionary<string, List<string>>();
            _markovService.GetMarkovDict()[guildId] = markovDictionary;

            //TODO change Newtonsoft.Json to System.Text.Json
            string markovJson = JsonConvert.SerializeObject(markovDictionary, Formatting.Indented);
            await File.WriteAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json", markovJson);

            await RespondAsync("Dictionary successfully reset.");
        }

        [SlashCommand("export", "Exports the current Markov chain dictionary as a json file.")]
        public async Task ExportDictionary()
        {
            var guildId = Context.Guild.Id;
            var dictionary = _markovService.GetMarkovDict()[guildId];

            var markovJson = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            await File.WriteAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json", markovJson);

            await RespondWithFileAsync("Resources/Dictionaries/" + guildId.ToString() + ".json");
        }

        [SlashCommand("reload", "Reloads the current Markov chain dictionary in case of issues.")]
        public async Task ReloadDictionary()
        {
            var guildId = Context.Guild.Id;
            Dictionary<string, List<string>> dictionary;

            dictionary = File.Exists("Resources/Dictionaries/" + guildId.ToString() + ".json") 
                ? JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(await File.ReadAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json")) 
                : new Dictionary<string, List<string>>();

            _markovService.GetMarkovDict()[guildId] = dictionary;
            await RespondAsync("Dictionary successfully reloaded.");
        }

        [SlashCommand("generate",
            "Scans last N words in this channel. WARNING: using this can inflate the number of words.")]
        public async Task GenerateDictionary([Summary("numberOfMessages")] int arg = 10)
        {
            await DeferAsync();
            var messages = Context.Channel.GetMessagesAsync(Context.Interaction.Id, Direction.Before, arg, CacheMode.AllowDownload, RequestOptions.Default).FlattenAsync().Result;
            foreach (var message in messages)
            {
                if (message.Author.IsBot)
                    continue;

                await _markovService.HandleTextAsync(message.Content, Context.Guild.Id);
            }

            await FollowupAsync($"Successfully scanned {messages.Count()} messages.");
        }
    }
}
