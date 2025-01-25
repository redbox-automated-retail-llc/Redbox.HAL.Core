using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core.Descriptors
{
    internal abstract class AbstractTouchscreenDescriptor :
      AbstractDeviceDescriptor,
      ITouchscreenDescriptor,
      IDeviceDescriptor
    {
        protected readonly IDriverDescriptor DriverDesc;

        public bool SoftReset() => this.OnSoftReset();

        public bool HardReset() => this.OnHardReset();

        public string ReadFirmware() => this.OnReadFirmware();

        protected virtual bool OnSoftReset() => throw new NotImplementedException("SoftReset");

        protected virtual bool OnHardReset() => throw new NotImplementedException("HardReset");

        protected virtual string OnReadFirmware() => "UNKNOWN";

        internal AbstractTouchscreenDescriptor(
          IUsbDeviceService s,
          DeviceClass clazz,
          IDriverDescriptor desc)
          : base(s, clazz)
        {
            this.DriverDesc = desc;
        }
    }
}
