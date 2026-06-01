namespace KeepAwake
{
    /// <summary>
    /// Abstraction over the operating-system calls KeepAwake relies on.
    /// Keeping these behind an interface lets <see cref="KeepAwakeController"/>
    /// be unit tested without touching real Win32 APIs.
    /// </summary>
    public interface ISystemActivity
    {
        /// <summary>
        /// Request that the system stays awake, optionally keeping the
        /// display on too. The request persists until cleared.
        /// </summary>
        void SetExecutionState(bool keepDisplayOn);

        /// <summary>
        /// Drop any standing keep-awake request so the machine may sleep.
        /// </summary>
        void ClearExecutionState();

        /// <summary>
        /// Generate a tiny mouse movement to register user activity.
        /// </summary>
        void WiggleMouse();
    }
}
