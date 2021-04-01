using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using wow2.Verbose;
using wow2.Data;

namespace wow2.Modules.Moderator
{
    [Name("Moderator")]
    [Group("mod")]
    [Alias("moderator")]
    [Summary("For using tools to manage the server.")]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        [Summary("Sends a warning to a user with an optional message. Requires the 'Ban Members' permission.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task WarnAsync(SocketGuildUser user, [Name("MESSAGE")] params string[] messageSplit)
        {
            var config = DataManager.GetModeratorConfigForGuild(Context.Guild);

            string message = messageSplit.Length == 0 ? 
                "No reason was provided by the moderator." : $"Reason: {string.Join(' ', messageSplit)}";

            GetUserRecord(config, user.Id).Warnings.Add(new Warning()
            {
                RequestedBy = Context.User.Id,
                DateTimeBinary = DateTime.Now.ToBinary()
            });

            IDMChannel dmChannel = await user.GetOrCreateDMChannelAsync();
            await GenericMessenger.SendWarningAsync(
                channel: dmChannel,
                description: $"You have recieved a warning from {Context.User.Mention} in the server '{Context.Guild.Name}'\nFurther warnings may result in a ban.\n```\n{message}\n```",
                title: "You have been warned!");

            await GenericMessenger.SendSuccessAsync(
                channel: Context.Channel,
                description: $"The user {user.Mention} has been warned by {Context.User.Mention}.");

            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
        }

        [Command("mute")]
        [Alias("silence", "timeout")]
        [Summary("Temporarily disables a user's permission to speak. Requires the 'Ban Members' permission.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task MuteAsync(SocketUser user, string time = "30m", string message = "No reason given.")
        {
            throw new NotImplementedException(); 
        }

        [Command("set-warnings-until-ban")]
        [Summary("Sets the number of warnings required before a user is automatically banned. Requires the 'Ban Members' permission.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SetWarningsUntilBan(int number)
        {
            if (number < 2)
                throw new CommandReturnException(Context, "Number is too small.");
            
            DataManager.GetModeratorConfigForGuild(Context.Guild).WarningsUntilBan = number;
            await GenericMessenger.SendSuccessAsync(Context.Channel, $"{number} warnings will result in a ban.");
            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
        }

        private UserRecord GetUserRecord(ModeratorModuleConfig config, ulong id)
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
    }
}