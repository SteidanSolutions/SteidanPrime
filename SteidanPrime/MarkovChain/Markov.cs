using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SteidanPrime.MarkovChain
{
    class Markov
    {
        private readonly DiscordSocketClient _client;
        public Dictionary<ulong, Dictionary<string, List<string>>> MarkovDict { get; set; }

        public Markov(DiscordSocketClient client)
        {
            MarkovDict = new Dictionary<ulong, Dictionary<string, List<string>>>();
            _client = client;
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
    }
}
