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
// ReSharper disable UnusedMember.Global
#endregion

namespace TestTool
{
  /// <summary>
  /// Class for testing bitmap functions
  /// </summary>
  public partial class BitmapTests : ConsoleExtras
  {
    /// <summary>
    /// Run all basic tests
    /// </summary>
    public static void TestAllValues()
    {
      TestFastBitmap();

      TestAlphaMask();

      TestPixelDistances();
    }

    /// <summary>
    /// Run all visible tests
    /// </summary>
    public static void ShowAllTestPictures()
    {
      ShowSubPixelTest();

      ShowPixelDistanceTest();

      ShowShinyPieces();

      ShowDrawLines();
      ShowDrawCircles();
      ShowDrawTriangles();

      ShowTextureMappedSimple();
      ShowTextureMappedTextured();
      ShowTextureMappedTexturedPerspective();

      ShowDrawBitmap();
      ShowAlphaBitmap();
      ShowDrawPerformanceNaive();
      ShowDrawPerformanceOptimized();
      ShowCheckerPerformanceNaive();
      ShowCheckerPerformanceOptimized();
      ShowCheckerRotate();

      ShowShadowRealtime();
    }

    /// <summary>
    /// Run all Bitmap-Tests
    /// </summary>
    public static void Run()
    {
      //TestAllValues();

      //ShowAllTestPictures();

      ShowPixelDistanceTest();
    }
  }
}
