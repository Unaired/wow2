using System;
using Discord.Commands;
using wow2.Verbose;

namespace wow2.Modules
{
    /// <summary>The exception that is thrown when a command returns with a warning message. Usually isn't fatal, only used to quickly return out of a command with a reply.</summary>
    public class CommandReturnException : Exception
    {
        public CommandReturnException(SocketCommandContext context, string description, string title = null) : base(title + description)
        {
            _ = GenericMessenger.SendWarningAsync(context.Channel, description, title);
        }
    }
}