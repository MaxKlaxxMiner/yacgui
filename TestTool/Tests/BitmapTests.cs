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
  public partial class BitmapTests : ConsoleExtras
  {
    public static void TestAllValues()
    {
      TestFastBitmap();

      TestAlphaMask();

      TestPixelDistances();
    }

    public static void ShowAllTestPictures()
    {
      ShowSubPixelTest();

      ShowPixelDistanceTest();

      ShowShinyPieces();
    }

    /// <summary>
    /// Run all Bitmap-Tests
    /// </summary>
    public static void Run()
    {
      //TestAllValues();

      //ShowAllTestPictures();

      ShowDrawCircles();
    }
  }
}
