using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using SteidanPrime.Models.Sokoban;
using Game = SteidanPrime.Commands.Sokoban.Game;

namespace SteidanPrime.Services.Sokoban
{
    public class SokobanService : ISokobanService
    {
        public Dictionary<ulong, Game> SokobanGameDictionary = new Dictionary<ulong, Game>();

        public SokobanService(DiscordSocketClient client)
        {
            client.ButtonExecuted += SokobanButtonHandler;
        }

        public async Task SokobanButtonHandler(SocketMessageComponent component)
        {
            if (component.User.Id != component.Message.Interaction.User.Id)
                return;

            if (!component.HasResponded)
            {
                var wonComponents = new ComponentBuilder()
                    .WithButton("          ", "btnEmptyL", ButtonStyle.Secondary, disabled: true)
                    .WithButton(null, "btnUp", ButtonStyle.Secondary, Emoji.Parse(":arrow_up:"), disabled: true)
                    .WithButton("          ", "btnEmptyR", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Reset", "btnReset", ButtonStyle.Primary, disabled: true)
                    .WithButton("Stop", "btnStop", ButtonStyle.Danger)
                    .WithButton(null, "btnLeft", ButtonStyle.Secondary, Emoji.Parse(":arrow_left:"), disabled: true, row: 1)
                    .WithButton(null, "btnDown", ButtonStyle.Secondary, Emoji.Parse(":arrow_down:"), disabled: true, row: 1)
                    .WithButton(null, "btnRight", ButtonStyle.Secondary, Emoji.Parse(":arrow_right:"), disabled: true, row: 1)
                    .WithButton("Next level", "btnNextLevel", ButtonStyle.Success, row: 1).Build();

                switch (component.Data.CustomId)
                {
                    case "btnUp":
                        var embedResultUp = SokobanGameDictionary[component.User.Id].MovePlayerAsync(Movement.UP).Result;
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = embedResultUp.Item1;
                            if (embedResultUp.Item2) x.Components = wonComponents;
                        });
                        break;
                    case "btnLeft":
                        var embedResultLeft = SokobanGameDictionary[component.User.Id].MovePlayerAsync(Movement.LEFT).Result;
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = embedResultLeft.Item1;
                            if (embedResultLeft.Item2) x.Components = wonComponents;
                        });
                        break;
                    case "btnDown":
                        var embedResultDown = SokobanGameDictionary[component.User.Id].MovePlayerAsync(Movement.DOWN).Result;
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = embedResultDown.Item1;
                            if (embedResultDown.Item2) x.Components = wonComponents;
                        });
                        break;
                    case "btnRight":
                        var embedResultRight = SokobanGameDictionary[component.User.Id].MovePlayerAsync(Movement.RIGHT).Result;
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = embedResultRight.Item1;
                            if (embedResultRight.Item2) x.Components = wonComponents;
                        });
                        break;
                    case "btnReset":
                        var embedResultReset = SokobanGameDictionary[component.User.Id].MovePlayerAsync(Movement.RESET).Result;
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = embedResultReset.Item1;
                            if (embedResultReset.Item2) x.Components = wonComponents;
                        });
                        break;
                    case "btnNextLevel":
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = ContinueGame(component.User.Id).Result.Item1;
                            x.Components = new ComponentBuilder().WithButton("          ", "btnEmptyL",
                                    ButtonStyle.Secondary, disabled: true)
                                .WithButton(null, "btnUp", ButtonStyle.Secondary, Emoji.Parse(":arrow_up:"))
                                .WithButton("          ", "btnEmptyR", ButtonStyle.Secondary, disabled: true)
                                .WithButton("Reset", "btnReset", ButtonStyle.Primary)
                                .WithButton("Stop", "btnStop", ButtonStyle.Danger)
                                .WithButton(null, "btnLeft", ButtonStyle.Secondary, Emoji.Parse(":arrow_left:"), row: 1)
                                .WithButton(null, "btnDown", ButtonStyle.Secondary, Emoji.Parse(":arrow_down:"), row: 1)
                                .WithButton(null, "btnRight", ButtonStyle.Secondary, Emoji.Parse(":arrow_right:"),
                                    row: 1)
                                .WithButton("Next level", "btnNextLevel", ButtonStyle.Success, disabled: true, row: 1)
                                .Build();
                        });
                        break;
                    case "btnStop":
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = StopGame(component.User.Id).Result;
                            x.Components = new ComponentBuilder()
                                .WithButton("          ", "btnEmptyL", ButtonStyle.Secondary, disabled: true)
                                .WithButton(null, "btnUp", ButtonStyle.Secondary, Emoji.Parse(":arrow_up:"), disabled: true)
                                .WithButton("          ", "btnEmptyR", ButtonStyle.Secondary, disabled: true)
                                .WithButton("Reset", "btnReset", ButtonStyle.Primary, disabled: true)
                                .WithButton("Stop", "btnStop", ButtonStyle.Danger, disabled: true)
                                .WithButton(null, "btnLeft", ButtonStyle.Secondary, Emoji.Parse(":arrow_left:"), disabled: true, row: 1)
                                .WithButton(null, "btnDown", ButtonStyle.Secondary, Emoji.Parse(":arrow_down:"), disabled: true, row: 1)
                                .WithButton(null, "btnRight", ButtonStyle.Secondary, Emoji.Parse(":arrow_right:"), disabled: true, row: 1)
                                .WithButton("Next level", "btnNextLevel", ButtonStyle.Success, disabled: true, row: 1).Build();
                        });
                        break;
                }
            }
        }

        public async Task<(Embed, bool)> NewGameAsync(ulong userId)
        {
            SokobanGameDictionary[userId] = new Game();
            return await GetGameEmbed(userId);
        }

        public async Task<Embed> StopGame(ulong userId)
        {
            var embed = await SokobanGameDictionary[userId].StopGameEmbed();
            SokobanGameDictionary.Remove(userId);
            return embed;
        }

        public async Task<(Embed, bool)> ContinueGame(ulong userId)
        {
            return await SokobanGameDictionary[userId].ContinueGame();
        }

        public async Task<(Embed, bool)> GetGameEmbed(ulong userId)
        {
            return await SokobanGameDictionary[userId].GetGameEmbed();
        }
    }
}
