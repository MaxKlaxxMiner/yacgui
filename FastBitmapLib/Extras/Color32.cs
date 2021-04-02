using System.Drawing;
using System.Runtime.CompilerServices;

namespace FastBitmapLib.Extras
{
  /// <summary>
  /// Helper class for 32-Bit Colors (8-Bit-Channels: alpha, red, green and blue)
  /// </summary>
  public static class Color32
  {
    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="int"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: false = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(int color, bool useAlpha = false)
    {
      return useAlpha
        ? (uint)color                 // use entire value
        : (uint)color | 0xff000000u;  // overwrite alpha channel -> 100% opacity
    }

    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="System.Drawing.Color"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(Color color, bool useAlpha = true)
    {
      return From(color.ToArgb(), useAlpha);
    }
  }
}
