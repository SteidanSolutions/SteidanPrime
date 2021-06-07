using System.Threading.Tasks;
using Discord.Commands;

namespace SteidanPrime.Services.Sokoban
{
    [Group("Sokoban")]
    public class SokobanCommandHandler : ModuleBase<SocketCommandContext>
    {
        private readonly SokobanService _sokobanService;

        public SokobanCommandHandler(SokobanService sokobanService)
        {
            _sokobanService = sokobanService;
        }

        [Remarks("Starts a game of Sokoban in a text channel using embeds. Follow instructions in the message to play. Bot should probably have ``Manage Messages`` permission so it can delete messages used to play the game and prevent the spam.")]
        [Summary("Starts the Sokoban game in an embed message.")]
        [Command("play")]
        public async Task StartGame()
        {
            await _sokobanService.NewGameAsync(Context);
            await Context.Message.DeleteAsync();
        }

        [Remarks("Use to end the current game of Sokoban.")]
        [Summary("End the current Sokoban game.")]
        [Command("stop")]
        public async Task StopGame()
        {
            if (_sokobanService.SokobanGameDictionary.ContainsKey(Context.Guild.Id))
            {
                _sokobanService.StopGame(Context);
                await Context.Channel.SendMessageAsync("Sokoban game stopped.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("No game currently running.");
            }

            await Context.Message.DeleteAsync();
        }

        [Remarks("Use after you finish a level of Sokoban to continue to the next one.")]
        [Summary("Continue the current Sokoban and start the next level.")]
        [Command("continue")]
        public async Task ContinueGame()
        {
            await _sokobanService.ContinueGame(Context);
            await Context.Message.DeleteAsync();
        }
        
    }
}
