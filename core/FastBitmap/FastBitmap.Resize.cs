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
    public FastBitmap GetResizedSimple(int newWidth, int newHeight)
    {
      if (newWidth <= 0 && newHeight <= 0) throw new ArgumentException();

      if (newWidth <= 0) newWidth = newHeight * width / height;
      if (newHeight <= 0) newHeight = newWidth * height / width;

      var result = new FastBitmap(newWidth, newHeight);

      int cxStep = 256 * width / newWidth;
      for (int y = 0; y < newHeight; y++)
      {
        int cy = y * height / newHeight;
        for (int x = 0, cx = 0; x < newWidth; x++, cx += cxStep)
        {
          result.SetPixel(x, y, GetPixel(cx >> 8, cy));
        }
      }

      return result;
    }
  }
}
