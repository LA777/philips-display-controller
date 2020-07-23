using CSDDCHelperAPILib;
using System;
using System.Collections.Generic;
using System.Management;

namespace pdc
{
    class Program
    {
        public static List<string> displayNames = new List<string>();
        public static string currentDisplayName = string.Empty;

        static void Main(string[] args)
        {
            ShowAvailableDisplayInfo();
        }

        private static void ShowAvailableDisplayInfo()
        {
            InitDisplayInfo();

            for (int index = 0; index < displayNames.Count; index++)
            {
                Console.WriteLine($"Display index: {index} | Display name: {displayNames[index]}");
            }
        }

        private static void InitDisplayInfo()
        {
            DDCHelperAPI.DDCCIHelperIni_CS();
            var displayIdIndex = DDCHelperAPI.EnumDisplayIDIni_CS(ref displayNames, ref currentDisplayName);
        }

        private static void SetDisplayBrightness(int displayIndex, int brightness)
        {
            InitDisplayInfo();
            DDCHelperAPI.setStandardDDCCIValue_CS(displayIndex, CSDDCHelperAPILib.VCPCodeCmd.CGMenuStdVCPCmd.eVCPOpCode_E.OP_10_Luminance, brightness);
        }
    }
}
