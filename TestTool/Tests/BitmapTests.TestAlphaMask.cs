﻿#region # using *.*
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
  public partial class BitmapTests
  {
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
  }
}