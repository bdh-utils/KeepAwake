namespace KeepAwake
{
    /// <summary>
    /// Controls whether KeepAwake launches automatically when the user signs
    /// in to Windows. Behind an interface so the UI can be tested without
    /// touching the real registry.
    /// </summary>
    public interface IStartupRegistration
    {
        /// <summary>Whether auto-start is currently registered.</summary>
        bool IsEnabled();

        /// <summary>Register or unregister auto-start at sign-in.</summary>
        void SetEnabled(bool enabled);
    }
}
