using System;

namespace KeepAwake
{
    /// <summary>
    /// The WPF-free heart of KeepAwake. Owns the running state and decides
    /// which <see cref="ISystemActivity"/> calls to make as the mode, the
    /// display preference, and the start/stop state change.
    ///
    /// For <see cref="KeepAwakeMode.MouseWiggle"/> the host is expected to
    /// call <see cref="Tick"/> on a timer; the controller itself owns no timer
    /// so it stays trivially testable.
    /// </summary>
    public class KeepAwakeController
    {
        private readonly ISystemActivity _system;

        public KeepAwakeController(ISystemActivity system)
        {
            _system = system ?? throw new ArgumentNullException(nameof(system));
        }

        /// <summary>Whether keep-awake is currently active.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>The mechanism in use. Defaults to execution-state.</summary>
        public KeepAwakeMode Mode { get; private set; } = KeepAwakeMode.ExecutionState;

        /// <summary>
        /// Whether the display should be kept on as well (only meaningful for
        /// <see cref="KeepAwakeMode.ExecutionState"/>). Defaults to true.
        /// </summary>
        public bool KeepDisplayOn { get; private set; } = true;

        /// <summary>Begin keeping the machine awake. No-op if already running.</summary>
        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            Apply();
        }

        /// <summary>Stop keeping the machine awake. No-op if already stopped.</summary>
        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            // Always clear, even in mouse-wiggle mode: it is idempotent and
            // guarantees we never leave a standing execution-state request.
            _system.ClearExecutionState();
        }

        /// <summary>
        /// Timer callback for mouse-wiggle mode. Does nothing unless running in
        /// <see cref="KeepAwakeMode.MouseWiggle"/>, so the host can call it
        /// unconditionally on every tick.
        /// </summary>
        public void Tick()
        {
            if (IsRunning && Mode == KeepAwakeMode.MouseWiggle)
            {
                _system.WiggleMouse();
            }
        }

        /// <summary>
        /// Switch the keep-awake mechanism. If running, the old mechanism is
        /// torn down and the new one applied immediately.
        /// </summary>
        public void SetMode(KeepAwakeMode mode)
        {
            if (Mode == mode) return;

            // Leaving execution-state mode while running: drop the request.
            if (IsRunning && Mode == KeepAwakeMode.ExecutionState)
            {
                _system.ClearExecutionState();
            }

            Mode = mode;

            if (IsRunning)
            {
                Apply();
            }
        }

        /// <summary>
        /// Update the keep-display-on preference. Re-applies live if running in
        /// execution-state mode; otherwise just records the preference.
        /// </summary>
        public void SetKeepDisplayOn(bool value)
        {
            if (KeepDisplayOn == value) return;
            KeepDisplayOn = value;

            if (IsRunning && Mode == KeepAwakeMode.ExecutionState)
            {
                _system.SetExecutionState(KeepDisplayOn);
            }
        }

        private void Apply()
        {
            // Mouse-wiggle has no immediate action; the periodic Tick does it.
            if (Mode == KeepAwakeMode.ExecutionState)
            {
                _system.SetExecutionState(KeepDisplayOn);
            }
        }
    }
}
