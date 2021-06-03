using Discord.Commands;
using System.Threading.Tasks;

namespace SteidanPrime.Sokoban
{
    [Group("sokoban")]
    public class SokobanCommandHandler : ModuleBase<SocketCommandContext>
    {
        [Command("play")]
        public async Task StartGame()
        {
            Program.Sokoban.NewGame(Context);
            await Context.Message.DeleteAsync();
        }

        [Command("stop")]
        public async Task StopGame()
        {
            if (Program.Sokoban.GameActive)
            {
                Program.Sokoban.StopGame();
                await Context.Channel.SendMessageAsync("Game stopped.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("No game currently running.");
            }

            await Context.Message.DeleteAsync();
        }

        [Command("continue")]
        public async Task ContinueGame()
        {
            await Program.Sokoban.ContinueGame();
            await Context.Message.DeleteAsync();
        }
        
    }
}
