using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime.MarkovChain
{
    public class Chain : ModuleBase<SocketCommandContext>
    {
        [Command("chain")]
        public async Task PrintChain()
        {
            if (Program.Markov.MarkovDict[Context.Guild.Id].Keys.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Type something first you cunt.");
                return;
            }

            SocketGuild guild = Context.Guild;
            string message = Program.Markov.GetChain(guild);
            await Context.Channel.SendMessageAsync(message);
        }
    }
}
