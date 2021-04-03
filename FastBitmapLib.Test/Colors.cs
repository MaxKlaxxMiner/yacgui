#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Drawing;
using FastBitmapLib.Extras;
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
  }
}
