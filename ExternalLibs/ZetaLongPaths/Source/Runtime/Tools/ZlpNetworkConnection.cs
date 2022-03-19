namespace ZetaLongPaths.Tools
{
    using Properties;

    /// <inheritdoc />
    /// <summary>
    /// Simple helper class for accessing files and folders on an UNC network location
    /// with another user than the current thread's user.
    /// It is basically a wrapper for the "NET USE" functionality.
    /// </summary>
    /// <remarks>
    /// See also https://stackoverflow.com/a/1197430/107625
    /// Encapsulate an instance into a using-directive like e.g.:
    /// ...
    /// using ( new ZlpNetworkConnection( "\\myserver\myshare", "myUserName", "myPassword" ) )
    /// {
    ///     ...
    ///     [code that accesses the share under the new context/user]
    ///     ...
    /// }
    /// ...
    /// </remarks>
    [PublicAPI]
    public sealed class ZlpNetworkConnection : IDisposable
    {
        private string _networkName;

        [PublicAPI]
        public ZlpNetworkConnection()
        {
        }

        [PublicAPI]
        public ZlpNetworkConnection(
            string networkName,
            string userName,
            string password,
            ZlpNetworkConnectionResourceScope scope = ZlpNetworkConnectionResourceScope.GlobalNetwork,
            ZlpNetworkConnectionResourceType resourceType = ZlpNetworkConnectionResourceType.Disk,
            ZlpNetworkConnectionResourceDisplayType displayType = ZlpNetworkConnectionResourceDisplayType.Share,
            ZlpNetworkConnectionResourceUsage usage = ZlpNetworkConnectionResourceUsage.None,
            ZlpNetworkConnectionFlags flags = ZlpNetworkConnectionFlags.None)
        {
            Connect(networkName, userName, password, scope, resourceType, displayType, usage, flags);
        }

        [PublicAPI]
        public ZlpNetworkConnection(
            string networkName,
            bool activate,
            string userName,
            string password,
            ZlpNetworkConnectionResourceScope scope = ZlpNetworkConnectionResourceScope.GlobalNetwork,
            ZlpNetworkConnectionResourceType resourceType = ZlpNetworkConnectionResourceType.Disk,
            ZlpNetworkConnectionResourceDisplayType displayType = ZlpNetworkConnectionResourceDisplayType.Share,
            ZlpNetworkConnectionResourceUsage usage = ZlpNetworkConnectionResourceUsage.None,
            ZlpNetworkConnectionFlags flags = ZlpNetworkConnectionFlags.None)
        {
            Connect(networkName, activate, userName, password, scope, resourceType, displayType, usage, flags);
        }

        [PublicAPI]
        public void Connect(
            string networkName,
            string userName,
            string password,
            ZlpNetworkConnectionResourceScope scope = ZlpNetworkConnectionResourceScope.GlobalNetwork,
            ZlpNetworkConnectionResourceType resourceType = ZlpNetworkConnectionResourceType.Disk,
            ZlpNetworkConnectionResourceDisplayType displayType = ZlpNetworkConnectionResourceDisplayType.Share,
            ZlpNetworkConnectionResourceUsage usage = ZlpNetworkConnectionResourceUsage.None,
            ZlpNetworkConnectionFlags flags = ZlpNetworkConnectionFlags.None)
        {
            doNetUse(networkName, userName, password, scope, resourceType, displayType, usage, flags);
        }

        [PublicAPI]
        public void Connect(
            string networkName,
            bool activate,
            string userName,
            string password,
            ZlpNetworkConnectionResourceScope scope = ZlpNetworkConnectionResourceScope.GlobalNetwork,
            ZlpNetworkConnectionResourceType resourceType = ZlpNetworkConnectionResourceType.Disk,
            ZlpNetworkConnectionResourceDisplayType displayType = ZlpNetworkConnectionResourceDisplayType.Share,
            ZlpNetworkConnectionResourceUsage usage = ZlpNetworkConnectionResourceUsage.None,
            ZlpNetworkConnectionFlags flags = ZlpNetworkConnectionFlags.None)
        {
            if (activate)
            {
                Connect(networkName, userName, password, scope, resourceType, displayType, usage, flags);
            }
        }

        private void doNetUse(
            string networkName,
            string userName,
            string password,
            ZlpNetworkConnectionResourceScope scope,
            ZlpNetworkConnectionResourceType resourceType,
            ZlpNetworkConnectionResourceDisplayType displayType,
            ZlpNetworkConnectionResourceUsage usage,
            ZlpNetworkConnectionFlags flags)
        {
            var netResource = new NetResource
            {
                Scope = scope,
                ResourceType = resourceType,
                DisplayType = displayType,
                RemoteName = networkName,
                Usage = usage
            };

            var result = WNetAddConnection2(
                netResource,
                password,
                userName,
                flags);

            if (result != 0)
            {
                throw new IOException(string.Format(Resources.ErrorConnectingToRemoteShare, networkName, result),
                    result);
            }

            _networkName = networkName;
        }

        ~ZlpNetworkConnection()
        {
            doDispose();
        }

        public void Dispose()
        {
            doDispose();
            GC.SuppressFinalize(this);
        }

        private void doDispose()
        {
            if (!string.IsNullOrEmpty(_networkName))
            {
                var result = WNetCancelConnection2(_networkName, 0, true);
#if WANT_TRACE
                Trace.TraceInformation($@"Result for canceling network connection: {result}.");
#endif
            }
        }

        [DllImport(@"mpr.dll")]
        private static extern int WNetAddConnection2(
            NetResource netResource,
            string password,
            string username,
            ZlpNetworkConnectionFlags flags);

        [DllImport(@"mpr.dll")]
        private static extern int WNetCancelConnection2(
            string name,
            int flags,
            bool force);

        [StructLayout(LayoutKind.Sequential)]
        [PublicAPI]
        private sealed class NetResource
        {
#pragma warning disable 414
#pragma warning disable 169
#pragma warning disable 649
            // ReSharper disable NotAccessedField.Global
            // ReSharper disable UnusedMember.Global
            // ReSharper disable NotAccessedField.Local
            public ZlpNetworkConnectionResourceScope Scope;
            public ZlpNetworkConnectionResourceType ResourceType;
            public ZlpNetworkConnectionResourceDisplayType DisplayType;
            public ZlpNetworkConnectionResourceUsage Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
            // ReSharper restore NotAccessedField.Local
            // ReSharper restore UnusedMember.Global
            // ReSharper restore NotAccessedField.Global
#pragma warning restore 649
#pragma warning restore 169
#pragma warning restore 414
        }
    }

    [PublicAPI]
    public enum ZlpNetworkConnectionResourceScope
    {
#pragma warning disable 414
#pragma warning disable 169
        // ReSharper disable NotAccessedField.Global
        // ReSharper disable UnusedMember.Global
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
        // ReSharper restore UnusedMember.Global
        // ReSharper restore NotAccessedField.Global
#pragma warning restore 169
#pragma warning restore 414
    }

    [PublicAPI]
    public enum ZlpNetworkConnectionResourceType
    {
#pragma warning disable 414
#pragma warning disable 169
        // ReSharper disable NotAccessedField.Global
        // ReSharper disable UnusedMember.Global
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
        // ReSharper restore UnusedMember.Global
        // ReSharper restore NotAccessedField.Global
#pragma warning restore 169
#pragma warning restore 414
    }

    [PublicAPI]
    public enum ZlpNetworkConnectionResourceDisplayType
    {
#pragma warning disable 414
#pragma warning disable 169
        // ReSharper disable NotAccessedField.Global
        // ReSharper disable UnusedMember.Global
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
        // ReSharper restore UnusedMember.Global
        // ReSharper restore NotAccessedField.Global
#pragma warning restore 169
#pragma warning restore 414
    }

    [PublicAPI]
    [Flags]
    public enum ZlpNetworkConnectionResourceUsage
    {
#pragma warning disable 414
#pragma warning disable 169
        // ReSharper disable NotAccessedField.Global
        // ReSharper disable UnusedMember.Global
        None = 0x0,
        Connectable = 0x01,
        Container = 0x02,
        NoLocalDevice = 0x04,
        Sibling = 0x08,
        Attached = 0x10
        // ReSharper restore NotAccessedField.Global
#pragma warning restore 169
#pragma warning restore 414
    }

    [PublicAPI]
    [Flags]
    public enum ZlpNetworkConnectionFlags
    {
#pragma warning disable 414
#pragma warning disable 169
        // ReSharper disable NotAccessedField.Global
        // ReSharper disable UnusedMember.Global
        None = 0x0,
        ConnectUpdateProfile = 0x0001,
        ConnectUpdateRecent = 0x0002,
        ConnectTemporary = 0x0004,
        ConnectInteractive = 0x0008,
        ConnectPrompt = 0x0010,
        ConnectRedirect = 0x0080,
        ConnectCurrentMedia = 0x0200,
        ConnectCommandline = 0x0800,
        ConnectCmdSaveCredentials = 0x1000,
        ConnectCredentialsReset = 0x2000

        // ReSharper restore NotAccessedField.Global
#pragma warning restore 169
#pragma warning restore 414
    }
}