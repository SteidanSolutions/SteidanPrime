using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace SteidanPrime.Services.Sokoban
{
    public class SokobanService
    {
        public Dictionary<ulong, Game> SokobanGameDictionary = new Dictionary<ulong, Game>();

        public async Task NewGameAsync(SocketCommandContext context)
        {
            SokobanGameDictionary[context.Guild.Id] = new Game();
            await SendGameEmbed(context);
        }

        public void StopGame(SocketCommandContext context)
        {
            SokobanGameDictionary.Remove(context.Guild.Id);
        }

        public async Task ContinueGame(SocketCommandContext context)
        {
            await SokobanGameDictionary[context.Guild.Id].ContinueGame(context);
        }

        public async Task SendGameEmbed(SocketCommandContext context)
        {
            await SokobanGameDictionary[context.Guild.Id].SendGameEmbed(context);
        }

        public async Task ParseInput(SocketCommandContext context)
        {
            await SokobanGameDictionary[context.Guild.Id].ParseInput(context);
        }
    }
}
