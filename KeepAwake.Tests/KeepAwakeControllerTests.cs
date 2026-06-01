using System;
using KeepAwake;
using Xunit;

namespace KeepAwake.Tests
{
    public class KeepAwakeControllerTests
    {
        private static (KeepAwakeController controller, FakeSystemActivity system) Create()
        {
            var system = new FakeSystemActivity();
            return (new KeepAwakeController(system), system);
        }

        // ---- Construction & defaults --------------------------------------

        [Fact]
        public void Constructor_NullSystem_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new KeepAwakeController(null!));
        }

        [Fact]
        public void Defaults_AreStoppedExecutionStateDisplayOn()
        {
            var (controller, _) = Create();

            Assert.False(controller.IsRunning);
            Assert.Equal(KeepAwakeMode.ExecutionState, controller.Mode);
            Assert.True(controller.KeepDisplayOn);
        }

        // ---- Start / Stop in execution-state mode -------------------------

        [Fact]
        public void Start_ExecutionState_AppliesWithDisplayOn()
        {
            var (controller, system) = Create();

            controller.Start();

            Assert.True(controller.IsRunning);
            Assert.Equal(1, system.SetCount);
            Assert.True(system.LastKeepDisplayOn);
        }

        [Fact]
        public void Start_WhenAlreadyRunning_IsNoOp()
        {
            var (controller, system) = Create();

            controller.Start();
            controller.Start();

            Assert.Equal(1, system.SetCount);
        }

        [Fact]
        public void Stop_ClearsExecutionStateAndStops()
        {
            var (controller, system) = Create();
            controller.Start();

            controller.Stop();

            Assert.False(controller.IsRunning);
            Assert.Equal(1, system.ClearCount);
        }

        [Fact]
        public void Stop_WhenNotRunning_IsNoOp()
        {
            var (controller, system) = Create();

            controller.Stop();

            Assert.Equal(0, system.ClearCount);
            Assert.False(controller.IsRunning);
        }

        // ---- Display preference -------------------------------------------

        [Fact]
        public void Start_WithDisplayOff_AppliesWithDisplayOff()
        {
            var (controller, system) = Create();
            controller.SetKeepDisplayOn(false);

            controller.Start();

            Assert.False(system.LastKeepDisplayOn);
        }

        [Fact]
        public void SetKeepDisplayOn_WhileRunning_ReappliesLive()
        {
            var (controller, system) = Create();
            controller.Start();          // Set(true)

            controller.SetKeepDisplayOn(false);

            Assert.Equal(2, system.SetCount);
            Assert.False(system.LastKeepDisplayOn);
        }

        [Fact]
        public void SetKeepDisplayOn_WhileStopped_DoesNotCallSystem()
        {
            var (controller, system) = Create();

            controller.SetKeepDisplayOn(false);

            Assert.Equal(0, system.SetCount);
            Assert.False(controller.KeepDisplayOn);
        }

        [Fact]
        public void SetKeepDisplayOn_SameValue_IsNoOp()
        {
            var (controller, system) = Create();
            controller.Start();          // Set(true)

            controller.SetKeepDisplayOn(true);

            Assert.Equal(1, system.SetCount);
        }

        [Fact]
        public void SetKeepDisplayOn_InMouseMode_DoesNotApplyExecutionState()
        {
            var (controller, system) = Create();
            controller.SetMode(KeepAwakeMode.MouseWiggle);
            controller.Start();

            controller.SetKeepDisplayOn(false);

            Assert.Equal(0, system.SetCount);
        }

        // ---- Tick / mouse wiggle ------------------------------------------

        [Fact]
        public void Tick_WhenStopped_DoesNotWiggle()
        {
            var (controller, system) = Create();
            controller.SetMode(KeepAwakeMode.MouseWiggle);

            controller.Tick();

            Assert.Equal(0, system.WiggleCount);
        }

        [Fact]
        public void Tick_RunningInMouseMode_Wiggles()
        {
            var (controller, system) = Create();
            controller.SetMode(KeepAwakeMode.MouseWiggle);
            controller.Start();

            controller.Tick();
            controller.Tick();

            Assert.Equal(2, system.WiggleCount);
        }

        [Fact]
        public void Tick_RunningInExecutionStateMode_DoesNotWiggle()
        {
            var (controller, system) = Create();
            controller.Start();

            controller.Tick();

            Assert.Equal(0, system.WiggleCount);
        }

        [Fact]
        public void Start_InMouseMode_DoesNotApplyExecutionState()
        {
            var (controller, system) = Create();
            controller.SetMode(KeepAwakeMode.MouseWiggle);

            controller.Start();

            Assert.Equal(0, system.SetCount);
            Assert.True(controller.IsRunning);
        }

        // ---- Mode switching -----------------------------------------------

        [Fact]
        public void SetMode_SameMode_IsNoOp()
        {
            var (controller, system) = Create();
            controller.Start();          // Set(true)

            controller.SetMode(KeepAwakeMode.ExecutionState);

            Assert.Equal(1, system.SetCount);
            Assert.Equal(0, system.ClearCount);
        }

        [Fact]
        public void SetMode_ToMouseWiggle_WhileRunning_ClearsExecutionState()
        {
            var (controller, system) = Create();
            controller.Start();          // Set(true)

            controller.SetMode(KeepAwakeMode.MouseWiggle);

            Assert.Equal(KeepAwakeMode.MouseWiggle, controller.Mode);
            Assert.Equal(1, system.ClearCount);
        }

        [Fact]
        public void SetMode_ToExecutionState_WhileRunning_AppliesExecutionState()
        {
            var (controller, system) = Create();
            controller.SetMode(KeepAwakeMode.MouseWiggle);
            controller.Start();          // no Set in mouse mode

            controller.SetMode(KeepAwakeMode.ExecutionState);

            Assert.Equal(1, system.SetCount);
            Assert.True(system.LastKeepDisplayOn);
        }

        [Fact]
        public void SetMode_WhileStopped_DoesNotTouchSystem()
        {
            var (controller, system) = Create();

            controller.SetMode(KeepAwakeMode.MouseWiggle);
            controller.SetMode(KeepAwakeMode.ExecutionState);

            Assert.Empty(system.Calls);
        }

        [Fact]
        public void FullCycle_SwitchModesWhileRunning_KeepsConsistentState()
        {
            var (controller, system) = Create();

            controller.Start();                              // Set(true)
            controller.SetMode(KeepAwakeMode.MouseWiggle);   // Clear
            controller.Tick();                               // Wiggle
            controller.SetMode(KeepAwakeMode.ExecutionState);// Set(true)
            controller.Stop();                               // Clear

            Assert.Equal(new[] { "Set(True)", "Clear", "Wiggle", "Set(True)", "Clear" }, system.Calls);
            Assert.False(controller.IsRunning);
        }
    }
}
