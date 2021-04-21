// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Local

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// ReSharper disable RedundantIfElseBlock
// ReSharper disable UnusedMethodReturnValue.Local

namespace FastBitmapLib
{
  /// <summary>
  /// Compressed-Version of FastBitmap
  /// </summary>
  public unsafe class CompressedBitmap : IFastBitmapSimple32
  {
    readonly MiniMemoryManager mem;
    readonly MiniMemoryManager.Entry[] memIndex;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedBitmap(int width, int height, uint backgroundColor = 0x00000000)
      : base(width, height, backgroundColor)
    {
      mem = new MiniMemoryManager();
      memIndex = new MiniMemoryManager.Entry[height + 1];
      memIndex[height] = mem.Alloc((uint)width * sizeof(uint) * 2 + 4 + (uint)width / 2048); // last line = raw-cache + comp-cache + comp-overhead
    }

    #region # // --- CompressLine() ---
    void SearchRepeats(uint* pixelPtr, uint pixelStart, out uint sameAlpha, out uint sameColor)
    {
      uint currentPixel = pixelPtr[pixelStart - 1];
      uint nextPixel;
      for (nextPixel = pixelStart; nextPixel < width && pixelPtr[nextPixel] == currentPixel; nextPixel++) { }

      if (nextPixel < width && (pixelPtr[nextPixel] & 0xff000000) == (currentPixel & 0xff000000))
      {
        sameColor = nextPixel - pixelStart;
        for (nextPixel++; nextPixel < width && (pixelPtr[nextPixel] & 0xff000000) == (currentPixel & 0xff000000); nextPixel++) { }
        sameAlpha = nextPixel - pixelStart;
      }
      else
      {
        sameAlpha = nextPixel - pixelStart;
        for (; nextPixel < width && (pixelPtr[nextPixel] & 0xffffff) == (currentPixel & 0xffffff); nextPixel++) { }
        sameColor = nextPixel - pixelStart;
      }
    }

    uint SearchNextQuadAlpha(uint* pixelPtr, uint pixelStart)
    {
      pixelPtr += pixelStart;
      for (uint pos = 3; pixelStart + pos < width; pos++)
      {
        if ((pixelPtr[pos - 1] & 0xff000000) == (pixelPtr[pos] & 0xff000000)
         && (pixelPtr[pos - 2] & 0xff000000) == (pixelPtr[pos - 1] & 0xff000000)
         && (pixelPtr[pos - 3] & 0xff000000) == (pixelPtr[pos - 2] & 0xff000000)) return pos - 3;
      }
      return uint.MaxValue;
    }

    uint SearchNextDuplicateColor(uint* pixelPtr, uint pixelStart)
    {
      pixelPtr += pixelStart;
      for (uint pos = 1; pixelStart + pos < width; pos++)
      {
        if ((pixelPtr[pos - 1] & 0xffffff) == (pixelPtr[pos] & 0xffffff)) return pos - 1;
      }
      return uint.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint WritePacketInt(byte* ptr, ulong val)
    {
      uint p = 0;
      for (; ; )
      {
        ptr[p++] = (byte)(val & 127);
        if (val < 128) break;
        ptr[p - 1] |= 128;
        val >>= 7;
      }
      return p;
    }

    static uint PackValue(byte* compPtr, uint* pixelPtr, uint count, uint sameAlpha, uint sameColor)
    {
      uint p = WritePacketInt(compPtr, count << 2 | sameAlpha << 1 | sameColor);
      compPtr += p;

      if (sameAlpha == sameColor)
      {
        if (sameAlpha == 1)
        {
          // --- sameAlpha = true, sameColor = true ---
          return p;
        }
        else
        {
          // --- sameAlpha = false, sameColor = false ---
          for (int i = 0; i < count; i++)
          {
            ((uint*)compPtr)[i] = pixelPtr[i]; // copy full compressed pixel
          }
          return p + count * sizeof(uint);
        }
      }
      else
      {
        if (sameAlpha == 1)
        {
          // --- sameAlpha = true, sameColor = false ---
          for (int i = 0; i < count; i++, compPtr += 3)
          {
            *(uint*)compPtr = pixelPtr[i]; // copy 3 color-bytes (ignore alpha)
          }
          return p + count * 3;
        }
        else
        {
          // --- sameAlpha = false, sameColor = true ---
          for (int i = 0; i < count; i++)
          {
            compPtr[i] = (byte)(pixelPtr[i] >> 24);
          }
          return p + count;
        }
      }
    }

    uint CompressLine()
    {
      uint p = 0;

      fixed (byte* ptr = &mem.data[memIndex[height].ofs])
      {
        uint* rawPixels = (uint*)ptr;
        byte* compPtr = (byte*)&rawPixels[width];

        for (uint pixelCount = 0; pixelCount < width; )
        {
          *(uint*)(compPtr + p) = rawPixels[pixelCount++]; p += sizeof(uint);
          uint sameAlpha, sameColor;
          SearchRepeats(rawPixels, pixelCount, out sameAlpha, out sameColor);

          uint nextRepeatedAlpha = SearchNextQuadAlpha(rawPixels, pixelCount + sameAlpha);
          uint nextRepeatedColor = SearchNextDuplicateColor(rawPixels, pixelCount + sameColor);

          if (nextRepeatedAlpha == uint.MaxValue) nextRepeatedAlpha = (uint)width - pixelCount;
          if (nextRepeatedColor == uint.MaxValue) nextRepeatedColor = (uint)width - pixelCount;

          if (sameAlpha <= 2 && sameColor == 0) // uncompressable?
          {
            uint copyCount = Math.Min(nextRepeatedAlpha, nextRepeatedColor);
            p += PackValue(compPtr + p, rawPixels + pixelCount, copyCount, 0, 0);
            pixelCount += copyCount;
            continue;
          }

          if (sameColor == 0) // only same alpha
          {
            uint copyCount = Math.Min(sameAlpha, nextRepeatedColor);
            p += PackValue(compPtr + p, rawPixels + pixelCount, copyCount, 1, 0);
            pixelCount += copyCount;
            continue;
          }
          if (sameAlpha <= 2) // only same color
          {
            uint copyCount = Math.Min(sameColor, nextRepeatedAlpha);
            p += PackValue(compPtr + p, rawPixels + pixelCount, copyCount, 0, 1);
            pixelCount += copyCount;
            continue;
          }

          // same alpha and color
          {
            uint copyCount = Math.Min(sameAlpha, sameColor);
            p += PackValue(compPtr + p, rawPixels + pixelCount, copyCount, 1, 1);
            pixelCount += copyCount;
          }
        }
      }

      Debug.Assert((uint)width * sizeof(uint) + p < memIndex[height].len);
      return p;
    }
    #endregion

    #region # // --- DecompressLine() ---
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint ReadPacketInt(byte* ptr, out ulong val)
    {
      uint p = 0;
      val = ptr[p++];
      if (val > 127)
      {
        val &= 127;
        for (int bit = 7; ; bit += 7)
        {
          byte b = ptr[p++];
          val |= (ulong)(b & 127) << bit;
          if (b <= 127) break;
        }
      }
      return p;
    }

    static uint UnpackValue(uint currentPixel, byte* compPtr, uint* pixelPtr, out uint count)
    {
      ulong val;
      uint p = ReadPacketInt(compPtr, out val);
      compPtr += p;
      count = (uint)(val >> 2);

      switch (val & 0x3)
      {
        case 0x0: // uncompressed
        {
          for (uint i = 0; i < count; i++)
          {
            pixelPtr[i] = ((uint*)compPtr)[i];
          }
          return p + count * sizeof(uint);
        }
        case 0x1: // repeated color
        {
          uint color = currentPixel & 0xffffff;
          for (uint i = 0; i < count; i++)
          {
            pixelPtr[i] = (uint)compPtr[i] << 24 | color;
          }
          return p + count;
        }
        case 0x2: // repeated alpha
        {
          uint alpha = currentPixel & 0xff000000;
          for (uint i = 0; i < count; i++, compPtr += 3)
          {
            pixelPtr[i] = *(uint*)compPtr & 0xffffff | alpha;
          }
          return p + count * 3;
        }
        case 0x3: // full repeated alpha+color
        {
          for (uint i = 0; i < count; i++)
          {
            pixelPtr[i] = currentPixel;
          }
          return p;
        }
        default: return 0;
      }
    }

    uint DecompressLine()
    {
      uint p = 0;

      fixed (byte* ptr = &mem.data[memIndex[height].ofs])
      {
        uint* rawPixels = (uint*)ptr;
        byte* compPtr = (byte*)&rawPixels[width];

        for (uint pixelCount = 0; pixelCount < width; )
        {
          uint currentPixel = *(uint*)(compPtr + p); p += sizeof(uint);
          rawPixels[pixelCount++] = currentPixel;
          uint count;
          p += UnpackValue(currentPixel, compPtr + p, rawPixels + pixelCount, out count);
          pixelCount += count;
          Debug.Assert(pixelCount <= width);
        }
      }
      return p;
    }
    #endregion

    public void Test()
    {
      var rnd = new Random(12345);
      byte[] randomBytes = new byte[width * sizeof(uint)];
      rnd.NextBytes(randomBytes);

      for (int i = 0; i < randomBytes.Length; i += 4) randomBytes[i + 3] = 0xff;

      for (int r = 100; r < 130; r++) if (r % 4 != 3) randomBytes[r] = 0x00;

      Array.Copy(randomBytes, 0, mem.data, (int)memIndex[height].ofs, randomBytes.Length);

      uint compLen = CompressLine();

      Array.Clear(mem.data, (int)memIndex[height].ofs, randomBytes.Length);

      uint decompLen = DecompressLine();

      if (compLen != decompLen) throw new Exception();

      int ofs = (int)memIndex[height].ofs;
      for (int i = 0; i < randomBytes.Length; i++) if (mem.data[ofs + i] != randomBytes[i]) throw new Exception();
    }

    /// <summary>
    /// Set the pixel color at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public override void SetPixelUnsafe(int x, int y, uint color)
    {
    }

    /// <summary>
    /// Get the pixel color from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public override uint GetPixelUnsafe32(int x, int y)
    {
      return 0;
    }

    /// <summary>
    /// Release unmanaged ressources
    /// </summary>
    public override void Dispose()
    {
    }
  }
}
