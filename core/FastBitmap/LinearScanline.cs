using System;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Struct of a scanline with helper methods
  /// </summary>
  public unsafe struct LinearScanLine
  {
    /// <summary>
    /// Start of the scanline
    /// </summary>
    public int startX;
    /// <summary>
    /// End of the scanline
    /// </summary>
    public int endX;

    /// <summary>
    /// Adds a pixel and expands the start/end values if necessary
    /// </summary>
    /// <param name="x">x-position to add</param>
    public void AddPixel(int x)
    {
      if (x < startX) startX = x;
      if (x > endX) endX = x;
    }

    /// <summary>
    /// Adds a line to a scanline array (the y-positions must remain within the arrays)
    /// </summary>
    /// <param name="ptr">Pointer to the scanline array</param>
    /// <param name="x1">Start x-position</param>
    /// <param name="y1">Start y-position</param>
    /// <param name="x2">End x-position</param>
    /// <param name="y2">End y-position</param>
    public static void AddLine(LinearScanLine* ptr, int x1, int y1, int x2, int y2)
    {
      int dx = x2 - x1; // x-direction
      int dy = y2 - y1; // y-direction

      // --- check for horizontal line ---
      if (dy == 0)
      {
        ptr[y1].AddPixel(x1);
        ptr[y1].AddPixel(x2);
        return;
      }

      // --- check for vertical line ---
      if (dx == 0)
      {
        if (y1 > y2)
        {
          int swap = y1; y1 = y2; y2 = swap;
        }

        for (int y = y1; y <= y2; y++) ptr[y].AddPixel(x1);

        return;
      }

      // --- draw diagonal line (based on bresenham algorithm) ---
      int stepx, stepy;
      if (dy < 0)
      {
        dy = -dy; stepy = -1;
      }
      else
      {
        stepy = 1;
      }
      if (dx < 0)
      {
        dx = -dx; stepx = -1;
      }
      else
      {
        stepx = 1;
      }
      dy <<= 1;
      dx <<= 1;

      ptr[y1].AddPixel(x1); // first pixel
      if (dx > dy)
      {
        int fraction = dy - (dx >> 1);
        while (x1 != x2)
        {
          if (fraction >= 0)
          {
            y1 += stepy;
            fraction -= dx;
          }
          x1 += stepx;
          fraction += dy;
          ptr[y1].AddPixel(x1);
        }
      }
      else
      {
        int fraction = dx - (dy >> 1);
        while (y1 != y2)
        {
          if (fraction >= 0)
          {
            x1 += stepx;
            fraction -= dy;
          }
          y1 += stepy;
          fraction += dx;
          ptr[y1].AddPixel(x1);
        }
      }
    }

    /// <summary>
    /// Adds a line to a scanline array (the y-positions must remain within the arrays)
    /// </summary>
    /// <param name="scanlines">array with the scanlines</param>
    /// <param name="x1">Start x-position</param>
    /// <param name="y1">Start y-position</param>
    /// <param name="x2">End x-position</param>
    /// <param name="y2">End y-position</param>
    public static void AddLine(LinearScanLine[] scanlines, int x1, int y1, int x2, int y2)
    {
      if (scanlines == null) throw new NullReferenceException("scanlines");
      if ((uint)y1 >= scanlines.Length || (uint)y2 >= scanlines.Length) throw new ArgumentOutOfRangeException();

      fixed (LinearScanLine* ptr = &scanlines[0])
      {
        AddLine(ptr, x1, y1, x2, y2);
      }
    }

    /// <summary>
    /// Initializes a scanline array
    /// </summary>
    /// <param name="ptr">Pointer to the scanline array</param>
    /// <param name="count">Count of elements in the array</param>
    public static void Clear(LinearScanLine* ptr, int count)
    {
      var fill = new LinearScanLine
      {
        startX = int.MaxValue,
        endX = int.MinValue
      };
      for (int i = 0; i < count; i++) ptr[i] = fill;
    }

    /// <summary>
    /// Initializes a scanline array
    /// </summary>
    /// <param name="scanlines">Array with scanlines</param>
    public static void Clear(LinearScanLine[] scanlines)
    {
      if (scanlines == null) throw new NullReferenceException("scanlines");

      fixed (LinearScanLine* ptr = &scanlines[0])
      {
        Clear(ptr, scanlines.Length);
      }
    }

    /// <summary>
    /// Returns the properties as a readable string.
    /// </summary>
    /// <returns>Readable string</returns>
    public override string ToString()
    {
      return new { startX, endX }.ToString();
    }
  }

  /// <summary>
  /// Struct of a scanline with helper methods and linear UV-Values (texture coordinates)
  /// </summary>
  public unsafe struct LinearScanLineUV
  {
    /// <summary>
    /// Start of the scanline
    /// </summary>
    public int startX;
    /// <summary>
    /// End of the scanline
    /// </summary>
    public int endX;
    /// <summary>
    /// Start of the U-texture coordinate (comparable with x)
    /// </summary>
    public double startU;
    /// <summary>
    /// Start of the V-texture coordinate (comparable with y)
    /// </summary>
    public double startV;
    /// <summary>
    /// End of the U-texture coordinate (compareable with x)
    /// </summary>
    public double endU;
    /// <summary>
    /// End of the V-texture coordinate (compareable with y)
    /// </summary>
    public double endV;

    /// <summary>
    /// Adds a pixel and expands the start/end values if necessary
    /// </summary>
    /// <param name="x">x-position to add</param>
    /// <param name="u">U-texture coordinate</param>
    /// <param name="v">V-texture coordinate</param>
    public void AddPixel(int x, double u, double v)
    {
      if (x < startX)
      {
        startX = x;
        startU = u;
        startV = v;
      }
      if (x > endX)
      {
        endX = x;
        endU = u;
        endV = v;
      }
    }

    /// <summary>
    /// Adds a line to a scanline array (the y-positions must remain within the arrays)
    /// </summary>
    /// <param name="ptr">Pointer to the scanline array</param>
    /// <param name="x1">Start x-position</param>
    /// <param name="y1">Start y-position</param>
    /// <param name="x2">End x-position</param>
    /// <param name="y2">End y-position</param>
    /// <param name="u1">Start U-texture coordinate</param>
    /// <param name="v1">Start V-texture coordinate</param>
    /// <param name="u2">End U-texture coordinate</param>
    /// <param name="v2">End V-texture coordinate</param>
    public static void AddLine(LinearScanLineUV* ptr, int x1, int y1, int x2, int y2, double u1, double v1, double u2, double v2)
    {
      int dx = x2 - x1; // x-direction
      int dy = y2 - y1; // y-direction
      double du = u2 - u1; // u-texture direction
      double dv = v2 - v1; // v-texture direction

      // --- check for horizontal line ---
      if (dy == 0)
      {
        ptr[y1].AddPixel(x1, u1, v1);
        ptr[y1].AddPixel(x2, u2, v2);
        return;
      }

      // --- check for vertical line ---
      if (dx == 0)
      {
        double uStep = du / dy;
        double vStep = dv / dy;
        if (y1 <= y2)
        {
          for (int i = y1; i <= y2; i++)
          {
            ptr[i].AddPixel(x1, u1, v1);
            u1 += uStep;
            v1 += vStep;
          }
        }
        else
        {
          for (int i = y2; i <= y1; i++)
          {
            ptr[i].AddPixel(x1, u2, v2);
            u2 += uStep;
            v2 += vStep;
          }
        }
        return; // fertig
      }

      // --- draw diagonal line (based on bresenham algorithm) ---
      int stepx, stepy;
      if (dy < 0)
      {
        dy = -dy; stepy = -1;
      }
      else
      {
        stepy = 1;
      }
      if (dx < 0)
      {
        dx = -dx; stepx = -1;
      }
      else
      {
        stepx = 1;
      }
      dy <<= 1;
      dx <<= 1;

      ptr[y1].AddPixel(x1, u1, v1); // first pixel
      if (dx > dy)
      {
        int fraction = dy - (dx >> 1);

        du /= dx >> 1;
        dv /= dx >> 1;

        while (x1 != x2)
        {
          if (fraction >= 0)
          {
            y1 += stepy;
            fraction -= dx;
          }
          x1 += stepx;
          fraction += dy;
          u1 += du;
          v1 += dv;
          ptr[y1].AddPixel(x1, u1, v1);
        }
      }
      else
      {
        int fraction = dx - (dy >> 1);

        du /= dy >> 1;
        dv /= dy >> 1;

        while (y1 != y2)
        {
          if (fraction >= 0)
          {
            x1 += stepx;
            fraction -= dy;
          }
          y1 += stepy;
          fraction += dx;
          u1 += du;
          v1 += dv;
          ptr[y1].AddPixel(x1, u1, v1);
        }
      }
    }

    /// <summary>
    /// Adds a line to a scanline array (the y-positions must remain within the arrays)
    /// </summary>
    /// <param name="scanlines">Array of the scanlines</param>
    /// <param name="x1">Start x-position</param>
    /// <param name="y1">Start y-position</param>
    /// <param name="x2">End x-position</param>
    /// <param name="y2">End y-position</param>
    /// <param name="u1">Start U-texture coordinate</param>
    /// <param name="v1">Start V-texture coordinate</param>
    /// <param name="u2">End U-texture coordinate</param>
    /// <param name="v2">End V-texture coordinate</param>
    public static void AddLine(LinearScanLineUV[] scanlines, int x1, int y1, int x2, int y2, double u1, double v1, double u2, double v2)
    {
      if (scanlines == null) throw new NullReferenceException("scanlines");
      if ((uint)y1 >= scanlines.Length || (uint)y2 >= scanlines.Length) throw new ArgumentOutOfRangeException();

      fixed (LinearScanLineUV* ptr = &scanlines[0])
      {
        AddLine(ptr, x1, y1, x2, y2, u1, v1, u2, v2);
      }
    }

    /// <summary>
    /// Initializes a scanline array
    /// </summary>
    /// <param name="ptr">Pointer to the scanline array</param>
    /// <param name="count">Count of elements in the array</param>
    public static void Clear(LinearScanLineUV* ptr, int count)
    {
      var fill = new LinearScanLineUV
      {
        startX = int.MaxValue,
        endX = int.MinValue,
        startU = 0.0,
        startV = 0.0,
        endU = 0.0,
        endV = 0.0
      };
      for (int i = 0; i < count; i++) ptr[i] = fill;
    }

    /// <summary>
    /// Initializes a scanline array
    /// </summary>
    /// <param name="scanlines">Array with scanlines</param>
    public static void Clear(LinearScanLineUV[] scanlines)
    {
      if (scanlines == null) throw new NullReferenceException("scanlines");

      fixed (LinearScanLineUV* ptr = &scanlines[0])
      {
        Clear(ptr, scanlines.Length);
      }
    }

    /// <summary>
    /// Returns the properties as a readable string.
    /// </summary>
    /// <returns>Readable string</returns>
    public override string ToString()
    {
      return new { startX, endX, startU, endU, startV, endV }.ToString();
    }
  }
}
