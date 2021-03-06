﻿#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using FastBitmapLib;
using YacGui;
// ReSharper disable MemberCanBePrivate.Global
#endregion

namespace TestTool
{
  /// <summary>
  /// Class for testing bitmap functions
  /// </summary>
  public partial class BitmapTests
  {
    /// <summary>
    /// Draw surfaces with gradient colors
    /// </summary>
    public static void ShowTextureMappedSimple()
    {
      var fastBitmap = new FastBitmapOld(1024, 576, 0xff252525);

      Func<double, double, uint> map = (u, v) => FastBitmapOld.ColorBlend(FastBitmapOld.ColorBlend(0xff0000, 0x00ff00, u), FastBitmapOld.ColorBlend(0x0000ff, 0x000000, u), v);

      fastBitmap.MappedTriangle(50, 50, 100, 400, 150, 500, 0, 0, 0, 1, 1, 1, map);
      fastBitmap.MappedTriangle(50, 50, 300, 100, 150, 500, 0, 0, 1, 0, 1, 1, map);

      fastBitmap.MappedTriangle(350, 50, 600, 100, 400, 400, 0, 0, 1, 0, 0, 1, map);
      fastBitmap.MappedTriangle(450, 500, 600, 100, 400, 400, 1, 1, 1, 0, 0, 1, map);

      fastBitmap.MappedQuad(650, 50, 900, 100, 750, 500, 700, 400, 0, 0, 1, 0, 1, 1, 0, 1, map);

      ShowPicture(fastBitmap.ToGDIBitmap(), "Simple Textured");
    }

    /// <summary>
    /// Draw textures with nearest and biliniear filters
    /// </summary>
    public static void ShowTextureMappedTextured()
    {
      var fastBitmap = new FastBitmapOld(1024, 768, 0xff252525);

      var texture = GetDemoTextureOld();
      double mulX = texture.width / 6;
      double ofsX = texture.width / 6 * 3;
      double mulY = texture.height / 2;
      double ofsY = texture.height / 2;

      Func<double, double, uint, uint> mapNearest = (u, v, dstColor) =>
      {
        int sx = (int)(u * mulX + ofsX);
        int sy = (int)(v * mulY + ofsY);

        if (sx < 0) sx = 0;
        if (sx >= texture.width) sx = texture.width - 1;
        if (sy < 0) sy = 0;
        if (sy >= texture.height) sy = texture.height - 1;

        uint txColor = texture.pixels[sx + sy * texture.width];

        return FastBitmapOld.ColorBlendFast(dstColor, txColor, txColor >> 24);
      };

      Func<double, double, uint, uint> mapLinear = (u, v, dstColor) =>
      {
        int sx = (int)((u * mulX + ofsX) * 256);
        int sy = (int)((v * mulY + ofsY) * 256);
        int fractX = sx & 0xff;
        int fractY = sy & 0xff;
        sx /= 256;
        sy /= 256;

        if (sx < 0) sx = 0;
        if (sx >= texture.width - 1) sx = texture.width - 2;
        if (sy < 0) sy = 0;
        if (sy >= texture.height - 1) sy = texture.height - 2;

        uint txColor1 = texture.pixels[sx + sy * texture.width];
        uint txColor2 = texture.pixels[sx + 1 + sy * texture.width];
        uint txColor3 = texture.pixels[sx + (sy + 1) * texture.width];
        uint txColor4 = texture.pixels[sx + 1 + (sy + 1) * texture.width];

        uint txColorTop = FastBitmapOld.ColorBlendAlphaFast(txColor1, txColor2, (uint)fractX);
        uint txColorBottom = FastBitmapOld.ColorBlendAlphaFast(txColor3, txColor4, (uint)fractX);
        uint txColor = FastBitmapOld.ColorBlendAlphaFast(txColorTop, txColorBottom, (uint)fractY);

        return FastBitmapOld.ColorBlendFast(dstColor, txColor, txColor >> 24);
      };

      fastBitmap.MappedQuad(-150, -50, 650, 50, 500, 750, 100, 800, 0, 0, 1, 0, 1, 1, 0, 1, mapNearest);

      fastBitmap.MappedQuad(350, -50, 1150, 50, 1000, 750, 600, 800, 0, 0, 1, 0, 1, 1, 0, 1, mapLinear);

      ShowPicture(fastBitmap.ToGDIBitmap(), "Nearest/Linear Textured");
    }

    /// <summary>
    /// Compare default (linear affine) and perspective corrected texture mapping
    /// </summary>
    public static void ShowTextureMappedTexturedPerspective()
    {
      var fastBitmap = new FastBitmapOld(1024, 768, 0xff252525);

      var texture = GetDemoTextureOld();
      double mulX = texture.width / 6;
      double ofsX = texture.width / 6 * 3;
      double mulY = texture.height / 2;
      double ofsY = texture.height / 2;

      Func<double, double, uint, uint> mapLinear = (u, v, dstColor) =>
      {
        int sx = (int)((u * mulX + ofsX) * 256);
        int sy = (int)((v * mulY + ofsY) * 256);
        int fractX = sx & 0xff;
        int fractY = sy & 0xff;
        sx /= 256;
        sy /= 256;

        if (sx < 0) sx = 0;
        if (sx >= texture.width - 1) sx = texture.width - 2;
        if (sy < 0) sy = 0;
        if (sy >= texture.height - 1) sy = texture.height - 2;

        uint txColor1 = texture.pixels[sx + sy * texture.width];
        uint txColor2 = texture.pixels[sx + 1 + sy * texture.width];
        uint txColor3 = texture.pixels[sx + (sy + 1) * texture.width];
        uint txColor4 = texture.pixels[sx + 1 + (sy + 1) * texture.width];

        uint txColorTop = FastBitmapOld.ColorBlendAlphaFast(txColor1, txColor2, (uint)fractX);
        uint txColorBottom = FastBitmapOld.ColorBlendAlphaFast(txColor3, txColor4, (uint)fractX);
        uint txColor = FastBitmapOld.ColorBlendAlphaFast(txColorTop, txColorBottom, (uint)fractY);

        return FastBitmapOld.ColorBlendFast(dstColor, txColor, txColor >> 24);
      };

      fastBitmap.MappedQuad(-150, -50, 650, 50, 500, 750, 100, 800, 0, 0, 1, 0, 1, 1, 0, 1, mapLinear);

      fastBitmap.MappedQuadPerspective(350, -50, 1150, 50, 1000, 750, 600, 800, 0, 0, 1, 0, 1, 1, 0, 1, mapLinear);

      ShowPicture(fastBitmap.ToGDIBitmap(), "Perspective corrected textured");
    }
  }
}
