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
using FastBitmapLib.Extras;
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
    static void TestFastBitmap32()
    {
      var bitmap = new Bitmap(10, 5, PixelFormat.Format32bppArgb);
      bitmap.SetPixel(0, 0, Color.Blue);  // set blue pixel (0xff0000ff) at the beginning of the image (top left)
      bitmap.SetPixel(1, 0, Color.Green); // set green pixel (0xff008000) in the next column
      bitmap.SetPixel(0, 1, Color.Red);   // set red pixel (0xffff0000) at the beginning of the next line

      var fastBitmap = new FastBitmap(bitmap);
      Debug.Assert(fastBitmap.width == 10 && fastBitmap.height == 5);
      Debug.Assert(fastBitmap.GetPixel32(0, 0) == Color32.From(Color.Blue));
      Debug.Assert(fastBitmap.GetPixel32(1, 0) == Color32.From(Color.Green));
      Debug.Assert(fastBitmap.GetPixel32(0, 1) == Color32.From(Color.Red));

      fastBitmap.SetPixel(9, 4, 0xff0066ff); // Places a light blue pixel in the bottom right corner of the image

      var newBitmap = fastBitmap.ToGDIBitmap();
      Debug.Assert(newBitmap.Width == 10 && newBitmap.Height == 5);
      Debug.Assert(Color32.From(newBitmap.GetPixel(9, 4)) == 0xff0066ff); // check light blue pixel in the bottom right corner

      bitmap.Dispose();
      newBitmap.Dispose();
    }

    static void TestFastBitmap3264()
    {
      var bitmap = new Bitmap(10, 5, PixelFormat.Format32bppArgb);
      bitmap.SetPixel(0, 0, Color.Blue);  // set blue pixel (0xff0000ff) at the beginning of the image (top left)
      bitmap.SetPixel(1, 0, Color.Green); // set green pixel (0xff008000) in the next column
      bitmap.SetPixel(0, 1, Color.Red);   // set red pixel (0xffff0000) at the beginning of the next line

      var fastBitmap = new FastBitmap64(bitmap);
      Debug.Assert(fastBitmap.width == 10 && fastBitmap.height == 5);
      Debug.Assert(fastBitmap.GetPixel32(0, 0) == Color32.From(Color.Blue));
      Debug.Assert(fastBitmap.GetPixel32(1, 0) == Color32.From(Color.Green));
      Debug.Assert(fastBitmap.GetPixel32(0, 1) == Color32.From(Color.Red));

      fastBitmap.SetPixel(9, 4, 0xff0066ff); // Places a light blue pixel in the bottom right corner of the image

      var newBitmap = fastBitmap.ToGDIBitmap();
      Debug.Assert(newBitmap.PixelFormat == PixelFormat.Format64bppArgb);
      Debug.Assert(newBitmap.Width == 10 && newBitmap.Height == 5);
      Debug.Assert(Color32.From(newBitmap.GetPixel(9, 4)) == 0xff0066ff); // check light blue pixel in the bottom right corner

      bitmap.Dispose();
      newBitmap.Dispose();
    }

    static void TestFastBitmap64()
    {
      var bitmap = new Bitmap(10, 5, PixelFormat.Format64bppArgb);
      Debug.Assert(bitmap.PixelFormat == PixelFormat.Format64bppArgb);
      bitmap.SetPixel(0, 0, Color.Blue);  // set blue pixel (0xff0000ff) at the beginning of the image (top left)
      bitmap.SetPixel(1, 0, Color.Green); // set green pixel (0xff008000) in the next column
      bitmap.SetPixel(0, 1, Color.Red);   // set red pixel (0xffff0000) at the beginning of the next line

      var fastBitmap = new FastBitmap64(bitmap);
      Debug.Assert(fastBitmap.width == 10 && fastBitmap.height == 5);
      Debug.Assert(fastBitmap.GetPixel32(0, 0) == Color32.From(Color.Blue));
      Debug.Assert(fastBitmap.GetPixel32(1, 0) == Color32.From(Color.Green));
      Debug.Assert(fastBitmap.GetPixel32(0, 1) == Color32.From(Color.Red));

      fastBitmap.SetPixel(9, 4, 0xff0066ff); // Places a light blue pixel in the bottom right corner of the image

      var newBitmap = fastBitmap.ToGDIBitmap();
      Debug.Assert(newBitmap.PixelFormat == PixelFormat.Format64bppArgb);
      Debug.Assert(newBitmap.Width == 10 && newBitmap.Height == 5);
      Debug.Assert(Color32.From(newBitmap.GetPixel(9, 4)) == 0xff0066ff); // check light blue pixel in the bottom right corner

      bitmap.Dispose();
      newBitmap.Dispose();
    }

    /// <summary>
    /// Check the basics of the <see cref="FastBitmap"/> class
    /// </summary>
    public static void TestFastBitmap()
    {
      TestFastBitmap32();
      TestFastBitmap3264();
      TestFastBitmap64();
    }
  }
}
