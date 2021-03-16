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
    public static void ShowTextureMappedSimple()
    {
      var fastBitmap = new FastBitmap(1024, 576, 0xff252525);

      Func<double, double, uint> map = (u, v) => FastBitmap.ColorBlend(FastBitmap.ColorBlend(0xff0000, 0x00ff00, u), FastBitmap.ColorBlend(0x0000ff, 0x000000, u), v);

      fastBitmap.MappedTriangle(50, 50, 100, 400, 150, 500, 0, 0, 0, 1, 1, 1, map);
      fastBitmap.MappedTriangle(50, 50, 300, 100, 150, 500, 0, 0, 1, 0, 1, 1, map);

      fastBitmap.MappedTriangle(350, 50, 600, 100, 400, 400, 0, 0, 1, 0, 0, 1, map);
      fastBitmap.MappedTriangle(450, 500, 600, 100, 400, 400, 1, 1, 1, 0, 0, 1, map);

      fastBitmap.MappedQuad(650, 50, 900, 100, 750, 500, 700, 400, 0, 0, 1, 0, 1, 1, 0, 1, map);

      ShowPicture(fastBitmap.ToGDIBitmap(), "Simple Textured");
    }
  }
}
