using ArgumentParser;

namespace pdc
{
    class Program
    {
        static void Main(string[] args)
        {
            var argumentsProvider = new SupportedArguments();
            Parser.Parse(args, argumentsProvider.GetArgumentsList());
        }
    }
}
