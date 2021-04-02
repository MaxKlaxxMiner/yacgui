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
using FastBitmapLib.Extras;
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
    public static void ShowShadowRealtime()
    {
      int checkerSize = 125;

      var fastBitmap = new FastBitmapOld(checkerSize * 8, checkerSize * 8, 0xff252525);

      var texture = new FastBitmapOld(MainForm.DefaultChessPieces);
      texture.ConvertGreenPixelsToAlpha();
      texture = new FastBitmapOld(texture, texture.width / 6 * 3, texture.height / 2 * 1, texture.width / 6, texture.height / 2);
      texture = texture.GetResizedCanvas(64, 64, 64, 64);

      var distMap = DistanceTransform.GenerateMap(texture.pixels.Select(x => (byte)(x >> 24)).ToArray(), texture.width, texture.height);
      var shadow = new FastBitmapOld(texture.width, texture.height);

      var resultBitmap = fastBitmap.ToGDIBitmap();
      int countFpsTick = Environment.TickCount + 1000;
      int countFps = 0;
      ShowPicture(resultBitmap, runLoop: (form, pictureBox) =>
      {
        for (int cy = 0; cy < fastBitmap.height; cy += checkerSize)
        {
          for (int cx = 0; cx < fastBitmap.width; cx += checkerSize)
          {
            fastBitmap.FillRectangle(cx, cy, checkerSize, checkerSize, ((cx ^ cy) & 1) == 0 ? 0xfffcd29c : 0xffa76124);
          }
        }

        double time = Stopwatch.GetTimestamp() * 10000.0 / Stopwatch.Frequency % 36000.0 * 0.1;
        int distMul = (int)((Math.Sin(time * 2 * Math.PI / 1800.0) + 1) * 1500.0 + 1000.0);

        for (int i = 0; i < shadow.pixels.Length; i++)
        {
          uint opacity = (uint)Math.Max(0, 255 - (distMap[i] * distMul >> 16));
          shadow.pixels[i] = (opacity * 3 >> 2) << 24;
        }

        fastBitmap.DrawBitmapAlpha(texture, fastBitmap.width / 4 - texture.width / 2, fastBitmap.height / 4 - texture.height / 2);
        fastBitmap.DrawBitmapAlpha(shadow, fastBitmap.width / 4 * 3 - shadow.width / 2, fastBitmap.height / 4 - shadow.height / 2);

        fastBitmap.DrawBitmapAlpha(shadow, fastBitmap.width / 2 - shadow.width / 2, fastBitmap.height / 4 * 3 - shadow.height / 2);
        fastBitmap.DrawBitmapAlpha(texture, fastBitmap.width / 2 - shadow.width / 2, fastBitmap.height / 4 * 3 - shadow.height / 2);

        fastBitmap.CopyToGDIBitmap(resultBitmap);
        pictureBox.Invalidate();
        countFps++;
        if (Environment.TickCount > countFpsTick)
        {
          form.Text = "Draw Realtime Shadows (" + countFps + " fps)";
          countFpsTick += 1000;
          countFps = 0;
        }
      });
    }
  }
}
