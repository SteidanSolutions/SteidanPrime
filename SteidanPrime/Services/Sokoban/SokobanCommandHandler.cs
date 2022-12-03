using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace SteidanPrime.Services.Sokoban
{
    [Group("sokoban", "sokoban game")]
    public class SokobanCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SokobanService _sokobanService;

        public SokobanCommandHandler(SokobanService sokobanService)
        {
            _sokobanService = sokobanService;
        }

        [SlashCommand("play", "play gaem")]
        public async Task StartGame()
        {
            (Embed, bool) embed = await _sokobanService.NewGameAsync(Context.User.Id);
            await RespondAsync(embed:embed.Item1,
                components: new ComponentBuilder().WithButton("          ", "btnEmptyL", ButtonStyle.Secondary, disabled:true)
                    .WithButton(null, "btnUp", ButtonStyle.Secondary, Emoji.Parse(":arrow_up:"))
                    .WithButton("          ", "btnEmptyR", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Reset", "btnReset", ButtonStyle.Primary)
                    .WithButton("Stop", "btnStop", ButtonStyle.Danger)
                    .WithButton(null, "btnLeft", ButtonStyle.Secondary, Emoji.Parse(":arrow_left:"), row: 1)
                    .WithButton(null, "btnDown", ButtonStyle.Secondary, Emoji.Parse(":arrow_down:"), row: 1)
                    .WithButton(null, "btnRight", ButtonStyle.Secondary, Emoji.Parse(":arrow_right:"), row: 1)
                    .WithButton("Next level", "btnNextLevel", ButtonStyle.Success, disabled:true, row: 1).Build());
            //await _sokobanService.NewGameAsync(Context);
        }


    }
}
