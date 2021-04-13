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
    /// Create a resized bitmap with Cleartype sub-pixel reduction (only useful when downsizing images, only 32-Bit pictures supported)
    /// </summary>
    /// <param name="bitmap">FastBitmap</param>
    /// <param name="newWidth">Width of the new picture</param>
    /// <param name="newHeight">Height of the nw picture</param>
    /// <param name="clearLevel">Cleartype-Level (0 = low, 5 = high)</param>
    /// <param name="constructor">Optional: constructor for the new FastBitmap</param>
    /// <returns>Resized bitmap</returns>
    public static IFastBitmap GetResizedClear(this IFastBitmap bitmap, int newWidth, int newHeight, int clearLevel, Func<int, int, IFastBitmap> constructor = null)
    {
      if (newWidth <= 0 && newHeight <= 0) throw new ArgumentException();

      if (newWidth <= 0) newWidth = newHeight * bitmap.width / bitmap.height;
      if (newHeight <= 0) newHeight = newWidth * bitmap.height / bitmap.width;

      if (constructor == null)
      {
        constructor = (w, h) => new FastBitmap(w, h);
      }

      var result = constructor(newWidth, newHeight);
      if (result == null) throw new ArgumentNullException("constructor");
      if (result.width != newWidth || result.height != newHeight) throw new ArgumentOutOfRangeException("constructor");

      newWidth *= 3;
      var xMap = LinearPixel.GenerateMapping(bitmap.width, newWidth);
      var yMap = LinearPixel.GenerateMapping(bitmap.height, newHeight);

      var clearPixels = new uint[3];

      uint clearFull, clearHalf;
      const int clearShift = 5; // div 32

      switch (clearLevel)
      {
        default: clearFull = 12; break;
        case 1: clearFull = 16; break;
        case 2: clearFull = 20; break;
        case 3: clearFull = 24; break;
        case 4: clearFull = 28; break;
        case 5: clearFull = 32; break;
      }
      clearHalf = (32 - clearFull) / 2;

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
          clearPixels[x % 3] = (uint)(a >> LinearPixel.PrecessionShift * 2 << 24
                                    | r >> LinearPixel.PrecessionShift * 2 << 16
                                    | g >> LinearPixel.PrecessionShift * 2 << 8
                                    | b >> LinearPixel.PrecessionShift * 2);

          if (x % 3 == 2) // last of three clear Pixels?
          {
            uint pr = clearPixels[0];
            uint pg = clearPixels[1];
            uint pb = clearPixels[2];

            uint oa = ((pr >> 24) + (pg >> 24) + (pb >> 24)) / 3;
            uint or = ((pr >> 16 & 0xff) * clearFull + (pg >> 16 & 0xff) * clearHalf + (pb >> 16 & 0xff) * clearHalf) >> clearShift;
            uint og = ((pr >> 8 & 0xff) * clearHalf + (pg >> 8 & 0xff) * clearFull + (pb >> 8 & 0xff) * clearHalf) >> clearShift;
            uint ob = ((pr & 0xff) * clearHalf + (pg & 0xff) * clearHalf + (pb & 0xff) * clearFull) >> clearShift;

            result.SetPixelUnsafe(x / 3, y, oa << 24 | or << 16 | og << 8 | ob);
          }
        }
      }

      return result;
    }
  }
}
