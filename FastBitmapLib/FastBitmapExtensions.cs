
using System.Drawing;
using System.Drawing.Imaging;

namespace FastBitmapLib
{
  public static class FastBitmapExtensions
  {
    /// <summary>
    /// scan bitmap if use alpha-channel
    /// </summary>
    /// <param name="bitmap">Bitmap to scan</param>
    /// <returns>true, if pixels with alpha channel found</returns>
    public static bool HasAlphaPixels(this IFastBitmap bitmap)
    {
      if (bitmap is IFastBitmap32)
      {
        var tmp = new uint[bitmap.width];
        for (int line = 0; line < bitmap.height; line++)
        {
          bitmap.ReadScanLine(line, tmp);
          foreach (uint color in tmp)
          {
            if (color < 0xff000000) return true; // alpha channel found (opacity < 100%)
          }
        }
      }
      else
      {
        var tmp = new ulong[bitmap.width];
        for (int line = 0; line < bitmap.height; line++)
        {
          bitmap.ReadScanLine(line, tmp);
          foreach (ulong color in tmp)
          {
            if (color < 0xffff000000000000) return true; // alpha channel found (opacity < 100%)
          }
        }
      }
      return false; // no alpha channel found
    }

    /// <summary>
    /// Convert the FastBitmap to a GDI-Image
    /// </summary>
    /// <param name="bitmap">FastBitmap to convert</param>
    /// <param name="skipAlphaScan">optional: skip scan for Alpha-Channel (default: false)</param>
    /// <returns>new GDI-Bitmap</returns>
    public static Bitmap ToGDIBitmap(this IFastBitmap bitmap, bool skipAlphaScan = false)
    {
      bool hasAlpha = skipAlphaScan || bitmap.HasAlphaPixels();

      PixelFormat pixelFormat;

      if (bitmap is IFastBitmap32)
      {
        pixelFormat = hasAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb;
      }
      else
      {
        pixelFormat = hasAlpha ? PixelFormat.Format64bppArgb : PixelFormat.Format48bppRgb;
      }

      var result = new Bitmap(bitmap.width, bitmap.height, pixelFormat);
      bitmap.CopyToGDIBitmap(result);
      return result;
    }
  }
}
