using System.Threading.Tasks;
using Discord;

namespace wow2.Verbose.Messages
{
    /// <summary>Base class for sending and building embed messages.</summary>
    public abstract class Message
    {
        public const ulong SuccessEmoteId = 823595458978512997;
        public const ulong InfoEmoteId = 804732580423008297;
        public const ulong WarningEmoteId = 804732632751407174;
        public const ulong ErrorEmoteId = 804732656721199144;

        public ulong ReplyToMessageId { get; set; }
        public IUserMessage SentMessage { get; protected set; }
        public MessageReference MessageReference
        {
            get { return ReplyToMessageId != 0 ? new MessageReference(ReplyToMessageId) : null; }
        }
        public Embed Embed
        {
            get { return EmbedBuilder.Build(); }
        }

        protected EmbedBuilder EmbedBuilder;

        public virtual async Task<IUserMessage> SendAsync(IMessageChannel channel)
        {
            SentMessage = await channel.SendMessageAsync(
                embed: EmbedBuilder.Build(),
                allowedMentions: AllowedMentions.None,
                messageReference: MessageReference);
            return SentMessage;
        }

        protected static string GetStatusMessageFormattedDescription(string description, string title)
            => (title == null ? null : $"**{title}**\n") + description;
    }
}