using ArgumentParser;

namespace pdc
{
    class Program
    {
        static void Main(string[] args)
        {
            var argumnetsProvider = new SupportedArguments();
            Parser.Parse(args, argumnetsProvider.GetArgumentsList());
        }
    }
}
