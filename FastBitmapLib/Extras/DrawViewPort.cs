using System.Drawing;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib.Extras
{
  /// <summary>
  /// Helper class to calculate a viewport
  /// </summary>
  public sealed class DrawViewPort
  {
    /// <summary>
    /// start-x (left side)
    /// </summary>
    public int startX;
    /// <summary>
    /// start-y (top edge)
    /// </summary>
    public int startY;
    /// <summary>
    /// end-x (right side)
    /// </summary>
    public int endX;
    /// <summary>
    /// end-y (bottom edge)
    /// </summary>
    public int endY;

    /// <summary>
    /// Add a Pixel and expand the viewport
    /// </summary>
    /// <param name="x">X-position</param>
    /// <param name="y">Y-position</param>
    public void Expand(int x, int y)
    {
      if (x < startX) startX = x;
      if (x > endX) endX = x;
      if (y < startY) startY = y;
      if (y > endY) endY = y;
    }

    /// <summary>
    /// Add a rectangle and expand the viewport
    /// </summary>
    /// <param name="x">X-position (left side)</param>
    /// <param name="y">Y-position (top edge)</param>
    /// <param name="w">Width of the rectangle</param>
    /// <param name="h">Height of the rectangle</param>
    public void Expand(int x, int y, int w, int h)
    {
      Expand(x, y);
      Expand(x + w - 1, y + h - 1);
    }

    /// <summary>
    /// Add a rectangle and expand the viewport
    /// </summary>
    /// <param name="rect">Rectangle to add</param>
    public void Expand(Rectangle rect)
    {
      Expand(rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    /// Reset the Viewport to the initial state
    /// </summary>
    public void Reset()
    {
      startX = int.MaxValue;
      startY = int.MaxValue;
      endX = int.MinValue;
      endY = int.MinValue;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public DrawViewPort()
    {
      Reset();
    }
  }
}
