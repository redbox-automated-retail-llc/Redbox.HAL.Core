using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class IdTechRev2 : AbstractDeviceDescriptor
    {
        internal IdTechRev2(IUsbDeviceService ds, IDeviceSetupClassFactory factory)
          : base(ds, DeviceClass.HIDClass)
        {
            this.Vendor = "0ACD";
            this.Product = "0520";
            this.Friendlyname = "ID Tech Dual";
        }
    }
}
