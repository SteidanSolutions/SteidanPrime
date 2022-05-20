using System.Threading.Tasks;
using Discord.Interactions;

namespace SteidanPrime.Services.Markov
{
    public class Chain : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private readonly MarkovService _markovService;
        public Chain(MarkovService markovService)
        {
            _markovService = markovService;
        }

        [SlashCommand("chain", "Constructs a sentence using Markov chains. Option to have it begin with a specified word.")]
        public async Task PrintChain([Summary("word")] string arg = "")
        {
            if (_markovService.MarkovDict[Context.Guild.Id].Keys.Count == 0)
            {
                await RespondAsync("Type something first you cunt.");
                return;
            }

            var guild = Context.Guild;
            var message = arg == "" ? _markovService.GetChain(guild) : _markovService.GetChainWithSpecificWord(guild, arg);
            if (message == "")
                await RespondAsync("Could not find a chain with the specified word.");
            else
                await RespondAsync(message);
        }
    }
}
