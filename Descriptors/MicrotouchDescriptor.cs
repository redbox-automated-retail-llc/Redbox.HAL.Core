using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class MicrotouchDescriptor : AbstractTouchscreenDescriptor
    {
        protected override bool OnSoftReset() => new _3mSoftResetCommand().Reset();

        protected override bool OnHardReset() => this.OnResetDriver(2000);

        protected override string OnReadFirmware() => new _3mFirmwareCommand().GetFirmwareRevision();

        protected override DeviceStatus OnGetStatus() => throw new NotImplementedException();

        protected override bool OnMatchDriver()
        {
            return this.OnMatchDriver((IDeviceDescriptor)this, this.DriverDesc);
        }

        internal MicrotouchDescriptor(IUsbDeviceService s, DeviceClass clazz, IDriverDescriptor desc)
          : base(s, clazz, desc)
        {
            this.Vendor = "0596";
            this.Product = "0001";
            this.Friendlyname = "3M";
        }
    }
}
