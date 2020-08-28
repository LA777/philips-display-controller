using ArgumentParser;
using pdc.DisplayApi;
using System;
using System.Linq;

namespace pdc.Arguments
{
    public class BrightnessArgument : IArgument
    {
        private string _brightnessParameter = string.Empty;

        public string Name => "brightness";

        public string ShortName => "b";

        public string Description => "Adjusts display brightness. Example: 'pdc --brightness 1:20;2:20' Where 1 and 2 are display indexes, 20 is brightness level (from 0 to 100). Example: 'pdc --brightness all:20' Where all - all available monitors, 20 is brightness level.";

        public string Parameter
        {
            get => _brightnessParameter;
            set => _brightnessParameter = value;
        }

        public void Action()
        {
            if (Parameter == string.Empty)
            {
                Console.WriteLine("No argument provided.");
                return;
            }

            var parameters = Parameter.Split(';');

            if (parameters.Length == 1)
            {
                var values = parameters.First().Split(':');
                if (values[0] == "all")
                {
                    DisplayApiService.SetAllDisplayBrightness(Convert.ToInt32(values[1]));
                    return;
                }

                DisplayApiService.SetDisplayBrightness(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
                return;
            }

            foreach (var parameter in parameters)
            {
                var values = parameter.Split(':');
                DisplayApiService.SetDisplayBrightness(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
            }
        }
    }
}
