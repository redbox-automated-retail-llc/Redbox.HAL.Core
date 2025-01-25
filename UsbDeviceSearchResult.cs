using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core
{
    internal sealed class UsbDeviceSearchResult : IUsbDeviceSearchResult, IDisposable
    {
        private bool Disposed;

        public void Dispose()
        {
            if (this.Disposed)
                return;
            this.Disposed = true;
            this.Errors.Clear();
            this.Matches.Clear();
        }

        public bool Found => this.Errors.Count == 0 && this.Matches.Count > 0;

        public ErrorList Errors { get; private set; }

        public List<IDeviceDescriptor> Matches { get; private set; }

        internal UsbDeviceSearchResult()
        {
            this.Errors = new ErrorList();
            this.Matches = new List<IDeviceDescriptor>();
        }
    }
}
