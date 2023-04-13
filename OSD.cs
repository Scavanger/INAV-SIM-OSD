using GameOverlay.Windows;
using Graphics = GameOverlay.Drawing.Graphics;
using Image = GameOverlay.Drawing.Image;
using PixelFormat = SharpDX.WIC.PixelFormat;


namespace INAV_SIM_OSD
{
    public class OSD : IDisposable
    {
        private const int CHAR_WIDTH = 24;
        private const int CHAR_HEIGHT = 36;
        private const int CHARS_PER_FILE = 256;
        private const int BYTES_PER_PIXEl = 4;

        private enum SubCommand : byte
        {
            ClearScreen = 2,
            WriteString = 3,
            DrawScreen = 4,
            SetOptions = 5
        }

        public const string FONT_FOLDER = "osd_font";

        private const int SCREEN_ROWS = 22;
        private const int SCREEN_COLS = 60;
        private const int Y_OFFSET = 20; // Don't draw over menu bar

        private readonly List<Image> images;
        private readonly StickyWindow overlayWindow;

        private UInt16[,] screen;
        private bool needsRedraw = true;

        public string FontName { get; set; }
        public bool Show { get; set; }

        public OSD(IntPtr targetWindow)
        {
            images = new List<Image>();

            Graphics graphic = new(targetWindow, 200, 100)
            {
                PerPrimitiveAntiAliasing = true,
            };

            screen = new ushort[SCREEN_ROWS, SCREEN_COLS];
            Show = true;
            FontName = "";

            overlayWindow = new StickyWindow(targetWindow, graphic)
            {
                FPS = 60,
                IsTopmost = true,
                IsVisible = true
            };

            overlayWindow.DestroyGraphics += OverlayWindow_DestroyGraphics;
            overlayWindow.DrawGraphics += OverlayWindow_DrawGraphics;
            overlayWindow.SetupGraphics += OverlayWindow_SetupGraphics;
        }

        private enum OsdPayloadState
        {
            SubCmd,
            PositionX,
            PositionY,
            StringInfo,
            String
        }

        public void Decode(byte[] buffer, int length)
        {
            if (buffer.Length < 2)
                return;

            if (length <= 0)
                return;

            OsdPayloadState osdPayLoadState = OsdPayloadState.SubCmd;
            int row = 0;
            int col = 0;
            int strIdx = 0;
            bool isExtdChar = false;

            foreach (byte b in buffer[..length])
            {
                switch (osdPayLoadState)
                {
                    case OsdPayloadState.SubCmd:
                        switch ((SubCommand)b)
                        {
                            case SubCommand.ClearScreen:
                                Array.Clear(screen);
                                needsRedraw = true;
                                break;
                            case SubCommand.WriteString:
                                row = col = strIdx = 0;
                                osdPayLoadState = OsdPayloadState.PositionY;
                                break;
                            case SubCommand.DrawScreen:
                                needsRedraw = true;
                                osdPayLoadState = OsdPayloadState.SubCmd;
                                break;
                        }
                        break;
                    case OsdPayloadState.PositionY:
                        row = b;
                        osdPayLoadState = OsdPayloadState.PositionX;
                        break;
                    case OsdPayloadState.PositionX:
                        col = b;
                        osdPayLoadState = OsdPayloadState.StringInfo;
                        break;
                    case OsdPayloadState.StringInfo:
                        isExtdChar = b > 0;
                        osdPayLoadState = OsdPayloadState.String;
                        break;
                    case OsdPayloadState.String:
                        if (row < SCREEN_ROWS && col + strIdx < SCREEN_COLS)
                            screen[row, col + strIdx++] = (UInt16)(isExtdChar ? b | 0x100 : b);

                        break;
                }
            }

        }

        private void OverlayWindow_SetupGraphics(object? sender, SetupGraphicsEventArgs e)
        {
            Graphics graphics = e.Graphics;

            if (e.RecreateResources)
            {
                foreach (var image in images)
                    image.Dispose();
            }

            byte[] fontRaw = File.ReadAllBytes(Path.Combine(FONT_FOLDER, FontName, "font_inav_hd.bin"));
            byte[] font2Raw = File.ReadAllBytes(Path.Combine(FONT_FOLDER, FontName, "font_inav_hd_2.bin"));

            images.AddRange(WTFOSOsdFontToImages(fontRaw, graphics));
            images.AddRange(WTFOSOsdFontToImages(font2Raw, graphics));
        }

        private static Image[] WTFOSOsdFontToImages(byte[] fontBytes, Graphics graphics)
        {
            Image[] images = new Image[CHARS_PER_FILE];

            using MemoryStream ms = new();
            for (int i = 0; i < CHARS_PER_FILE; i++)
            {
                var img = new Bitmap(CHAR_WIDTH, CHAR_HEIGHT, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                int c = i * (CHAR_HEIGHT * CHAR_WIDTH * BYTES_PER_PIXEl);
                for (int y = 0; y < CHAR_HEIGHT; y++)
                {
                    for (int x = 0; x < CHAR_WIDTH; x++)
                    {
                        img.SetPixel(x, y, Color.FromArgb(fontBytes[c + 3], fontBytes[c], fontBytes[c + 1], fontBytes[c + 2]));
                        c += BYTES_PER_PIXEl;
                    }
                }
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                images[i] = graphics.CreateImage(ms.GetBuffer(), PixelFormat.Format32bppPRGBA);                
            }
            return images;
        }

        public void Run(TaskScheduler scheduler)
        {
            overlayWindow.Create();
            overlayWindow.Join();
        }

        private void OverlayWindow_DrawGraphics(object? sender, DrawGraphicsEventArgs e)
        {
            Graphics graphics = e.Graphics;

            if (!Show)
            {
                graphics.ClearScene();
                return;
            }

            if (!needsRedraw)
                return;

            graphics.ClearScene();

            int elementWith = (int)Math.Floor((double)(graphics.Width / SCREEN_COLS));
            int elemntHeight = (int)Math.Floor((double)((graphics.Height - Y_OFFSET) / SCREEN_ROWS));
            int borderLeft = (graphics.Width - elementWith * SCREEN_COLS) / 2;
            int borderTop = (graphics.Height - elemntHeight * SCREEN_ROWS) / 2 + Y_OFFSET;

            for (int y = 0; y < SCREEN_ROWS; y++)
            {
                for (int x = 0; x < SCREEN_COLS; x++)
                {
                    UInt16 character = screen[y, x];
                    // Skip space
                    if (character == 0x20 || character == 0x00)
                        continue;

                    int left = borderLeft + x * elementWith;
                    int top = borderTop + y * elemntHeight;
                    int right = left + elementWith;
                    int bottom = top + elemntHeight;
                    graphics.DrawImage(
                        images[character],
                        left,
                        top,
                        right,
                        bottom,
                        1,
                        true
                    );

                }
            }
            needsRedraw = false;
        }

        private void OverlayWindow_DestroyGraphics(object? sender, DestroyGraphicsEventArgs e)
        {
            foreach (var image in images)
                image?.Dispose();
        }

        ~OSD()
        {
            Dispose(false);
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                overlayWindow.Dispose();

        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
