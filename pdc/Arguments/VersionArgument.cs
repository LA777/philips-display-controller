using ArgumentParser;
using System;

namespace pdc.Arguments
{
    public class VersionArgument : IArgument
    {
        private const string Version = "0.0.1";

        public string Name => "version";

        public string ShortName => "v";

        public string Description => "Shows application version.";

        public string Parameter { get; set; } = null;

        public void Action()
        {
            Console.WriteLine(Version);
        }
    }
}
