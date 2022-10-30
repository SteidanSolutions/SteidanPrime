using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;

namespace SteidanPrime.Services.Gambling
{
    [Group("gambling", "Gambling stuff.")]
    public class GamblingCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        //private readonly int PaydayCooldown = 3600000;
        private readonly int PaydayCooldown = 5000;
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
                await RespondAsync("You don't have a bank account open yet! Type ``/gambling register`` to open one.");
                return;
            }

            var player = _gamblingService.Players[Context.User.Id];
            if (player.TimeUntilPayday.ElapsedMilliseconds >= PaydayCooldown)
            {
                player.VergilBucks += VergilBucksPerPayday;
                player.TimeUntilPayday.Restart();
                await RespondAsync(
                    $"You got paid {VergilBucksPerPayday} Vbucks (Vergil bucks)! Your current balance is {player.VergilBucks}.");
            }
            else
            {
                await RespondAsync(
                    $"It's too early for your payday! You can get paid in ``{(PaydayCooldown - player.TimeUntilPayday.ElapsedMilliseconds) / 60000}`` minutes and ``{((PaydayCooldown - player.TimeUntilPayday.ElapsedMilliseconds) / 1000) % 60}`` seconds.");
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
            await RespondAsync("You don't have a bank account open yet! Type ``/gambling register`` to open one.");
        }

        [SlashCommand("register",
            "Open a bank account and start getting Vbucks (Vergil bucks) to gamble with. Don't get it twisted.")]
        public async Task Register()
        {
            if (!_gamblingService.Players.ContainsKey(Context.User.Id))
            {
                _gamblingService.Players.Add(Context.User.Id, new Player());
                await RespondAsync(
                    $"You have successfully opened an account with Devil May Bank! Explore our options (slash commands) to receive and use Vbucks (Vergil bucks).");
            }
            else
            {
                await RespondAsync("You already have an account with our bank!");
            }
        }
    }
}
