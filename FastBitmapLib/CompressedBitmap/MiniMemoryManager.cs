#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UnusedType.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable RedundantIfElseBlock
#pragma warning disable 414
#endregion

namespace FastBitmapLib
{
  public class MiniMemoryManager
  {
    public struct Entry
    {
      public readonly ulong ofs;
      public readonly ulong len;
      public ulong end { get { return ofs + len; } }
      public Entry(ulong ofs, ulong len)
      {
        this.ofs = ofs;
        this.len = len;
      }
      public override string ToString()
      {
        return new { ofs, len }.ToString();
      }
    }

    public byte[] data;
    public ulong dataSize;
    public ulong dataFilled;
    public ulong dataFragmented;

    public MiniMemoryManager()
    {
      data = new byte[0];
      dataSize = 0;
      dataFilled = 0;
      dataFragmented = 0;
    }

    public MiniMemoryManager(ulong exactSize)
    {
      data = new byte[exactSize];
      dataSize = exactSize;
      dataFilled = 0;
      dataFragmented = 0;
    }

    public void SetMemoryLength(ulong minSize)
    {
      ulong targetSize = 255;
      while (targetSize < minSize)
      {
        targetSize *= 2;
      }
      if (targetSize == dataSize) return; // same size?

      if (targetSize > dataSize) // extend
      {
        ulong extend = targetSize - dataSize;
        dataSize += extend;
      }
      else // reduce
      {
        ulong reduce = dataSize - targetSize;
        if (dataSize - reduce < dataFilled) return; // can't reduce already used memory
        dataSize -= reduce;
      }
      Array.Resize(ref data, checked((int)dataSize));

      Debug.Assert(dataSize >= minSize);
      Debug.Assert(dataSize >= dataFilled);
      Debug.Assert(dataSize == (ulong)data.Length);
    }

    public Entry Alloc(ulong size)
    {
      if (size > dataSize - dataFilled)
      {
        SetMemoryLength(dataFilled + size);
        if (size > dataSize - dataFilled) throw new OutOfMemoryException();
      }

      ulong ofs = dataFilled;
      dataFilled += size;

      return new Entry(ofs, size);
    }

    public void Free(Entry entry)
    {
      if (entry.len == 0) return;

      Debug.Assert(dataFilled <= dataSize);
      Debug.Assert(entry.ofs < dataFilled);
      Debug.Assert(entry.end <= dataFilled);

      if (entry.end == dataFilled)
      {
        dataFilled -= entry.len;
      }
      else
      {
        dataFragmented += entry.len;
      }

      if (dataFragmented == dataFilled)
      {
        dataFragmented = 0;
        dataFilled = 0;
      }

      Debug.Assert(dataFilled <= dataSize);
      Debug.Assert(dataFragmented <= dataFilled);
    }

    public Entry Resize(Entry entry, ulong size)
    {
      if (entry.len == size) return entry; // no changes

      if (size > entry.len) // extend
      {
        if (size > dataSize - dataFilled)
        {
          SetMemoryLength(dataFilled + size);
          if (size > dataSize - dataFilled) throw new OutOfMemoryException();
        }

        ulong extend = size - entry.len;
        if (entry.end == dataFilled) // extend existing memory
        {
          dataFilled += extend;
          return new Entry(entry.ofs, size);
        }
        else // create new memory and copy existing date
        {
          var newEntry = Alloc(size);
          for (ulong i = 0; i < entry.len; i++)
          {
            data[entry.ofs + i] = data[newEntry.ofs + i];
          }
          Free(entry);
          return newEntry;
        }
      }
      else // reduce
      {
        ulong reduce = entry.len - size;
        if (entry.end == dataFilled)
        {
          dataFilled -= reduce;
        }
        else
        {
          dataFragmented += reduce;
        }
        return new Entry(entry.ofs, size);
      }
    }

    public void Validate(Entry[] entries)
    {
      // --- check basics ---
      if (dataFilled > dataSize) throw new Exception();
      if (dataFragmented > dataFilled) throw new Exception();
      if (dataSize != (ulong)data.Length) throw new Exception();

      // --- clone & sort ---
      var checkEntries = new Entry[entries.Length];
      Array.Copy(entries, checkEntries, entries.Length);
      Array.Sort(checkEntries, (x, y) => x.ofs.CompareTo(y.ofs));

      // --- overlap check ---
      for (int i = 1; i < checkEntries.Length; i++)
      {
        if (checkEntries[i - 1].end > checkEntries[i].ofs) throw new Exception("overlap error");
      }

      // --- size check ---
      ulong total = 0;
      for (int i = 0; i < checkEntries.Length; i++)
      {
        if (checkEntries[i].len > dataSize) throw new Exception("oversize error");
        total += checkEntries[i].len;
      }
      if (dataFilled - dataFragmented != total) throw new Exception("size error");

      // --- boundary check ---
      for (int i = 0; i < checkEntries.Length; i++)
      {
        if (checkEntries[i].end > dataFilled) throw new Exception("boundary error");
      }
    }

    void CopyMem(ulong dest, ulong source, ulong count)
    {
      if (dest < source)
      {
        for (ulong i = 0; i < count; i++)
        {
          data[dest + i] = data[source + i];
        }
      }
      else
      {
        for (ulong i = count - 1; i != ulong.MaxValue; i--)
        {
          data[dest + i] = data[source + i];
        }
      }
    }

    public void Optimize(Entry[] entries, bool resize = true)
    {
      int[] pointers = Enumerable.Range(0, entries.Length).ToArray();
      Array.Sort(pointers, (x, y) => entries[x].ofs.CompareTo(entries[y].ofs));

      dataFilled = 0;
      dataFragmented = 0;
      foreach (var pointer in pointers)
      {
        var entry = entries[pointer];
        var newEntry = new Entry(dataFilled, entry.len);

        CopyMem(newEntry.ofs, entry.ofs, entry.len);

        dataFilled += newEntry.len;
        entries[pointer] = newEntry;
      }

      if (resize)
      {
        SetMemoryLength(dataFilled);
      }
    }

    public void OverwriteManager(MiniMemoryManager newMem)
    {
      data = newMem.data;
      dataSize = newMem.dataSize;
      dataFilled = newMem.dataFilled;
      dataFragmented = newMem.dataFragmented;
    }
  }
}
