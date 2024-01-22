using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SteidanPrime.Services.Sokoban;

namespace SteidanPrime.Services.Markov
{
    public class MarkovService : IMarkovService
    {
        private readonly DiscordSocketClient _client;
        private Dictionary<ulong, Dictionary<string, List<string>>> MarkovDict { get; set; }

        public MarkovService(DiscordSocketClient client)
        {
            MarkovDict = new Dictionary<ulong, Dictionary<string, List<string>>>();
            _client = client;
            _client.JoinedGuild += JoinedGuild;
            _client.MessageReceived += HandleTextAsync;
            DeserializeDict();
        }

        public Dictionary<ulong, Dictionary<string, List<string>>> GetMarkovDict()
        {
            return MarkovDict;
        }

        private Task HandleTextAsync(SocketMessage messageParam)
        {
            return Task.Run(async () =>
            {
                var message = messageParam as SocketUserMessage;
                if (message == null) return;

                var channel = message.Channel as SocketGuildChannel;
                var guild = channel.Guild;

                if (message.Author.IsBot)
                    return;

                if (message.Content.ToLower().Contains("madge"))
                    await new SocketCommandContext(_client, message).Message.AddReactionAsync(
                        Emote.Parse("<:Madge:788536698098810881>"));

                if (message.Content.StartsWith("/chain") || message.Content.StartsWith("!chain"))
                    await message.Channel.SendMessageAsync("https://tenor.com/view/devil-may-cry-dante-angry-gif-13322659");

                var msg = message.ToString().Trim().ToLower();

                msg = Regex.Replace(msg,
                        @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?", " ");
                msg = Regex.Replace(msg, "[*\",_&^*\\-+.?;[\\]'/|\\\\`~{}]+", " ");
                msg = Regex.Replace(msg, @"\s+", " ");
                await ParseMarkovWords(msg.Split(' ', StringSplitOptions.RemoveEmptyEntries), guild.Id);
            });
        }

        public Task HandleTextAsync(string message, ulong guildId)
        {
            return Task.Run(async () =>
            {
                var msg = message.ToString().Trim().ToLower();

                msg = Regex.Replace(msg,
                    @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?", " ");
                msg = Regex.Replace(msg, "[*\",_&^*\\-+.?;[\\]'/|\\\\`~{}]+", " ");
                msg = Regex.Replace(msg, @"\s+", " ");
                await ParseMarkovWords(msg.Split(' ', StringSplitOptions.RemoveEmptyEntries), guildId);
            });
        }

        public string GetChain(SocketGuild guild)
        {
            var rand = new Random();
            Dictionary<string, List<string>> guildDict = MarkovDict[guild.Id];
            List<string> keys = new List<string>(guildDict.Keys);

            var currentPair = keys[rand.Next(keys.Count)];
            var message = currentPair;

            while (!guildDict.ContainsKey(currentPair))
                currentPair = keys[rand.Next(keys.Count)];

            for (int i = 0; i < 20; i++)
            {

                if (!guildDict.ContainsKey(currentPair))
                    break;

                List<string> words = guildDict[currentPair];

                string nextWord = words[rand.Next(words.Count)];

                message += " " + nextWord;

                string[] pairSplit = currentPair.Split(' ');
                currentPair = pairSplit[1] + " " + nextWord;

            }

            return message;
        }

        public string GetChainWithSpecificWord(SocketGuild guild, string word)
        {
            var rand = new Random();
            Dictionary<string, List<string>> guildDict = MarkovDict[guild.Id];
            List<string> keys = new List<string>(guildDict.Keys);

            keys = keys.Where(key => key.Split(" ").Contains(word.ToLower())).ToList();

            if (keys.Count == 0)
                return "";

            var currentPair = keys[rand.Next(keys.Count)];
            var message = currentPair;

            while (!guildDict.ContainsKey(currentPair))
                currentPair = keys[rand.Next(keys.Count)];

            for (int i = 0; i < 20; i++)
            {

                if (!guildDict.ContainsKey(currentPair))
                    break;

                List<string> words = guildDict[currentPair];

                string nextWord = words[rand.Next(words.Count)];

                message += " " + nextWord;

                string[] pairSplit = currentPair.Split(' ');
                currentPair = pairSplit[1] + " " + nextWord;

            }

            return message;
        }

        public void SerializeDict()
        {
            foreach (var guild in _client.Guilds)
            {
                //TODO change Newtonsoft.Json to System.Text.Json
                var markovJson = JsonConvert.SerializeObject(MarkovDict[guild.Id], Formatting.Indented);
                File.WriteAllText("Resources/Dictionaries/" + guild.Id.ToString() + ".json", markovJson);
            }
        }

        public void DeserializeDict()
        {
            foreach (var guild in _client.Guilds)
            {
                if (File.Exists("Resources/Dictionaries/" + guild.Id.ToString() + ".json"))
                    MarkovDict[guild.Id] = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("Resources/Dictionaries/" + guild.Id.ToString() + ".json"));
                else
                    MarkovDict[guild.Id] = new Dictionary<string, List<string>>();
            }
        }

        public Task ParseMarkovWords(string[] words, ulong guildId)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < words.Length - 2; i++)
                {
                    string key = words[i] + ' ' + words[i + 1];

                    if (MarkovDict[guildId].ContainsKey(key))
                    {
                        MarkovDict[guildId][key].Add(words[i + 2]);
                    }
                    else
                    {
                        var v = new List<string> {words[i + 2]};
                        MarkovDict[guildId][key] = v;
                    }
                }
            });
        }

        private Task JoinedGuild(SocketGuild guild)
        {
            return Task.Run(() =>
            {
                if (!MarkovDict.ContainsKey(guild.Id))
                    MarkovDict[guild.Id] = new Dictionary<string, List<string>>();
            });
        }

        public ulong GetTotalWords()
        {
            ulong total = 0;
            foreach (var dictionary in MarkovDict.Values)
            {
                foreach (var wordsList in dictionary.Values)
                {
                    total += (ulong) wordsList.Count;
                }
            }

            return total;
        }

        public void FixMissingDictionaries()
        {
            foreach (var guild in _client.Guilds)
            {
                if (!MarkovDict.ContainsKey(guild.Id))
                    MarkovDict[guild.Id] = new Dictionary<string, List<string>>();
            }
        }
    }
}
