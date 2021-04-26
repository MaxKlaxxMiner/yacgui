#region # using *.*
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
  public unsafe class FastBitmap : IFastBitmap32
  {
    /// <summary>
    /// pixel-data
    /// </summary>
#if DEBUG
    readonly uint[] pixels;

    /// <summary>
    /// Max width of the Bitmap
    /// </summary>
    public override int MaxSize { get { return 33554431; } }
#else
    uint* pixels;

    /// <summary>
    /// Max width of the Bitmap
    /// </summary>
    public override int MaxSize { get { return 200000000; } }
#endif

    #region # // --- Constructors ---
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public FastBitmap(int width, int height, uint backgroundColor = 0x00000000)
      : base(width, height, backgroundColor)
    {
      long size = (long)width * height;
#if DEBUG
      pixels = new uint[size];
      if (backgroundColor != 0x00000000) Clear();
#else
      pixels = (uint*)Marshal.AllocHGlobal((IntPtr)(size * sizeof(uint)));
      if (pixels == null) throw new OutOfMemoryException();
      Clear();
#endif
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public FastBitmap(IFastBitmap bitmap, uint backgroundColor = 0x00000000)
      : this(bitmap.width, bitmap.height, backgroundColor)
    {
      CopyFromBitmap(bitmap);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public FastBitmap(Bitmap bitmap, uint backgroundColor = 0x00000000)
      : this(bitmap.Width, bitmap.Height, backgroundColor)
    {
      CopyFromGDIBitmap(bitmap);
    }
    #endregion

    #region # // --- SetPixel() / GetPixel() ---
    /// <summary>
    /// Set the pixel <see cref="Color32"/> at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color32">Pixel <see cref="Color32"/></param>
    public override void SetPixelUnsafe(int x, int y, uint color32)
    {
      pixels[x + (long)y * width] = color32;
    }

    /// <summary>
    /// Get the pixel <see cref="Color32"/> from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel <see cref="Color32"/></returns>
    public override uint GetPixelUnsafe32(int x, int y)
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
    public override unsafe void FillScanlineUnsafe(int x, int y, int w, uint color)
    {
#if DEBUG
      fixed (uint* ptr = &pixels[x + y * width])
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
    public override unsafe void WriteScanLineUnsafe(int x, int y, int w, uint* srcPixels)
    {
#if DEBUG
      fixed (uint* ptr = &pixels[x + y * width])
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
    public override unsafe void ReadScanLineUnsafe(int x, int y, int w, uint* destPixels)
    {
#if DEBUG
      fixed (uint* ptr = &pixels[x + y * width])
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
