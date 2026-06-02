using System;
using KeepAwake;
using Xunit;

namespace KeepAwake.Tests
{
    public class StartupRegistrationTests
    {
        private const string ExePath = @"C:\Apps\KeepAwake\KeepAwake.exe";

        private static (StartupRegistration reg, FakeRunKeyStore store) Create(string exePath = ExePath)
        {
            var store = new FakeRunKeyStore();
            return (new StartupRegistration(store, exePath), store);
        }

        [Fact]
        public void Constructor_NullStore_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new StartupRegistration(null!, ExePath));
        }

        [Fact]
        public void Constructor_NullExecutablePath_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new StartupRegistration(new FakeRunKeyStore(), null!));
        }

        [Fact]
        public void IsEnabled_WhenNotRegistered_ReturnsFalse()
        {
            var (reg, _) = Create();

            Assert.False(reg.IsEnabled());
        }

        [Fact]
        public void SetEnabled_True_WritesQuotedPathUnderValueName()
        {
            var (reg, store) = Create();

            reg.SetEnabled(true);

            Assert.Equal($"\"{ExePath}\"", store.GetValue(StartupRegistration.ValueName));
            Assert.True(reg.IsEnabled());
        }

        [Fact]
        public void SetEnabled_False_RemovesValue()
        {
            var (reg, store) = Create();
            reg.SetEnabled(true);

            reg.SetEnabled(false);

            Assert.Null(store.GetValue(StartupRegistration.ValueName));
            Assert.False(reg.IsEnabled());
        }

        [Fact]
        public void SetEnabled_FalseWhenNotRegistered_DoesNotThrow()
        {
            var (reg, store) = Create();

            reg.SetEnabled(false);

            Assert.False(reg.IsEnabled());
            Assert.Equal(1, store.DeleteCount);
        }

        [Fact]
        public void SetEnabled_TrueTwice_RewritesCurrentPath()
        {
            var store = new FakeRunKeyStore();
            var stale = new StartupRegistration(store, @"C:\Old\KeepAwake.exe");
            stale.SetEnabled(true);

            var current = new StartupRegistration(store, ExePath);
            current.SetEnabled(true);

            Assert.Equal($"\"{ExePath}\"", store.GetValue(StartupRegistration.ValueName));
            Assert.Equal(2, store.SetCount);
        }

        [Fact]
        public void IsEnabled_WhenValueIsEmptyString_ReturnsFalse()
        {
            var store = new FakeRunKeyStore();
            store.SetValue(StartupRegistration.ValueName, string.Empty);
            var reg = new StartupRegistration(store, ExePath);

            Assert.False(reg.IsEnabled());
        }

        [Fact]
        public void Toggle_OnThenOff_LeavesNoRegistration()
        {
            var (reg, store) = Create();

            reg.SetEnabled(true);
            reg.SetEnabled(false);

            Assert.False(reg.IsEnabled());
            Assert.Equal(1, store.SetCount);
            Assert.Equal(1, store.DeleteCount);
        }
    }
}
