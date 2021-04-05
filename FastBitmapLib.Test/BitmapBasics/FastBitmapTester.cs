using System;
using System.Drawing;
using FastBitmapLib.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastBitmapLib.Test.BitmapBasics
{
  /// <summary>
  /// Test-Class
  /// </summary>
  public abstract class FastBitmapTester
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
    uint Get32(Random rnd)
    {
      var buf = new byte[4];
      rnd.NextBytes(buf);
      return BitConverter.ToUInt32(buf, 0);
    }

    ulong Get64(Random rnd)
    {
      var buf = new byte[8];
      rnd.NextBytes(buf);
      return BitConverter.ToUInt64(buf, 0);
    }
    #endregion

    protected void Test01_Constructor()
    {
      InternalTest(320, 240, Color.FromArgb(0), "Constructor");
    }

    protected void Test02_Constructor_Background()
    {
      InternalTest(40, 30, Color.Red, "Constructor + Background-Red");
      InternalTest(41, 31, Color.Green, "Constructor + Background-Green");
      InternalTest(42, 32, Color.Blue, "Constructor + Background-Blue");
      InternalTest(43, 33, Color.Black, "Constructor + Background-Black");
    }

    protected void Test03_SetPixel32()
    {
      InternalTest(42, 32, Color.FromArgb(0), "SetPixel",
        b =>
        {
          b.SetPixelUnsafe(0, 0, Color32.From(Color.Red));
          b.SetPixelUnsafe(b.width - 1, 0, Color32.From(Color.Green));
          b.SetPixelUnsafe(0, b.height - 1, Color32.From(Color.Blue));
          b.SetPixelUnsafe(b.width - 1, b.height - 1, Color32.From(Color.White));
          return "SetPixelUnsafe x32 - corners";
        },
        b =>
        {
          var rnd = new Random(12345 + 1);
          for (int i = 0; i < 100; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd), false));
          }
          return "SetPixelUnsafe x32 - random";
        },
        b =>
        {
          var rnd = new Random(12345 + 2);
          for (int i = 0; i < 100; i++)
          {
            b.SetPixelUnsafe(rnd.Next(b.width), rnd.Next(b.height), Color32.From(Get32(rnd)));
          }
          return "SetPixelUnsafe x32 - random+alpha";
        }
      );
    }
  }
}
