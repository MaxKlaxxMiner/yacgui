#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Drawing;
using FastBitmapLib.Extras;
using FastBitmapLib.Test.BitmapBasics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable MemberCanBePrivate.Global
#endregion

namespace FastBitmapLib.Test
{
  [TestClass]
  public sealed class Colors
  {
    #region # public static Color[] GetTestColors(bool basics = true, bool gradients = false, bool alphas = false, bool randoms = false)
    public static Color[] GetTestColors(bool basics = true, bool gradients = false, bool alphas = false, bool randoms = false)
    {
      var result = new List<Color>();

      if (basics)
      {
        foreach (var known in (KnownColor[])Enum.GetValues(typeof(KnownColor)))
        {
          if (known == KnownColor.Transparent && !alphas) continue;
          result.Add(Color.FromKnownColor(known));
        }
      }

      if (gradients)
      {
        for (int r = 0; r < 255; r++) result.Add(Color.FromArgb(r, 0, 0));
        for (int g = 0; g < 255; g++) result.Add(Color.FromArgb(255 - g, g, 0));
        for (int b = 0; b < 255; b++) result.Add(Color.FromArgb(0, 255 - b, b));
        for (int c = 0; c < 255; c++) result.Add(Color.FromArgb(0, c, 255));
        for (int m = 0; m < 255; m++) result.Add(Color.FromArgb(m, 255 - m, 255));
        for (int y = 0; y < 255; y++) result.Add(Color.FromArgb(255, y, 255 - y));
        for (int w = 0; w < 255; w++) result.Add(Color.FromArgb(255, 255, w));
        for (int b = 0; b <= 255; b++) result.Add(Color.FromArgb(255 - b, 255 - b, 255 - b));
      }

      if (alphas)
      {
        int count = result.Count;
        int[] alphaValues = { 0, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233 };
        foreach (int alpha in alphaValues)
        {
          for (int i = 0; i < count; i++)
          {
            result.Add(Color.FromArgb(alpha, result[i]));
          }
        }
        for (int alpha = 0; alpha < 256; alpha++)
        {
          result.Add(Color.FromArgb(alpha, Color.Black));
          result.Add(Color.FromArgb(alpha, Color.Red));
          result.Add(Color.FromArgb(alpha, Color.Green));
          result.Add(Color.FromArgb(alpha, Color.Blue));
          result.Add(Color.FromArgb(alpha, Color.White));
        }
      }

      if (randoms)
      {
        var rnd = new Random(12345);
        int count = result.Count;
        for (int i = 0; i < count; i++)
        {
          var c = result[i];
          result.Add(Color.FromArgb(
            (byte)(c.A + (alphas ? rnd.Next(-50, 50) : 0)),
            (byte)(c.R + rnd.Next(-50, 50)),
            (byte)(c.G + rnd.Next(-50, 50)),
            (byte)(c.B + rnd.Next(-50, 50))));
        }
        for (int i = 0; i < 1024; i++)
        {
          result.Add(Color.FromArgb(
            alphas ? rnd.Next(0, 256) : 255,
            rnd.Next(0, 256),
            rnd.Next(0, 256),
            rnd.Next(0, 256)));
        }
      }

      return result.ToArray();
    }
    #endregion

    #region # // --- CountColors ---
    [TestMethod]
    public void CountColorsKnown()
    {
      var testColors = GetTestColors(); // basics (named known colors)
      Assert.AreEqual(174 - 1, testColors.Length);
    }

    [TestMethod]
    public void CountColorsGradients()
    {
      var testColors = GetTestColors(false, true); // gradients
      Assert.AreEqual(8 * 255 + 1, testColors.Length);
    }

    [TestMethod]
    public void CountColorsAlphas()
    {
      var testColors = GetTestColors(false, false, true); // alphas
      Assert.AreEqual(5 * 256, testColors.Length);
    }

    [TestMethod]
    public void CountColorsAlphasExtra()
    {
      var testColors = GetTestColors(true, true, true); // (basic + gradients) * 6 alphas
      Assert.AreEqual((174 + 8 * 255 + 1) * 14 + 5 * 256, testColors.Length);
    }

    [TestMethod]
    public void CountColorsRandom()
    {
      var testColors = GetTestColors(false, false, false, true); // random
      Assert.AreEqual(1024, testColors.Length);
    }

    [TestMethod]
    public void CountColorsFull()
    {
      var testColors = GetTestColors(true, true, true, true); // full
      Assert.AreEqual(((174 + 8 * 255 + 1) * 14 + 5 * 256) * 2 + 1024, testColors.Length);
    }
    #endregion

    #region # // --- Convert ---
    static uint ColorTo32(Color color)
    {
      return (uint)color.ToArgb();
    }

    static ulong ColorTo64(Color color)
    {
      return (ulong)color.A << 56 | (ulong)color.A << 48 |
             (ulong)color.R << 40 | (ulong)color.R << 32 |
             (ulong)color.G << 24 | (ulong)color.G << 16 |
             (ulong)color.B << 8 | color.B;
    }

    [TestMethod]
    public void Convert32Basics()
    {
      var testColors = GetTestColors(true, true, false, true); // all colors without alpha
      foreach (var color in testColors)
      {
        var val = Color32.From(color, false);
        Assert.AreEqual(ColorTo32(color), val);

        val = Color32.From(color.R, color.G, color.B);
        Assert.AreEqual(ColorTo32(color), val);
      }
    }

    [TestMethod]
    public void Convert64Basics()
    {
      var testColors = GetTestColors(true, true, false, true); // all colors without alpha
      foreach (var color in testColors)
      {
        var val = Color64.From(color, false);
        Assert.AreEqual(ColorTo64(color), val);

        val = Color64.From(color.R, color.G, color.B);
        Assert.AreEqual(ColorTo64(color), val);

        val = Color64.From((ushort)(color.R | color.R << 8), (ushort)(color.G | color.G << 8), (ushort)(color.B | color.B << 8));

        var val32 = Color32.From(val, false);
        Assert.AreEqual(ColorTo32(color), val32);
      }
    }

    [TestMethod]
    public void Convert32Alpha()
    {
      var testColors = GetTestColors(true, true, true, true); // all colors
      foreach (var color in testColors)
      {
        var val = Color32.From(color);
        Assert.AreEqual(ColorTo32(color), val);

        var valNoAlpha = Color32.From(color, false);
        Assert.AreEqual(ColorTo32(color) | 0xff000000, valNoAlpha);

        var valAlpha = Color32.From(color, 0x33);
        Assert.AreEqual((ColorTo32(color) & 0xffffff) | 0x33000000, valAlpha);
      }
    }

    [TestMethod]
    public void Convert64Alpha()
    {
      var testColors = GetTestColors(true, true, true, true); // all colors
      foreach (var color in testColors)
      {
        var val = Color64.From(color);
        Assert.AreEqual(ColorTo64(color), val);

        var valNoAlpha = Color64.From(color, false);
        Assert.AreEqual(ColorTo64(color) | 0xffff000000000000, valNoAlpha);

        var valAlpha = Color64.From(color, 0x1234);
        Assert.AreEqual((ColorTo64(color) & 0xffffffffffff) | 0x1234000000000000, valAlpha);

        var val32 = Color32.From(val);
        Assert.AreEqual(ColorTo32(color), val32);

        var valNoAlpha32 = Color32.From(valNoAlpha, false);
        Assert.AreEqual(ColorTo32(color) | 0xff000000, valNoAlpha32);

        var valAlpha32 = Color32.From(valAlpha);
        Assert.AreEqual((ColorTo32(color) & 0xffffff) | 0x12000000, valAlpha32);
      }
    }
    #endregion

    #region # // --- Blend ---
    static uint BlendSlow32(uint firstColor, uint secondColor, uint amountSecond)
    {
      double r1 = (firstColor >> 16 & 0xff) / 256.0;
      double g1 = (firstColor >> 8 & 0xff) / 256.0;
      double b1 = (firstColor & 0xff) / 256.0;
      double r2 = (secondColor >> 16 & 0xff) / 256.0;
      double g2 = (secondColor >> 8 & 0xff) / 256.0;
      double b2 = (secondColor & 0xff) / 256.0;
      double amount1 = (256 - amountSecond) / 256.0;
      double amount2 = amountSecond / 256.0;
      double r = r1 * amount1 + r2 * amount2;
      double g = g1 * amount1 + g2 * amount2;
      double b = b1 * amount1 + b2 * amount2;
      uint ur = Math.Min(255, Math.Max(0, (uint)(r * 256.0)));
      uint ug = Math.Min(255, Math.Max(0, (uint)(g * 256.0)));
      uint ub = Math.Min(255, Math.Max(0, (uint)(b * 256.0)));
      return 0xff000000 | ur << 16 | ug << 8 | ub;
    }

    static ulong BlendSlow64(ulong firstColor, ulong secondColor, ulong amountSecond)
    {
      double r1 = (firstColor >> 32 & 0xffff) / 65536.0;
      double g1 = (firstColor >> 16 & 0xffff) / 65536.0;
      double b1 = (firstColor & 0xffff) / 65536.0;
      double r2 = (secondColor >> 32 & 0xffff) / 65536.0;
      double g2 = (secondColor >> 16 & 0xffff) / 65536.0;
      double b2 = (secondColor & 0xffff) / 65536.0;
      double amount1 = (65536 - amountSecond) / 65536.0;
      double amount2 = amountSecond / 65536.0;
      double r = r1 * amount1 + r2 * amount2;
      double g = g1 * amount1 + g2 * amount2;
      double b = b1 * amount1 + b2 * amount2;
      ulong ur = Math.Min(65535, Math.Max(0, (uint)(r * 65536.0)));
      ulong ug = Math.Min(65535, Math.Max(0, (uint)(g * 65536.0)));
      ulong ub = Math.Min(65535, Math.Max(0, (uint)(b * 65536.0)));
      return 0xffff000000000000 | ur << 32 | ug << 16 | ub;
    }

    static void CheckBlend32(uint first, uint second)
    {
      for (uint amount = 0; amount <= 256; amount++)
      {
        uint c1 = BlendSlow32(first, second, amount);
        uint c2 = Color32.BlendFast(first, second, amount);
        Assert.AreEqual(c1, c2);
      }
    }

    static void CheckBlend64(ulong first, ulong second)
    {
      for (ulong amount = 0; amount <= 65536; amount++)
      {
        ulong c1 = BlendSlow64(first, second, amount);
        ulong c2 = Color64.BlendFast(first, second, amount);
        Assert.AreEqual(c1, c2);
      }
    }

    static void CheckBlend64Quick(ulong first, ulong second)
    {
      for (ulong amount = 0; amount <= 65536; amount += 123)
      {
        ulong c1 = BlendSlow64(first, second, amount);
        ulong c2 = Color64.BlendFast(first, second, amount);
        Assert.AreEqual(c1, c2);
      }
    }

    [TestMethod]
    public void Blend32()
    {
      CheckBlend32(0xff0000, 0x00ff00);
      CheckBlend32(0x002288, 0x99aa13);
      CheckBlend32(0x112233, 0x99ccff);
    }

    [TestMethod]
    public void Blend32Full()
    {
      for (uint cFirst = 0; cFirst < 256; cFirst++)
      {
        for (uint cSecond = 0; cSecond < 256; cSecond++)
        {
          CheckBlend32(cFirst, cSecond);
        }
      }
      var rnd = new Random(12345);
      for (int i = 0; i < 20000; i++)
      {
        uint c1 = FastBitmapTester.Get32(rnd);
        uint c2 = FastBitmapTester.Get32(rnd);
        CheckBlend32(c1, c2);
      }
    }

    [TestMethod]
    public void Blend64()
    {
      CheckBlend64(0xffff00000000, 0x0000ffff0000);
      CheckBlend64(0x000022228888, 0x9999aaaa1313);
      CheckBlend64(0x111122223333, 0x9999ccccffff);
    }

    [TestMethod]
    public void Blend64Full()
    {
      for (uint cFirst = 0; cFirst < 256; cFirst++)
      {
        for (uint cSecond = 0; cSecond < 256; cSecond++)
        {
          CheckBlend64Quick(Color64.From(cFirst), Color64.From(cSecond));
        }
      }
      var rnd = new Random(12345);
      for (int i = 0; i < 10000; i++)
      {
        ulong c1 = FastBitmapTester.Get64(rnd);
        ulong c2 = FastBitmapTester.Get64(rnd);
        CheckBlend64Quick(c1, c2);
      }
    }
    #endregion
  }
}
