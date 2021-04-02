using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FastBitmapLib.Extras;

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
    /// map the Pixelformat to their 32-Bit Version
    /// </summary>
    /// <param name="bitmap">selected PixelFormat from Bitmap</param>
    /// <returns>new Pixelformat</returns>
    static PixelFormat MapPixelFormat32(Bitmap bitmap)
    {
      return MapPixelFormat32(bitmap.PixelFormat);
    }

    /// <summary>
    /// Copies the pixel data from a GDI bitmap of the same size
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    public void CopyFromGDIBitmap(Bitmap srcBitmap)
    {
      if (srcBitmap == null) throw new NullReferenceException("srcBitmap");
      if (srcBitmap.Width != width || srcBitmap.Height != height) throw new ArgumentException("Size of the image does not match");

      // Lock Bitmap-data for fast copy
      var bits = srcBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, MapPixelFormat32(srcBitmap));

      // Copy raw pixels (cast uint[] to int[] ist possible, see: https://www.c-sharpcorner.com/uploadfile/b942f9/how-to-convert-unsigned-integer-arrays-to-signed-arrays-and-vice-versa/)
      Marshal.Copy(bits.Scan0, (object)pixels as int[], 0, width * height);

      // Release the lock
      srcBitmap.UnlockBits(bits);
    }

    /// <summary>
    /// Copies the pixel data to a GDI bitmap of the same size
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    public void CopyToGDIBitmap(Bitmap destBitmap)
    {
      if (destBitmap == null) throw new NullReferenceException("destBitmap");
      if (destBitmap.Width != width || destBitmap.Height != height) throw new ArgumentException("Size of the image does not match");

      // Lock Bitmap-data for fast write
      var bits = destBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, MapPixelFormat32(destBitmap));

      // Copy raw pixels
      Marshal.Copy((object)pixels as int[], 0, bits.Scan0, width * height);

      // Release the lock
      destBitmap.UnlockBits(bits);
    }

    /// <summary>
    /// Copies the pixel data to a GDI bitmap of the same size
    /// </summary>
    /// <param name="destBitmap">Image where the pixel data is written to</param>
    /// <param name="viewPort">Draw-Viewport</param>
    public unsafe Rectangle CopyToGDIBitmap(Bitmap destBitmap, DrawViewPort viewPort)
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
      var bits = destBitmap.LockBits(rect, ImageLockMode.WriteOnly, MapPixelFormat32(destBitmap));

      fixed (uint* pixelsPtr = pixels)
      {
        var pPtr = pixelsPtr + vx + vy * width;
        for (int line = 0; line < vh; line++, pPtr += width)
        {
          // Copy line of raw pixels
          CopyScanLine((uint*)(bits.Scan0.ToInt64() + line * bits.Stride), pPtr, vw);
        }
      }

      // Release the lock
      destBitmap.UnlockBits(bits);

      return rect;
    }

    /// <summary>
    /// returns the entire image as a GDI bitmap
    /// </summary>
    /// <param name="pixelFormat">Optional: select pixelformat (default: Format32bppRgb)</param>
    /// <returns>GDI bitmap</returns>
    public Bitmap ToGDIBitmap(PixelFormat pixelFormat = PixelFormat.Format32bppRgb)
    {
      var resultBitmap = new Bitmap(width, height, pixelFormat);

      CopyToGDIBitmap(resultBitmap);

      return resultBitmap;
    }

    /// <summary>
    /// returns the entire image as a GDI bitmap with alpha channel
    /// </summary>
    /// <returns>GDI bitmap</returns>
    public Bitmap ToGDIBitmapAlpha()
    {
      return ToGDIBitmap(PixelFormat.Format32bppArgb);
    }
  }
}
