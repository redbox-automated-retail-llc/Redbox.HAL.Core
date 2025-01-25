using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors
{
    internal abstract class _3MResetCommand : Abstract3MCommand
    {
        private readonly _3MResetCommand.ResetType_3M Type;

        protected abstract byte ResetByte { get; }

        protected abstract byte CompletionByte { get; }

        protected override byte[] OnReadResponse(SafeFileHandle handle) => new byte[1];

        protected override byte OnFillIndex(int idx)
        {
            byte num = 0;
            switch (idx)
            {
                case 0:
                    num = (byte)64;
                    break;
                case 1:
                    num = (byte)7;
                    break;
                case 2:
                    num = this.ResetByte;
                    break;
            }
            return num;
        }

        protected _3MResetCommand(_3MResetCommand.ResetType_3M type) => this.Type = type;

        internal bool Reset()
        {
            return this.SendReceive().Length != 0 && this.GetStatus(this.Type == _3MResetCommand.ResetType_3M.Soft ? 750 : 15000);
        }

        private bool GetStatus(int pause)
        {
            IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
            bool status = false;
            _3mStatusCommand obj = new _3mStatusCommand();
            for (int index = 0; index < 5; ++index)
            {
                service.Wait(pause);
                byte[] numArray = obj.SendReceive();
                if (numArray.Length != 0)
                {
                    if ((int)numArray[3] == (int)this.CompletionByte)
                    {
                        status = true;
                        break;
                    }
                }
                else
                    break;
            }
            return status;
        }

        protected enum ResetType_3M
        {
            Soft,
            Hard,
        }
    }
}
