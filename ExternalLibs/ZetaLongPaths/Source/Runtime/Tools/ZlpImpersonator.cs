namespace ZetaLongPaths.Tools
{
    using System.ComponentModel;
    using System.Security;
    using System.Security.Principal;

    /// <inheritdoc />
    /// <summary>
    /// Simple helper class to ease the impersonation of the current thread.
    /// I.e. run the current thread as another user than the current one.
    /// Impersonation of a user. Allows to execute code under another user context.
    /// The account that instantiates the ZlpImpersonator class
    /// needs to have the 'Act as part of operating system' privilege set.
    /// </summary>
    /// <remarks>
    /// See also https://www.codeproject.com/Articles/10090/A-small-C-Class-for-impersonating-a-User
    /// This class is based on the information in the Microsoft knowledge base
    /// article http://support.microsoft.com/default.aspx?scid=kb;en-us;Q306158
    /// Encapsulate an instance into a using-directive like e.g.:
    /// ...
    /// using ( new ZlpImpersonator( "myUsername", "myDomainname", "myPassword" ) )
    /// {
    ///     ...
    ///     [code that executes under the new context]
    ///     ...
    /// }
    /// ...
    /// </remarks>
    [PublicAPI]
    public sealed class ZlpImpersonator : IDisposable
    {
        private const int Logon32ProviderDefault = 0;
        private WindowsImpersonationContext _impersonationContext;
        private IntPtr _impersonationToken = IntPtr.Zero;
        private ZlpImpersonatorProfileBehaviour _profileBehaviour = ZlpImpersonatorProfileBehaviour.DontLoad;
        private PROFILEINFO _profileInfo;

        public ZlpImpersonator()
        {
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                profileBehaviour);
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour);
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                profileBehaviour);
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour);
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            string password)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            SecureString password)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        public ZlpImpersonator(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            UndoImpersonation();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                profileBehaviour);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                profileBehaviour);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            string password)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            SecureString password)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Starts the impersonation with the given credentials.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        [PublicAPI]
        public void Impersonate(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType)
        {
            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Undoes the impersonation. Safe to call even if not yet
        /// impersonized.
        /// </summary>
        [PublicAPI]
        public void Undo()
        {
            UndoImpersonation();
        }

        ~ZlpImpersonator()
        {
            UndoImpersonation();
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            string password)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive);
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                loginType);
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            SecureString password)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive);
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                loginType);
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if this instance [can log in] the specified user name; otherwise, <c>false</c>.
        /// </returns>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            string password,
            out Exception exception)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                out exception);
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if this instance [can log in] the specified user name; otherwise, <c>false</c>.
        /// </returns>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            out Exception exception)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                loginType,
                out exception);
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if this instance [can log in] the specified user name; otherwise, <c>false</c>.
        /// </returns>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            SecureString password,
            out Exception exception)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                out exception);
        }

        /// <summary>
        /// Static method to check whether user can log in.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if this instance [can log in] the specified user name; otherwise, <c>false</c>.
        /// </returns>
        [PublicAPI]
        public static bool CanLogIn(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            out Exception exception)
        {
            return CanLogInValidUser(
                userName,
                domainName,
                password,
                loginType,
                out exception);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                profileBehaviour);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                profileBehaviour);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            string password)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            SecureString password)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                ZlpImpersonatorLoginType.Interactive,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Static method to check whether user can impersonate.
        /// The account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">The login type.</param>
        [PublicAPI]
        public static bool CanImpersonate(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType)
        {
            return CanImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                ZlpImpersonatorProfileBehaviour.DontLoad);
        }

        /// <summary>
        /// Logons the user.
        /// </summary>
        /// <param name="lpszUserName">Name of the LPSZ user.</param>
        /// <param name="lpszDomain">The LPSZ domain.</param>
        /// <param name="lpszPassword">The LPSZ password.</param>
        /// <param name="dwLogonType">Type of the dw logon.</param>
        /// <param name="dwLogonProvider">The dw logon provider.</param>
        /// <param name="phToken">The ph token.</param>
        /// <returns></returns>
        [DllImport(@"advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LogonUser(
            string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        /// <summary>
        /// Logons the user2.
        /// </summary>
        /// <param name="lpszUserName">Name of the LPSZ user.</param>
        /// <param name="lpszDomain">The LPSZ domain.</param>
        /// <param name="password">The password.</param>
        /// <param name="dwLogonType">Type of the dw logon.</param>
        /// <param name="dwLogonProvider">The dw logon provider.</param>
        /// <param name="phToken">The ph token.</param>
        /// <returns></returns>
        [DllImport(@"advapi32.dll", EntryPoint = @"LogonUser", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int LogonUser2(
            string lpszUserName,
            string lpszDomain,
            IntPtr password,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        /// <summary>
        /// Duplicates the token.
        /// </summary>
        /// <param name="hToken">The h token.</param>
        /// <param name="impersonationLevel">The impersonation level.</param>
        /// <param name="hNewToken">The h new token.</param>
        /// <returns></returns>
        [DllImport(@"advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(
            IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        /// <summary>
        /// Reverts to self.
        /// </summary>
        /// <returns></returns>
        [DllImport(@"advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        /// <summary>
        /// Closes the handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(
            IntPtr handle);

        /// <summary>
        /// Loads the user profile.
        /// </summary>
        /// <param name="hToken">The h token.</param>
        /// <param name="lpProfileInfo">The lp profile info.</param>
        /// <returns></returns>
        [DllImport(@"userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LoadUserProfile(
            IntPtr hToken,
            ref PROFILEINFO lpProfileInfo);

        /// <summary>
        /// Unloads the user profile.
        /// </summary>
        /// <param name="hToken">The h token.</param>
        /// <param name="hProfile">The h profile.</param>
        /// <returns></returns>
        [DllImport(@"userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool UnloadUserProfile(
            IntPtr hToken,
            IntPtr hProfile);

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        private void ImpersonateValidUser(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {

            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour,
                out var exception);

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Does the actual check for impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        private static bool CanImpersonateValidUser(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            using var impersonator = new ZlpImpersonator();
            impersonator.ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour,
                out var exception);

            return exception == null;
        }

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can log in the specified user name; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanLogInValidUser(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType)
        {

            return CanLogInValidUser(
                userName,
                domainName,
                password,
                loginType,
                out _);
        }

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can log in the specified user name; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanLogInValidUser(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            out Exception exception)
        {
#if WANT_TRACE
            Trace.TraceInformation(@"[Impersonation] About to check for login as domain '{0}', user '{1}'.", domainName,
                userName);
#endif

            exception = null;

            if (domainName is {Length: <= 0})
            {
                domainName = null;
            }

            var token = IntPtr.Zero;

            try
            {
                if (LogonUser(
                        userName,
                        domainName,
                        password,
                        (int)loginType,
                        Logon32ProviderDefault,
                        ref token) == 0)
                {
                    var le = Marshal.GetLastWin32Error();
                    exception = new Win32Exception(le);
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }

            if (exception == null)
            {
#if WANT_TRACE
                Trace.TraceInformation(@"[Impersonation] Successfully check for login as domain '{0}', user '{1}'.",
                    domainName, userName);
#endif

                return true;
            }
            else
            {
#if WANT_TRACE
                Trace.TraceError(
                    $@"[Impersonation] Error check for login as domain '{domainName}', user '{userName}'.",
                    exception);
#endif

                return false;
            }
        }

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        /// <param name="exception">The exception.</param>
        private void ImpersonateValidUser(
            string userName,
            string domainName,
            string password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour,
            out Exception exception)
        {
#if WANT_TRACE
            Trace.TraceInformation(@"[Impersonation] About to impersonate as domain '{0}', user '{1}'.", domainName,
                userName);
#endif

            exception = null;

            if (domainName is {Length: <= 0})
            {
                domainName = null;
            }

            var token = IntPtr.Zero;

            try
            {
                if (RevertToSelf())
                {
                    if (LogonUser(
                            userName,
                            domainName,
                            password,
                            (int)loginType,
                            Logon32ProviderDefault,
                            ref token) != 0)
                    {
                        if (DuplicateToken(token, 2, ref _impersonationToken) != 0)
                        {
                            CheckLoadProfile(profileBehaviour);

                            var tempWindowsIdentity =
                                new WindowsIdentity(_impersonationToken);
                            _impersonationContext =
                                tempWindowsIdentity.Impersonate();
                        }
                        else
                        {
                            var le = Marshal.GetLastWin32Error();
                            exception = new Win32Exception(le);
                        }
                    }
                    else
                    {
                        var le = Marshal.GetLastWin32Error();
                        exception = new Win32Exception(le);
                    }
                }
                else
                {
                    var le = Marshal.GetLastWin32Error();
                    exception = new Win32Exception(le);
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }

            if (exception == null)
            {
#if WANT_TRACE
                Trace.TraceInformation(@"[Impersonation] Successfully impersonated as domain '{0}', user '{1}'.",
                    domainName, userName);
#endif
            }
            else
            {
#if WANT_TRACE
                Trace.TraceError(
                    $@"[Impersonation] Error impersonating as domain '{domainName}', user '{userName}'.",
                    exception);
#endif
            }
        }

        /// <summary>
        /// Does the actual check for impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        private static bool CanImpersonateValidUser(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            using var impersonator = new ZlpImpersonator();
            impersonator.ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour,
                out var exception);

            return exception == null;
        }

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        private void ImpersonateValidUser(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {

            ImpersonateValidUser(
                userName,
                domainName,
                password,
                loginType,
                profileBehaviour,
                out var exception);

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loginType">Type of the login.</param>
        /// <param name="profileBehaviour">The profile behaviour.</param>
        /// <param name="exception">The exception.</param>
        private void ImpersonateValidUser(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            ZlpImpersonatorProfileBehaviour profileBehaviour,
            out Exception exception)
        {
#if WANT_TRACE
            Trace.TraceInformation(@"[Impersonation] About to impersonate as domain '{0}', user '{1}'.", domainName,
                userName);
#endif

            exception = null;

            if (domainName is {Length: <= 0})
            {
                domainName = null;
            }

            var token = IntPtr.Zero;
            var passwordPtr = IntPtr.Zero;

            try
            {
                if (RevertToSelf())
                {
                    // Marshal the SecureString to unmanaged memory.
                    passwordPtr =
                        Marshal.SecureStringToGlobalAllocUnicode(password);

                    if (LogonUser2(
                            userName,
                            domainName,
                            passwordPtr,
                            (int)loginType,
                            Logon32ProviderDefault,
                            ref token) != 0)
                    {
                        if (DuplicateToken(token, 2, ref _impersonationToken) != 0)
                        {
                            CheckLoadProfile(profileBehaviour);

                            var tempWindowsIdentity =
                                new WindowsIdentity(_impersonationToken);
                            _impersonationContext =
                                tempWindowsIdentity.Impersonate();
                        }
                        else
                        {
                            var le = Marshal.GetLastWin32Error();
                            exception = new Win32Exception(le);
                        }
                    }
                    else
                    {
                        var le = Marshal.GetLastWin32Error();
                        exception = new Win32Exception(le);
                    }
                }
                else
                {
                    var le = Marshal.GetLastWin32Error();
                    exception = new Win32Exception(le);
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }

                // Zero-out and free the unmanaged string reference.
                Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
            }

            if (exception == null)
            {
#if WANT_TRACE
                Trace.TraceInformation(@"[Impersonation] Successfully impersonated as domain '{0}', user '{1}'.",
                    domainName, userName);
#endif
            }
            else
            {
#if WANT_TRACE
                Trace.TraceError(
                    $@"[Impersonation] Error impersonating as domain '{domainName}', user '{userName}'.",
                    exception);
#endif
            }
        }

        private static bool CanLogInValidUser(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType)
        {

            return CanLogInValidUser(
                userName,
                domainName,
                password,
                loginType,
                out _);
        }

        private static bool CanLogInValidUser(
            string userName,
            string domainName,
            SecureString password,
            ZlpImpersonatorLoginType loginType,
            out Exception exception)
        {
#if WANT_TRACE
            Trace.TraceInformation(@"[Impersonation] About to check for login as domain '{0}', user '{1}'.", domainName,
                userName);
#endif

            exception = null;

            if (domainName is {Length: <= 0})
            {
                domainName = null;
            }

            var token = IntPtr.Zero;
            var passwordPtr = IntPtr.Zero;

            try
            {
                // Marshal the SecureString to unmanaged memory.
                passwordPtr =
                    Marshal.SecureStringToGlobalAllocUnicode(password);

                if (LogonUser2(
                        userName,
                        domainName,
                        passwordPtr,
                        (int)loginType,
                        Logon32ProviderDefault,
                        ref token) == 0)
                {
                    var le = Marshal.GetLastWin32Error();
                    exception = new Win32Exception(le);
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }

                // Zero-out and free the unmanaged string reference.
                Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
            }

            if (exception == null)
            {
#if WANT_TRACE
                Trace.TraceInformation(@"[Impersonation] Successfully check for login as domain '{0}', user '{1}'.",
                    domainName, userName);
#endif

                return true;
            }
            else
            {
#if WANT_TRACE
                Trace.TraceError(
                    $@"[Impersonation] Error check for login as domain '{domainName}', user '{userName}'.",
                    exception);
#endif

                return false;
            }
        }

        private void CheckLoadProfile(
            ZlpImpersonatorProfileBehaviour profileBehaviour)
        {
            if (profileBehaviour == ZlpImpersonatorProfileBehaviour.Load)
            {
                _profileInfo = new PROFILEINFO();
                _profileInfo.dwSize = Marshal.SizeOf(_profileInfo);
                var windowsIdentity = WindowsIdentity.GetCurrent();
                _profileInfo.lpUserName = windowsIdentity.Name;

                if (LoadUserProfile(_impersonationToken, ref _profileInfo))
                {
                    _profileBehaviour = profileBehaviour;
                }
                else
                {
                    var le = Marshal.GetLastWin32Error();
                    throw new Win32Exception(le);
                }
            }
        }

        private void UndoImpersonation()
        {
            if (_impersonationContext != null)
            {
#if WANT_TRACE
                Trace.TraceInformation(
                    @"[Impersonation] About to undo impersonation.");
#endif

                try
                {
                    _impersonationContext.Undo();
                    _impersonationContext = null;
                }
                catch (Exception)
                {
#if WANT_TRACE
                    Trace.TraceError(
                        @"[Impersonation] Error undoing impersonation.");
#endif

                    throw;
                }

#if WANT_TRACE
                Trace.TraceInformation(
                    @"[Impersonation] Successfully undone impersonation.");
#endif
            }

            // --

            if (_profileBehaviour == ZlpImpersonatorProfileBehaviour.Load)
            {
#if WANT_TRACE
                Trace.TraceInformation(
                    @"[Impersonation] About to unload user profile.");
#endif

                try
                {
                    if (!UnloadUserProfile(_impersonationToken, _profileInfo.hProfile))
                    {
                        var le = Marshal.GetLastWin32Error();
                        throw new Win32Exception(le);
                    }

                    _profileBehaviour = ZlpImpersonatorProfileBehaviour.DontLoad;
                }
                catch (Exception)
                {
#if WANT_TRACE
                    Trace.TraceError(
                        @"[Impersonation] Error unloading user profile.");
#endif

                    throw;
                }
            }

            if (_impersonationToken != IntPtr.Zero)
            {
                CloseHandle(_impersonationToken);
                _impersonationToken = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable InconsistentNaming
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable MemberCanBePrivate.Local
        private struct PROFILEINFO
        {
            public int dwSize;
            public int dwFlags;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpUserName;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpProfilePath;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpDefaultPath;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpServerName;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpPolicyPath;
            public IntPtr hProfile;
        }
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local
    }

    /// <summary>
    /// How to log in the user.
    /// </summary>
    public enum ZlpImpersonatorLoginType
    {
        /// <summary>
        /// Interactive. This is the default.
        /// </summary>
        [PublicAPI] Interactive = 2,

        [PublicAPI] Batch = 4,
        [PublicAPI] Network = 3,
        [PublicAPI] NetworkClearText = 0,
        [PublicAPI] Service = 5,
        [PublicAPI] Unlock = 7,
        [PublicAPI] NewCredentials = 9
    }

    /// <summary>
    /// How to deal with the user's profile.
    /// </summary>
    /// <remarks>
    /// 2008-05-21, suggested and implemented by Tim Daplyn 
    /// (TDaplyn@MedcomSoft.com).
    /// </remarks>
    public enum ZlpImpersonatorProfileBehaviour
    {
        /// <summary>
        /// Do not load the user's profile. This is the default behaviour.
        /// </summary>
        DontLoad,

        /// <summary>
        /// Load the user's profile.
        /// </summary>
        Load
    }
}