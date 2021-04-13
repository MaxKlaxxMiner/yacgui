using System;
using FastBitmapLib.Extras;
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable JoinDeclarationAndInitializer

namespace FastBitmapLib
{
  public static partial class FastBitmapExtensions
  {
    /// <summary>
    /// Create a resized bitmap (simple nearest pixel)
    /// </summary>
    /// <param name="bitmap">FastBitmap</param>
    /// <param name="newWidth">Width of the new picture</param>
    /// <param name="newHeight">Height of the nw picture</param>
    /// <param name="constructor">Optional: constructor for the new FastBitmap</param>
    /// <returns>Resized bitmap</returns>
    public static IFastBitmap GetResizedHigh(this IFastBitmap bitmap, int newWidth, int newHeight, Func<int, int, IFastBitmap> constructor = null)
    {
      if (newWidth <= 0 && newHeight <= 0) throw new ArgumentException();

      if (newWidth <= 0) newWidth = newHeight * bitmap.width / bitmap.height;
      if (newHeight <= 0) newHeight = newWidth * bitmap.height / bitmap.width;

      if (constructor == null)
      {
        if (bitmap is IFastBitmap64)
        {
          constructor = (w, h) => new FastBitmap64(w, h);
        }
        else
        {
          constructor = (w, h) => new FastBitmap(w, h);
        }
      }

      var result = constructor(newWidth, newHeight);
      if (result == null) throw new ArgumentNullException("constructor");
      if (result.width != newWidth || result.height != newHeight) throw new ArgumentOutOfRangeException("constructor");

      var xMap = LinearPixel.GenerateMapping(bitmap.width, newWidth);
      var yMap = LinearPixel.GenerateMapping(bitmap.height, newHeight);

      if (result is IFastBitmap64)
      {
        for (int y = 0; y < newHeight; y++)
        {
          var yM = yMap[y];
          for (int x = 0; x < newWidth; x++)
          {
            var xM = xMap[x];
            ulong pixel;
            ulong mul;
            ulong a = 0, r = 0, g = 0, b = 0;

            // --- top left ---
            pixel = bitmap.GetPixelUnsafe64((int)xM.first, (int)yM.first);
            mul = xM.firstMul * yM.firstMul;
            a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;

            // --- left column ---
            mul = xM.firstMul * yM.midMul;
            for (uint cy = yM.first + 1; cy < yM.last; cy++)
            {
              pixel = bitmap.GetPixelUnsafe64((int)xM.first, (int)cy);
              a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;
            }

            // --- bottom left ---
            pixel = bitmap.GetPixelUnsafe64((int)xM.first, (int)yM.last);
            mul = xM.firstMul * yM.lastMul;
            a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;

            // --- top row ---
            mul = xM.midMul * yM.firstMul;
            for (uint cx = xM.first + 1; cx < xM.last; cx++)
            {
              pixel = bitmap.GetPixelUnsafe64((int)cx, (int)yM.first);
              a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;
            }

            // --- center Pixels ---
            mul = xM.midMul * yM.midMul;
            for (uint cy = yM.first + 1; cy < yM.last; cy++)
            {
              for (uint cx = xM.first + 1; cx < xM.last; cx++)
              {
                pixel = bitmap.GetPixelUnsafe64((int)cx, (int)cy);
                a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;
              }
            }

            // --- bottom row ---
            mul = xM.midMul * yM.lastMul;
            for (uint cx = xM.first + 1; cx < xM.last; cx++)
            {
              pixel = bitmap.GetPixelUnsafe64((int)cx, (int)yM.last);
              a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;
            }

            // --- right top ---
            pixel = bitmap.GetPixelUnsafe64((int)xM.last, (int)yM.first);
            mul = xM.lastMul * yM.firstMul;
            a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;

            // --- right column ---
            mul = xM.lastMul * yM.midMul;
            for (uint cy = yM.first + 1; cy < yM.last; cy++)
            {
              pixel = bitmap.GetPixelUnsafe64((int)xM.last, (int)cy);
              a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;
            }

            // --- right bottom ---
            pixel = bitmap.GetPixelUnsafe64((int)xM.last, (int)yM.last);
            mul = xM.lastMul * yM.lastMul;
            a += (pixel >> 48) * mul; r += (pixel >> 32 & 0xffff) * mul; g += (pixel >> 16 & 0xffff) * mul; b += (pixel & 0xffff) * mul;

            // --- set pixel ---
            result.SetPixelUnsafe(x, y, a >> LinearPixel.PrecessionShift * 2 << 48
                                      | r >> LinearPixel.PrecessionShift * 2 << 32
                                      | g >> LinearPixel.PrecessionShift * 2 << 16
                                      | b >> LinearPixel.PrecessionShift * 2);
          }
        }
      }
      else
      {
        for (int y = 0; y < newHeight; y++)
        {
          var yM = yMap[y];
          for (int x = 0; x < newWidth; x++)
          {
            var xM = xMap[x];
            uint pixel, mul;
            ulong a = 0, r = 0, g = 0, b = 0;

            // --- top left ---
            pixel = bitmap.GetPixelUnsafe32((int)xM.first, (int)yM.first);
            mul = xM.firstMul * yM.firstMul;
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

            // --- left column ---
            mul = xM.firstMul * yM.midMul;
            for (uint cy = yM.first + 1; cy < yM.last; cy++)
            {
              pixel = bitmap.GetPixelUnsafe32((int)xM.first, (int)cy);
              a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
            }

            // --- bottom left ---
            pixel = bitmap.GetPixelUnsafe32((int)xM.first, (int)yM.last);
            mul = xM.firstMul * yM.lastMul;
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

            // --- top row ---
            mul = xM.midMul * yM.firstMul;
            for (uint cx = xM.first + 1; cx < xM.last; cx++)
            {
              pixel = bitmap.GetPixelUnsafe32((int)cx, (int)yM.first);
              a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
            }

            // --- center Pixels ---
            mul = xM.midMul * yM.midMul;
            for (uint cy = yM.first + 1; cy < yM.last; cy++)
            {
              for (uint cx = xM.first + 1; cx < xM.last; cx++)
              {
                pixel = bitmap.GetPixelUnsafe32((int)cx, (int)cy);
                a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
              }
            }

            // --- bottom row ---
            mul = xM.midMul * yM.lastMul;
            for (uint cx = xM.first + 1; cx < xM.last; cx++)
            {
              pixel = bitmap.GetPixelUnsafe32((int)cx, (int)yM.last);
              a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
            }

            // --- right top ---
            pixel = bitmap.GetPixelUnsafe32((int)xM.last, (int)yM.first);
            mul = xM.lastMul * yM.firstMul;
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

            // --- right column ---
            mul = xM.lastMul * yM.midMul;
            for (uint cy = yM.first + 1; cy < yM.last; cy++)
            {
              pixel = bitmap.GetPixelUnsafe32((int)xM.last, (int)cy);
              a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
            }

            // --- right bottom ---
            pixel = bitmap.GetPixelUnsafe32((int)xM.last, (int)yM.last);
            mul = xM.lastMul * yM.lastMul;
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

            // --- set pixel ---
            result.SetPixelUnsafe(x, y, (uint)(a >> LinearPixel.PrecessionShift * 2 << 24
                                             | r >> LinearPixel.PrecessionShift * 2 << 16
                                             | g >> LinearPixel.PrecessionShift * 2 << 8
                                             | b >> LinearPixel.PrecessionShift * 2));
          }
        }
      }

      return result;
    }
  }
}
