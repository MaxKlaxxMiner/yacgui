#region # using *.*

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable JoinDeclarationAndInitializer
#endregion

namespace YacGui
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public partial class FastBitmap
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
    public FastBitmap GetResizedCanvas(int addLeft = 0, int addRight = 0, int addTop = 0, int addBottom = 0, uint fillColor = 0x00000000)
    {
      int newWidth = addLeft + width + addRight;
      int newHeight = addTop + height + addBottom;
      if (newWidth < 1 || newHeight < 1) throw new ArgumentOutOfRangeException();

      var result = new FastBitmap(newWidth, newHeight);
      result.DrawBitmap(this, addLeft, addTop);
      if (addLeft > 0) result.FillRectangle(0, 0, addLeft, newHeight, fillColor);
      if (addRight > 0) result.FillRectangle(newWidth - addRight, 0, addRight, newHeight, fillColor);
      if (addTop > 0) result.FillRectangle(addLeft, 0, width, addTop, fillColor);
      if (addBottom > 0) result.FillRectangle(addLeft, newHeight - addBottom, width, addBottom, fillColor);
      return result;
    }
  }
}
