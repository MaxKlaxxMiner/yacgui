
using System;
using System.Drawing;
using FastBitmapLib.Extras;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable VirtualMemberNeverOverriden.Global
// ReSharper disable UnusedParameter.Global

namespace FastBitmapLib
{
  /// <summary>
  /// abstract Main class for FastBitmap
  /// </summary>
  public unsafe abstract class IFastBitmap : IDisposable
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

    #region # // --- basic methods ---
    /// <summary>
    /// Set the pixel <see cref="Color32"/> at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color32">Pixel <see cref="Color32"/></param>
    public abstract void SetPixel(int x, int y, uint color32);

    /// <summary>
    /// Set the pixel <see cref="Color64"/> at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color64">Pixel <see cref="Color64"/></param>
    public abstract void SetPixel(int x, int y, ulong color64);

    /// <summary>
    /// Get the pixel <see cref="Color32"/> from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel <see cref="Color32"/></returns>
    public abstract uint GetPixel32(int x, int y);

    /// <summary>
    /// Get the pixel <see cref="Color64"/> from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel <see cref="Color64"/></returns>
    public abstract ulong GetPixel64(int x, int y);

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color32"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color32">fill-<see cref="Color32"/></param>
    public abstract void FillScanline(int y, uint color32);

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color32"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color32">fill-<see cref="Color32"/></param>
    public abstract void FillScanline(int x, int y, int w, uint color32);

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color64"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color64">fill-<see cref="Color64"/></param>
    public abstract void FillScanline(int y, ulong color64);

    /// <summary>
    /// Fill the Scanline with a specific <see cref="Color64"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color64">fill-<see cref="Color64"/></param>
    public abstract void FillScanline(int x, int y, int w, ulong color64);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color32"/>)</param>
    public abstract void WriteScanLine(int y, uint[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color32"/>)</param>
    public abstract void WriteScanLine(int x, int y, int w, uint[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int y, uint* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color32"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int x, int y, int w, uint* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color64"/>)</param>
    public abstract void WriteScanLine(int y, ulong[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Source array of pixels</param>
    /// <param name="srcPixelOffset">Offset in srcPixels (<see cref="Color64"/>)</param>
    public abstract void WriteScanLine(int x, int y, int w, ulong[] srcPixels, int srcPixelOffset = 0);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int y, ulong* srcPixels);

    /// <summary>
    /// Writes a Scanline with a array of specific <see cref="Color64"/>s.
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public abstract void WriteScanLine(int x, int y, int w, ulong* srcPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color32"/>)</param>
    public abstract void ReadScanLine(int y, uint[] destPixels, int destPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color32"/>)</param>
    public abstract void ReadScanLine(int x, int y, int w, uint[] destPixels, int destPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int y, uint* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color32"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int x, int y, int w, uint* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color64"/>)</param>
    public abstract void ReadScanLine(int y, ulong[] destPixels, int destPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Destination array to write pixels</param>
    /// <param name="destPixelOffset">Offset in destPixels (<see cref="Color64"/>)</param>
    public abstract void ReadScanLine(int x, int y, int w, ulong[] destPixels, int destPixelOffset = 0);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int y, ulong* destPixels);

    /// <summary>
    /// Read a Scanline array of pixels type: <see cref="Color64"/>
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public abstract void ReadScanLine(int x, int y, int w, ulong* destPixels);
    #endregion

    #region # // --- addition methods ---
    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color32">Fill<see cref="Color32"/></param>
    public abstract void Clear(uint color32);

    /// <summary>
    /// Clear the bitmap
    /// </summary>
    /// <param name="color64">Fill<see cref="Color64"/></param>
    public abstract void Clear(ulong color64);

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
