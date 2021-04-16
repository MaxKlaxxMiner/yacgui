#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable RedundantIfElseBlock
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable StaticMemberInGenericType
#pragma warning disable 169
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

  public class BucketList2<T> : IList<T>
  {
    static readonly int MaxBucketSize = Marshal.SizeOf(default(T)) * 64;
    static readonly int MaxClusterSize = MaxBucketSize * 16;
    static readonly int MaxRegionSize = MaxClusterSize * 64;

    static readonly int MinBucketSize = MaxBucketSize / 4;
    static readonly int MinClusterSize = MaxClusterSize / 4;
    static readonly int MinRegionSize = MaxRegionSize / 4;

    #region # // --- Structs + Data ---
    // --- Buckets ---
    struct Bucket
    {
      public int dataCount;
      public int dataOfs;
      public int prev;
      public int next;
      public Bucket(int dataCount, int dataOfs, int prev, int next)
      {
        this.dataCount = dataCount;
        this.dataOfs = dataOfs;
        this.prev = prev;
        this.next = next;
      }
      public override string ToString()
      {
        return new { dataCount, dataOfs, prev, next }.ToString();
      }
    }
    Bucket[] buckets;
    int bucketsFill;

    // --- Clusters ---
    struct Cluster
    {
      public int dataCount;
      public int bucketStart;
      public int prev;
      public int next;
      public Cluster(int dataCount, int bucketStart, int prev, int next)
      {
        this.dataCount = dataCount;
        this.bucketStart = bucketStart;
        this.prev = prev;
        this.next = next;
      }
      public override string ToString()
      {
        return new { dataCount, bucketStart, prev, next }.ToString();
      }
    }
    Cluster[] clusters;
    int clustersFill;

    // --- Regions ---
    struct Region
    {
      public int dataCount;
      public int clusterStart;
      public int prev;
      public int next;
      public Region(int dataCount, int clusterStart, int prev, int next)
      {
        this.dataCount = dataCount;
        this.clusterStart = clusterStart;
        this.prev = prev;
        this.next = next;
      }
      public override string ToString()
      {
        return new { dataCount, clusterStart, prev, next }.ToString();
      }
    }
    Region[] regions;
    int regionsFill;

    // --- Data ---
    T[] data;
    int dataCount;
    #endregion

    #region # // --- constructors ---
    /// <summary>
    /// constructor
    /// </summary>
    public BucketList2()
    {
      data = new T[MaxBucketSize];
      dataCount = 0;
      buckets = new[] { new Bucket(0, 0, -1, -1) };
      bucketsFill = buckets.Length;
      clusters = new[] { new Cluster(0, 0, -1, -1) };
      clustersFill = clusters.Length;
      regions = new[] { new Region(0, 0, -1, -1) };
      regionsFill = regions.Length;
    }
    #endregion

    #region # // --- IList ---
    /// <summary>
    /// Ruft die Anzahl der Elemente ab, die in <see cref="T:System.Collections.Generic.ICollection`1"/> enthalten sind.
    /// </summary>
    /// <returns>
    /// Die Anzahl der Elemente, die in <see cref="T:System.Collections.Generic.ICollection`1"/> enthalten sind.
    /// </returns>
    public int Count { get { return dataCount; } }
    #endregion

    void MoveData(int destOfs, int srcOfs, int count)
    {
      var ds = data;
      for (int i = 0; i < count; i++)
      {
        ds[destOfs + i] = ds[srcOfs + i];
        ds[srcOfs + i] = default(T);
      }
    }

    void ResizeBuckets(int minBucketCount)
    {
      int newSize = buckets.Length;

      while (newSize > MaxBucketSize && newSize / 2 > bucketsFill) newSize /= 2;

      while (minBucketCount > newSize) newSize *= 2;

      if (newSize != buckets.Length)
      {
        Array.Resize(ref buckets, newSize);
        Array.Resize(ref data, newSize * MaxBucketSize);
      }
    }

    void ResizeClusters(int minClusterCount)
    {
      int newSize = clusters.Length;

      while (newSize > MaxBucketSize && newSize / 2 > clustersFill) newSize /= 2;

      while (minClusterCount > newSize) newSize *= 2;

      if (newSize != clusters.Length)
      {
        Array.Resize(ref clusters, newSize);
      }
    }

    void ResizeRegions(int minRegionCount)
    {
      int newSize = regions.Length;

      while (newSize > MaxBucketSize && newSize / 2 > regionsFill) newSize /= 2;

      while (minRegionCount > newSize) newSize *= 2;

      if (newSize != regions.Length)
      {
        Array.Resize(ref regions, newSize);
      }
    }

    int SplitBucket(int bucket)
    {
      ResizeBuckets(bucketsFill + 1);

      var bs = buckets;

      // --- calculate sizes ---
      int newLeftCount = bs[bucket].dataCount / 2;
      int newRightCount = bs[bucket].dataCount - newLeftCount;
      bs[bucket].dataCount = newLeftCount;

      // --- create new bucket & linking ---
      int newBucket = bucketsFill++;
      bs[newBucket] = new Bucket(newRightCount, newBucket * MaxBucketSize, bucket, bs[bucket].next);
      if (bs[bucket].next >= 0) bs[bs[bucket].next].prev = newBucket;
      bs[bucket].next = newBucket;

      // --- copy (half) data to the new bucket ---
      MoveData(bs[newBucket].dataOfs, bs[bucket].dataOfs + newLeftCount, newRightCount);

      return newBucket;
    }

    int SplitCluster(int cluster)
    {
      ResizeClusters(clustersFill + 1);

      var bs = buckets;
      var cs = clusters;

      // --- calculate sizes ---
      int minLeftCount = cs[cluster].dataCount / 2;
      int newLeftCount = 0;
      int bucketSplit = cs[cluster].bucketStart;
      while (newLeftCount < minLeftCount && bs[bucketSplit].next >= 0)
      {
        newLeftCount += bs[bucketSplit].dataCount;
        bucketSplit = bs[bucketSplit].next;
      }
      int newRightCount = cs[cluster].dataCount - newLeftCount;
      Debug.Assert(newLeftCount > 0);
      Debug.Assert(newRightCount > 0);
      cs[cluster].dataCount = newLeftCount;

      // --- create new cluster & linking ---
      int newCluster = clustersFill++;
      cs[newCluster] = new Cluster(newRightCount, bucketSplit, cluster, cs[cluster].next);
      if (cs[cluster].next >= 0) cs[cs[cluster].next].prev = newCluster;
      cs[cluster].next = newCluster;

      return newCluster;
    }

    int SplitRegion(int region)
    {
      ResizeRegions(regionsFill + 1);

      var cs = clusters;
      var rs = regions;

      // --- calculate sizes ---
      int minLeftCount = rs[region].dataCount / 2;
      int newLeftCount = 0;
      int clusterSplit = rs[region].clusterStart;
      while (newLeftCount < minLeftCount && cs[clusterSplit].next >= 0)
      {
        newLeftCount += cs[clusterSplit].dataCount;
        clusterSplit = cs[clusterSplit].next;
      }
      int newRightCount = rs[region].dataCount - newLeftCount;
      Debug.Assert(newLeftCount > 0);
      Debug.Assert(newRightCount > 0);
      rs[region].dataCount = newLeftCount;

      // --- create new region & linking ---
      int newRegion = regionsFill++;
      rs[newRegion] = new Region(newRightCount, clusterSplit, region, rs[region].next);
      if (rs[region].next >= 0) rs[rs[region].next].prev = newRegion;
      rs[region].next = newRegion;

      return newRegion;
    }

    void OptimizeRegion(int region)
    {
      if (regions[region].dataCount > MaxRegionSize)
      {
        SplitRegion(region);
      }
      else if (regions[region].dataCount < MinRegionSize)
      {
        if (regions[region].prev >= 0 && regions[regions[region].prev].dataCount + regions[region].dataCount < MaxRegionSize)
        {
          // --- merge previous region ---
          throw new NotImplementedException();
        }
        else if (regions[region].next >= 0 && regions[regions[region].next].dataCount + regions[region].dataCount < MaxRegionSize)
        {
          // --- merge next region ---
          throw new NotImplementedException();
        }
      }
    }

    void OptimizeCluster(int cluster, int region)
    {
      if (clusters[cluster].dataCount > MaxClusterSize)
      {
        SplitCluster(cluster);
        OptimizeRegion(region);
      }
      else if(clusters[cluster].dataCount < MinClusterSize)
      {
        if (clusters[cluster].prev >= 0 && clusters[clusters[cluster].prev].dataCount + clusters[cluster].dataCount < MaxClusterSize)
        {
          // --- merge previous cluster ---
          throw new NotImplementedException();
        }
        else if (clusters[cluster].next >= 0 && clusters[clusters[cluster].next].dataCount + clusters[cluster].dataCount < MaxClusterSize)
        {
          // --- merge next cluster ---
          throw new NotImplementedException();
        }
      }
    }

    /// <summary>
    /// Fügt am angegebenen Index ein Element in die <see cref="T:System.Collections.Generic.IList`1"/> ein.
    /// </summary>
    /// <param name="index">Der nullbasierte Index, an dem <paramref name="item"/> eingefügt werden soll.</param><param name="item">Das in die <see cref="T:System.Collections.Generic.IList`1"/> einzufügende Objekt.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> ist kein gültiger Index in der <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Die <see cref="T:System.Collections.Generic.IList`1"/> ist schreibgeschützt.</exception>
    public void Insert(int index, T item)
    {
      if ((uint)index > dataCount) throw new IndexOutOfRangeException("index");

      // --- search bucket per index ---
      int region, cluster, bucket;
      for (region = 0; index > regions[region].dataCount; region = regions[region].next) { index -= regions[region].dataCount; }
      for (cluster = regions[region].clusterStart; index > clusters[cluster].dataCount; cluster = clusters[cluster].next) { index -= clusters[cluster].dataCount; }
      for (bucket = clusters[cluster].bucketStart; index > buckets[bucket].dataCount; bucket = buckets[bucket].next) { index -= buckets[bucket].dataCount; }

      // --- increment data-counts ---
      buckets[bucket].dataCount++;
      clusters[cluster].dataCount++;
      regions[region].dataCount++;
      dataCount++;

      // --- split bucket, if full ---
      if (buckets[bucket].dataCount == MaxBucketSize)
      {
        int newBucket = SplitBucket(bucket);
        OptimizeCluster(cluster, region);
        if (index > buckets[bucket].dataCount)
        {
          index -= buckets[bucket].dataCount;
          bucket = newBucket;
        }
      }

      // --- insert data ---
      int dataOfs = buckets[bucket].dataOfs;
      for (int m = buckets[bucket].dataCount; m > index; m--)
      {
        data[dataOfs + m] = data[dataOfs + m - 1];
      }
      data[dataOfs + index] = item;
    }

    /// <summary>
    /// Gibt einen Enumerator zurück, der die Auflistung durchläuft.
    /// </summary>
    /// <returns>
    /// Ein <see cref="T:System.Collections.Generic.IEnumerator`1"/>, der zum Durchlaufen der Auflistung verwendet werden kann.
    /// </returns>
    public IEnumerator<T> GetEnumerator()
    {
      yield break;
    }

    /// <summary>
    /// Gibt einen Enumerator zurück, der eine Auflistung durchläuft.
    /// </summary>
    /// <returns>
    /// Ein <see cref="T:System.Collections.IEnumerator"/>-Objekt, das zum Durchlaufen der Auflistung verwendet werden kann.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Fügt der <see cref="T:System.Collections.Generic.ICollection`1"/> ein Element hinzu.
    /// </summary>
    /// <param name="item">Das Objekt, das <see cref="T:System.Collections.Generic.ICollection`1"/> hinzugefügt werden soll.</param><exception cref="T:System.NotSupportedException"><see cref="T:System.Collections.Generic.ICollection`1"/> ist schreibgeschützt.</exception>
    public void Add(T item)
    {
    }

    /// <summary>
    /// Entfernt alle Elemente aus <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <exception cref="T:System.NotSupportedException"><see cref="T:System.Collections.Generic.ICollection`1"/> ist schreibgeschützt.</exception>
    public void Clear()
    {
    }

    /// <summary>
    /// Ermittelt, ob die <see cref="T:System.Collections.Generic.ICollection`1"/> einen bestimmten Wert enthält.
    /// </summary>
    /// <returns>
    /// true, wenn sich <paramref name="item"/> in <see cref="T:System.Collections.Generic.ICollection`1"/> befindet, andernfalls false.
    /// </returns>
    /// <param name="item">Das im <see cref="T:System.Collections.Generic.ICollection`1"/> zu suchende Objekt.</param>
    public bool Contains(T item)
    {
      return false;
    }

    /// <summary>
    /// Kopiert die Elemente von <see cref="T:System.Collections.Generic.ICollection`1"/> in ein <see cref="T:System.Array"/>, beginnend bei einem bestimmten <see cref="T:System.Array"/>-Index.
    /// </summary>
    /// <param name="array">Das eindimensionale <see cref="T:System.Array"/>, das das Ziel der aus der <see cref="T:System.Collections.Generic.ICollection`1"/> kopierten Elemente ist. Für das <see cref="T:System.Array"/> muss eine nullbasierte Indizierung verwendet werden.</param><param name="arrayIndex">Der nullbasierte Index in <paramref name="array"/>, an dem das Kopieren beginnt.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> ist null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> ist kleiner als 0.</exception><exception cref="T:System.ArgumentException">Die Anzahl der Elemente in der Quell-<see cref="T:System.Collections.Generic.ICollection`1"/> ist größer als der verfügbare Platz von <paramref name="arrayIndex"/> bis zum Ende des Ziel-<paramref name="array"/>.</exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
    }

    /// <summary>
    /// Entfernt das erste Vorkommen eines angegebenen Objekts aus der <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <returns>
    /// true, wenn das <paramref name="item"/> erfolgreich aus der <see cref="T:System.Collections.Generic.ICollection`1"/> entfernt wurde, andernfalls false. Diese Methode gibt auch dann false zurück, wenn <paramref name="item"/> nicht in der ursprünglichen <see cref="T:System.Collections.Generic.ICollection`1"/> gefunden wurde.
    /// </returns>
    /// <param name="item">Das aus dem <see cref="T:System.Collections.Generic.ICollection`1"/> zu entfernende Objekt.</param><exception cref="T:System.NotSupportedException"><see cref="T:System.Collections.Generic.ICollection`1"/> ist schreibgeschützt.</exception>
    public bool Remove(T item)
    {
      return false;
    }

    /// <summary>
    /// Ruft einen Wert ab, der angibt, ob das <see cref="T:System.Collections.Generic.ICollection`1"/> schreibgeschützt ist.
    /// </summary>
    /// <returns>
    /// true, wenn das <see cref="T:System.Collections.Generic.ICollection`1"/> schreibgeschützt ist, andernfalls false.
    /// </returns>
    public bool IsReadOnly { get; private set; }

    /// <summary>
    /// Bestimmt den Index eines bestimmten Elements in der <see cref="T:System.Collections.Generic.IList`1"/>.
    /// </summary>
    /// <returns>
    /// Der Index von <paramref name="item"/>, wenn das Element in der Liste gefunden wird, andernfalls -1.
    /// </returns>
    /// <param name="item">Das im <see cref="T:System.Collections.Generic.IList`1"/> zu suchende Objekt.</param>
    public int IndexOf(T item)
    {
      return 0;
    }

    /// <summary>
    /// Entfernt das <see cref="T:System.Collections.Generic.IList`1"/>-Element am angegebenen Index.
    /// </summary>
    /// <param name="index">Der nullbasierte Index des zu entfernenden Elements.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> ist kein gültiger Index in der <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Die <see cref="T:System.Collections.Generic.IList`1"/> ist schreibgeschützt.</exception>
    public void RemoveAt(int index)
    {
    }

    /// <summary>
    /// Ruft das Element am angegebenen Index ab oder legt dieses fest.
    /// </summary>
    /// <returns>
    /// Das Element am angegebenen Index.
    /// </returns>
    /// <param name="index">Der nullbasierte Index des Elements, das abgerufen oder festgelegt werden soll.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> ist kein gültiger Index in der <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Die Eigenschaft wird festgelegt, und die <see cref="T:System.Collections.Generic.IList`1"/> ist schreibgeschützt.</exception>
    public T this[int index]
    {
      get { return default(T); }
      set { }
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
      var b1 = new BucketList2<int>();
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
