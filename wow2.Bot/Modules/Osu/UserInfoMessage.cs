using System;
using System.Collections.Generic;
using Discord;
using wow2.Bot.Verbose.Messages;

namespace wow2.Bot.Modules.Osu
{
    public class UserInfoMessage : Message
    {
        public UserInfoMessage(UserData userData, Score[] bestScores)
        {
            var fieldBuildersForScores = new List<EmbedFieldBuilder>();

            foreach (Score score in bestScores)
            {
                fieldBuildersForScores.Add(new EmbedFieldBuilder()
                {
                    Name = OsuModule.MakeScoreTitle(score),
                    Value = OsuModule.MakeScoreDescription(score),
                });
            }

            EmbedBuilder = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"{userData.username} | #{userData.statistics.global_rank}",
                    IconUrl = userData.avatar_url.StartsWith("http") ? userData.avatar_url : null,
                    Url = $"https://osu.ppy.sh/users/{userData.id}",
                },
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Joined: {DateTime.Parse(userData.join_date)}",
                },
                Description = $"**Performance:** {Math.Round(userData.statistics.pp)}pp\n**Accuracy:** {Math.Round(userData.statistics.hit_accuracy, 2)}%\n**Time Played:** {userData.statistics.play_time / 3600}h",
                ImageUrl = userData.cover_url,
                Fields = fieldBuildersForScores,
                Color = Color.LightGrey,
            };
            }
        }
    }