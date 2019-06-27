using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime
{
    public class HelloThere : ModuleBase<SocketCommandContext>
    {
        [Command("hello")]
        [Alias("hello there")]
        [Summary("hello there command")]
        public async Task Kenobi()
        {
            await Context.Channel.SendMessageAsync("General Kenobi");
        }
    }
}
