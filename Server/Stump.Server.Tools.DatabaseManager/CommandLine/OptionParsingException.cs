using System;

namespace Stump.Server.Tools.DatabaseManager.CommandLine
{
    public class OptionParsingException : Exception
    {
        public OptionParsingException(string message) : base(message)
        {
        }
    }
}
