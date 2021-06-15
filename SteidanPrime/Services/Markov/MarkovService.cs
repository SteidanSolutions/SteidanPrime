using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace SteidanPrime.Services.Markov
{
    public class MarkovService
    {
        private readonly DiscordSocketClient _client;
        public Dictionary<ulong, Dictionary<string, List<string>>> MarkovDict { get; set; }

        public MarkovService(DiscordSocketClient client)
        {
            MarkovDict = new Dictionary<ulong, Dictionary<string, List<string>>>();
            _client = client;
            _client.JoinedGuild += JoinedGuild;
            DeserializeDict();
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

        public void SerializeDict()
        {
            foreach (var guild in _client.Guilds)
            {
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
                if (MarkovDict.ContainsKey(guild.Id))
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
    }
}
