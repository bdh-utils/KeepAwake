using Microsoft.Win32;

namespace KeepAwake
{
    /// <summary>
    /// The real <see cref="IRunKeyStore"/>, backed by the per-user
    /// HKCU\...\CurrentVersion\Run key. Per-user means no admin rights are
    /// required to register auto-start.
    /// </summary>
    internal sealed class RegistryRunKeyStore : IRunKeyStore
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public string? GetValue(string name)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            return key?.GetValue(name) as string;
        }

        public void SetValue(string name, string value)
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            key.SetValue(name, value, RegistryValueKind.String);
        }

        public void DeleteValue(string name)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            key?.DeleteValue(name, throwOnMissingValue: false);
        }
    }
}
