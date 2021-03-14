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
  public partial class FastBitmap
  {
    /// <summary>
    /// Copies the pixel data from a GDI bitmap of the same size
    /// </summary>
    /// <param name="srcBitmap">Image from where to read the pixel data</param>
    public void CopyFromGDIBitmap(Bitmap srcBitmap)
    {
      if (srcBitmap == null) throw new NullReferenceException("srcBitmap");
      if (srcBitmap.Width != width || srcBitmap.Height != height) throw new ArgumentException("Size of the image does not match");

      // Lock Bitmap-data for fast copy
      var bits = srcBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

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
      var bits = destBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

      // Copy raw pixels
      Marshal.Copy((object)pixels as int[], 0, bits.Scan0, width * height);

      // Release the lock
      destBitmap.UnlockBits(bits);
    }

    /// <summary>
    /// returns the entire image as a GDI bitmap
    /// </summary>
    /// <returns>GDI bitmap</returns>
    public Bitmap ToGDIBitmap()
    {
      var resultBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

      CopyToGDIBitmap(resultBitmap);

      return resultBitmap;
    }
  }
}
