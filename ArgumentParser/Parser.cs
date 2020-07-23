using System;
using System.Collections.Generic;
using System.Linq;

namespace ArgumentParser
{
    public class Parser
    {
        private const string ShortPrefix = "-";
        private const string Prefix = "--";
        private const string KeyValueDelimiter = ":";
        private const string ParameterDelimiter = ";";

        public static void Parse(string[] arguments, IEnumerable<IArgument> supportedArguments)
        {
            foreach (var argument in arguments)
            {
                var matchedArgument = supportedArguments.FirstOrDefault(x => $"{Prefix}{x.Name}" == argument);
                if (matchedArgument != null)
                {
                    if (arguments.Length > 1)
                    {
                        matchedArgument.Parameter = arguments[1];
                    }

                    matchedArgument.Action();
                }
                else
                {
                    var matchedShortArgument = supportedArguments.FirstOrDefault(x => $"{ShortPrefix}{x.ShortName}" == argument);
                    if (matchedShortArgument != null)
                    {
                        if (arguments.Length > 1)
                        {
                            matchedShortArgument.Parameter = arguments[1];
                        }

                        matchedShortArgument.Action();
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Unknown command. Please enter --help to see available arguments list.");
                    }
                }

                return;
            }
        }
    }
}
