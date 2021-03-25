#region # using *.*

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
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
    /// Draw a bitmap
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to be drawn</param>
    /// <param name="x">Optional: x-position to draw (default: 0)</param>
    /// <param name="y">Optional: y-position to draw (default: 0)</param>
    /// <param name="spriteX">Optional: x-position for part of srcBitmap (default: 0)</param>
    /// <param name="spriteY">Optional: y-position for part of srcBitmap (default: 0)</param>
    /// <param name="spriteWidth">Optional: width for part of srcBitmap (default: full with)</param>
    /// <param name="spriteHeight">Optional: height for part of srcBitmap (default: full height)</param>
    public void DrawBitmap(FastBitmap srcBitmap, int x = 0, int y = 0, int spriteX = 0, int spriteY = 0, int spriteWidth = int.MaxValue, int spriteHeight = int.MaxValue)
    {
      if (srcBitmap == null) throw new NullReferenceException("srcBitmap");

      if ((uint)spriteX >= srcBitmap.width) throw new ArgumentOutOfRangeException("spriteX");
      if ((uint)spriteY >= srcBitmap.height) throw new ArgumentOutOfRangeException("spriteY");
      if ((uint)(spriteX + spriteWidth) >= srcBitmap.width) spriteWidth = srcBitmap.width - spriteX;
      if ((uint)(spriteY + spriteHeight) >= srcBitmap.height) spriteHeight = srcBitmap.height - spriteY;

      if (x < 0) // cut left
      {
        spriteWidth += x;
        if (spriteWidth <= 0) return; // complete left outside
        spriteX -= x;
        x = 0;
      }
      if (y < 0) // cut top
      {
        spriteHeight += y;
        if (spriteHeight <= 0) return; // complete top outside
        spriteY -= y;
        y = 0;
      }
      if (x + spriteWidth > width) // cut right
      {
        spriteWidth = width - x;
        if (spriteWidth <= 0) return; // complete right outside
      }
      if (y + spriteHeight > height) // cur bottom
      {
        spriteHeight = height - y;
        if (spriteHeight <= 0) return; // complete bottom outside
      }

      fixed (uint* dstPtr = &pixels[x + y * width])
      fixed (uint* srcPtr = &srcBitmap.pixels[spriteX + spriteY * srcBitmap.width])
      {
        var dstP = dstPtr;
        var srcP = srcPtr;
        for (int line = 0; line < spriteHeight; line++)
        {
          CopyScanLine(dstP, srcP, spriteWidth);
          dstP += width;
          srcP += srcBitmap.width;
        }
      }
    }

    /// <summary>
    /// Draw a bitmap with alpha channel
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to be drawn</param>
    /// <param name="x">Optional: x-position to draw (default: 0)</param>
    /// <param name="y">Optional: y-position to draw (default: 0)</param>
    /// <param name="spriteX">Optional: x-position for part of srcBitmap (default: 0)</param>
    /// <param name="spriteY">Optional: y-position for part of srcBitmap (default: 0)</param>
    /// <param name="spriteWidth">Optional: width for part of srcBitmap (default: full with)</param>
    /// <param name="spriteHeight">Optional: height for part of srcBitmap (default: full height)</param>
    public void DrawBitmapAlpha(FastBitmap srcBitmap, int x = 0, int y = 0, int spriteX = 0, int spriteY = 0, int spriteWidth = int.MaxValue, int spriteHeight = int.MaxValue)
    {
      if (srcBitmap == null) throw new NullReferenceException("srcBitmap");

      if ((uint)spriteX >= srcBitmap.width) throw new ArgumentOutOfRangeException("spriteX");
      if ((uint)spriteY >= srcBitmap.height) throw new ArgumentOutOfRangeException("spriteY");
      if ((uint)(spriteX + spriteWidth) >= srcBitmap.width) spriteWidth = srcBitmap.width - spriteX;
      if ((uint)(spriteY + spriteHeight) >= srcBitmap.height) spriteHeight = srcBitmap.height - spriteY;

      if (x < 0) // cut left
      {
        spriteWidth += x;
        if (spriteWidth <= 0) return; // complete left outside
        spriteX -= x;
        x = 0;
      }
      if (y < 0) // cut top
      {
        spriteHeight += y;
        if (spriteHeight <= 0) return; // complete top outside
        spriteY -= y;
        y = 0;
      }
      if (x + spriteWidth > width) // cut right
      {
        spriteWidth = width - x;
        if (spriteWidth <= 0) return; // complete right outside
      }
      if (y + spriteHeight > height) // cur bottom
      {
        spriteHeight = height - y;
        if (spriteHeight <= 0) return; // complete bottom outside
      }

      fixed (uint* dstPtr = &pixels[x + y * width])
      fixed (uint* srcPtr = &srcBitmap.pixels[spriteX + spriteY * srcBitmap.width])
      {
        var dstP = dstPtr;
        var srcP = srcPtr;
        for (int line = 0; line < spriteHeight; line++)
        {
          CopyScanLineAlpha(dstP, srcP, spriteWidth);
          dstP += width;
          srcP += srcBitmap.width;
        }
      }
    }
  }
}
