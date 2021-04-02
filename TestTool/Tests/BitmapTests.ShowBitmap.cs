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
    /// Draw bitmaps (as sprites) on a bitmap, ignore alpha channel
    /// </summary>
    public static void ShowDrawBitmap()
    {
      var fastBitmap = new FastBitmapOld(1024, 576, 0xff252525);

      var texture = GetDemoTexture();

      fastBitmap.DrawBitmap(texture, -10, -10);

      var resultBitmap = fastBitmap.ToGDIBitmapAlpha();

      ShowPicture(resultBitmap, "Sprite Draw", (form, mousePos) =>
      {
        fastBitmap.Clear();
        fastBitmap.DrawBitmap(texture, mousePos.X - texture.width / 2, mousePos.Y - texture.height / 2);
        fastBitmap.DrawBitmap(texture,
          mousePos.X - texture.width / 6 / 2, mousePos.Y - texture.height / 2 / 2, // x, y
          texture.width / 6 * 3, texture.height / 2 * 1, // spriteX, spriteY
          texture.width / 6, texture.height / 2 // spriteWidth, spriteHeight
        );
        fastBitmap.CopyToGDIBitmap(resultBitmap);
        form.Refresh();
      });
    }

    /// <summary>
    /// Draw bitmaps (as sprites) on a bitmap, use bitmap alpha
    /// </summary>
    public static void ShowAlphaBitmap()
    {
      var fastBitmap = new FastBitmapOld(1024, 576, 0xff252525);

      var texture = GetDemoTexture();

      fastBitmap.DrawBitmapAlpha(texture, -10, -10);

      var resultBitmap = fastBitmap.ToGDIBitmap();

      ShowPicture(resultBitmap, "Sprite Draw Alpha", (form, mousePos) =>
      {
        fastBitmap.Clear();
        fastBitmap.DrawBitmapAlpha(texture, mousePos.X - texture.width / 2, mousePos.Y - texture.height / 2);
        fastBitmap.DrawBitmapAlpha(texture,
          mousePos.X - texture.width / 6 / 2, mousePos.Y - texture.height / 2 / 2, // x, y
          texture.width / 6 * 3, texture.height / 2 * 1, // spriteX, spriteY
          texture.width / 6, texture.height / 2 // spriteWidth, spriteHeight
        );
        fastBitmap.CopyToGDIBitmap(resultBitmap);
        form.Refresh();
      });
    }
  }
}
