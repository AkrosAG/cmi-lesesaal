using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net;
using Serilog;

namespace CMI.Access.Repository.Systems.Rosetta.Helper
{
    public class ConnectToSharedFolder : IDisposable
    {
        private readonly string networkName;

        public ConnectToSharedFolder(string networkName, NetworkCredential credentials)
        {
            this.networkName = networkName;
            var netResource = new NetResource
            {
                scope = ResourceScope.GlobalNetwork,
                resourceType = ResourceType.Disk,
                displayType = ResourceDisplaytype.Share,
                remoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : $@"{credentials.Domain}\{credentials.UserName}";

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                userName,
                0);

            if (result != 0)
            {
                var exception = new Win32Exception(result);
                Log.Error(exception, $"LogonUser failed with error code: {result}");
                throw exception;
            }
        }

        ~ConnectToSharedFolder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope scope;
            public ResourceType resourceType;
            public ResourceDisplaytype displayType;
            public int usage;
            public string localName;
            public string remoteName;
            public string comment;
            public string provider;
        }

        public enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };

        public enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
    }
}