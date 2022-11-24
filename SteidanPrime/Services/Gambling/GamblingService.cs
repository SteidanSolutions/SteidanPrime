using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SteidanPrime.Services.Gambling.Blackjack;
using Game = SteidanPrime.Services.Gambling.Blackjack.Game;

namespace SteidanPrime.Services.Gambling
{
    public class GamblingService
    {
        public Dictionary<ulong, Player> Players { get; set; }
        public Dictionary<Player, Game> BlackjackGameDictionary { get; set; } = new Dictionary<Player, Game>();


        public GamblingService(DiscordSocketClient client)
        {
            client.ButtonExecuted += GamblingButtonHandler;
            DeserializePlayers();
        }

        private async Task GamblingButtonHandler(SocketMessageComponent component)
        {
            if (component.User.Id != component.Message.Interaction.User.Id)
                return;

            if (!component.HasResponded)
            {
                var player = Players[component.User.Id];
                var gameOverButtons = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success, disabled: true)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary, disabled: true)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger, disabled: true)
                    .WithButton("Play again", "btnAgain", ButtonStyle.Success).Build();

                var gameOverButtonsNoMoney = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success, disabled: true)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary, disabled: true)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger, disabled: true)
                    .WithButton("Play again", "btnAgain", ButtonStyle.Success, disabled: true).Build();

                var newGameButtons = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger).Build();

                switch (component.Data.CustomId)
                {
                    case "btnHit":
                        var hitEmbed = await PlayerHit(component.User.Id);
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = hitEmbed.Item1;
                            if (player.VergilBucks < BlackjackGameDictionary[player].Bet) x.Components = gameOverButtonsNoMoney;
                            else if (hitEmbed.Item2 != Result.NOTHING) x.Components = gameOverButtons;
                        });
                        break;
                    case "btnStand":
                        var standEmbed = await PlayerStands(component.User.Id);
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = standEmbed.Item1;
                            if (player.VergilBucks < BlackjackGameDictionary[player].Bet) x.Components = gameOverButtonsNoMoney;
                            else if (standEmbed.Item2 != Result.NOTHING) x.Components = gameOverButtons;
                        });
                        break;
                    case "btnForfeit":
                        player.BlackjackLosses++;
                        player.CurrentlyPlayingBlackjack = false;
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = ForfeitBlackjackGame(component.User.Id).Result.Item1;
                            x.Components = player.VergilBucks < BlackjackGameDictionary[player].Bet ? gameOverButtonsNoMoney : gameOverButtons;
                        });
                        break;
                    case "btnAgain":
                        var playAgainEmbed = await NewBlackjackGame(component.User.Id, BlackjackGameDictionary[Players[component.User.Id]].Bet);
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = playAgainEmbed.Item1;
                            if (player.VergilBucks < BlackjackGameDictionary[player].Bet) x.Components = gameOverButtonsNoMoney;
                            x.Components = playAgainEmbed.Item2 != Result.NOTHING ? gameOverButtons : newGameButtons;
                        });
                        break;
                }
            }
        }

        public async Task<(Embed, Result)> NewBlackjackGame(ulong userId, double bet)
        {
            var player = Players[userId];
            if (player.CurrentlyPlayingBlackjack)
                return await BlackjackGameDictionary[player].GetGameEmbed();
            BlackjackGameDictionary[player] = new Game(bet, player);
            player.CurrentlyPlayingBlackjack = true;
            return await BlackjackGameDictionary[player].GetGameEmbed();
        }

        public async Task<(Embed, Result)> ForfeitBlackjackGame(ulong userId)
        {
            var embed = await BlackjackGameDictionary[Players[userId]].StopGameEmbed();
            Players[userId].CurrentlyPlayingBlackjack = false;
            return embed;
        }

        public async Task<(Embed, Result)> PlayerHit(ulong userId)
        {
            return await BlackjackGameDictionary[Players[userId]].PlayerHit();
        }
        private async Task<(Embed, Result)> PlayerStands(ulong userId)
        {
            return await BlackjackGameDictionary[Players[userId]].PlayerStands();
        }

        public void SerializePlayers()
        {
            File.WriteAllText("Resources/wallet.json", JsonConvert.SerializeObject(Players));
        }

        public void DeserializePlayers()
        {
            if (File.Exists("Resources/wallet.json"))
                Players = JsonConvert.DeserializeObject<Dictionary<ulong, Player>>(
                    File.ReadAllText("Resources/wallet.json"));
            else
                Players = new Dictionary<ulong, Player>();
        }


    }
}
