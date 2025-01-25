using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core
{
    internal sealed class QueryUsbDeviceResult : IQueryUsbDeviceResult
    {
        public override string ToString()
        {
            return !this.Found ? "UNKNOWN device" : string.Format("{0} ( {1} ) status = {2}", (object)this.Descriptor, (object)this.Descriptor.Friendlyname, (object)this.Status);
        }

        public IDeviceDescriptor Descriptor { get; private set; }

        public bool Found => (this.Status & DeviceStatus.Found) != 0;

        public DeviceStatus Status { get; internal set; }

        public bool IsDisabled
        {
            get
            {
                this.ThrowIfNotFound();
                return (DeviceStatus.Disabled & this.Status) != 0;
            }
        }

        public bool IsNotStarted
        {
            get
            {
                this.ThrowIfNotFound();
                return (this.Status & DeviceStatus.NotStarted) != 0;
            }
        }

        public bool Running
        {
            get
            {
                return (this.Status & DeviceStatus.Found) != DeviceStatus.None && (this.Status & DeviceStatus.Enabled) != 0;
            }
        }

        internal QueryUsbDeviceResult(IDeviceDescriptor descriptor)
        {
            this.Status = DeviceStatus.None;
            this.Descriptor = descriptor;
        }

        private void ThrowIfNotFound()
        {
            if (!this.Found)
                throw new InvalidOperationException("Device not found");
        }
    }
}
