using System;
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public sealed unsafe partial class FastBitmapOld
  {
    /// <summary>
    /// drawing normal line (safe-mode)
    /// </summary>
    /// <param name="ptr">pointer to the backbuffer</param>
    /// <param name="x1">start x-position</param>
    /// <param name="y1">start y-position</param>
    /// <param name="x2">end x-position</param>
    /// <param name="y2">end y-position</param>
    /// <param name="color">line-color</param>
    void DrawLineSafe(uint* ptr, int x1, int y1, int x2, int y2, uint color)
    {
      bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
      if (steep) { int t = x1; x1 = y1; y1 = t; t = x2; x2 = y2; y2 = t; }
      if (x1 > x2) { int t = x1; x1 = x2; x2 = t; t = y1; y1 = y2; y2 = t; }

      int dx = x2 - x1;
      int dy = Math.Abs(y2 - y1);
      int err = dx / 2;
      int ystep = y1 < y2 ? 1 : -1;

      if (steep)
      {
        for (; x1 <= x2; x1++)
        {
          if ((uint)y1 < width && (uint)x1 < height) ptr[y1 + x1 * width] = color;
          err -= dy;
          if (err < 0)
          {
            y1 += ystep;
            err += dx;
          }
        }
      }
      else
      {
        for (; x1 <= x2; x1++)
        {
          if ((uint)x1 < width && (uint)y1 < height) ptr[x1 + y1 * width] = color;
          err -= dy;
          if (err < 0)
          {
            y1 += ystep;
            err += dx;
          }
        }
      }
    }

    /// <summary>
    /// drawing normal line
    /// </summary>
    /// <param name="ptr">pointer to the backbuffer (+start position)</param>
    /// <param name="x1">start x-position</param>
    /// <param name="y1">start y-position</param>
    /// <param name="x2">end x-position</param>
    /// <param name="y2">end y-position</param>
    /// <param name="color">line-color</param>
    void DrawLineFast(uint* ptr, int x1, int y1, int x2, int y2, uint color)
    {
      if ((uint)x1 < width && (uint)y1 < height && (uint)x2 < width && (uint)y2 < height)
      {
        bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
        if (steep) { int t = x1; x1 = y1; y1 = t; t = x2; x2 = y2; y2 = t; }
        if (x1 > x2) { int t = x1; x1 = x2; x2 = t; t = y1; y1 = y2; y2 = t; }

        int dx = x2 - x1;
        int dy = Math.Abs(y2 - y1);
        int err = dx / 2;
        int ystep = y1 < y2 ? 1 : -1;

        if (steep)
        {
          var p = ptr + y1 + x1 * width;
          for (; x1 <= x2; x1++)
          {
            *p = color;
            err -= dy;
            p += width;
            if (err < 0)
            {
              p += ystep;
              err += dx;
            }
          }
        }
        else
        {
          var p = ptr + x1 + y1 * width;
          ystep *= width;
          for (; x1 <= x2; x1++)
          {
            *p = color;
            err -= dy;
            p++;
            if (err < 0)
            {
              err += dx;
              p += ystep;
            }
          }
        }
      }
      else
      {
        DrawLineSafe(ptr, x1, y1, x2, y2, color);
      }
    }

    /// <summary>
    /// drawing normal line
    /// </summary>
    /// <param name="x1">start x-position</param>
    /// <param name="y1">start y-position</param>
    /// <param name="x2">end x-position</param>
    /// <param name="y2">end y-position</param>
    /// <param name="color">line-color</param>
    public void DrawLine(int x1, int y1, int x2, int y2, uint color)
    {
      fixed (uint* ptr = pixels)
      {
        DrawLineFast(ptr, x1, y1, x2, y2, color);
      }
    }
  }
}
