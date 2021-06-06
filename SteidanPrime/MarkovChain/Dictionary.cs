using Discord.Commands;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace SteidanPrime.MarkovChain
{
    [Group("Dictionary")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class Dictionary : ModuleBase<SocketCommandContext>
    {
        [Remarks("Deletes everything from the Markov dictionary for this server in case you want to start fresh. Requires ``Administrator`` permission to use.")]
        [Summary("Resets the current dictionary for Markov chain.")]
        [Command("reset")]
        public async Task ResetDictionary()
        {
            ulong guildId = Context.Guild.Id;
            Dictionary<string, List<string>> markovDictionary = new Dictionary<string, List<string>>();
            Program.Markov.MarkovDict[guildId] = markovDictionary;

            string markovJson = JsonConvert.SerializeObject(markovDictionary, Formatting.Indented);
            await File.WriteAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json", markovJson);

            await Context.Channel.SendMessageAsync("Dictionary successfully reset.");
        }

        [Remarks("In case you want to see what's in the Markov dictionary for this server. Exports it as a json file in the same channel where the command is called. Requires ``Administrator`` permission to use.")]
        [Summary("Exports the current Markov chain dictionary as a json file.")]
        [Command("export")]
        public async Task ExportDictionary()
        {
            ulong guildId = Context.Guild.Id;
            Dictionary<string, List<string>> dictionary = Program.Markov.MarkovDict[guildId];

            string markovJson = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            await File.WriteAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json", markovJson);

            await Context.Channel.SendFileAsync("Resources/Dictionaries/" + guildId.ToString() + ".json");
        }

        [Remarks("If you are experiencing issues with Markov, try reloading the dictionary, but it shouldn't be necessary. Requires ``Administrator`` permission to use.")]
        [Summary("Reloads the current Markov chain dictionary in case of issues.")]
        [Command("reload")]
        public async Task ReloadDictionary()
        {
            ulong guildId = Context.Guild.Id;
            Dictionary<string, List<string>> dictionary;

            if (File.Exists("Resources/Dictionaries/" + guildId.ToString() + ".json"))
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(await File.ReadAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json"));
            else
                dictionary = new Dictionary<string, List<string>>();

            Program.Markov.MarkovDict[guildId] = dictionary;
            await Context.Channel.SendMessageAsync("Dictionary successfully reloaded.");
        }
    }
}
