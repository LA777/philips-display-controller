using ArgumentParser;
using System;
using System.Reflection;

namespace pdc.Arguments
{
    public class VersionArgument : IArgument
    {
        public string Name => "version";

        public string ShortName => "v";

        public string Description => "Shows application version.";

        public string Parameter { get; set; } = null;

        public void Action()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine(version.ToString());
        }
    }
}
