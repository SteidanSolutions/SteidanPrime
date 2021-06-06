using Discord;
using Discord.Commands;
using Discord.Rest;
using System;
using System.Threading.Tasks;

namespace SteidanPrime.Sokoban
{
    class Game
    {
        public Boolean GameActive { get; set; }
        public int Level { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Grid Grid { get; set; }
        public SocketCommandContext Context { get; set; }
        public RestUserMessage CurrentEmbed { get; set; }

        public Game()
        {
            this.GameActive = false;
        }

        public void NewGame(SocketCommandContext Context)
        {
            this.GameActive = true;
            this.Level = 1;
            this.Width = 9;
            this.Height = 6;
            Grid = new Grid(Width, Height, Level);
            this.Context = Context;
            this.CurrentEmbed = null;
            SendGameEmbed();
        }

        public void StopGame()
        {
            this.GameActive = false;
        }

        public async Task MovePlayerAsync(Movement Direction)
        {
            if (!Grid.HasWon())
            {
                switch(Direction)
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
                    await SendGameEmbed();
            }

            if (Grid.HasWon())
            {
                GameActive = false;
                await SendWinEmbed();
            }
        }

        public async Task ContinueGame()
        {
            Level += 1;

            if (Width < 13)
                Width += 2;

            if (Height < 8)
                Height += 1;

            Grid = new Grid(Width, Height, Level);
            GameActive = true;
            CurrentEmbed = null;
            await SendGameEmbed();
        }

        public async Task SendWinEmbed()
        {
            String Prefix = Program.Settings.CommandPrefix;
            EmbedBuilder EmbedBuilder = new EmbedBuilder();
            EmbedBuilder.Title = "You win!";
            EmbedBuilder.Description = $"Type ``{Prefix}sokoban continue`` to continue to level {Level}" +
                $" or ``{Prefix}sokoban stop`` to quit.";
            Embed Embed = EmbedBuilder.Build();
            await Context.Channel.SendMessageAsync(null, false, Embed);
        }

        public async Task SendGameEmbed()
        {
            EmbedBuilder EmbedBuilder = new EmbedBuilder();
            EmbedBuilder.Title = $"Level {Level}";
            EmbedBuilder.Description = Grid.ToString();

            EmbedFooterBuilder EmbedFooterBuilder = new EmbedFooterBuilder();
            EmbedFooterBuilder.Text = "Type W, A, S or D to move or R to reset.";

            EmbedBuilder.Footer = EmbedFooterBuilder;
            Embed Embed = EmbedBuilder.Build();

            if (CurrentEmbed == null)
                CurrentEmbed = await Context.Channel.SendMessageAsync(null, false, Embed);
            else
                await CurrentEmbed.ModifyAsync(x =>
                {
                    x.Content = "";
                    x.Embed = new EmbedBuilder()
                        .WithTitle($"Level {Level}")
                        .WithDescription(Grid.ToString())
                        .WithFooter(EmbedFooterBuilder)
                        .Build();
                });
        }
    }
}
