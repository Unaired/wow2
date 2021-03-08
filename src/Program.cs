﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace wow2
{
    public class Program
    {
        public static DiscordSocketClient Client;

        private static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            string token = File.ReadAllText("discord.token");

            await DataManager.InitializeAsync();
            await EventHandlers.InstallCommandsAsync();

            //Client = new DiscordShardedClient(new DiscordSocketConfig() { TotalShards = 1 });
            Client = new DiscordSocketClient();

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            Client.Log += EventHandlers.LogAsync;
            Client.MessageReceived += EventHandlers.MessageRecievedAsync;

            await Task.Delay(-1);
        }
    }
}