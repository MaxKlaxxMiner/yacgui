#region # using *.*

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
#endregion

namespace YacGui
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public class FastBitmap
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
    /// Stored pixel data of the bitmap (ARGB-Pixel value: 0xaa112233, aa = Alpha, 11 = Red, 22 = Green, 33 = Blue)
    /// </summary>
    public readonly uint[] pixels;

    /// <summary>
    /// Constructor: Create a bitmap from GDI-Bitmap
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    public FastBitmap(Bitmap bitmap)
    {
      width = bitmap.Width;
      height = bitmap.Height;
      pixels = new uint[width * height];

      // Lock Bitmap-data for fast copy
      var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

      // Copy raw pixels (cast uint[] to int[] ist possible, see: https://www.c-sharpcorner.com/uploadfile/b942f9/how-to-convert-unsigned-integer-arrays-to-signed-arrays-and-vice-versa/)
      Marshal.Copy(bits.Scan0, (object)pixels as int[], 0, pixels.Length);

      // Release the lock
      bitmap.UnlockBits(bits);
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
      var bits = destBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

      // Copy raw pixels
      Marshal.Copy((object)pixels as int[], 0, bits.Scan0, pixels.Length);

      // Release the lock
      destBitmap.UnlockBits(bits);
    }

    /// <summary>
    /// returns the entire image as a GDI bitmap
    /// </summary>
    /// <returns>GDI bitmap</returns>
    public Bitmap GetGDIBitmap()
    {
      var resultBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

      CopyToGDIBitmap(resultBitmap);

      return resultBitmap;
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
  }
}
