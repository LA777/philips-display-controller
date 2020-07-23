using ArgumentParser;
using pdc.Arguments;
using System.Collections.Generic;

namespace pdc
{
    public class SupportedArguments : IArgumentProvider
    {
        private static readonly IEnumerable<IArgument> Arguments = new List<IArgument>
        {
            new VersionArgument(),
            new HelpArgument(),
            new InfoArgument(),
            new BrightnessArgument()
        };

        public IEnumerable<IArgument> GetArgumentsList()
        {
            return Arguments;
        }
    }
}
