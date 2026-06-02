using System;

namespace KeepAwake
{
    /// <summary>
    /// Auto-start logic, decoupled from the registry via <see cref="IRunKeyStore"/>.
    /// Enabling writes the (quoted) executable path under a fixed value name so
    /// Windows runs it at sign-in; disabling removes that value.
    /// </summary>
    public sealed class StartupRegistration : IStartupRegistration
    {
        /// <summary>The Run-key value name KeepAwake registers under.</summary>
        public const string ValueName = "KeepAwake";

        private readonly IRunKeyStore _store;
        private readonly string _executablePath;

        public StartupRegistration(IRunKeyStore store, string executablePath)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _executablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
        }

        public bool IsEnabled()
        {
            return !string.IsNullOrEmpty(_store.GetValue(ValueName));
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                // Always (re)write the current path so a moved executable is
                // corrected the next time the user toggles this on.
                _store.SetValue(ValueName, Quote(_executablePath));
            }
            else
            {
                _store.DeleteValue(ValueName);
            }
        }

        private static string Quote(string path) => "\"" + path + "\"";
    }
}
