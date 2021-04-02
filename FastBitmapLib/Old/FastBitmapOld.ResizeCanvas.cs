using System;
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public sealed partial class FastBitmapOld
  {
    /// <summary>
    /// Create a resized bitmap without stretching (add/remove pixels)
    /// </summary>
    /// <param name="addLeft">Add/Remove pixels on the left side</param>
    /// <param name="addRight">Add/Remove pixels on the right side</param>
    /// <param name="addTop">Add/Remove pixels on top</param>
    /// <param name="addBottom">Add/Remove pixels on bottom</param>
    /// <param name="fillColor">Fillcolor (if add pixels, default: transparency)</param>
    /// <returns>Resized bitmap</returns>
    public FastBitmapOld GetResizedCanvas(int addLeft = 0, int addRight = 0, int addTop = 0, int addBottom = 0, uint fillColor = 0x00000000)
    {
      int newWidth = addLeft + width + addRight;
      int newHeight = addTop + height + addBottom;
      if (newWidth < 1 || newHeight < 1) throw new ArgumentOutOfRangeException();

      var result = new FastBitmapOld(newWidth, newHeight);
      result.DrawBitmap(this, addLeft, addTop);
      if (addLeft > 0) result.FillRectangle(0, 0, addLeft, newHeight, fillColor);
      if (addRight > 0) result.FillRectangle(newWidth - addRight, 0, addRight, newHeight, fillColor);
      if (addTop > 0) result.FillRectangle(addLeft, 0, width, addTop, fillColor);
      if (addBottom > 0) result.FillRectangle(addLeft, newHeight - addBottom, width, addBottom, fillColor);
      return result;
    }
  }
}
