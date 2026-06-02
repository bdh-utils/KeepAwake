using System.Collections.Generic;
using KeepAwake;

namespace KeepAwake.Tests
{
    /// <summary>
    /// In-memory <see cref="IRunKeyStore"/> for testing
    /// <see cref="StartupRegistration"/> without touching the real registry.
    /// </summary>
    internal sealed class FakeRunKeyStore : IRunKeyStore
    {
        private readonly Dictionary<string, string> _values = new();

        public int SetCount { get; private set; }
        public int DeleteCount { get; private set; }

        public string? GetValue(string name)
            => _values.TryGetValue(name, out var value) ? value : null;

        public void SetValue(string name, string value)
        {
            _values[name] = value;
            SetCount++;
        }

        public void DeleteValue(string name)
        {
            _values.Remove(name);
            DeleteCount++;
        }
    }
}
