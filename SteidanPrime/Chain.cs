using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime
{
    public class Chain : ModuleBase<SocketCommandContext>
    {
        [Command("chain")]
        public async Task PrintChain()
        {
            if (Program.MarkovDict.Keys.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Type something first you cunt.");
                return;
            }

            var rand = new Random();
            List<string> keys = new List<string>(Program.MarkovDict.Keys);

            string currentPair = keys[rand.Next(keys.Count)];
            string message = currentPair;

            for (int i = 0; i < 15; i++)
            {
                while (!Program.MarkovDict.ContainsKey(currentPair))
                    currentPair = keys[rand.Next(keys.Count)];

                List<string> words = Program.MarkovDict[currentPair];

                string nextWord = words[rand.Next(words.Count)];
                message += " " + nextWord;

                string[] pairSplit = currentPair.Split(' ');
                currentPair = pairSplit[1] + " " + nextWord;
            }

            Program.MarkovDict.Clear();
            await Context.Channel.SendMessageAsync(message);
        }
    }
}
