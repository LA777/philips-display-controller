using ArgumentParser;
using CSDDCHelperAPILib;
using System;
using System.Collections.Generic;
using System.Management;

namespace pdc.DisplayApi
{
    public static class DisplayApiService
    {
        private const string PhilipsDisplayName = "PHL";
        public static List<string> displayNames = new List<string>();
        public static string currentDisplayName = string.Empty;

        public static void ShowAvailableDisplayInfo()
        {
            InitDisplayInfo();

            for (int index = 0; index < displayNames.Count; index++)
            {
                Console.WriteLine($"Display index: {index} | Display name: {displayNames[index]}");
            }
        }

        public static void InitDisplayInfo()
        {
            DDCHelperAPI.DDCCIHelperIni_CS();
            DDCHelperAPI.EnumDisplayIDIni_CS(ref displayNames, ref currentDisplayName);
        }

        public static void SetAllDisplayBrightness(int brightness)
        {
            InitDisplayInfo();

            for (int index = 0; index < displayNames.Count; index++)
            {
                if (displayNames[index].Contains(PhilipsDisplayName))
                {
                    DDCHelperAPI.setStandardDDCCIValue_CS(index, CSDDCHelperAPILib.VCPCodeCmd.CGMenuStdVCPCmd.eVCPOpCode_E.OP_10_Luminance, brightness);
                }
                else
                {
                    SetBrightnessForNonPhilipsDisplay(brightness);
                }
            }
        }

        public static void SetDisplayBrightnessIndividually(string[] parameters)
        {
            InitDisplayInfo();

            foreach (var parameter in parameters)
            {
                var values = parameter.Split(Parser.KeyValueDelimiter);
                var displayIndex = Convert.ToInt32(values[0]);
                var displayBrightness = Convert.ToInt32(values[1]);

                if (displayNames[displayIndex].Contains(PhilipsDisplayName))
                {
                    DDCHelperAPI.setStandardDDCCIValue_CS(displayIndex, CSDDCHelperAPILib.VCPCodeCmd.CGMenuStdVCPCmd.eVCPOpCode_E.OP_10_Luminance, displayBrightness);
                }
                else
                {
                    SetBrightnessForNonPhilipsDisplay(displayBrightness);
                }
            }
        }

        public static void SetBrightnessForNonPhilipsDisplay(int brightness)
        {
            const string scope = "root\\WMI";
            var managementScope = new ManagementScope(scope);

            const string query = "WmiMonitorBrightnessMethods";
            var selectQuery = new SelectQuery(query);

            const string mehodName = "WmiSetBrightness";
            using (var managementObjectSearcher = new ManagementObjectSearcher(managementScope, selectQuery))
            using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
            {
                foreach (ManagementObject managementObject in managementObjectCollection)
                {
                    managementObject.InvokeMethod(mehodName, new Object[] { UInt32.MaxValue, (byte)brightness });
                    break;
                }
            }
        }
    }
}
