using System.Threading.Tasks;
using Discord.Interactions;
using SteidanPrime.Services.Admin;

namespace SteidanPrime.Commands.Admin
{
    //TODO deal with how to set Owner properly
    [RequireOwner]
    [Group("admin", "Admin stuff")]
    public class AdminCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IAdminService _adminService;
        public AdminCommandHandler(IAdminService adminService)
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
