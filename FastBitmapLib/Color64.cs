using System.Drawing;
using System.Runtime.CompilerServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Helper class for 64-Bit Colors (16-Bit-Channels: alpha, red, green and blue)
  /// </summary>
  public static class Color64
  {
    #region # // --- internals ---
    /// <summary>
    /// Unpack a 32-Bit Value
    /// </summary>
    /// <param name="value32">64-Bit value to unpack</param>
    /// <returns>unpacket 64-Bit value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ulong Unpack32(uint value32)
    {
      ulong t = value32;                      // ........ aarrggbb
      t = (t | t << 16) & 0x0000ffff0000ffff; // ....aarr ....ggbb
      t = (t | t << 8) & 0x00ff00ff00ff00ff;  // ..aa..rr ..gg..bb
      return t | t << 8;                      // aaaarrrr ggggbbbb
    }
    #endregion

    #region # // --- From(int32) ---
    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="uint"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(uint color32, bool useAlpha = true)
    {
      return From(Unpack32(color32), useAlpha);
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="int"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(int color32, bool useAlpha = true)
    {
      return From((uint)color32, useAlpha);
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="uint"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 65535 = 100% opacity)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(uint color32, ushort setAlpha)
    {
      return From(Unpack32(color32), setAlpha);
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="int"/> value
    /// </summary>
    /// <param name="color32">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 65535 = 100% opacity)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(int color32, ushort setAlpha)
    {
      return From((uint)color32, setAlpha);
    }
    #endregion

    #region # // --- From(int64) ---
    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="ulong"/> value
    /// </summary>
    /// <param name="color64">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(ulong color64, bool useAlpha = true)
    {
      return useAlpha
        ? color64                          // use entire value
        : color64 | 0xffff000000000000UL;  // overwrite alpha channel -> 100% opacity
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="ulong"/> value
    /// </summary>
    /// <param name="color64">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 65535 = 100% opacity)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(ulong color64, ushort setAlpha)
    {
      return color64 & 0xffffffffffff | (ulong)setAlpha << 48;
    }
    #endregion

    #region # // --- From(Drawing.Color) ---
    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="System.Drawing.Color"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true, if false = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(Color color, bool useAlpha = true)
    {
      return From(color.ToArgb(), useAlpha);
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="System.Drawing.Color"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="setAlpha">set alpha (0 = transparency, 65535 = 100% opacity)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(Color color, ushort setAlpha)
    {
      return From(color.ToArgb(), setAlpha);
    }
    #endregion

    #region # // --- From(a, r, g, b) ---
    /// <summary>
    /// Get the 64-Bit Color-Code from three color components and alpha channel
    /// </summary>
    /// <param name="a">byte alpha value (0-255)</param>
    /// <param name="r">byte red value (0-255)</param>
    /// <param name="g">byte green value (0-255)</param>
    /// <param name="b">byte blue value (0-255)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(byte a, byte r, byte g, byte b)
    {
      return Unpack32((uint)a << 24 | (uint)r << 16 | (uint)g << 8 | b);
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from three color components and alpha channel
    /// </summary>
    /// <param name="a">ushort alpha value (0-65535)</param>
    /// <param name="r">ushort red value (0-65535)</param>
    /// <param name="g">ushort green value (0-65535)</param>
    /// <param name="b">ushort blue value (0-65535)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(ushort a, ushort r, ushort g, ushort b)
    {
      return (ulong)a << 48 | (ulong)r << 32 | (ulong)g << 16 | b;
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from three color components
    /// </summary>
    /// <param name="r">byte red value (0-255)</param>
    /// <param name="g">byte green value (0-255)</param>
    /// <param name="b">byte blue value (0-255)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(byte r, byte g, byte b)
    {
      return From(byte.MaxValue, r, g, b);
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from three color components
    /// </summary>
    /// <param name="r">ushort red value (0-65535)</param>
    /// <param name="g">ushort green value (0-65535)</param>
    /// <param name="b">ushort blue value (0-65535)</param>
    /// <returns>64-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(ushort r, ushort g, ushort b)
    {
      return From(ushort.MaxValue, r, g, b);
    }
    #endregion

    #region # // --- Blend ---
    /// <summary>
    /// Blend two colors
    /// </summary>
    /// <param name="firstColor">First color</param>
    /// <param name="secondColor">Second color</param>
    /// <param name="amountSecond">Amount of second color (0-65536)</param>
    /// <returns>new color</returns>
    public static ulong BlendFast(ulong firstColor, ulong secondColor, ulong amountSecond)
    {
      ulong amountFirst = 65536UL - amountSecond;
      return (((firstColor & 0xffff0000ffff) * amountFirst + (secondColor & 0xffff0000ffff) * amountSecond) & 0xffff0000ffff0000 // red & blue
            | ((firstColor & 0xffff0000) * amountFirst + (secondColor & 0xffff0000) * amountSecond) & 0xffff00000000             // green
        ) >> 16 | 0xffff000000000000;
    }
    #endregion
  }
}
