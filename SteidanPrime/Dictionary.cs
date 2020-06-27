using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime
{
    [Group("dictionary")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class Dictionary : ModuleBase<SocketCommandContext>
    {
        [Command("reset")]
        public async Task ResetDictionary()
        {
            ulong GuildId = Context.Guild.Id;
            Program.markov.MarkovDict[GuildId] = new Dictionary<string, List<string>>();
            await Context.Channel.SendMessageAsync("Dictionary successfully reset.");
        }

        [Command("export")]
        public async Task ExportDictionary()
        {
            ulong GuildId = Context.Guild.Id;
            Dictionary<string, List<string>> Dictionary = Program.markov.MarkovDict[GuildId];

            string MarkovJson = JsonConvert.SerializeObject(Dictionary, Formatting.Indented);
            System.IO.File.WriteAllText(GuildId.ToString() + ".json", MarkovJson);

            await Context.Channel.SendFileAsync(GuildId.ToString() + ".json");
        }

        [Command("reload")]
        public async Task ReloadDictionary()
        {
            ulong GuildId = Context.Guild.Id;
            Dictionary<string, List<string>> Dictionary = Program.markov.MarkovDict[GuildId];

            if (File.Exists(GuildId.ToString() + ".json"))
                Dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(GuildId.ToString() + ".json"));
            else
                Dictionary = new Dictionary<string, List<string>>();

            Program.markov.MarkovDict[GuildId] = Dictionary;
            await Context.Channel.SendMessageAsync("Dictionary successfully reloaded.");
        }
    }
}
