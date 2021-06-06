using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SteidanPrime.MarkovChain
{
    public class Chain : ModuleBase<SocketCommandContext>
    {
        [Remarks("The bot logs messages sent by users into a dictionary. When the command is called it uses Markov chains to construct a sentence. Purely for fun.")]
        [Summary("Generates a random message using Markov chains.")]
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
