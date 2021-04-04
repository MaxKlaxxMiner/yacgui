using System;
using System.Drawing;
using System.Drawing.Imaging;
using FastBitmapLib.Extras;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace FastBitmapLib
{
  /// <summary>
  /// abstract Main class for FastBitmap for 32-Bit Pixels: <see cref="Color32"/>
  /// </summary>
  public abstract unsafe class IFastBitmap32 : IFastBitmap
  {
    /// <summary>
    /// <see cref="Color32"/> of the background
    /// </summary>
    public uint backgroundColor;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    protected IFastBitmap32(int width, int height, uint backgroundColor = 0x00000000)
      : base(width, height)
    {
      this.backgroundColor = backgroundColor;
    }

    /// <summary>
    /// Set the pixel <see cref="Color64"/> at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color64">Pixel <see cref="Color64"/></param>
    public override void SetPixel(int x, int y, ulong color64)
    {
      SetPixel(x, y, Color32.From(color64));
    }

    /// <summary>
    /// Get the pixel <see cref="Color64"/> from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel <see cref="Color64"/></returns>
    public override ulong GetPixel64(int x, int y)
    {
      return Color64.From(GetPixel32(x, y));
    }

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color32"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color32">fill-<see cref="Color32"/></param>
    public override void FillScanline(int y, uint color32)
    {
      FillScanline(0, y, width, color32);
    }

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color32"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color32">fill-<see cref="Color32"/></param>
    public override void FillScanline(int x, int y, int w, uint color32)
    {
      for (int i = 0; i < w; i++)
      {
        SetPixel(x + i, y, color32);
      }
    }

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color64"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color64">fill-<see cref="Color64"/></param>
    public override void FillScanline(int y, ulong color64)
    {
      FillScanline(0, y, width, color64);
    }

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color64"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color64">fill-<see cref="Color64"/></param>
    public override void FillScanline(int x, int y, int w, ulong color64)
    {
      FillScanline(x, y, w, Color32.From(color64));
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color32"/>)</param>
    public override void WriteScanLine(int y, uint[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + width > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if ((uint)y < height) return;
      fixed (uint* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLine(y, ptr);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color32"/>)</param>
    public override void WriteScanLine(int x, int y, int w, uint[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + w > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 1 | (uint)y < height) return;
      fixed (uint* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLine(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int y, uint* srcPixels)
    {
      WriteScanLine(0, y, width, srcPixels);
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int x, int y, int w, uint* srcPixels)
    {
      for (int i = 0; i < w; i++)
      {
        SetPixel(x + i, y, srcPixels[i]);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color64"/>)</param>
    public override void WriteScanLine(int y, ulong[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + width > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if ((uint)y < height) return;
      fixed (ulong* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLine(0, y, width, ptr);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color64"/>)</param>
    public override void WriteScanLine(int x, int y, int w, ulong[] srcPixels, int srcPixelOffset = 0)
    {
      if (srcPixels == null) throw new ArgumentNullException("srcPixels");
      if (srcPixelOffset < 0 || srcPixelOffset + w > srcPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 1 || (uint)y < height) return;
      fixed (ulong* ptr = &srcPixels[srcPixelOffset])
      {
        WriteScanLine(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int y, ulong* srcPixels)
    {
      WriteScanLine(0, y, width, srcPixels);
    }

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLine(int x, int y, int w, ulong* srcPixels)
    {
      if (w < 1 || (uint)y < height) return;
      var tmp = new uint[w];
      for (int i = 0; i < tmp.Length; i++)
      {
        tmp[i] = Color32.From(srcPixels[i]);
      }
      WriteScanLine(x, y, w, tmp);
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color32"/>)</param>
    public override void ReadScanLine(int y, uint[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + width > destPixels.Length) throw new ArgumentOutOfRangeException();
      fixed (uint* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(y, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color32"/>)</param>
    public override void ReadScanLine(int x, int y, int w, uint[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + w > destPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 0) return;
      fixed (uint* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(x, y, w, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int y, uint* destPixels)
    {
      ReadScanLine(0, y, width, destPixels);
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int x, int y, int w, uint* destPixels)
    {
      for (int i = 0; i < w; i++)
      {
        destPixels[x] = GetPixel32(x + i, y);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color64"/>)</param>
    public override void ReadScanLine(int y, ulong[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + width > destPixels.Length) throw new ArgumentOutOfRangeException();
      fixed (ulong* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(y, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color64"/>)</param>
    public override void ReadScanLine(int x, int y, int w, ulong[] destPixels, int destPixelOffset = 0)
    {
      if (destPixels == null) throw new ArgumentNullException("destPixels");
      if (destPixelOffset < 0 || destPixelOffset + width > destPixels.Length) throw new ArgumentOutOfRangeException();
      if (w < 1) return;
      fixed (ulong* ptr = &destPixels[destPixelOffset])
      {
        ReadScanLine(0, y, w, ptr);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int y, ulong* destPixels)
    {
      ReadScanLine(0, y, width, destPixels);
    }

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLine(int x, int y, int w, ulong* destPixels)
    {
      for (int i = 0; i < w; i++)
      {
        destPixels[x] = GetPixel64(x + i, y);
      }
    }

    /// <summary>
    /// Clear the bitmap with background-color
    /// </summary>
    public void Clear()
    {
      Clear(backgroundColor);
    }

    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color32">Fill<see cref="Color32"/></param>
    public override void Clear(uint color32)
    {
      for (int y = 0; y < height; y++)
      {
        FillScanline(y, color32);
      }
    }

    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color64">Fill<see cref="Color64"/></param>
    public override void Clear(ulong color64)
    {
      Clear(Color32.From(color64));
    }

    /// <summary>
    /// map the Pixelformat to their 32-Bit Version
    /// </summary>
    /// <param name="pixelFormat">selected PixelFormat</param>
    /// <returns>new Pixelformat</returns>
    static PixelFormat MapPixelFormat32(PixelFormat pixelFormat)
    {
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
      var bits = srcBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, MapPixelFormat32(srcBitmap.PixelFormat));

      // Copy raw pixels
      var ptr = (byte*)bits.Scan0;
      for (int y = 0; y < height; y++)
      {
        WriteScanLine(y, (uint*)ptr);
        ptr += bits.Stride;
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
      var bits = destBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, MapPixelFormat32(destBitmap.PixelFormat));

      // Copy raw pixels
      var ptr = (byte*)bits.Scan0;
      for (int y = 0; y < height; y++)
      {
        ReadScanLine(y, (uint*)ptr);
        ptr += bits.Stride;
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

      int vx = Math.Max(0, viewPort.startX);
      int vy = Math.Max(0, viewPort.startY);
      int vw = viewPort.endX - vx + 1;
      int vh = viewPort.endY - vy + 1;
      if (vx + vw > width) vw = width - vx;
      if (vy + vh > height) vh = height - vy;
      if (vw < 1 || vh < 1) return new Rectangle(0, 0, 0, 0);
      var rect = new Rectangle(vx, vy, vw, vh);

      // Lock Bitmap-data for fast write
      var bits = srcBitmap.LockBits(rect, ImageLockMode.WriteOnly, MapPixelFormat32(srcBitmap.PixelFormat));

      // Copy raw pixels
      for (int line = 0; line < vh; line++)
      {
        WriteScanLine(vx, vy + line, vw, (uint*)(bits.Scan0.ToInt64() + line * bits.Stride));
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

      int vx = Math.Max(0, viewPort.startX);
      int vy = Math.Max(0, viewPort.startY);
      int vw = viewPort.endX - vx + 1;
      int vh = viewPort.endY - vy + 1;
      if (vx + vw > width) vw = width - vx;
      if (vy + vh > height) vh = height - vy;
      if (vw < 1 || vh < 1) return new Rectangle(0, 0, 0, 0);
      var rect = new Rectangle(vx, vy, vw, vh);

      // Lock Bitmap-data for fast write
      var bits = destBitmap.LockBits(rect, ImageLockMode.WriteOnly, MapPixelFormat32(destBitmap.PixelFormat));

      // Copy raw pixels
      for (int line = 0; line < vh; line++)
      {
        ReadScanLine(vx, vy + line, vw, (uint*)(bits.Scan0.ToInt64() + line * bits.Stride));
      }

      // Release the lock
      destBitmap.UnlockBits(bits);

      return rect;
    }
  }
}
