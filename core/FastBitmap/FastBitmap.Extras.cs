#region # using *.*

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
#endregion

namespace YacGui
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public partial class FastBitmap
  {
    /// <summary>
    /// Is 64-Bit Process?
    /// </summary>
    static readonly bool x64 = Environment.Is64BitProcess;

    /// <summary>
    /// Convert all green pixels to black with transparency (depending on the green value)
    /// </summary>
    /// <returns>Number of pixels affected</returns>
    public int ConvertGreenPixelsToAlpha()
    {
      int count = 0;

      for (int i = 0; i < pixels.Length; i++)
      {
        uint color = pixels[i];

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
        pixels[i] = color;
      }

      return count;
    }

    /// <summary>
    /// Blend two colors
    /// </summary>
    /// <param name="firstColor">First color</param>
    /// <param name="secondColor">Second color</param>
    /// <param name="amountSecond">Amount of second color (0-256)</param>
    /// <returns>new color</returns>
    public static uint ColorBlendFast(uint firstColor, uint secondColor, uint amountSecond)
    {
      uint amountFirst = 256u - amountSecond;
      return (((firstColor & 0xff0000) * amountFirst + (secondColor & 0xff0000) * amountSecond) & 0xff000000 // red
            | ((firstColor & 0xff00) * amountFirst + (secondColor & 0xff00) * amountSecond) & 0xff0000       // green
             | (firstColor & 0xff) * amountFirst + (secondColor & 0xff) * amountSecond                       // blue
             ) >> 8 | 0xff000000;
    }

    /// <summary>
    /// Blend two colors inclusive alpha channel
    /// </summary>
    /// <param name="firstColor">First color</param>
    /// <param name="secondColor">Second color</param>
    /// <param name="amountSecond">Amount of second color (0-256)</param>
    /// <returns>new color</returns>
    public static uint ColorBlendAlphaFast(uint firstColor, uint secondColor, uint amountSecond)
    {
      uint amountFirst = 256u - amountSecond;
      return (((firstColor & 0xff0000) * amountFirst + (secondColor & 0xff0000) * amountSecond) & 0xff000000 // red
            | ((firstColor & 0xff00) * amountFirst + (secondColor & 0xff00) * amountSecond) & 0xff0000       // green
             | (firstColor & 0xff) * amountFirst + (secondColor & 0xff) * amountSecond                       // blue
      ) >> 8 | (firstColor >> 24) * amountFirst + (secondColor >> 24) * amountSecond >> 8 << 24;             // alpha
    }

    /// <summary>
    /// Blend two colors
    /// </summary>
    /// <param name="firstColor">First color</param>
    /// <param name="secondColor">Second color</param>
    /// <param name="amountSecond">Amount of second color (0-256)</param>
    /// <returns>new color</returns>
    public static uint ColorBlend(uint firstColor, uint secondColor, int amountSecond)
    {
      if (amountSecond < 0) amountSecond = 0;
      if (amountSecond > 256) amountSecond = 256;
      return ColorBlendFast(firstColor, secondColor, (uint)amountSecond);
    }

    /// <summary>
    /// Blend two colors inclusive alpha channel
    /// </summary>
    /// <param name="firstColor">First color</param>
    /// <param name="secondColor">Second color</param>
    /// <param name="amountSecond">Amount of second color (0-256)</param>
    /// <returns>new color</returns>
    public static uint ColorBlendAlpha(uint firstColor, uint secondColor, int amountSecond)
    {
      if (amountSecond < 0) amountSecond = 0;
      if (amountSecond > 256) amountSecond = 256;
      return ColorBlendAlphaFast(firstColor, secondColor, (uint)amountSecond);
    }

    /// <summary>
    /// Blend two colors
    /// </summary>
    /// <param name="firstColor">First color</param>
    /// <param name="secondColor">Second color</param>
    /// <param name="amountSecond">Amount of second color (0.0-1.0)</param>
    /// <returns>new color</returns>
    public static uint ColorBlend(uint firstColor, uint secondColor, double amountSecond)
    {
      return ColorBlend(firstColor, secondColor, (int)(amountSecond * 256.0));
    }

    /// <summary>
    /// Blend two colors inclusive alpha channel
    /// </summary>
    /// <param name="firstColor">First color</param>
    /// <param name="secondColor">Second color</param>
    /// <param name="amountSecond">Amount of second color (0.0-1.0)</param>
    /// <returns>new color</returns>
    public static uint ColorBlendAlpha(uint firstColor, uint secondColor, double amountSecond)
    {
      return ColorBlendAlpha(firstColor, secondColor, (int)(amountSecond * 256.0));
    }
  }
}
