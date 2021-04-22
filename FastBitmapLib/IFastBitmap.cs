#region # using *.*
using System;
using System.Drawing;
using FastBitmapLib.Extras;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable VirtualMemberNeverOverriden.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
#endregion

namespace FastBitmapLib
{
  /// <summary>
  /// abstract Main class for FastBitmap
  /// </summary>
  public abstract unsafe class IFastBitmap : IDisposable
  {
    /// <summary>
    /// Width of the image in pixels
    /// </summary>
    public readonly int width;
    /// <summary>
    /// Height of the image in pixels
    /// </summary>
    public readonly int height;

    /// <summary>
    /// Max dimensions of the Bitmap for width and height
    /// </summary>
    public const int MaxSize = 1000000;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    protected IFastBitmap(int width, int height)
    {
      if (width < 1 || width > MaxSize) throw new ArgumentOutOfRangeException("width");
      if (height < 1 || height > MaxSize) throw new ArgumentOutOfRangeException("height");

      this.width = width;
      this.height = height;
    }

    #region # // --- Color convert ---
    /// <summary>
    /// Convert color value from 64-Bit to 32-Bit
    /// </summary>
    /// <param name="color">Color to Convert</param>
    /// <returns>Converted color</returns>
    protected static uint Conv(ulong color)
    {
      return Color32.From(color);
    }

    /// <summary>
    /// Convert color value from 32-Bit to 64-Bit
    /// </summary>
    /// <param name="color">Color to Convert</param>
    /// <returns>Converted color</returns>
    protected static ulong Conv(uint color)
    {
      return Color64.From(color);
    }
    #endregion

    #region # // --- basic methods ---

    #region # // --- Color32 ---
    /// <summary>
    /// Set the pixel color at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public abstract void SetPixelUnsafe(int x, int y, uint color);

    /// <summary>
    /// Set the pixel color at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public abstract void SetPixel(int x, int y, uint color);

    /// <summary>
    /// Get the pixel color from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public abstract uint GetPixelUnsafe32(int x, int y);

    /// <summary>
    /// Get the pixel color from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public abstract uint GetPixel32(int x, int y);

    /// <summary>
    /// Fill the Scanline with a specific color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public abstract void FillScanlineUnsafe(int x, int y, int w, uint color);

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public abstract void FillScanline(int x, int y, int w, uint color);

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">fill-color</param>
    public abstract void FillScanline(int y, uint color);

    /// <summary>
    /// Writes a Scanline with a array of specific colors. (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLineUnsafe(int x, int y, int w, uint* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int x, int y, int w, uint* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public abstract void WriteScanLine(int x, int y, int w, uint[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int y, uint* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public abstract void WriteScanLine(int y, uint[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: color32 (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLineUnsafe(int x, int y, int w, uint* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: color32
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int x, int y, int w, uint* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: color32
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public abstract void ReadScanLine(int x, int y, int w, uint[] destPixels, int destPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: color32
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int y, uint* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: color32
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public abstract void ReadScanLine(int y, uint[] destPixels, int destPixelOffset = 0);
    #endregion

    #region # // --- Color64 ---
    /// <summary>
    /// Set the pixel color at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public abstract void SetPixelUnsafe(int x, int y, ulong color);

    /// <summary>
    /// Set the pixel color at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public abstract void SetPixel(int x, int y, ulong color);

    /// <summary>
    /// Get the pixel color from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public abstract ulong GetPixelUnsafe64(int x, int y);

    /// <summary>
    /// Get the pixel color from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public abstract ulong GetPixel64(int x, int y);

    /// <summary>
    /// Fill the Scanline with a specific color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public abstract void FillScanlineUnsafe(int x, int y, int w, ulong color);

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public abstract void FillScanline(int x, int y, int w, ulong color);

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">fill-color</param>
    public abstract void FillScanline(int y, ulong color);

    /// <summary>
    /// Writes a Scanline with a array of specific colors. (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLineUnsafe(int x, int y, int w, ulong* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int x, int y, int w, ulong* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public abstract void WriteScanLine(int x, int y, int w, ulong[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int y, ulong* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific colors.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (color)</param>
    public abstract void WriteScanLine(int y, ulong[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: color64 (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLineUnsafe(int x, int y, int w, ulong* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: color64
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int x, int y, int w, ulong* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: color64
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public abstract void ReadScanLine(int x, int y, int w, ulong[] destPixels, int destPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: color64
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int y, ulong* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: color64
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (color)</param>
    public abstract void ReadScanLine(int y, ulong[] destPixels, int destPixelOffset = 0);
    #endregion

    #region # // --- ColorGDI ---
    /// <summary>
    /// Set the pixel color at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public virtual void SetPixel(int x, int y, Color color)
    {
      SetPixel(x, y, (uint)color.ToArgb());
    }

    /// <summary>
    /// Get the pixel color from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public virtual Color GetPixelColor(int x, int y)
    {
      return Color.FromArgb((int)GetPixel32(x, y));
    }

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public virtual void FillScanline(int x, int y, int w, Color color)
    {
      FillScanline(x, y, w, (uint)color.ToArgb());
    }

    /// <summary>
    /// Fill the Scanline with a specific color
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">fill-color</param>
    public virtual void FillScanline(int y, Color color)
    {
      FillScanline(y, (uint)color.ToArgb());
    }
    #endregion

    #endregion

    #region # // --- additional methods ---

    #region # // --- Color32 ---
    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color">Fillcolor</param>
    public abstract void Clear(uint color);

    /// <summary>
    /// Update-Function for all pixels
    /// </summary>
    /// <param name="func">Update function: uint Func(uint sourceColor)</param>
    public virtual void UpdatePixels32(Func<uint, uint> func)
    {
      var tmp = new uint[width];
      fixed (uint* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          int w = width;
          for (int x = 0; x < w; x++)
          {
            ptr[x] = func(ptr[x]);
          }
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }

    /// <summary>
    /// Update-Function for all pixels
    /// </summary>
    /// <param name="func">Update function: uint Func(int index, uint sourceColor)</param>
    public virtual void UpdatePixels32(Func<int, uint, uint> func)
    {
      var tmp = new uint[width];
      fixed (uint* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          int yp = y * width;
          int w = width;
          for (int x = 0; x < w; x++)
          {
            ptr[x] = func(yp + x, ptr[x]);
          }
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }

    /// <summary>
    /// Update-Function for all pixels
    /// </summary>
    /// <param name="func">Update function: uint Func(int x, int y, uint sourceColor)</param>
    public virtual void UpdatePixels32(Func<int, int, uint, uint> func)
    {
      var tmp = new uint[width];
      fixed (uint* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          int w = width;
          for (int x = 0; x < w; x++)
          {
            ptr[x] = func(x, y, ptr[x]);
          }
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }

    /// <summary>
    /// Update-Function for all pixels in scanlines
    /// </summary>
    /// <param name="func">Update function: void Func(uint[] pixels, int y)</param>
    public virtual void UpdateScanlines32(Action<uint[], int> func)
    {
      var tmp = new uint[width];
      fixed (uint* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          func(tmp, y);
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }
    #endregion

    #region # // --- Color64 ---
    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color">Fillcolor</param>
    public abstract void Clear(ulong color);

    /// <summary>
    /// Update-Function for all pixels
    /// </summary>
    /// <param name="func">Update function: ulong Func(ulong sourceColor)</param>
    public virtual void UpdatePixels64(Func<ulong, ulong> func)
    {
      var tmp = new ulong[width];
      fixed (ulong* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          int w = width;
          for (int x = 0; x < w; x++)
          {
            ptr[x] = func(ptr[x]);
          }
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }

    /// <summary>
    /// Update-Function for all pixels
    /// </summary>
    /// <param name="func">Update function: ulong Func(int index, ulong sourceColor)</param>
    public virtual void UpdatePixels64(Func<int, ulong, ulong> func)
    {
      var tmp = new ulong[width];
      fixed (ulong* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          int yp = y * width;
          int w = width;
          for (int x = 0; x < w; x++)
          {
            ptr[x] = func(yp + x, ptr[x]);
          }
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }

    /// <summary>
    /// Update-Function for all pixels
    /// </summary>
    /// <param name="func">Update function: uint Func(int x, int y, ulong sourceColor)</param>
    public virtual void UpdatePixels64(Func<int, int, ulong, ulong> func)
    {
      var tmp = new ulong[width];
      fixed (ulong* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          int w = width;
          for (int x = 0; x < w; x++)
          {
            ptr[x] = func(x, y, ptr[x]);
          }
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }

    /// <summary>
    /// Update-Function for all pixels in scanlines
    /// </summary>
    /// <param name="func">Update function: void Func(ulong[] pixels, int y)</param>
    public virtual void UpdateScanlines64(Action<ulong[], int> func)
    {
      var tmp = new ulong[width];
      fixed (ulong* ptr = tmp)
      {
        for (int y = 0; y < height; y++)
        {
          ReadScanLineUnsafe(0, y, width, ptr);
          func(tmp, y);
          WriteScanLineUnsafe(0, y, width, ptr);
        }
      }
    }
    #endregion

    #region # // --- ColorGDI ---
    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color">Fillcolor</param>
    public virtual void Clear(Color color)
    {
      Clear((uint)color.ToArgb());
    }
    #endregion

    #region # // --- ColorXX ---
    /// <summary>
    /// Clear the bitmap with background color
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Copies the pixel data from a bitmap of the same size
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    public abstract void CopyFromBitmap(IFastBitmap srcBitmap);

    /// <summary>
    /// Copies the pixel data to a bitmap of the same size
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    public abstract void CopyToBitmap(IFastBitmap destBitmap);

    /// <summary>
    /// Copies the pixel data from a GDI bitmap of the same size
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    public abstract void CopyFromGDIBitmap(Bitmap srcBitmap);

    /// <summary>
    /// Copies the pixel data to a GDI bitmap of the same size
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    public abstract void CopyToGDIBitmap(Bitmap destBitmap);

    /// <summary>
    /// Copies the pixel data from a GDI bitmap of the same size with specified <see cref="DrawViewPort"/>
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    /// <param name="viewPort">Draw-Viewport</param>
    /// <returns>Rectangle of copied pixels</returns>
    public abstract Rectangle CopyFromGDIBitmap(Bitmap srcBitmap, DrawViewPort viewPort);

    /// <summary>
    /// Copies the pixel data to a GDI bitmap of the same size with specified <see cref="DrawViewPort"/>
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    /// <param name="viewPort">Draw-Viewport</param>
    /// <returns>Rectangle of copied pixels</returns>
    public abstract Rectangle CopyToGDIBitmap(Bitmap destBitmap, DrawViewPort viewPort);
    #endregion

    #endregion

    #region # // --- IDisposable ---
    /// <summary>
    /// Release unmanaged ressources
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// Destructor (called from GarbageCollector)
    /// </summary>
    ~IFastBitmap()
    {
      Dispose();
    }
    #endregion
  }
}
