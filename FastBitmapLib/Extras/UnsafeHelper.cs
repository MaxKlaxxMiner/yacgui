using System.Diagnostics;
using System.Runtime.CompilerServices;
// ReSharper disable RedundantUnsafeContext
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib.Extras
{
  public static unsafe class UnsafeHelper
  {
    //[SuppressUnmanagedCodeSecurity]
    //[DllImport("msvcrt.dll", SetLastError = false)]
    //public static extern void memcpy(long dest, long src, int count);

    //[SuppressUnmanagedCodeSecurity]
    //[DllImport("kernel32.dll")]
    //public static extern void CopyMemory(long destination, long source, int length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MemCopyForward(long dest, long src, int count)
    {
      Debug.Assert(dest <= src);

      uint to = (uint)count >> 3;
      for (uint i = 0; i < to; i++)
      {
        *((ulong*)dest + i) = *((ulong*)src + i);
      }
      uint ofs = to << 3;
      for (; ofs < (uint)count; ofs++)
      {
        *((byte*)dest + ofs) = *((byte*)src + ofs);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MemCopyBackward(long dest, long src, int count)
    {
      Debug.Assert(dest >= src);

      uint ofs;
      for (ofs = (uint)count - 1; ofs < (uint)int.MaxValue && (ofs & 0x7) > 0; ofs--)
      {
        *((byte*)dest + ofs) = *((byte*)src + ofs);
      }
      for (uint i = (uint)((int)ofs >> 3); i < (uint)int.MaxValue; i--)
      {
        *((ulong*)dest + i) = *((ulong*)src + i);
      }
    }

    public static void MemCopy(long dest, long src, int count)
    {
      if (dest <= src)
      {
        MemCopyForward(dest, src, count);
      }
      else
      {
        MemCopyBackward(dest, src, count);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint WritePacketInt(byte* ptr, ulong val)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadPacketInt(byte* ptr, out ulong val)
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
  }
}
