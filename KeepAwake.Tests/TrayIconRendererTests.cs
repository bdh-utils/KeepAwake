using KeepAwake;
using Xunit;

namespace KeepAwake.Tests
{
    public class TrayIconRendererTests
    {
        // A point well inside the filled cup body (rect 7,13,13,13).
        private const int CupX = 13;
        private const int CupY = 19;

        [Fact]
        public void Create_ReturnsIconOfExpectedSize()
        {
            using var icon = TrayIconRenderer.Create(running: true);

            Assert.NotNull(icon);
            Assert.Equal(TrayIconRenderer.Size, icon.Width);
            Assert.Equal(TrayIconRenderer.Size, icon.Height);
        }

        [Fact]
        public void Create_Running_FillsCupWithBrandAccent()
        {
            using var icon = TrayIconRenderer.Create(running: true);
            using var bmp = icon.ToBitmap();

            var p = bmp.GetPixel(CupX, CupY);
            Assert.Equal(0xF1, p.R);
            Assert.Equal(0x50, p.G);
            Assert.Equal(0x25, p.B);
        }

        [Fact]
        public void Create_Stopped_FillsCupWithBrandMuted()
        {
            using var icon = TrayIconRenderer.Create(running: false);
            using var bmp = icon.ToBitmap();

            var p = bmp.GetPixel(CupX, CupY);
            Assert.Equal(0x5A, p.R);
            Assert.Equal(0x5E, p.G);
            Assert.Equal(0x5A, p.B);
        }

        [Fact]
        public void Create_RunningAndStopped_Differ()
        {
            using var running = TrayIconRenderer.Create(running: true);
            using var stopped = TrayIconRenderer.Create(running: false);
            using var rb = running.ToBitmap();
            using var sb = stopped.ToBitmap();

            Assert.NotEqual(rb.GetPixel(CupX, CupY).ToArgb(), sb.GetPixel(CupX, CupY).ToArgb());
        }
    }
}
