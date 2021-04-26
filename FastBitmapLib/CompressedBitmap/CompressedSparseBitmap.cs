//#define ExtraDebugValidation

#region # using *.*
using System;
using System.Diagnostics;
using System.Drawing;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantIfElseBlock
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable ClassCanBeSealed.Global
#endregion

namespace FastBitmapLib
{
  /// <summary>
  /// Compressed-Version of FastBitmap for sparse filled pictures
  /// </summary>
  public unsafe class CompressedSparseBitmap : IFastBitmap32
  {
    readonly MiniMemoryManager mem;
    readonly MiniMemoryManager.Entry[] memIndex;

    #region # // --- Constructors ---
    /// <summary>
    /// Max width of the Bitmap
    /// </summary>
    public override int MaxSize { get { return 134217723; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedSparseBitmap(int width, int height, uint backgroundColor = 0x00000000)
      : base(width, height, backgroundColor)
    {
      mem = new MiniMemoryManager();
      memIndex = new MiniMemoryManager.Entry[height + 1];
      memIndex[height] = mem.Alloc((uint)width * sizeof(uint) * 2); // last line = comp-cache + comp-overhead

      uint[] emptyLine = { backgroundColor, (uint)width };
      uint compressedSize = (uint)emptyLine.Length * sizeof(uint);

      fixed (uint* ptr = emptyLine)
      {
        for (int y = 0; y < height; y++)
        {
          var entry = mem.Alloc(compressedSize);
          memIndex[y] = entry;
          for (uint i = 0; i < compressedSize; i++)
          {
            mem.data[entry.ofs + i] = ((byte*)ptr)[i];
          }
        }
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedSparseBitmap(IFastBitmap bitmap, uint backgroundColor = 0x00000000)
      : this(bitmap.width, bitmap.height, backgroundColor)
    {
      CopyFromBitmap(bitmap);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedSparseBitmap(Bitmap bitmap, uint backgroundColor = 0x00000000)
      : this(bitmap.Width, bitmap.Height, backgroundColor)
    {
      CopyFromGDIBitmap(bitmap);
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
        return (ulong)width * (ulong)height * sizeof(uint);
      }
    }
    #endregion

    #region # // --- Helper methods ---
    void CopyComp(byte* srcPtr, ulong destOfs, ulong len)
    {
      Debug.Assert(len % 8 == 0);

      fixed (byte* destPtr = &mem.data[destOfs])
      {
        len >>= 3;
        for (uint i = 0; i < len; i++)
        {
          *((ulong*)destPtr + i) = *((ulong*)srcPtr + i);
        }

#if ExtraDebugValidation && DEBUG
        uint srcP = 0;
        uint cx = 0;
        for (; cx < width; )
        {
          srcP += sizeof(uint); // skip color
          uint srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          cx += srcCount;
          Debug.Assert(cx <= width);
        }
        Debug.Assert(cx == width);
        Debug.Assert(srcP == len * 8);
#endif
      }
    }
    #endregion

    #region # // --- IFastBitmap32 ---
    /// <summary>
    /// Set the pixel color at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public override void SetPixelUnsafe(int x, int y, uint color)
    {
      fixed (byte* srcPtr = &mem.data[memIndex[y].ofs], destPtr = &mem.data[memIndex[height].ofs])
      {
        uint srcP = 0;
        uint srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
        uint srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
        uint destP = 0;

        uint cx = 0;

        // --- copy start ---
        for (; cx < x; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          uint copyCount = Math.Min((uint)x - cx, srcCount);
          *(uint*)(destPtr + destP) = srcColor; destP += sizeof(uint);
          *(uint*)(destPtr + destP) = copyCount; destP += sizeof(uint);
          cx += copyCount;
          srcCount -= copyCount;
        }

        // --- set middle ---
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          cx++;
          srcCount--;
          *(uint*)(destPtr + destP) = color; destP += sizeof(uint);
          *(uint*)(destPtr + destP) = 1; destP += sizeof(uint);
        }

        // --- copy end ---
        for (; cx < width; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          *(uint*)(destPtr + destP) = srcColor; destP += sizeof(uint);
          *(uint*)(destPtr + destP) = srcCount; destP += sizeof(uint);
          cx += srcCount;
          srcCount = 0;
        }
        Debug.Assert(srcCount == 0);

        var newMem = mem.Resize(memIndex[y], destP);
        memIndex[y] = newMem;
        CopyComp(destPtr, newMem.ofs, newMem.len);
      }
    }

    /// <summary>
    /// Get the pixel color from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public override uint GetPixelUnsafe32(int x, int y)
    {
      fixed (byte* srcPtr = &mem.data[memIndex[y].ofs])
      {
        uint srcP = 0;
        uint cx = 0;

        // --- decode ---
        for (; ; )
        {
          uint srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          uint srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          cx += srcCount;
          if (cx > x) return srcColor;
          Debug.Assert(cx < width);
        }
      }
    }

    /// <summary>
    /// Fill the Scanline with a specific color (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="color">fill-color</param>
    public override void FillScanlineUnsafe(int x, int y, int w, uint color)
    {
      fixed (byte* srcPtr = &mem.data[memIndex[y].ofs], destPtr = &mem.data[memIndex[height].ofs])
      {
        uint srcP = 0;
        uint srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
        uint srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
        uint destP = 0;

        uint cx = 0;

        // --- copy start ---
        for (; cx < x; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          uint copyCount = Math.Min((uint)x - cx, srcCount);
          *(uint*)(destPtr + destP) = srcColor; destP += sizeof(uint);
          *(uint*)(destPtr + destP) = copyCount; destP += sizeof(uint);
          cx += copyCount;
          srcCount -= copyCount;
        }

        // --- set middle ---
        *(uint*)(destPtr + destP) = color; destP += sizeof(uint);
        *(uint*)(destPtr + destP) = (uint)w; destP += sizeof(uint);
        x += w;
        for (; cx < x; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          uint stepX = Math.Min((uint)x - cx, srcCount);
          cx += stepX;
          srcCount -= stepX;
        }

        // --- copy end ---
        for (; cx < width; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          *(uint*)(destPtr + destP) = srcColor; destP += sizeof(uint);
          *(uint*)(destPtr + destP) = srcCount; destP += sizeof(uint);
          cx += srcCount;
          srcCount = 0;
        }
        Debug.Assert(srcCount == 0);

        var newMem = mem.Resize(memIndex[y], destP);
        memIndex[y] = newMem;
        CopyComp(destPtr, newMem.ofs, newMem.len);
      }
    }

    /// <summary>
    /// Writes a Scanline with a array of specific colors. (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="srcPixels">Pointer at Source array of pixels</param>
    public override void WriteScanLineUnsafe(int x, int y, int w, uint* srcPixels)
    {
      fixed (byte* srcPtr = &mem.data[memIndex[y].ofs], destPtr = &mem.data[memIndex[height].ofs])
      {
        uint srcP = 0;
        uint srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
        uint srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
        uint destP = 0;

        uint cx = 0;

        // --- copy start ---
        for (; cx < x; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          uint copyCount = Math.Min((uint)x - cx, srcCount);
          *(uint*)(destPtr + destP) = srcColor; destP += sizeof(uint);
          *(uint*)(destPtr + destP) = copyCount; destP += sizeof(uint);
          cx += copyCount;
          srcCount -= copyCount;
        }

        // --- set middle ---
        Debug.Assert(w > 0);
        x += w;
        uint color = *srcPixels++;
        *(uint*)(destPtr + destP) = color; destP += sizeof(uint);
        *(uint*)(destPtr + destP) = 1; destP += sizeof(uint);
        w--;
        for (; w > 0; w--)
        {
          uint color2 = *srcPixels++;
          if (color2 == color)
          {
            ((uint*)(destPtr + destP - sizeof(uint)))[0]++;
          }
          else
          {
            color = color2;
            *(uint*)(destPtr + destP) = color; destP += sizeof(uint);
            *(uint*)(destPtr + destP) = 1; destP += sizeof(uint);
          }
        }
        for (; cx < x; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          uint stepX = Math.Min((uint)x - cx, srcCount);
          cx += stepX;
          srcCount -= stepX;
        }

        // --- copy end ---
        for (; cx < width; )
        {
          if (srcCount == 0)
          {
            srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
            srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          }
          *(uint*)(destPtr + destP) = srcColor; destP += sizeof(uint);
          *(uint*)(destPtr + destP) = srcCount; destP += sizeof(uint);
          cx += srcCount;
          srcCount = 0;
        }
        Debug.Assert(srcCount == 0);

        var newMem = mem.Resize(memIndex[y], destP);
        memIndex[y] = newMem;
        CopyComp(destPtr, newMem.ofs, newMem.len);
      }
    }

    /// <summary>
    /// Read a Scanline array of pixels type: color32 (without boundary check)
    /// </summary>
    /// <param name="x">X-Start (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="w">width</param>
    /// <param name="destPixels">Pointer at Destination array to write pixels</param>
    public override void ReadScanLineUnsafe(int x, int y, int w, uint* destPixels)
    {
      fixed (byte* srcPtr = &mem.data[memIndex[y].ofs])
      {
        uint srcP = 0;
        uint cx = 0;

        // --- decode ---
        for (; ; )
        {
          uint srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          uint srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
          cx += srcCount;
          if (cx > x)
          {
            srcCount = cx - (uint)x;
            while (w > 0)
            {
              if (srcCount == 0)
              {
                srcColor = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
                srcCount = *(uint*)(srcPtr + srcP); srcP += sizeof(uint);
              }

              *destPixels++ = srcColor;
              srcCount--;
              w--;
            }
            if (w == 0) break;
          }
          Debug.Assert(cx < width);
        }
      }
    }

    /// <summary>
    /// Release unmanaged ressources
    /// </summary>
    public override void Dispose()
    {
    }
    #endregion
  }
}
