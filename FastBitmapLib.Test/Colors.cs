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
  public class Colors
  {
    public static Color[] GetTestColors(bool basics = true)
    {
      var result = new List<Color>();

      if (basics)
      {
        foreach (var known in (KnownColor[])Enum.GetValues(typeof(KnownColor)))
        {
          result.Add(Color.FromKnownColor(known));
        }
      }

      return result.ToArray();
    }

    [TestMethod]
    public void TestMethod1()
    {
      var testColors = GetTestColors();
      Assert.AreEqual(testColors.Length, 174);
    }
  }
}
