using ArgumentParser;
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

        // Physical monitor handles + metadata for the currently enumerated displays.
        // Index in this list matches the display index shown by --info and used by --brightness.
        private static readonly List<MonitorEntry> _monitors = new List<MonitorEntry>();

        public static void ShowAvailableDisplayInfo()
        {
            try
            {
                InitDisplayInfo();

                for (int index = 0; index < displayNames.Count; index++)
                {
                    Console.WriteLine($"Display index: {index} | Display name: {displayNames[index]}");
                }
            }
            finally
            {
                DisposeMonitors();
            }
        }

        public static void InitDisplayInfo()
        {
            DisposeMonitors();
            EnumeratePhysicalMonitors();

            displayNames = new List<string>();
            foreach (var monitor in _monitors)
            {
                displayNames.Add(monitor.Name);
            }
        }

        public static void SetAllDisplayBrightness(int brightness)
        {
            try
            {
                InitDisplayInfo();

                for (int index = 0; index < _monitors.Count; index++)
                {
                    if (_monitors[index].IsPhilips)
                    {
                        SetPhilipsBrightness(_monitors[index], brightness);
                    }
                    else
                    {
                        SetBrightnessForNonPhilipsDisplay(brightness);
                    }
                }
            }
            finally
            {
                DisposeMonitors();
            }
        }

        public static void SetDisplayBrightnessIndividually(string[] parameters)
        {
            try
            {
                InitDisplayInfo();

                foreach (var parameter in parameters)
                {
                    var values = parameter.Split(Parser.KeyValueDelimiter);
                    var displayIndex = Convert.ToInt32(values[0]);
                    var displayBrightness = Convert.ToInt32(values[1]);

                    if (displayIndex < 0 || displayIndex >= _monitors.Count)
                    {
                        Console.WriteLine($"Display index {displayIndex} is out of range.");
                        continue;
                    }

                    if (_monitors[displayIndex].IsPhilips)
                    {
                        SetPhilipsBrightness(_monitors[displayIndex], displayBrightness);
                    }
                    else
                    {
                        SetBrightnessForNonPhilipsDisplay(displayBrightness);
                    }
                }
            }
            finally
            {
                DisposeMonitors();
            }
        }

        // Sets brightness on an external DDC/CI monitor via VCP code 0x10 (Luminance).
        private static void SetPhilipsBrightness(MonitorEntry monitor, int brightness)
        {
            NativeMethods.SetVCPFeature(monitor.Handle, NativeMethods.VcpLuminance, (uint)brightness);
        }

        // Internal/laptop panels do not speak DDC/CI; the OS exposes their backlight via WMI.
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
                    managementObject.InvokeMethod(mehodName, new object[] { uint.MaxValue, (byte)brightness });
                    break;
                }
            }
        }

        // Walks every physical monitor on the system, building the index -> handle mapping
        // and a display name that carries the PnP id (so the "PHL" Philips check still works).
        private static void EnumeratePhysicalMonitors()
        {
            // The delegate must stay rooted for the duration of the (synchronous) enumeration.
            NativeMethods.MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdc, ref NativeMethods.RECT rect, IntPtr data) =>
            {
                var info = new NativeMethods.MONITORINFOEX { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFOEX>() };
                if (!NativeMethods.GetMonitorInfo(hMonitor, ref info))
                {
                    return true;
                }

                bool isPrimary = (info.dwFlags & NativeMethods.MONITORINFOF_PRIMARY) != 0;

                // Friendly name + PnP hardware id (e.g. MONITOR\PHLxxxx\...) from the child monitor device.
                string friendlyName = info.szDevice;
                string pnpId = string.Empty;
                var device = new NativeMethods.DISPLAY_DEVICE { cb = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.DISPLAY_DEVICE>() };
                if (NativeMethods.EnumDisplayDevices(info.szDevice, 0, ref device, 0))
                {
                    if (!string.IsNullOrWhiteSpace(device.DeviceString))
                    {
                        friendlyName = device.DeviceString;
                    }
                    pnpId = device.DeviceID ?? string.Empty;
                }

                if (isPrimary)
                {
                    currentDisplayName = friendlyName;
                }

                if (!NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint count) || count == 0)
                {
                    return true;
                }

                var physicalMonitors = new NativeMethods.PHYSICAL_MONITOR[count];
                if (!NativeMethods.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors))
                {
                    return true;
                }

                bool isPhilips = pnpId.IndexOf(PhilipsDisplayName, StringComparison.OrdinalIgnoreCase) >= 0
                    || (friendlyName != null && friendlyName.IndexOf(PhilipsDisplayName, StringComparison.OrdinalIgnoreCase) >= 0);

                foreach (var physical in physicalMonitors)
                {
                    string description = string.IsNullOrWhiteSpace(friendlyName)
                        ? physical.szPhysicalMonitorDescription
                        : friendlyName;

                    string name = string.IsNullOrWhiteSpace(pnpId)
                        ? description
                        : $"{description} [{pnpId}]";

                    _monitors.Add(new MonitorEntry(physical.hPhysicalMonitor, name, isPhilips));
                }

                return true;
            };

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            GC.KeepAlive(callback);
        }

        private static void DisposeMonitors()
        {
            foreach (var monitor in _monitors)
            {
                if (monitor.Handle != IntPtr.Zero)
                {
                    NativeMethods.DestroyPhysicalMonitor(monitor.Handle);
                }
            }
            _monitors.Clear();
        }

        private readonly struct MonitorEntry
        {
            public MonitorEntry(IntPtr handle, string name, bool isPhilips)
            {
                Handle = handle;
                Name = name;
                IsPhilips = isPhilips;
            }

            public IntPtr Handle { get; }
            public string Name { get; }
            public bool IsPhilips { get; }
        }
    }
}
