namespace KeepAwake
{
    /// <summary>
    /// Minimal abstraction over the Windows "Run" registry key, so the
    /// auto-start logic in <see cref="StartupRegistration"/> can be unit
    /// tested against a fake without writing to the real registry.
    /// </summary>
    public interface IRunKeyStore
    {
        /// <summary>Return the named value, or null if it is not present.</summary>
        string? GetValue(string name);

        /// <summary>Create or overwrite the named value.</summary>
        void SetValue(string name, string value);

        /// <summary>Remove the named value if it exists; no-op otherwise.</summary>
        void DeleteValue(string name);
    }
}
