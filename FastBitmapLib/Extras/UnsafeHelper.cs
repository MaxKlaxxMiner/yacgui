using System.Diagnostics;
using System.Runtime.CompilerServices;
// ReSharper disable RedundantUnsafeContext
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

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
  }
}
