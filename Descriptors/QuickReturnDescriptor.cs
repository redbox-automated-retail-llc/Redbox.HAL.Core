using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core.Descriptors
{
    public sealed class QuickReturnDescriptor : AbstractDeviceDescriptor
    {
        private readonly IDriverDescriptor Driver = (IDriverDescriptor)new DriverDescriptor("2.4.6.0", "FTDI");

        protected override bool OnResetDriver() => throw new NotImplementedException();

        protected override DeviceStatus OnGetStatus() => throw new NotImplementedException();

        protected override bool OnMatchDriver()
        {
            return this.OnMatchDriver((IDeviceDescriptor)this, this.Driver);
        }

        public QuickReturnDescriptor(IUsbDeviceService service)
          : base(service, DeviceClass.Ports)
        {
            this.Vendor = "0403";
            this.Product = "6001";
            this.Friendlyname = "Quick Return";
        }
    }
}
