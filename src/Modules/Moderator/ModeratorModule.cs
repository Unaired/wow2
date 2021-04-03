using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Collections.Generic;
using System.Collections;
using wow2.Verbose.Messages;
using wow2.Data;
using wow2.Extentions;

namespace wow2.Modules.Moderator
{
    [Name("Moderator")]
    [Group("mod")]
    [Alias("moderator")]
    [Summary("For using tools to manage the server. (UNFINISHED)")]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        public static async Task CheckMessageWithAutoMod(SocketMessage message)
        {
            var config = GetConfigForGuild(message.GetGuild());

            if (!config.IsAutoModOn) return;

            UserRecord record = GetUserRecord(config, message.Author.Id);
            record.Messages.Add(message);

            Console.WriteLine(CheckMessagesForSpam(record.Messages).ToString());
        }

        [Command("warn")]
        [Summary("Sends a warning to a user with an optional message. Requires the 'Ban Members' permission.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task WarnAsync([Name("MENTION")] SocketGuildUser user, [Name("MESSAGE")][Remainder] string message)
        {
            var config = GetConfigForGuild(Context.Guild);

            message = string.IsNullOrWhiteSpace(message) ?
                "No reason was provided by the moderator." : $"Reason: {message}";

            GetUserRecord(config, user.Id).Warnings.Add(new Warning()
            {
                RequestedBy = Context.User.Id,
                DateTimeBinary = DateTime.Now.ToBinary()
            });

            IDMChannel dmChannel = await user.GetOrCreateDMChannelAsync();
            await new WarningMessage(
                description: $"You have recieved a warning from {Context.User.Mention} in the server '{Context.Guild.Name}'\nFurther warnings may result in a ban.\n```\n{message}\n```",
                title: "You have been warned!")
                    .SendAsync(dmChannel);

            await new SuccessMessage($"The user {user.Mention} has been warned by {Context.User.Mention}.")
                .SendAsync(dmChannel);

            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
        }

        [Command("mute")]
        [Alias("silence", "timeout")]
        [Summary("Temporarily disables a user's permission to speak. Requires the 'Ban Members' permission.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task MuteAsync([Name("MENTION")] SocketGuildUser user, string time = "30m", string message = "No reason given.")
        {
            throw new NotImplementedException();
        }

        [Command("user-record")]
        [Alias("user", "record")]
        [Summary("Gets a user record.")]
        public async Task UserAsync([Name("MENTION")] SocketGuildUser user)
        {
            var config = GetConfigForGuild(Context.Guild);
            UserRecord record = GetUserRecord(config, user.Id);

            await new InfoMessage($"`{record.Warnings.Count()}` warnings, {record.Mutes.Count()} mutes.")
                .SendAsync(Context.Channel);
        }

        [Command("set-warnings-until-ban")]
        [Summary("Sets the number of warnings required before a user is automatically banned. Requires the 'Ban Members' permission.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SetWarningsUntilBan(int number)
        {
            if (number < 2)
                throw new CommandReturnException(Context, "Number is too small.");

            GetConfigForGuild(Context.Guild).WarningsUntilBan = number;
            await new SuccessMessage($"{number} warnings will result in a ban.")
                .SendAsync(Context.Channel);
            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
        }

        [Command("toggle-auto-mod")]
        [Summary("Toggles whether the bot will mute or give warnings to users, for example if spam is detected.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ToggleAutoMod()
        {
            var config = GetConfigForGuild(Context.Guild);

            config.IsAutoModOn = !config.IsAutoModOn;
            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
            await new SuccessMessage($"Auto mod is now `{(config.IsAutoModOn ? "on" : "off")}`")
                .SendAsync(Context.Channel);
        }

        private static UserRecord GetUserRecord(ModeratorModuleConfig config, ulong id)
        {
            UserRecord matchingRecord = config.UserRecords
                .Where(record => record.UserId == id)
                .FirstOrDefault();

            // Ensure the user record exists
            if (matchingRecord == null)
            {
                config.UserRecords.Add(new UserRecord()
                {
                    UserId = id
                });

                // Could potentially be unsafe?
                matchingRecord = config.UserRecords.Last();
            }

            return matchingRecord;
        }

        private static bool CheckMessagesForSpam(IEnumerable<SocketMessage> messages)
        {
            const int numberOfMessagesToCheckForSpam = 7;

            // Order the list with newest messages first.
            messages = messages.OrderByDescending(message => message.Timestamp);
            
            // No need to check small number of messages.
            if (messages.Count() <= numberOfMessagesToCheckForSpam)
                return false;

            // Check if there is large number of messages in a small period of time.
            var timeSpan = messages.First().Timestamp - messages.ElementAt(numberOfMessagesToCheckForSpam).Timestamp;
            if (timeSpan < TimeSpan.FromSeconds(12))
                return true;

            return false;

            /*int totalMessagesChecked = 0;
            int totalRepeatedMessages = 0;

            
            foreach (SocketMessage messageToCheck in )
            {
                if (totalMessagesChecked >= numberOfMessagesToCheckForSpam) break;



                totalMessagesChecked++;
            }*/
        }

        private static ModeratorModuleConfig GetConfigForGuild(SocketGuild guild)
            => DataManager.DictionaryOfGuildData[guild.Id].Moderator;
    }
}