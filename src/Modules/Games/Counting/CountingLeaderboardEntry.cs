using System;
using wow2.Modules.Games.Counting;

namespace wow2.Modules.Games
{
    public class CountingLeaderboardEntry
    {
        public CountingLeaderboardEntry()
        {
        }

        public CountingLeaderboardEntry(CountingGameConfig gameConfig)
        {
            Increment = gameConfig.Increment;
            FinalNumber = gameConfig.NextNumber - gameConfig.Increment;
            NumberOfCorrectMessages = gameConfig.ListOfMessages.Count;
        }

        public float Increment { get; }
        public float FinalNumber { get; }
        public int NumberOfCorrectMessages { get; }
    }
}