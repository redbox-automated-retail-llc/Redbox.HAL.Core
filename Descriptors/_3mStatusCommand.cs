using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.Core.Descriptors
{
    internal sealed class _3mStatusCommand : Abstract3MCommand
    {
        private readonly byte ResponseSize = 20;

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
                    num = (byte)6;
                    break;
                case 6:
                    num = this.ResponseSize;
                    break;
            }
            return num;
        }
    }
}
