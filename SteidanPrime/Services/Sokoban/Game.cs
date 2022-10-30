using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using static System.Net.Mime.MediaTypeNames;

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

        public Task<Embed> StopGameEmbed()
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = $"LOSER",
                Description = Grid.ToString()
            };
            var embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = "YOU GAVE UP LMAO"
            };

            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return Task.FromResult(embed);
        }

        public Task<(Embed, bool)> GetGameEmbed()
        {
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;

            embedBuilder = new EmbedBuilder
            { 
                Title = $"Level {Level}",
                Description = Grid.ToString()
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = "Use buttons below to play."
            };

            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return Task.FromResult((embed, Grid.HasWon()));
        }

        public async Task<(Embed, bool)> ContinueGame()
        {
            Level += 1;

            if (Width < 13)
                Width += 2;

            if (Height < 8)
                Height += 1;

            Grid = new Grid(Width, Height, Level);
            CurrentEmbed = null;
            return await GetGameEmbed();
        }

        public async Task<(Embed, bool)> MovePlayerAsync(Movement direction)
        {
            if (Grid.HasWon()) return await GetGameEmbed();
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

            return await GetGameEmbed();
        }
    }
}
