using Redbox.HAL.Component.Model;
using Redbox.HAL.Core.Descriptors;
using System;

namespace Redbox.HAL.Core
{
    public sealed class AbeDeviceDescriptor : AbstractDeviceDescriptor
    {
        private readonly bool Debug;

        public AbeDeviceDescriptor(IUsbDeviceService service)
          : this(service, false)
        {
        }

        public AbeDeviceDescriptor(IUsbDeviceService s, bool debug)
          : base(s, DeviceClass.HIDClass)
        {
            this.Debug = debug;
            this.Vendor = "2047";
            this.Product = "0902";
            this.Friendlyname = "ABE Device";
        }

        protected override bool OnResetDriver()
        {
            throw new NotImplementedException("AbeDeviceDescriptor::ResetDriver is not implemented.");
        }

        protected override DeviceStatus OnGetStatus()
        {
            throw new NotImplementedException("AbeDeviceDescriptor::GetStatus is not implemented.");
        }
    }
}
