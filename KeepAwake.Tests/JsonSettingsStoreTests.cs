using System;
using System.IO;
using KeepAwake;
using Xunit;

namespace KeepAwake.Tests
{
    /// <summary>
    /// Tests for <see cref="JsonSettingsStore"/>. Each test uses a unique temp
    /// path and cleans up after itself, so there is no shared filesystem state
    /// and no reliance on the real per-user AppData location.
    /// </summary>
    public sealed class JsonSettingsStoreTests : IDisposable
    {
        private readonly string _dir;
        private readonly string _path;

        public JsonSettingsStoreTests()
        {
            // Unique directory per test instance (xUnit creates one instance per test).
            _dir = Path.Combine(Path.GetTempPath(), "KeepAwakeTests", Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_dir, "settings.json");
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_dir))
                {
                    Directory.Delete(_dir, recursive: true);
                }
            }
            catch
            {
                // Cleanup is best-effort; a leaked temp file must not fail the test run.
            }
        }

        // ---- Load when nothing is saved -----------------------------------

        [Fact]
        public void Load_FileDoesNotExist_ReturnsDefaults()
        {
            var store = new JsonSettingsStore(_path);

            var settings = store.Load();

            Assert.Equal(KeepAwakeMode.ExecutionState, settings.Mode);
            Assert.True(settings.KeepDisplayOn);
            Assert.Equal(30, settings.WiggleIntervalSeconds);
            Assert.False(settings.StartMinimised);
        }

        // ---- Round-trip ----------------------------------------------------

        [Fact]
        public void SaveThenLoad_RoundTripsAllProperties()
        {
            var store = new JsonSettingsStore(_path);
            var original = new AppSettings
            {
                Mode = KeepAwakeMode.MouseWiggle,
                KeepDisplayOn = false,
                WiggleIntervalSeconds = 90,
                StartMinimised = true
            };

            store.Save(original);
            var loaded = store.Load();

            Assert.Equal(KeepAwakeMode.MouseWiggle, loaded.Mode);
            Assert.False(loaded.KeepDisplayOn);
            Assert.Equal(90, loaded.WiggleIntervalSeconds);
            Assert.True(loaded.StartMinimised);
        }

        [Fact]
        public void SaveThenLoad_WithNewStoreInstance_ReadsPersistedData()
        {
            var original = new AppSettings
            {
                Mode = KeepAwakeMode.MouseWiggle,
                KeepDisplayOn = false,
                WiggleIntervalSeconds = 5
            };

            new JsonSettingsStore(_path).Save(original);
            var loaded = new JsonSettingsStore(_path).Load();

            Assert.Equal(KeepAwakeMode.MouseWiggle, loaded.Mode);
            Assert.False(loaded.KeepDisplayOn);
            Assert.Equal(5, loaded.WiggleIntervalSeconds);
        }

        // ---- Corrupt / unreadable file ------------------------------------

        [Fact]
        public void Load_CorruptJson_ReturnsDefaults()
        {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(_path, "this is not valid json {{{");
            var store = new JsonSettingsStore(_path);

            var settings = store.Load();

            Assert.Equal(KeepAwakeMode.ExecutionState, settings.Mode);
            Assert.True(settings.KeepDisplayOn);
            Assert.Equal(30, settings.WiggleIntervalSeconds);
        }

        [Fact]
        public void Load_EmptyFile_ReturnsDefaults()
        {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(_path, string.Empty);
            var store = new JsonSettingsStore(_path);

            var settings = store.Load();

            Assert.Equal(KeepAwakeMode.ExecutionState, settings.Mode);
            Assert.True(settings.KeepDisplayOn);
            Assert.Equal(30, settings.WiggleIntervalSeconds);
        }

        [Fact]
        public void Load_JsonLiteralNull_ReturnsDefaults()
        {
            // Deserialize<AppSettings>("null") yields null; the store must fall
            // back to defaults rather than returning null.
            Directory.CreateDirectory(_dir);
            File.WriteAllText(_path, "null");
            var store = new JsonSettingsStore(_path);

            var settings = store.Load();

            Assert.NotNull(settings);
            Assert.Equal(KeepAwakeMode.ExecutionState, settings.Mode);
            Assert.True(settings.KeepDisplayOn);
            Assert.Equal(30, settings.WiggleIntervalSeconds);
        }

        // ---- Save creates file and parent directory -----------------------

        [Fact]
        public void Save_CreatesFileAndMissingParentDirectory()
        {
            // Use a path with a not-yet-existing nested parent directory.
            var nestedPath = Path.Combine(_dir, "nested", "deeper", "settings.json");
            var store = new JsonSettingsStore(nestedPath);

            Assert.False(File.Exists(nestedPath));

            store.Save(new AppSettings());

            Assert.True(File.Exists(nestedPath));
        }

        // ---- Enum persisted as a readable string --------------------------

        [Fact]
        public void Save_PersistsModeAsStringName()
        {
            var store = new JsonSettingsStore(_path);

            store.Save(new AppSettings { Mode = KeepAwakeMode.MouseWiggle });

            var json = File.ReadAllText(_path);
            Assert.Contains("\"MouseWiggle\"", json);
            // It must not be written as the numeric enum value.
            Assert.DoesNotContain("\"Mode\": 1", json);
        }
    }
}
