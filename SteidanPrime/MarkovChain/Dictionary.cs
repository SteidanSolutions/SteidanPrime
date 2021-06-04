using Discord.Commands;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace SteidanPrime.MarkovChain
{
    [Group("dictionary")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class Dictionary : ModuleBase<SocketCommandContext>
    {
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

        [Command("export")]
        public async Task ExportDictionary()
        {
            ulong guildId = Context.Guild.Id;
            Dictionary<string, List<string>> dictionary = Program.Markov.MarkovDict[guildId];

            string markovJson = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            await File.WriteAllTextAsync("Resources/Dictionaries/" + guildId.ToString() + ".json", markovJson);

            await Context.Channel.SendFileAsync("Resources/Dictionaries/" + guildId.ToString() + ".json");
        }

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
