using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Tiff File Encoder
  /// </summary>
  public static class TiffEncoder
  {
    /// <summary>
    /// Save the bitmap as uncompressed Tiff-File
    /// </summary>
    /// <param name="bitmap">Bitmap to save</param>
    /// <param name="fileName">Write Path and Filename</param>
    /// <returns>Written Bytes</returns>
    public static int SaveTiffFile(this IFastBitmap bitmap, string fileName)
    {
      using (var wdat = File.Create(fileName))
      {
        return SaveTiffStream(bitmap, wdat);
      }
    }

    /// <summary>
    /// Write the bitmap as uncompressed Tiff-Stream
    /// </summary>
    /// <param name="bitmap">Bitmap to save</param>
    /// <param name="writeStream">Stream to write</param>
    /// <returns></returns>
    public static int SaveTiffStream(this IFastBitmap bitmap, Stream writeStream)
    {
      bool is32Bit = bitmap is IFastBitmap32;
      bool hasAlpha = bitmap.HasAlphaPixels();

      int writtenBytes = WriteHeader(writeStream, bitmap, is32Bit, hasAlpha);
      writtenBytes += WriteData(writeStream, bitmap, is32Bit, hasAlpha);

      return writtenBytes;
    }

    /// <summary>
    /// write the TIFF Header
    /// </summary>
    /// <param name="writeStream">Stream to write the Header</param>
    /// <param name="bitmap">Bitmap</param>
    /// <param name="is32Bit">has 32-Bit Pixels (false: 64-Bit)</param>
    /// <param name="hasAlpha">Bitmap use alpha channel</param>
    /// <returns>written Bytes</returns>
    static int WriteHeader(Stream writeStream, IFastBitmap bitmap, bool is32Bit, bool hasAlpha)
    {
      var h = new List<byte>();                        // collect header-bytes

      h.AddRange(Encoding.ASCII.GetBytes("II"));       // "II" - IntelInside = little endian ("MM" = Motorola = big endian)
      h.AddRange(BitConverter.GetBytes((short)42));    // TIFF Magic-Code 42
      int ifdOff = 8;
      h.AddRange(BitConverter.GetBytes(ifdOff));       // Offset of the IFD-Table

      short ifdCount = (short)(hasAlpha ? 15 : 14);    // Tag-Count
      int ifdEnd = ifdOff + 2 + ifdCount * 12 + 4;     // Size: start + count + datasets + end

      Action<List<byte>, short, short, int, int> AddIfd = (result, idfTag, valueType, count, value) =>
      {
        result.AddRange(BitConverter.GetBytes(idfTag));
        result.AddRange(BitConverter.GetBytes(valueType));
        result.AddRange(BitConverter.GetBytes(count));
        result.AddRange(BitConverter.GetBytes(value));
      };

      h.AddRange(BitConverter.GetBytes(ifdCount));     // Count of the datasets in the IFD-Table
      AddIfd(h, 0x0fe, 4, 1, 0);                       // 01: Marker: NewSubfileType
      AddIfd(h, 0x100, 3, 1, bitmap.width);            // 02: Width of the bitmap
      AddIfd(h, 0x101, 3, 1, bitmap.height);           // 03: Height of the bitmap
      AddIfd(h, 0x102, 3, (hasAlpha ? 4 : 3), ifdEnd); // 04: ref* at Count of color channels
      AddIfd(h, 0x103, 3, 1, 1);                       // 05: Compression (1 = none)
      AddIfd(h, 0x106, 3, 1, 2);                       // 06: Colormode (2 = RGB)
      AddIfd(h, 0x111, 4, 1, ifdEnd + (hasAlpha ? 4 : 3) * 2 + 16); // 07: ref* start of the pixeldata
      AddIfd(h, 0x115, 3, 1, (hasAlpha ? 4 : 3));      // 08: Values per Pixel
      AddIfd(h, 0x116, 3, 1, bitmap.height);           // 09: Count scanline
      AddIfd(h, 0x117, 4, 1, bitmap.width * bitmap.height * (is32Bit ? 1 : 2) * (hasAlpha ? 4 : 3)); // 10: Size of pixeldata in bytes
      AddIfd(h, 0x11a, 5, 1, ifdEnd + (hasAlpha ? 4 : 3) * 2);      // 11: ref* DPI horizonal
      AddIfd(h, 0x11b, 5, 1, ifdEnd + (hasAlpha ? 4 : 3) * 2 + 8);  // 12: ref* DPI vertical
      AddIfd(h, 0x11c, 3, 1, 1);                       // 13: Pixel-Style: 1 = Chunky (RGB RGB RGB), 2 = Planar (RRR GGG BBB)
      AddIfd(h, 0x128, 3, 1, 2);                       // 14: measurement of DPI (default: 2 = Inch)
      if (hasAlpha) AddIfd(h, 0x152, 3, 1, 1);         // 15: alpha-mode
      h.AddRange(new byte[4]);                         // placeholder (end marker): 2x uint16

      // --- channel table ---
      h.AddRange(BitConverter.GetBytes((short)(is32Bit ? 8 : 16)));               // bitcount for red channel
      h.AddRange(BitConverter.GetBytes((short)(is32Bit ? 8 : 16)));               // bitcount for green channel
      h.AddRange(BitConverter.GetBytes((short)(is32Bit ? 8 : 16)));               // bitcount for blue channel
      if (hasAlpha) h.AddRange(BitConverter.GetBytes((short)(is32Bit ? 8 : 16))); // bitcount for alpha channel

      h.AddRange(BitConverter.GetBytes(0x000ea57a)); h.AddRange(BitConverter.GetBytes(0x00002710)); // DPI-X
      h.AddRange(BitConverter.GetBytes(0x000ea57a)); h.AddRange(BitConverter.GetBytes(0x00002710)); // DPI-Y

      writeStream.Write(h.ToArray(), 0, h.Count);
      return h.Count;
    }

    /// <summary>
    /// write the TIFF Pixel-Data
    /// </summary>
    /// <param name="writeStream">Stream to write the Header</param>
    /// <param name="bitmap">Bitmap</param>
    /// <param name="is32Bit">has 32-Bit Pixels (false: 64-Bit)</param>
    /// <param name="hasAlpha">Bitmap use alpha channel</param>
    /// <returns>written Bytes</returns>
    static int WriteData(Stream writeStream, IFastBitmap bitmap, bool is32Bit, bool hasAlpha)
    {
      var bufferLine = new byte[bitmap.width * (is32Bit ? (hasAlpha ? 4 : 3) : (hasAlpha ? 8 : 6))];
      int writtenBytes = 0;

      if (is32Bit)
      {
        var scanLine = new uint[bitmap.width];

        for (int line = 0; line < bitmap.height; line++)
        {
          bitmap.ReadScanLine(line, scanLine);
          if (hasAlpha) // with alpha channel
          {
            for (int i = 0; i < scanLine.Length; i++)
            {
              uint pixel = scanLine[i];
              bufferLine[i * 4 + 0] = (byte)(pixel >> 16); // red
              bufferLine[i * 4 + 1] = (byte)(pixel >> 8);  // green
              bufferLine[i * 4 + 2] = (byte)pixel;         // blue
              bufferLine[i * 4 + 3] = (byte)(pixel >> 24); // alpha
            }
          }
          else // without alpha channel
          {
            for (int i = 0; i < scanLine.Length; i++)
            {
              uint pixel = scanLine[i];

              bufferLine[i * 3 + 0] = (byte)(pixel >> 16); // red
              bufferLine[i * 3 + 1] = (byte)(pixel >> 8);  // green
              bufferLine[i * 3 + 2] = (byte)pixel;         // blue
            }
          }
          writeStream.Write(bufferLine, 0, bufferLine.Length);
          writtenBytes += bufferLine.Length;
        }
      }
      else // 64-Bit Pixel
      {
        var scanLine = new ulong[bitmap.width];

        for (int line = 0; line < bitmap.height; line++)
        {
          bitmap.ReadScanLine(line, scanLine);
          if (hasAlpha) // with alpha channel
          {
            for (int i = 0; i < scanLine.Length; i++)
            {
              ulong pixel = scanLine[i];

              bufferLine[i * 8 + 0] = (byte)(pixel >> 32); // lo: red
              bufferLine[i * 8 + 1] = (byte)(pixel >> 40); // hi: red

              bufferLine[i * 8 + 2] = (byte)(pixel >> 16); // lo: green
              bufferLine[i * 8 + 3] = (byte)(pixel >> 24); // hi: green

              bufferLine[i * 8 + 4] = (byte)pixel;         // lo: blue
              bufferLine[i * 8 + 5] = (byte)(pixel >> 8);  // hi: blue

              bufferLine[i * 8 + 6] = (byte)(pixel >> 48); // lo: alpha
              bufferLine[i * 8 + 7] = (byte)(pixel >> 56); // hi: alpha
            }
          }
          else // without alpha channel
          {
            for (int i = 0; i < scanLine.Length; i++)
            {
              ulong pixel = scanLine[i];

              bufferLine[i * 6 + 0] = (byte)(pixel >> 32); // lo: red
              bufferLine[i * 6 + 1] = (byte)(pixel >> 40); // hi: red

              bufferLine[i * 6 + 2] = (byte)(pixel >> 16); // lo: green
              bufferLine[i * 6 + 3] = (byte)(pixel >> 24); // hi: green

              bufferLine[i * 6 + 4] = (byte)pixel;         // lo: blue
              bufferLine[i * 6 + 5] = (byte)(pixel >> 8);  // hi: blue
            }
          }
          writeStream.Write(bufferLine, 0, bufferLine.Length);
          writtenBytes += bufferLine.Length;
        }
      }

      return writtenBytes;
    }
  }
}
