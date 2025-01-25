using Redbox.HAL.Component.Model;
using System;
using System.IO.Ports;

namespace Redbox.HAL.Core
{
    internal sealed class AsyncReadPort : RedboxSerialPort
    {
        protected override IChannelResponse OnSendReceive(byte[] command, int readTimeout)
        {
            ChannelResponse response = new ChannelResponse(this.Port.ReadBufferSize);
            if (this.EnableDebugging)
                LogHelper.Instance.Log("[AsyncReadPort, {0}] SendReceive response id = {1}", (object)this.DisplayName, (object)response.ID);
            try
            {
                this.Port.Write(command, 0, command.Length);
                if (this.WriteTerminator != null)
                    this.Port.Write(this.WriteTerminator, 0, this.WriteTerminator.Length);
                this.RuntimeService.SpinWait(this.WritePause);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[AsyncReadPort, {0}] WriteCommand caught an exception.", (object)this.DisplayName), ex);
                response.Error = ErrorCodes.CommunicationError;
                return (IChannelResponse)response;
            }
            this.ReadInner(response);
            try
            {
                if (!response.Wait(readTimeout))
                {
                    LogHelper.Instance.Log(LogEntryType.Error, "[AsyncReadPort, {0}] Communication ERROR: port read timed out", (object)this.DisplayName);
                    response.Error = ErrorCodes.CommunicationError;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(LogEntryType.Error, string.Format("[AsyncPort, {0}] Send/Recv caught an exception.", (object)this.DisplayName), (object)ex);
                response.Error = ErrorCodes.CommunicationError;
            }
            return (IChannelResponse)response;
        }

        internal AsyncReadPort(SerialPort port)
          : base(port)
        {
        }

        private void ReadInner(ChannelResponse response)
        {
            if (this.EnableDebugging)
                LogHelper.Instance.Log("[AsyncReadPort, {0}] Read inner id = {1}", (object)this.DisplayName, (object)response.ID);
            try
            {
                this.Port.BaseStream.BeginRead(response.Buffer, 0, response.Buffer.Length, new AsyncCallback(this.EndReadCallback), (object)response);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[AsyncReadPort, {0}] ReadInner caught an exception id = {1}", (object)this.DisplayName, (object)response.ID), ex);
            }
        }

        private void EndReadCallback(IAsyncResult result)
        {
            ChannelResponse asyncState;
            int len;
            try
            {
                asyncState = (ChannelResponse)result.AsyncState;
                len = this.Port.BaseStream.EndRead(result);
                if (this.EnableDebugging)
                    LogHelper.Instance.Log("[AsyncReadPort, {0}] EndReadCallback bytesRead = {1}, current buffer size = {2} ID  = {3}", (object)this.DisplayName, (object)len, (object)asyncState.RawResponse.Length, (object)asyncState.ID);
                if (len == 0)
                {
                    asyncState.ReponseValid = this.ValidateResponse((IChannelResponse)asyncState);
                    asyncState.ReadEnd();
                    return;
                }
                this.Port.BaseStream.Flush();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[AsyncReadPort, {0}] EndReadCallback caught an exception.", (object)this.DisplayName), ex);
                return;
            }
            asyncState.Accumulate(len);
            if (!this.ValidateResponse((IChannelResponse)asyncState))
            {
                this.ReadInner(asyncState);
            }
            else
            {
                asyncState.ReponseValid = true;
                asyncState.ReadEnd();
            }
        }
    }
}
