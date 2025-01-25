using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core.Descriptors
{
    public abstract class AbstractDeviceDescriptor : IDeviceDescriptor
    {
        protected readonly IUsbDeviceService UsbService;
        protected readonly IRuntimeService RuntimeService;

        public override string ToString()
        {
            return string.Format("vid_{0}&pid_{1}", (object)this.Vendor, (object)this.Product);
        }

        public bool ResetDriver() => this.OnResetDriver();

        public bool MatchDriver() => this.OnMatchDriver();

        public bool Locate() => this.OnLocate();

        public DeviceStatus GetStatus() => this.OnGetStatus();

        public string Vendor { get; protected set; }

        public string Product { get; protected set; }

        public string Friendlyname { get; protected set; }

        public IDeviceSetupClass SetupClass { get; private set; }

        protected virtual bool OnResetDriver() => this.OnResetDriver(1500);

        protected virtual bool OnLocate() => this.UsbService.FindDevice((IDeviceDescriptor)this).Found;

        protected virtual DeviceStatus OnGetStatus()
        {
            return this.UsbService.FindDeviceStatus((IDeviceDescriptor)this);
        }

        protected virtual bool OnMatchDriver() => throw new NotImplementedException();

        protected bool OnResetDriver(int pause)
        {
            if (!this.UsbService.ChangeByHWID((IDeviceDescriptor)this, DeviceState.Disable))
                return false;
            this.RuntimeService.Wait(pause);
            int num = this.UsbService.ChangeByHWID((IDeviceDescriptor)this, DeviceState.Enable) ? 1 : 0;
            this.RuntimeService.SpinWait(pause);
            return num != 0;
        }

        protected bool OnMatchDriver(IDeviceDescriptor desc, IDriverDescriptor driverDesc)
        {
            bool flag = this.UsbService.MatchDriver(desc, driverDesc);
            if (!flag)
                LogHelper.Instance.Log("[{0}] Could not match driver.", (object)this.GetType().Name);
            return flag;
        }

        protected AbstractDeviceDescriptor(IUsbDeviceService service, DeviceClass clazz)
        {
            this.UsbService = service;
            this.RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
            this.SetupClass = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>().Get(clazz);
        }
    }
}
