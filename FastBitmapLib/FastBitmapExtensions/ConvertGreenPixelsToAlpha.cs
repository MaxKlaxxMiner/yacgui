// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  public static partial class FastBitmapExtensions
  {
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
