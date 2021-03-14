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
    public static void ShowDrawLines()
    {
      var fastBitmap = new FastBitmap(1024 + 1, 576, 0xff252525);

      for (int x = 0; x < fastBitmap.width; x += 8)
      {
        fastBitmap.DrawLine(x, 0, 0, fastBitmap.height - 1, FastBitmap.ColorBlend(0xff0000, 0x00ff00, (uint)(x * 256 / fastBitmap.width)));
      }
      for (int y = 0; y < fastBitmap.height; y += 5)
      {
        fastBitmap.DrawLine(fastBitmap.width - 1, y, 0, fastBitmap.height - 1, FastBitmap.ColorBlend(0x00ff00, 0x0000ff, (uint)(y * 256 / fastBitmap.height)));
      }

      ShowPicture(fastBitmap.ToGDIBitmap(), "Draw lines");
    }
  }
}
