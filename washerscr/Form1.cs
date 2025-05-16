using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using washerscr.Properties;

namespace MyScreensaver
{
    public partial class Form1 : Form
    {
        private Image topImage;
        private Image rotatingImage;

        private float rotationAngle = 0f;
        private DateTime lastFrameTime;
        private Point initialMousePos;

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;

            this.TopMost = true;
            Cursor.Hide();

            // 加载图片
            topImage = Resources.top;     // 尺寸：1080x840（即 1920 - 1080）
            rotatingImage = Resources.drum;  // 尺寸：1080x1080

            this.KeyDown += (_, __) => Application.Exit();
            this.MouseMove += OnMouseMove;

            initialMousePos = Cursor.Position;
            lastFrameTime = DateTime.Now;

            Application.Idle += OnAppIdle;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Distance(Cursor.Position, initialMousePos) > 10)
                Application.Exit();
        }

        private double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private void OnAppIdle(object sender, EventArgs e)
        {
            while (AppStillIdle)
            {
                DateTime now = DateTime.Now;
                float delta = (float)(now - lastFrameTime).TotalSeconds;
                lastFrameTime = now;

                rotationAngle += delta * 400f; // 每秒30度
                if (rotationAngle >= 360f) rotationAngle -= 360f;

                Invalidate(); // 请求重绘
                Application.DoEvents(); // 保持消息流通
            }
        }

        private bool AppStillIdle
        {
            get
            {
                NativeMethods.PeekMessage(out var msg, IntPtr.Zero, 0, 0, 0);
                return msg.message == 0;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle clientRect = this.ClientRectangle;
            float formAspect = (float)clientRect.Width / clientRect.Height;
            float targetAspect = 9f / 16f;
            Rectangle drawRect = clientRect;

            if (formAspect > targetAspect)
            {
                int targetWidth = (int)(clientRect.Height * targetAspect);
                int margin = (clientRect.Width - targetWidth) / 2;
                drawRect = new Rectangle(margin, 0, targetWidth, clientRect.Height);
            }
            else if (formAspect < targetAspect)
            {
                int targetHeight = (int)(clientRect.Width / targetAspect);
                int margin = (clientRect.Height - targetHeight) / 2;
                drawRect = new Rectangle(0, margin, clientRect.Width, targetHeight);
            }

            int totalHeight = drawRect.Height;
            int bottomSize = drawRect.Width;
            int topHeight = totalHeight - bottomSize;

            Rectangle topRect = new Rectangle(drawRect.X, drawRect.Y, drawRect.Width, topHeight);
            Rectangle bottomRect = new Rectangle(drawRect.X, drawRect.Bottom - bottomSize, drawRect.Width, bottomSize);

            if (topImage != null)
                g.DrawImage(topImage, topRect);

            if (rotatingImage != null)
            {
                Bitmap rotated = new Bitmap(bottomRect.Width, bottomRect.Height);
                using (Graphics rg = Graphics.FromImage(rotated))
                {
                    rg.TranslateTransform(bottomRect.Width / 2f, bottomRect.Height / 2f);
                    rg.RotateTransform(rotationAngle);
                    rg.TranslateTransform(-bottomRect.Width / 2f, -bottomRect.Height / 2f);
                    rg.DrawImage(rotatingImage, 0, 0, bottomRect.Width, bottomRect.Height);
                }
                g.DrawImage(rotated, bottomRect);
                rotated.Dispose();
            }
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point pt;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd,
            uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
    }
}