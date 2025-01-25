using Redbox.HAL.Component.Model;
using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;

namespace Redbox.HAL.Core
{
    public sealed class Computerinfo
    {
        public long DiskFreeSpace { get; private set; }

        public long InstalledMemory { get; private set; }

        public string Manufacturer { get; private set; }

        public string Model { get; private set; }

        public Computerinfo()
        {
            foreach (ManagementObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
            {
                this.Manufacturer = instance[nameof(Manufacturer)].ToString();
                this.Model = instance[nameof(Model)].ToString();
            }
            long totalMemoryInKilobytes;
            if (Computerinfo.GetPhysicallyInstalledSystemMemory(out totalMemoryInKilobytes))
                this.InstalledMemory = totalMemoryInKilobytes;
            else
                LogHelper.Instance.Log("[{0}] Unable to obtain memory: GLE = {1}", (object)this.GetType().Name, (object)Marshal.GetLastWin32Error());
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.Name.StartsWith("c", StringComparison.CurrentCultureIgnoreCase) && drive.IsReady)
                    this.DiskFreeSpace = drive.TotalFreeSpace;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);
    }
}
