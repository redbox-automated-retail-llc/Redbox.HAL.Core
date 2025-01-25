using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class GenericTouchscreenDescriptor : AbstractTouchscreenDescriptor
    {
        protected override bool OnResetDriver() => throw new NotImplementedException("ResetDriver");

        protected override DeviceStatus OnGetStatus() => throw new NotImplementedException("GetStatus");

        protected override bool OnMatchDriver()
        {
            return this.OnMatchDriver((IDeviceDescriptor)this, this.DriverDesc);
        }

        internal GenericTouchscreenDescriptor(
          string v,
          string p,
          string f,
          IDriverDescriptor desc,
          IUsbDeviceService s,
          DeviceClass clazz)
          : base(s, clazz, desc)
        {
            this.Vendor = v;
            this.Product = p;
            this.Friendlyname = f;
        }
    }
}
