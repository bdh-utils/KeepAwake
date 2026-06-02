namespace KeepAwake
{
    /// <summary>
    /// User preferences persisted between runs. Defaults here are the
    /// out-of-the-box behaviour when no settings file exists yet.
    /// </summary>
    public sealed class AppSettings
    {
        public KeepAwakeMode Mode { get; set; } = KeepAwakeMode.ExecutionState;

        public bool KeepDisplayOn { get; set; } = true;

        public int WiggleIntervalSeconds { get; set; } = 30;

        /// <summary>Start hidden in the system tray rather than showing the window.</summary>
        public bool StartMinimised { get; set; } = false;
    }
}
