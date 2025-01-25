using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Redbox.HAL.Core
{
    public sealed class PortManager : IPortManagerService
    {
        private const string MsgHeader = "[Port Scanner]";
        private readonly List<string> InUsePorts = new List<string>();
        private readonly object ScanLock = new object();

        public bool Register(ICommPort port)
        {
            lock (this.ScanLock)
                return this.RegisterUnderLock(port);
        }

        public void Dispose(ICommPort port)
        {
            lock (this.ScanLock)
            {
                this.InUsePorts.Remove(port.PortName.ToUpper());
                port.Dispose();
            }
        }

        public ICommChannelConfiguration CreateConfiguration()
        {
            return (ICommChannelConfiguration)new RedboxSerialPortConfiguration();
        }

        public ICommPort Create(SerialPort port) => this.Create(port, CommPortReadModes.Async);

        public ICommPort Create(SerialPort port, CommPortReadModes mode)
        {
            RedboxSerialPort port1 = port != null ? this.OnCreate(port, mode) : throw new ArgumentException(nameof(port));
            if (port1 != null)
                this.Register((ICommPort)port1);
            return (ICommPort)port1;
        }

        public ICommPort Scan(
          ICommChannelConfiguration conf,
          Predicate<ICommPort> probe,
          CommPortReadModes mode)
        {
            return this.Scan((string)null, conf, probe, mode);
        }

        public ICommPort Scan(
          string tryFirst,
          ICommChannelConfiguration conf,
          Predicate<ICommPort> probe,
          CommPortReadModes mode)
        {
            lock (this.ScanLock)
            {
                using (ExecutionTimer executionTimer = new ExecutionTimer())
                {
                    ICommPort port = (ICommPort)null;
                    if (tryFirst != null)
                        port = (ICommPort)this.Probe(tryFirst, conf, probe, mode);
                    if (port == null)
                        port = (ICommPort)this.ProbePorts(conf, probe, mode);
                    executionTimer.Stop();
                    LogHelper.Instance.Log("[Port Scan] Time to scan ports: {0}ms", (object)executionTimer.ElapsedMilliseconds);
                    if (port != null)
                        this.RegisterUnderLock(port);
                    return port;
                }
            }
        }

        private RedboxSerialPort Probe(
          string portName,
          ICommChannelConfiguration conf,
          Predicate<ICommPort> probe,
          CommPortReadModes mode)
        {
            if (this.InUsePorts.Find((Predicate<string>)(each => each.Equals(portName, StringComparison.CurrentCultureIgnoreCase))) != null)
            {
                LogHelper.Instance.Log("{0} The port {1} is reported in-use by the port manager.", (object)"[Port Scanner]", (object)portName);
                return (RedboxSerialPort)null;
            }
            RedboxSerialPort redboxSerialPort = this.OnCreate(new SerialPort(portName, 115200, Parity.None, 8, StopBits.One), mode);
            redboxSerialPort.Configure(conf);
            LogHelper.Instance.Log("{0} try port {1}", (object)"[Port Scanner]", (object)portName);
            if (!redboxSerialPort.Open())
            {
                LogHelper.Instance.Log("{0} Could not open port {1}", (object)"[Port Scanner]", (object)portName);
                redboxSerialPort.Dispose();
                return (RedboxSerialPort)null;
            }
            if (probe((ICommPort)redboxSerialPort))
            {
                LogHelper.Instance.Log("{0} Probe port {1} returned true", (object)"[Port Scanner]", (object)portName);
                return redboxSerialPort;
            }
            redboxSerialPort.Dispose();
            return (RedboxSerialPort)null;
        }

        private bool RegisterUnderLock(ICommPort port)
        {
            string key = port.PortName.ToUpper();
            if (this.InUsePorts.Find((Predicate<string>)(each => each.Equals(key, StringComparison.CurrentCultureIgnoreCase))) != null)
                return false;
            this.InUsePorts.Add(key);
            return true;
        }

        private RedboxSerialPort OnCreate(SerialPort port, CommPortReadModes mode)
        {
            RedboxSerialPort redboxSerialPort = (RedboxSerialPort)null;
            switch (mode)
            {
                case CommPortReadModes.Async:
                    AsyncReadPort asyncReadPort = new AsyncReadPort(port);
                    asyncReadPort.Mode = CommPortReadModes.Async;
                    redboxSerialPort = (RedboxSerialPort)asyncReadPort;
                    break;
                case CommPortReadModes.Callback:
                    CallbackReadPort callbackReadPort = new CallbackReadPort(port);
                    callbackReadPort.Mode = CommPortReadModes.Callback;
                    redboxSerialPort = (RedboxSerialPort)callbackReadPort;
                    break;
            }
            return redboxSerialPort;
        }

        private RedboxSerialPort ProbePorts(
          ICommChannelConfiguration conf,
          Predicate<ICommPort> probe,
          CommPortReadModes mode)
        {
            foreach (string portName in SerialPort.GetPortNames())
            {
                RedboxSerialPort redboxSerialPort = this.Probe(portName, conf, probe, mode);
                if (redboxSerialPort != null)
                    return redboxSerialPort;
            }
            return (RedboxSerialPort)null;
        }
    }
}
