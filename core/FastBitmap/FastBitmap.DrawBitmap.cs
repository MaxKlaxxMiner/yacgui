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
    static void CopyScanLine(uint* dst, uint* src, int count)
    {
      count--;
      for (int i = 0; i < count; i += 2)
      {
        *(ulong*)(dst + i) = *(ulong*)(src + i); // copy two pixels at once
      }
      if ((count & 1) == 0)
      {
        dst[count] = src[count]; // copy last pixel if necessary
      }
    }

    public void DrawBitmap(FastBitmap spriteBitmap, int x = 0, int y = 0, int spriteX = 0, int spriteY = 0, int spriteWidth = int.MaxValue, int spriteHeight = int.MaxValue)
    {
      if (spriteBitmap == null) throw new NullReferenceException("spriteBitmap");

      if ((uint)spriteX >= spriteBitmap.width) throw new ArgumentOutOfRangeException("spriteX");
      if ((uint)spriteY >= spriteBitmap.height) throw new ArgumentOutOfRangeException("spriteY");
      if ((uint)(spriteX + spriteWidth) >= spriteBitmap.width) spriteWidth = spriteBitmap.width - spriteX;
      if ((uint)(spriteY + spriteHeight) >= spriteBitmap.height) spriteHeight = spriteBitmap.height - spriteY;

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
      fixed (uint* srcPtr = &spriteBitmap.pixels[spriteX + spriteY * spriteBitmap.width])
      {
        var dstP = dstPtr;
        var srcP = srcPtr;
        for (int line = 0; line < spriteHeight; line++)
        {
          CopyScanLine(dstP, srcP, spriteWidth);
          dstP += width;
          srcP += spriteBitmap.width;
        }
      }
    }

    static void CopyScanLineAlpha(uint* dst, uint* src, int count)
    {
      for (int i = 0; i < count; i++)
      {
        uint dstColor = dst[i];
        uint srcColor = src[i];
        dstColor = ColorBlendAlphaFast(dstColor, srcColor, srcColor >> 24);
        dst[i] = dstColor;
      }
    }

    public void DrawBitmapAlpha(FastBitmap spriteBitmap, int x = 0, int y = 0, int spriteX = 0, int spriteY = 0, int spriteWidth = int.MaxValue, int spriteHeight = int.MaxValue)
    {
      if (spriteBitmap == null) throw new NullReferenceException("spriteBitmap");

      if ((uint)spriteX >= spriteBitmap.width) throw new ArgumentOutOfRangeException("spriteX");
      if ((uint)spriteY >= spriteBitmap.height) throw new ArgumentOutOfRangeException("spriteY");
      if ((uint)(spriteX + spriteWidth) >= spriteBitmap.width) spriteWidth = spriteBitmap.width - spriteX;
      if ((uint)(spriteY + spriteHeight) >= spriteBitmap.height) spriteHeight = spriteBitmap.height - spriteY;

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
      fixed (uint* srcPtr = &spriteBitmap.pixels[spriteX + spriteY * spriteBitmap.width])
      {
        var dstP = dstPtr;
        var srcP = srcPtr;
        for (int line = 0; line < spriteHeight; line++)
        {
          CopyScanLineAlpha(dstP, srcP, spriteWidth);
          dstP += width;
          srcP += spriteBitmap.width;
        }
      }
    }
  }
}
