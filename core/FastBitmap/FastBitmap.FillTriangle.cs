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
  public unsafe partial class FastBitmap
  {
    void FillScanlinesUnsafe(int x, int y, LinearScanLine[] scanlines, uint color)
    {
      fixed (uint* pixelPtr = pixels)
      {
        int w = width;
        uint* ptr = pixelPtr + w * y + x;
        for (int line = 0; line < scanlines.Length; line++)
        {
          int endX = scanlines[line].endX;
          for (int scanX = scanlines[line].startX; scanX <= endX; scanX++) ptr[scanX] = color;
          ptr += w;
        }
      }
    }

    public void FillTriangleUnsafe(int x1, int y1, int x2, int y2, int x3, int y3, uint color)
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

      var scanlines = new LinearScanLine[y3 + 1];
      fixed (LinearScanLine* ptr = scanlines)
      {
        LinearScanLine.Clear(ptr, scanlines.Length);
        LinearScanLine.AddLine(ptr, x1, 0, x2, y2);
        LinearScanLine.AddLine(ptr, x2, y2, x3, y3);
        LinearScanLine.AddLine(ptr, x3, y3, x1, 0);

        FillScanlinesUnsafe(0, y1, scanlines, color);
      }
    }

    void FillScanlinesSafe(int x, int y, LinearScanLine[] scanlines, uint color)
    {
      fixed (uint* pixelPtr = pixels)
      {
        int w = width;
        uint* ptr = pixelPtr + w * y;
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

    void FillTriangleSafe(int x1, int y1, int x2, int y2, int x3, int y3, uint color)
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

        FillScanlinesSafe(0, y1, scanlines, color);
      }
    }

    public void FillTriangle(int x1, int y1, int x2, int y2, int x3, int y3, uint color)
    {
      //if ((uint)x1 < width && (uint)y1 < height && (uint)x2 < width && (uint)y2 < height && (uint)x3 < width && (uint)y3 < height)
      //{
      //  FillTriangleUnsafe(x1, y1, x2, y2, x3, y3, color);
      //  return;
      //}

      FillTriangleSafe(x1, y1, x2, y2, x3, y3, color);
    }
  }
}
