using Discord;
using wow2.Bot.Verbose.Messages;

namespace wow2.Bot.Modules.Timers
{
    public class TimerStartedMessage : InteractiveMessage
    {
        protected override ActionButton[] ActionButtons => new[]
        {
            new ActionButton()
            {
                Label = "Get notified for this timer",
                Style = ButtonStyle.Primary,
                Action = async component =>
                {
                    if (Timer.NotifyUserMentions.Remove(component.User.Mention))
                    {
                        await component.FollowupAsync(
                            embed: new SuccessMessage("You'll no longer be notified about this timer.").Embed,
                            ephemeral: true);
                    }
                    else
                    {
                        Timer.NotifyUserMentions.Add(component.User.Mention);
                        await component.FollowupAsync(
                            embed: new SuccessMessage("Changed your mind? Click the button again.", "You'll be notified when this timer elapses").Embed,
                            ephemeral: true);
                    }
                },
            },
            new ActionButton()
            {
                Label = Timer.IsDeleted ? "Restore this deleted timer" : "Delete timer",
                Style = ButtonStyle.Danger,
                Action = async component =>
                {
                    if (Timer.IsDeleted)
                    {
                        Timer.Start();
                        await new SuccessMessage($"Timer was restarted on request of {component.User.Mention}")
                            .SendAsync(component.Channel);
                    }
                    else
                    {
                        Timer.Stop();
                        await new SuccessMessage($"Timer was deleted on request of {component.User.Mention}")
                            .SendAsync(component.Channel);
                    }

                    await UpdateMessageAsync();
                },
            },
        };

        public TimerStartedMessage(UserTimer timer)
        {
            EmbedBuilder = new EmbedBuilder()
            {
                Description = $"{new Emoji($"<:wowsuccess:{SuccessEmoteId}>")} {GetStatusMessageFormattedDescription("Timer was started.", null)}",
                Color = new Color(0x2ECC71),
            };

            Timer = timer;
        }

        private UserTimer Timer { get; }
    }
}