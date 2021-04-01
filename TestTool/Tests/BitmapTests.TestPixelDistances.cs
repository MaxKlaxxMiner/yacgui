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
    /// Test pixel distance calculation
    /// </summary>
    public static void TestPixelDistances()
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

      for (int i = 1024, c = 256; i < 1024 + 256; i++, c--)
      {
        // 256, 254, 253, 252 ....
        Debug.Assert(distances[i] == c);
        if (c == 256) c--;
      }
    }
  }
}
