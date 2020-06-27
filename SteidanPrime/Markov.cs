using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteidanPrime
{
    class Markov
    {
        private readonly DiscordSocketClient client;
        public Dictionary<ulong ,Dictionary<string, List<string>>> MarkovDict { get; set; }

        public Markov(DiscordSocketClient client)
        {
            MarkovDict = new Dictionary<ulong, Dictionary<string, List<string>>>();
            this.client = client;
            DeserializeDict();
        }

        public string GetChain(SocketGuild guild)
        {
            var rand = new Random();
            Dictionary<string, List<string>> GuildDict = MarkovDict[guild.Id];
            List<string> keys = new List<string>(GuildDict.Keys);

            string currentPair = keys[rand.Next(keys.Count)];
            string message = currentPair;

            while (!GuildDict.ContainsKey(currentPair))
                currentPair = keys[rand.Next(keys.Count)];

            for (int i = 0; i < 15; i++)
            {

                if (!GuildDict.ContainsKey(currentPair))
                    break;

                List<string> words = GuildDict[currentPair];

                string nextWord = words[rand.Next(words.Count)];

                message += " " + nextWord;

                string[] pairSplit = currentPair.Split(' ');
                currentPair = pairSplit[1] + " " + nextWord;

            }

            return message;
        }

        public void SerializeDict()
        {
            foreach (var Guild in client.Guilds)
            {
                string markovJson = JsonConvert.SerializeObject(MarkovDict[Guild.Id], Formatting.Indented);
                System.IO.File.WriteAllText("Resources/Dictionaries/" + Guild.Id.ToString() + ".json", markovJson);
            }
        }

        public void DeserializeDict()
        {
            foreach (var Guild in client.Guilds)
            {
                if (File.Exists("Resources/Dictionaries/" + Guild.Id.ToString() + ".json"))
                    MarkovDict[Guild.Id] = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("Resources/Dictionaries/" + Guild.Id.ToString() + ".json"));
                else
                    MarkovDict[Guild.Id] = new Dictionary<string, List<string>>();
            }
        }
    }
}
