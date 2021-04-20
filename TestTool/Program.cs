#define UnsafeBuckets

#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using FastBitmapLib;
using FastBitmapLib.Extras;
using YacGui;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable RedundantIfElseBlock
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable TailRecursiveCall
// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable UnreachableCode
#pragma warning disable 414
#pragma warning disable 169
#pragma warning disable 649
#pragma warning disable 162
#endregion

namespace TestTool
{
  /// <summary>
  /// Bucket-Struct
  /// </summary>
  [StructLayout(LayoutKind.Explicit, Size = 20)]
  public struct Bucket
  {
    /// <summary>
    /// total containing data count
    /// </summary>
    [FieldOffset(0)]
    public int dataCount;
    /// <summary>
    /// previuos Bucket at same level (-1 = first bucket)
    /// </summary>
    [FieldOffset(4)]
    public int prev;
    /// <summary>
    /// next Bucket at same level (-1 = last bucket)
    /// </summary>
    [FieldOffset(8)]
    public int next;
    /// <summary>
    /// parent Bucket one level above (-1 = top bucket level)
    /// </summary>
    [FieldOffset(12)]
    public int parent;
    /// <summary>
    /// first child Bucket (lower than 0 = Data-Offset - int.MinValue)
    /// </summary>
    [FieldOffset(16)]
    public int childStart;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataCount">total containing data count</param>
    /// <param name="prev">previuos Bucket at same level (-1 = first bucket)</param>
    /// <param name="next">next Bucket at same level (-1 = last bucket)</param>
    /// <param name="parent">parent Bucket one level above (-1 = top bucket level)</param>
    /// <param name="childStart">first child Bucket (lower than 0 = Data-Offset - int.MinValue)</param>
    public Bucket(int dataCount, int prev, int next, int parent, int childStart)
    {
      this.dataCount = dataCount;
      this.prev = prev;
      this.next = next;
      this.parent = parent;
      this.childStart = childStart;
    }

    /// <summary>
    /// return true if a Data-Bucket
    /// </summary>
    public bool HasData { get { return childStart < 0; } }

    /// <summary>
    /// get or set DataOffset (use childStart)
    /// </summary>
    public int DataOffset
    {
      get
      {
        Debug.Assert(childStart < 0);
        return childStart + int.MinValue;
      }
      set
      {
        childStart = value - int.MinValue;
      }
    }

    /// <summary>
    /// get last unused Element as global Data-Array
    /// </summary>
    public int DataEndOffset
    {
      get
      {
        Debug.Assert(HasData);
        Debug.Assert(dataCount >= 0);
        return childStart + int.MinValue + dataCount;
      }
    }

    /// <summary>
    /// return a readable string of content
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (HasData)
      {
        return new { dataCount, prev, next, parent, DataOffset }.ToString();
      }
      else
      {
        return new { dataCount, prev, next, parent, childStart }.ToString();
      }
    }
  }

  public unsafe class BucketList<T> : IList<T>
  {
    /// <summary>
    /// Size of Bucket (max Elements per Bucket)
    /// </summary>
    const int MaxBucketSize = 128;
    /// <summary>
    /// max Bucket-Count per level
    /// </summary>
    const int LevelMultiplicator = 8;
    /// <summary>
    /// divisor for lowest count to merge neighbors
    /// </summary>
    const int LowMergeLevelDiv = 4;

    #region # // --- static type scan ---
    static readonly int UnsafeSize = GetUnsafeSize();
    static readonly bool NeedSafeData = UnsafeSize == 0;
    static readonly bool UnsafeMode = !NeedSafeData;

    static int GetUnsafeSize()
    {
      if (typeof(T).IsClass) return 0;

      if (!typeof(T).IsPrimitive)
      {
        try
        {
          GCHandle.Alloc(new T[1], GCHandleType.Pinned).Free();
        }
        catch
        {
          return 0;
        }
      }

      return Marshal.SizeOf(typeof(T));
    }
    #endregion

    #region # // --- members & data ---
    /// <summary>
    /// base data array with all elements
    /// </summary>
    T[] dataRaw;
    /// <summary>
    /// total used Elements
    /// </summary>
    int dataCount;
    /// <summary>
    /// used data-elements as buckets (&gt;= dataCount)
    /// </summary>
    int dataFill;
    /// <summary>
    /// all buckets
    /// </summary>
#if UnsafeBuckets
    Bucket* buckets;
#else
    Bucket[] buckets;
#endif
    /// <summary>
    /// current reserved buckets
    /// </summary>
    int bucketsReserved;
    /// <summary>
    /// used buckets in array
    /// </summary>
    int bucketsFill;
    /// <summary>
    /// last bucket (on lowest level)
    /// </summary>
    int lastBucket;
    #endregion

    #region # // --- Constructor / Destructor ---
    GCHandle unsafeDataPinnedHandle;
    long unsafeDataPinned;

    /// <summary>
    /// Constructor
    /// </summary>
    public BucketList()
    {
      dataRaw = new T[MaxBucketSize];
      if (UnsafeMode)
      {
        unsafeDataPinnedHandle = GCHandle.Alloc(dataRaw, GCHandleType.Pinned);
        unsafeDataPinned = (long)unsafeDataPinnedHandle.AddrOfPinnedObject();
      }
      bucketsReserved = 1;
#if UnsafeBuckets
      buckets = (Bucket*)Marshal.AllocHGlobal(bucketsReserved * sizeof(Bucket));
#else
      buckets = new Bucket[bucketsReserved];
#endif
      Clear();
    }

    ~BucketList()
    {
      if (UnsafeMode)
      {
        unsafeDataPinned = 0;
        unsafeDataPinnedHandle.Free();
        unsafeDataPinnedHandle = default(GCHandle);
      }

#if UnsafeBuckets
      Marshal.FreeHGlobal((IntPtr)buckets);
#endif
    }
    #endregion

    #region # // --- Helper methods ---
    /// <summary>
    /// create a new Bucket
    /// </summary>
    /// <returns>ID of the Bucket</returns>
    int GetNewBucket()
    {
      if (bucketsFill == bucketsReserved)
      {
        bucketsReserved *= 2;
#if UnsafeBuckets
        buckets = (Bucket*)Marshal.ReAllocHGlobal((IntPtr)buckets, (IntPtr)((long)bucketsReserved * sizeof(Bucket)));
#else
        Array.Resize(ref buckets, bucketsReserved);
#endif
      }
      return bucketsFill++;
    }

    /// <summary>
    /// create a new Data-Bucket (and set the data offset)
    /// </summary>
    /// <param name="prev">previous Bucket</param>
    /// <param name="next">next Bucket</param>
    /// <param name="parent">parent Bucket</param>
    /// <returns>ID of the Bucket</returns>
    int GetNewDataBucket(int prev, int next, int parent)
    {
      int newBucket = GetNewBucket();

      if (dataFill + MaxBucketSize > dataRaw.Length)
      {
        if (UnsafeMode)
        {
          unsafeDataPinned = 0;
          unsafeDataPinnedHandle.Free();
        }
        Array.Resize(ref dataRaw, dataRaw.Length * 2);
        if (UnsafeMode)
        {
          unsafeDataPinnedHandle = GCHandle.Alloc(dataRaw, GCHandleType.Pinned);
          unsafeDataPinned = (long)unsafeDataPinnedHandle.AddrOfPinnedObject();
        }
      }

      buckets[newBucket] = new Bucket(0, prev, next, parent, unchecked(dataFill - int.MinValue));
      dataFill += MaxBucketSize;

      return newBucket;
    }

    /// <summary>
    /// update the Parent-Marker on a Bucket-Chain
    /// </summary>
    /// <param name="firstBucket">first Bucket in a Bucket-Chain</param>
    /// <param name="newParent">new Parent-Marker to set</param>
    void UpdateParentsInChain(int firstBucket, int newParent)
    {
      if (firstBucket == int.MaxValue) return;
      int oldParent = buckets[firstBucket].parent;
      for (; firstBucket >= 0 && buckets[firstBucket].parent == oldParent; firstBucket = buckets[firstBucket].next)
      {
        buckets[firstBucket].parent = newParent;
      }
    }

    /// <summary>
    /// Move a Bucket + linking
    /// </summary>
    /// <param name="srcBucket">old Bucket</param>
    /// <param name="destBucket">new Bucket</param>
    void MoveBucket(int srcBucket, int destBucket)
    {
      Debug.Assert(srcBucket != destBucket);

      // --- copy Data ---
      var b = buckets[srcBucket];
      buckets[destBucket] = b;

      // --- update previous & next marker on neighbors ---
      if (b.next >= 0) buckets[b.next].prev = destBucket;
      if (b.prev >= 0) buckets[b.prev].next = destBucket;

      // --- update child-marker in parent (if first bucket) ---
      if (buckets[destBucket].parent >= 0 && buckets[buckets[destBucket].parent].childStart == srcBucket)
      {
        buckets[buckets[destBucket].parent].childStart = destBucket;
      }

      // --- update the parent-marker in childs ---
      if (!buckets[destBucket].HasData)
      {
        UpdateParentsInChain(buckets[destBucket].childStart, destBucket);
      }
      else
      {
        if (srcBucket == lastBucket) lastBucket = destBucket;
      }

      buckets[srcBucket] = default(Bucket);
    }

    /// <summary>
    /// Move Data-Elements (and zero old values)
    /// </summary>
    /// <param name="srcOffset">Source-Offset in Data Array</param>
    /// <param name="destOffset">Destination-Offset in Date Array</param>
    /// <param name="count">Element-Count</param>
    void MoveData(int srcOffset, int destOffset, int count)
    {
      if (UnsafeMode)
      {
        UnsafeHelper.MemCopy(unsafeDataPinned + destOffset * UnsafeSize, unsafeDataPinned + srcOffset * UnsafeSize, count * UnsafeSize);
      }
      else
      {
        if (srcOffset > destOffset)
        {
          if (srcOffset == destOffset) return;
          for (int i = 0; i < count; i++)
          {
            dataRaw[destOffset + i] = dataRaw[srcOffset + i];
            dataRaw[srcOffset + i] = default(T);
          }
        }
        else
        {
          for (int i = count - 1; i >= 0; i--)
          {
            dataRaw[destOffset + i] = dataRaw[srcOffset + i];
            dataRaw[srcOffset + i] = default(T);
          }
        }
      }
    }

    /// <summary>
    /// Move Data-Elements (and zero old values)
    /// </summary>
    /// <param name="srcOffset">Source-Offset in Data Array</param>
    /// <param name="destOffset">Destination-Offset in Date Array</param>
    /// <param name="count">Element-Count</param>
    void MoveDataLeft(int srcOffset, int destOffset, int count)
    {
      Debug.Assert(srcOffset > destOffset);

      if (UnsafeMode)
      {
        UnsafeHelper.MemCopyForward(unsafeDataPinned + destOffset * UnsafeSize, unsafeDataPinned + srcOffset * UnsafeSize, count * UnsafeSize);
      }
      else
      {
        if (srcOffset == destOffset) return;
        for (int i = 0; i < count; i++)
        {
          dataRaw[destOffset + i] = dataRaw[srcOffset + i];
          dataRaw[srcOffset + i] = default(T);
        }
      }
    }

    /// <summary>
    /// Move Data-Elements (and zero old values)
    /// </summary>
    /// <param name="srcOffset">Source-Offset in Data Array</param>
    /// <param name="destOffset">Destination-Offset in Date Array</param>
    /// <param name="count">Element-Count</param>
    void MoveDataRight(int srcOffset, int destOffset, int count)
    {
      Debug.Assert(srcOffset < destOffset);

      if (UnsafeMode)
      {
        UnsafeHelper.MemCopyBackward(unsafeDataPinned + destOffset * UnsafeSize, unsafeDataPinned + srcOffset * UnsafeSize, count * UnsafeSize);
      }
      else
      {
        for (int i = count - 1; i >= 0; i--)
        {
          dataRaw[destOffset + i] = dataRaw[srcOffset + i];
          dataRaw[srcOffset + i] = default(T);
        }
      }
    }

    /// <summary>
    /// Split a (nonData) Bucket
    /// </summary>
    /// <param name="bucket">Bucket to be split</param>
    /// <returns>new second Bucket-Id</returns>
    int SplitNonDataBucket(int bucket)
    {
      Debug.Assert(!buckets[bucket].HasData);
      Debug.Assert(buckets[bucket].dataCount > buckets[buckets[bucket].childStart].dataCount * 2);

      // --- search middle ---
      int targetCount = buckets[bucket].dataCount / 2;
      int newCount = 0;
      int subBucket;
      for (subBucket = buckets[bucket].childStart; newCount < targetCount; subBucket = buckets[subBucket].next)
      {
        newCount += buckets[subBucket].dataCount;
      }

      Debug.Assert(buckets[subBucket].parent == bucket);

      // --- create new bucket + linking ---
      int newBucket = GetNewBucket();
      buckets[newBucket] = new Bucket(buckets[bucket].dataCount - newCount, bucket, buckets[bucket].next, buckets[bucket].parent, subBucket);
      buckets[bucket] = new Bucket(newCount, buckets[bucket].prev, newBucket, buckets[bucket].parent, buckets[bucket].childStart);
      if (buckets[newBucket].next >= 0) buckets[buckets[newBucket].next].prev = newBucket;
      UpdateParentsInChain(subBucket, newBucket);

      return newBucket;
    }

    /// <summary>
    /// Split a Data-Bucket
    /// </summary>
    /// <param name="bucket">Bucket to be split</param>
    /// <returns>new second Bucket-Id</returns>
    int SplitDataBucket(int bucket)
    {
      Debug.Assert(buckets[bucket].HasData);
      Debug.Assert(buckets[bucket].dataCount > 1);

      // --- create new bucket + linking ---
      int newBucket = GetNewDataBucket(bucket, buckets[bucket].next, buckets[bucket].parent);
      int leftCount = buckets[bucket].dataCount / 2;
      int rightCount = buckets[bucket].dataCount - leftCount;
      buckets[newBucket].dataCount = rightCount;
      buckets[bucket].dataCount = leftCount;
      buckets[bucket].next = newBucket;
      if (buckets[newBucket].next >= 0) buckets[buckets[newBucket].next].prev = newBucket; else lastBucket = newBucket;

      // --- copy data ---
      Debug.Assert(buckets[bucket].HasData);
      Debug.Assert(buckets[newBucket].HasData);
      MoveDataRight(buckets[bucket].DataEndOffset, buckets[newBucket].DataOffset, rightCount);

      return newBucket;
    }

    /// <summary>
    /// creates global levelup
    /// </summary>
    void LevelUp()
    {
      int moveBucket = GetNewBucket();
      MoveBucket(0, moveBucket);
      buckets[0] = new Bucket(dataCount, -1, -1, -1, moveBucket);
      UpdateParentsInChain(moveBucket, 0);
    }

    /// <summary>
    /// increment the dataCount in bucket (inclusive parents)
    /// </summary>
    /// <param name="bucket">Bucket to update</param>
    void IncrementCount(int bucket)
    {
      Debug.Assert((uint)bucket < bucketsFill);
      Debug.Assert(buckets[bucket].HasData);

      int currentMaxCount = MaxBucketSize;
      int todoOptimize = -1;
      while ((uint)bucket < bucketsReserved)
      {
        if (buckets[(uint)bucket].dataCount++ > currentMaxCount)
        {
          Debug.Assert(!buckets[(uint)bucket].HasData);
          if (todoOptimize < 0) todoOptimize = bucket;
        }
        currentMaxCount *= LevelMultiplicator;
        Debug.Assert(currentMaxCount > 0);
        bucket = buckets[(uint)bucket].parent;
      }

      if (todoOptimize >= 0)
      {
        SplitNonDataBucket(todoOptimize);
      }

      if (dataCount++ > currentMaxCount)
      {
        LevelUp();
      }
    }

    /// <summary>
    /// remove an empty bucket
    /// </summary>
    /// <param name="bucket">Bucket to remove</param>
    void RemoveEmptyBucket(int bucket)
    {
      var b = buckets[bucket];
      Debug.Assert(b.dataCount == 0);
      Debug.Assert(b.prev >= 0 || b.next >= 0);

      if (b.prev >= 0) buckets[b.prev].next = b.next;
      if (b.next >= 0) buckets[b.next].prev = b.prev; else if (b.HasData) lastBucket = b.prev;

      int nextEmptyBucket = -1;

      if (b.parent >= 0 && buckets[b.parent].childStart == bucket)
      {
        if (buckets[b.parent].dataCount == 0 && buckets[b.parent].prev + buckets[b.parent].next != -2)
        {
          buckets[b.parent].childStart = int.MaxValue;
          nextEmptyBucket = b.parent;
          if (nextEmptyBucket == bucketsFill - 1) nextEmptyBucket = bucket;
        }
        else
        {
          buckets[b.parent].childStart = b.next;
        }
      }

      bucketsFill--;
      if (bucket == bucketsFill)
      {
        buckets[bucket] = default(Bucket);
      }
      else
      {
        MoveBucket(bucketsFill, bucket);
      }

      if (nextEmptyBucket >= 0) RemoveEmptyBucket(nextEmptyBucket);
    }

    /// <summary>
    /// decrement the dataCount in bucket (inclusive parents)
    /// </summary>
    /// <param name="bucket">Bucket to update</param>
    void DecrementCount(int bucket)
    {
      Debug.Assert((uint)bucket < bucketsFill);
      Debug.Assert(buckets[bucket].HasData);
      Debug.Assert(buckets[bucket].dataCount > 0);

      int b = bucket;
      while ((uint)b < bucketsReserved)
      {
        buckets[(uint)b].dataCount--;
        b = buckets[(uint)b].parent;
      }

      dataCount--;

      if (buckets[bucket].dataCount == 0 && buckets[bucket].prev + buckets[bucket].next != -2)
      {
        RemoveEmptyBucket(bucket);
      }
    }

    /// <summary>
    /// subtract from dataCount in bucket (inclusive parents)
    /// </summary>
    /// <param name="bucket">Bucket to update</param>
    /// <param name="count">count to subtract</param>
    void SubtractCount(int bucket, int count)
    {
      Debug.Assert((uint)bucket < bucketsFill);
      Debug.Assert(count > 0);
      Debug.Assert(buckets[bucket].HasData);
      Debug.Assert(buckets[bucket].dataCount >= count);

      int b = bucket;
      while ((uint)b < bucketsReserved)
      {
        buckets[(uint)b].dataCount -= count;
        b = buckets[(uint)b].parent;
      }

      dataCount -= count;

      if (buckets[bucket].dataCount == 0 && buckets[bucket].prev + buckets[bucket].next != -2)
      {
        RemoveEmptyBucket(bucket);
      }
    }

    /// <summary>
    /// get (Data-)Bucket from Index
    /// </summary>
    /// <param name="index">index to search</param>
    /// <param name="innerIndex">fraction of the index inner the bucket</param>
    /// <returns>Bucket-ID</returns>
    int GetDataBucketFromIndex(int index, out int innerIndex)
    {
      Debug.Assert(index >= 0 && index < dataCount);

      int bucket = 0;

      for (; ; )
      {
        if (index >= buckets[bucket].dataCount)
        {
          index -= buckets[bucket].dataCount;
          bucket = buckets[bucket].next;
          continue;
        }
        if (buckets[bucket].HasData) break;
        bucket = buckets[bucket].childStart;
      }

      innerIndex = index;

      Debug.Assert(buckets[bucket].HasData);
      Debug.Assert(innerIndex >= 0 && innerIndex < buckets[bucket].dataCount);

      return bucket;
    }
    #endregion

    #region # // --- Validate ---
    IEnumerable<int> Validate_ScanNext(int firstBucket, int sameParent, HashSet<int> knownBuckets)
    {
      int limitCount = bucketsFill;
      for (; firstBucket >= 0; )
      {
        if (GetBucket(firstBucket).parent != sameParent) yield break;
        if (knownBuckets.Contains(firstBucket)) throw new Exception("validate: duplicate used bucket: " + firstBucket);
        knownBuckets.Add(firstBucket);
        yield return firstBucket;
        int next = GetBucket(firstBucket).next;
        if (next < 0) break;
        if (GetBucket(next).prev != firstBucket) throw new Exception("validate: invalid prev/next linking in " + next + ": " + GetBucket(next).prev + " != " + firstBucket);
        firstBucket = next;
        if (--limitCount < 0) throw new Exception("validate: found bucket endless loop at " + next);
      }
    }

    void Validate_Scan(int firstBucket, int parent, HashSet<int> knownBuckets)
    {
      var map = Validate_ScanNext(firstBucket, parent, knownBuckets).ToArray();
      int sumCount = map.Sum(bucket => buckets[bucket].dataCount);
      int parentCount = parent < 0 ? dataCount : buckets[parent].dataCount;
      if (sumCount != parentCount) throw new Exception("validate: count error [start: " + firstBucket + "]: " + sumCount + " != " + parentCount);

      // --- check Data-Status and parents ---
      bool hasData = buckets[map[0]].HasData;
      bool hasParentChanged = false;
      foreach (var b in map)
      {
        if (buckets[b].HasData != hasData) throw new Exception("validate: hasData-Error");
        if (buckets[b].parent == parent)
        {
          if (hasParentChanged) throw new Exception("validate: invalid reused parent " + parent + " in " + b);
        }
        else
        {
          if (parent != -1) throw new Exception("validate: root-parent error in " + b);
          hasParentChanged = true;
        }
      }

      // --- check Childs ---
      if (!hasData)
      {
        foreach (var b in map)
        {
          Validate_Scan(buckets[b].childStart, b, knownBuckets);
        }
      }
    }

    IEnumerable<int> Validate_ScanNext(int firstBucket, HashSet<int> knownBuckets)
    {
      int limitCount = bucketsFill;
      for (; firstBucket >= 0; )
      {
        if (knownBuckets.Contains(firstBucket)) throw new Exception("validate: duplicate used bucket: " + firstBucket);
        knownBuckets.Add(firstBucket);
        yield return firstBucket;
        int next = GetBucket(firstBucket).next;
        if (next < 0) break;
        if (GetBucket(next).prev != firstBucket) throw new Exception("validate: invalid prev/next linking in " + next + ": " + GetBucket(next).prev + " != " + firstBucket);
        firstBucket = next;
        if (--limitCount < 0) throw new Exception("validate: found bucket endless loop at " + next);
      }
    }

    void Validate_Scan(int firstBucket, HashSet<int> knownBuckets)
    {
      var map = Validate_ScanNext(firstBucket, knownBuckets).ToArray();
      int sumCount = map.Sum(bucket => buckets[bucket].dataCount);
      if (sumCount != dataCount) throw new Exception("validate: count error [start: " + firstBucket + "]: " + sumCount + " != " + dataCount);

      // --- check Data-Status ---
      bool hasData = buckets[map[0]].HasData;
      foreach (var b in map)
      {
        if (buckets[b].HasData != hasData) throw new Exception("validate: hasData-Error");
      }

      // --- check next level ---
      if (!hasData)
      {
        Validate_Scan(buckets[map[0]].childStart, knownBuckets);
      }
    }

    /// <summary>
    /// check the internal bucket consistency
    /// </summary>
    public void Validate()
    {
      // --- check last bucket ---
      for (int i = 0; i < bucketsFill; i++)
      {
        if (buckets[i].next == -1 && buckets[i].HasData && lastBucket != i) throw new Exception("validate: wrong last Bucket: " + i + " != " + lastBucket);
      }

      // --- check root bucket ---
      if (buckets[0].parent != -1 || buckets[0].prev != -1) throw new Exception("validate: invalid root-Bucket: " + buckets[0]);

      // --- child-search ---
      var knownBuckets = new HashSet<int>();
      Validate_Scan(0, -1, knownBuckets);
      if (knownBuckets.Count != bucketsFill) throw new Exception("validate: unused buckets found: " + string.Join(", ", Enumerable.Range(0, bucketsFill).Where(b => !knownBuckets.Contains(b))));

      // --- level-search ---
      knownBuckets.Clear();
      Validate_Scan(0, knownBuckets);
      if (knownBuckets.Count != bucketsFill) throw new Exception("validate: unused buckets found: " + string.Join(", ", Enumerable.Range(0, bucketsFill).Where(b => !knownBuckets.Contains(b))));

      // --- duplicate childs check ---
      var knownChilds = new HashSet<int>();
      for (int b = 0; b < bucketsFill; b++)
      {
        if (buckets[b].HasData) continue;
        if (knownChilds.Contains(buckets[b].childStart)) throw new Exception("validate: duplicate childs: " + buckets[b].childStart);
        knownChilds.Add(buckets[b].childStart);
      }

      // --- check empty buckets ---
      for (int b = bucketsFill; b < bucketsReserved; b++)
      {
        if (buckets[b].dataCount != 0 || buckets[b].prev != 0 || buckets[b].next != 0 || buckets[b].parent != 0 || buckets[b].childStart != 0)
        {
          throw new Exception("validate: unclean bucket [" + b + "]: " + buckets[b]);
        }
      }
    }

    public string DebugBuckets()
    {
#if UnsafeBuckets
      return "activated UnsafeBuckets";
#else
      var sb = new StringBuilder();
      foreach (var g in buckets.Take(bucketsFill).Select((b, i) => new { b, i }).GroupBy(x => x.b.parent))
      {
        var src = g.ToList();
        var dest = src.Take(0).ToList();
        for (var i = 0; i < src.Count; i++)
        {
          var b = src[i];
          if (src.Any(x => x.i == b.b.prev)) continue;
          dest.Add(b);
          src.RemoveAt(i);
          break;
        }
        if (dest.Count == 0)
        {
          dest.Add(src[0]);
          src.RemoveAt(0);
          sb.AppendLine("error-first");
        }
        while (src.Count > 0)
        {
          int find = -1;
          for (int i = 0; i < src.Count; i++)
          {
            if (src[i].i == dest[dest.Count - 1].b.next)
            {
              find = i;
            }
          }
          if (find < 0)
          {
            sb.AppendLine("error-connect");
            find = 0;
          }
          dest.Add(src[find]);
          src.RemoveAt(find);
        }
        foreach (var b in dest)
        {
          sb.AppendLine("[" + b.i + "] " + b.b);
        }
        sb.AppendLine();
      }
      return sb.ToString();
#endif
    }
    #endregion

    #region # // --- IList<T> ---
    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
    public void Clear()
    {
      if (NeedSafeData) Array.Clear(dataRaw, 0, dataFill);
      dataFill = MaxBucketSize;
      buckets[0] = new Bucket(0, -1, -1, -1, unchecked(0 - int.MinValue));
      bucketsFill = 1;
      lastBucket = 0;
      dataCount = 0;
    }

    /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    public int Count { get { return dataCount; } }

    /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
    /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
    public bool IsReadOnly { get { return false; } }

    /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    public bool Remove(T item)
    {
      int index = IndexOf(item);
      if (index < 0) return false;
      RemoveAt(index);
      return true;
    }

    Bucket GetBucket(int bucket)
    {
      return buckets[bucket];
    }

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
      int firstIndex;
      int b = GetDataBucketFromIndex(0, out firstIndex);
      Debug.Assert(firstIndex == 0);
      while (b >= 0)
      {
        var bucket = GetBucket(b);
        Debug.Assert(bucket.HasData);

        int end = bucket.DataEndOffset;
        for (int i = bucket.DataOffset; i < end; i++)
        {
          yield return dataRaw[i];
        }

        b = bucket.next;
      }
    }

    /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    public void Add(T item)
    {
      int bucket = lastBucket;

      if (buckets[bucket].dataCount == MaxBucketSize) // if bucket full?
      {
        bucket = GetNewDataBucket(bucket, -1, buckets[bucket].parent);
        buckets[lastBucket].next = bucket;
        lastBucket = bucket;
      }

      Debug.Assert(buckets[bucket].HasData);
      Debug.Assert(buckets[bucket].next == -1);

      dataRaw[buckets[bucket].DataEndOffset] = item;
      IncrementCount(bucket);
    }

    /// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
    public void Insert(int index, T item)
    {
      if (index >= dataCount)
      {
        if (index == dataCount)
        {
          Add(item);
          return;
        }
        throw new IndexOutOfRangeException("index");
      }

      int bucket = GetDataBucketFromIndex(index, out index);

      var bs = buckets;
      if (bs[bucket].dataCount == MaxBucketSize) // if bucket full?
      {
        int newBucket = SplitDataBucket(bucket);
        bs = buckets;
        if (index >= bs[bucket].dataCount)
        {
          bucket = newBucket;
          index -= bs[bucket].dataCount;
        }
      }

      int dataOffset = bs[bucket].DataOffset + index;
      MoveDataRight(dataOffset, dataOffset + 1, bs[bucket].dataCount - index);

      dataRaw[dataOffset] = item;
      IncrementCount(bucket);
    }

    /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
    /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    public bool Contains(T item)
    {
      return IndexOf(item) >= 0;
    }

    /// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
    /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
    public int IndexOf(T item)
    {
      var bs = buckets;

      var equalityComparer = EqualityComparer<T>.Default;
      for (int bucket = 0, index = 0; bucket >= 0; bucket = bs[bucket].next)
      {
        int end = bs[bucket].DataEndOffset;
        for (int i = bs[bucket].DataOffset; i < end; i++)
        {
          if (equalityComparer.Equals(dataRaw[i], item)) return index + i - bs[bucket].DataOffset;
        }
        index += bs[bucket].dataCount;
      }

      return -1;
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
      if (arrayIndex + dataCount > array.Length) throw new ArgumentOutOfRangeException("arrayIndex");

      int firstIndex;
      int firstBucket = GetDataBucketFromIndex(0, out firstIndex);
      Debug.Assert(firstIndex == 0);

      var bs = buckets;
      for (int bucket = firstBucket; bucket >= 0; bucket = bs[bucket].next)
      {
        Array.Copy(dataRaw, bs[bucket].DataOffset, array, arrayIndex, bs[bucket].dataCount);
        arrayIndex += bs[bucket].dataCount;
      }
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
        if ((uint)index >= dataCount) throw new IndexOutOfRangeException("index");

        int bucket = GetDataBucketFromIndex(index, out index);

        return dataRaw[buckets[bucket].DataOffset + index];
      }
      set
      {
        if ((uint)index >= dataCount) throw new IndexOutOfRangeException("index");

        int bucket = GetDataBucketFromIndex(index, out index);

        dataRaw[buckets[bucket].DataOffset + index] = value;
      }
    }

    /// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
    public void RemoveAt(int index)
    {
      if ((uint)index >= dataCount) throw new IndexOutOfRangeException("index");

      int bucket = GetDataBucketFromIndex(index, out index);

      MoveDataLeft(buckets[bucket].DataOffset + index + 1, buckets[bucket].DataOffset + index, buckets[bucket].dataCount - index - 1);

      if (NeedSafeData) dataRaw[buckets[bucket].DataEndOffset - 1] = default(T);

      DecrementCount(bucket);
    }
    #endregion

    #region # // --- additional Methods ---
    /// <summary>
    /// remove multiple items
    /// </summary>
    /// <param name="index">startposition in the list</param>
    /// <param name="count">count of items to remove</param>
    public void RemoveRange(int index, int count)
    {
      if ((uint)index >= dataCount) throw new IndexOutOfRangeException("index");
      if (count < 1) return;
      if (index + count > dataCount) throw new ArgumentOutOfRangeException("count");

      int localIndex;
      int bucket = GetDataBucketFromIndex(index, out localIndex);
      int next = buckets[bucket].next;

      // --- remove first items --
      if (localIndex > 0)
      {
        int removeItems = Math.Min(buckets[bucket].dataCount - localIndex, count);
        MoveDataLeft(buckets[bucket].DataOffset + localIndex + removeItems, buckets[bucket].DataOffset + localIndex, buckets[bucket].dataCount - localIndex - removeItems);
        if (NeedSafeData) Array.Clear(dataRaw, buckets[bucket].DataEndOffset - removeItems, removeItems); // clear last items
        SubtractCount(bucket, removeItems);
        count -= removeItems;
      }
      else next = bucket;

      while (count > 0)
      {
        if (next < bucketsFill)
        {
          bucket = next;
        }
        else
        {
          bucket = GetDataBucketFromIndex(index, out localIndex);
          Debug.Assert(localIndex == 0);
        }
        next = buckets[bucket].next;
        Debug.Assert((uint)bucket < bucketsFill);

        // --- remove last items ---
        if (count < buckets[bucket].dataCount)
        {
          MoveDataLeft(buckets[bucket].DataOffset + count, buckets[bucket].DataOffset, buckets[bucket].dataCount - count);
          if (NeedSafeData) Array.Clear(dataRaw, buckets[bucket].DataEndOffset - count, count); // clear last items
          SubtractCount(bucket, count);
          break;
        }

        // --- remove all items from a bucket ---
        if (NeedSafeData) Array.Clear(dataRaw, buckets[bucket].DataOffset, buckets[bucket].dataCount); // clear all items
        count -= buckets[bucket].dataCount;
        SubtractCount(bucket, buckets[bucket].dataCount);
      }
    }
    #endregion
  }

  class Program : ConsoleExtras
  {
    #region # // --- Memtest ---
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
    #endregion

    static void FullCompare(IList<int> org, IList<int> cmp, bool checkDetail)
    {
      if (checkDetail)
      {
        var b1array = cmp.ToArray();
        int c = 0;
        foreach (var v in cmp)
        {
          if (org[c] != v)
          {
            throw new Exception("value dif [" + c + "]: " + v + " != " + org[c]);
          }
          if (v != b1array[c])
          {
            throw new Exception("array error [" + c + "]: " + v + " != " + b1array[c]);
          }
          if (v != cmp[c])
          {
            throw new Exception("indexer error [" + c + "]: " + v + " != " + cmp[c]);
          }
          c++;
        }
      }
      else
      {
        var orgArray = org.ToArray();
        var cmpArray = cmp.ToArray();
        if (orgArray.Length != cmpArray.Length) throw new Exception("?");
        for (int i = 0; i < cmpArray.Length; i++)
        {
          if (cmpArray[i] != orgArray[i])
          {
            throw new Exception("value dif [" + i + "]: " + cmpArray[i] + " != " + orgArray[i]);
          }
        }
      }
    }

    static void BucketTest()
    {
      const int Validate = 0;
      const int Count = 10000000;
      const bool Remover = true;

      var b1 = new BucketList<int>();
      var b2 = new List<int>();
      var rnd = new Random(12345);

      var time = Stopwatch.StartNew();
      int tick = Environment.TickCount;
      for (int i = 0; i < Count; i++)
      {
        if ((i & 0xff) == 0 && tick != Environment.TickCount)
        {
          tick = Environment.TickCount;
          Console.WriteLine(i.ToString("N0") + " / " + Count.ToString("N0") + " (" + b1.Count.ToString("N0") + ")");
        }

        int next = rnd.Next(b1.Count + 1);

        b1.Insert(next, i);
        if (Validate > 0) b2.Insert(next, i);

        if (Validate > 1)
        {
          b1.Validate();
          if (Validate > 3) FullCompare(b2, b1, true);
        }

        if (Remover)
        {
          int removeCount = Math.Max(0, rnd.Next(rnd.Next(rnd.Next(b1.Count + 1))) - Count / 10);
          if (removeCount > 0)
          {
            if (rnd.Next(2) == 0) // chunk remove
            {
              int pos = rnd.Next(b1.Count - removeCount);
              if (Validate > 0)
              {
                for (int x = removeCount - 1; x >= 0; x--) b2.RemoveAt(pos + x);
              }

              b1.RemoveRange(pos, removeCount);
              //for (int x = 0; x < removeCount; x++) b1.RemoveAt(pos);

              if (Validate > 1) b1.Validate();
            }
            else // random remove
            {
              for (int r = 0; r < removeCount; r++)
              {
                next = rnd.Next(b1.Count);

                b1.RemoveAt(next);

                if (Validate > 1) b1.Validate();

                if (Validate > 0)
                {
                  b2.RemoveAt(next);
                  if (b1.Count != b2.Count) throw new Exception();
                }
              }
            }
          }
        }

        if (Validate > 0)
        {
          if (b1.Count != b2.Count) throw new Exception();
          if (Validate > 2) FullCompare(b1, b2, Validate > 3);
        }
      }

      time.Stop();
      Console.WriteLine(time.ElapsedMilliseconds.ToString("N0") + " ms");

      if (Validate > 0)
      {
        b1.Validate();
        int c = 0;
        foreach (var v in b1)
        {
          if (b2[c] != v)
          {
            File.WriteAllText(@"C:\Users\Max\Desktop\prog\DBs\lol-dumm.txt", string.Join("\r\n", b1));
            File.WriteAllText(@"C:\Users\Max\Desktop\prog\DBs\lol-soll.txt", string.Join("\r\n", b2));
            throw new Exception();
          }
          c++;
        }
        FullCompare(b2, b1, true);
      }
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
