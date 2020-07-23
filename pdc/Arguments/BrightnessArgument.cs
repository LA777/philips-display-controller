using ArgumentParser;
using pdc.DisplayApi;
using System;

namespace pdc.Arguments
{
    public class BrightnessArgument : IArgument
    {
        private string _brightnessParameter = string.Empty;

        public string Name => "brightness";

        public string ShortName => "b";

        public string Description => "Adjusts display brightness. Example: 'pdc --brightness 1:20;2:20' Where 1 and 2 are display indexes, 20 is brightness level (from 0 to 100).";

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

            foreach (var parameter in parameters)
            {
                var values = parameter.Split(':');
                DisplayApiService.SetDisplayBrightness(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
            }
        }
    }
}
