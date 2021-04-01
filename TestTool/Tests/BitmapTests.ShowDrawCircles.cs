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
    /// Draw colored circles
    /// </summary>
    public static void ShowDrawCircles()
    {
      var fastBitmap = new FastBitmap(1024, 576, 0xff252525);

      int maxRadius = (int)Math.Sqrt(fastBitmap.width * fastBitmap.width + fastBitmap.height * fastBitmap.height) / 2;

      for (int r = 0; r < maxRadius; r += 5)
      {
        fastBitmap.DrawCircle(fastBitmap.width / 2, fastBitmap.height / 2, r, FastBitmap.ColorBlend(0x00ff00, 0xff0000, r * 256 / maxRadius));
      }

      ShowPicture(fastBitmap.ToGDIBitmap(), "Draw circles");
    }
  }
}
