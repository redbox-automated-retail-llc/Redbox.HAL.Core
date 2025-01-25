using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;
using System.IO.Ports;
using System.Text;

namespace Redbox.HAL.Core
{
    internal abstract class RedboxSerialPort : ICommPort, IDisposable
    {
        protected readonly SerialPort Port;
        protected readonly IRuntimeService RuntimeService;
        private string m_displayName;
        private bool Disposed;

        public bool IsOpen => this.Port != null && this.PortIsOpen();

        public string PortName
        {
            get => this.Port != null ? this.Port.PortName : throw new NullReferenceException();
        }

        public bool EnableDebugging { get; set; }

        public byte[] WriteTerminator { get; set; }

        public int OpenPause { get; set; }

        public int WritePause { get; set; }

        public string DisplayName
        {
            get => this.m_displayName;
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.m_displayName = (string)this.Port.PortName.Clone();
                else
                    this.m_displayName = string.Format("{0} ( {1} )", (object)this.Port.PortName, (object)value);
            }
        }

        public CommPortReadModes Mode { get; internal set; }

        public int ReadTimeout { get; set; }

        public int WriteTimeout { get; set; }

        public int? ReceiveBufferSize { get; set; }

        public Predicate<IChannelResponse> ValidateResponse { get; set; }

        public void Dispose()
        {
            if (this.EnableDebugging)
                LogHelper.Instance.Log("[RedboxSerialPort, {0}] dispose port", (object)this.DisplayName);
            this.Dispose(true);
        }

        ~RedboxSerialPort()
        {
            if (this.EnableDebugging)
                LogHelper.Instance.Log("[RedboxSerialPort, {0}] finalize port ", (object)this.DisplayName);
            this.Dispose(false);
        }

        public void Configure(ICommChannelConfiguration configuration)
        {
            this.ReceiveBufferSize = configuration.ReceiveBufferSize;
            this.WriteTerminator = configuration.WriteTerminator;
            this.WritePause = configuration.WritePause;
            this.WriteTimeout = configuration.WriteTimeout;
            this.OpenPause = configuration.OpenPause;
            this.ReadTimeout = configuration.ReadTimeout;
            if (configuration.ReceiveBufferSize.HasValue)
                this.Port.ReadBufferSize = this.ReceiveBufferSize.Value;
            this.OnConfigure();
        }

        public bool Open()
        {
            if (this.Port == null)
            {
                LogHelper.Instance.Log("[RedboxSerialPort] no port is configured.");
                return false;
            }
            if (this.PortIsOpen())
                return true;
            this.OnPreOpenPort();
            try
            {
                this.Port.Open();
                this.Port.ReadTimeout = this.ReadTimeout;
                this.Port.WriteTimeout = this.WriteTimeout;
                this.Port.RtsEnable = true;
                this.Port.DtrEnable = true;
                this.RuntimeService.Wait(this.OpenPause);
                LogHelper.Instance.Log("[RedboxSerialPort, {0}] Port is open in mode {1}.", (object)this.DisplayName, (object)this.Mode);
                this.OnPortOpen();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[RedbosSerialPort, {0}] Open() caught an exception.", (object)this.DisplayName), ex);
                return false;
            }
            if (this.ResetPortBuffers(true))
                return true;
            this.Close();
            return false;
        }

        public bool Close()
        {
            if (!this.Port.IsOpen)
                return false;
            try
            {
                this.Port.RtsEnable = true;
                this.Port.DtrEnable = false;
                this.Port.Close();
                LogHelper.Instance.Log("[RedboxSerialPort, {0}] Port is closed.", (object)this.DisplayName);
                this.OnPortClosed();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[RedboxSerialPort, {0}] Close caught an exception.", (object)this.DisplayName), ex);
                return false;
            }
        }

        public IChannelResponse SendRecv(byte[] bytes, int readTimeout)
        {
            this.ResetPortBuffers(true);
            if (!this.Port.IsOpen)
            {
                LogHelper.Instance.Log(LogEntryType.Error, "[RedboxSerialPort, {0}] Send/Recv: the port is not open.", (object)this.DisplayName);
                return (IChannelResponse)new ChannelResponse()
                {
                    Error = ErrorCodes.CommunicationError
                };
            }
            if (this.EnableDebugging)
                LogHelper.Instance.Log("[RedboxSerialPort, {0}] Send command {1}", (object)this.DisplayName, (object)bytes.AsString());
            IChannelResponse response = this.OnSendReceive(bytes, readTimeout);
            this.LogResponse(response);
            return response;
        }

        public IChannelResponse SendRecv(string command, int readTimeout)
        {
            this.ResetPortBuffers(true);
            if (!this.Port.IsOpen)
            {
                LogHelper.Instance.Log(LogEntryType.Error, "[RedboxSerialPort, {0}] Send/Recv: the port is not open.", (object)this.DisplayName);
                return (IChannelResponse)new ChannelResponse()
                {
                    Error = ErrorCodes.CommunicationError
                };
            }
            if (this.EnableDebugging)
                LogHelper.Instance.Log("[RedboxSerialPort, {0}] Send command {1}", (object)this.DisplayName, (object)command);
            IChannelResponse response = this.OnSendReceive(Encoding.ASCII.GetBytes(command), readTimeout);
            this.LogResponse(response);
            return response;
        }

        protected virtual void OnPreOpenPort()
        {
        }

        protected virtual void OnPortOpen()
        {
        }

        protected virtual void OnConfigure()
        {
        }

        protected virtual void OnPortClosed()
        {
        }

        protected virtual void OnDispose()
        {
        }

        protected abstract IChannelResponse OnSendReceive(byte[] command, int timeout);

        protected bool ResetPortBuffers(bool resetOut)
        {
            try
            {
                if (!this.Port.IsOpen)
                {
                    LogHelper.Instance.Log("[RedbosSerialPort, {0}] ResetPortBuffers: port is closed.", (object)this.DisplayName);
                    return false;
                }
                this.Port.DiscardInBuffer();
                if (resetOut)
                    this.Port.DiscardOutBuffer();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[RedbosSerialPort] Reset buffer caught an exception.", (object)this.DisplayName), ex);
                return false;
            }
        }

        protected bool PortIsOpen()
        {
            try
            {
                return this.Port.IsOpen;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(LogEntryType.Error, string.Format("[RedboxCommPort, {0}] IsOpen generated an unhandled exception.", (object)this.DisplayName), (object)ex);
                return false;
            }
        }

        protected RedboxSerialPort(SerialPort port)
        {
            this.Port = port != null ? port : throw new ArgumentNullException(nameof(port));
            this.RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
            this.WritePause = 10;
            this.ReadTimeout = -1;
            this.WriteTimeout = 5000;
            this.OpenPause = 3000;
            this.m_displayName = this.Port.PortName;
        }

        private void LogResponse(IChannelResponse response)
        {
            if (!this.EnableDebugging && response.CommOk)
                return;
            LogHelper.Instance.Log("[RedboxCommPort, {0}] Read {1}, bytes in buffer {2}", (object)this.DisplayName, !response.CommOk ? (object)"timed out" : (object)"ok", (object)response.RawResponse.Length);
            response.RawResponse.Dump();
        }

        private void Dispose(bool fromDispose)
        {
            if (this.Disposed)
                return;
            this.Disposed = true;
            this.OnDispose();
            this.Close();
            this.Port.Dispose();
            GC.SuppressFinalize((object)this);
        }
    }
}
