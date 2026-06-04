using KeepAwake;
using Xunit;

namespace KeepAwake.Tests
{
    public class CloseDecisionTests
    {
        [Fact]
        public void NormalClose_MinimisesToTray()
        {
            Assert.Equal(
                WindowCloseAction.MinimiseToTray,
                CloseDecision.Decide(userExiting: false, sessionEnding: false));
        }

        [Fact]
        public void UserExit_Exits()
        {
            Assert.Equal(
                WindowCloseAction.Exit,
                CloseDecision.Decide(userExiting: true, sessionEnding: false));
        }

        [Fact]
        public void SessionEnding_Exits()
        {
            // Regression: logging off while running must close cleanly rather
            // than cancelling the OS-initiated close and minimising to tray.
            Assert.Equal(
                WindowCloseAction.Exit,
                CloseDecision.Decide(userExiting: false, sessionEnding: true));
        }

        [Fact]
        public void UserExitDuringSessionEnding_Exits()
        {
            Assert.Equal(
                WindowCloseAction.Exit,
                CloseDecision.Decide(userExiting: true, sessionEnding: true));
        }
    }
}
