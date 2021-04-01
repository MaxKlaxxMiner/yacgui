using System;
// ReSharper disable UnusedMember.Global
// ReSharper disable JoinDeclarationAndInitializer

namespace FastBitmapLib
{
  /// <summary>
  /// Fast class to create and draw pictures
  /// </summary>
  public partial class FastBitmap
  {
    /// <summary>
    /// Create a resized bitmap (simple nearest pixel)
    /// </summary>
    /// <param name="newWidth">Width of the new picture</param>
    /// <param name="newHeight">Height of the nw picture</param>
    /// <returns>Resized bitmap</returns>
    public FastBitmap GetResizedSimple(int newWidth, int newHeight)
    {
      if (newWidth <= 0 && newHeight <= 0) throw new ArgumentException();

      if (newWidth <= 0) newWidth = newHeight * width / height;
      if (newHeight <= 0) newHeight = newWidth * height / width;

      var result = new FastBitmap(newWidth, newHeight);

      int cxStep = 256 * width / newWidth;
      for (int y = 0; y < newHeight; y++)
      {
        int cy = y * height / newHeight;
        for (int x = 0, cx = 0; x < newWidth; x++, cx += cxStep)
        {
          result.SetPixel(x, y, GetPixel(cx >> 8, cy));
        }
      }

      return result;
    }

    /// <summary>
    /// Create a resized bitmap (bilinear interpolation)
    /// </summary>
    /// <param name="newWidth">Width of the new picture</param>
    /// <param name="newHeight">Height of the nw picture</param>
    /// <returns>Resized bitmap</returns>
    public FastBitmap GetResizedHigh(int newWidth, int newHeight)
    {
      if (newWidth <= 0 && newHeight <= 0) throw new ArgumentException();

      if (newWidth <= 0) newWidth = newHeight * width / height;
      if (newHeight <= 0) newHeight = newWidth * height / width;

      var result = new FastBitmap(newWidth, newHeight);

      var xMap = LinearPixel.GenerateMapping(width, newWidth);
      var yMap = LinearPixel.GenerateMapping(height, newHeight);

      for (int y = 0; y < newHeight; y++)
      {
        var yM = yMap[y];
        for (int x = 0; x < newWidth; x++)
        {
          var xM = xMap[x];
          uint pixel, mul;
          ulong a = 0, r = 0, g = 0, b = 0;

          // --- top left ---
          pixel = GetPixel((int)xM.first, (int)yM.first);
          mul = xM.firstMul * yM.firstMul;
          a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

          // --- left column ---
          mul = xM.firstMul * yM.midMul;
          for (uint cy = yM.first + 1; cy < yM.last; cy++)
          {
            pixel = GetPixel((int)xM.first, (int)cy);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- bottom left ---
          pixel = GetPixel((int)xM.first, (int)yM.last);
          mul = xM.firstMul * yM.lastMul;
          a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

          // --- top row ---
          mul = xM.midMul * yM.firstMul;
          for (uint cx = xM.first + 1; cx < xM.last; cx++)
          {
            pixel = GetPixel((int)cx, (int)yM.first);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- center Pixels ---
          mul = xM.midMul * yM.midMul;
          for (uint cy = yM.first + 1; cy < yM.last; cy++)
          {
            for (uint cx = xM.first + 1; cx < xM.last; cx++)
            {
              pixel = GetPixel((int)cx, (int)cy);
              a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
            }
          }

          // --- bottom row ---
          mul = xM.midMul * yM.lastMul;
          for (uint cx = xM.first + 1; cx < xM.last; cx++)
          {
            pixel = GetPixel((int)cx, (int)yM.last);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- right top ---
          pixel = GetPixel((int)xM.last, (int)yM.first);
          mul = xM.lastMul * yM.firstMul;
          a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

          // --- right column ---
          mul = xM.lastMul * yM.midMul;
          for (uint cy = yM.first + 1; cy < yM.last; cy++)
          {
            pixel = GetPixel((int)xM.last, (int)cy);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- right bottom ---
          pixel = GetPixel((int)xM.last, (int)yM.last);
          mul = xM.lastMul * yM.lastMul;
          a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

          // --- set pixel ---
          result.SetPixel(x, y, (uint)(a >> LinearPixel.PrecessionShift * 2 << 24
                                       | r >> LinearPixel.PrecessionShift * 2 << 16
                                       | g >> LinearPixel.PrecessionShift * 2 << 8
                                       | b >> LinearPixel.PrecessionShift * 2));
        }
      }

      return result;
    }

    /// <summary>
    /// Create a resized bitmap with Cleartype sub-pixel reduction (only useful when downsizing images)
    /// </summary>
    /// <param name="newWidth">Width of the new picture</param>
    /// <param name="newHeight">Height of the nw picture</param>
    /// <param name="clearLevel">Cleartype-Level (0 = low, 5 = high)</param>
    /// <returns>Resized bitmap</returns>
    public FastBitmap GetResizedClear(int newWidth, int newHeight, int clearLevel)
    {
      if (newWidth <= 0 && newHeight <= 0) throw new ArgumentException();

      if (newWidth <= 0) newWidth = newHeight * width / height;
      if (newHeight <= 0) newHeight = newWidth * height / width;

      var result = new FastBitmap(newWidth, newHeight);

      newWidth *= 3;
      var xMap = LinearPixel.GenerateMapping(width, newWidth);
      var yMap = LinearPixel.GenerateMapping(height, newHeight);

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
          pixel = GetPixel((int)xM.first, (int)yM.first);
          mul = xM.firstMul * yM.firstMul;
          a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

          // --- left column ---
          mul = xM.firstMul * yM.midMul;
          for (uint cy = yM.first + 1; cy < yM.last; cy++)
          {
            pixel = GetPixel((int)xM.first, (int)cy);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- bottom left ---
          pixel = GetPixel((int)xM.first, (int)yM.last);
          mul = xM.firstMul * yM.lastMul;
          a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

          // --- top row ---
          mul = xM.midMul * yM.firstMul;
          for (uint cx = xM.first + 1; cx < xM.last; cx++)
          {
            pixel = GetPixel((int)cx, (int)yM.first);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- center Pixels ---
          mul = xM.midMul * yM.midMul;
          for (uint cy = yM.first + 1; cy < yM.last; cy++)
          {
            for (uint cx = xM.first + 1; cx < xM.last; cx++)
            {
              pixel = GetPixel((int)cx, (int)cy);
              a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
            }
          }

          // --- bottom row ---
          mul = xM.midMul * yM.lastMul;
          for (uint cx = xM.first + 1; cx < xM.last; cx++)
          {
            pixel = GetPixel((int)cx, (int)yM.last);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- right top ---
          pixel = GetPixel((int)xM.last, (int)yM.first);
          mul = xM.lastMul * yM.firstMul;
          a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;

          // --- right column ---
          mul = xM.lastMul * yM.midMul;
          for (uint cy = yM.first + 1; cy < yM.last; cy++)
          {
            pixel = GetPixel((int)xM.last, (int)cy);
            a += (pixel >> 24) * mul; r += (pixel >> 16 & 0xff) * mul; g += (pixel >> 8 & 0xff) * mul; b += (pixel & 0xff) * mul;
          }

          // --- right bottom ---
          pixel = GetPixel((int)xM.last, (int)yM.last);
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

            result.SetPixel(x / 3, y, oa << 24 | or << 16 | og << 8 | ob);
          }
        }
      }

      return result;
    }
  }
}
