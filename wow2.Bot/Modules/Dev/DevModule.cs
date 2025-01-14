using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using wow2.Bot.Data;
using wow2.Bot.Extensions;
using wow2.Bot.Modules.Keywords;
using wow2.Bot.Verbose;
using wow2.Bot.Verbose.Messages;

namespace wow2.Bot.Modules.Dev
{
    [Name("Developer")]
    [Group("dev")]
    [RequireOwner(Group = "Permission")]
    [Summary("Boring stuff for developers.")]
    public class DevModule : Module
    {
        [Command("load-guild-data")]
        [Alias("load")]
        [Summary("Loads guild data from file to memory, discarding any unsaved changes.")]
        public async Task LoadGuildDataAsync(ulong id = 0)
        {
            if (id == 0)
            {
                await DataManager.LoadGuildDataFromFileAsync();
                await new SuccessMessage($"`{DataManager.AllGuildData.Count}` guilds has their data loaded.")
                    .SendAsync(Context.Channel);
            }
            else
            {
                await DataManager.LoadGuildDataFromFileAsync(id);
                await new SuccessMessage($"Guild `{id}` had their data loaded.")
                    .SendAsync(Context.Channel);
            }
        }

        [Command("save-guild-data")]
        [Alias("save")]
        [Summary("Save guild data from memory to file, optionally stopping the bot.")]
        public async Task SaveGuildDataAsync(ulong id = 0)
        {
            if (id == 0)
            {
                await DataManager.SaveGuildDataToFileAsync();
                await new SuccessMessage($"`{DataManager.AllGuildData.Count}` guilds has their data saved.")
                    .SendAsync(Context.Channel);
            }
            else
            {
                await DataManager.SaveGuildDataToFileAsync(id);
                await new SuccessMessage($"Guild `{id}` had their data saved.")
                    .SendAsync(Context.Channel);
            }
        }

        [Command("guild-list")]
        [Summary("Displays a list of guilds the bot is currently in.")]
        public async Task GuildListAsync()
        {
            var listOfFieldBuilders = new List<EmbedFieldBuilder>();
            var guilds = BotService.Client.Guilds;

            foreach (var guild in guilds)
            {
                DataManager.AllGuildData.TryGetValue(guild.Id, out GuildData guildData);
                listOfFieldBuilders.Add(new EmbedFieldBuilder()
                {
                    Name = $"{guild.Name} *({guild.Id})*",
                    Value = $"Joined {DateTime.FromBinary(guildData?.DateTimeJoinedBinary ?? default).ToDiscordTimestamp("F")} with {guild.MemberCount} members.",
                });
            }

            string descExt = guilds.Count != DataManager.AllGuildData.Count ? $" but {DataManager.AllGuildData.Count} guild data files are loaded" : null;
            await new PagedMessage(
                fieldBuilders: listOfFieldBuilders,
                description: $"The bot is in {guilds.Count} guilds{descExt}.",
                title: "Joined Guilds",
                page: 1)
                    .SendAsync(Context.Channel);
        }

        [Command("get-guild-data")]
        [Summary("Gets the json file for the specified guild, default current guild.")]
        public async Task GetGuildDataAsync(ulong id = 0)
        {
            await Context.Channel.SendFileAsync(
                filePath: $"{DataManager.AppDataDirPath}/GuildData/{(id != 0 ? id : Context.Guild.Id)}.json");
        }

        [Command("set-status")]
        [Summary("Sets the 'playing' text and the status of the bot user.")]
        public async Task SetStatus(string message, UserStatus status)
        {
            await BotService.Client.SetGameAsync(message);
            await BotService.Client.SetStatusAsync(status);
            await new SuccessMessage("Set status.")
                .SendAsync(Context.Channel);
        }

        [Command("run-test")]
        [Alias("test")]
        [Summary("Runs a list of commands.")]
        public async Task TestAsync(params string[] groups)
        {
            if (groups.Length == 0)
            {
                string result = string.Empty;
                foreach (string name in Tests.TestList.Keys)
                    result += $"`{name}`\n";

                await new GenericMessage(result, "List of tests")
                    .SendAsync(Context.Channel);
                return;
            }

            foreach (string group in groups)
            {
                try
                {
                    await Tests.TestList[group](Context);
                    await new SuccessMessage("Finished test.")
                        .SendAsync(Context.Channel);
                }
                catch (Exception ex)
                {
                    await new ErrorMessage($"```{ex}```", "Test failed due to exception.")
                        .SendAsync(Context.Channel);
                }
            }
        }

        [Command("commands-list")]
        [Alias("commands")]
        [Summary("Creates a COMMANDS.md file with a list of all commands.")]
        public async Task CommandsListAsync()
        {
            string md = BotService.MakeCommandsMarkdown();
            await Context.Channel.SendFileAsync(md.ToMemoryStream(), "COMMANDS.md");
        }

        [Command("get-logs")]
        [Alias("logs", "log")]
        [Summary("Sends the log file for this session.")]
        public async Task GetLogsAsync()
        {
            string logs = await Logger.GetLogsForSessionAsync();
            await Context.Channel.SendFileAsync(logs.ToMemoryStream(), "wow2.log");
        }

        [Command("panic")]
        [Summary("Disables the bot for non-owner and changes the bot's Discord status.")]
        public async Task PanicAsync()
        {
            await BotService.Client.SetGameAsync("*under maintenance*");
            await BotService.Client.SetStatusAsync(UserStatus.DoNotDisturb);
            BotService.IsDisabled = true;

            await new SuccessMessage("Done.")
                .SendAsync(Context.Channel);
        }

        [Command("unpanic")]
        [Summary("Enables the bot for all.")]
        public async Task UnpanicAsync()
        {
            await BotService.Client.SetGameAsync("!wow help");
            await BotService.Client.SetStatusAsync(UserStatus.Online);
            BotService.IsDisabled = false;

            await new SuccessMessage("Done.")
                .SendAsync(Context.Channel);
        }

        [Command("poll-run")]
        [Alias("poll-invoke")]
        [Summary("Runs a polling task.")]
        public async Task PollRunAsync(int id)
        {
            PollingTask task = PollingService.PollingServiceTimers.ElementAtOrDefault(id - 1);
            if (task == null)
                throw new CommandReturnException(Context, "No matching polling task.");

            await task.InvokeAsync();
            await new SuccessMessage($"Run finished for `{task.Name}`")
                .SendAsync(Context.Channel);
        }

        [Command("poll-unblock")]
        [Summary("Unblocks a polling task.")]
        public async Task PollUnblockAsync(int id)
        {
            PollingTask task = PollingService.PollingServiceTimers.ElementAtOrDefault(id - 1);
            if (task == null)
                throw new CommandReturnException(Context, "No matching polling task.");

            task.Blocked = false;
            await new SuccessMessage($"Unblocked `{task.Name}` with interval {task.IntervalMinutes}min")
                .SendAsync(Context.Channel);
        }

        [Command("poll-block")]
        [Summary("Blocks a polling service.")]
        public async Task PollBlockAsync(int id)
        {
            PollingTask task = PollingService.PollingServiceTimers.ElementAtOrDefault(id - 1);
            if (task == null)
                throw new CommandReturnException(Context, "No matching polling task.");

            task.Blocked = true;
            await new SuccessMessage($"Blocked `{task.Name}`")
                .SendAsync(Context.Channel);
        }

        [Command("poll-list")]
        [Summary("Lists all polling services.")]
        public async Task PollListAsync()
        {
            // Would make more sense to build fields here instead.
            StringBuilder stringBuilder = new();
            int num = 1;
            foreach (PollingTask task in PollingService.PollingServiceTimers)
            {
                stringBuilder
                    .Append(num)
                    .Append(". ")
                    .AppendLine(task.Name)
                    .AppendLine(task.Blocked ? " - BLOCKED" : " - UNBLOCKED")
                    .Append(" - INTERVAL: ")
                    .Append(task.IntervalMinutes)
                    .AppendLine("m")
                    .Append(" - LAST RUN: ");

                if (task.LastRun == default)
                {
                    stringBuilder
                        .AppendLine("never")
                        .AppendLine();
                }
                else
                {
                    TimeSpan lastRun = DateTime.Now - task.LastRun;
                    stringBuilder
                        .Append(Math.Floor(lastRun.TotalMinutes))
                        .AppendLine("m")
                        .AppendLine();
                }

                num++;
            }

            await new GenericMessage($"```md\n{stringBuilder}\n```", "Polling services")
                .SendAsync(Context.Channel);
        }

        [Command("reconnect")]
        [Summary("Disconnect and reconnect the bot.")]
        public async Task ReconnectAsync()
        {
            await BotService.Client.StopAsync();
            await BotService.Client.StartAsync();

            await new SuccessMessage("Done.")
                .SendAsync(Context.Channel);
        }

        [Command("execute-manual-script")]
        [Alias("execute-manual", "manual-script", "script", "manual")]
        [Summary("Run the file at the environment variable WOW2_MANUAL_SCRIPT.")]
        public async Task ExecuteManualScriptAsync()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            using (var process = new Process())
            {
                process.StartInfo.FileName = isWindows ? "cmd" : "bash";
                process.StartInfo.Arguments = $"{(isWindows ? "/c" : "-c")} {Environment.GetEnvironmentVariable("WOW2_MANUAL_SCRIPT")}";
                process.Start();
                await process.WaitForExitAsync();
            }

            await new SuccessMessage("Done.")
                .SendAsync(Context.Channel);
        }

        [Command("stop-program")]
        [Summary("Prepares to stop the program.")]
        public async Task StopProgramAsync(bool exit = false)
        {
            await SaveGuildDataAsync();

            await BotService.Client.SetGameAsync("RESTARTING...");
            await BotService.Client.SetStatusAsync(UserStatus.DoNotDisturb);
            BotService.IsDisabled = true;

            foreach (GuildData guildData in DataManager.AllGuildData.Values)
            {
                foreach (ResponseMessage message in guildData.Keywords.ListOfResponseMessages.ToArray())
                {
                    try
                    {
                        await message.SentMessage.RemoveAllReactionsAsync();
                        Logger.Log($"Removed reactions from {message.SentMessage.Id}", LogSeverity.Debug);
                    }
                    catch
                    {
                        Logger.Log($"Failed to remove reactions from {message.SentMessage?.Id}", LogSeverity.Debug);
                    }
                }

                foreach (InteractiveMessage message in guildData.InteractiveMessages.ToArray())
                {
                    try
                    {
                        await message.StopAsync();
                    }
                    catch
                    {
                        Logger.Log($"Failed to stop interactive message {message.SentMessage?.Id}", LogSeverity.Debug);
                    }
                }
            }

            await new SuccessMessage(exit ? "The bot will be stopped." : "The bot is ready to be stopped.")
                .SendAsync(Context.Channel);

            if (exit)
                Environment.Exit(0);
        }

        [Command("throw")]
        [Summary("Throws an unhandled exception.")]
        public Task Throw()
            => throw new Exception("This is a test exception.");
    }
}