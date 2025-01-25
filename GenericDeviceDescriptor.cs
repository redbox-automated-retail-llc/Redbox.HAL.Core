using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core
{
    internal class GenericDeviceDescriptor : IDeviceDescriptor
    {
        private static readonly char[] HwidSeparator = new char[1]
        {
      '&'
        };

        public override string ToString()
        {
            return this.SetupClass != null ? string.Format("{0}\\{1}&{2}", (object)this.SetupClass.Class.ToString(), (object)this.Vendor, (object)this.Product) : this.Vendor;
        }

        public bool ResetDriver() => throw new NotImplementedException();

        public bool MatchDriver() => throw new NotImplementedException();

        public bool Locate() => throw new NotImplementedException();

        public DeviceStatus GetStatus() => throw new NotImplementedException();

        public string Vendor { get; internal set; }

        public string Product { get; internal set; }

        public string Friendlyname => throw new NotImplementedException();

        public IDeviceSetupClass SetupClass { get; internal set; }

        internal static GenericDeviceDescriptor Create(string hwid, DeviceClass clazz)
        {
            if (string.IsNullOrEmpty(hwid))
                return (GenericDeviceDescriptor)null;
            hwid = hwid.ToLower();
            string[] strArray1 = hwid.Split('\\');
            if (strArray1.Length != 2)
                return (GenericDeviceDescriptor)null;
            string str = strArray1[0];
            string p = strArray1[1];
            int startIndex = p.IndexOf("vid");
            if (-1 == startIndex)
                return GenericDeviceDescriptor.Create(string.Empty, p, clazz);
            string[] strArray2 = p.Substring(startIndex).Split(GenericDeviceDescriptor.HwidSeparator, StringSplitOptions.RemoveEmptyEntries);
            return strArray2.Length < 2 ? (GenericDeviceDescriptor)null : GenericDeviceDescriptor.Create(strArray2[0].Substring(4).ToLower(), strArray2[1].Substring(4).ToLower(), clazz);
        }

        internal static GenericDeviceDescriptor Create(string v, string p, DeviceClass clazz)
        {
            GenericDeviceDescriptor deviceDescriptor = new GenericDeviceDescriptor()
            {
                Vendor = v,
                Product = p
            };
            if (clazz != DeviceClass.None)
            {
                IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
                deviceDescriptor.SetupClass = service.Get(clazz);
            }
            return deviceDescriptor;
        }
    }
}
