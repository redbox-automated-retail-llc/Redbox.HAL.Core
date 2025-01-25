using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core
{
    public sealed class DeviceSetupClassFactory : IDeviceSetupClassFactory
    {
        private readonly Dictionary<DeviceClass, IDeviceSetupClass> ClassMap = new Dictionary<DeviceClass, IDeviceSetupClass>();

        public IDeviceSetupClass Get(DeviceClass clazz)
        {
            return !this.ClassMap.ContainsKey(clazz) ? (IDeviceSetupClass)null : this.ClassMap[clazz];
        }

        public DeviceSetupClassFactory()
        {
            this.ClassMap[DeviceClass.Ports] = (IDeviceSetupClass)new DeviceSetupClassFactory.PortsSetupClass();
            this.ClassMap[DeviceClass.USB] = (IDeviceSetupClass)new DeviceSetupClassFactory.UsbSetupClass();
            this.ClassMap[DeviceClass.Image] = (IDeviceSetupClass)new DeviceSetupClassFactory.ImageSetupClass();
            this.ClassMap[DeviceClass.Monitor] = (IDeviceSetupClass)new DeviceSetupClassFactory.MonitorSetupClass();
            this.ClassMap[DeviceClass.HIDClass] = (IDeviceSetupClass)new DeviceSetupClassFactory.HidSetupClass();
            this.ClassMap[DeviceClass.Mouse] = (IDeviceSetupClass)new DeviceSetupClassFactory.MouseSetupClass();
            this.ClassMap[DeviceClass.Battery] = (IDeviceSetupClass)new DeviceSetupClassFactory.BatterySetupClass();
            this.ClassMap[DeviceClass.None] = (IDeviceSetupClass)new DeviceSetupClassFactory.NoSetupClass();
        }

        private class NoSetupClass : IDeviceSetupClass
        {
            public DeviceClass Class => DeviceClass.None;

            public Guid Guid => Guid.Empty;
        }

        private class PortsSetupClass : IDeviceSetupClass
        {
            private readonly Guid Ports = new Guid("4D36E978-E325-11CE-BFC1-08002BE10318");

            public DeviceClass Class => DeviceClass.Ports;

            public Guid Guid => this.Ports;
        }

        private class UsbSetupClass : IDeviceSetupClass
        {
            private readonly Guid USB = new Guid("36FC9E60-C465-11CF-8056-444553540000");

            public DeviceClass Class => DeviceClass.USB;

            public Guid Guid => this.USB;
        }

        private class ImageSetupClass : IDeviceSetupClass
        {
            private readonly Guid Image = new Guid("6BDD1FC6-810F-11D0-BEC7-08002BE2092F");

            public DeviceClass Class => DeviceClass.Image;

            public Guid Guid => this.Image;
        }

        private class MonitorSetupClass : IDeviceSetupClass
        {
            private readonly Guid Monitor = new Guid("4d36e96e-e325-11ce-bfc1-08002be10318");

            public DeviceClass Class => DeviceClass.Monitor;

            public Guid Guid => this.Monitor;
        }

        private class HidSetupClass : IDeviceSetupClass
        {
            private readonly Guid HID = new Guid("745a17a0-74d3-11d0-b6fe-00a0c90f57da");

            public DeviceClass Class => DeviceClass.HIDClass;

            public Guid Guid => this.HID;
        }

        private class MouseSetupClass : IDeviceSetupClass
        {
            private readonly Guid Mouse = new Guid("4D36E96F-E325-11CE-BFC1-08002BE10318");

            public DeviceClass Class => DeviceClass.Mouse;

            public Guid Guid => this.Mouse;
        }

        private class BatterySetupClass : IDeviceSetupClass
        {
            private readonly Guid Batt = new Guid("72631e54-78a4-11d0-bcf7-00aa00b7b32a");

            public DeviceClass Class => DeviceClass.Battery;

            public Guid Guid => this.Batt;
        }
    }
}
