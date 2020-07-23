using ArgumentParser;
using System;

namespace pdc.Arguments
{
    public class HelpArgument : IArgument
    {
        public string Name => "help";

        public string ShortName => "h";

        public string Description => "Shows list of available commands.";

        public string Parameter { get; set; } = null;
        public void Action()
        {
            var argumnetsProvider = new SupportedArguments();
            var arguments = argumnetsProvider.GetArgumentsList();

            foreach (var argument in arguments)
            {
                Console.WriteLine($"-{argument.ShortName}, --{argument.Name}\t\t{argument.Description}");
            }
        }
    }
}
