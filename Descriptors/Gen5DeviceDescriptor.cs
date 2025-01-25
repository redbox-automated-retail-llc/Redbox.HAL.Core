using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class Gen5DeviceDescriptor : AbstractDeviceDescriptor
    {
        private readonly IDriverDescriptor DriverInfo = (IDriverDescriptor)new DriverDescriptor("2.0.0.2", "Scanner Manufacturer");

        protected override bool OnResetDriver() => false;

        protected override bool OnLocate()
        {
            DeviceStatus status = this.GetStatus();
            return (status & DeviceStatus.Found) != DeviceStatus.None && (status & DeviceStatus.Enabled) != 0;
        }

        protected override bool OnMatchDriver()
        {
            return this.OnMatchDriver((IDeviceDescriptor)this, this.DriverInfo);
        }

        internal Gen5DeviceDescriptor(IUsbDeviceService s)
          : base(s, DeviceClass.USB)
        {
            this.Vendor = "11FA";
            this.Product = "0204";
            this.Friendlyname = "Cortex Scanner";
        }
    }
}
