using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime.Services.Gambling.Blackjack
{
    public class Game
    {
        public Player Player { get; set; }
        public Grid Grid { get; set; }
        public RestUserMessage CurrentEmbed { get; set; }
        public double Bet { get; set; }
        

        public Game(double bet, Player player)
        {
            Player = player;
            Grid = new Grid();
            Bet = bet;
            Player.VergilBucks -= bet;
            CurrentEmbed = null;
        }

        public async Task<(Embed, bool)> GetInitialEmbed()
        {
            var result = Grid.CheckIfBlackJack();
            return result switch
            {
                Result.STAND_OFF => await StandOffEmbed(),
                Result.PLAYER_BLACKJACK => await BlackjackEmbed(result),
                Result.DEALER_BLACKJACK => await BlackjackEmbed(result),
                _ => await GetGameEmbed()
            };
        }

        public async Task<(Embed, bool)> PlayerHit()
        {
            Grid.PlayerCards.Add(Grid.Deck.DrawCard());
            var tempPlayerCards = Grid.PlayerCards;
            for (int i = 0; i < tempPlayerCards.Count; i++)
            {
                if (tempPlayerCards[i] > 10)
                    tempPlayerCards[i] = 10;
            }
            if (tempPlayerCards.Sum() > 21)
                return await PlayerBustEmbed();
            return await GetGameEmbed();
        }

        public async Task<(Embed, bool)> PlayerStands()
        {
            Grid.RevealFaceDownCard = true;
            if (Grid.DealerCards.Sum() > 17)
            {
                return await CheckDealerCards();
            }

            Grid.DealerCards.Add(Grid.Deck.DrawCard());
            return await CheckDealerCards();
        }

        public async Task<(Embed, bool)> CheckDealerCards()
        {
            var tempDealerCards = Grid.DealerCards;
            var tempPlayerCards = Grid.PlayerCards;
            for (int i = 0; i < tempDealerCards.Count; i++)
            {
                if (tempDealerCards[i] > 10)
                    tempDealerCards[i] = 10;
            }

            for (int i = 0; i < tempPlayerCards.Count; i++)
            {
                if (tempPlayerCards[i] > 10)
                    tempPlayerCards[i] = 10;
            }

            if (tempDealerCards.Sum() <= 17)
            {
                for (int i = 0; i < tempDealerCards.Count; i++)
                {
                    if (tempDealerCards[i] == 1)
                        tempDealerCards[i] = 11;
                }

                if (tempDealerCards.Sum() >= 17)
                    await CheckDealerCards();
                else
                {
                    Grid.DealerCards.Add(Grid.Deck.DrawCard());
                    await CheckDealerCards();
                }
            }
            if (tempDealerCards.Sum() > 21)
                return await DealerBustEmbed();
            if (tempDealerCards.Sum() == tempPlayerCards.Sum())
            {
                return await PushEmbed();
            }
            if (tempDealerCards.Sum() > tempPlayerCards.Sum())
                return await DealerWonEmbed();
            return await PlayerWonEmbed();
        }

        private async Task<(Embed, bool)> PlayerWonEmbed()
        {
            Grid.RevealFaceDownCard = true;
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;
            var winnings = 2 * Bet;

            embedBuilder = new EmbedBuilder
            {
                Title = $"PLAYER WON!",
                Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = $"YOU WON {winnings} VERGIL BUCKS!"
            };

            Player.BlackjackWins++;
            Player.CurrentlyPlayingBlackjack = false;
            Player.VergilBucks += winnings;
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, true));
        }

        private async Task<(Embed, bool)> DealerWonEmbed()
        {
            Grid.RevealFaceDownCard = true;
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;

            embedBuilder = new EmbedBuilder
            {
                Title = $"DEALER WON!",
                Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = $"YOU LOST {Bet} VERGIL BUCKS!"
            };

            Player.BlackjackLosses++;
            Player.CurrentlyPlayingBlackjack = false;
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, true));
        }

        private async Task<(Embed, bool)> DealerBustEmbed()
        {
            Grid.RevealFaceDownCard = true;
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;
            var winnings = 2 * Bet;

            embedBuilder = new EmbedBuilder
            {
                Title = $"DEALER BUST!",
                Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = $"YOU WON {winnings} VERGIL BUCKS!"
            };

            Player.BlackjackWins++;
            Player.CurrentlyPlayingBlackjack = false;
            Player.VergilBucks += winnings;
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, true));
        }

        private async Task<(Embed, bool)> PushEmbed()
        {
            Grid.RevealFaceDownCard = true;
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;

            embedBuilder = new EmbedBuilder
            {
                Title = $"PUSH!",
                Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = $"Your bet of {Bet} has been returned to you."
            };

            Player.BlackjackPushes++;
            Player.CurrentlyPlayingBlackjack = false;
            Player.VergilBucks += Bet;
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, true));
        }

        public async Task<(Embed, bool)> PlayerBustEmbed()
        {
            Grid.RevealFaceDownCard = true;
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;

            embedBuilder = new EmbedBuilder
            {
                Title = $"PLAYER BUST!",
                Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = $"YOU LOST {Bet} VERGIL BUCKS!"
            };

            Player.BlackjackLosses++;
            Player.CurrentlyPlayingBlackjack = false;
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, true));
        }

        private async Task<(Embed, bool)> StandOffEmbed()
        {
            Grid.RevealFaceDownCard = true;
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;

            embedBuilder = new EmbedBuilder
            {
                Title = $"STAND-OFF!",
                Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = $"Your bet of {Bet} has been returned to you."
            };

            Player.BlackjackStandOffs++;
            Player.CurrentlyPlayingBlackjack = false;
            Player.VergilBucks += Bet;
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, true));
        }

        public async Task<(Embed, bool)> BlackjackEmbed(Result result)
        {
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;

            if (result == Result.PLAYER_BLACKJACK)
            {
                var winnings = 2.5 * Bet;
                embedBuilder = new EmbedBuilder
                {
                    Title = "PLAYER HAS THE BLACKJACK!",
                    Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
                };
                Player.Blackjacks++;
                Player.BlackjackWins++;
                Player.VergilBucks += winnings;
                embedFooterBuilder = new EmbedFooterBuilder
                {
                    Text = $"YOU WON {winnings} VERGIL BUCKS!"
                };
            }
            else
            {
                embedBuilder = new EmbedBuilder
                {
                    Title = "DEALER HAS THE BLACKJACK!",
                    Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
                };
                Player.BlackjackLosses++;
                embedFooterBuilder = new EmbedFooterBuilder
                {
                    Text = $"YOU LOST {Bet} VERGIL BUCKS!"
                };
            }

            Player.CurrentlyPlayingBlackjack = false;
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, true));
        }

        public async Task<(Embed, bool)> GetGameEmbed()
        {
            EmbedBuilder embedBuilder;
            EmbedFooterBuilder embedFooterBuilder;

            embedBuilder = new EmbedBuilder
            {
                Title = $"Bet:  {Bet} VBucks",
                Description = "``            DEALER            ``\n" + Grid + "``            PLAYER            ``"
            };
            embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = "Use buttons below to play."
            };

            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, false));
        }

        public async Task<(Embed, bool)> StopGameEmbed()
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
            return await Task.FromResult((embed, true));
        }

    }
}
