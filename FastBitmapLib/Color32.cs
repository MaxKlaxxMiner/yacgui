using System.Drawing;
using System.Runtime.CompilerServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Helper class for 32-Bit Colors (8-Bit-Channels: alpha, red, green and blue)
  /// </summary>
  public static class Color32
  {
    #region # // --- internals ---
    /// <summary>
    /// Pack a 64-Bit value to 32-Bit, with byte interleaving (truncate lower bits)
    /// </summary>
    /// <param name="value64">64-Bit value to pack</param>
    /// <returns>packet 32-Bit value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint Pack32(ulong value64)
    {
      ulong t = value64;                      // aaaarrrrggggbbbb
      t = (t & 0xff00ff00ff00ff00) >> 8;      // ..aa..rr..gg..bb
      t = (t | t >> 8) & 0x0000ffff0000ffff;  // ....aarr....ggbb
      return (uint)(t | t >> 16);             // ........aarrggbb
    }
    #endregion

    #region # // --- From(int32) ---
    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="uint"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(uint color32, bool useAlpha = true)
    {
      return useAlpha
        ? color32                 // use entire value
        : color32 | 0xff000000u;  // overwrite alpha channel -> 100% opacity
    }

    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="int"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(int color32, bool useAlpha = true)
    {
      return From((uint)color32, useAlpha);
    }

    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="uint"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 255 = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(uint color32, byte setAlpha)
    {
      return color32 & 0xffffff | (uint)setAlpha << 24;
    }

    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="int"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 255 = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(int color32, byte setAlpha)
    {
      return From((uint)color32, setAlpha);
    }
    #endregion

    #region # // --- From(int64) ---
    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="ulong"/> value
    /// </summary>
    /// <param name="color64">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(ulong color64, bool useAlpha = true)
    {
      return From(Pack32(color64), useAlpha);
    }

    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="ulong"/> value
    /// </summary>
    /// <param name="color64">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 255 = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(ulong color64, byte setAlpha)
    {
      return From(Pack32(color64), setAlpha);
    }
    #endregion

    #region # // --- From(Drawing.Color) ---
    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="System.Drawing.Color"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(Color color, bool useAlpha = true)
    {
      return From(color.ToArgb(), useAlpha);
    }

    /// <summary>
    /// Get the 32-Bit Color-Code from a <see cref="System.Drawing.Color"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 255 = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(Color color, byte setAlpha)
    {
      return From(color.ToArgb(), setAlpha);
    }
    #endregion

    #region # // --- From(a, r, g, b) ---
    /// <summary>
    /// Get the 32-Bit Color-Code from three color components and alpha channel
    /// </summary>
    /// <param name="a">byte alpha value (0-255)</param>
    /// <param name="r">byte red value (0-255)</param>
    /// <param name="g">byte green value (0-255)</param>
    /// <param name="b">byte blue value (0-255)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(byte a, byte r, byte g, byte b)
    {
      return (uint)a << 24 | (uint)r << 16 | (uint)g << 8 | b;
    }

    /// <summary>
    /// Get the 32-Bit Color-Code from three color components
    /// </summary>
    /// <param name="r">byte red value (0-255)</param>
    /// <param name="g">byte green value (0-255)</param>
    /// <param name="b">byte blue value (0-255)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint From(byte r, byte g, byte b)
    {
      return From(byte.MaxValue, r, g, b);
    }
    #endregion
  }
}
