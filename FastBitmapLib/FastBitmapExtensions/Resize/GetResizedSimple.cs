using System;
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  public static partial class FastBitmapExtensions
  {
    /// <summary>
    /// Create a resized bitmap (simple nearest pixel)
    /// </summary>
    /// <param name="bitmap">FastBitmap</param>
    /// <param name="newWidth">Width of the new picture</param>
    /// <param name="newHeight">Height of the nw picture</param>
    /// <param name="constructor">Optional: constructor for the new FastBitmap</param>
    /// <returns>Resized bitmap</returns>
    public static IFastBitmap GetResizedSimple(this IFastBitmap bitmap, int newWidth, int newHeight, Func<int, int, IFastBitmap> constructor = null)
    {
      if (newWidth <= 0 && newHeight <= 0) throw new ArgumentException();

      if (newWidth <= 0) newWidth = newHeight * bitmap.width / bitmap.height;
      if (newHeight <= 0) newHeight = newWidth * bitmap.height / bitmap.width;

      if (constructor == null)
      {
        if (bitmap is IFastBitmap64)
        {
          constructor = (w, h) => new FastBitmap64(w, h);
        }
        else
        {
          constructor = (w, h) => new FastBitmap(w, h);
        }
      }

      var result = constructor(newWidth, newHeight);
      if (result == null) throw new ArgumentNullException("constructor");
      if (result.width != newWidth || result.height != newHeight) throw new ArgumentOutOfRangeException("constructor");

      int cxStep = 256 * bitmap.width / newWidth;
      if (result is IFastBitmap64)
      {
        ulong[] srcLine = new ulong[bitmap.width];
        ulong[] dstLine = new ulong[newWidth];
        for (int y = 0; y < newHeight; y++)
        {
          int cy = y * bitmap.height / newHeight;
          bitmap.ReadScanLine(cy, srcLine);
          for (int x = 0, cx = 0; x < newWidth; x++, cx += cxStep)
          {
            dstLine[x] = srcLine[cx >> 8];
          }
          result.WriteScanLine(y, dstLine);
        }
      }
      else
      {
        uint[] srcLine = new uint[bitmap.width];
        uint[] dstLine = new uint[newWidth];
        for (int y = 0; y < newHeight; y++)
        {
          int cy = y * bitmap.height / newHeight;
          bitmap.ReadScanLine(cy, srcLine);
          for (int x = 0, cx = 0; x < newWidth; x++, cx += cxStep)
          {
            dstLine[x] = srcLine[cx >> 8];
          }
          result.WriteScanLine(y, dstLine);
        }
      }

      return result;
    }
  }
}
