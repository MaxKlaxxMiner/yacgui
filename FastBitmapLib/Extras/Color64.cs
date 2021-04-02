using System.Drawing;
using System.Runtime.CompilerServices;

namespace FastBitmapLib.Extras
{
  /// <summary>
  /// Helper class for 64-Bit Colors (16-Bit-Channels: alpha, red, green and blue)
  /// </summary>
  public static class Color64
  {
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

    /// <summary>
    /// Pack a 64-Bit value to 32-Bit, with byte interleaving (truncate lower bits)
    /// </summary>
    /// <param name="value64">64-Bit value to pack</param>
    /// <returns>packet 32-Bit value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint Pack32(ulong value64)
    {
      ulong t = value64;                      // aaaarrrrggggbbbb
      t = t >> 8 & 0xff00ff00ff00ff00;        // ..aa..rr..gg..bb
      t = (t | t >> 8) & 0x0000ffff0000ffff;  // ....aarr....ggbb
      return (uint)(t | t >> 16);             // ........aarrggbb
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="int"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: false = 100% opacity)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(int color, bool useAlpha = false)
    {
      return Unpack32(Color32.From(color, useAlpha));
    }

    /// <summary>
    /// Get the 64-Bit Color-Code from a <see cref="System.Drawing.Color"/> value
    /// </summary>
    /// <param name="color">selected color</param>
    /// <param name="useAlpha">Optional: use alpha channel from color (default: true)</param>
    /// <returns>32-Bit Color-Code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong From(Color color, bool useAlpha = true)
    {
      return From(color.ToArgb(), useAlpha);
    }
  }
}
