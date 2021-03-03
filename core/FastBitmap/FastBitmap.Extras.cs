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
  }
}
