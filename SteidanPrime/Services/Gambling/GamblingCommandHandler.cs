﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace SteidanPrime.Services.Gambling
{
    [Group("gambling", "Gambling stuff.")]
    public class GamblingCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly int _paydayCooldownMilliseconds = 3600000;
        private readonly int VergilBucksPerPayday = 200;
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
                player.VergilBucks += VergilBucksPerPayday;
                player.LastPaydayTime = currentTimeInMilliseconds;
                await RespondAsync(
                    $"You got paid {VergilBucksPerPayday} Vbucks (Vergil bucks)! Your current balance is {player.VergilBucks}.", ephemeral: true);
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
                _gamblingService.Players.Add(Context.User.Id, new Player());
                await RespondAsync(
                    $"You have successfully opened an account with Devil May Bank! Explore our options (slash commands) to receive and use Vbucks (Vergil bucks).", ephemeral: true);
            }
            else
            {
                await RespondAsync("You already have an account with our bank!", ephemeral: true);
            }
        }

        [SlashCommand("blackjack", "Begins a new or resumes the previous game of blackjack.")]
        public async Task Blackjack()
        {
            var embed = new EmbedBuilder()
            {
                Title = "Blackjack",
                Description = "```\n        DEALER        \n```" + ":black_large_square::black_large_square::regional_indicator_a::black_large_square::five::black_large_square::regional_indicator_j::black_large_square::black_large_square::black_large_square:\n" +
                              ":black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square:\n" +
                              ":black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square:\n" +
                              ":black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square::black_large_square:\n" +
                              ":black_large_square::black_large_square::three::black_large_square::regional_indicator_a::black_large_square::four::black_large_square::black_large_square::black_large_square:" +
                              "```\n         YOU        \n```"
            };
            await RespondAsync(embed: embed.Build(),
                components: new ComponentBuilder().WithButton("Hit", "btnHit", ButtonStyle.Success)
                    .WithButton("Stand", "btnStand", ButtonStyle.Primary)
                    .WithButton("Split", "btnSplit", ButtonStyle.Secondary)
                    .WithButton("Forfeit", "btnForfeit", ButtonStyle.Danger).Build());
        }
    }
}