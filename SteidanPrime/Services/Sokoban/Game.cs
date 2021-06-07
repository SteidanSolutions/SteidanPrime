using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;

namespace SteidanPrime.Services.Sokoban
{
    public class Game
    {
        public int Level { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Grid Grid { get; set; }
        public RestUserMessage CurrentEmbed { get; set; }

        public Game()
        {
            Level = 1;
            Width = 9;
            Height = 6;
            Grid = new Grid(Width, Height, Level);
            CurrentEmbed = null;
        }

        public async Task SendGameEmbed(SocketCommandContext context)
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = $"Level {Level}",
                Description = Grid.ToString()
            };

            var embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = "Type W, A, S or D to move or R to reset."
            };

            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();

            if (CurrentEmbed == null)
                CurrentEmbed = await context.Channel.SendMessageAsync(null, false, embed);
            else
                await CurrentEmbed.ModifyAsync(x =>
                {
                    x.Content = "";
                    x.Embed = new EmbedBuilder()
                        .WithTitle($"Level {Level}")
                        .WithDescription(Grid.ToString())
                        .WithFooter(embedFooterBuilder)
                        .Build();
                });
        }

        public async Task ContinueGame(SocketCommandContext context)
        {
            Level += 1;

            if (Width < 13)
                Width += 2;

            if (Height < 8)
                Height += 1;

            Grid = new Grid(Width, Height, Level);
            CurrentEmbed = null;
            await SendGameEmbed(context);
        }

        public async Task SendWinEmbed(SocketCommandContext context)
        {
            var prefix = Program.Settings.CommandPrefix;
            var embedBuilder = new EmbedBuilder
            {
                Title = "You win!",
                Description = $"Type ``{prefix}sokoban continue`` to continue to level {Level}" +
                              $" or ``{prefix}sokoban stop`` to quit."
            };
            var embed = embedBuilder.Build();
            await context.Channel.SendMessageAsync(null, false, embed);
        }

        public async Task MovePlayerAsync(SocketCommandContext context, Movement direction)
        {
            if (!Grid.HasWon())
            {
                switch (direction)
                {
                    case Movement.UP:
                        Grid.Player.MoveUp();
                        break;

                    case Movement.RIGHT:
                        Grid.Player.MoveRight();
                        break;

                    case Movement.DOWN:
                        Grid.Player.MoveDown();
                        break;

                    case Movement.LEFT:
                        Grid.Player.MoveLeft();
                        break;

                    case Movement.RESET:
                        Grid.Reset();
                        break;
                }

                if (!Grid.HasWon())
                    await SendGameEmbed(context);
            }

            if (Grid.HasWon())
            {
                await SendWinEmbed(context);
            }
        }

        public async Task ParseInput(SocketCommandContext context)
        {
            var moved = true;

            switch (context.Message.Content.Trim().ToLower())
            {
                case "w":
                    await MovePlayerAsync(context, Movement.UP);
                    break;

                case "d":
                    await MovePlayerAsync(context, Movement.RIGHT);
                    break;

                case "s":
                    await MovePlayerAsync(context, Movement.DOWN);
                    break;

                case "a":
                    await MovePlayerAsync(context, Movement.LEFT);
                    break;

                case "r":
                    await MovePlayerAsync(context, Movement.RESET);
                    break;
                default:
                    moved = false;
                    break;
            }

            if (moved)
                await context.Message.DeleteAsync();
        }
    }
}
