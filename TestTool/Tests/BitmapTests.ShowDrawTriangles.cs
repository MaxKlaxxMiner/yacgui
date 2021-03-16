#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
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
    public static void ShowDrawTriangles()
    {
      var fastBitmap = new FastBitmap(1024, 576, 0xff252525);

      fastBitmap.FillTriangle(200, 100, 350, 450, 100, 250, 0xff0088ff);

      fastBitmap.DrawLine(200, 100, 350, 450, 0xffffffff);
      fastBitmap.DrawLine(350, 450, 100, 250, 0xffffffff);
      fastBitmap.DrawLine(100, 250, 200, 100, 0xffffffff);

      const int rStep = 10;
      const int ofsX = 736;
      const int ofsY = 288;
      const int radius = 260;

      uint[] colors = { 0xffff00, 0xff0000, 0x0000ff, 0x00ff00 };
      int degPerColor = 360 / colors.Length;

      for (int r = 0; r < 360; r += rStep)
      {
        uint color = FastBitmap.ColorBlend(colors[r / degPerColor % colors.Length], colors[(r / degPerColor + 1) % colors.Length], r * 256 / degPerColor % 256);

        fastBitmap.FillTriangle((int)(Math.Sin(r / 180.0 * Math.PI) * radius) + ofsX, (int)(-Math.Cos(r / 180.0 * Math.PI) * radius) + ofsY,
          (int)(Math.Sin((r + rStep) / 180.0 * Math.PI) * radius) + ofsX, (int)(-Math.Cos((r + rStep) / 180.0 * Math.PI) * radius) + ofsY,
          ofsX, ofsY, color);
      }

      ShowPicture(fastBitmap.ToGDIBitmap(), "Draw triangles");
    }
  }
}
