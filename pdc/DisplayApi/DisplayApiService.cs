using CSDDCHelperAPILib;
using System;
using System.Collections.Generic;
using System.Management;

namespace pdc.DisplayApi
{
    public static class DisplayApiService
    {
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

        public static void SetDisplayBrightness(int displayIndex, int brightness)
        {
            InitDisplayInfo();
            DDCHelperAPI.setStandardDDCCIValue_CS(displayIndex, CSDDCHelperAPILib.VCPCodeCmd.CGMenuStdVCPCmd.eVCPOpCode_E.OP_10_Luminance, brightness);
        }

        public static void SetAllDisplayBrightness(int brightness)
        {
            InitDisplayInfo();

            var indexes = new List<int>();

            for (int index = 0; index < displayNames.Count; index++)
            {
                if (displayNames[index].Contains("PHL"))
                {
                    indexes.Add(index);
                }
                else
                {
                    SetBrightnessForNonPhilipsDisplay(brightness);
                }
            }

            foreach (var index in indexes)
            {
                DDCHelperAPI.setStandardDDCCIValue_CS(index, CSDDCHelperAPILib.VCPCodeCmd.CGMenuStdVCPCmd.eVCPOpCode_E.OP_10_Luminance, brightness);
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
