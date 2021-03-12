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
  public class BitmapTests : ConsoleExtras
  {
    /// <summary>
    /// Check the basics of the <see cref="FastBitmap"/> class
    /// </summary>
    public static void TestFastBitmap()
    {
      var bitmap = new Bitmap(10, 5, PixelFormat.Format32bppArgb);
      bitmap.SetPixel(0, 0, Color.Blue);  // set blue pixel (0xff0000ff) at the beginning of the image (top left)
      bitmap.SetPixel(1, 0, Color.Green); // set green pixel (0xff00ff00) in the next column
      bitmap.SetPixel(0, 1, Color.Red);   // set red pixel (0xffff0000) at the beginning of the next line

      var fastBitmap = new FastBitmap(bitmap);
      Debug.Assert(fastBitmap.width == 10 && fastBitmap.height == 5);
      Debug.Assert(fastBitmap.GetPixel(0, 0) == (uint)Color.Blue.ToArgb());
      Debug.Assert(fastBitmap.GetPixel(1, 0) == (uint)Color.Green.ToArgb());
      Debug.Assert(fastBitmap.GetPixel(0, 1) == (uint)Color.Red.ToArgb());

      fastBitmap.SetPixel(9, 4, 0xff0066ff); // Places a light blue pixel in the bottom right corner of the image

      var newBitmap = fastBitmap.ToGDIBitmap();
      Debug.Assert(newBitmap.Width == 10 && newBitmap.Height == 5);
      Debug.Assert((uint)newBitmap.GetPixel(9, 4).ToArgb() == 0xff0066ff);

      bitmap.Dispose();
      newBitmap.Dispose();
    }

    /// <summary>
    /// Checks the transparent effects of images
    /// </summary>
    /// <param name="showPicture">Optional: show test picture</param>
    public static void TestAlphaMask(bool showPicture = false)
    {
      var bitmapPieces = MainForm.DefaultChessPieces;
      Debug.Assert((uint)bitmapPieces.GetPixel(0, 0).ToArgb() == 0xff00ff00); // A green pixel is expected at the top left

      var fastBitmap = new FastBitmap(bitmapPieces);
      fastBitmap.ConvertGreenPixelsToAlpha();

      var newBitmap = fastBitmap.ToGDIBitmap();
      Debug.Assert((uint)newBitmap.GetPixel(0, 0).ToArgb() == 0x00000000); // black pixel with expected alpha = 0

      if (showPicture) ShowPicture(newBitmap, "Opacity-Test", (form, pos) => form.Text = pos + " - Opacity: " + (newBitmap.GetPixel(pos.X, pos.Y).A * 100 / 255) + " %", Color.FromArgb(0x0066ff - 16777216));

      newBitmap.Dispose();
    }

    /// <summary>
    /// Test Subpixel-Versions
    /// </summary>
    public static void TestSubPixel()
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

    static void TestPixelDistance()
    {
      var tmpBitmap = new Bitmap(480, 360, PixelFormat.Format32bppRgb);
      var g = Graphics.FromImage(tmpBitmap);
      g.SmoothingMode = SmoothingMode.HighQuality;
      //g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(120, 220), new Point(160, 260), new Point(320, 260), new Point(360, 220), new Point(360, 240), new Point(320, 280), new Point(160, 280), new Point(120, 240) });
      //g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(180, 220), new Point(220, 200), new Point(260, 200), new Point(300, 220), new Point(240, 140) });
      //g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(180, 80), new Point(200, 80), new Point(200, 120) });
      //g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(280, 80), new Point(280, 120), new Point(300, 80) });
      //g.FillEllipse(new SolidBrush(Color.White), 140, 120, 40, 40);
      //g.FillEllipse(new SolidBrush(Color.White), 300, 120, 40, 40);

      g.FillPolygon(new SolidBrush(Color.White), new[] { new Point(100, 20), new Point(103, 340), new Point(53, 340), new Point(50, 20) });

      var fastBitmap = new FastBitmap(tmpBitmap);

      //bool[] bits = new bool[fastBitmap.width * fastBitmap.height];
      //for (int i = 0; i < bits.Length; i++) bits[i] = (fastBitmap.pixels[i] & 0xffffff) > 0x333333;

      byte[] bits = new byte[fastBitmap.width * fastBitmap.height];
      for (int i = 0; i < bits.Length; i++) bits[i] = (byte)fastBitmap.pixels[i];
      //for (int i = 0; i < bits.Length; i++) bits[i] = (byte)fastBitmap.pixels[i] == 255 ? byte.MaxValue : byte.MinValue;

      //var distances = DistanceTransform.GenerateMapSlowReference(bits, fastBitmap.width, fastBitmap.height);
      var distances = DistanceTransform.GenerateMap(bits, fastBitmap.width, fastBitmap.height);

      for (int y = 0; y < fastBitmap.height; y++)
      {
        for (int x = 0; x < fastBitmap.width; x++)
        {
          if (bits[x + y * fastBitmap.width] > 0)
          {
            fastBitmap.SetPixel(x, y, 0xff808080);
            continue;
          }
          uint val = 255 - (uint)Math.Max(0, 255 - distances[x + y * fastBitmap.width] / 2048 * 16);
          //uint val = 255 - (uint)Math.Max(0, 255 - distances[x + y * fastBitmap.width] / 128);
          fastBitmap.SetPixel(x, y, 0xff000000 | val << 16 | val << 8 | val);
        }
      }

      tmpBitmap = fastBitmap.ToGDIBitmap();

      ShowPicture(tmpBitmap);
    }

    static void TestPixelDistance2()
    {
      int width = 512;
      int height = 200;
      var testBytes = new byte[width * height];
      for (int y = 0; y < 2; y++)
      {
        for (int x = 0; x < width; x++)
        {
          testBytes[x + y * width] = 255;
        }
      }
      for (int x = 0; x < 256; x++)
      {
        testBytes[x + 2 * width] = (byte)x;
        testBytes[x + 256 + 2 * width] = 255;
        testBytes[x + 256 + 3 * width] = (byte)x;
      }

      var distances = DistanceTransform.GenerateMap(testBytes, width, height);

      for (int i = 1024; i < 1024 + 256; i++) Console.Write(distances[i] + ","); // 256-0
    }

    /// <summary>
    /// Run Bitmap-Tests
    /// </summary>
    public static void Run()
    {
      //TestFastBitmap();

      //TestAlphaMask();

      //TestSubPixel();

      //TestPixelDistance();
      TestPixelDistance2();
    }
  }
}
