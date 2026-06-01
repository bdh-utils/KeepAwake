namespace KeepAwake
{
    /// <summary>
    /// The mechanism used to keep the machine awake.
    /// </summary>
    public enum KeepAwakeMode
    {
        /// <summary>
        /// Ask Windows to stay awake via SetThreadExecutionState. Cleanest
        /// option; can optionally keep the display on as well.
        /// </summary>
        ExecutionState,

        /// <summary>
        /// Nudge the mouse cursor periodically so Windows registers user
        /// activity. Useful where the execution-state request is overridden
        /// by policy (e.g. some managed/corporate machines).
        /// </summary>
        MouseWiggle
    }
}
