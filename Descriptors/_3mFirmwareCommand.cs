using Microsoft.Win32.SafeHandles;
using System;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class _3mFirmwareCommand : Abstract3MCommand
    {
        private readonly byte ResponseSize = 24;

        protected override byte[] OnReadResponse(SafeFileHandle handle)
        {
            return this.Read(handle, (int)this.ResponseSize);
        }

        protected override byte OnFillIndex(int idx)
        {
            byte num = 0;
            switch (idx)
            {
                case 0:
                    num = (byte)192;
                    break;
                case 1:
                    num = (byte)10;
                    break;
                case 6:
                    num = this.ResponseSize;
                    break;
            }
            return num;
        }

        internal string GetFirmwareRevision()
        {
            byte[] numArray = this.SendReceive();
            return numArray.Length != 0 ? string.Format("{0}.{1}", (object)BitConverter.ToString(numArray, 3, 1), (object)BitConverter.ToString(numArray, 4, 1)) : "UNKNOWN";
        }

        internal _3mFirmwareCommand()
        {
        }
    }
}
