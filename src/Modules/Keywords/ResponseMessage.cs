using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using wow2.Data;
using wow2.Extentions;
using wow2.Verbose.Messages;

namespace wow2.Modules.Keywords
{
    public class ResponseMessage : GenericMessage
    {
        public static readonly IEmote DeleteReactionEmote = new Emoji("🗑");
        public static readonly IEmote LikeReactionEmote = new Emoji("👍");

        private const int MaxCountOfRememberedKeywordResponses = 50;

        public ResponseMessage(KeywordValue keywordValue)
             : base(string.Empty)
        {
            AllowMentions = false;

            KeywordValue = keywordValue;
            EmbedBuilder.Description = KeywordValue.Content;
            EmbedBuilder.Title = KeywordValue.Title;
        }

        public KeywordValue KeywordValue { get; }

        /// <summary>Checks if a message was a keyword response sent by the bot, and acts on the reaction if so.</summary>
        public static async Task<bool> ActOnReactionAsync(SocketReaction reaction, IUserMessage message)
        {
            var config = KeywordsModule.GetConfigForGuild(message.GetGuild());

            ResponseMessage responseMessage = config.ListOfResponseMessages.Find(
                m => m.SentMessage?.Id == message.Id);
            if (responseMessage == null)
                return false;

            if (reaction.Emote.Name == DeleteReactionEmote.Name && config.IsDeleteReactionOn)
            {
                config.ListOfResponseMessages.Remove(responseMessage);
                await responseMessage.SentMessage.DeleteAsync();
                await DataManager.SaveGuildDataToFileAsync(responseMessage.SentMessage.GetGuild().Id);
            }
            else if (reaction.Emote.Name == LikeReactionEmote.Name && config.IsLikeReactionOn)
            {
                // Record like.
            }
            else
            {
                return false;
            }

            return true;
        }

        public async Task<IUserMessage> RespondToMessageAsync(SocketMessage message)
        {
            IGuild guild = message.GetGuild();
            var config = KeywordsModule.GetConfigForGuild(guild);
            ReplyToMessageId = message.Id;

            // Don't use embed message if the value to send contains a link.
            if (KeywordValue.Content.Contains("http://") || KeywordValue.Content.Contains("https://"))
            {
                await SendAsPlainTextAsync(message.Channel);
            }
            else
            {
                await SendAsync(message.Channel);
            }

            if (config.IsLikeReactionOn)
                await SentMessage.AddReactionAsync(LikeReactionEmote);
            if (config.IsDeleteReactionOn)
                await SentMessage.AddReactionAsync(DeleteReactionEmote);

            // Remember the messages that are actually keyword responses by adding them to a list.
            config.ListOfResponseMessages.Add(this);

            // Remove the oldest message if ListOfResponsesId has reached its max.
            if (config.ListOfResponseMessages.Count > MaxCountOfRememberedKeywordResponses)
                config.ListOfResponseMessages.RemoveAt(0);

            await DataManager.SaveGuildDataToFileAsync(guild.Id);
            return SentMessage;
        }
    }
}