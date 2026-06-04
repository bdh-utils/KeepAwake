namespace KeepAwake
{
    /// <summary>What the window should do when it is asked to close.</summary>
    public enum WindowCloseAction
    {
        /// <summary>Hide to the tray and keep running (the normal close-button behaviour).</summary>
        MinimiseToTray,

        /// <summary>Actually shut down and release resources.</summary>
        Exit
    }

    /// <summary>
    /// Decides how to handle a window close. Pulled out of the WPF code-behind
    /// so it can be unit tested.
    /// </summary>
    public static class CloseDecision
    {
        /// <summary>
        /// The close button should minimise to the tray, but an explicit exit
        /// or an ending Windows session (log off / shut down) must let the app
        /// close — cancelling an OS-initiated session-end close throws.
        /// </summary>
        public static WindowCloseAction Decide(bool userExiting, bool sessionEnding)
        {
            return (userExiting || sessionEnding)
                ? WindowCloseAction.Exit
                : WindowCloseAction.MinimiseToTray;
        }
    }
}
