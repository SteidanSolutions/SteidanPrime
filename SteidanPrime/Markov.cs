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
        public Dictionary<string, List<string>> MarkovDict { get; set; }

        public Markov(DiscordSocketClient client, string path)
        {
            this.client = client;
            DeserializeDict(path);
        }

        public string GetChain(SocketGuild guild)
        {
            var rand = new Random();
            List<string> keys = new List<string>(MarkovDict.Keys);

            string currentPair = keys[rand.Next(keys.Count)];
            string message = currentPair;

            while (!MarkovDict.ContainsKey(currentPair))
                currentPair = keys[rand.Next(keys.Count)];

            for (int i = 0; i < 15; i++)
            {

                if (!MarkovDict.ContainsKey(currentPair))
                    break;

                List<string> words = MarkovDict[currentPair];

                string nextWord = words[rand.Next(words.Count)];

                message += " " + nextWord;

                string[] pairSplit = currentPair.Split(' ');
                currentPair = pairSplit[1] + " " + nextWord;

            }

            return message;
        }

        public void SerializeDict(string path)
        {
            string markovJson = JsonConvert.SerializeObject(MarkovDict, Formatting.Indented);
            System.IO.File.WriteAllText("markovDict.json", markovJson);
        }

        public void DeserializeDict(string path)
        {
            if (File.Exists(path))
                MarkovDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("markovDict.json"));
            else
                MarkovDict = new Dictionary<string, List<string>>();
        }
    }
}
