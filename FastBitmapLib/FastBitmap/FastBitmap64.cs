﻿#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Drawing;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantUnsafeContext
// ReSharper disable DoNotCallOverridableMethodsInConstructor
// ReSharper disable UnusedType.Global
// ReSharper disable ClassCanBeSealed.Global
#endregion

namespace FastBitmapLib
{
  /// <summary>
  /// Base-Version of FastBitmap
  /// </summary>
  public unsafe class FastBitmap64 : IFastBitmap64
  {
    /// <summary>
    /// pixel-data
    /// </summary>
#if DEBUG
    readonly ulong[] pixels;

    /// <summary>
    /// Max width of the Bitmap
    /// </summary>
    public override int MaxSize { get { return 16777215; } }
#else
    ulong* pixels;

    /// <summary>
    /// Max width of the Bitmap
    /// </summary>
    public override int MaxSize { get { return 100000000; } }
#endif

    #region # // --- Constructors ---
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public FastBitmap64(int width, int height, ulong backgroundColor = 0x0000000000000000)
      : base(width, height, backgroundColor)
    {
#if DEBUG
      pixels = new ulong[(long)width * height];
      if (backgroundColor != 0x0000000000000000) Clear();
#else
      pixels = (ulong*)Marshal.AllocHGlobal((IntPtr)((long)width * height * sizeof(ulong)));
      if (pixels == null) throw new OutOfMemoryException();
      Clear();
#endif
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public FastBitmap64(IFastBitmap bitmap, ulong backgroundColor = 0x0000000000000000)
      : this(bitmap.width, bitmap.height, backgroundColor)
    {
      CopyFromBitmap(bitmap);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public FastBitmap64(Bitmap bitmap, ulong backgroundColor = 0x0000000000000000)
      : this(bitmap.Width, bitmap.Height, backgroundColor)
    {
      CopyFromGDIBitmap(bitmap);
    }
    #endregion

    #region # // --- SetPixel() / GetPixel() ---
    /// <summary>
    /// Set the pixel <see cref="Color64"/> at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color64">Pixel <see cref="Color64"/></param>
    public override void SetPixelUnsafe(int x, int y, ulong color64)
    {
      pixels[x + (long)y * width] = color64;
    }

    /// <summary>
    /// Get the pixel <see cref="Color64"/> from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel <see cref="Color64"/></returns>
    public override ulong GetPixelUnsafe64(int x, int y)
    {
      return pixels[x + (long)y * width];
    }
    #endregion

    #region # // --- Scanline-Methods ---
    /// <summary>
    /// Fill the Scanline with a specific color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public override unsafe void FillScanlineUnsafe(int x, int y, int w, ulong color)
    {
#if DEBUG
      fixed (ulong* ptr = &pixels[x + (long)y * width])
#else
      var ptr = &pixels[x + (long)y * width];
#endif
      {
        for (int i = 0; i < w; i++) ptr[i] = color;
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors. (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override unsafe void WriteScanLineUnsafe(int x, int y, int w, ulong* srcPixels)
    {
#if DEBUG
      fixed (ulong* ptr = &pixels[x + (long)y * width])
#else
      var ptr = &pixels[x + (long)y * width];
#endif
      {
        for (int i = 0; i < w; i++) ptr[i] = srcPixels[i];
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override unsafe void ReadScanLineUnsafe(int x, int y, int w, ulong* destPixels)
    {
#if DEBUG
      fixed (ulong* ptr = &pixels[x + (long)y * width])
#else
      var ptr = &pixels[x + (long)y * width];
#endif
      {
        for (int i = 0; i < w; i++) destPixels[i] = ptr[i];
      }
    }
    #endregion

    #region # // --- IDisposable ---
    /// <summary>
    /// Release unmanaged ressources
    /// </summary>
    public override void Dispose()
    {
#if DEBUG
      // pixels = null; // not needed
#else
      if (pixels != null)
      {
        Marshal.FreeHGlobal((IntPtr)pixels);
        pixels = null;
      }
#endif
    }
    #endregion
  }
}
