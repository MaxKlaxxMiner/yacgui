#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using FastBitmapLib;
using YacGui;
// ReSharper disable MemberCanBePrivate.Global
#endregion

namespace TestTool
{
  /// <summary>
  /// Class for testing bitmap functions
  /// </summary>
  public partial class BitmapTests
  {
    /// <summary>
    /// Draw a testpicture with calculated pixel distances (to simulate a shadow)
    /// </summary>
    public static void ShowPixelDistanceTest()
    {
      var tmpBitmap = new Bitmap(480, 360, PixelFormat.Format32bppRgb);
      var g = Graphics.FromImage(tmpBitmap);
      g.SmoothingMode = SmoothingMode.HighQuality;
      g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(120, 220), new Point(160, 260), new Point(320, 260), new Point(360, 220), new Point(360, 240), new Point(320, 280), new Point(160, 280), new Point(120, 240) });
      g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(180, 220), new Point(220, 200), new Point(260, 200), new Point(300, 220), new Point(240, 140) });
      g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(180, 80), new Point(200, 80), new Point(200, 120) });
      g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(280, 80), new Point(280, 120), new Point(300, 80) });
      g.FillEllipse(new SolidBrush(Color.White), 140, 120, 40, 40);
      g.FillEllipse(new SolidBrush(Color.White), 300, 120, 40, 40);

      var fastBitmap = new FastBitmap(tmpBitmap);

      var bits = new byte[fastBitmap.width * fastBitmap.height];
      for (int i = 0; i < bits.Length; i++) bits[i] = (byte)fastBitmap.pixels[i];

      var distances = DistanceTransform.GenerateMap(bits, fastBitmap.width, fastBitmap.height);

      for (int y = 0; y < fastBitmap.height; y++)
      {
        for (int x = 0; x < fastBitmap.width; x++)
        {
          if (bits[x + y * fastBitmap.width] > 0)
          {
            fastBitmap.SetPixel(x, y, FastBitmap.ColorBlend(0x000000, 0x808080, bits[x + y * fastBitmap.width]));
            continue;
          }
          //uint val = 255 - (uint)Math.Max(0, 255 - distances[x + y * fastBitmap.width] / 2048 * 16);
          uint val = 255 - (uint)Math.Max(0, 255 - distances[x + y * fastBitmap.width] / 128);
          fastBitmap.SetPixel(x, y, 0xff000000 | val << 16 | val << 8 | val);
        }
      }

      tmpBitmap = fastBitmap.ToGDIBitmap();

      ShowPicture(tmpBitmap, "Pixel distance test");
    }
  }
}
