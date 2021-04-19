using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
using wow2.Modules.Main;
using wow2.Modules.Keywords;

namespace wow2.Modules.Dev
{
    public static class Tests
    {
        public static Dictionary<string, Func<ICommandContext, Task>> TestList = new Dictionary<string, Func<ICommandContext, Task>>()
        {
            {
                "aliases", async (context) =>
                {
                    var config = MainModule.GetConfigForGuild(context.Guild);
                    const string aliasName = "testing_alias";

                    await ExecuteCommandsAsync(context,
                        $"alias {aliasName} \"{config.CommandPrefix} help\"");
                    await AssertAsync(context, new Dictionary<string, bool>()
                    {
                        {"key exists in dictionary", config.AliasesDictionary.ContainsKey(aliasName)},
                        {"check definition", config.AliasesDictionary[aliasName] == "help"}
                    });

                    await ExecuteCommandsAsync(context,
                        $"alias {aliasName}");
                    await AssertAsync(context,
                        "alias has been removed", !config.AliasesDictionary.ContainsKey(aliasName)); 
                }
            },
            {
                "keywords", async (context) =>
                {
                    var config = KeywordsModule.GetConfigForGuild(context.Guild);
                    const string keywordName = "testing_keyword";
                    List<KeywordValue> keywordValues;

                    await ExecuteCommandsAsync(context,
                        $"keywords remove {keywordName}");
                    await AssertAsync(context,
                        "keyword doesn't exist", !config.KeywordsDictionary.ContainsKey(keywordName));

                    await ExecuteCommandsAsync(context,
                        $"keywords add {keywordName} value1",
                        $"keywords add \"{keywordName}\" \"value **Title!**with title\"");
                    await AssertAsync(context, new Dictionary<string, bool>()
                    {
                        {$"keyword exists in dictionary", config.KeywordsDictionary.TryGetValue(keywordName, out keywordValues)},
                        {$"check value1", keywordValues[0].Content == "value1"},
                        {$"check value2", keywordValues[1].Content == "value2 with title" && keywordValues[1].Title == "Title!"}
                    });

                    await ExecuteCommandsAsync(context,
                        $"keywords remove {keywordName} value1");
                    await AssertAsync(context,
                        "value was removed", keywordValues.Count == 1);

                    await ExecuteCommandsAsync(context,
                        "keywords remove testing_keyword");
                    await AssertAsync(context,
                        "keyword was removed", !config.KeywordsDictionary.ContainsKey(keywordName));
                }
            }
        };

        private static async Task ExecuteCommandsAsync(ICommandContext context, params string[] commands)
        {
            string commandPrefix = MainModule.GetConfigForGuild(context.Guild).CommandPrefix;

            foreach (string command in commands)
            {
                await context.Channel.SendMessageAsync($"`{commandPrefix} {command}`");

                await Task.Delay(1000);
                await EventHandlers.ExecuteCommandAsync(context, command);
                await Task.Delay(1000);
            }
        }

        private static async Task AssertAsync(ICommandContext context, string description, bool value)
        {
            if (!value) throw new Exception($"Assert failure ({description})");
            await context.Channel.SendMessageAsync($"**✅ ASSERT:** {description}");
        }

        private static async Task AssertAsync(ICommandContext context, Dictionary<string, bool> asserts)
        {
            foreach (var assert in asserts)
            {
                await AssertAsync(context, assert.Key, assert.Value);
            }
        }
    }
}