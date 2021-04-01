#region # using *.*
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
    /// Create a demo picture with shiny pieces (with alpha channel)
    /// </summary>
    /// <returns>Demo picture as FastBitmap</returns>
    static FastBitmap GetDemoTexture()
    {
      var texture = new FastBitmap(MainForm.DefaultChessPieces);
      texture.ConvertGreenPixelsToAlpha();

      var distMap = DistanceTransform.GenerateMap(texture.pixels.Select(x => (byte)(x >> 24)).ToArray(), texture.width, texture.height);
      for (int i = 0; i < distMap.Length; i++)
      {
        uint opacity = (uint)Math.Max(0, 255 - Math.Pow(distMap[i], 0.3) * 18);
        if (opacity == 0) continue; // too far
        texture.pixels[i] = FastBitmap.ColorBlendFast(0xffcc00, texture.pixels[i], texture.pixels[i] >> 24) & 0xffffff | opacity << 24;
      }

      return texture;
    }

    /// <summary>
    /// Draw shiny pieces with alpha channel
    /// </summary>
    public static void ShowShinyPieces()
    {
      var fastBitmap = GetDemoTexture();

      ShowPicture(fastBitmap.ToGDIBitmapAlpha(), "Shiny Pieces", backgroundColor: Color.Black);
    }
  }
}
