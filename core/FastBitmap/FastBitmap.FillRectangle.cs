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
    /// Draw a filled rectangle
    /// </summary>
    /// <param name="x">x-position (left side)</param>
    /// <param name="y">y-position (top edge)</param>
    /// <param name="width">Width of the rectangle</param>
    /// <param name="height">Height of the rectangle</param>
    /// <param name="color">Fill color</param>
    public void FillRectangle(int x, int y, int width, int height, uint color)
    {
      if (x < 0) { width += x; x = 0; }
      if (y < 0) { height += y; y = 0; }
      if (x + width > this.width) { width = this.width - x; }
      if (y + height > this.height) { height = this.height - y; }

      fixed (uint* pixelsPtr = &pixels[x + y * this.width])
      {
        for (int line = 0; line < height; line++)
        {
          FillScanline(pixelsPtr + line * this.width, width, color);
        }
      }
    }
  }
}
