using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core.Descriptors;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.HAL.Core
{
    public sealed class UsbDeviceService : IUsbDeviceService
    {
        private readonly DriverDescriptor Driver_Elo = new DriverDescriptor(new Version("5.5.1.4"), "Elo Touch Solutions");
        private readonly DriverDescriptor Driver_GeneralTouch_HID1 = new DriverDescriptor(new Version("4.2.2.1"), "General Touch Technology Co.,Ltd.");
        private readonly DriverDescriptor Driver_GeneralTouch_HID2 = new DriverDescriptor(new Version("2.10.1781.0"), "General Touch Technology Co., Ltd.");
        private readonly DriverDescriptor Driver_TouchBase = new DriverDescriptor(new Version("4.0.2.0"), "Touch-Base Ltd");
        private readonly List<ITouchscreenDescriptor> TouchScreenDescriptors = new List<ITouchscreenDescriptor>();
        private readonly List<IDeviceDescriptor> CreditCardReaders = new List<IDeviceDescriptor>();
        private readonly IDriverDescriptor Gen4DriverDescriptor = (IDriverDescriptor)new DriverDescriptor(new Version("2.1.0.0"), "AVEO");
        private readonly IDriverDescriptor Gen3DriverDescriptor = (IDriverDescriptor)new DriverDescriptor(new Version("5.7.19104.104"), "Sonix");
        private readonly List<IDeviceDescriptor> Cameras = new List<IDeviceDescriptor>();
        private readonly bool Debug;
        private const int CR_SUCCESS = 0;
        private const int CM_PROB_FAILED_START = 10;
        private const int CM_PROB_DISABLED = 22;
        private const int DN_HAS_PROBLEM = 1024;
        private const int DN_STARTED = 8;
        private const int SPDIT_NODRIVER = 0;
        private const int SPDIT_CLASSDRIVER = 1;
        private const int SPDIT_COMPATDRIVER = 2;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int DIGCF_ALLCLASSES = 4;
        private const int DIGCF_PRESENT = 2;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int MAX_DEV_LEN = 1000;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        private const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        private const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        private const int DBT_DEVNODES_CHANGED = 7;
        private const int WM_DEVICECHANGE = 537;
        private const int DIF_PROPERTYCHANGE = 18;
        private const int DICS_FLAG_GLOBAL = 1;
        private const int DICS_FLAG_CONFIGSPECIFIC = 2;
        private const int DICS_ENABLE = 1;
        private const int DICS_DISABLE = 2;
        private const int SPDRP_DEVICEDESC = 0;
        private const int SPDRP_HARDWAREID = 1;
        private const int SPDRP_COMPATIBLEIDS = 2;
        private const int SPDRP_UNUSED0 = 3;
        private const int SPDRP_SERVICE = 4;
        private const int SPDRP_UNUSED1 = 5;
        private const int SPDRP_UNUSED2 = 6;
        private const int SPDRP_CLASS = 7;
        private const int SPDRP_CLASSGUID = 8;
        private const int SPDRP_DRIVER = 9;
        private const int SPDRP_CONFIGFLAGS = 10;
        private const int SPDRP_MFG = 11;
        private const int SPDRP_FRIENDLYNAME = 12;
        private const int SPDRP_LOCATION_INFORMATION = 13;
        private const int SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 14;
        private const int SPDRP_CAPABILITIES = 15;
        private const int SPDRP_UI_NUMBER = 16;
        private const int SPDRP_UPPERFILTERS = 17;
        private const int SPDRP_LOWERFILTERS = 18;
        private const int SPDRP_BUSTYPEGUID = 19;
        private const int SPDRP_LEGACYBUSTYPE = 20;
        private const int SPDRP_BUSNUMBER = 21;
        private const int SPDRP_ENUMERATOR_NAME = 22;
        private const int SPDRP_SECURITY = 23;
        private const int SPDRP_SECURITY_SDS = 24;
        private const int SPDRP_DEVTYPE = 25;
        private const int SPDRP_EXCLUSIVE = 26;
        private const int SPDRP_CHARACTERISTICS = 27;
        private const int SPDRP_ADDRESS = 28;
        private const int SPDRP_UI_NUMBER_DESC_FORMAT = 29;
        private const int SPDRP_DEVICE_POWER_DATA = 30;
        private const int SPDRP_REMOVAL_POLICY = 31;
        private const int SPDRP_REMOVAL_POLICY_HW_DEFAULT = 32;
        private const int SPDRP_REMOVAL_POLICY_OVERRIDE = 33;
        private const int SPDRP_INSTALL_STATE = 34;
        private const int SPDRP_LOCATION_PATHS = 35;
        private const int SPDRP_BASE_CONTAINERID = 36;

        public IDeviceDescriptor FindActiveCamera(bool matchDriver)
        {
            IDeviceDescriptor activeCamera = this.Cameras.Find((Predicate<IDeviceDescriptor>)(each => each.Locate()));
            if (activeCamera == null)
                return (IDeviceDescriptor)null;
            LogHelper.Instance.Log("[UsbDeviceService] FindActiveCamera: HWID found {0}", (object)activeCamera.ToString());
            if (!matchDriver || activeCamera.MatchDriver())
                return activeCamera;
            LogHelper.Instance.Log("[{0}] unable to match camera driver.", (object)this.GetType().Name);
            return (IDeviceDescriptor)null;
        }

        public IDeviceDescriptor FromQueryString(string hwid)
        {
            return (IDeviceDescriptor)GenericDeviceDescriptor.Create(hwid, DeviceClass.None);
        }

        public bool SetDeviceState(IDeviceDescriptor descriptor, DeviceState state)
        {
            this.MakeBuffer();
            int lastError = 0;
            int failures = 0;
            this.EnumerateDevices(descriptor.SetupClass.Guid, this.SearchFlags(descriptor), (UsbDeviceService.ProcessDeviceInfo)((IntPtr devInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                string hwid = this.QueryHardwareId(devInfo, ref did, out lastError);
                if (hwid == null)
                    return UsbDeviceService.ProcessDeviceInfoResult.Continue;
                IDeviceDescriptor left = this.FromQueryString(hwid);
                if (left != null && this.Match(left, descriptor))
                {
                    bool flag = this.ChangeState(devInfo, ref did, state);
                    LogHelper.Instance.Log("[{0}] Found HWID {1}; change state to {2} returned {3}", (object)this.GetType().Name, (object)hwid, (object)state.ToString(), flag ? (object)"OK" : (object)"FAIL");
                    if (!flag)
                        ++failures;
                }
                return UsbDeviceService.ProcessDeviceInfoResult.Continue;
            }));
            return failures == 0;
        }

        public bool ChangeByHWID(IDeviceDescriptor descriptor, DeviceState state)
        {
            int error = 0;
            return this.EnumerateDevices(descriptor.SetupClass.Guid, this.SearchFlags(descriptor), (UsbDeviceService.ProcessDeviceInfo)((IntPtr devInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                string hwid = this.QueryHardwareId(devInfo, ref did, out error);
                if (hwid == null)
                    return UsbDeviceService.ProcessDeviceInfoResult.Continue;
                IDeviceDescriptor left = this.FromQueryString(hwid);
                UsbDeviceService.ProcessDeviceInfoResult deviceInfoResult = UsbDeviceService.ProcessDeviceInfoResult.Continue;
                if (left != null && this.Match(left, descriptor))
                {
                    if (this.Debug)
                        LogHelper.Instance.Log("[{0}] Change state of {1} to {2}", (object)this.GetType().Name, (object)hwid, (object)state.ToString());
                    deviceInfoResult = this.ChangeState(devInfo, ref did, state) ? UsbDeviceService.ProcessDeviceInfoResult.Success : UsbDeviceService.ProcessDeviceInfoResult.Error;
                }
                return deviceInfoResult;
            }));
        }

        public DeviceStatus FindDeviceStatus(IDeviceDescriptor deviceInfo)
        {
            DeviceStatus rv = DeviceStatus.None;
            this.EnumerateDevices(deviceInfo.SetupClass.Guid, this.SearchFlags(deviceInfo), (UsbDeviceService.ProcessDeviceInfo)((IntPtr devInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                string hwid = this.QueryHardwareId(devInfo, ref did, out int _);
                if (hwid == null)
                    return UsbDeviceService.ProcessDeviceInfoResult.Continue;
                IDeviceDescriptor left = this.FromQueryString(hwid);
                LogHelper.Instance.Log("[USBService] Find device status hwid = {0}", (object)hwid);
                rv = DeviceStatus.None;
                if (left != null && this.Match(left, deviceInfo))
                {
                    rv |= DeviceStatus.Found;
                    uint status;
                    uint probNum;
                    if (UsbDeviceService.CM_Get_DevNode_Status(out status, out probNum, did.devInst, 0) == 0)
                    {
                        LogHelper.Instance.Log("[USBService] CM_Get_DevNode {0} ( {1} ) status = {2} problem = {3}", (object)deviceInfo.ToString(), (object)deviceInfo.Friendlyname, (object)status, (object)probNum);
                        if (((int)status & 1024) != 0)
                        {
                            if (22U == probNum)
                                rv |= DeviceStatus.Disabled;
                            else if (10U == probNum)
                                rv |= DeviceStatus.NotStarted;
                        }
                        else if (((int)status & 8) != 0)
                            rv |= DeviceStatus.Enabled;
                        return UsbDeviceService.ProcessDeviceInfoResult.Success;
                    }
                }
                return UsbDeviceService.ProcessDeviceInfoResult.Continue;
            }));
            return rv;
        }

        public bool MatchDriverByVendor(IDeviceDescriptor desc, IDriverDescriptor driverInfo)
        {
            return this.EnumerateDevices(desc.SetupClass.Guid, this.SearchFlags(desc), (UsbDeviceService.ProcessDeviceInfo)((IntPtr hDevInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                string str = this.QueryHardwareId(hDevInfo, ref did, out int _);
                if (str == null)
                    return UsbDeviceService.ProcessDeviceInfoResult.Continue;
                UsbDeviceService.ProcessDeviceInfoResult deviceInfoResult = UsbDeviceService.ProcessDeviceInfoResult.Continue;
                if (!string.IsNullOrEmpty(str) && str.Equals(desc.Vendor, StringComparison.CurrentCultureIgnoreCase))
                    deviceInfoResult = this.MatchDriverInner(hDevInfo, ref did, driverInfo, str.ToString()) ? UsbDeviceService.ProcessDeviceInfoResult.Success : UsbDeviceService.ProcessDeviceInfoResult.Continue;
                return deviceInfoResult;
            }));
        }

        public bool MatchDriver(IDeviceDescriptor descriptor, IDriverDescriptor driverInfo)
        {
            return this.EnumerateDevices(descriptor.SetupClass.Guid, 2U, (UsbDeviceService.ProcessDeviceInfo)((IntPtr hDevInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                string str = this.QueryHardwareId(hDevInfo, ref did, out int _);
                if (str == null)
                    return UsbDeviceService.ProcessDeviceInfoResult.Continue;
                LogHelper.Instance.Log("[MatchDriver] Processing HWID {0}", (object)str.ToString());
                IDeviceDescriptor left = this.FromQueryString(str.ToString());
                if (left == null || !this.Match(left, descriptor))
                    return UsbDeviceService.ProcessDeviceInfoResult.Continue;
                LogHelper.Instance.Log("[MatchDriver] HWID found {0}", (object)str.ToString());
                return !this.MatchDriverInner(hDevInfo, ref did, driverInfo, str.ToString()) ? UsbDeviceService.ProcessDeviceInfoResult.Continue : UsbDeviceService.ProcessDeviceInfoResult.Success;
            }));
        }

        public IUsbDeviceSearchResult FindDevice(IDeviceDescriptor descriptor)
        {
            int lastError = 0;
            UsbDeviceSearchResult result = new UsbDeviceSearchResult();
            if (this.Debug)
                LogHelper.Instance.Log("[FindDevice] Search device class {0}", (object)descriptor.SetupClass.Class.ToString());
            this.EnumerateDevices(descriptor.SetupClass.Guid, this.SearchFlags(descriptor), (UsbDeviceService.ProcessDeviceInfo)((IntPtr devInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                string hwid = this.QueryHardwareId(devInfo, ref did, out lastError);
                if (hwid != null)
                {
                    LogHelper.Instance.Log("  [FindDeviceWithVid] found hwid = {0}", (object)hwid);
                    IDeviceDescriptor left = this.FromQueryString(hwid);
                    if (left != null && this.Match(left, descriptor))
                        result.Matches.Add(left);
                }
                return UsbDeviceService.ProcessDeviceInfoResult.Continue;
            }));
            return (IUsbDeviceSearchResult)result;
        }

        public IUsbDeviceSearchResult FindVendorDevices(string _vendor)
        {
            int lastError = 0;
            UsbDeviceSearchResult result = new UsbDeviceSearchResult();
            if (this.Debug)
                LogHelper.Instance.Log("[FindVendorDevices] Search for devices from vendor {0}", (object)_vendor);
            this.EnumerateDevices(Guid.Empty, 6U, (UsbDeviceService.ProcessDeviceInfo)((IntPtr devInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                string hwid = this.QueryHardwareId(devInfo, ref did, out lastError);
                if (this.Debug && hwid != null)
                    LogHelper.Instance.Log("  [FindVendorDevices] Examine hwid = {0} for match", (object)hwid);
                IDeviceDescriptor deviceDescriptor = this.FromQueryString(hwid);
                if (deviceDescriptor != null && _vendor.Equals(deviceDescriptor.Vendor, StringComparison.CurrentCultureIgnoreCase))
                    result.Matches.Add(deviceDescriptor);
                return UsbDeviceService.ProcessDeviceInfoResult.Continue;
            }));
            return (IUsbDeviceSearchResult)result;
        }

        public void EnumDevices(Action<string, string> onDeviceFound)
        {
            int lastError = 0;
            StringBuilder clazzNameBuffer = this.MakeBuffer();
            this.EnumerateDevices(Guid.Empty, 6U, (UsbDeviceService.ProcessDeviceInfo)((IntPtr devInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                if (!UsbDeviceService.SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7U, 0U, clazzNameBuffer, 1000U, IntPtr.Zero))
                {
                    lastError = Marshal.GetLastWin32Error();
                    LogHelper.Instance.Log("[{0}] SetupDiGetDeviceRegistryProperty returned error {1}", (object)this.GetType().Name, (object)lastError);
                    return UsbDeviceService.ProcessDeviceInfoResult.Error;
                }
                string str = this.QueryHardwareId(devInfo, ref did, out lastError);
                if (!string.IsNullOrEmpty(str))
                    onDeviceFound(clazzNameBuffer.ToString(), str);
                return UsbDeviceService.ProcessDeviceInfoResult.Continue;
            }));
        }

        public List<IDeviceDescriptor> FindDevices(DeviceClass clazz)
        {
            int lastError = 0;
            List<IDeviceDescriptor> result = new List<IDeviceDescriptor>();
            if (clazz == DeviceClass.None)
                return result;
            StringBuilder clazzNameBuffer = this.MakeBuffer();
            Guid deviceClass = Guid.Empty;
            IDeviceSetupClass deviceSetupClass = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>().Get(clazz);
            if (deviceSetupClass != null)
                deviceClass = deviceSetupClass.Guid;
            this.EnumerateDevices(deviceClass, 2U, (UsbDeviceService.ProcessDeviceInfo)((IntPtr devInfo, ref UsbDeviceService.SP_DEVINFO_DATA did) =>
            {
                if (!UsbDeviceService.SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7U, 0U, clazzNameBuffer, 1000U, IntPtr.Zero))
                {
                    lastError = Marshal.GetLastWin32Error();
                    return UsbDeviceService.ProcessDeviceInfoResult.Error;
                }
                DeviceClass ignoringCase = Enum<DeviceClass>.ParseIgnoringCase(clazzNameBuffer.ToString(), DeviceClass.None);
                if (this.Debug)
                    LogHelper.Instance.Log("[{0}] Requested search = {1}, found clazz = {2}", (object)this.GetType().Name, (object)clazz.ToString(), (object)clazzNameBuffer.ToString());
                if (ignoringCase == clazz)
                {
                    string hwid = this.QueryHardwareId(devInfo, ref did, out lastError);
                    if (!string.IsNullOrEmpty(hwid))
                    {
                        if (this.Debug)
                            LogHelper.Instance.Log("[{0}] Requested search = {1}, found clazz = {2} HWID = {3}", (object)this.GetType().Name, (object)clazz.ToString(), (object)clazzNameBuffer.ToString(), (object)hwid);
                        GenericDeviceDescriptor deviceDescriptor = GenericDeviceDescriptor.Create(hwid, ignoringCase);
                        if (deviceDescriptor != null)
                            result.Add((IDeviceDescriptor)deviceDescriptor);
                    }
                }
                return UsbDeviceService.ProcessDeviceInfoResult.Continue;
            }));
            return result;
        }

        public ITouchscreenDescriptor FindTouchScreen(bool matchDriver)
        {
            foreach (ITouchscreenDescriptor screenDescriptor in this.TouchScreenDescriptors)
            {
                LogHelper.Instance.Log("[UsbDeviceService] Searching for device {0}", (object)screenDescriptor.ToString());
                if (screenDescriptor.Locate())
                {
                    LogHelper.Instance.Log("[UsbDeviceService] Found Touchscreen device {0}.", (object)screenDescriptor.ToString());
                    if (!matchDriver)
                        return screenDescriptor;
                    if (screenDescriptor.MatchDriver())
                    {
                        LogHelper.Instance.Log("[UsbDeviceService] Touch screen driver matched.");
                        return screenDescriptor;
                    }
                }
            }
            LogHelper.Instance.Log("[UsbDeviceService] Could not match TS driver.");
            return (ITouchscreenDescriptor)null;
        }

        public ITouchscreenDescriptor FindTouchScreen() => this.FindTouchScreen(false);

        public IQueryUsbDeviceResult FindCCR()
        {
            return (IQueryUsbDeviceResult)this.OnLocate(this.CreditCardReaders);
        }

        public IQueryUsbDeviceResult FindCamera()
        {
            return (IQueryUsbDeviceResult)this.OnLocate(this.Cameras);
        }

        public UsbDeviceService(bool debug)
        {
            this.Debug = debug;
            DriverDescriptor desc = new DriverDescriptor(new Version("7.13.14.0"), "3M");
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new MicrotouchDescriptor((IUsbDeviceService)this, DeviceClass.HIDClass, (IDriverDescriptor)desc));
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new MicrotouchDescriptor((IUsbDeviceService)this, DeviceClass.Mouse, (IDriverDescriptor)desc));
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new GenericTouchscreenDescriptor("04e7", "0020", "Elo", (IDriverDescriptor)this.Driver_Elo, (IUsbDeviceService)this, DeviceClass.Mouse));
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new GenericTouchscreenDescriptor("04e7", "0042", "Elo_2", (IDriverDescriptor)this.Driver_Elo, (IUsbDeviceService)this, DeviceClass.Mouse));
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", (IDriverDescriptor)this.Driver_GeneralTouch_HID2, (IUsbDeviceService)this, DeviceClass.HIDClass));
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", (IDriverDescriptor)this.Driver_GeneralTouch_HID2, (IUsbDeviceService)this, DeviceClass.HIDClass));
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", (IDriverDescriptor)this.Driver_GeneralTouch_HID1, (IUsbDeviceService)this, DeviceClass.Mouse));
            this.TouchScreenDescriptors.Add((ITouchscreenDescriptor)new GenericTouchscreenDescriptor("14c8", "0003", "Zytronic", (IDriverDescriptor)this.Driver_TouchBase, (IUsbDeviceService)this, DeviceClass.Mouse));
            IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
            this.CreditCardReaders.Add((IDeviceDescriptor)new IdTechRev1((IUsbDeviceService)this, service));
            this.CreditCardReaders.Add((IDeviceDescriptor)new IdTechRev2((IUsbDeviceService)this, service));
            this.Cameras.Add((IDeviceDescriptor)new Gen5DeviceDescriptor((IUsbDeviceService)this));
            this.Cameras.Add((IDeviceDescriptor)new LegacyDeviceDescriptor("1871", "0d01", "4th Gen (Color)", this.Gen4DriverDescriptor, (IUsbDeviceService)this));
            this.Cameras.Add((IDeviceDescriptor)new LegacyDeviceDescriptor("1871", "0f01", "4th Gen", this.Gen4DriverDescriptor, (IUsbDeviceService)this));
            this.Cameras.Add((IDeviceDescriptor)new LegacyDeviceDescriptor("0c45", "627b", "3rd Gen", this.Gen3DriverDescriptor, (IUsbDeviceService)this));
        }

        public UsbDeviceService()
          : this(false)
        {
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(
          ref Guid gClass,
          uint iEnumerator,
          IntPtr hParent,
          uint nFlags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern int CM_Get_DevNode_Status(
          out uint status,
          out uint probNum,
          uint devInst,
          int flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetupDiBuildDriverInfoList(
          IntPtr deviceInfoSet,
          ref UsbDeviceService.SP_DEVINFO_DATA deviceInfoData,
          UsbDeviceService.DriverType driverType);

        [DllImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetupDiEnumDriverInfo(
          IntPtr deviceInfoSet,
          ref UsbDeviceService.SP_DEVINFO_DATA deviceInfoData,
          UsbDeviceService.DriverType driverType,
          uint memberIndex,
          ref UsbDeviceService.DriverInfoData driverInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetupDiEnumDeviceInfo(
          IntPtr deviceInfoSet,
          uint memberIndex,
          ref UsbDeviceService.SP_DEVINFO_DATA data);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
          IntPtr lpInfoSet,
          ref UsbDeviceService.SP_DEVINFO_DATA infoData,
          uint Property,
          uint PropertyRegDataType,
          StringBuilder PropertyBuffer,
          uint PropertyBufferSize,
          IntPtr RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiSetClassInstallParams(
          IntPtr dis,
          ref UsbDeviceService.SP_DEVINFO_DATA did,
          ref UsbDeviceService.SP_PROPCHANGE_PARAMS spp,
          int sz);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiCallClassInstaller(
          uint func,
          IntPtr dis,
          ref UsbDeviceService.SP_DEVINFO_DATA did);

        private uint SearchFlags(IDeviceDescriptor desc)
        {
            uint num = 2;
            if (desc.SetupClass.Class == DeviceClass.None)
                num |= 4U;
            return num;
        }

        private string QueryDeviceDescription(
          IntPtr devInfo,
          ref UsbDeviceService.SP_DEVINFO_DATA deviceInfoData,
          out int lastError)
        {
            lastError = 0;
            StringBuilder PropertyBuffer = this.MakeBuffer();
            if (UsbDeviceService.SetupDiGetDeviceRegistryProperty(devInfo, ref deviceInfoData, 0U, 0U, PropertyBuffer, 1000U, IntPtr.Zero))
                return PropertyBuffer.ToString().ToLower();
            lastError = Marshal.GetLastWin32Error();
            return (string)null;
        }

        private string QueryHardwareId(
          IntPtr devInfo,
          ref UsbDeviceService.SP_DEVINFO_DATA did,
          out int lastError)
        {
            lastError = 0;
            StringBuilder PropertyBuffer = this.MakeBuffer();
            if (UsbDeviceService.SetupDiGetDeviceRegistryProperty(devInfo, ref did, 1U, 0U, PropertyBuffer, 1000U, IntPtr.Zero))
                return PropertyBuffer.ToString().ToLower();
            lastError = Marshal.GetLastWin32Error();
            return (string)null;
        }

        private bool ChangeState(
          IntPtr hDevInfo,
          ref UsbDeviceService.SP_DEVINFO_DATA devInfoData,
          DeviceState devState)
        {
            try
            {
                UsbDeviceService.SP_PROPCHANGE_PARAMS spp = new UsbDeviceService.SP_PROPCHANGE_PARAMS();
                spp.ClassInstallHeader = new UsbDeviceService.SP_CLASSINSTALL_HEADER();
                if (DeviceState.Enable == devState)
                {
                    spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(UsbDeviceService.SP_CLASSINSTALL_HEADER));
                    spp.ClassInstallHeader.InstallFunction = 18;
                    spp.StateChange = 1;
                    spp.Scope = 1;
                    spp.HwProfile = 0;
                    if (UsbDeviceService.SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref spp, Marshal.SizeOf(typeof(UsbDeviceService.SP_PROPCHANGE_PARAMS))))
                        UsbDeviceService.SetupDiCallClassInstaller(18U, hDevInfo, ref devInfoData);
                    spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(UsbDeviceService.SP_CLASSINSTALL_HEADER));
                    spp.ClassInstallHeader.InstallFunction = 18;
                    spp.StateChange = 1;
                    spp.Scope = 2;
                    spp.HwProfile = 0;
                }
                else
                {
                    spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(UsbDeviceService.SP_CLASSINSTALL_HEADER));
                    spp.ClassInstallHeader.InstallFunction = 18;
                    spp.StateChange = 2;
                    spp.Scope = 2;
                    spp.HwProfile = 0;
                }
                if (!UsbDeviceService.SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref spp, Marshal.SizeOf(typeof(UsbDeviceService.SP_PROPCHANGE_PARAMS))))
                {
                    LogHelper.Instance.Log("SetupDiSetClassInstallParams returned false.");
                    return false;
                }
                if (UsbDeviceService.SetupDiCallClassInstaller(18U, hDevInfo, ref devInfoData))
                    return true;
                LogHelper.Instance.Log("SetupDiCallClassInstaller returned false ( Win32 error = {0} )", (object)Marshal.GetLastWin32Error());
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private QueryUsbDeviceResult OnLocate(List<IDeviceDescriptor> descriptors)
        {
            foreach (IDeviceDescriptor descriptor in descriptors)
            {
                DeviceStatus deviceStatus = this.FindDeviceStatus(descriptor);
                if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None)
                    return new QueryUsbDeviceResult(descriptor)
                    {
                        Status = deviceStatus
                    };
            }
            return new QueryUsbDeviceResult((IDeviceDescriptor)null)
            {
                Status = DeviceStatus.None
            };
        }

        private bool MatchDriverInner(
          IntPtr hDevInfo,
          ref UsbDeviceService.SP_DEVINFO_DATA did,
          IDriverDescriptor driverInfo,
          string hwid)
        {
            if (driverInfo != null && UsbDeviceService.SetupDiBuildDriverInfoList(hDevInfo, ref did, UsbDeviceService.DriverType.SPDIT_COMPATDRIVER))
            {
                UsbDeviceService.DriverInfoData driverInfoData = new UsbDeviceService.DriverInfoData();
                driverInfoData.Size = Marshal.SizeOf((object)driverInfoData);
                for (uint memberIndex = 0; UsbDeviceService.SetupDiEnumDriverInfo(hDevInfo, ref did, UsbDeviceService.DriverType.SPDIT_COMPATDRIVER, memberIndex, ref driverInfoData); ++memberIndex)
                {
                    if (this.Debug)
                    {
                        LogHelper.Instance.Log("Driver data:", LogEntryType.Info);
                        LogHelper.Instance.Log(driverInfoData.ToString(), LogEntryType.Info);
                    }
                    if (driverInfoData.GetVersion() != driverInfo.DriverVersion)
                    {
                        if (this.Debug)
                            LogHelper.Instance.Log("** driver version {0} didn't match {1}", (object)driverInfoData.GetVersion().ToString(), (object)driverInfo.DriverVersion.ToString());
                    }
                    else
                    {
                        if (driverInfoData.ProviderName.Equals(driverInfo.Provider, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (this.Debug)
                            {
                                LogHelper.Instance.Log("Found HWID {0}; driver info:", (object)hwid);
                                LogHelper.Instance.Log("{0}", (object)driverInfoData.ToString());
                            }
                            return true;
                        }
                        if (this.Debug)
                            LogHelper.Instance.Log("** driver provider {0} didn't match {1}", (object)driverInfoData.ProviderName, (object)driverInfo.Provider);
                    }
                }
            }
            return false;
        }

        private bool EnumerateDevices(
          Guid deviceClass,
          uint searchFlags,
          UsbDeviceService.ProcessDeviceInfo callback)
        {
            IntPtr num = IntPtr.Zero;
            try
            {
                num = UsbDeviceService.SetupDiGetClassDevs(ref deviceClass, 0U, IntPtr.Zero, searchFlags);
                if (num.ToInt32() == -1)
                    return false;
                UsbDeviceService.SP_DEVINFO_DATA structure = new UsbDeviceService.SP_DEVINFO_DATA();
                structure.cbSize = Marshal.SizeOf((object)structure);
                for (uint memberIndex = 0; UsbDeviceService.SetupDiEnumDeviceInfo(num, memberIndex, ref structure); ++memberIndex)
                {
                    UsbDeviceService.ProcessDeviceInfoResult deviceInfoResult = callback(num, ref structure);
                    if (UsbDeviceService.ProcessDeviceInfoResult.Error == deviceInfoResult)
                        return false;
                    if (UsbDeviceService.ProcessDeviceInfoResult.Success == deviceInfoResult)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[UsbDeviceService] Unable to enumerate device tree.");
                return false;
            }
            finally
            {
                try
                {
                    if (num != IntPtr.Zero)
                    {
                        if (num.ToInt32() != -1)
                            UsbDeviceService.SetupDiDestroyDeviceInfoList(num);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("[UsbDeviceService] Clearing the device info list caught an exception.", ex);
                }
            }
        }

        private StringBuilder MakeBuffer()
        {
            return new StringBuilder("") { Capacity = 1000 };
        }

        private bool Match(IDeviceDescriptor left, IDeviceDescriptor right)
        {
            return this.Match(left, right, UsbDeviceService.DeviceDescriptorMatchOption.VidPid);
        }

        private bool Match(
          IDeviceDescriptor left,
          IDeviceDescriptor right,
          UsbDeviceService.DeviceDescriptorMatchOption option)
        {
            if (UsbDeviceService.DeviceDescriptorMatchOption.Product == option)
                return left.Product.ToLower().Equals(right.Product.ToLower(), StringComparison.CurrentCultureIgnoreCase);
            return (UsbDeviceService.DeviceDescriptorMatchOption.Vendor == option || left.Product.ToLower().Equals(right.Product.ToLower(), StringComparison.CurrentCultureIgnoreCase)) && left.Vendor.ToLower().Equals(right.Vendor.ToLower(), StringComparison.CurrentCultureIgnoreCase);
        }

        private enum DriverType : uint
        {
            SPDIT_NODRIVER,
            SPDIT_CLASSDRIVER,
            SPDIT_COMPATDRIVER,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct DriverInfoData
        {
            public int Size;
            public UsbDeviceService.DriverType DriverType;
            public IntPtr Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Description;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string MfgName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ProviderName;
            public long DriverDate;
            public ulong DriverVersion;

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(this.DriverType.ToString());
                stringBuilder.AppendLine(this.Description);
                stringBuilder.AppendLine(this.MfgName);
                stringBuilder.AppendLine(this.ProviderName);
                stringBuilder.AppendLine(DateTime.FromFileTime(this.DriverDate).ToShortDateString());
                stringBuilder.AppendLine(this.GetVersion().ToString());
                return stringBuilder.ToString();
            }

            public Version GetVersion()
            {
                return new Version((int)(this.DriverVersion >> 48), (int)((long)(this.DriverVersion >> 32) & (long)ushort.MaxValue), (int)((long)(this.DriverVersion >> 16) & (long)ushort.MaxValue), (int)((long)this.DriverVersion & (long)ushort.MaxValue));
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SP_DEVINFO_DATA
        {
            internal int cbSize;
            internal Guid classGuid;
            internal uint devInst;
            internal IntPtr reserved;
        }

        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal int cbSize;
            internal Guid interfaceClassGuid;
            internal int flags;
            private UIntPtr reserved;
        }

        internal struct SP_PROPCHANGE_PARAMS
        {
            internal UsbDeviceService.SP_CLASSINSTALL_HEADER ClassInstallHeader;
            internal int StateChange;
            internal int Scope;
            internal int HwProfile;
        }

        internal struct SP_CLASSINSTALL_HEADER
        {
            internal int cbSize;
            internal int InstallFunction;
        }

        private enum DeviceDescriptorMatchOption
        {
            VidPid,
            Vendor,
            Product,
        }

        private enum ProcessDeviceInfoResult
        {
            None,
            Continue,
            Error,
            Success,
        }

        private delegate UsbDeviceService.ProcessDeviceInfoResult ProcessDeviceInfo(
          IntPtr devInfo,
          ref UsbDeviceService.SP_DEVINFO_DATA did);
    }
}
