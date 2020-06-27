using Discord.Commands;
using Discord.WebSocket;
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
            if (Program.markov.MarkovDict[Context.Guild.Id].Keys.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Type something first you cunt.");
                return;
            }

            SocketGuild guild = Context.Guild;
            string message = Program.markov.GetChain(guild);
            await Context.Channel.SendMessageAsync(message);
        }
    }
}
