#region # using *.*

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable JoinDeclarationAndInitializer
#endregion

namespace YacGui
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public unsafe partial class FastBitmap
  {
    /// <summary>
    /// Draw a circle
    /// </summary>
    /// <param name="x">Center x-position</param>
    /// <param name="y">Center y-position</param>
    /// <param name="r">Radius</param>
    /// <param name="color">Color</param>
    public void DrawCircle(int x, int y, int r, uint color)
    {
      fixed (uint* ptr = pixels)
      {
        int f = 1 - r;
        int ddF_x = 1;
        int ddF_y = -2 * r;
        int px = 0;
        int py = r;

        if ((uint)x < width)
        {
          if ((uint)(y + r) < height) ptr[x + (y + r) * width] = color;
          if ((uint)(y - r) < height) ptr[x + (y - r) * width] = color;
        }
        if ((uint)y < height)
        {
          if ((uint)(x + r) < width) ptr[x + r + y * width] = color;
          if ((uint)(x - r) < width) ptr[x - r + y * width] = color;
        }

        while (px < py)
        {
          if (f >= 0)
          {
            py--;
            ddF_y += 2;
            f += ddF_y;
          }
          px++;
          ddF_x += 2;
          f += ddF_x;

          if ((uint)(x + px) < width && (uint)(y + py) < height) ptr[x + px + (y + py) * width] = color;
          if ((uint)(x - px) < width && (uint)(y + py) < height) ptr[x - px + (y + py) * width] = color;
          if ((uint)(x + px) < width && (uint)(y - py) < height) ptr[x + px + (y - py) * width] = color;
          if ((uint)(x - px) < width && (uint)(y - py) < height) ptr[x - px + (y - py) * width] = color;
          if ((uint)(x + py) < width && (uint)(y + px) < height) ptr[x + py + (y + px) * width] = color;
          if ((uint)(x - py) < width && (uint)(y + px) < height) ptr[x - py + (y + px) * width] = color;
          if ((uint)(x + py) < width && (uint)(y - px) < height) ptr[x + py + (y - px) * width] = color;
          if ((uint)(x - py) < width && (uint)(y - px) < height) ptr[x - py + (y - px) * width] = color;
        }
      }
    }
  }
}
