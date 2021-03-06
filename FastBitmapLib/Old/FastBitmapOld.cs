﻿using System;
using System.Drawing;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public sealed partial class FastBitmapOld
  {
    /// <summary>
    /// Maximum width in pixels
    /// </summary>
    public const int MaxWidth = 10000000;
    /// <summary>
    /// Maximum height int pixels
    /// </summary>
    public const int MaxHeight = 10000000;

    /// <summary>
    /// Width of the image in pixels
    /// </summary>
    public readonly int width;
    /// <summary>
    /// Height of the image in pixels
    /// </summary>
    public readonly int height;
    /// <summary>
    /// Stored pixel data of the bitmap (ARGB-Pixel value: 0xaa112233, aa = Alpha, 11 = Red, 22 = Green, 33 = Blue)
    /// </summary>
    public readonly uint[] pixels;

    /// <summary>
    /// Constructor: Create a bitmap
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public FastBitmapOld(int width, int height, uint backgroundColor = 0x00000000)
    {
      if (width < 1 || width > MaxWidth) throw new ArgumentOutOfRangeException("width");
      if (height < 1 || height > MaxHeight) throw new ArgumentOutOfRangeException("height");
      this.width = width;
      this.height = height;
      pixels = new uint[width * height];
      if (backgroundColor != 0x00000000) Clear(backgroundColor);
    }

    /// <summary>
    /// Constructor: Create a bitmap from GDI-Bitmap
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    public FastBitmapOld(Bitmap bitmap)
    {
      if (bitmap == null) throw new NullReferenceException("bitmap");
      width = bitmap.Width;
      height = bitmap.Height;
      pixels = new uint[width * height];

      CopyFromGDIBitmap(bitmap);
    }

    /// <summary>
    /// Constructor: Create a bitmap from bitmap
    /// </summary>
    /// <param name="FastBitmapOld">Bitmap to be used</param>
    /// <param name="startX">Optional: X-Start from source bitmap (default: 0)</param>
    /// <param name="startY">Optional: Y-Start from source bitmap (default: 0)</param>
    /// <param name="width">Optional: width from source bitmap (default: max)</param>
    /// <param name="height">Optional: height from source bitmap (default: max)</param>
    public FastBitmapOld(FastBitmapOld FastBitmapOld, int startX = 0, int startY = 0, int width = int.MaxValue, int height = int.MaxValue)
    {
      if (FastBitmapOld == null) throw new NullReferenceException("FastBitmapOld");
      if (width > FastBitmapOld.width) width = FastBitmapOld.width;
      if (height > FastBitmapOld.height) height = FastBitmapOld.height;
      if (startX < 0) startX = 0;
      if (startY < 0) startY = 0;
      if (startX >= FastBitmapOld.width) startX = FastBitmapOld.width - 1;
      if (startY >= FastBitmapOld.height) startY = FastBitmapOld.height - 1;
      if (startX + width > FastBitmapOld.width) width = FastBitmapOld.width - startX;
      if (startY + height > FastBitmapOld.height) height = FastBitmapOld.height - startY;

      this.width = width;
      this.height = height;
      pixels = new uint[width * height];

      DrawBitmap(FastBitmapOld, 0, 0, startX, startY, width, height);
    }

    /// <summary>
    /// returns the pixel color (ARGB) from a given position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Color of the pixel</returns>
    public uint GetPixel(int x, int y)
    {
      if ((uint)x >= width || (uint)y >= height) throw new ArgumentOutOfRangeException(); // check ((uint)val >= max)  is the same as:  (val < 0 || val >= max), but faster :)

      return pixels[x + y * width];
    }

    /// <summary>
    /// set the pixel color (ARGB) at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="argbColor">Pixel color (ARGB-value: 0xaa112233, aa = Alpha, 11 = Red, 22 = Green, 33 = Blue)</param>
    public void SetPixel(int x, int y, uint argbColor)
    {
      if ((uint)x >= width || (uint)y >= height) throw new ArgumentOutOfRangeException();

      pixels[x + y * width] = argbColor;
    }

    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color">Fillcolor</param>
    public unsafe void Clear(uint color = 0xff000000)
    {
      fixed (uint* pixelsPtr = pixels)
      {
        FillScanline(pixelsPtr, pixels.Length, color);
      }
    }

    /// <summary>
    /// Returns the properties as a readable string.
    /// </summary>
    /// <returns>Readable string</returns>
    public override string ToString()
    {
      return new { width, height, pixels = "uint[" + pixels.Length + "]" }.ToString();
    }
  }
}
