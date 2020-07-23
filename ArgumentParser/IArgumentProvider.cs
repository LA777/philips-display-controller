using System.Collections.Generic;

namespace ArgumentParser
{
    public interface IArgumentProvider
    {
        IEnumerable<IArgument> GetArgumentsList();
    }
}
