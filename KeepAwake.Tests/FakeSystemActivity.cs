using System.Collections.Generic;
using KeepAwake;

namespace KeepAwake.Tests
{
    /// <summary>
    /// Records the calls <see cref="KeepAwakeController"/> makes so tests can
    /// assert on them, with no real OS interaction.
    /// </summary>
    internal sealed class FakeSystemActivity : ISystemActivity
    {
        public int SetCount { get; private set; }
        public int ClearCount { get; private set; }
        public int WiggleCount { get; private set; }

        /// <summary>The keepDisplayOn argument of the most recent SetExecutionState call.</summary>
        public bool? LastKeepDisplayOn { get; private set; }

        /// <summary>Ordered log of calls, useful for sequencing assertions.</summary>
        public List<string> Calls { get; } = new();

        public void SetExecutionState(bool keepDisplayOn)
        {
            SetCount++;
            LastKeepDisplayOn = keepDisplayOn;
            Calls.Add($"Set({keepDisplayOn})");
        }

        public void ClearExecutionState()
        {
            ClearCount++;
            Calls.Add("Clear");
        }

        public void WiggleMouse()
        {
            WiggleCount++;
            Calls.Add("Wiggle");
        }
    }
}
