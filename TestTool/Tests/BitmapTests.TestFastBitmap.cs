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
    /// Check the basics of the <see cref="FastBitmapOld"/> class
    /// </summary>
    public static void TestFastBitmap()
    {
      var bitmap = new Bitmap(10, 5, PixelFormat.Format32bppArgb);
      bitmap.SetPixel(0, 0, Color.Blue);  // set blue pixel (0xff0000ff) at the beginning of the image (top left)
      bitmap.SetPixel(1, 0, Color.Green); // set green pixel (0xff00ff00) in the next column
      bitmap.SetPixel(0, 1, Color.Red);   // set red pixel (0xffff0000) at the beginning of the next line

      var fastBitmap = new FastBitmapOld(bitmap);
      Debug.Assert(fastBitmap.width == 10 && fastBitmap.height == 5);
      Debug.Assert(fastBitmap.GetPixel(0, 0) == (uint)Color.Blue.ToArgb());
      Debug.Assert(fastBitmap.GetPixel(1, 0) == (uint)Color.Green.ToArgb());
      Debug.Assert(fastBitmap.GetPixel(0, 1) == (uint)Color.Red.ToArgb());

      fastBitmap.SetPixel(9, 4, 0xff0066ff); // Places a light blue pixel in the bottom right corner of the image

      var newBitmap = fastBitmap.ToGDIBitmap();
      Debug.Assert(newBitmap.Width == 10 && newBitmap.Height == 5);
      Debug.Assert((uint)newBitmap.GetPixel(9, 4).ToArgb() == 0xff0066ff);

      bitmap.Dispose();
      newBitmap.Dispose();
    }
  }
}
