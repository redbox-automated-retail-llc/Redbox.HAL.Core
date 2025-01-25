using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;
using System;
using System.Runtime.InteropServices;

namespace Redbox.HAL.Core.Descriptors
{
    internal abstract class Abstract3MCommand
    {
        protected readonly byte[] ErrorResponse = new byte[0];
        protected readonly byte[] Command;

        protected virtual int CommandSize => 8;

        protected abstract byte[] OnReadResponse(SafeFileHandle handle);

        protected abstract byte OnFillIndex(int idx);

        protected Abstract3MCommand()
        {
            this.Command = new byte[this.CommandSize];
            for (int idx = 0; idx < this.Command.Length; ++idx)
                this.Command[idx] = this.OnFillIndex(idx);
        }

        protected SafeFileHandle Connect()
        {
            SafeFileHandle file = Redbox.HAL.Component.Model.Interop.Win32.CreateFile("\\\\.\\TwTouchDevice1", Redbox.HAL.Component.Model.Interop.Win32.AccessFlags.GENERIC_READ_WRITE, Redbox.HAL.Component.Model.Interop.Win32.ShareFlags.FILE_SHARE_READ_WRITE, IntPtr.Zero, 3U, 128U, IntPtr.Zero);
            if (file.IsInvalid)
                LogHelper.Instance.Log("[MicrotouchDescriptor] CreateFile failed: Error = {0}", (object)Marshal.GetLastWin32Error());
            return file;
        }

        protected bool Write(SafeFileHandle handle, byte[] command)
        {
            bool flag = true;
            if (!Redbox.HAL.Component.Model.Interop.Win32.WriteFile(handle, command, command.Length, out int _, IntPtr.Zero))
            {
                LogHelper.Instance.Log("[{0}] WriteFile failed: Error = {1}", (object)this.GetType().Name, (object)Marshal.GetLastWin32Error());
                flag = false;
            }
            return flag;
        }

        protected byte[] Read(SafeFileHandle handle, int responseLength)
        {
            byte[] lpBuffer = new byte[responseLength];
            if (!Redbox.HAL.Component.Model.Interop.Win32.ReadFile(handle, lpBuffer, lpBuffer.Length, out int _, IntPtr.Zero))
            {
                lpBuffer = this.ErrorResponse;
                LogHelper.Instance.Log("[{0}] ReadFile failed: Error = {1}", (object)this.GetType().Name, (object)Marshal.GetLastWin32Error());
            }
            return lpBuffer;
        }

        internal byte[] SendReceive()
        {
            using (SafeFileHandle handle = this.Connect())
                return handle.IsInvalid ? this.ErrorResponse : (!this.Write(handle, this.Command) ? this.ErrorResponse : this.OnReadResponse(handle));
        }
    }
}
