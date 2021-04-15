#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastBitmapLib;
using YacGui;
// ReSharper disable UnusedType.Global
// ReSharper disable NotAccessedField.Local
// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable UnusedMember.Local
// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable UnusedMember.Global
#pragma warning disable 414
#endregion

namespace TestTool
{
  public class BucketList<T> : IList<T>
  {
    const int MaxBuckets = 32;
    int elementCount;
    int subLevels;
    readonly BucketList<T> parent;
    readonly List<IList<T>> buckets;

    public BucketList()
    {
      elementCount = 0;
      subLevels = 0;
      parent = null;
      buckets = new List<IList<T>>(MaxBuckets) { new List<T>(MaxBuckets) };
    }

    BucketList(BucketList<T> parent, List<T> baseList)
    {
      elementCount = baseList.Count;
      subLevels = 0;
      this.parent = parent;
      parent.UpdateSubLevel(subLevels + 1);
      buckets = new List<IList<T>>(MaxBuckets) { baseList };
    }

    void UpdateSubLevel(int minSublevel)
    {
      if (subLevels < minSublevel)
      {
        subLevels = minSublevel;
        if (parent != null) parent.UpdateSubLevel(minSublevel + 1);
      }
    }

    #region # // --- IList ---
    /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    public int Count { get { return elementCount; } }

    public int CalcCount { get { return buckets.Sum(b => b is BucketList<T> ? ((BucketList<T>)b).CalcCount : b.Count); } }

    /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
    /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    public bool Contains(T item)
    {
      foreach (var b in buckets)
      {
        if (b.Contains(item)) return true;
      }
      return false;
    }

    /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
    public void Clear()
    {
      foreach (var b in buckets)
      {
        b.Clear();
      }
      buckets.Clear();
    }

    /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    public void Add(T item)
    {
      buckets[buckets.Count - 1].Add(item);
      elementCount++;
    }

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
      foreach (var b in buckets)
      {
        foreach (var el in b)
        {
          yield return el;
        }
      }
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is null.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
      if (array == null) throw new ArgumentNullException("array");
      if (arrayIndex + elementCount > array.Length) throw new ArgumentOutOfRangeException("arrayIndex");

      foreach (var b in buckets)
      {
        b.CopyTo(array, arrayIndex);
        arrayIndex += b.Count;
      }
    }

    /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
    /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
    public bool IsReadOnly { get { return false; } }

    /// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
    /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
    public int IndexOf(T item)
    {
      int index = 0;
      foreach (var b in buckets)
      {
        int i = b.IndexOf(item);
        if (i >= 0) return index + i;
        index += b.Count;
      }
      return -1;
    }

    /// <summary>Gets or sets the element at the specified index.</summary>
    /// <returns>The element at the specified index.</returns>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
    public T this[int index]
    {
      get
      {
        if ((uint)index > elementCount) throw new IndexOutOfRangeException();
        foreach (var b in buckets)
        {
          if (b.Count > index) return b[index];
          index -= b.Count;
        }
        throw new IndexOutOfRangeException();
      }
      set
      {
        if ((uint)index > elementCount) throw new IndexOutOfRangeException();
        foreach (var b in buckets)
        {
          if (b.Count > index)
          {
            b[index] = value;
            return;
          }
          index -= b.Count;
        }
        throw new IndexOutOfRangeException();
      }
    }

    public override string ToString()
    {
      return new { Count, subLevels }.ToString();
    }
    #endregion

    /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    public bool Remove(T item)
    {
      return false;
    }

    /// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
    public void Insert(int index, T item)
    {
      if ((uint)index > elementCount) throw new IndexOutOfRangeException("index");

      elementCount++;

      for (int b = 0; b < buckets.Count; b++)
      {
        var bucket = buckets[b];
        if (index <= bucket.Count)
        {
          // --- append to existing bucket ---
          if (index == bucket.Count)
          {
            bucket.Add(item);
            return;
          }

          // --- insert into existing bucket (if small enough or if a sub-bucket) ---
          if (bucket.Count < MaxBuckets || bucket is BucketList<T>)
          {
            bucket.Insert(index, item);
            return;
          }

          // --- split existing bucket in two buckets (if possible) ---
          if (buckets.Count < MaxBuckets)
          {
            int leftCount = bucket.Count / 2;
            var rightList = new List<T>(Math.Max(bucket.Count - leftCount, MaxBuckets));
            for (int i = leftCount; i < bucket.Count; i++) rightList.Add(bucket[i]);
            for (int i = bucket.Count - 1; i >= leftCount; i--) bucket.RemoveAt(i);
            if (index <= bucket.Count)
            {
              bucket.Insert(index, item);
            }
            else
            {
              index -= bucket.Count;
              rightList.Insert(index, item);
            }
            buckets.Insert(b + 1, rightList);
            return;
          }

          // --- create new sub-bucket ---
          buckets[b] = bucket = new BucketList<T>(this, (List<T>)bucket);
          bucket.Insert(index, item);
          return;
        }
        index -= bucket.Count;
      }
      throw new IndexOutOfRangeException();
    }

    /// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
    public void RemoveAt(int index)
    {
    }
  }

  class Program : ConsoleExtras
  {
    static void PrintDebug(MemoryManager mem, MemoryManager.Entry[] entries)
    {
      mem.Validate(entries);
      Console.WriteLine("------------------------\r\n" +
                        string.Join("\r\n", entries.OrderBy(x => x.ofs).Select(x => "          " + x)) +
                        "\r\n------------------------ " +
                        "(fragmented: " + (100.0 / mem.dataFilled * mem.dataFragmented).ToString("N2") + " %, reserved: " + (mem.dataSize / 1048576.0).ToString("N1") + " MB) ------");
    }

    static void MemTest()
    {
      var mem = new MemoryManager();

      var rnd = new Random(12345);

      var entries = new List<MemoryManager.Entry>();

      var time = Stopwatch.StartNew();

      for (int i = 0; i < 10000000; i++)
      {
        if (rnd.Next(entries.Count + 1080) < entries.Count) // löschen
        {
          if (entries.Count > 0)
          {
            int kill = rnd.Next(entries.Count);
            //Console.WriteLine("    free: " + entries[kill]);
            mem.Free(entries[kill]);
            entries.RemoveAt(kill);
          }
        }
        else // reservieren
        {
          ulong size = (ulong)rnd.Next(1920);
          var newEntry = mem.Alloc(size);
          //Console.WriteLine("   alloc: " + newEntry);
          entries.Add(newEntry);
          //entries.Insert(rnd.Next(entries.Count + 1), newEntry);
        }
        if ((i & 0xfffff) == 0)
        {
          Console.Title = i.ToString("N0") + " - " + entries.Count.ToString("N0") + " - " + (mem.dataSize / 1048576.0).ToString("N1") + " MB";
          Console.WriteLine("gc: {0:N1} MB", Process.GetCurrentProcess().PrivateMemorySize64 / 1048576.0);
        }

        // mem.dataFragmented > mem.dataSize * 99 / 100 ||
        //if (mem.dataSize > 1000000000 && mem.dataFragmented > 100000000)
        //if (mem.dataFragmented > mem.dataSize * 90 / 100) // optimize > 90% fragmented
        //if (mem.dataFragmented > mem.dataSize * 95 / 100) // optimize > 95% fragmented
        if (mem.dataFragmented > mem.dataSize * 99 / 100) // optimize > 99% fragmented
        {
          var tmp = entries.ToArray();
          mem.Optimize(tmp, false);
          entries.Clear();
          entries.AddRange(tmp);
        }
      }

      time.Stop();
      Console.WriteLine("time: {0:N0} ms", time.ElapsedMilliseconds);
      Console.ReadLine();
    }

    static void BucketTest()
    {
      var b1 = new BucketList<int>();
      var rnd = new Random(12345);

      var time = Stopwatch.StartNew();
      for (int i = 0; i < 10000000; i++)
      {
        if ((i & 0xffff) == 0) Console.WriteLine(i.ToString("N0"));
        int next = rnd.Next(b1.Count + 1);
        b1.Insert(next, i);
      }
      time.Stop();
      Console.WriteLine(time.ElapsedMilliseconds.ToString("N0") + " ms");
    }

    /// <summary>
    /// TestTool program entry
    /// </summary>
    static void Main()
    {
      ConsoleHead("Test Tool: " + MainForm.FullName);

      //var comp = new CompressedBitmap(1920, 1080);

      //BitmapTests.Run();
      BucketTest();
    }
  }
}
