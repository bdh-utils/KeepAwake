using System;
using System.Runtime.InteropServices;

namespace KeepAwake
{
    /// <summary>
    /// The real <see cref="ISystemActivity"/> implementation, backed by Win32.
    /// </summary>
    internal sealed class Win32SystemActivity : ISystemActivity
    {
        public void SetExecutionState(bool keepDisplayOn)
        {
            // ES_CONTINUOUS makes the request persist on this thread rather
            // than being a one-shot reset.
            var flags = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;
            if (keepDisplayOn)
            {
                flags |= EXECUTION_STATE.ES_DISPLAY_REQUIRED;
            }
            SetThreadExecutionState(flags);
        }

        public void ClearExecutionState()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        public void WiggleMouse()
        {
            // A 1px relative move out and immediately back: enough for Windows
            // to register activity without the cursor visibly drifting.
            mouse_event(MOUSEEVENTF_MOVE, 1, 0, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_MOVE, unchecked((uint)-1), 0, 0, UIntPtr.Zero);
        }

        [Flags]
        private enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002
        }

        private const uint MOUSEEVENTF_MOVE = 0x0001;

        [DllImport("kernel32.dll")]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
    }
}
