//#define C32

#region # using *.*
using System;
using System.Drawing;
using System.Drawing.Imaging;
using FastBitmapLib.Extras;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable VirtualMemberNeverOverriden.Global
// ReSharper disable BuiltInTypeReferenceStyle
#endregion

#if C32
using ColorType = System.UInt32;
using ColorTypeB = System.UInt64;
#else
using ColorType = System.UInt64;
using ColorTypeB = System.UInt32;
#endif

namespace FastBitmapLib
{
#if C32
  /// <summary>
  /// simple version of abstract Main class for FastBitmap with 32-Bit Pixels: <see cref="Color32"/>
  /// </summary>
  public abstract unsafe class IFastBitmapSimple32 : IFastBitmap32
#else
  /// <summary>
  /// simple version of abstract Main class for FastBitmap with 64-Bit Pixels: <see cref="Color64"/>
  /// </summary>
  public abstract unsafe class IFastBitmapSimple64 : IFastBitmap64
#endif
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
#if C32
    protected IFastBitmapSimple32(int width, int height, ColorType backgroundColor = 0x00000000) : base(width, height, backgroundColor) { }
#else
    protected IFastBitmapSimple64(int width, int height, ColorType backgroundColor = 0x0000000000000000) : base(width, height, backgroundColor) { }
#endif

    #region # // --- basic methods ---

    #region # // --- ColorType (primary color type) ---
    /// <summary>
    /// Fill the Scanline with a specific color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public override void FillScanlineUnsafe(int x, int y, int w, ColorType color)
    {
      for (int i = 0; i < w; i++)
      {
        SetPixelUnsafe(x + i, y, color);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors. (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLineUnsafe(int x, int y, int w, ColorType* srcPixels)
    {
      for (int i = 0; i < w; i++)
      {
        SetPixelUnsafe(x + i, y, srcPixels[i]);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLineUnsafe(int x, int y, int w, ColorType* destPixels)
    {
      for (int i = 0; i < w; i++)
      {
#if C32
        destPixels[i] = GetPixelUnsafe32(x + i, y);
#else
        destPixels[i] = GetPixelUnsafe64(x + i, y);
#endif
      }
    }
    #endregion

    #endregion
  }

#if C32
  /// <summary>
  /// abstract Main class for FastBitmap with 32-Bit Pixels: <see cref="Color32"/>
  /// </summary>
  public abstract unsafe class IFastBitmap32 : IFastBitmap
#else
  /// <summary>
  /// abstract Main class for FastBitmap with 64-Bit Pixels: <see cref="Color64"/>
  /// </summary>
  public abstract unsafe class IFastBitmap64 : IFastBitmap
#endif
  {
    /// <summary>
    /// <see cref="ColorType"/> of the background
    /// </summary>
    public ColorType backgroundColor;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
#if C32
    protected IFastBitmap32(int width, int height, ColorType backgroundColor = 0x00000000)
      : base(width, height)
#else
    protected IFastBitmap64(int width, int height, ColorType backgroundColor = 0x0000000000000000)
      : base(width, height)
#endif
    {
      this.backgroundColor = backgroundColor;
    }

    #region # // --- basic methods ---

    #region # // --- ColorType (primary color type) ---
    /// <summary>
    /// Set the pixel color at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public override void SetPixel(int x, int y, ColorType color)
    {
      if ((uint)x >= width || (uint)y >= height) return;
      SetPixelUnsafe(x, y, color);
    }

    /// <summary>
    /// Get the pixel color from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
#if C32
    public override ColorType GetPixel32(int x, int y)
    {
      if ((uint)x >= width || (uint)y >= height) return backgroundColor;
      return GetPixelUnsafe32(x, y);
    }
#else
    public override ColorType GetPixel64(int x, int y)
    {
      if ((uint)x >= width || (uint)y >= height) return backgroundColor;
      return GetPixelUnsafe64(x, y);
    }
#endif

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public override void FillScanline(int x, int y, int w, ColorType color)
    {
      if ((uint)y >= height) return;

      if (x < 0)
      {
        w += x;
        x = 0;
      }

      if (x + w > width)
      {
        w = width - x;
      }

      if (w < 1) return;

      FillScanlineUnsafe(x, y, w, color);
    }

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">fill-color</param>
    public override void FillScanline(int y, ColorType color)
    {
      if ((uint)y >= height) return;
      FillScanlineUnsafe(0, y, width, color);
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int x, int y, int w, ColorType* srcPixels)
    {
      if ((uint)y >= height) return;

      if (x < 0)
      {
        w += x;
        srcPixels -= x;
        x = 0;
      }

      if (x + w > width)
      {
        w = width - x;
      }

      if (w < 1) return;

      WriteScanLineUnsafe(x, y, w, srcPixels);
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public override void WriteScanLine(int x, int y, int w, ColorType[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + w > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 1 || (uint)y >= height) return;
      fixed (ColorType* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLine(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int y, ColorType* srcPixels)
    {
      if ((uint)y >= height) return;
      WriteScanLineUnsafe(0, y, width, srcPixels);
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public override void WriteScanLine(int y, ColorType[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + width > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if ((uint)y >= height) return;
      fixed (ColorType* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLineUnsafe(0, y, width, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int x, int y, int w, ColorType* destPixels)
    {
      if ((uint)y >= height)
      {
        for (int i = 0; i < w; i++) destPixels[i] = backgroundColor;
        return;
      }

      if (x < 0)
      {
        int end = Math.Min(-x, w);
        for (int i = 0; i < end; i++) destPixels[i] = backgroundColor;
        w += x;
        destPixels -= x;
        x = 0;
      }

      if (x + w > width)
      {
        for (int i = Math.Max(0, width - x); i < w; i++) destPixels[i] = backgroundColor;
        w = width - x;
      }

      if (w < 1) return;

      ReadScanLineUnsafe(x, y, w, destPixels);
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public override void ReadScanLine(int x, int y, int w, ColorType[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + w > destPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 1) return;
      fixed (ColorType* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int y, ColorType* destPixels)
    {
      ReadScanLine(0, y, width, destPixels);
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public override void ReadScanLine(int y, ColorType[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + width > destPixels.Length) throw new ArgumentOutOfRangeException();
      fixed (ColorType* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(y, ptr);
      }
    }
    #endregion

    #region # // --- ColorTypeB (secondary compatibility color type) ---
    /// <summary>
    /// Set the pixel color at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public override void SetPixelUnsafe(int x, int y, ColorTypeB color)
    {
      SetPixelUnsafe(x, y, Conv(color));
    }

    /// <summary>
    /// Set the pixel color at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public override void SetPixel(int x, int y, ColorTypeB color)
    {
      SetPixel(x, y, Conv(color));
    }

    /// <summary>
    /// Get the pixel color from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
#if C32
    public override ColorTypeB GetPixelUnsafe64(int x, int y)
    {
      return Conv(GetPixelUnsafe32(x, y));
    }
#else
    public override ColorTypeB GetPixelUnsafe32(int x, int y)
    {
      return Conv(GetPixelUnsafe64(x, y));
    }
#endif

    /// <summary>
    /// Get the pixel color from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
#if C32
    public override ColorTypeB GetPixel64(int x, int y)
    {
      if ((uint)x >= width || (uint)y >= height) return Conv(backgroundColor);
      return GetPixelUnsafe64(x, y);
    }
#else
    public override ColorTypeB GetPixel32(int x, int y)
    {
      if ((uint)x >= width || (uint)y >= height) return Conv(backgroundColor);
      return GetPixelUnsafe32(x, y);
    }
#endif

    /// <summary>
    /// Fill the Scanline with a specific color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public override void FillScanlineUnsafe(int x, int y, int w, ColorTypeB color)
    {
      FillScanlineUnsafe(x, y, w, Conv(color));
    }

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public override void FillScanline(int x, int y, int w, ColorTypeB color)
    {
      FillScanline(x, y, w, Conv(color));
    }

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">fill-color</param>
    public override void FillScanline(int y, ColorTypeB color)
    {
      FillScanline(y, Conv(color));
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors. (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLineUnsafe(int x, int y, int w, ColorTypeB* srcPixels)
    {
      var tmp = new ColorType[w];
      for (int i = 0; i < tmp.Length; i++)
      {
        tmp[i] = Conv(srcPixels[i]);
      }
      fixed (ColorType* ptr = tmp)
      {
        WriteScanLineUnsafe(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int x, int y, int w, ColorTypeB* srcPixels)
    {
      if ((uint)y >= height) return;

      if (x < 0)
      {
        w += x;
        srcPixels -= x;
        x = 0;
      }

      if (x + w > width)
      {
        w = width - x;
      }

      if (w < 1) return;

      WriteScanLineUnsafe(x, y, w, srcPixels);
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public override void WriteScanLine(int x, int y, int w, ColorTypeB[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + w > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 1 || (uint)y >= height) return;
      fixed (ColorTypeB* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLine(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int y, ColorTypeB* srcPixels)
    {
      if ((uint)y >= height) return;
      WriteScanLineUnsafe(0, y, width, srcPixels);
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public override void WriteScanLine(int y, ColorTypeB[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + width > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if ((uint)y >= height) return;
      fixed (ColorTypeB* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLineUnsafe(0, y, width, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLineUnsafe(int x, int y, int w, ColorTypeB* destPixels)
    {
      var tmp = new ColorType[w];
      fixed (ColorType* ptr = tmp)
      {
        ReadScanLineUnsafe(x, y, w, ptr);
      }
      for (int i = 0; i < tmp.Length; i++)
      {
        destPixels[i] = Conv(tmp[i]);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int x, int y, int w, ColorTypeB* destPixels)
    {
      ColorTypeB bgColor = Conv(backgroundColor);

      if ((uint)y >= height)
      {
        for (int i = 0; i < w; i++) destPixels[i] = bgColor;
        return;
      }

      if (x < 0)
      {
        int end = Math.Min(-x, w);
        for (int i = 0; i < end; i++) destPixels[i] = bgColor;
        w += x;
        destPixels -= x;
        x = 0;
      }

      if (x + w > width)
      {
        for (int i = Math.Max(0, width - x); i < w; i++) destPixels[i] = bgColor;
        w = width - x;
      }

      if (w < 1) return;

      ReadScanLineUnsafe(x, y, w, destPixels);
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public override void ReadScanLine(int x, int y, int w, ColorTypeB[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + width > destPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 1) return;
      fixed (ColorTypeB* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int y, ColorTypeB* destPixels)
    {
      ReadScanLine(0, y, width, destPixels);
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public override void ReadScanLine(int y, ColorTypeB[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + width > destPixels.Length) throw new ArgumentOutOfRangeException();
      fixed (ColorTypeB* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(y, ptr);
      }
    }
    #endregion

    #endregion

    #region # // --- additional methods ---

    #region # // --- ColorType (primary color type) ---
    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color">Fillcolor</param>
    public override void Clear(ColorType color)
    {
      for (int y = 0; y < height; y++)
      {
        FillScanline(y, color);
      }
    }

    /// <summary>
    /// drawing normal line
    /// </summary>
    /// <param name="x1">start x-position</param>
    /// <param name="y1">start y-position</param>
    /// <param name="x2">end x-position</param>
    /// <param name="y2">end y-position</param>
    /// <param name="color">line-color</param>
    public override void DrawLine(int x1, int y1, int x2, int y2, ColorType color)
    {
      bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
      if (steep) { int t = x1; x1 = y1; y1 = t; t = x2; x2 = y2; y2 = t; }
      if (x1 > x2) { int t = x1; x1 = x2; x2 = t; t = y1; y1 = y2; y2 = t; }

      int dx = x2 - x1;
      int dy = Math.Abs(y2 - y1);
      int err = dx / 2;
      int ystep = y1 < y2 ? 1 : -1;

      if (steep)
      {
        for (; x1 <= x2; x1++)
        {
          if ((uint)y1 < width && (uint)x1 < height) SetPixelUnsafe(y1, x1, color);
          err -= dy;
          if (err < 0)
          {
            y1 += ystep;
            err += dx;
          }
        }
      }
      else
      {
        for (; x1 <= x2; x1++)
        {
          if ((uint)x1 < width && (uint)y1 < height) SetPixelUnsafe(x1, y1, color);
          err -= dy;
          if (err < 0)
          {
            y1 += ystep;
            err += dx;
          }
        }
      }
    }

    /// <summary>
    /// Draw a circle
    /// </summary>
    /// <param name="x">Center x-position</param>
    /// <param name="y">Center y-position</param>
    /// <param name="r">Radius</param>
    /// <param name="color">Color</param>
    public override void DrawCircle(int x, int y, int r, ColorType color)
    {
      int f = 1 - r;
      int ddF_x = 1;
      int ddF_y = -2 * r;
      int px = 0;
      int py = r;

      if ((uint)x < width)
      {
        if ((uint)(y + r) < height) SetPixelUnsafe(x, y + r, color);
        if ((uint)(y - r) < height) SetPixelUnsafe(x, y - r, color);
      }
      if ((uint)y < height)
      {
        if ((uint)(x + r) < width) SetPixelUnsafe(x + r, y, color);
        if ((uint)(x - r) < width) SetPixelUnsafe(x - r, y, color);
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

        if ((uint)(x + px) < width && (uint)(y + py) < height) SetPixelUnsafe(x + px, y + py, color);
        if ((uint)(x - px) < width && (uint)(y + py) < height) SetPixelUnsafe(x - px, y + py, color);
        if ((uint)(x + px) < width && (uint)(y - py) < height) SetPixelUnsafe(x + px, y - py, color);
        if ((uint)(x - px) < width && (uint)(y - py) < height) SetPixelUnsafe(x - px, y - py, color);
        if ((uint)(x + py) < width && (uint)(y + px) < height) SetPixelUnsafe(x + py, y + px, color);
        if ((uint)(x - py) < width && (uint)(y + px) < height) SetPixelUnsafe(x - py, y + px, color);
        if ((uint)(x + py) < width && (uint)(y - px) < height) SetPixelUnsafe(x + py, y - px, color);
        if ((uint)(x - py) < width && (uint)(y - px) < height) SetPixelUnsafe(x - py, y - px, color);
      }
    }
    #endregion

    #region # // --- ColorTypeB (secondary compatibility color type) ---
    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color">Fillcolor</param>
    public override void Clear(ColorTypeB color)
    {
      ColorType c = Conv(color);
      for (int y = 0; y < height; y++)
      {
        FillScanline(y, c);
      }
    }

    /// <summary>
    /// drawing normal line
    /// </summary>
    /// <param name="x1">start x-position</param>
    /// <param name="y1">start y-position</param>
    /// <param name="x2">end x-position</param>
    /// <param name="y2">end y-position</param>
    /// <param name="color">line-color</param>
    public override void DrawLine(int x1, int y1, int x2, int y2, ColorTypeB color)
    {
      DrawLine(x1, y1, x2, y2, Conv(color));
    }

    /// <summary>
    /// Draw a circle
    /// </summary>
    /// <param name="x">Center x-position</param>
    /// <param name="y">Center y-position</param>
    /// <param name="r">Radius</param>
    /// <param name="color">Color</param>
    public override void DrawCircle(int x, int y, int r, ColorTypeB color)
    {
      DrawCircle(x, y, r, Conv(color));
    }
    #endregion

    #region # // --- undefined ColorType ---
    /// <summary>
    /// Clear the bitmap with background color
    /// </summary>
    public override void Clear()
    {
      Clear(backgroundColor);
    }

    /// <summary>
    /// map the Pixelformat to their ColorType Version
    /// </summary>
    /// <param name="pixelFormat">selected PixelFormat</param>
    /// <returns>new Pixelformat</returns>
    static PixelFormat MapPixelFormat(PixelFormat pixelFormat)
    {
#if C32
      switch (pixelFormat)
      {
        case PixelFormat.Format1bppIndexed:
        case PixelFormat.Format4bppIndexed:
        case PixelFormat.Format8bppIndexed:
        case PixelFormat.Format16bppRgb555:
        case PixelFormat.Format16bppRgb565:
        case PixelFormat.Format24bppRgb:
        case PixelFormat.Format32bppRgb:
        case PixelFormat.Format48bppRgb: return PixelFormat.Format32bppRgb;

        default: return PixelFormat.Format32bppArgb;
      }
#else
      return PixelFormat.Format64bppArgb;
#endif
    }

    /// <summary>
    /// Copies the pixel data from a bitmap of the same size
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    public override void CopyFromBitmap(IFastBitmap srcBitmap)
    {
      if (srcBitmap == null) throw new NullReferenceException("srcBitmap");
      if (srcBitmap.width != width || srcBitmap.height != height) throw new ArgumentException("Size of the image does not match");

      ColorType[] line = new ColorType[width];
      fixed (ColorType* linePtr = line)
      {
        for (int y = 0; y < height; y++)
        {
          srcBitmap.ReadScanLineUnsafe(0, y, width, linePtr);
          WriteScanLineUnsafe(0, y, width, linePtr);
        }
      }
    }

    /// <summary>
    /// Copies the pixel data to a GDI bitmap of the same size
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    public override void CopyToBitmap(IFastBitmap destBitmap)
    {
      if (destBitmap == null) throw new NullReferenceException("destBitmap");
      if (destBitmap.width != width || destBitmap.height != height) throw new ArgumentException("Size of the image does not match");

      ColorType[] line = new ColorType[width];
      fixed (ColorType* linePtr = line)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, linePtr);
          destBitmap.WriteScanLineUnsafe(0, y, width, linePtr);
        }
      }
    }

    /// <summary>
    /// Copies the pixel data from a GDI bitmap of the same size
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    public override void CopyFromGDIBitmap(Bitmap srcBitmap)
    {
      if (srcBitmap == null) throw new NullReferenceException("srcBitmap");
      if (srcBitmap.Width != width || srcBitmap.Height != height) throw new ArgumentException("Size of the image does not match");

      // Lock Bitmap-data for fast copy
      var bits = srcBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, MapPixelFormat(srcBitmap.PixelFormat));

      // Copy raw pixels
      if (bits.PixelFormat != PixelFormat.Format64bppArgb)
      {
        var ptr = (byte*)bits.Scan0;
        for (int y = 0; y < height; y++)
        {
          WriteScanLineUnsafe(0, y, width, (ColorType*)ptr);
          ptr += bits.Stride;
        }
      }
      else // "special" format: 13 Bits per Channel
      {
        var map = Color13Mapping.GetMap13to16();

        var ptr = (byte*)bits.Scan0;
        ushort[] tmp = new ushort[bits.Stride / 2];
        fixed (ushort* tmpPtr = tmp)
        {
          for (int y = 0; y < height; y++)
          {
            for (int i = 0; i < tmp.Length; i++)
            {
              tmp[i] = map[Math.Min(8192, (uint)((ushort*)ptr)[i])];
            }
            WriteScanLineUnsafe(0, y, width, (ColorType*)tmpPtr);
            ptr += bits.Stride;
          }
        }
      }

      // Release the lock
      srcBitmap.UnlockBits(bits);
    }

    /// <summary>
    /// Copies the pixel data to a GDI bitmap of the same size
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    public override void CopyToGDIBitmap(Bitmap destBitmap)
    {
      if (destBitmap == null) throw new NullReferenceException("destBitmap");
      if (destBitmap.Width != width || destBitmap.Height != height) throw new ArgumentException("Size of the image does not match");

      // Lock Bitmap-data for fast write
      var bits = destBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, MapPixelFormat(destBitmap.PixelFormat));

      if (bits.PixelFormat != PixelFormat.Format64bppArgb)
      {
        // Copy raw pixels
        var ptr = (byte*)bits.Scan0;
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, (ColorType*)ptr);
          ptr += bits.Stride;
        }
      }
      else // "special" format: 13 Bits per Channel
      {
        var map = Color13Mapping.GetMap16to13();

        var ptr = (byte*)bits.Scan0;
        ushort[] tmp = new ushort[bits.Stride / 2];
        fixed (ushort* tmpPtr = tmp)
        {
          for (int y = 0; y < height; y++)
          {
            ReadScanLineUnsafe(0, y, width, (ColorType*)tmpPtr);
            for (int i = 0; i < tmp.Length; i++)
            {
              if ((i & 3) < 3)
              {
                ((ushort*)ptr)[i] = map[tmp[i]]; // gamma corrected color values
              }
              else
              {
                ((ushort*)ptr)[i] = (ushort)(tmp[i] * 8192 / 65535); // linear alpha channel
              }
            }
            ptr += bits.Stride;
          }
        }
      }

      // Release the lock
      destBitmap.UnlockBits(bits);
    }

    /// <summary>
    /// Copies the pixel data from a GDI bitmap of the same size with specified <see cref="DrawViewPort"/>
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    /// <param name="viewPort">Draw-Viewport</param>
    /// <returns>Rectangle of copied pixels</returns>
    public override Rectangle CopyFromGDIBitmap(Bitmap srcBitmap, DrawViewPort viewPort)
    {
      if (srcBitmap == null) throw new NullReferenceException("srcBitmap");
      if (srcBitmap.Width != width || srcBitmap.Height != height) throw new ArgumentException("Size of the image does not match");

      // Normalize ViewPort values
      int vx = Math.Max(0, viewPort.startX);
      int vy = Math.Max(0, viewPort.startY);
      int vw = viewPort.endX - vx + 1;
      int vh = viewPort.endY - vy + 1;
      if (vx + vw > width) vw = width - vx;
      if (vy + vh > height) vh = height - vy;
      if (vw < 1 || vh < 1) return new Rectangle(0, 0, 0, 0);
      var rect = new Rectangle(vx, vy, vw, vh);

      // Lock Bitmap-data for fast write
      var bits = srcBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

      // Copy raw pixels
      for (int line = 0; line < vh; line++)
      {
        WriteScanLineUnsafe(vx, vy + line, vw, (uint*)(bits.Scan0.ToInt64() + line * bits.Stride));
      }

      // Release the lock
      srcBitmap.UnlockBits(bits);

      return rect;
    }

    /// <summary>
    /// Copies the pixel data to a GDI bitmap of the same size with specified <see cref="DrawViewPort"/>
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    /// <param name="viewPort">Draw-Viewport</param>
    /// <returns>Rectangle of copied pixels</returns>
    public override Rectangle CopyToGDIBitmap(Bitmap destBitmap, DrawViewPort viewPort)
    {
      if (destBitmap == null) throw new NullReferenceException("destBitmap");
      if (destBitmap.Width != width || destBitmap.Height != height) throw new ArgumentException("Size of the image does not match");

      // Normalize ViewPort values
      int vx = Math.Max(0, viewPort.startX);
      int vy = Math.Max(0, viewPort.startY);
      int vw = viewPort.endX - vx + 1;
      int vh = viewPort.endY - vy + 1;
      if (vx + vw > width) vw = width - vx;
      if (vy + vh > height) vh = height - vy;
      if (vw < 1 || vh < 1) return new Rectangle(0, 0, 0, 0);
      var rect = new Rectangle(vx, vy, vw, vh);

      // Lock Bitmap-data for fast write
      var bits = destBitmap.LockBits(rect, ImageLockMode.WriteOnly, MapPixelFormat(destBitmap.PixelFormat));

      // Copy raw pixels
      for (int line = 0; line < vh; line++)
      {
        ReadScanLineUnsafe(vx, vy + line, vw, (ColorType*)(bits.Scan0.ToInt64() + line * bits.Stride));
      }

      // Release the lock
      destBitmap.UnlockBits(bits);

      return rect;
    }
    #endregion

    #endregion
  }
}
