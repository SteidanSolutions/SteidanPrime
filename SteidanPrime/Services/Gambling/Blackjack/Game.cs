using System;
using Discord;
using Discord.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteidanPrime.Services.Gambling.Blackjack
{
    public class Game
    {
        public Player Player { get; set; }
        public Grid Grid { get; set; }
        public RestUserMessage CurrentEmbed { get; set; }
        public bool PlayerBlackjack { get; set; } = false;
        public bool DealerBlackjack { get; set; } = false;
        public bool InitialDrawOver { get; set; } = false;
        public bool PlayerStood { get; set; } = false;
        public bool DealerDoneDrawing { get; set; } = false;
        public double Bet { get; set; }



        public Game(double bet, Player player)
        {
            Player = player;
            Grid = new Grid();
            Bet = bet;
            Player.VergilBucks -= bet;
            CurrentEmbed = null;
        }

        public Task<(Embed, Result)> PlayerHit()
        {
            Grid.PlayerCards.Add(Grid.Deck.DrawCard());
            return GetGameEmbed();
        }
        public Task<(Embed, Result)> PlayerStands()
        {
            Grid.RevealFaceDownCard = true;
            PlayerStood = true;
            DrawDealerCards();
            return GetGameEmbed();
        }
        public Result CheckGameResult()
        {
            var tempDealerCards = Grid.DealerCards.ToList();
            var tempPlayerCards = Grid.PlayerCards.ToList();

            tempDealerCards = ConvertFacesToTens(tempDealerCards);
            tempPlayerCards = ConvertFacesToTens(tempPlayerCards);

            if (CheckIfBust(tempPlayerCards))
                return Result.PLAYER_BUST;
            if (CheckIfBust(tempDealerCards))
                return Result.DEALER_BUST;

            if (!InitialDrawOver)
            {
                PlayerBlackjack = Grid.PlayerCards.Contains(11) &&
                                  (Grid.PlayerCards.Contains(10) || Grid.PlayerCards.Contains(12) || Grid.PlayerCards.Contains(13) || Grid.PlayerCards.Contains(14));
                DealerBlackjack = Grid.DealerCards.Contains(11) &&
                                  (Grid.DealerCards.Contains(10) || Grid.DealerCards.Contains(12) || Grid.DealerCards.Contains(13) || Grid.DealerCards.Contains(14));

                if (PlayerBlackjack && DealerBlackjack)
                {
                    Grid.RevealFaceDownCard = true;
                    return Result.STAND_OFF;
                }
                if (PlayerBlackjack)
                {
                    Grid.RevealFaceDownCard = true;
                    return Result.PLAYER_BLACKJACK;
                }
                if (DealerBlackjack)
                {
                    Grid.RevealFaceDownCard = true;
                    return Result.DEALER_BLACKJACK;
                }

                InitialDrawOver = true;
            }


            if (DealerDoneDrawing && PlayerStood)
            {
                Grid.RevealFaceDownCard = true;
                if (tempDealerCards.Sum() > tempPlayerCards.Sum())
                    return Result.DEALER_WON;
                if (tempDealerCards.Sum() < tempPlayerCards.Sum())
                    return Result.PLAYER_WON;
                if (tempDealerCards.Sum() == tempPlayerCards.Sum())
                    return Result.PUSH;
            }

            return Result.NOTHING;
        }

        public bool CheckIfBust(List<int> cards)
        {
            cards = ConvertFacesToTens(cards);
            if (cards.Contains(11) && cards.Sum() > 21)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    if (cards[i] != 11) continue;
                    cards[i] = 1;
                    CheckIfBust(cards);
                }
            }
            return cards.Sum() > 21;
        }

        public void DrawDealerCards()
        {
            var tempDealerCards = Grid.DealerCards.ToList();
            tempDealerCards = ConvertFacesToTens(tempDealerCards);

            if (tempDealerCards.Sum() < 17)
            {
                Grid.DealerCards.Add(Grid.Deck.DrawCard());
                DrawDealerCards();
            }

            if (tempDealerCards.Sum() > 21 && tempDealerCards.Contains(11))
            {
                for (int i = 0; i < tempDealerCards.Count; i++)
                {
                    if (tempDealerCards[i] == 11)
                    {
                        tempDealerCards[i] = 1;
                    }
                }

                if (tempDealerCards.Sum() < 17)
                {
                    Grid.DealerCards.Add(Grid.Deck.DrawCard());
                    DrawDealerCards();
                }
            }

            DealerDoneDrawing = true;
        }

        private static List<int> ConvertFacesToTens(List<int> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                switch (cards[i])
                {
                    case 12:
                    case 13:
                    case 14:
                        cards[i] = 10;
                        break;
                }

            }

            return cards;
        }

        public async Task<(Embed, Result)> GetGameEmbed()
        {
            var tempDealerCards = Grid.DealerCards.ToList();
            var tempPlayerCards = Grid.PlayerCards.ToList();

            tempDealerCards = ConvertFacesToTens(tempDealerCards);
            tempPlayerCards = ConvertFacesToTens(tempPlayerCards);

            var i = 0;
            while (tempPlayerCards.Sum() > 21 && tempPlayerCards.Contains(11) && (i < tempPlayerCards.Count))
            {
                if (tempPlayerCards[i] == 11)
                    tempPlayerCards[i] = 1;
                i++;
            }

            var embedBuilder = new EmbedBuilder();
            if (DealerDoneDrawing)
                embedBuilder.Description = $"``          DEALER: {tempDealerCards.Sum()}          ``\n" 
                                           + Grid +
                                           $"``          PLAYER: {tempPlayerCards.Sum()}          ``";
            else
                embedBuilder.Description = $"``            DEALER            ``\n" 
                                           + Grid +
                                           $"``          PLAYER: {tempPlayerCards.Sum()}          ``";

            var embedFooterBuilder = new EmbedFooterBuilder();
            var result = CheckGameResult();

            switch (result)
            {
                case Result.PLAYER_WON:
                    Player.BlackjackWins++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    Player.VergilBucks += (2 * Bet);
                    embedBuilder.Title = "PLAYER WON!";
                    embedFooterBuilder.Text = $"YOU WON {2 * Bet} VERGIL BUCKS!";
                    break;
                case Result.DEALER_WON:
                    Player.BlackjackLosses++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    embedBuilder.Title = "DEALER WON!";
                    embedFooterBuilder.Text = $"YOU LOST {Bet} VERGIL BUCKS!";
                    break;
                case Result.DEALER_BUST:
                    Player.BlackjackWins++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    Player.VergilBucks += (2 * Bet);
                    embedBuilder.Title = "DEALER BUST!";
                    embedFooterBuilder.Text = $"YOU WON {2 * Bet} VERGIL BUCKS!";
                    break;
                case Result.PLAYER_BUST:
                    Player.BlackjackLosses++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    embedBuilder.Title = "PLAYER BUST!";
                    embedFooterBuilder.Text = $"YOU LOST {Bet} VERGIL BUCKS!";
                    break;
                case Result.PUSH:
                    Player.BlackjackPushes++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    Player.VergilBucks += Bet;
                    embedBuilder.Title = "PUSH!";
                    embedFooterBuilder.Text = $"Your bet of {Bet} has been returned to you.";
                    break;
                case Result.PLAYER_BLACKJACK:
                    Player.Blackjacks++;
                    Player.BlackjackWins++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    Player.VergilBucks += (2.5 * Bet);
                    embedBuilder.Title = "PLAYER HAS THE BLACKJACK!";
                    embedFooterBuilder.Text = $"YOU WON {2.5 * Bet} VERGIL BUCKS!";
                    break;
                case Result.DEALER_BLACKJACK:
                    Player.BlackjackLosses++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    embedBuilder.Title = "DEALER HAS THE BLACKJACK!";
                    embedFooterBuilder.Text = $"YOU LOST {Bet} VERGIL BUCKS!";
                    break;
                case Result.STAND_OFF:
                    Player.BlackjackStandOffs++;
                    Player.CurrentlyPlayingBlackjack = false;
                    Grid.RevealFaceDownCard = true;
                    Player.VergilBucks += Bet;
                    embedBuilder.Title = "STAND-OFF!";
                    embedFooterBuilder.Text = $"Your bet of {Bet} has been returned to you.";
                    break;
                case Result.NOTHING:
                    embedBuilder.Title = $"Bet: {Bet} VBucks";
                    embedFooterBuilder.Text = "Use buttons below to play.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            embedFooterBuilder.Text += $"\nYour balance: {Player.VergilBucks}";
            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, result));
        }

        public async Task<(Embed, Result)> StopGameEmbed()
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = $"YOU FORFEIT",
                Description = Grid.ToString()
            };
            var embedFooterBuilder = new EmbedFooterBuilder
            {
                Text = $"YOU LOST {Bet} VERGIL BUCKS!"
            };

            embedBuilder.Footer = embedFooterBuilder;
            var embed = embedBuilder.Build();
            return await Task.FromResult((embed, Result.FORFEIT));
        }

    }
}
