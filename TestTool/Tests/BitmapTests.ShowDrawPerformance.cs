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
    public static void ShowDrawPerformanceNaive()
    {
      var fastBitmap = new FastBitmap(1900, 1000, 0xff252525);

      var texture = GetDemoTexture();

      var resultBitmap = fastBitmap.ToGDIBitmap();

      int countFpsTick = Environment.TickCount + 1000;
      int countFps = 0;

      ShowPicture(resultBitmap, runLoop: (form, pictureBox) =>
      {
        int time = Environment.TickCount % 3600;
        int cx = (int)(Math.Sin(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 50)) + fastBitmap.width / 2;
        int cy = (int)(-Math.Cos(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 50)) + fastBitmap.height / 2;

        fastBitmap.Clear();
        fastBitmap.DrawBitmapAlpha(texture,
          cx - texture.width / 6 / 2, cy - texture.height / 2 / 2, // x, y
          texture.width / 6 * 3, texture.height / 2 * 1, // spriteX, spriteY
          texture.width / 6, texture.height / 2 // spriteWidth, spriteHeight
        );
        fastBitmap.CopyToGDIBitmap(resultBitmap);
        pictureBox.Invalidate();

        countFps++;
        if (Environment.TickCount > countFpsTick)
        {
          form.Text = "Draw Performance Naive (" + countFps + " fps)";
          countFpsTick += 1000;
          countFps = 0;
        }
      });
    }
  }
}
