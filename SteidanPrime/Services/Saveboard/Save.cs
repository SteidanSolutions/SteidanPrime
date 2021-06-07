using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Color = Discord.Color;

namespace SteidanPrime.Services.Saveboard
{
    public class Save : ModuleBase<SocketCommandContext>
    {
        private readonly SaveboardService _saveboardService;

        public Save(SaveboardService saveboardService)
        {
            _saveboardService = saveboardService;
        }

        [RequireUserPermission(GuildPermission.ManageMessages, Group = "Permissions")]
        [RequireOwner(Group = "Permissions")]
        [Remarks("Since the number of pinned messages per channel is limited, you can use this command to save any message you want to a channel designated using ``!savehere``. Use either message ID or message link with the command. Requires ``Manage Messages`` permission to use.")]
        [Summary("Saves a message to the designated channel.")]
        [Command("save")]
        public async Task SaveMessage([Summary("<message id> | <message link>")] string message)
        {
            if (!_saveboardService.SaveChannels.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync(
                    "Saveboard channel not set. Use ``!savehere`` to set the channel.");
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
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} the message link you provided is incorrect.");
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
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} the message Id you provided is incorrect.");
                    return;
                }

            if (msg == null)
            {
                await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} there were issues saving the message. Please try again.");
                return;
            }



            var embedBuilder = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.IconUrl = msg.Author.GetAvatarUrl();
                    author.Name = msg.Author.Username;
                })
                .WithFooter(footer =>
                    footer.Text = $"Saved by {Context.Message.Author}"
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
            await Context.Guild.GetTextChannel(_saveboardService.SaveChannels[Context.Guild.Id])
                .SendMessageAsync(null, false, embed);
        }

        [RequireUserPermission(GuildPermission.Administrator, Group = "Permissions")]
        [RequireOwner(Group = "Permissions")]
        [Remarks("Use to designate the channel which the bot will use to save messages. You should probably have a dedicated channel for it and let only the bot send messages there. Requires ``Administrator`` permission to use.")]
        [Summary("Designate the channel this message is typed in to be used as the saveboard.")]
        [Command("savehere")]
        public async Task SaveHere()
        {
            _saveboardService.SaveChannels[Context.Guild.Id] = Context.Channel.Id;
            await Context.Channel.SendMessageAsync($"Saveboard will now save messages in <#{Context.Channel.Id}>");
        }
    }
}
