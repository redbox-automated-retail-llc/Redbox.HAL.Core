using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;
using System;
using System.IO.Ports;

namespace Redbox.HAL.Core
{
    internal sealed class CallbackReadPort : RedboxSerialPort
    {
        private readonly AtomicFlag ReadingFlag = new AtomicFlag();
        private ChannelResponse Current;
        private byte[] ReadBuffer;

        protected override void OnPreOpenPort()
        {
            this.Port.DataReceived += new SerialDataReceivedEventHandler(this.Port_DataReceived);
            this.ReadBuffer = new byte[this.Port.ReadBufferSize];
        }

        protected override void OnPortClosed() => this.ReadBuffer = (byte[])null;

        protected override IChannelResponse OnSendReceive(byte[] command, int readTimeout)
        {
            ChannelResponse channelResponse = new ChannelResponse();
            this.Current = channelResponse;
            try
            {
                try
                {
                    if (!this.ReadingFlag.Set())
                    {
                        LogHelper.Instance.Log(LogEntryType.Error, "[CallbackReadPort, {0}] The port is already reading.", (object)this.DisplayName);
                        channelResponse.Error = ErrorCodes.CommunicationError;
                        return (IChannelResponse)channelResponse;
                    }
                    this.Port.Write(command, 0, command.Length);
                    if (this.WriteTerminator != null)
                        this.Port.Write(this.WriteTerminator, 0, this.WriteTerminator.Length);
                    this.RuntimeService.SpinWait(this.WritePause);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log(string.Format("[CallbackReadPort, {0}] Write caught an exception.", (object)this.DisplayName), ex);
                    channelResponse.Error = ErrorCodes.CommunicationError;
                    return (IChannelResponse)channelResponse;
                }
                if (!channelResponse.Wait(readTimeout))
                {
                    channelResponse.Error = ErrorCodes.CommunicationError;
                    LogHelper.Instance.Log(LogEntryType.Error, "[CallbackReadPort, {0}] Communication ERROR: port read timed out", (object)this.DisplayName);
                }
                return (IChannelResponse)channelResponse;
            }
            finally
            {
                this.Current = (ChannelResponse)null;
                this.ReadingFlag.Clear();
            }
        }

        internal CallbackReadPort(SerialPort port)
          : base(port)
        {
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = this.Port.BytesToRead;
                if (this.EnableDebugging)
                    LogHelper.Instance.Log("[CallbackReadPort, {0}] data received bytes = {1} args = {2}", (object)this.DisplayName, (object)bytesToRead, (object)e.EventType);
                if (bytesToRead == 0 || this.Current == null)
                    return;
                int num = this.Port.Read(this.ReadBuffer, 0, bytesToRead);
                if (num != bytesToRead)
                    LogHelper.Instance.Log("[CallbackReadPort, {0}] !! The bytes to read {1} doesn't match the read count {2} !!", (object)this.DisplayName, (object)num, (object)bytesToRead);
                if (!this.ReadingFlag.IsSet)
                    return;
                this.Current.Accumulate(this.ReadBuffer, bytesToRead);
                if (!this.ValidateResponse((IChannelResponse)this.Current))
                    return;
                this.Current.ReadEnd();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[CallbackReadPort, {0}] data received caught an exception.", (object)this.DisplayName), ex);
            }
        }
    }
}
