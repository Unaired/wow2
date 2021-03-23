using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using wow2.Verbose;
using wow2.Data;
using wow2.Extentions;

namespace wow2.Modules.Main
{
    [Name("Main")]
    [Summary("Commands that don't fit anywhere else.")]
    public class MainModule : ModuleBase<SocketCommandContext>
    {
        [Command("about")]
        [Summary("Shows infomation about the bot.")]
        public async Task AboutAsync()
        {
            await SendAboutMessageToChannelAsync(Context);
        }

        [Command("help")]
        [Summary("If MODULE is left empty, displays all commands. Otherwise displays detailed info about a specific group of commands.")]
        public async Task HelpAsync([Name("MODULE")] string group = null)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                await ReplyAsync(
                    embed: Messenger.Fields(await ModuleInfoToEmbedFields(), "Help")
                );
            }
            else
            {
                await ReplyAsync(
                    embed: Messenger.Fields(await CommandInfoToEmbedFieldsAsync(group), $"Command Help")
                );
            }
        }

        [Command("alias")]
        [Alias("aliases")]
        [Summary("Sets an alias. Typing the NAME of an alias will execute '!wow DEFINITION' as a command. Set the DEFINITION of an alias to \"\" to remove it.")]
        public async Task AliasAsync(string name, [Name("DEFINITION")] params string[] definitionSplit)
        {
            var config = DataManager.GetMainConfigForGuild(Context.Guild);
            string definition = string.Join(" ", definitionSplit);

            if (!config.AliasesDictionary.TryAdd(name, definition))
            {
                if (definition != "")
                    throw new CommandReturnException(Context, $"The alias `{name}` already exists.\nTo remove the alias, type `{EventHandlers.CommandPrefix} alias \"{name}\" \"\"`");

                config.AliasesDictionary.Remove(name);
                await Messenger.SendSuccessAsync(Context.Channel, $"The alias `{name}` was removed.");
                return;
            }

            if (definition == "")
            {
                config.AliasesDictionary.Remove(name);
                throw new CommandReturnException(Context, $"An alias should have a definition that isn't blank.");
            }

            await Messenger.SendSuccessAsync(Context.Channel, $"Typing `{name}` will now execute `{EventHandlers.CommandPrefix} {definition}`");

            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
        }

        [Command("alias-list")]
        [Alias("alias", "aliases", "list-alias", "list-aliases")]
        [Summary("Displays a list of aliases.")]
        public async Task AliasAsync()
        {
            var config = DataManager.GetMainConfigForGuild(Context.Guild);

            var listOfFieldBuilders = new List<EmbedFieldBuilder>();
            foreach (var aliasPair in config.AliasesDictionary)
            {
                var fieldBuilderForAlias = new EmbedFieldBuilder()
                {
                    Name = aliasPair.Key,
                    Value = $"`{EventHandlers.CommandPrefix} {aliasPair.Value}`",
                    IsInline = true
                };
                listOfFieldBuilders.Add(fieldBuilderForAlias);
            }

            await ReplyAsync(
                embed: Messenger.Fields(listOfFieldBuilders, "List of Aliases", "To remove any of these aliases, just set the alias definition to \"\"")
            );
        }

        [Command("savedata")]
        [Summary("Uploads the raw data stored about this server by the bot.")]
        public async Task UploadRawGuildDataAsync()
        {
            await Context.Channel.SendFileAsync(
                filePath: $"{DataManager.AppDataDirPath}/GuildData/{Context.Guild.Id}.json"
            );
        }

        public static async Task SendAboutMessageToChannelAsync(SocketCommandContext context)
        {
            var appInfo = await Program.Client.GetApplicationInfoAsync();

            var embedBuilder = new EmbedBuilder()
            {
                Title = appInfo.Name,
                Description = string.IsNullOrWhiteSpace(appInfo.Description) ? "" : appInfo.Description,
                Color = Color.LightGrey,
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"Hosted by {appInfo.Owner}",
                    IconUrl = appInfo.Owner.GetAvatarUrl(),
                },
                Footer = new EmbedFooterBuilder()
                {
                    Text = $" - To view a list of commands, type `{EventHandlers.CommandPrefix} help`"
                }
            };

            await context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        public static async Task<bool> CheckForAliasAsync(SocketMessage message)
        {
            var config = DataManager.GetMainConfigForGuild(message.GetGuild());

            var aliasesFound = config.AliasesDictionary.Where(a => message.Content.StartsWithWord(a.Key));

            if (aliasesFound.Count() != 0)
            {
                var context = new SocketCommandContext(Program.Client, (SocketUserMessage)message);
                var aliasToExecute = aliasesFound.First();

                IResult result = await EventHandlers.BotCommandService.ExecuteAsync
                (
                    context: context,
                    input: aliasToExecute.Value + message.Content.Replace(aliasToExecute.Key, "", true, null),
                    services: null
                );

                if (result.Error.HasValue)
                    await EventHandlers.SendErrorMessageToChannel(result.Error, context.Message.Channel);

                return true;
            }
            return false;
        }

        /// <summary>Builds embed fields for all command modules.</summary>
        private async Task<List<EmbedFieldBuilder>> ModuleInfoToEmbedFields()
        {
            var listOfModules = EventHandlers.BotCommandService.Modules;

            // Create a help text string for each module
            var listOfFieldBuilders = new List<EmbedFieldBuilder>();
            await Task.Run(() =>
            {
                foreach (ModuleInfo module in listOfModules)
                {
                    var fieldBuilderForModule = new EmbedFieldBuilder()
                    {
                        Name = $"{module.Name} Module"
                    };

                    if (!String.IsNullOrWhiteSpace(module.Summary))
                        fieldBuilderForModule.Value += $"*{module.Summary}*\n";

                    fieldBuilderForModule.Value += $"`{EventHandlers.CommandPrefix} help {module.Name.ToLower()}`";

                    // TODO: find a way to get name attribute of this class instead of hardcoding module name.
                    if (module.Name == "Main")
                    {
                        listOfFieldBuilders.Insert(0, fieldBuilderForModule);
                    }
                    else
                    {
                        listOfFieldBuilders.Add(fieldBuilderForModule);
                    }
                }
            });
            return listOfFieldBuilders;
        }

        /// <summary>Builds embed fields for commands in a single module</summary>
        private async Task<List<EmbedFieldBuilder>> CommandInfoToEmbedFieldsAsync(string specifiedModuleName)
        {
            // Find commands in module.
            var listOfCommandInfo = (await EventHandlers.BotCommandService.GetExecutableCommandsAsync(
                new CommandContext(Context.Client, Context.Message), null
            ))
            .Where(c =>
                c.Module.Name.Equals(specifiedModuleName, StringComparison.CurrentCultureIgnoreCase)
                || c.Module.Aliases.Contains(specifiedModuleName.ToLower())
            );

            var listOfFieldBuilders = new List<EmbedFieldBuilder>();
            foreach (CommandInfo command in listOfCommandInfo)
            {
                listOfFieldBuilders.Add(new EmbedFieldBuilder()
                {
                    Name = $"`{EventHandlers.CommandPrefix} {(string.IsNullOrWhiteSpace(command.Module.Group) ? "" : $"{command.Module.Group} ")}{command.Name}{ParametersToString(command.Parameters)}`",
                    Value = $"*{(string.IsNullOrWhiteSpace(command.Summary) ? "No description provided." : command.Summary)}*"
                });
            }
            return listOfFieldBuilders;
        }

        private string ParametersToString(IReadOnlyList<ParameterInfo> parameters)
        {
            string parametersInfo = "";
            foreach (ParameterInfo parameter in parameters)
            {
                string optionalText = parameter.IsOptional ? "optional:" : "";
                parametersInfo += $" [{optionalText}{parameter.Name.ToUpper()}]";
            }
            return parametersInfo;
        }
    }
}