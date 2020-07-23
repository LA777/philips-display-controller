using ArgumentParser;
using pdc.DisplayApi;

namespace pdc.Arguments
{
    public class InfoArgument : IArgument
    {
        public string Name => "info";

        public string ShortName => "i";

        public string Description => "Shows information about available displays.";

        public string Parameter { get; set; } = null;

        public void Action()
        {
            DisplayApiService.ShowAvailableDisplayInfo();
        }
    }
}
