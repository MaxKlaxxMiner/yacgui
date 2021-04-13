
using System.Drawing;
using System.Drawing.Imaging;
// ReSharper disable UnusedMethodReturnValue.Global

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

    /// <summary>
    /// Convert all green pixels to black with transparency (depending on the green value, only support 8-Bit per channel)
    /// </summary>
    /// <param name="bitmap">affected FastBitmap</param>
    /// <returns>Number of pixels affected</returns>
    public static int ConvertGreenPixelsToAlpha(this IFastBitmap bitmap)
    {
      int count = 0;

      uint[] pixelsLine = new uint[bitmap.width];
      for (int line = 0; line < bitmap.height; line++)
      {
        bitmap.ReadScanLine(line, pixelsLine);

        for (int i = 0; i < pixelsLine.Length; i++)
        {
          uint color = pixelsLine[i];

          int a = (byte)(color >> 24);
          int r = (byte)(color >> 16);
          int g = (byte)(color >> 8);
          int b = (byte)color;

          if (g > r && g > b) // if green brighter than red and blue -> use alpha
          {
            a = 255 - (g - r + g - b >> 1);
            g = (r + b) >> 1;
            count++;
          }

          color = (uint)(a << 24 | r << 16 | g << 8 | b);
          pixelsLine[i] = color;
        }

        bitmap.WriteScanLine(line, pixelsLine);
      }

      return count;
    }
  }
}
