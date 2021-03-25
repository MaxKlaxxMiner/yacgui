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
    /// <summary>
    /// Fill scanlines with a custom uvMap function (without boundary checks)
    /// </summary>
    /// <param name="x">x-position (left offset)</param>
    /// <param name="y">y-position (top offset)</param>
    /// <param name="scanlines">Scanlines to be draw</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v)</param>
    void FillScanlinesUnsafe(int x, int y, LinearScanLineUV[] scanlines, Func<double, double, uint> uvMap)
    {
      fixed (uint* pixelPtr = pixels)
      {
        int w = width;
        uint* ptr = pixelPtr + w * y + x;
        for (int line = 0; line < scanlines.Length; line++)
        {
          int endX = scanlines[line].endX;
          int d = endX - scanlines[line].startX;
          double u = scanlines[line].startU;
          double v = scanlines[line].startV;
          double du = scanlines[line].endU - u;
          double dv = scanlines[line].endV - v;

          du /= d; dv /= d;

          for (int scanX = scanlines[line].startX; scanX <= endX; scanX++)
          {
            uint color = uvMap(u, v);
            ptr[scanX] = color;
            u += du;
            v += dv;
          }
          ptr += w;
        }
      }
    }

    /// <summary>
    /// Fill scanlines with a custom uvMap function (without boundary checks)
    /// </summary>
    /// <param name="x">x-position (left offset)</param>
    /// <param name="y">y-position (top offset)</param>
    /// <param name="scanlines">Scanlines to be draw</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v, uint sourceColor)</param>
    void FillScanlinesUnsafe(int x, int y, LinearScanLineUV[] scanlines, Func<double, double, uint, uint> uvMap)
    {
      fixed (uint* pixelPtr = pixels)
      {
        int w = width;
        uint* ptr = pixelPtr + w * y + x;
        for (int line = 0; line < scanlines.Length; line++)
        {
          int endX = scanlines[line].endX;
          int d = endX - scanlines[line].startX;
          double u = scanlines[line].startU;
          double v = scanlines[line].startV;
          double du = scanlines[line].endU - u;
          double dv = scanlines[line].endV - v;

          du /= d; dv /= d;

          for (int scanX = scanlines[line].startX; scanX <= endX; scanX++)
          {
            uint color = uvMap(u, v, ptr[scanX]);
            ptr[scanX] = color;
            u += du;
            v += dv;
          }
          ptr += w;
        }
      }
    }

    /// <summary>
    /// Fill scanlines with a custom uvMap function (inclusive boundary checks)
    /// </summary>
    /// <param name="x">x-position (left offset)</param>
    /// <param name="y">y-position (top offset)</param>
    /// <param name="scanlines">Scanlines to be draw</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v)</param>
    void FillScanlinesSafe(int x, int y, LinearScanLineUV[] scanlines, Func<double, double, uint> uvMap)
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
            int d = endX - startX;
            double u = scanlines[line].startU;
            double v = scanlines[line].startV;
            double du = scanlines[line].endU - u;
            double dv = scanlines[line].endV - v;

            du /= d; dv /= d;

            for (int scanX = startX; scanX <= endX; scanX++)
            {
              if (scanX >= 0 && scanX < w)
              {
                uint color = uvMap(u, v);
                ptr[scanX] = color;
              }
              u += du;
              v += dv;
            }
          }
          ptr += w;
        }
      }
    }

    /// <summary>
    /// Fill scanlines with a custom uvMap function (inclusive boundary checks)
    /// </summary>
    /// <param name="x">x-position (left offset)</param>
    /// <param name="y">y-position (top offset)</param>
    /// <param name="scanlines">Scanlines to be draw</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v, uint sourceColor)</param>
    void FillScanlinesSafe(int x, int y, LinearScanLineUV[] scanlines, Func<double, double, uint, uint> uvMap)
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
            int d = endX - startX;
            double u = scanlines[line].startU;
            double v = scanlines[line].startV;
            double du = scanlines[line].endU - u;
            double dv = scanlines[line].endV - v;

            du /= d; dv /= d;

            for (int scanX = startX; scanX <= endX; scanX++)
            {
              if (scanX >= 0 && scanX < w)
              {
                uint color = uvMap(u, v, ptr[scanX]);
                ptr[scanX] = color;
              }
              u += du;
              v += dv;
            }
          }
          ptr += w;
        }
      }
    }

    /// <summary>
    /// Fill scanlines with a custom uvMap function and use a 3D tranformation matrix
    /// </summary>
    /// <param name="x">x-position (left offset)</param>
    /// <param name="y">y-position (top offset)</param>
    /// <param name="scanlines">Scanlines to be draw</param>
    /// <param name="transformMatrix">3D-Matrix for the perspective correction</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v, uint sourceColor)</param>
    void FillScanlinesMatrix(int x, int y, LinearScanLineUV[] scanlines, Mapping3D.Matrix33 transformMatrix, Func<double, double, uint, uint> uvMap)
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
            double sy = line;
            double smz = sy * transformMatrix.m12 + transformMatrix.m22;
            double smx = sy * transformMatrix.m10 + transformMatrix.m20;
            double smy = sy * transformMatrix.m11 + transformMatrix.m21;

            for (int scanX = startX; scanX <= endX; scanX++)
            {
              if (scanX >= 0 && scanX < w)
              {
                double sx = scanX;
                double z = 1.0 / (sx * transformMatrix.m02 + smz);
                double u = (sx * transformMatrix.m00 + smx) * z;
                double v = (sx * transformMatrix.m01 + smy) * z;
                uint color = uvMap(u, v, ptr[scanX]);
                ptr[scanX] = color;
              }
            }
          }
          ptr += w;
        }
      }
    }

    /// <summary>
    /// Draw a linear filled triangle with a custom uvMap function
    /// </summary>
    /// <param name="x1">First x-position</param>
    /// <param name="y1">First y-position</param>
    /// <param name="x2">Second x-position</param>
    /// <param name="y2">Second y-position</param>
    /// <param name="x3">Third x-position</param>
    /// <param name="y3">Third y-position</param>
    /// <param name="u1">First u-coordinate (x-pos of texture)</param>
    /// <param name="v1">First v-coordinate (y-pos of texture)</param>
    /// <param name="u2">Second u-coordinate (x-pos of texture)</param>
    /// <param name="v2">Second v-coordinate (y-pos of texture)</param>
    /// <param name="u3">Third u-coordinate (x-pos of texture)</param>
    /// <param name="v3">Third v-coordinate (y-pos of texture)</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v)</param>
    public void MappedTriangle(int x1, int y1, int x2, int y2, int x3, int y3, double u1, double v1, double u2, double v2, double u3, double v3, Func<double, double, uint> uvMap)
    {
      // --- sort by y ---
      if (y1 > y2)
      {
        int tx = x1; int ty = y1; x1 = x2; y1 = y2; x2 = tx; y2 = ty;
        double tu = u1; double tv = v1; u1 = u2; v1 = v2; u2 = tu; v2 = tv;
      }
      if (y2 > y3)
      {
        int tx = x2; int ty = y2; x2 = x3; y2 = y3; x3 = tx; y3 = ty;
        double tu = u2; double tv = v2; u2 = u3; v2 = v3; u3 = tu; v3 = tv;
      }
      if (y1 > y2)
      {
        int tx = x1; int ty = y1; x1 = x2; y1 = y2; x2 = tx; y2 = ty;
        double tu = u1; double tv = v1; u1 = u2; v1 = v2; u2 = tu; v2 = tv;
      }

      // --- calculate offsets ---
      y2 -= y1;
      y3 -= y1;

      if (y1 >= height) return; // below picture
      if (y3 < 0) return; // above picture

      var scanlines = new LinearScanLineUV[y3 + 1];
      fixed (LinearScanLineUV* ptr = scanlines)
      {
        LinearScanLineUV.Clear(ptr, scanlines.Length);
        LinearScanLineUV.AddLine(ptr, x1, 0, x2, y2, u1, v1, u2, v2);
        LinearScanLineUV.AddLine(ptr, x2, y2, x3, y3, u2, v2, u3, v3);
        LinearScanLineUV.AddLine(ptr, x3, y3, x1, 0, u3, v3, u1, v1);

        if ((uint)x1 < width && (uint)x2 < width && (uint)x3 < width && y1 >= 0 && y1 + y3 < height)
        {
          FillScanlinesUnsafe(0, y1, scanlines, uvMap);
        }
        else
        {
          FillScanlinesSafe(0, y1, scanlines, uvMap);
        }
      }
    }

    /// <summary>
    /// Draw a linear filled triangle with a custom uvMap function (inclusive source color for alpha blend etc.)
    /// </summary>
    /// <param name="x1">First x-position</param>
    /// <param name="y1">First y-position</param>
    /// <param name="x2">Second x-position</param>
    /// <param name="y2">Second y-position</param>
    /// <param name="x3">Third x-position</param>
    /// <param name="y3">Third y-position</param>
    /// <param name="u1">First u-coordinate (x-pos of texture)</param>
    /// <param name="v1">First v-coordinate (y-pos of texture)</param>
    /// <param name="u2">Second u-coordinate (x-pos of texture)</param>
    /// <param name="v2">Second v-coordinate (y-pos of texture)</param>
    /// <param name="u3">Third u-coordinate (x-pos of texture)</param>
    /// <param name="v3">Third v-coordinate (y-pos of texture)</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v, uint sourceColor)</param>
    public void MappedTriangle(int x1, int y1, int x2, int y2, int x3, int y3, double u1, double v1, double u2, double v2, double u3, double v3, Func<double, double, uint, uint> uvMap)
    {
      // --- sort by y ---
      if (y1 > y2)
      {
        int tx = x1; int ty = y1; x1 = x2; y1 = y2; x2 = tx; y2 = ty;
        double tu = u1; double tv = v1; u1 = u2; v1 = v2; u2 = tu; v2 = tv;
      }
      if (y2 > y3)
      {
        int tx = x2; int ty = y2; x2 = x3; y2 = y3; x3 = tx; y3 = ty;
        double tu = u2; double tv = v2; u2 = u3; v2 = v3; u3 = tu; v3 = tv;
      }
      if (y1 > y2)
      {
        int tx = x1; int ty = y1; x1 = x2; y1 = y2; x2 = tx; y2 = ty;
        double tu = u1; double tv = v1; u1 = u2; v1 = v2; u2 = tu; v2 = tv;
      }

      // --- calculate offsets ---
      y2 -= y1;
      y3 -= y1;

      if (y1 >= height) return; // below picture
      if (y3 < 0) return; // above picture

      var scanlines = new LinearScanLineUV[y3 + 1];
      fixed (LinearScanLineUV* ptr = scanlines)
      {
        LinearScanLineUV.Clear(ptr, scanlines.Length);
        LinearScanLineUV.AddLine(ptr, x1, 0, x2, y2, u1, v1, u2, v2);
        LinearScanLineUV.AddLine(ptr, x2, y2, x3, y3, u2, v2, u3, v3);
        LinearScanLineUV.AddLine(ptr, x3, y3, x1, 0, u3, v3, u1, v1);

        if ((uint)x1 < width && (uint)x2 < width && (uint)x3 < width && y1 >= 0 && y1 + y3 < height)
        {
          FillScanlinesUnsafe(0, y1, scanlines, uvMap);
        }
        else
        {
          FillScanlinesSafe(0, y1, scanlines, uvMap);
        }
      }
    }

    /// <summary>
    /// Draw a linear filled quad with a custom uvMap function
    /// </summary>
    /// <param name="x1">First x-position (e.g. from top left)</param>
    /// <param name="y1">First y-position (e.g. from top left)</param>
    /// <param name="x2">Second x-position (e.g. from top right)</param>
    /// <param name="y2">Second y-position (e.g. from top right)</param>
    /// <param name="x3">Third x-position (e.g. from bottom left)</param>
    /// <param name="y3">Third y-position (e.g. from bottom left)</param>
    /// <param name="x4">Fourth x-position (e.g. from bottom right)</param>
    /// <param name="y4">Fourth y-position (e.g. from bottom right)</param>
    /// <param name="u1">First u-coordinate (x-pos of texture)</param>
    /// <param name="v1">First v-coordinate (y-pos of texture)</param>
    /// <param name="u2">Second u-coordinate (x-pos of texture)</param>
    /// <param name="v2">Second v-coordinate (y-pos of texture)</param>
    /// <param name="u3">Third u-coordinate (x-pos of texture)</param>
    /// <param name="v3">Third v-coordinate (y-pos of texture)</param>
    /// <param name="u4">Fourth u-coordinate (x-pos of texture)</param>
    /// <param name="v4">Fourth v-coordinate (y-pos of texture)</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v)</param>
    public void MappedQuad(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, double u1, double v1, double u2, double v2, double u3, double v3, double u4, double v4, Func<double, double, uint> uvMap)
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

      var scanlines = new LinearScanLineUV[bottomY - topY + 1];
      fixed (LinearScanLineUV* ptr = scanlines)
      {
        LinearScanLineUV.Clear(ptr, scanlines.Length);
        LinearScanLineUV.AddLine(ptr, x1, y1, x2, y2, u1, v1, u2, v2);
        LinearScanLineUV.AddLine(ptr, x2, y2, x3, y3, u2, v2, u3, v3);
        LinearScanLineUV.AddLine(ptr, x3, y3, x4, y4, u3, v3, u4, v4);
        LinearScanLineUV.AddLine(ptr, x4, y4, x1, y1, u4, v4, u1, v1);

        if ((uint)x1 < width && (uint)x2 < width && (uint)x3 < width && (uint)x4 < width && topY >= 0 && bottomY < height)
        {
          FillScanlinesUnsafe(0, topY, scanlines, uvMap);
        }
        else
        {
          FillScanlinesSafe(0, topY, scanlines, uvMap);
        }
      }
    }

    /// <summary>
    /// Draw a linear filled quad with a custom uvMap function (inclusive source color for alpha blend etc.)
    /// </summary>
    /// <param name="x1">First x-position (e.g. from top left)</param>
    /// <param name="y1">First y-position (e.g. from top left)</param>
    /// <param name="x2">Second x-position (e.g. from top right)</param>
    /// <param name="y2">Second y-position (e.g. from top right)</param>
    /// <param name="x3">Third x-position (e.g. from bottom left)</param>
    /// <param name="y3">Third y-position (e.g. from bottom left)</param>
    /// <param name="x4">Fourth x-position (e.g. from bottom right)</param>
    /// <param name="y4">Fourth y-position (e.g. from bottom right)</param>
    /// <param name="u1">First u-coordinate (x-pos of texture)</param>
    /// <param name="v1">First v-coordinate (y-pos of texture)</param>
    /// <param name="u2">Second u-coordinate (x-pos of texture)</param>
    /// <param name="v2">Second v-coordinate (y-pos of texture)</param>
    /// <param name="u3">Third u-coordinate (x-pos of texture)</param>
    /// <param name="v3">Third v-coordinate (y-pos of texture)</param>
    /// <param name="u4">Fourth u-coordinate (x-pos of texture)</param>
    /// <param name="v4">Fourth v-coordinate (y-pos of texture)</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v, uint sourceColor)</param>
    public void MappedQuad(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, double u1, double v1, double u2, double v2, double u3, double v3, double u4, double v4, Func<double, double, uint, uint> uvMap)
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

      var scanlines = new LinearScanLineUV[bottomY - topY + 1];
      fixed (LinearScanLineUV* ptr = scanlines)
      {
        LinearScanLineUV.Clear(ptr, scanlines.Length);
        LinearScanLineUV.AddLine(ptr, x1, y1, x2, y2, u1, v1, u2, v2);
        LinearScanLineUV.AddLine(ptr, x2, y2, x3, y3, u2, v2, u3, v3);
        LinearScanLineUV.AddLine(ptr, x3, y3, x4, y4, u3, v3, u4, v4);
        LinearScanLineUV.AddLine(ptr, x4, y4, x1, y1, u4, v4, u1, v1);

        if ((uint)x1 < width && (uint)x2 < width && (uint)x3 < width && (uint)x4 < width && topY >= 0 && bottomY < height)
        {
          FillScanlinesUnsafe(0, topY, scanlines, uvMap);
        }
        else
        {
          FillScanlinesSafe(0, topY, scanlines, uvMap);
        }
      }
    }

    /// <summary>
    /// Draw a perspective corrected quad with a custom uvMap function (inclusive source color for alpha blend etc.)
    /// </summary>
    /// <param name="x1">First x-position (e.g. from top left)</param>
    /// <param name="y1">First y-position (e.g. from top left)</param>
    /// <param name="x2">Second x-position (e.g. from top right)</param>
    /// <param name="y2">Second y-position (e.g. from top right)</param>
    /// <param name="x3">Third x-position (e.g. from bottom left)</param>
    /// <param name="y3">Third y-position (e.g. from bottom left)</param>
    /// <param name="x4">Fourth x-position (e.g. from bottom right)</param>
    /// <param name="y4">Fourth y-position (e.g. from bottom right)</param>
    /// <param name="u1">First u-coordinate (x-pos of texture)</param>
    /// <param name="v1">First v-coordinate (y-pos of texture)</param>
    /// <param name="u2">Second u-coordinate (x-pos of texture)</param>
    /// <param name="v2">Second v-coordinate (y-pos of texture)</param>
    /// <param name="u3">Third u-coordinate (x-pos of texture)</param>
    /// <param name="v3">Third v-coordinate (y-pos of texture)</param>
    /// <param name="u4">Fourth u-coordinate (x-pos of texture)</param>
    /// <param name="v4">Fourth v-coordinate (y-pos of texture)</param>
    /// <param name="uvMap">Map-function which returns the color value (params: double u, double v, uint sourceColor)</param>
    public void MappedQuadPerspective(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, double u1, double v1, double u2, double v2, double u3, double v3, double u4, double v4, Func<double, double, uint, uint> uvMap)
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

      var scanlines = new LinearScanLineUV[bottomY - topY + 1];
      fixed (LinearScanLineUV* ptr = scanlines)
      {
        LinearScanLineUV.Clear(ptr, scanlines.Length);
        LinearScanLineUV.AddLine(ptr, x1, y1, x2, y2, u1, v1, u2, v2);
        LinearScanLineUV.AddLine(ptr, x2, y2, x3, y3, u2, v2, u3, v3);
        LinearScanLineUV.AddLine(ptr, x3, y3, x4, y4, u3, v3, u4, v4);
        LinearScanLineUV.AddLine(ptr, x4, y4, x1, y1, u4, v4, u1, v1);

        var poly = new[]
        {
          new Mapping3D.Vertex { u = u1, v = v1, x = x1, y = y1 },
          new Mapping3D.Vertex { u = u2, v = v2, x = x2, y = y2 },
          new Mapping3D.Vertex { u = u4, v = v4, x = x4, y = y4 },
          new Mapping3D.Vertex { u = u3, v = v3, x = x3, y = y3 },
        };

        Mapping3D.Matrix33 mappingMatrix;

        var mappingType = Mapping3D.MapPolygon(poly, out mappingMatrix);
        if (mappingType == Mapping3D.MappingType.Projective)
        {
          FillScanlinesMatrix(0, topY, scanlines, mappingMatrix, uvMap);
        }
        else // for affine or invalid mappings
        {
          if ((uint)x1 < width && (uint)x2 < width && (uint)x3 < width && (uint)x4 < width && topY >= 0 && bottomY < height)
          {
            FillScanlinesUnsafe(0, topY, scanlines, uvMap);
          }
          else
          {
            FillScanlinesSafe(0, topY, scanlines, uvMap);
          }
        }
      }
    }
  }
}
