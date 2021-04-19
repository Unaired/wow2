using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using wow2.Verbose.Messages;
using wow2.Extentions;
using wow2.Data;

namespace wow2.Modules.Games.NumberMemory
{
    public class NumberMemoryGame
    {
        /// <returns>True if the message was related to the game.</returns>
        public static async Task<bool> CheckMessageAsync(SocketMessage receivedMessage)
        {
            var config = GetConfigForGuild(receivedMessage.GetGuild());
            return false;
        }

        public static async Task StartGame(SocketCommandContext context)
        {
            throw new NotImplementedException();

            var config = GetConfigForGuild(context.Guild);

            config.IsGameStarted = true;
            config.HighestNumberOfDigits = 0;

            await new InfoMessage(
                description: "When the number disappears, try retype the number. Don't start typing before the message gets deleted!",
                title: $"Number memory has started for {context.User.Mention}")
                    .SendAsync(context.Channel);
        }

        private static async Task EndGameAsync(NumberMemoryConfig config)
        {
            config.IsGameStarted = false;
        }

        public static NumberMemoryConfig GetConfigForGuild(IGuild guild)
            => DataManager.DictionaryOfGuildData[guild.Id].Games.NumberMemory;
    }
}