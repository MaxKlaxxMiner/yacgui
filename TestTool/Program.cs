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
  unsafe class Program : ConsoleExtras
  {
    #region # // --- Memtest ---
    static void PrintDebug(MiniMemoryManager mem, MiniMemoryManager.Entry[] entries)
    {
      mem.Validate(entries);
      Console.WriteLine("------------------------\r\n" +
                        string.Join("\r\n", entries.OrderBy(x => x.ofs).Select(x => "          " + x)) +
                        "\r\n------------------------ " +
                        "(fragmented: " + (100.0 / mem.dataFilled * mem.dataFragmented).ToString("N2") + " %, reserved: " + (mem.dataSize / 1048576.0).ToString("N1") + " MB) ------");
    }

    static void MemTest()
    {
      var mem = new MiniMemoryManager();

      var rnd = new Random(12345);

      var entries = new BucketList<MiniMemoryManager.Entry>();
      var entriesByte = new BucketList<byte>();

      var time = Stopwatch.StartNew();

      for (int i = 0; i < 10000000; i++)
      {
        if (rnd.Next(entries.Count + 1080) < entries.Count) // löschen
        {
          if (entries.Count > 0)
          {
            int kill = rnd.Next(entries.Count);
            var checkByte = entriesByte[kill];

            var entry = entries[kill];
            fixed (byte* ptr = &mem.data[entry.ofs])
            {
              for (uint x = 0; x < entry.len; x++)
              {
                if (ptr[x] != (byte)(checkByte + x)) throw new Exception();
              }
            }

            mem.Free(entries[kill]);

            entries.RemoveAt(kill);
            entriesByte.RemoveAt(kill);
          }
        }
        else // reservieren
        {
          ulong size = (ulong)rnd.Next(1920);
          var newEntry = mem.Alloc(size);
          entries.Add(newEntry);
          entriesByte.Add((byte)newEntry.ofs);

          fixed (byte* ptr = &mem.data[newEntry.ofs])
          {
            for (uint x = 0; x < newEntry.len; x++)
            {
              ptr[x] = (byte)(newEntry.ofs + x);
            }
          }
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


    /// <summary>
    /// TestTool program entry
    /// </summary>
    static void Main()
    {
      ConsoleHead("Test Tool: " + MainForm.FullName);

      //var comp = new CompressedBitmap(1920, 1080);

      //BitmapTests.Run();
      MemTest();
    }
  }
}
