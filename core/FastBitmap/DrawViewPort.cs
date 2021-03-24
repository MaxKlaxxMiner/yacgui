// ReSharper disable MemberCanBePrivate.Global

using System.Drawing;

namespace YacGui
{
  public sealed class DrawViewPort
  {
    public int startX;
    public int startY;
    public int endX;
    public int endY;

    public void Expand(int x, int y)
    {
      if (x < startX) startX = x;
      if (x > endX) endX = x;
      if (y < startY) startY = y;
      if (y > endY) endY = y;
    }

    public void Expand(int x, int y, int w, int h)
    {
      Expand(x, y);
      Expand(x + w - 1, y + h - 1);
    }

    public void Expand(Rectangle rect)
    {
      Expand(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void Reset()
    {
      startX = int.MaxValue;
      startY = int.MaxValue;
      endX = int.MinValue;
      endY = int.MinValue;
    }

    public DrawViewPort()
    {
      Reset();
    }
  }
}
