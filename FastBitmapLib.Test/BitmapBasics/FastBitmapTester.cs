using System;
using System.Collections.Generic;
using System.Drawing;
using FastBitmapLib.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastBitmapLib.Test.BitmapBasics
{
  /// <summary>
  /// Test-Class
  /// </summary>
  public abstract unsafe class FastBitmapTester
  {
    /// <summary>
    /// Create Testbitmap
    /// </summary>
    /// <param name="width">Width in Pixels</param>
    /// <param name="height">Height in Pixels</param>
    /// <param name="backgroundColor">Initial Background-Color</param>
    /// <returns>Created Bitmap</returns>
    public abstract IFastBitmap CreateBitmap(int width, int height, Color backgroundColor);

    /// <summary>
    /// Run internal Tests
    /// </summary>
    /// <param name="width">Width of the Test-Picture</param>
    /// <param name="height">Height of the Test-Picture</param>
    /// <param name="backgroundColor">Initial Background-Color of the Test-Picture</param>
    /// <param name="name">Name of the Test</param>
    /// <param name="actions">Test-Actions</param>
    void InternalTest(int width, int height, Color backgroundColor, string name, params Func<IFastBitmap, string>[] actions)
    {
      var testBitmap = CreateBitmap(width, height, backgroundColor);
      Assert.IsNotNull(testBitmap);

      bool is32Bit = testBitmap is IFastBitmap32;

      var refBitmap = is32Bit
        ? (IFastBitmap)new ReferenceBitmap(width, height, Color32.From(backgroundColor))
        : new ReferenceBitmap64(width, height, Color64.From(backgroundColor));

      if (actions.Length > 0)
      {
        for (int i = 0; i < actions.Length; i++)
        {
          string exname = actions[i](refBitmap);
          string exname2 = actions[i](testBitmap);
          if (exname != exname2) throw new Exception("different names?!");
          if (exname != "") exname = name + " / " + exname; else exname = name;
          Ex.AreEqual(refBitmap, testBitmap, exname + " [" + (i + 1) + " / " + (actions.Length + 1) + "]");
        }
      }
      else
      {
        Ex.AreEqual(refBitmap, testBitmap, name + " [" + (actions.Length + 1) + " / " + (actions.Length + 1) + "]");
      }

      refBitmap.Dispose();
      testBitmap.Dispose();
    }

    #region # // --- Helper Methods ---
    public static uint Get32(Random rnd)
    {
      var buf = new byte[4];
      rnd.NextBytes(buf);
      return BitConverter.ToUInt32(buf, 0);
    }

    public static ulong Get64(Random rnd)
    {
      var buf = new byte[8];
      rnd.NextBytes(buf);
      return BitConverter.ToUInt64(buf, 0);
    }

    static void Fill<T>(T[] array, T value)
    {
      for (int i = 0; i < array.Length; i++)
      {
        array[i] = value;
      }
    }
    #endregion

    #region # protected void Test01_Constructor()
    protected void Test01_Constructor()
    {
      InternalTest(320, 240, Color.FromArgb(0), "Constructor");
    }
    #endregion

    #region # protected void Test02_Constructor_Background()
    protected void Test02_Constructor_Background()
    {
      InternalTest(40, 30, Color.Red, "Constructor + Background-Red");
      InternalTest(41, 31, Color.Green, "Constructor + Background-Green");
      InternalTest(42, 32, Color.Blue, "Constructor + Background-Blue");
      InternalTest(43, 33, Color.Black, "Constructor + Background-Black");
    }
    #endregion

    #region # protected void Test03_SetPixel32()
    protected void Test03_SetPixel32()
    {
      InternalTest(160, 90, Color.FromArgb(0), "SetPixel32",
        b =>
        {
          b.SetPixelUnsafe(0, 0, Color32.From(Color.Red));
          b.SetPixelUnsafe(b.width - 1, 0, Color32.From(Color.Green));
          b.SetPixelUnsafe(0, b.height - 1, Color32.From(Color.Blue));
          b.SetPixelUnsafe(b.width - 1, b.height - 1, Color32.From(Color.White));
          return "SetPixelUnsafe - corners";
        },
        b =>
        {
          var rnd = new Random(12345 + 0301);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd), false));
          }
          return "SetPixelUnsafe - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0302);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd)));
          }
          return "SetPixelUnsafe - random+alpha";
        },
        b =>
        {
          var rnd = new Random(12345 + 0303);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixel(rnd.Next(b.width + 100) - 50, rnd.Next(b.height + 100) - 50, Color32.From(Get32(rnd), false));
          }
          return "SetPixel - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0304);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixel(rnd.Next(b.width + 100) - 50, rnd.Next(b.height + 100) - 50, Color32.From(Get32(rnd)));
          }
          return "SetPixel - random+alpha";
        }
      );
    }
    #endregion
    #region # protected void Test03_SetPixel64()
    protected void Test03_SetPixel64()
    {
      InternalTest(160, 90, Color.FromArgb(0), "SetPixel64",
        b =>
        {
          b.SetPixelUnsafe(0, 0, Color64.From(Color.Red));
          b.SetPixelUnsafe(b.width - 1, 0, Color64.From(Color.Green));
          b.SetPixelUnsafe(0, b.height - 1, Color64.From(Color.Blue));
          b.SetPixelUnsafe(b.width - 1, b.height - 1, Color64.From(Color.White));
          return "SetPixelUnsafe - corners";
        },
        b =>
        {
          var rnd = new Random(12345 + 0301);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color64.From(Get64(rnd), false));
          }
          return "SetPixelUnsafe - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0302);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color64.From(Get64(rnd)));
          }
          return "SetPixelUnsafe - random+alpha";
        },
        b =>
        {
          var rnd = new Random(12345 + 0303);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixel(rnd.Next(b.width + 100) - 50, rnd.Next(b.height + 100) - 50, Color64.From(Get64(rnd), false));
          }
          return "SetPixel - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0304);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixel(rnd.Next(b.width + 100) - 50, rnd.Next(b.height + 100) - 50, Color64.From(Get64(rnd)));
          }
          return "SetPixel - random+alpha";
        }
      );
    }
    #endregion

    #region # protected void Test04_GetPixel32()
    protected void Test04_GetPixel32()
    {
      InternalTest(160, 90, Color.Black, "GetPixel32",
        b =>
        {
          b.SetPixelUnsafe(0, 0, Color32.From(Color.Red));
          b.SetPixelUnsafe(b.width - 1, 0, Color32.From(Color.Green));
          b.SetPixelUnsafe(0, b.height - 1, Color32.From(Color.Blue));
          b.SetPixelUnsafe(b.width - 1, b.height - 1, Color32.From(Color.White));

          Assert.AreEqual(Color32.From(Color.Red), b.GetPixelUnsafe32(0, 0));
          Assert.AreEqual(Color32.From(Color.Green), b.GetPixelUnsafe32(b.width - 1, 0));
          Assert.AreEqual(Color32.From(Color.Blue), b.GetPixelUnsafe32(0, b.height - 1));
          Assert.AreEqual(Color32.From(Color.White), b.GetPixelUnsafe32(b.width - 1, b.height - 1));

          return "GetPixelUnsafe - corners";
        },
        b =>
        {
          var rnd = new Random(12345 + 0401);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            b.SetPixelUnsafe(posX, posY, Color32.From(Get32(rnd), false));
            knownPos[posX + posY * b.width] = i;
          }

          rnd = new Random(12345 + 0401);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            uint expectedColor = Color32.From(Get32(rnd), false);
            uint actualColor = b.GetPixelUnsafe32(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              Assert.AreEqual(expectedColor, actualColor);
            }
          }
          return "GetPixelUnsafe - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0402);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            b.SetPixelUnsafe(posX, posY, Color32.From(Get32(rnd)));
            knownPos[posX + posY * b.width] = i;
          }

          rnd = new Random(12345 + 0402);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            uint expectedColor = Color32.From(Get32(rnd));
            uint actualColor = b.GetPixelUnsafe32(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              Assert.AreEqual(expectedColor, actualColor);
            }
          }

          return "GetPixelUnsafe - random+alpha";
        },
        b =>
        {
          var rnd = new Random(12345 + 0403);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            b.SetPixel(posX, posY, Color32.From(Get32(rnd), false));
            knownPos[posX + posY * b.width] = i;
          }

          rnd = new Random(12345 + 0403);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            uint expectedColor = Color32.From(Get32(rnd), false);
            if (posX < 0 || posX >= b.width || posY < 0 || posY >= b.height) expectedColor = Color32.From(Color.Black); // expected background-color if outbound
            uint actualColor = b.GetPixel32(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              Assert.AreEqual(expectedColor, actualColor);
            }
          }

          return "GetPixel - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0404);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            b.SetPixel(posX, posY, Color32.From(Get32(rnd)));
            knownPos[posX + posY * b.width] = i;
          }

          rnd = new Random(12345 + 0404);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            uint expectedColor = Color32.From(Get32(rnd));
            if (posX < 0 || posX >= b.width || posY < 0 || posY >= b.height) expectedColor = Color32.From(Color.Black); // expected background-color if outbound
            uint actualColor = b.GetPixel32(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              Assert.AreEqual(expectedColor, actualColor);
            }
          }

          return "GetPixel - random+alpha";
        }
      );
    }
    #endregion
    #region # protected void Test04_GetPixel64()
    protected void Test04_GetPixel64()
    {
      InternalTest(160, 90, Color.Black, "GetPixel64",
        b =>
        {
          b.SetPixelUnsafe(0, 0, Color64.From(Color.Red));
          b.SetPixelUnsafe(b.width - 1, 0, Color64.From(Color.Green));
          b.SetPixelUnsafe(0, b.height - 1, Color64.From(Color.Blue));
          b.SetPixelUnsafe(b.width - 1, b.height - 1, Color64.From(Color.White));

          Assert.AreEqual(Color64.From(Color.Red), b.GetPixelUnsafe64(0, 0));
          Assert.AreEqual(Color64.From(Color.Green), b.GetPixelUnsafe64(b.width - 1, 0));
          Assert.AreEqual(Color64.From(Color.Blue), b.GetPixelUnsafe64(0, b.height - 1));
          Assert.AreEqual(Color64.From(Color.White), b.GetPixelUnsafe64(b.width - 1, b.height - 1));

          return "GetPixelUnsafe - corners";
        },
        b =>
        {
          var rnd = new Random(12345 + 0401);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            b.SetPixelUnsafe(posX, posY, Color64.From(Get64(rnd), false));
            knownPos[posX + posY * b.width] = i;
          }

          bool is32Bit = b is IFastBitmap32;
          rnd = new Random(12345 + 0401);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            ulong expectedColor = Color64.From(Get64(rnd), false);
            ulong actualColor = b.GetPixelUnsafe64(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              if (is32Bit) expectedColor = Color64.From(Color32.From(expectedColor));
              Assert.AreEqual(expectedColor, actualColor);
            }
          }
          return "GetPixelUnsafe - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0402);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            b.SetPixelUnsafe(posX, posY, Color64.From(Get64(rnd)));
            knownPos[posX + posY * b.width] = i;
          }

          bool is32Bit = b is IFastBitmap32;
          rnd = new Random(12345 + 0402);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            ulong expectedColor = Color64.From(Get64(rnd));
            ulong actualColor = b.GetPixelUnsafe64(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              if (is32Bit) expectedColor = Color64.From(Color32.From(expectedColor));
              Assert.AreEqual(expectedColor, actualColor);
            }
          }

          return "GetPixelUnsafe - random+alpha";
        },
        b =>
        {
          var rnd = new Random(12345 + 0403);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            b.SetPixel(posX, posY, Color64.From(Get64(rnd), false));
            knownPos[posX + posY * b.width] = i;
          }

          bool is32Bit = b is IFastBitmap32;
          rnd = new Random(12345 + 0403);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            ulong expectedColor = Color64.From(Get64(rnd), false);
            if (posX < 0 || posX >= b.width || posY < 0 || posY >= b.height) expectedColor = Color64.From(Color.Black); // expected background-color if outbound
            ulong actualColor = b.GetPixel64(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              if (is32Bit) expectedColor = Color64.From(Color32.From(expectedColor));
              Assert.AreEqual(expectedColor, actualColor);
            }
          }

          return "GetPixel - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0404);
          var knownPos = new Dictionary<int, int>();
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            b.SetPixel(posX, posY, Color64.From(Get64(rnd)));
            knownPos[posX + posY * b.width] = i;
          }

          bool is32Bit = b is IFastBitmap32;
          rnd = new Random(12345 + 0404);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            ulong expectedColor = Color64.From(Get64(rnd));
            if (posX < 0 || posX >= b.width || posY < 0 || posY >= b.height) expectedColor = Color64.From(Color.Black); // expected background-color if outbound
            ulong actualColor = b.GetPixel64(posX, posY);
            if (knownPos[posX + posY * b.width] == i)
            {
              if (is32Bit) expectedColor = Color64.From(Color32.From(expectedColor));
              Assert.AreEqual(expectedColor, actualColor);
            }
          }

          return "GetPixel - random+alpha";
        }
      );
    }
    #endregion

    #region # protected void Test05_FillScanline32()
    protected void Test05_FillScanline32()
    {
      InternalTest(160, 90, Color.Black, "FillScanline32",
        b =>
        {
          b.FillScanlineUnsafe(0, 0, b.width, Color32.From(Color.Red));
          b.FillScanlineUnsafe(0, 1, b.width, Color32.From(Color.Red));
          b.FillScanlineUnsafe(b.width / 4, 0, b.width / 2, Color32.From(Color.Green));
          b.FillScanlineUnsafe(b.width / 3, b.height - 2, b.width / 3, Color32.From(Color.Blue));
          b.FillScanlineUnsafe(b.width / 3, b.height - 1, b.width / 3, Color32.From(Color.Blue));
          b.FillScanlineUnsafe(b.width / 2, b.height - 1, b.width / 2, Color32.From(Color.White));
          return "FillScanlineUnsafe32 - simple";
        },
        b =>
        {
          for (int w = 1; w < b.width; w++) b.FillScanlineUnsafe(0, 3, w, Color32.From(w * 3, false));
          for (int w = 1; w < b.width; w += 2) b.FillScanlineUnsafe(0, 4, w, Color32.From(w * 5, false));
          for (int w = 1; w < b.width; w += 3) b.FillScanlineUnsafe(0, 5, w, Color32.From(w * 7, false));
          for (int w = 1; w < b.width; w += 4) b.FillScanlineUnsafe(0, 6, w, Color32.From(w * 11, false));
          for (int w = 1; w < b.width; w += 5) b.FillScanlineUnsafe(0, 7, w, Color32.From(w * 13, false));
          for (int w = b.width; w >= 1; w--) b.FillScanlineUnsafe(0, 8, w, Color32.From(w * 17, false));
          for (int w = b.width; w >= 1; w -= 2) b.FillScanlineUnsafe(0, 9, w, Color32.From(w * 19, false));
          for (int w = b.width; w >= 1; w -= 3) b.FillScanlineUnsafe(0, 10, w, Color32.From(w * 23, false));
          for (int w = b.width; w >= 1; w -= 4) b.FillScanlineUnsafe(0, 11, w, Color32.From(w * 29, false));
          for (int w = b.width; w >= 1; w -= 5) b.FillScanlineUnsafe(0, 12, w, Color32.From(w * 31, false));
          for (int x = 0; x < b.width - 1; x++) b.FillScanlineUnsafe(x, 13, b.width - x, Color32.From(x * 37, false));
          for (int x = 0; x < b.width - 1; x += 2) b.FillScanlineUnsafe(x, 14, b.width - x, Color32.From(x * 41, false));
          for (int x = 0; x < b.width - 1; x += 3) b.FillScanlineUnsafe(x, 15, b.width - x, Color32.From(x * 43, false));
          for (int x = 0; x < b.width - 1; x += 4) b.FillScanlineUnsafe(x, 16, b.width - x, Color32.From(x * 47, false));
          for (int x = 0; x < b.width - 1; x += 5) b.FillScanlineUnsafe(x, 17, b.width - x, Color32.From(x * 53, false));
          for (int x = b.width - 1; x >= 0; x--) b.FillScanlineUnsafe(x, 18, b.width - x, Color32.From(x * 59, false));
          for (int x = b.width - 1; x >= 0; x -= 2) b.FillScanlineUnsafe(x, 19, b.width - x, Color32.From(x * 61, false));
          for (int x = b.width - 1; x >= 0; x -= 3) b.FillScanlineUnsafe(x, 20, b.width - x, Color32.From(x * 67, false));
          for (int x = b.width - 1; x >= 0; x -= 4) b.FillScanlineUnsafe(x, 21, b.width - x, Color32.From(x * 71, false));
          for (int x = b.width - 1; x >= 0; x -= 5) b.FillScanlineUnsafe(x, 22, b.width - x, Color32.From(x * 73, false));
          for (int x = 0, w = b.width; w > 0; x++, w -= 2) b.FillScanlineUnsafe(x, 23, w, Color32.From(x * 79, false));
          for (int x = 0, w = b.width; w > 0; x += 2, w -= 4) b.FillScanlineUnsafe(x, 24, w, Color32.From(x * 83, false));
          for (int x = 0, w = b.width; w > 0; x += 3, w -= 6) b.FillScanlineUnsafe(x, 25, w, Color32.From(x * 89, false));
          for (int x = 0, w = b.width; w > 0; x += 4, w -= 8) b.FillScanlineUnsafe(x, 26, w, Color32.From(x * 91, false));
          for (int x = 0, w = b.width; w > 0; x += 5, w -= 10) b.FillScanlineUnsafe(x, 27, w, Color32.From(x * 97, false));
          return "FillScanlineUnsafe32 - fill";
        },
        b =>
        {
          var rnd = new Random(12345 + 0501);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            int w = rnd.Next(b.width - posX);
            b.FillScanlineUnsafe(posX, posY, w, Color32.From(Get32(rnd), false));
          }
          return "FillScanlineUnsafe32 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0502);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            b.FillScanline(posX, posY, w, Color32.From(Get32(rnd), false));
          }
          return "FillScanline32 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0503);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            b.FillScanline(posX, posY, w, Color32.From(Get32(rnd)));
          }
          return "FillScanline32 - random+alpha";
        },
        b =>
        {
          var rnd = new Random(12345 + 0504);
          for (int i = 0; i < 100; i++)
          {
            int posY = rnd.Next(b.height + 100) - 50;
            b.FillScanline(posY, Color32.From(Get32(rnd), false));
          }
          return "FillScanline32-y - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0505);
          for (int i = 0; i < 100; i++)
          {
            int posY = rnd.Next(b.height + 100) - 50;
            b.FillScanline(posY, Color32.From(Get32(rnd)));
          }
          return "FillScanline32-y - random+alpha";
        }
      );
    }
    #endregion
    #region # protected void Test05_FillScanline64()
    protected void Test05_FillScanline64()
    {
      InternalTest(160, 90, Color.Black, "FillScanline64",
        b =>
        {
          b.FillScanlineUnsafe(0, 0, b.width, Color64.From(Color.Red));
          b.FillScanlineUnsafe(0, 1, b.width, Color64.From(Color.Red));
          b.FillScanlineUnsafe(b.width / 4, 0, b.width / 2, Color64.From(Color.Green));
          b.FillScanlineUnsafe(b.width / 3, b.height - 2, b.width / 3, Color64.From(Color.Blue));
          b.FillScanlineUnsafe(b.width / 3, b.height - 1, b.width / 3, Color64.From(Color.Blue));
          b.FillScanlineUnsafe(b.width / 2, b.height - 1, b.width / 2, Color64.From(Color.White));
          return "FillScanlineUnsafe64 - simple";
        },
        b =>
        {
          for (int w = 1; w < b.width; w++) b.FillScanlineUnsafe(0, 3, w, Color64.From(w * 3, false));
          for (int w = 1; w < b.width; w += 2) b.FillScanlineUnsafe(0, 4, w, Color64.From(w * 5, false));
          for (int w = 1; w < b.width; w += 3) b.FillScanlineUnsafe(0, 5, w, Color64.From(w * 7, false));
          for (int w = 1; w < b.width; w += 4) b.FillScanlineUnsafe(0, 6, w, Color64.From(w * 11, false));
          for (int w = 1; w < b.width; w += 5) b.FillScanlineUnsafe(0, 7, w, Color64.From(w * 13, false));
          for (int w = b.width; w >= 1; w--) b.FillScanlineUnsafe(0, 8, w, Color64.From(w * 17, false));
          for (int w = b.width; w >= 1; w -= 2) b.FillScanlineUnsafe(0, 9, w, Color64.From(w * 19, false));
          for (int w = b.width; w >= 1; w -= 3) b.FillScanlineUnsafe(0, 10, w, Color64.From(w * 23, false));
          for (int w = b.width; w >= 1; w -= 4) b.FillScanlineUnsafe(0, 11, w, Color64.From(w * 29, false));
          for (int w = b.width; w >= 1; w -= 5) b.FillScanlineUnsafe(0, 12, w, Color64.From(w * 31, false));
          for (int x = 0; x < b.width - 1; x++) b.FillScanlineUnsafe(x, 13, b.width - x, Color64.From(x * 37, false));
          for (int x = 0; x < b.width - 1; x += 2) b.FillScanlineUnsafe(x, 14, b.width - x, Color64.From(x * 41, false));
          for (int x = 0; x < b.width - 1; x += 3) b.FillScanlineUnsafe(x, 15, b.width - x, Color64.From(x * 43, false));
          for (int x = 0; x < b.width - 1; x += 4) b.FillScanlineUnsafe(x, 16, b.width - x, Color64.From(x * 47, false));
          for (int x = 0; x < b.width - 1; x += 5) b.FillScanlineUnsafe(x, 17, b.width - x, Color64.From(x * 53, false));
          for (int x = b.width - 1; x >= 0; x--) b.FillScanlineUnsafe(x, 18, b.width - x, Color64.From(x * 59, false));
          for (int x = b.width - 1; x >= 0; x -= 2) b.FillScanlineUnsafe(x, 19, b.width - x, Color64.From(x * 61, false));
          for (int x = b.width - 1; x >= 0; x -= 3) b.FillScanlineUnsafe(x, 20, b.width - x, Color64.From(x * 67, false));
          for (int x = b.width - 1; x >= 0; x -= 4) b.FillScanlineUnsafe(x, 21, b.width - x, Color64.From(x * 71, false));
          for (int x = b.width - 1; x >= 0; x -= 5) b.FillScanlineUnsafe(x, 22, b.width - x, Color64.From(x * 73, false));
          for (int x = 0, w = b.width; w > 0; x++, w -= 2) b.FillScanlineUnsafe(x, 23, w, Color64.From(x * 79, false));
          for (int x = 0, w = b.width; w > 0; x += 2, w -= 4) b.FillScanlineUnsafe(x, 24, w, Color64.From(x * 83, false));
          for (int x = 0, w = b.width; w > 0; x += 3, w -= 6) b.FillScanlineUnsafe(x, 25, w, Color64.From(x * 89, false));
          for (int x = 0, w = b.width; w > 0; x += 4, w -= 8) b.FillScanlineUnsafe(x, 26, w, Color64.From(x * 91, false));
          for (int x = 0, w = b.width; w > 0; x += 5, w -= 10) b.FillScanlineUnsafe(x, 27, w, Color64.From(x * 97, false));
          return "FillScanlineUnsafe64 - fill";
        },
        b =>
        {
          var rnd = new Random(12345 + 0501);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width);
            int posY = rnd.Next(b.height);
            int w = rnd.Next(b.width - posX);
            b.FillScanlineUnsafe(posX, posY, w, Color64.From(Get64(rnd), false));
          }
          return "FillScanlineUnsafe64 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0502);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            b.FillScanline(posX, posY, w, Color64.From(Get64(rnd), false));
          }
          return "FillScanline64 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0503);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            b.FillScanline(posX, posY, w, Color64.From(Get64(rnd)));
          }
          return "FillScanline64 - random+alpha";
        },
        b =>
        {
          var rnd = new Random(12345 + 0504);
          for (int i = 0; i < 100; i++)
          {
            int posY = rnd.Next(b.height + 100) - 50;
            b.FillScanline(posY, Color64.From(Get64(rnd), false));
          }
          return "FillScanline64-y - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0505);
          for (int i = 0; i < 100; i++)
          {
            int posY = rnd.Next(b.height + 100) - 50;
            b.FillScanline(posY, Color64.From(Get64(rnd)));
          }
          return "FillScanline64-y - random+alpha";
        }
      );
    }
    #endregion

    #region # protected void Test06_WriteScanline32()
    protected void Test06_WriteScanline32()
    {
      InternalTest(160, 90, Color.Black, "WriteScanline32",
        b =>
        {
          var buf = new uint[b.width];
          fixed (uint* ptr = buf)
          {
            Fill(buf, Color32.From(Color.Red));
            b.WriteScanLineUnsafe(0, 0, b.width, ptr);
            b.WriteScanLineUnsafe(0, 1, b.width, ptr);
            Fill(buf, Color32.From(Color.Green));
            b.WriteScanLineUnsafe(b.width / 4, 0, b.width / 2, ptr);
            Fill(buf, Color32.From(Color.Blue));
            b.WriteScanLineUnsafe(b.width / 3, b.height - 2, b.width / 3, ptr);
            b.WriteScanLineUnsafe(b.width / 3, b.height - 1, b.width / 3, ptr);
            Fill(buf, Color32.From(Color.White));
            b.WriteScanLineUnsafe(b.width / 2, b.height - 1, b.width / 2, ptr);
          }
          return "WriteScanlineUnsafe32 - simple";
        },
        b =>
        {
          var rnd = new Random(12345 + 0601);
          var buf = new uint[b.width];
          fixed (uint* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width);
              int posY = rnd.Next(b.height);
              int w = rnd.Next(b.width - posX);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              Fill(buf, Color32.From(Get32(rnd), false));
              b.WriteScanLineUnsafe(posX, posY, w, ptr);
            }
          }
          return "WriteScanlineUnsafe32 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0602);
          var buf = new uint[b.width];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          fixed (uint* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width);
              int posY = rnd.Next(b.height);
              int w = rnd.Next(b.width - posX);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.WriteScanLineUnsafe(posX, posY, w, ptr);
            }
          }
          return "WriteScanlineUnsafe32 - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0603);
          var buf = new uint[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          fixed (uint* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width + 100) - 50;
              int posY = rnd.Next(b.height + 100) - 50;
              int w = rnd.Next(b.width - posX + 100);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.WriteScanLine(posX, posY, w, ptr);
            }
          }
          return "WriteScanline32 - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0604);
          var buf = new uint[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            if (w > buf.Length) throw new IndexOutOfRangeException();
            b.WriteScanLine(posX, posY, w, buf);
          }
          return "WriteScanline32[] - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0605);
          var buf = new uint[b.width];
          fixed (uint* ptr = buf)
          {
            for (int i = 0; i < 100; i++)
            {
              for (int j = 0; j < buf.Length; j++) buf[j] = Get32(rnd);
              int posY = rnd.Next(b.height + 100) - 50;
              b.WriteScanLine(posY, ptr);
            }
          }
          return "WriteScanline32-y - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0606);
          var buf = new uint[b.width];
          for (int i = 0; i < 100; i++)
          {
            for (int j = 0; j < buf.Length; j++) buf[j] = Get32(rnd);
            int posY = rnd.Next(b.height + 100) - 50;
            b.WriteScanLine(posY, buf);
          }
          return "WriteScanline32[]-y - randomX";
        }
      );
    }
    #endregion
    #region # protected void Test06_WriteScanline64()
    protected void Test06_WriteScanline64()
    {
      InternalTest(160, 90, Color.Black, "WriteScanline64",
        b =>
        {
          var buf = new ulong[b.width];
          fixed (ulong* ptr = buf)
          {
            Fill(buf, Color64.From(Color.Red));
            b.WriteScanLineUnsafe(0, 0, b.width, ptr);
            b.WriteScanLineUnsafe(0, 1, b.width, ptr);
            Fill(buf, Color64.From(Color.Green));
            b.WriteScanLineUnsafe(b.width / 4, 0, b.width / 2, ptr);
            Fill(buf, Color64.From(Color.Blue));
            b.WriteScanLineUnsafe(b.width / 3, b.height - 2, b.width / 3, ptr);
            b.WriteScanLineUnsafe(b.width / 3, b.height - 1, b.width / 3, ptr);
            Fill(buf, Color64.From(Color.White));
            b.WriteScanLineUnsafe(b.width / 2, b.height - 1, b.width / 2, ptr);
          }
          return "WriteScanlineUnsafe64 - simple";
        },
        b =>
        {
          var rnd = new Random(12345 + 0601);
          var buf = new ulong[b.width];
          fixed (ulong* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width);
              int posY = rnd.Next(b.height);
              int w = rnd.Next(b.width - posX);
              Fill(buf, Color64.From(Get64(rnd), false));
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.WriteScanLineUnsafe(posX, posY, w, ptr);
            }
          }
          return "WriteScanlineUnsafe64 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0602);
          var buf = new ulong[b.width];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get64(rnd);
          fixed (ulong* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width);
              int posY = rnd.Next(b.height);
              int w = rnd.Next(b.width - posX);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.WriteScanLineUnsafe(posX, posY, w, ptr);
            }
          }
          return "WriteScanlineUnsafe64 - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0603);
          var buf = new ulong[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get64(rnd);
          fixed (ulong* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width + 100) - 50;
              int posY = rnd.Next(b.height + 100) - 50;
              int w = rnd.Next(b.width - posX + 100);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.WriteScanLine(posX, posY, w, ptr);
            }
          }
          return "WriteScanline64 - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0604);
          var buf = new ulong[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get64(rnd);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            if (w > buf.Length) throw new IndexOutOfRangeException();
            b.WriteScanLine(posX, posY, w, buf);
          }
          return "WriteScanline64[] - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0605);
          var buf = new ulong[b.width];
          fixed (ulong* ptr = buf)
          {
            for (int i = 0; i < 100; i++)
            {
              for (int j = 0; j < buf.Length; j++) buf[j] = Get64(rnd);
              int posY = rnd.Next(b.height + 100) - 50;
              b.WriteScanLine(posY, ptr);
            }
          }
          return "WriteScanline64-y - randomX";
        },
        b =>
        {
          var rnd = new Random(12345 + 0606);
          var buf = new ulong[b.width];
          for (int i = 0; i < 100; i++)
          {
            for (int j = 0; j < buf.Length; j++) buf[j] = Get64(rnd);
            int posY = rnd.Next(b.height + 100) - 50;
            b.WriteScanLine(posY, buf);
          }
          return "WriteScanline64[]-y - randomX";
        }
      );
    }
    #endregion

    #region # protected void Test07_ReadScanline32()
    protected void Test07_ReadScanline32()
    {
      InternalTest(160, 90, Color.Black, "ReadScanline32",
        b =>
        {
          var buf = new uint[b.width];
          fixed (uint* ptr = buf)
          {
            Fill(buf, Color32.From(Color.Red));
            b.WriteScanLineUnsafe(0, 0, b.width, ptr);
            b.WriteScanLineUnsafe(0, 1, b.width, ptr);
            Fill(buf, Color32.From(Color.Green));
            b.WriteScanLineUnsafe(b.width / 4, 0, b.width / 2, ptr);
            Fill(buf, Color32.From(Color.Blue));
            b.WriteScanLineUnsafe(b.width / 3, b.height - 2, b.width / 3, ptr);
            b.WriteScanLineUnsafe(b.width / 3, b.height - 1, b.width / 3, ptr);
            Fill(buf, Color32.From(Color.White));
            b.WriteScanLineUnsafe(b.width / 2, b.height - 1, b.width / 2, ptr);

            b.ReadScanLineUnsafe(0, 0, b.width, ptr);
            for (int i = 0; i < b.width; i++)
            {
              Assert.AreEqual(i >= b.width / 4 && i < b.width / 4 + b.width / 2 ? Color32.From(Color.Green) : Color32.From(Color.Red), ptr[i]);
            }
            b.ReadScanLineUnsafe(b.width / 4 - 1, 0, b.width / 2 + 2, ptr);
            Assert.AreEqual(Color32.From(Color.Red), ptr[0]);
            Assert.AreEqual(Color32.From(Color.Red), ptr[b.width / 2 + 1]);
            for (int i = 1; i < b.width / 2 + 1; i++)
            {
              Assert.AreEqual(Color32.From(Color.Green), ptr[i]);
            }
          }
          return "ReadScanlineUnsafe32 - simple";
        },
        b =>
        {
          var rnd = new Random(12345 + 0701);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd), false));
          }

          var buf = new uint[b.width];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          fixed (uint* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width);
              int posY = rnd.Next(b.height);
              int w = rnd.Next(b.width - posX);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.ReadScanLineUnsafe(posX, posY, w, ptr);
              if (rnd.Next(2) == 0)
              {
                for (int x = 0; x < w; x++) Assert.AreEqual(b.GetPixelUnsafe32(posX + x, posY), ptr[x]);
              }
              else
              {
                for (int x = w - 1; x >= 0; x--) Assert.AreEqual(b.GetPixelUnsafe32(posX + x, posY), ptr[x]);
              }
            }
          }
          return "ReadScanlineUnsafe32 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0702);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd), false));
          }

          var buf = new uint[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          fixed (uint* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width + 100) - 50;
              int posY = rnd.Next(b.height + 100) - 50;
              int w = rnd.Next(b.width - posX + 100);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.ReadScanLine(posX, posY, w, ptr);

              if (rnd.Next(2) == 0)
              {
                for (int x = 0; x < w; x++) Assert.AreEqual(b.GetPixel32(posX + x, posY), ptr[x]);
              }
              else
              {
                for (int x = w - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel32(posX + x, posY), ptr[x]);
              }
            }
          }
          return "ReadScanline32 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0703);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd), false));
          }

          var buf = new uint[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            if (w > buf.Length) throw new IndexOutOfRangeException();
            int ofs = rnd.Next(100);
            if (ofs + w > buf.Length) ofs = buf.Length - w;
            b.ReadScanLine(posX, posY, w, buf, ofs);

            if (rnd.Next(2) == 0)
            {
              for (int x = 0; x < w; x++) Assert.AreEqual(b.GetPixel32(posX + x, posY), buf[x + ofs]);
            }
            else
            {
              for (int x = w - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel32(posX + x, posY), buf[x + ofs]);
            }
          }
          return "ReadScanline32[] - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0704);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd), false));
          }

          var buf = new uint[b.width];
          fixed (uint* ptr = buf)
          {
            for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
            for (int i = 0; i < 100; i++)
            {
              int posY = rnd.Next(b.height + 100) - 50;
              b.ReadScanLine(posY, ptr);

              if (rnd.Next(2) == 0)
              {
                for (int x = 0; x < b.width; x++) Assert.AreEqual(b.GetPixel32(x, posY), ptr[x]);
              }
              else
              {
                for (int x = b.width - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel32(x, posY), ptr[x]);
              }
            }
          }
          return "ReadScanline32-y - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0705);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd), false));
          }

          var buf = new uint[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          for (int i = 0; i < 100; i++)
          {
            int posY = rnd.Next(b.height + 100) - 50;
            int ofs = rnd.Next(200);
            if (ofs + b.width > buf.Length) throw new IndexOutOfRangeException();
            b.ReadScanLine(posY, buf, ofs);

            if (rnd.Next(2) == 0)
            {
              for (int x = 0; x < b.width; x++) Assert.AreEqual(b.GetPixel32(x, posY), buf[x + ofs]);
            }
            else
            {
              for (int x = b.width - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel32(x, posY), buf[x + ofs]);
            }
          }
          return "ReadScanline32-y[] - random";
        }
      );
    }
    #endregion
    #region # protected void Test07_ReadScanline64()
    protected void Test07_ReadScanline64()
    {
      InternalTest(160, 90, Color.Black, "ReadScanline64",
        b =>
        {
          var buf = new ulong[b.width];
          fixed (ulong* ptr = buf)
          {
            Fill(buf, Color64.From(Color.Red));
            b.WriteScanLineUnsafe(0, 0, b.width, ptr);
            b.WriteScanLineUnsafe(0, 1, b.width, ptr);
            Fill(buf, Color64.From(Color.Green));
            b.WriteScanLineUnsafe(b.width / 4, 0, b.width / 2, ptr);
            Fill(buf, Color64.From(Color.Blue));
            b.WriteScanLineUnsafe(b.width / 3, b.height - 2, b.width / 3, ptr);
            b.WriteScanLineUnsafe(b.width / 3, b.height - 1, b.width / 3, ptr);
            Fill(buf, Color64.From(Color.White));
            b.WriteScanLineUnsafe(b.width / 2, b.height - 1, b.width / 2, ptr);

            b.ReadScanLineUnsafe(0, 0, b.width, ptr);
            for (int i = 0; i < b.width; i++)
            {
              Assert.AreEqual(i >= b.width / 4 && i < b.width / 4 + b.width / 2 ? Color64.From(Color.Green) : Color64.From(Color.Red), ptr[i]);
            }
            b.ReadScanLineUnsafe(b.width / 4 - 1, 0, b.width / 2 + 2, ptr);
            Assert.AreEqual(Color64.From(Color.Red), ptr[0]);
            Assert.AreEqual(Color64.From(Color.Red), ptr[b.width / 2 + 1]);
            for (int i = 1; i < b.width / 2 + 1; i++)
            {
              Assert.AreEqual(Color64.From(Color.Green), ptr[i]);
            }
          }
          return "ReadScanlineUnsafe64 - simple";
        },
        b =>
        {
          var rnd = new Random(12345 + 0701);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color64.From(Get64(rnd), false));
          }

          var buf = new ulong[b.width];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get32(rnd);
          fixed (ulong* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width);
              int posY = rnd.Next(b.height);
              int w = rnd.Next(b.width - posX);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.ReadScanLineUnsafe(posX, posY, w, ptr);
              if (rnd.Next(2) == 0)
              {
                for (int x = 0; x < w; x++) Assert.AreEqual(b.GetPixelUnsafe64(posX + x, posY), ptr[x]);
              }
              else
              {
                for (int x = w - 1; x >= 0; x--) Assert.AreEqual(b.GetPixelUnsafe64(posX + x, posY), ptr[x]);
              }
            }
          }
          return "ReadScanlineUnsafe64 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0702);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color64.From(Get64(rnd), false));
          }

          var buf = new ulong[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get64(rnd);
          fixed (ulong* ptr = buf)
          {
            for (int i = 0; i < 1000; i++)
            {
              int posX = rnd.Next(b.width + 100) - 50;
              int posY = rnd.Next(b.height + 100) - 50;
              int w = rnd.Next(b.width - posX + 100);
              if (w > buf.Length) throw new IndexOutOfRangeException();
              b.ReadScanLine(posX, posY, w, ptr);

              if (rnd.Next(2) == 0)
              {
                for (int x = 0; x < w; x++) Assert.AreEqual(b.GetPixel64(posX + x, posY), ptr[x]);
              }
              else
              {
                for (int x = w - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel64(posX + x, posY), ptr[x]);
              }
            }
          }
          return "ReadScanline64 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0703);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color64.From(Get64(rnd), false));
          }

          var buf = new ulong[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get64(rnd);
          for (int i = 0; i < 1000; i++)
          {
            int posX = rnd.Next(b.width + 100) - 50;
            int posY = rnd.Next(b.height + 100) - 50;
            int w = rnd.Next(b.width - posX + 100);
            if (w > buf.Length) throw new IndexOutOfRangeException();
            int ofs = rnd.Next(100);
            if (ofs + w > buf.Length) ofs = buf.Length - w;
            b.ReadScanLine(posX, posY, w, buf, ofs);

            if (rnd.Next(2) == 0)
            {
              for (int x = 0; x < w; x++) Assert.AreEqual(b.GetPixel64(posX + x, posY), buf[x + ofs]);
            }
            else
            {
              for (int x = w - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel64(posX + x, posY), buf[x + ofs]);
            }
          }
          return "ReadScanline64[] - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0704);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color64.From(Get64(rnd), false));
          }

          var buf = new ulong[b.width];
          fixed (ulong* ptr = buf)
          {
            for (int i = 0; i < buf.Length; i++) buf[i] = Get64(rnd);
            for (int i = 0; i < 100; i++)
            {
              int posY = rnd.Next(b.height + 100) - 50;
              b.ReadScanLine(posY, ptr);

              if (rnd.Next(2) == 0)
              {
                for (int x = 0; x < b.width; x++) Assert.AreEqual(b.GetPixel64(x, posY), ptr[x]);
              }
              else
              {
                for (int x = b.width - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel64(x, posY), ptr[x]);
              }
            }
          }
          return "ReadScanline64-y - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 0705);
          for (int i = 0; i < 1000; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get64(rnd), false));
          }

          var buf = new ulong[b.width + 200];
          for (int i = 0; i < buf.Length; i++) buf[i] = Get64(rnd);
          for (int i = 0; i < 100; i++)
          {
            int posY = rnd.Next(b.height + 100) - 50;
            int ofs = rnd.Next(200);
            if (ofs + b.width > buf.Length) throw new IndexOutOfRangeException();
            b.ReadScanLine(posY, buf, ofs);

            if (rnd.Next(2) == 0)
            {
              for (int x = 0; x < b.width; x++) Assert.AreEqual(b.GetPixel64(x, posY), buf[x + ofs]);
            }
            else
            {
              for (int x = b.width - 1; x >= 0; x--) Assert.AreEqual(b.GetPixel64(x, posY), buf[x + ofs]);
            }
          }
          return "ReadScanline64-y[] - random";
        }
      );
    }
    #endregion
  }
}
