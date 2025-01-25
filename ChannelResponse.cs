using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Redbox.HAL.Core
{
    internal sealed class ChannelResponse : IChannelResponse, IDisposable
    {
        internal readonly byte[] Buffer;
        internal readonly string ID;
        private bool Disposed;
        private readonly List<byte> PortResponse = new List<byte>();
        private readonly EventWaitHandle ReadEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private static int IDCount;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        ~ChannelResponse() => this.Dispose(false);

        public int GetIndex(byte b)
        {
            byte[] array = this.PortResponse.ToArray();
            if (array == null || array.Length == 0)
                return -1;
            for (int index = 0; index < array.Length; ++index)
            {
                if ((int)array[index] == (int)b)
                    return index;
            }
            return -1;
        }

        public void DumpToLog()
        {
            LogHelper.Instance.Log(" -- Core Response dump -- ");
            if (this.PortResponse.Count == 0)
            {
                LogHelper.Instance.Log(" there is no response to dump.");
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int index = 0; index < this.PortResponse.Count; ++index)
                {
                    stringBuilder.AppendFormat("{0:x2} ", (object)this.PortResponse[index]);
                    if (index % 16 == 0)
                        stringBuilder.AppendLine();
                }
                LogHelper.Instance.Log(stringBuilder.ToString());
            }
        }

        public bool CommOk => this.Error == ErrorCodes.Success;

        public ErrorCodes Error { get; internal set; }

        public byte[] RawResponse => this.PortResponse.ToArray();

        public bool ReponseValid { get; internal set; }

        internal void Accumulate(int len) => this.Accumulate(this.Buffer, len);

        internal void Accumulate(byte[] bytes, int len)
        {
            for (int index = 0; index < len; ++index)
                this.PortResponse.Add(bytes[index]);
        }

        internal bool Wait(int timeout) => this.ReadEvent.WaitOne(timeout);

        internal void ReadEnd()
        {
            if (this.Disposed)
                return;
            this.ReadEvent.Set();
        }

        internal ChannelResponse()
          : this(0)
        {
        }

        internal ChannelResponse(int bufferSize)
        {
            this.Error = ErrorCodes.Success;
            this.Buffer = bufferSize <= 0 ? (byte[])null : new byte[bufferSize];
            this.ID = string.Format("ChannelResponse-{0}", (object)Interlocked.Increment(ref ChannelResponse.IDCount));
        }

        private void Dispose(bool disposing)
        {
            if (this.Disposed)
                return;
            this.Disposed = true;
            if (!disposing)
                return;
            this.ReadEvent.Close();
            this.PortResponse.Clear();
        }
    }
}
