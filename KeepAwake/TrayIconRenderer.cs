using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace KeepAwake
{
    /// <summary>
    /// Draws the system-tray icon in the bdh-utils palette. The mug is filled
    /// with the brand accent (#F15025) while KeepAwake is running and the muted
    /// grey (#5A5E5A) while it is stopped, with steam rising only when active,
    /// so the tray reflects state at a glance.
    /// </summary>
    public static class TrayIconRenderer
    {
        // bdh-utils palette.
        private static readonly Color Accent = Color.FromArgb(0xF1, 0x50, 0x25);
        private static readonly Color Muted = Color.FromArgb(0x5A, 0x5E, 0x5A);

        /// <summary>Canvas size; Windows scales this down for the tray as needed.</summary>
        public const int Size = 32;

        /// <summary>Create a tray icon for the given running state.</summary>
        public static Icon Create(bool running)
        {
            var colour = running ? Accent : Muted;

            using var bmp = new Bitmap(Size, Size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                DrawMug(g, colour, running);
            }

            return ToIcon(bmp);
        }

        private static void DrawMug(Graphics g, Color colour, bool running)
        {
            using var fill = new SolidBrush(colour);
            using var pen = new Pen(colour, 2.4f)
            {
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            // Cup body.
            var body = new Rectangle(7, 13, 13, 13);
            using (var path = RoundedRect(body, 3))
            {
                g.FillPath(fill, path);
            }

            // Handle on the right.
            g.DrawArc(pen, 18, 14, 9, 9, -70, 160);

            // Steam reads as "active", so only draw it while running.
            if (running)
            {
                DrawSteam(g, pen, 11);
                DrawSteam(g, pen, 16);
            }
        }

        private static void DrawSteam(Graphics g, Pen pen, float x)
        {
            using var path = new GraphicsPath();
            path.AddBezier(x, 11, x - 2.5f, 8.5f, x + 2.5f, 6.5f, x, 4);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Icon ToIcon(Bitmap bmp)
        {
            // GetHicon allocates an unmanaged HICON that Icon.FromHandle does not
            // own; clone into a self-contained managed icon, then free the HICON.
            IntPtr hicon = bmp.GetHicon();
            try
            {
                using var temp = Icon.FromHandle(hicon);
                return (Icon)temp.Clone();
            }
            finally
            {
                DestroyIcon(hicon);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}
