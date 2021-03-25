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
    /// <summary>
    /// Performance test with fps counter: Full bitmap update
    /// </summary>
    public static void ShowDrawPerformanceNaive()
    {
      var fastBitmap = new FastBitmap(1900, 1000, 0xff252525);

      var texture = GetDemoTexture();

      var resultBitmap = fastBitmap.ToGDIBitmap();

      int countFpsTick = Environment.TickCount + 1000;
      int countFps = 0;

      ShowPicture(resultBitmap, runLoop: (form, pictureBox) =>
      {
        double time = Stopwatch.GetTimestamp() * 10000.0 / Stopwatch.Frequency % 36000.0 * 0.1;
        int cx = (int)(Math.Sin(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 50)) + fastBitmap.width / 2;
        int cy = (int)(-Math.Cos(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 50)) + fastBitmap.height / 2;

        fastBitmap.Clear(0xff252525);
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

    /// <summary>
    /// Performance test with fps counter: Viewport optimized update
    /// </summary>
    public static void ShowDrawPerformanceOptimized()
    {
      var fastBitmap = new FastBitmap(1900, 1000, 0xff252525);

      var texture = GetDemoTexture();

      var resultBitmap = fastBitmap.ToGDIBitmap();

      int countFpsTick = Environment.TickCount + 1000;
      int countFps = 0;

      var viewPort = new DrawViewPort();
      var lastRect = new Rectangle(0, 0, 0, 0);

      ShowPicture(resultBitmap, runLoop: (form, pictureBox) =>
      {
        viewPort.Reset();

        if (lastRect.X < 0) { lastRect.Width += lastRect.X; lastRect.X = 0; }
        if (lastRect.Y < 0) { lastRect.Height += lastRect.Y; lastRect.Y = 0; }
        if (lastRect.X + lastRect.Width > fastBitmap.width) { lastRect.Width = fastBitmap.width - lastRect.X; }
        if (lastRect.Y + lastRect.Height > fastBitmap.height) { lastRect.Height = fastBitmap.height - lastRect.Y; }
        for (int y = 0; y < lastRect.Height; y++)
        {
          for (int x = 0; x < lastRect.Width; x++)
          {
            fastBitmap.pixels[x + lastRect.X + (y + lastRect.Y) * fastBitmap.width] = 0xff252525;
          }
        }
        viewPort.Expand(lastRect);

        double time = Stopwatch.GetTimestamp() * 10000.0 / Stopwatch.Frequency % 36000.0 * 0.1;
        int cx = (int)(Math.Sin(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 50)) + fastBitmap.width / 2;
        int cy = (int)(-Math.Cos(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 50)) + fastBitmap.height / 2;
        cx -= texture.width / 6 / 2;
        cy -= texture.height / 2 / 2;
        int sw = texture.width / 6;
        int sh = texture.height / 2;

        fastBitmap.DrawBitmapAlpha(texture,
          cx, cy, // x, y
          texture.width / 6 * 3, texture.height / 2 * 1, // spriteX, spriteY
          sw, sh // spriteWidth, spriteHeight
        );
        lastRect = new Rectangle(cx, cy, sw, sh);
        viewPort.Expand(cx, cy, sw, sh);

        var rect = fastBitmap.CopyToGDIBitmap(resultBitmap, viewPort);
        pictureBox.Invalidate(rect);

        countFps++;
        if (Environment.TickCount > countFpsTick)
        {
          form.Text = "Draw Performance Optimized (" + countFps + " fps)";
          countFpsTick += 1000;
          countFps = 0;
        }
      });
    }

    /// <summary>
    /// Performance test with fps counter and checkered background: Full bitmap update
    /// </summary>
    public static void ShowCheckerPerformanceNaive()
    {
      var fastBitmap = new FastBitmap(1000, 1000, 0xff252525);

      var texture = GetDemoTexture();

      var resultBitmap = fastBitmap.ToGDIBitmap();

      int countFpsTick = Environment.TickCount + 1000;
      int countFps = 0;

      ShowPicture(resultBitmap, runLoop: (form, pictureBox) =>
      {
        double time = Stopwatch.GetTimestamp() * 10000.0 / Stopwatch.Frequency % 72000.0 * 0.1;
        int cx = (int)(Math.Sin(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 100)) + fastBitmap.width / 2;
        int cy = (int)(-Math.Cos(time * 0.5 * Math.PI / 1800.0) * (fastBitmap.height / 2 - 100)) + fastBitmap.height / 2;

        for (int checkerY = 0; checkerY < 1000; checkerY += 125)
        {
          for (int checkerX = 0; checkerX < 1000; checkerX += 125)
          {
            fastBitmap.FillRectangle(checkerX, checkerY, 125, 125, ((checkerX ^ checkerY) & 1) == 0 ? 0xfffcd29c : 0xffa76124);
          }
        }
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
          form.Text = "Checker Performance Naive (" + countFps + " fps)";
          countFpsTick += 1000;
          countFps = 0;
        }
      });
    }

    /// <summary>
    /// Performance test with fps counter and checkered background: Viewport optimized update
    /// </summary>
    public static void ShowCheckerPerformanceOptimized()
    {
      var fastBitmap = new FastBitmap(1000, 1000, 0xff252525);

      var texture = GetDemoTexture();

      var resultBitmap = fastBitmap.ToGDIBitmap();

      int countFpsTick = Environment.TickCount + 1000;
      int countFps = 0;

      var viewPort = new DrawViewPort();
      var lastRect = new Rectangle(0, 0, 1000, 1000);

      ShowPicture(resultBitmap, runLoop: (form, pictureBox) =>
      {
        viewPort.Reset();

        if (lastRect.X < 0) { lastRect.Width += lastRect.X; lastRect.X = 0; }
        if (lastRect.Y < 0) { lastRect.Height += lastRect.Y; lastRect.Y = 0; }
        if (lastRect.X + lastRect.Width > fastBitmap.width) { lastRect.Width = fastBitmap.width - lastRect.X; }
        if (lastRect.Y + lastRect.Height > fastBitmap.height) { lastRect.Height = fastBitmap.height - lastRect.Y; }
        for (int checkerY = 0; checkerY < 1000; checkerY += 125)
        {
          for (int checkerX = 0; checkerX < 1000; checkerX += 125)
          {
            if (checkerX + 125 < lastRect.X || checkerY + 125 < lastRect.Y || checkerX >= lastRect.Right || checkerY >= lastRect.Bottom) continue;
            fastBitmap.FillRectangle(checkerX, checkerY, 125, 125, ((checkerX ^ checkerY) & 1) == 0 ? 0xfffcd29c : 0xffa76124);
          }
        }
        viewPort.Expand(lastRect);

        double time = Stopwatch.GetTimestamp() * 10000.0 / Stopwatch.Frequency % 72000.0 * 0.1;
        int cx = (int)(Math.Sin(time * Math.PI / 1800.0) * (fastBitmap.height / 2 - 100)) + fastBitmap.width / 2;
        int cy = (int)(-Math.Cos(time * 0.5 * Math.PI / 1800.0) * (fastBitmap.height / 2 - 100)) + fastBitmap.height / 2;
        cx -= texture.width / 6 / 2;
        cy -= texture.height / 2 / 2;
        int sw = texture.width / 6;
        int sh = texture.height / 2;

        fastBitmap.DrawBitmapAlpha(texture,
          cx, cy, // x, y
          texture.width / 6 * 3, texture.height / 2 * 1, // spriteX, spriteY
          sw, sh // spriteWidth, spriteHeight
        );
        lastRect = new Rectangle(cx, cy, sw, sh);
        viewPort.Expand(cx, cy, sw, sh);

        var rect = fastBitmap.CopyToGDIBitmap(resultBitmap, viewPort);
        pictureBox.Invalidate(rect);

        countFps++;
        if (Environment.TickCount > countFpsTick)
        {
          form.Text = "Checker Performance Optimized (" + countFps + " fps)";
          countFpsTick += 1000;
          countFps = 0;
        }
      });
    }
  }
}
