﻿using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace SteidanPrime.Services.Markov
{
    public class Chain : ModuleBase<SocketCommandContext>
    {
        private readonly MarkovService _markovService;
        public Chain(MarkovService markovService)
        {
            _markovService = markovService;
        }

        [Remarks("The bot logs messages sent by users into a dictionary. When the command is called it uses Markov chains to construct a sentence. Purely for fun.")]
        [Summary("Generates a random message using Markov chains.")]
        [Command("chain")]
        public async Task PrintChain()
        {
            if (_markovService.MarkovDict[Context.Guild.Id].Keys.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Type something first you cunt.");
                return;
            }

            SocketGuild guild = Context.Guild;
            string message = _markovService.GetChain(guild);
            await Context.Channel.SendMessageAsync(message);
        }

        [Remarks("Invoked with ``chain <word>``, works the same as normal chain, but begins the chain with the specified word.")]
        [Summary("Works the same as normal chain, but begins the chain with the specified word.")]
        [Command("chain")]
        public async Task PrintChain(string arg)
        {
            if (_markovService.MarkovDict[Context.Guild.Id].Keys.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Type something first you cunt.");
                return;
            }

            SocketGuild guild = Context.Guild;
            string message = _markovService.GetChainWithSpecificWord(guild, arg);
            if (message == "")
                await Context.Channel.SendMessageAsync("Could not find a chain with the specified word.");
            else
                await Context.Channel.SendMessageAsync(message);
        }
    }
}
