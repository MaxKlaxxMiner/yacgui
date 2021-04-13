using System.Drawing;
using System.Drawing.Imaging;
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  public static partial class FastBitmapExtensions
  {
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
