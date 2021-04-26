#region # using *.*
using System;
using System.Diagnostics;
using System.Drawing;
using FastBitmapLib.Extras;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantIfElseBlock
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable ClassCanBeSealed.Global
#endregion

namespace FastBitmapLib
{
  /// <summary>
  /// Compressed-Version of FastBitmap
  /// </summary>
  public unsafe class CompressedBitmap64 : IFastBitmap64
  {
    readonly MiniMemoryManager mem;
    readonly MiniMemoryManager.Entry[] memIndex;

    #region # // --- Constructors ---
    /// <summary>
    /// Max width of the Bitmap
    /// </summary>
    public override int MaxSize { get { return 100000000; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedBitmap64(int width, int height, ulong backgroundColor = 0x0000000000000000)
      : base(width, height, backgroundColor)
    {
      mem = new MiniMemoryManager();
      memIndex = new MiniMemoryManager.Entry[height + 1];
      memIndex[height] = mem.Alloc((ulong)(uint)width * sizeof(ulong) * 2 + 8 + (uint)width / 2048); // last line = raw-cache + comp-cache + comp-overhead

      fixed (byte* rawPixelPtr = &mem.data[memIndex[height].ofs])
      {
        for (uint i = 0; i < width; i++) ((ulong*)rawPixelPtr)[i] = backgroundColor;
      }

      uint compressedSize = CompressLine();

      fixed (byte* compPtr = &mem.data[memIndex[height].ofs + (uint)width * sizeof(ulong)])
      {
        for (int y = 0; y < height; y++)
        {
          var entry = mem.Alloc(compressedSize);
          memIndex[y] = entry;
          for (uint i = 0; i < compressedSize; i++)
          {
            mem.data[entry.ofs + i] = compPtr[i];
          }
        }
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedBitmap64(IFastBitmap bitmap, ulong backgroundColor = 0x0000000000000000)
      : this(bitmap.width, bitmap.height, backgroundColor)
    {
      CopyFromBitmap(bitmap);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedBitmap64(Bitmap bitmap, ulong backgroundColor = 0x0000000000000000)
      : this(bitmap.Width, bitmap.Height, backgroundColor)
    {
      CopyFromGDIBitmap(bitmap);
    }
    #endregion

    #region # // --- CompressLine() ---
    void SearchRepeats(ulong* pixelPtr, uint pixelStart, out uint sameAlpha, out uint sameColor)
    {
      ulong currentPixel = pixelPtr[pixelStart - 1];
      uint nextPixel;
      for (nextPixel = pixelStart; nextPixel < width && pixelPtr[nextPixel] == currentPixel; nextPixel++) { }

      if (nextPixel < width && (pixelPtr[nextPixel] & 0xffff000000000000) == (currentPixel & 0xffff000000000000))
      {
        sameColor = nextPixel - pixelStart;
        for (nextPixel++; nextPixel < width && (pixelPtr[nextPixel] & 0xffff000000000000) == (currentPixel & 0xffff000000000000); nextPixel++) { }
        sameAlpha = nextPixel - pixelStart;
      }
      else
      {
        sameAlpha = nextPixel - pixelStart;
        for (; nextPixel < width && (pixelPtr[nextPixel] & 0xffffffffffff) == (currentPixel & 0xffffffffffff); nextPixel++) { }
        sameColor = nextPixel - pixelStart;
      }
    }

    uint SearchNextDuplicateAlpha(ulong* pixelPtr, uint pixelStart)
    {
      pixelPtr += pixelStart;
      for (uint pos = 1; pixelStart + pos < width; pos++)
      {
        if ((pixelPtr[pos - 1] & 0xffff000000000000) == (pixelPtr[pos] & 0xffff000000000000)) return pos - 1;
      }
      return uint.MaxValue;
    }

    uint SearchNextDuplicateColor(ulong* pixelPtr, uint pixelStart)
    {
      pixelPtr += pixelStart;
      for (uint pos = 1; pixelStart + pos < width; pos++)
      {
        if ((pixelPtr[pos - 1] & 0xffffffffffff) == (pixelPtr[pos] & 0xffffffffffff)) return pos - 1;
      }
      return uint.MaxValue;
    }

    static uint PackValue(byte* compPtr, ulong* pixelPtr, uint count, uint sameAlpha, uint sameColor)
    {
      uint p = UnsafeHelper.WritePacketInt(compPtr, count << 2 | sameAlpha << 1 | sameColor);
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
            ((ulong*)compPtr)[i] = pixelPtr[i]; // copy full compressed pixel
          }
          return p + count * sizeof(ulong);
        }
      }
      else
      {
        if (sameAlpha == 1)
        {
          // --- sameAlpha = true, sameColor = false ---
          for (int i = 0; i < count; i++, compPtr += 6)
          {
            *(ulong*)compPtr = pixelPtr[i]; // copy 6 color-bytes (ignore alpha)
          }
          return p + count * 6;
        }
        else
        {
          // --- sameAlpha = false, sameColor = true ---
          for (int i = 0; i < count; i++)
          {
            ((ushort*)compPtr)[i] = (ushort)(pixelPtr[i] >> 48);
          }
          return p + count * sizeof(ushort);
        }
      }
    }

    uint CompressLine()
    {
      uint p = 0;

      fixed (byte* ptr = &mem.data[memIndex[height].ofs])
      {
        ulong* rawPixels = (ulong*)ptr;
        byte* compPtr = (byte*)&rawPixels[width];

        for (uint pixelCount = 0; pixelCount < width; )
        {
          *(ulong*)(compPtr + p) = rawPixels[pixelCount++]; p += sizeof(ulong);
          uint sameAlpha, sameColor;
          SearchRepeats(rawPixels, pixelCount, out sameAlpha, out sameColor);

          uint nextRepeatedAlpha = SearchNextDuplicateAlpha(rawPixels, pixelCount + sameAlpha);
          uint nextRepeatedColor = SearchNextDuplicateColor(rawPixels, pixelCount + sameColor);

          if (nextRepeatedAlpha == uint.MaxValue) nextRepeatedAlpha = (uint)width - pixelCount;
          if (nextRepeatedColor == uint.MaxValue) nextRepeatedColor = (uint)width - pixelCount;

          if (sameAlpha == 0 && sameColor == 0) // uncompressable?
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
          if (sameAlpha == 0) // only same color
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

      Debug.Assert((uint)width * sizeof(ulong) + p < memIndex[height].len);
      return p;
    }
    #endregion

    #region # // --- DecompressLine() ---
    static uint UnpackValue(ulong currentPixel, byte* compPtr, ulong* pixelPtr, out uint count)
    {
      ulong val;
      uint p = UnsafeHelper.ReadPacketInt(compPtr, out val);
      compPtr += p;
      count = (uint)(val >> 2);

      switch (val & 0x3)
      {
        case 0x0: // uncompressed
        {
          for (uint i = 0; i < count; i++)
          {
            pixelPtr[i] = ((ulong*)compPtr)[i];
          }
          return p + count * sizeof(ulong);
        }
        case 0x1: // repeated color
        {
          ulong color = currentPixel & 0xffffffffffff;
          for (uint i = 0; i < count; i++)
          {
            pixelPtr[i] = (ulong)((ushort*)compPtr)[i] << 48 | color;
          }
          return p + count * sizeof(ushort);
        }
        case 0x2: // repeated alpha
        {
          ulong alpha = currentPixel & 0xffff000000000000;
          for (uint i = 0; i < count; i++, compPtr += 6)
          {
            pixelPtr[i] = *(ulong*)compPtr & 0xffffffffffff | alpha;
          }
          return p + count * 6;
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

    uint DecompressLine(uint ofs)
    {
      uint p = 0;

      fixed (byte* ptr = &mem.data[memIndex[height].ofs], compPtr = &mem.data[ofs])
      {
        ulong* rawPixels = (ulong*)ptr;

        for (uint pixelCount = 0; pixelCount < width; )
        {
          ulong currentPixel = *(ulong*)(compPtr + p); p += sizeof(ulong);
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

    #region # // --- Helper Methods ---
    void CopyCompCacheTo(ulong ofs, ulong len)
    {
      ulong srcOfs = memIndex[height].ofs + (uint)width * sizeof(ulong);
      fixed (byte* compPtr = &mem.data[srcOfs], destPtr = &mem.data[ofs])
      {
        for (uint i = 0; i < len; i++)
        {
          destPtr[i] = compPtr[i];
        }
      }
    }

    int lastCacheLine = -1;
    ulong SetCacheLine(int y)
    {
      if (lastCacheLine == y) return memIndex[height].ofs;

      if (lastCacheLine >= 0)
      {
        uint newLen = CompressLine();
        var entry = mem.Resize(memIndex[lastCacheLine], newLen);
        memIndex[lastCacheLine] = entry;
        CopyCompCacheTo(entry.ofs, entry.len);
        if (mem.dataFragmented > mem.dataFilled / 4)
        {
          mem.Optimize(memIndex, false);
        }
      }

      lastCacheLine = y;
      DecompressLine((uint)memIndex[y].ofs);
      return memIndex[height].ofs;
    }
    #endregion

    #region # // --- Additional Compressed Features ---
    /// <summary>
    /// optimize the data structure
    /// </summary>
    /// <param name="full">true = full optimize (smallest version), false = quick optimize</param>
    public void Optimize(bool full)
    {
      if (full)
      {
        ulong totalSize = 0;
        foreach (var index in memIndex)
        {
          totalSize += index.len;
        }
        var newMem = new MiniMemoryManager(totalSize);
        var newMemIndex = new MiniMemoryManager.Entry[height + 1];
        for (int y = 0; y <= height; y++)
        {
          newMemIndex[y] = newMem.Alloc(memIndex[y].len);
          Debug.Assert(newMemIndex[y].len == memIndex[y].len);
          Array.Copy(mem.data, (int)memIndex[y].ofs, newMem.data, (int)newMemIndex[y].ofs, (int)memIndex[y].len);
        }
        for (int i = 0; i < memIndex.Length; i++) memIndex[i] = newMemIndex[i];
        mem.OverwriteManager(newMem);
        Debug.Assert(newMem.dataFilled == totalSize);
        Debug.Assert(newMem.dataSize == totalSize);
      }
      else
      {
        if (mem.dataFragmented > 0) mem.Optimize(memIndex);
      }
    }

    /// <summary>
    /// reserved memory in bytes
    /// </summary>
    public ulong CompressedSizeReserved
    {
      get
      {
        return (ulong)mem.data.Length + (ulong)memIndex.Length * (ulong)sizeof(MiniMemoryManager.Entry);
      }
    }

    /// <summary>
    /// used memory in bytes
    /// </summary>
    public ulong CompressedSizeUsed
    {
      get
      {
        return mem.dataFilled + (ulong)memIndex.Length * (ulong)sizeof(MiniMemoryManager.Entry);
      }
    }

    /// <summary>
    /// calculated uncompressed size
    /// </summary>
    public ulong UncompressedSize
    {
      get
      {
        return (ulong)width * (ulong)height * sizeof(ulong);
      }
    }
    #endregion

    #region # // --- IFastBitmap64 ---
    /// <summary>
    /// Set the pixel color at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public override void SetPixelUnsafe(int x, int y, ulong color)
    {
      ulong ofs = SetCacheLine(y);
      fixed (byte* ptr = &mem.data[ofs])
      {
        ((ulong*)ptr)[x] = color;
      }
    }

    /// <summary>
    /// Get the pixel color from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public override ulong GetPixelUnsafe64(int x, int y)
    {
      ulong ofs = SetCacheLine(y);
      fixed (byte* ptr = &mem.data[ofs])
      {
        return ((ulong*)ptr)[x];
      }
    }

    /// <summary>
    /// Fill the Scanline with a specific color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public override void FillScanlineUnsafe(int x, int y, int w, ulong color)
    {
      ulong ofs = SetCacheLine(y) + (uint)x * sizeof(ulong);
      fixed (byte* ptr = &mem.data[ofs])
      {
        for (uint i = 0; i < w; i++)
        {
          ((ulong*)ptr)[i] = color;
        }
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors. (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLineUnsafe(int x, int y, int w, ulong* srcPixels)
    {
      ulong ofs = SetCacheLine(y) + (uint)x * sizeof(ulong);
      fixed (byte* ptr = &mem.data[ofs])
      {
        for (uint i = 0; i < w; i++)
        {
          ((ulong*)ptr)[i] = srcPixels[i];
        }
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color32 (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLineUnsafe(int x, int y, int w, ulong* destPixels)
    {
      ulong ofs = SetCacheLine(y) + (uint)x * sizeof(ulong);
      fixed (byte* ptr = &mem.data[ofs])
      {
        for (uint i = 0; i < w; i++)
        {
          destPixels[i] = ((ulong*)ptr)[i];
        }
      }
    }

    /// <summary>
    /// Release unmanaged ressources
    /// </summary>
    public override void Dispose() { }
    #endregion
  }
}
