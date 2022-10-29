using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json;

namespace SteidanPrime.Services.Markov
{
    [Group("dictionary", "Contains all the text data used for chain commands.")]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public class Dictionary : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private readonly MarkovService _markovService;

        public Dictionary(MarkovService markovService)
        {
            _markovService = markovService;
        }

        [SlashCommand("reset", "Deletes everything from the Markov dictionary for this server. Requires ``Administrator`` to use.")]
        public async Task ResetDictionary()
        {
            ulong guildId = Context.Guild.Id;
            Dictionary<string, List<string>> markovDictionary = new Dictionary<string, List<string>>();
            _markovService.MarkovDict[guildId] = markovDictionary;

            string markovJson = JsonConvert.SerializeObject(markovDictionary, Formatting.Indented);
            await File.WriteAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json", markovJson);

            await RespondAsync("Dictionary successfully reset.");
        }

        [SlashCommand("export", "Exports the current Markov chain dictionary as a json file.")]
        public async Task ExportDictionary()
        {
            ulong guildId = Context.Guild.Id;
            Dictionary<string, List<string>> dictionary = _markovService.MarkovDict[guildId];

            string markovJson = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            await File.WriteAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json", markovJson);

            await RespondWithFileAsync("Resources/Dictionaries/" + guildId.ToString() + ".json");
        }

        [SlashCommand("reload", "Reloads the current Markov chain dictionary in case of issues.")]
        public async Task ReloadDictionary()
        {
            ulong guildId = Context.Guild.Id;
            Dictionary<string, List<string>> dictionary;

            if (File.Exists("Resources/Dictionaries/" + guildId.ToString() + ".json"))
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(await File.ReadAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json"));
            else
                dictionary = new Dictionary<string, List<string>>();

            _markovService.MarkovDict[guildId] = dictionary;
            await RespondAsync("Dictionary successfully reloaded.");
        }
    }
}
