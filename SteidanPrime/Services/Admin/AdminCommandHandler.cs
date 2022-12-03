using Discord.Interactions;
using SteidanPrime.Services.Gambling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteidanPrime.Services.Admin
{
    [RequireOwner]
    [Group("admin", "Admin stuff")]
    public class AdminCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AdminService _adminService;
        public AdminCommandHandler(AdminService adminService)
        {
            _adminService = adminService;
        }

        [SlashCommand("servers", "Shows all the servers that the bot is in currently.")]
        public async Task JoinedServers()
        {
            await RespondAsync(await _adminService.GetAllServers());
        }

        [SlashCommand("message", "Send a custom message in a specific channel in a specific guild.")]
        public async Task Message(string guildId, string channelId, string message)
        {
            var guild = await _adminService.GetGuild(ulong.Parse(guildId));
            var channel = await _adminService.GetTextChannel(guild, ulong.Parse(channelId));
            await channel.SendMessageAsync(message);
            await RespondAsync("bla bla");
        }
    }
}
