using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class IdTechRev1 : AbstractDeviceDescriptor
    {
        internal IdTechRev1(IUsbDeviceService ds, IDeviceSetupClassFactory factory)
          : base(ds, DeviceClass.HIDClass)
        {
            this.Vendor = "0ACD";
            this.Product = "0200";
            this.Friendlyname = "ID Tech Single";
        }
    }
}
