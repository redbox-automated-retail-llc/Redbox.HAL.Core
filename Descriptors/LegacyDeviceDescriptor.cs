using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class LegacyDeviceDescriptor : AbstractDeviceDescriptor
    {
        private readonly IDriverDescriptor DriverDescriptor;

        protected override bool OnLocate() => (this.GetStatus() & DeviceStatus.Found) != 0;

        protected override bool OnMatchDriver()
        {
            return this.OnMatchDriver((IDeviceDescriptor)this, this.DriverDescriptor);
        }

        internal LegacyDeviceDescriptor(
          string vendor,
          string product,
          string friendly,
          IDriverDescriptor driver,
          IUsbDeviceService s)
          : base(s, DeviceClass.Image)
        {
            this.Vendor = vendor.ToLower();
            this.Product = product.ToLower();
            this.Friendlyname = friendly;
            this.DriverDescriptor = driver;
        }
    }
}
