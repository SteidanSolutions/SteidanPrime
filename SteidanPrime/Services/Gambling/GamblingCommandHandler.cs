﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using SteidanPrime.Services.Gambling.Blackjack;

namespace SteidanPrime.Services.Gambling
{
    [Group("gambling", "Gambling stuff.")]
    public class GamblingCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly int _paydayCooldownMilliseconds = 3600000;
        private readonly int _vergilBucksPerPayday = 200;
        private readonly GamblingService _gamblingService;

        public GamblingCommandHandler(GamblingService gamblingService)
        {
            _gamblingService = gamblingService;
        }

        [SlashCommand("payday", "Gives you 200 Vbucks (Vergil bucks). Can be used once every hour.")]
        public async Task Payday()
        {
            if (!_gamblingService.Players.ContainsKey(Context.User.Id))
            {
                await RespondAsync("You don't have a bank account open yet! Type ``/gambling register`` to open one.", ephemeral: true);
                return;
            }

            var player = _gamblingService.Players[Context.User.Id];
            var currentTimeInMilliseconds = Context.Interaction.CreatedAt.ToUnixTimeMilliseconds();
            if ((currentTimeInMilliseconds - _paydayCooldownMilliseconds) >= player.LastPaydayTime)
            {
                player.VergilBucks += _vergilBucksPerPayday;
                player.LastPaydayTime = currentTimeInMilliseconds;
                await RespondAsync(
                    $"You got paid {_vergilBucksPerPayday} Vbucks (Vergil bucks)! Your current balance is {player.VergilBucks}.", ephemeral: true);
            }
            else
            {
                await RespondAsync(
                    $"It's too early for your payday! You can get paid in ``{Math.Abs(currentTimeInMilliseconds - player.LastPaydayTime - _paydayCooldownMilliseconds)/60000}`` minutes and ``{Math.Abs((currentTimeInMilliseconds - player.LastPaydayTime - _paydayCooldownMilliseconds) /1000) % 60}`` seconds.", ephemeral: true);
            }
        }

        [SlashCommand("balance", "Check your current Vbucks (Vergil bucks) balance.")]
        public async Task Balance()
        {
            if (_gamblingService.Players.ContainsKey(Context.User.Id))
            {
                await RespondAsync($"Your current balance of Vbucks (Vergil bucks) is ``{_gamblingService.Players[Context.User.Id].VergilBucks}``");
                return;
            }
            await RespondAsync("You don't have a bank account open yet! Type ``/gambling register`` to open one.", ephemeral: true);
        }

        [SlashCommand("register",
            "Open a bank account and start getting Vbucks (Vergil bucks) to gamble with. Don't get it twisted.")]
        public async Task Register()
        {
            if (!_gamblingService.Players.ContainsKey(Context.User.Id))
            {
                var player = new Player
                {
                    Id = Context.User.Id
                };
                _gamblingService.Players.Add(Context.User.Id, player);
                await RespondAsync(
                    $"You have successfully opened an account with Devil May Bank! Explore our options (slash commands) to receive and use Vbucks (Vergil bucks).", ephemeral: true);
            }
            else
            {
                await RespondAsync("You already have an account with our bank!", ephemeral: true);
            }
        }

        [SlashCommand("blackjack", "Begins a new or resumes the previous game of blackjack.")]
        public async Task Blackjack([MinValue(0)] double bet)
        {
            var player = _gamblingService.Players[Context.User.Id];

            if (!_gamblingService.Players.ContainsKey(Context.User.Id))
            {
                await RespondAsync(
                    $"You must register using the ``/gambling register`` command before you can gamble.", ephemeral: true);
            }
            else
            {
                if (bet > player.VergilBucks)
                {
                    await RespondAsync(
                        $"You cannot bet ``{bet}`` Vergil bucks as you only have ``{player.VergilBucks}``. Place a lower bet or get a loan.", ephemeral: true);
                    return;
                }
                var gameOverButtons = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success, disabled: true)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary, disabled: true)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger, disabled: true)
                    .WithButton("Play again", "btnAgain", ButtonStyle.Success).Build();

                var newGameButtons = new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger).Build();

                await DeferAsync();
                var embed = _gamblingService.NewBlackjackGame(Context.User.Id, bet).Result;
                if (embed.Item2 != Result.NOTHING)
                    await FollowupAsync(embed: embed.Item1,
                        components: gameOverButtons);
                else
                    await FollowupAsync(embed: embed.Item1,
                        components: newGameButtons);
            }
        }

        [SlashCommand("stats", "Gets your stats for gambling.")]
        public async Task Stats()
        {
            var player = _gamblingService.Players[Context.User.Id];
            await RespondAsync($"```Your stats for gambling are:\n" +
                               $"Blackjacks: {player.Blackjacks}\n" +
                               $"Blackjack Wins: {player.BlackjackWins}\n" +
                               $"Blackjack Losses: {player.BlackjackLosses}\n" +
                               $"Blackjack Stand-offs: {player.BlackjackStandOffs}\n" +
                               $"Blackjack Pushes: {player.BlackjackPushes}```");
        }
    }
}
