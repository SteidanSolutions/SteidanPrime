using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SteidanPrime.Commands.Gambling.Blackjack;
using SteidanPrime.Models.Gambling;
using Game = SteidanPrime.Commands.Gambling.Blackjack.Game;

namespace SteidanPrime.Services.Gambling
{
    public class GamblingService : IGamblingService
    {
        private Dictionary<ulong, Player> _players { get; set; }
        public Dictionary<Player, Game> BlackjackGameDictionary { get; set; } = new Dictionary<Player, Game>();


        public GamblingService(DiscordSocketClient client)
        {
            client.ButtonExecuted += GamblingButtonHandler;
            DeserializePlayers();
        }

        public Dictionary<ulong, Player> GetPlayers()
        {
            return _players;
        }
        
        private async Task GamblingButtonHandler(SocketMessageComponent component)
        {
            if (component.User.Id != component.Message.Interaction.User.Id)
                return;

            if (!component.HasResponded)
            {
                var player = _players[component.User.Id];
                var gameOverButtons = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success, disabled: true)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary, disabled: true)
                    .WithButton("Double Down", "btnDoubleDown", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger, disabled: true)
                    .WithButton("Play again", "btnAgain", ButtonStyle.Success).Build();

                var gameOverButtonsNoMoney = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success, disabled: true)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary, disabled: true)
                    .WithButton("Double Down", "btnDoubleDown", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger, disabled: true)
                    .WithButton("Play again", "btnAgain", ButtonStyle.Success, disabled: true).Build();

                var newGameButtons = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary)
                    .WithButton("Double Down", "btnDoubleDown", ButtonStyle.Secondary)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger).Build();

                var playerHasHitButtons = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary)
                    .WithButton("Double Down", "btnDoubleDown", ButtonStyle.Secondary, disabled: true)
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
                            else x.Components = playerHasHitButtons;
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
                    case "btnDoubleDown":
                        var doubleDownEmbed = await PlayerDoubledDown(component.User.Id);
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = doubleDownEmbed.Item1;
                            if (player.VergilBucks < BlackjackGameDictionary[player].Bet) x.Components = gameOverButtonsNoMoney;
                            else if (doubleDownEmbed.Item2 != Result.NOTHING) x.Components = gameOverButtons;
                        });
                        break;
                    case "btnAgain":
                        var playAgainEmbed = await NewBlackjackGame(component.User.Id, BlackjackGameDictionary[_players[component.User.Id]].Bet);
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = playAgainEmbed.Item1;
                            if (player.VergilBucks < BlackjackGameDictionary[player].Bet) x.Components = gameOverButtonsNoMoney;
                            else if (player.VergilBucks < BlackjackGameDictionary[player].Bet * 2) x.Components =
                            playerHasHitButtons;
                            else x.Components = playAgainEmbed.Item2 != Result.NOTHING ? gameOverButtons : newGameButtons;
                        });
                        break;
                }
            }
        }

        public async Task<(Embed, Result)> NewBlackjackGame(ulong userId, double bet)
        {
            var player = _players[userId];
            if (player.CurrentlyPlayingBlackjack)
                if (BlackjackGameDictionary.ContainsKey(player))
                    return await BlackjackGameDictionary[player].GetGameEmbed();
            BlackjackGameDictionary[player] = new Game(bet, player);
            player.CurrentlyPlayingBlackjack = true;
            return await BlackjackGameDictionary[player].GetGameEmbed();
        }

        public async Task<(Embed, Result)> ForfeitBlackjackGame(ulong userId)
        {
            var embed = await BlackjackGameDictionary[_players[userId]].StopGameEmbed();
            _players[userId].CurrentlyPlayingBlackjack = false;
            return embed;
        }

        public async Task<(Embed, Result)> PlayerHit(ulong userId)
        {
            return await BlackjackGameDictionary[_players[userId]].PlayerHit();
        }
        private async Task<(Embed, Result)> PlayerStands(ulong userId)
        {
            return await BlackjackGameDictionary[_players[userId]].PlayerStands();
        }

        public async Task<(Embed, Result)> PlayerDoubledDown(ulong userId)
        {
            return await BlackjackGameDictionary[_players[userId]].PlayerDoubledDown();
        }

        public void SerializePlayers()
        {
            File.WriteAllText("Resources/wallet.json", JsonConvert.SerializeObject(_players));
        }

        public void DeserializePlayers()
        {
            //TODO change Newtonsoft.Json to System.Text.Json
            if (File.Exists("Resources/wallet.json"))
                _players = JsonConvert.DeserializeObject<Dictionary<ulong, Player>>(
                    File.ReadAllText("Resources/wallet.json"));
            else
                _players = new Dictionary<ulong, Player>();
        }
    }
}
