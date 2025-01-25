using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core
{
    public sealed class DriverDescriptor : IDriverDescriptor
    {
        public DriverDescriptor(string version, string provider)
          : this(new Version(version), provider)
        {
        }

        public DriverDescriptor(Version version, string provider)
        {
            this.DriverVersion = version;
            this.Provider = provider;
        }

        public Version DriverVersion { get; private set; }

        public string Provider { get; private set; }
    }
}
