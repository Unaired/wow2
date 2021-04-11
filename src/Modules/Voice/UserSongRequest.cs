using System;
using Discord.WebSocket;

namespace wow2.Modules.Voice
{
    public class UserSongRequest
    {
        public VideoMetadata VideoMetadata { get; set; }
        public DateTime TimeRequested { get; set; }
        public SocketUser RequestedBy { get; set; }
    }
}