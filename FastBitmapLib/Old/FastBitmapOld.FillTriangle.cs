// ReSharper disable UnusedMember.Global

using FastBitmapLib.Extras;

namespace FastBitmapLib
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public sealed unsafe partial class FastBitmapOld
  {
    /// <summary>
    /// Fill the scanlines with a solid color (without boundary checks)
    /// </summary>
    /// <param name="x">x-position (left offset)</param>
    /// <param name="y">y-position (top offset)</param>
    /// <param name="scanlines">Scanlines to be draw</param>
    /// <param name="color">Fill color</param>
    void FillScanlinesUnsafe(int x, int y, LinearScanLine[] scanlines, uint color)
    {
      fixed (uint* pixelPtr = pixels)
      {
        int w = width;
        var ptr = pixelPtr + w * y + x;
        for (int line = 0; line < scanlines.Length; line++)
        {
          int endX = scanlines[line].endX;
          for (int scanX = scanlines[line].startX; scanX <= endX; scanX++) ptr[scanX] = color;
          ptr += w;
        }
      }
    }

    /// <summary>
    /// Fill the scanlines with a solid color (inclusive boundary checks)
    /// </summary>
    /// <param name="x">x-position (left offset)</param>
    /// <param name="y">y-position (top offset)</param>
    /// <param name="scanlines">Scanlines to be draw</param>
    /// <param name="color">Fill color</param>
    void FillScanlinesSafe(int x, int y, LinearScanLine[] scanlines, uint color)
    {
      fixed (uint* pixelPtr = pixels)
      {
        int w = width;
        var ptr = pixelPtr + w * y;
        for (int line = 0; line < scanlines.Length; line++)
        {
          if (y + line >= 0 && y + line < height)
          {
            int startX = scanlines[line].startX + x;
            int endX = scanlines[line].endX + x;
            if (startX < 0) startX = 0;
            if (endX >= width) endX = width - 1;
            for (int scanX = startX; scanX <= endX; scanX++) ptr[scanX] = color;
          }
          ptr += w;
        }
      }
    }

    /// <summary>
    /// Draw a filled triangle (safe-mode with boundary check)
    /// </summary>
    /// <param name="x1">First x-point</param>
    /// <param name="y1">First y-point</param>
    /// <param name="x2">Second x-point</param>
    /// <param name="y2">Second y-point</param>
    /// <param name="x3">Third x-point</param>
    /// <param name="y3">Third y-point</param>
    /// <param name="color">Fill color</param>
    public void FillTriangle(int x1, int y1, int x2, int y2, int x3, int y3, uint color)
    {
      // --- sort by y ---
      if (y1 > y2)
      {
        int tx = x1; int ty = y1; x1 = x2; y1 = y2; x2 = tx; y2 = ty;
      }
      if (y2 > y3)
      {
        int tx = x2; int ty = y2; x2 = x3; y2 = y3; x3 = tx; y3 = ty;
      }
      if (y1 > y2)
      {
        int tx = x1; int ty = y1; x1 = x2; y1 = y2; x2 = tx; y2 = ty;
      }

      // --- calculate offsets ---
      y2 -= y1;
      y3 -= y1;

      if (y1 >= height) return; // below picture
      if (y3 < 0) return; // above picture

      var scanlines = new LinearScanLine[y3 + 1];
      fixed (LinearScanLine* ptr = scanlines)
      {
        LinearScanLine.Clear(ptr, scanlines.Length);
        LinearScanLine.AddLine(ptr, x1, 0, x2, y2);
        LinearScanLine.AddLine(ptr, x2, y2, x3, y3);
        LinearScanLine.AddLine(ptr, x3, y3, x1, 0);

        if ((uint)x1 < width && (uint)x2 < width && (uint)x3 < width && y1 >= 0 && y1 + y3 < height)
        {
          FillScanlinesUnsafe(0, y1, scanlines, color);
        }
        else
        {
          FillScanlinesSafe(0, y1, scanlines, color);
        }
      }
    }

    /// <summary>
    /// Draw a filled triangle (safe-mode with boundary check)
    /// </summary>
    /// <param name="x1">First x-position (e.g. from top left)</param>
    /// <param name="y1">First y-position (e.g. from top left)</param>
    /// <param name="x2">Second x-position (e.g. from top right)</param>
    /// <param name="y2">Second y-position (e.g. from top right)</param>
    /// <param name="x3">Third x-position (e.g. from bottom right)</param>
    /// <param name="y3">Third y-position (e.g. from bottom right)</param>
    /// <param name="x4">Fourth x-position (e.g. from bottom left)</param>
    /// <param name="y4">Fourth y-position (e.g. from bottom left)</param>
    /// <param name="color">Fill color</param>
    public void FillQuad(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, uint color)
    {
      // --- sort by y ---
      int topY = y1;
      int bottomY = y1;
      if (y2 < topY) topY = y2;
      if (y2 > bottomY) bottomY = y2;
      if (y3 < topY) topY = y3;
      if (y3 > bottomY) bottomY = y3;
      if (y4 < topY) topY = y4;
      if (y4 > bottomY) bottomY = y4;

      // --- calculate offsets ---
      y1 -= topY;
      y2 -= topY;
      y3 -= topY;
      y4 -= topY;

      if (topY >= height) return; // below picture
      if (bottomY < 0) return; // above picture

      var scanlines = new LinearScanLine[bottomY - topY + 1];
      fixed (LinearScanLine* ptr = scanlines)
      {
        LinearScanLine.Clear(ptr, scanlines.Length);
        LinearScanLine.AddLine(ptr, x1, y1, x2, y2);
        LinearScanLine.AddLine(ptr, x2, y2, x3, y3);
        LinearScanLine.AddLine(ptr, x3, y3, x4, y4);
        LinearScanLine.AddLine(ptr, x4, y4, x1, y1);

        if ((uint)x1 < width && (uint)x2 < width && (uint)x3 < width && (uint)x4 < width && topY >= 0 && bottomY < height)
        {
          FillScanlinesUnsafe(0, topY, scanlines, color);
        }
        else
        {
          FillScanlinesSafe(0, topY, scanlines, color);
        }
      }
    }
  }
}
