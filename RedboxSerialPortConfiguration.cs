using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core
{
    internal sealed class RedboxSerialPortConfiguration : ICommChannelConfiguration
    {
        public int? ReceiveBufferSize { get; set; }

        public byte[] WriteTerminator { get; set; }

        public int WritePause { get; set; }

        public int ReadTimeout { get; set; }

        public int WriteTimeout { get; set; }

        public int OpenPause { get; set; }

        public bool EnableDebug { get; set; }

        internal RedboxSerialPortConfiguration()
        {
            this.WritePause = 10;
            this.ReadTimeout = -1;
            this.WriteTimeout = 5000;
            this.OpenPause = 3000;
            this.WriteTerminator = (byte[])null;
        }
    }
}
