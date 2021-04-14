using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using wow2.Verbose.Messages;
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
            var commandPrefix = GetConfigForGuild(Context.Guild).CommandPrefix;
            if (string.IsNullOrWhiteSpace(group))
            {
                await new GenericMessage(
                    fieldBuilders: await ModuleInfoToEmbedFieldsAsync(commandPrefix),
                    title: "📃 Help")
                        .SendAsync(Context.Channel);
            }
            else
            {
                await new GenericMessage(
                    fieldBuilders: await CommandInfoToEmbedFieldsAsync(group, commandPrefix),
                    title: $"📃 Command Help")
                        .SendAsync(Context.Channel);
            }
        }

        [Command("alias")]
        [Alias("aliases")]
        [Summary("Sets an alias. Typing the NAME of an alias will execute '!wow DEFINITION' as a command. Set the DEFINITION of an alias to \"\" to remove it.")]
        public async Task AliasAsync(string name, [Name("DEFINITION")] params string[] definitionSplit)
        {
            var config = GetConfigForGuild(Context.Guild);
            string removeAliasText = $"To remove the alias, type `{config.CommandPrefix} alias \"{name}\"`";

            string definition = string.Join(" ", definitionSplit.Where(w
                => !w.Equals(config.CommandPrefix, StringComparison.CurrentCultureIgnoreCase)));

            if (config.AliasesDictionary.ContainsKey(name))
            {
                if (!string.IsNullOrWhiteSpace(definition))
                {
                    // Assume the user wants to change the existing alias' definition.
                    config.AliasesDictionary[name] = definition;
                }
                else
                {
                    // Assume the user wants to remove the existing alias.
                    config.AliasesDictionary.Remove(name);
                    await new SuccessMessage($"The alias `{name}` was removed.")
                        .SendAsync(Context.Channel);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(definition))
                    throw new CommandReturnException(Context, $"An alias should have a definition that isn't blank.");

                config.AliasesDictionary.Add(name, definition);
            }

            await new SuccessMessage($"Typing `{name}` will now execute `{config.CommandPrefix} {definition}`\n{removeAliasText}")
                .SendAsync(Context.Channel);
            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
        }

        [Command("alias-list")]
        [Alias("alias", "aliases", "list-alias", "list-aliases")]
        [Summary("Displays a list of aliases.")]
        public async Task AliasListAsync()
        {
            var config = GetConfigForGuild(Context.Guild);

            var listOfFieldBuilders = new List<EmbedFieldBuilder>();
            foreach (var aliasPair in config.AliasesDictionary)
            {
                var fieldBuilderForAlias = new EmbedFieldBuilder()
                {
                    Name = aliasPair.Key,
                    Value = $"`{config.CommandPrefix} {aliasPair.Value}`",
                    IsInline = true
                };
                listOfFieldBuilders.Add(fieldBuilderForAlias);
            }

            await new GenericMessage(
                fieldBuilders: listOfFieldBuilders,
                title: "📎 List of Aliases",
                description: "To remove any of these aliases, set the alias definition to a blank value.")
                    .SendAsync(Context.Channel);
        }

        [Command("ping")]
        [Summary("Checks the latency between the message that executes a command, and the response that the bot sends.")]
        public async Task PingAsync()
        {
            IUserMessage pongMessage = await new SuccessMessage("That was about `...`ms", "Pong!")
                .SendAsync(Context.Channel);
            TimeSpan pongTimeSpan = pongMessage.Timestamp.Subtract(Context.Message.Timestamp);

            await pongMessage.ModifyAsync(message
                => message.Embed = new SuccessMessage($"That was about `{pongTimeSpan.Milliseconds}ms`", "Pong!").Embed);
        }

        [Command("set-command-prefix")]
        [Summary("Change the prefix used to identify commands. '!wow' is the default.")]
        public async Task SetCommandPrefixAsync(string prefix)
        {
            var config = GetConfigForGuild(Context.Guild);

            if (prefix.Contains(' ') || string.IsNullOrWhiteSpace(prefix))
                throw new CommandReturnException(Context, "The command prefix must not contain spaces.");

            config.CommandPrefix = prefix;
            await new SuccessMessage($"Changed command prefix to `{prefix}`")
                .SendAsync(Context.Channel);
            await DataManager.SaveGuildDataToFileAsync(Context.Guild.Id);
        }

        public static async Task SendAboutMessageToChannelAsync(SocketCommandContext context)
        {
            var commandPrefix = GetConfigForGuild(context.Guild).CommandPrefix;
            var appInfo = Program.ApplicationInfo;

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
                    Text = $" - To view a list of commands, type `{commandPrefix} help`"
                }
            };

            await context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        public static async Task<bool> CheckForAliasAsync(SocketMessage message)
        {
            var config = GetConfigForGuild(message.GetGuild());

            var aliasesFound = config.AliasesDictionary.Where(a => message.Content.StartsWithWord(a.Key));

            if (aliasesFound.Count() != 0)
            {
                var context = new SocketCommandContext(Program.Client, (SocketUserMessage)message);
                var aliasToExecute = aliasesFound.First();

                await EventHandlers.ExecuteCommandAsync(
                    context,
                    aliasToExecute.Value + message.Content.Replace(aliasToExecute.Key, "", true, null));

                return true;
            }
            return false;
        }

        /// <summary>Builds embed fields for all command modules.</summary>
        private async Task<List<EmbedFieldBuilder>> ModuleInfoToEmbedFieldsAsync(string commandPrefix)
        {
            var listOfModules = (await EventHandlers.BotCommandService.GetExecutableCommandsAsync(Context, null))
                .Select(command => command.Module)
                
                // Remove duplicate modules.
                .GroupBy(module => module)
                .Select(group => group.First());

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

                    fieldBuilderForModule.Value += $"`{commandPrefix} help {module.Name.ToLower()}`";

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
        private async Task<List<EmbedFieldBuilder>> CommandInfoToEmbedFieldsAsync(string specifiedModuleName, string commandPrefix)
        {
            // Find commands in module.
            var listOfCommandInfo = (await EventHandlers.BotCommandService.GetExecutableCommandsAsync(
                new CommandContext(Context.Client, Context.Message), null
            ))
            .Where(c =>
                c.Module.Name.Equals(specifiedModuleName, StringComparison.CurrentCultureIgnoreCase)
                || c.Module.Aliases.Contains(specifiedModuleName.ToLower())
            );

            if (listOfCommandInfo.Count() == 0)
                throw new CommandReturnException(Context, "Did you make a typo?", "No such module");

            var listOfFieldBuilders = new List<EmbedFieldBuilder>();
            foreach (CommandInfo command in listOfCommandInfo)
            {
                listOfFieldBuilders.Add(new EmbedFieldBuilder()
                {
                    Name = $"`{commandPrefix} {(string.IsNullOrWhiteSpace(command.Module.Group) ? "" : $"{command.Module.Group} ")}{command.Name}{ParametersToString(command.Parameters)}`",
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

        public static MainModuleConfig GetConfigForGuild(SocketGuild guild)
            => DataManager.DictionaryOfGuildData[guild.Id].Main;
    }
}