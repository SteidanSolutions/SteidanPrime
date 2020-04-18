using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime
{
    public class PrintDict : ModuleBase<SocketCommandContext>
    {
        [Command("print")]
        public async Task Print()
        {
            string str = "";

            foreach (KeyValuePair<string, List<string>> entry in Program.markov.MarkovDict)
            {
                if (entry.Value.Count > 0)
                {
                    str += entry.Key + ": ";
                    foreach (string word in entry.Value)
                    {
                        str += word + ' ';
                    }
                    str += "\n";
                }
            }

            //Program.MarkovDict.Clear();

            if (!str.Equals(""))
                await Context.Channel.SendMessageAsync(str);
            else
                await Context.Channel.SendMessageAsync("Type something first you cunt.");
        }
    }
}
