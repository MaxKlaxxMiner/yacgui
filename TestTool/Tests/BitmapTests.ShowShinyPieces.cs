#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
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
    public static void ShowShinyPieces()
    {
      var fastBitmap = new FastBitmap(MainForm.DefaultChessPieces);
      fastBitmap.ConvertGreenPixelsToAlpha();

      var distMap = DistanceTransform.GenerateMap(fastBitmap.pixels.Select(x => (byte)(x >> 24)).ToArray(), fastBitmap.width, fastBitmap.height);
      for (int i = 0; i < distMap.Length; i++)
      {
        uint opacity = (uint)Math.Max(0, 255 - Math.Pow(distMap[i], 0.3) * 18);
        if (opacity == 0) continue; // too far
        fastBitmap.pixels[i] = FastBitmap.ColorBlendFast(0xffcc00, fastBitmap.pixels[i], fastBitmap.pixels[i] >> 24) & 0xffffff | opacity << 24;
      }

      ShowPicture(fastBitmap.ToGDIBitmap(), "Shiny Pieces", backgroundColor: Color.Black);
    }
  }
}
