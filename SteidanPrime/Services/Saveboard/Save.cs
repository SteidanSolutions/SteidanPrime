using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Color = Discord.Color;

namespace SteidanPrime.Services.Saveboard
{
    public class Save : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private readonly SaveboardService _saveboardService;

        public Save(SaveboardService saveboardService)
        {
            _saveboardService = saveboardService;
        }

        [RequireUserPermission(GuildPermission.ManageMessages, Group = "Permissions")]
        [RequireOwner(Group = "Permissions")]
        [SlashCommand("save", "Use this command to save any message you want. Use either message ID or message link")]
        public async Task SaveMessage(string message)
        {
            if (!_saveboardService.SaveChannels.ContainsKey(Context.Guild.Id))
            {
                await RespondAsync("Saveboard channel not set. Use ``!savehere`` to set the channel.");
                return;
            }

            ulong messageId;
            IMessage msg;
            if (message.StartsWith("https://discord.com/channels/"))
                try
                {
                    messageId = ulong.Parse(message.Split('/').Last());
                    msg = await Context.Channel.GetMessageAsync(messageId);
                }
                catch
                {
                    await RespondAsync($"The message link you provided is incorrect.");
                    return;
                }
            else
                try
                {
                    messageId = ulong.Parse(message);
                    msg = await Context.Channel.GetMessageAsync(messageId);
                }
                catch
                {
                    await RespondAsync($"The message Id you provided is incorrect.");
                    return;
                }

            if (msg == null)
            {
                await RespondAsync($"There were issues saving the message. Please try again.");
                return;
            }

            var saveChannelId = _saveboardService.SaveChannels[Context.Guild.Id];
            await RespondAsync($"Message successfully saved in <#{saveChannelId}>");

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.IconUrl = msg.Author.GetAvatarUrl();
                    author.Name = msg.Author.Username;
                })
                .WithFooter(footer =>
                    footer.Text = $"Saved by {Context.Interaction.User}"
                )
                .WithTimestamp(msg.Timestamp)
                .WithDescription($"→ [original message]({msg.GetJumpUrl()}) in <#{msg.Channel.Id}>\n\n{msg.Content}\n\n")
                .WithColor((Color) ColorTranslator.FromHtml("#5684B4"));

            if (msg.Attachments.ToList().Count > 0)
            {
                embedBuilder.Description += $"📎[{msg.Attachments.ToList()[0].Filename}]({msg.Attachments.ToList()[0].Url})";
                embedBuilder.ImageUrl = msg.Attachments.ToList()[0].Url;
            }
            else if (msg.Embeds.ToList().Count > 0 && msg.Embeds.ToList()[0].Image.HasValue)
            {
                embedBuilder.ImageUrl = msg.Embeds.ToList()[0].Image.Value.Url;
            }

            var embed = embedBuilder.Build();
            await Context.Guild.GetTextChannel(saveChannelId)
                .SendMessageAsync(null, false, embed);
        }

        [RequireUserPermission(GuildPermission.Administrator, Group = "Permissions")]
        [RequireOwner(Group = "Permissions")]
        [SlashCommand("savehere", "Use to designate the channel which the bot will use to save messages.")]
        public async Task SaveHere()
        {
            _saveboardService.SaveChannels[Context.Guild.Id] = Context.Channel.Id;
            await RespondAsync($"Saveboard will now save messages in <#{Context.Channel.Id}>");
        }
    }
}
