// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  public static partial class FastBitmapExtensions
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
  }
}
 