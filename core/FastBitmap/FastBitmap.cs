#region # using *.*
using System.Drawing;
using System.Drawing.Imaging;
// ReSharper disable MemberCanBePrivate.Global
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
    /// Stored data of the bitmap
    /// </summary>
    public readonly uint[] data;

    /// <summary>
    /// Constructor: Create a bitmap from GDI-Bitmap
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    public FastBitmap(Bitmap bitmap)
    {
      width = bitmap.Width;
      height = bitmap.Height;

      var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);



      bitmap.UnlockBits(bits);
    }
  }
}
