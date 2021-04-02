using System;
// ReSharper disable MemberCanBePrivate.Global

namespace FastBitmapLib.Extras
{
  public struct LinearPixel
  {
    /// <summary>
    /// Precession bit-shifting, higher = better quality, but may problematic with high size changes
    /// </summary>
    public const int PrecessionShift = 10;
    /// <summary>
    /// Multiplicator-Value
    /// </summary>
    public const uint MulMax = 1u << PrecessionShift;
    /// <summary>
    /// First pixel position
    /// </summary>
    public uint first;
    /// <summary>
    /// Fraction of the first pixel
    /// </summary>
    public uint firstMul;
    /// <summary>
    /// Fraction per center pixel (if available)
    /// </summary>
    public uint midMul;
    /// <summary>
    /// Last pixel position
    /// </summary>
    public uint last;
    /// <summary>
    /// Fraction of the last pixel
    /// </summary>
    public uint lastMul;

    /// <summary>
    /// Generate a pixel mapping table for resizing
    /// </summary>
    /// <param name="oldSize">Previous size in pixels</param>
    /// <param name="newSize">New size in pixels</param>
    /// <returns>Generated LinearPixel-Map</returns>
    public static LinearPixel[] GenerateMapping(int oldSize, int newSize)
    {
      if (oldSize < 1) throw new ArgumentOutOfRangeException("oldSize");
      if (newSize < 1) throw new ArgumentOutOfRangeException("newSize");

      var elements = new LinearPixel[newSize];
      if (newSize > oldSize) // magnify
      {
        for (uint i = 0; i < elements.Length; i++)
        {
          var el = new LinearPixel();

          ulong pos = (ulong)((uint)oldSize - 1) * i * MulMax / ((uint)newSize - 1);

          el.first = (uint)(pos >> PrecessionShift);
          el.last = el.first + 1;
          el.lastMul = (uint)(pos & (MulMax - 1));
          el.firstMul = MulMax - el.lastMul;
          el.midMul = 0;

          if (el.last >= oldSize) el.last = (uint)oldSize - 1;

          elements[i] = el;
        }
      }
      else if (newSize < oldSize) // minify
      {
        for (uint i = 0; i < elements.Length; i++)
        {
          var el = new LinearPixel();

          double posFirst = (double)oldSize * i / newSize;
          double posLast = (double)oldSize * (i + 1) / newSize;

          el.first = (uint)posFirst;
          double mulFirst = 1 - (posFirst - el.first);
          el.last = (uint)posLast;
          if (el.last == posLast) el.last--;
          if (el.last >= oldSize) el.last = (uint)oldSize - 1;

          uint midPixels = el.last - el.first - 1;
          if (midPixels == uint.MaxValue) midPixels = 0;

          double mulLast = posLast - el.last;
          double divider = 1 / (mulFirst + midPixels + mulLast);

          el.firstMul = (uint)(MulMax * (mulFirst * divider));
          el.midMul = midPixels > 0 ? (uint)(MulMax * divider) : 0;
          el.lastMul = MulMax - el.firstMul - midPixels * el.midMul;

          elements[i] = el;
        }
      }
      else // simple 1:1 mapping
      {
        for (uint i = 0; i < elements.Length; i++)
        {
          elements[i] = new LinearPixel
          {
            first = i,
            firstMul = MulMax,
            midMul = 0,
            last = i,
            lastMul = 0
          };
        }
      }

      return elements;
    }

    /// <summary>
    /// Returns the properties as a readable string.
    /// </summary>
    /// <returns>Readable string</returns>
    public override string ToString()
    {
      double firstPercent = 100.0 / MulMax * firstMul;
      int midPixels = Math.Max(0, (int)last - (int)first - 1);
      double midPercent = 100.0 / MulMax * midMul;
      double lastPercent = 100.0 / MulMax * lastMul;
      double sumPercent = firstPercent + midPixels * midPercent + lastPercent;

      if (sumPercent < 99.9 || sumPercent > 100.1) return "-";

      return new
      {
        first = first + " (" + firstPercent.ToString("N2") + "%)",
        midMul = midPixels + " * " + midPercent.ToString("N2") + "%",
        last = last + " (" + lastPercent.ToString("N2") + "%)",
        sum = sumPercent.ToString("N2") + "%"
      }.ToString();
    }
  }
}