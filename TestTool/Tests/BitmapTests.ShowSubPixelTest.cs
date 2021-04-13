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
    /// Test/compare resizing and subpixel optimized versions
    /// </summary>
    public static void ShowSubPixelTest()
    {
      var fastBitmap = new FastBitmap(MainForm.DefaultChessPieces);
      fastBitmap.ConvertGreenPixelsToAlpha();

      int width = fastBitmap.width;
      int height = fastBitmap.height;

      var testBaseBitmap = new Bitmap(width, height * 4, PixelFormat.Format32bppArgb);
      var gTest = Graphics.FromImage(testBaseBitmap);
      gTest.InterpolationMode = InterpolationMode.HighQualityBicubic;
      gTest.CompositingQuality = CompositingQuality.HighQuality;
      gTest.SmoothingMode = SmoothingMode.HighQuality;
      gTest.Clear(Color.DarkSeaGreen);
      gTest.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);
      gTest.DrawImage(fastBitmap.ToGDIBitmap(), 0, 0, width, height);
      gTest.DrawImage(fastBitmap.ToGDIBitmap(), 0, height, width, height);
      gTest.FillRectangle(new SolidBrush(Color.LightYellow), 0, height * 2, width / 2, height);
      gTest.FillRectangle(new SolidBrush(Color.Black), width / 2, height * 2, width / 2, height);

      var poly = new List<PointF>();

      for (int i = 0; i <= 180; i += 30)
      {
        poly.Add(new PointF((float)Math.Cos(Math.PI / 180.0 * i), (float)-Math.Sin(Math.PI / 180.0 * i) * 3.0f));
        if (i < 180) poly.Add(new PointF((float)Math.Cos(Math.PI / 180.0 * (i + 15)) * 0.5f, (float)-Math.Sin(Math.PI / 180.0 * (i + 15)) * 0.5f));
      }

      gTest.DrawPolygon(new Pen(Color.Black, 5), poly.Select(p => new PointF(p.X * 120 + 180, p.Y * 120 + height * 2 + 400)).ToArray());
      gTest.FillPolygon(new SolidBrush(Color.Black), poly.Select(p => new PointF(p.X * 120 + 480, p.Y * 120 + height * 2 + 400)).ToArray());

      gTest.DrawPolygon(new Pen(Color.White, 5), poly.Select(p => new PointF(p.X * 120 + 180 + width / 2, p.Y * 120 + height * 2 + 400)).ToArray());
      gTest.FillPolygon(new SolidBrush(Color.White), poly.Select(p => new PointF(p.X * 120 + 480 + width / 2, p.Y * 120 + height * 2 + 400)).ToArray());


      int outWidth = testBaseBitmap.Width / 6;
      int outHeight = testBaseBitmap.Height / 6;

      var outputBitmap = new Bitmap(outWidth * 8, outHeight, PixelFormat.Format32bppArgb);
      var gOut = Graphics.FromImage(outputBitmap);
      gOut.InterpolationMode = InterpolationMode.HighQualityBicubic;
      gOut.CompositingQuality = CompositingQuality.HighQuality;
      gOut.SmoothingMode = SmoothingMode.HighQuality;
      gOut.InterpolationMode = InterpolationMode.HighQualityBilinear;

      Action<string> InfoText = txt =>
      {
        gTest.FillRectangle(new SolidBrush(Color.LightGray), 0, height * 3, width, height);
        var font = new Font("Helvetica", 50.0f);
        txt = "|||||| ////// iiiiiiii WWW\r\n" + txt;
        var measure = gTest.MeasureString(txt, font);
        gTest.DrawString(txt, font, new SolidBrush(Color.Black), (width - measure.Width) / 2, height * 3 + (height - measure.Height) / 2);
      };

      InfoText("clear - default");
      gOut.DrawImage(testBaseBitmap, 0, 0, outWidth, outHeight);
      InfoText("clear - fastest");
      gOut.DrawImageUnscaled(new FastBitmap(testBaseBitmap).GetResizedSimple(outWidth, outHeight).ToGDIBitmap(), outWidth, 0);
      InfoText("clear - high");
      gOut.DrawImageUnscaled(new FastBitmap(testBaseBitmap).GetResizedHigh(outWidth, outHeight).ToGDIBitmap(), outWidth * 2, 0);
      InfoText("clear - clear 1");
      gOut.DrawImageUnscaled(new FastBitmap(testBaseBitmap).GetResizedClear(outWidth, outHeight, 1).ToGDIBitmap(), outWidth * 3, 0);
      InfoText("clear - clear 2");
      gOut.DrawImageUnscaled(new FastBitmap(testBaseBitmap).GetResizedClear(outWidth, outHeight, 2).ToGDIBitmap(), outWidth * 4, 0);
      InfoText("clear - clear 3");
      gOut.DrawImageUnscaled(new FastBitmap(testBaseBitmap).GetResizedClear(outWidth, outHeight, 3).ToGDIBitmap(), outWidth * 5, 0);
      InfoText("clear - clear 4");
      gOut.DrawImageUnscaled(new FastBitmap(testBaseBitmap).GetResizedClear(outWidth, outHeight, 4).ToGDIBitmap(), outWidth * 6, 0);
      InfoText("clear - clear 5");
      gOut.DrawImageUnscaled(new FastBitmap(testBaseBitmap).GetResizedClear(outWidth, outHeight, 5).ToGDIBitmap(), outWidth * 7, 0);

      ShowPicture(outputBitmap, "SubPixel-Test");
    }
  }
}
