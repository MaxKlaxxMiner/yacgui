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
using YacGui.Core;
using YacGui.Core.SimpleBoard;

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
  class Program : ConsoleExtras
  {
    /// <summary>
    /// Matt in 14 mit Turm (alle Schritte)
    /// </summary>
    static readonly string[] MatesWithRook =
    {
      "5R1k/8/6K1/8/8/8/8/8 b - - 27 14",  // -M0 (no moves = Mate)
      "7k/5R2/6K1/8/8/8/8/8 w - - 26 14",  // +M1
      "6k1/5R2/6K1/8/8/8/8/8 b - - 25 13", // -M1
      "6k1/5R2/5K2/8/8/8/8/8 w - - 24 13", // +M2
      "7k/5R2/5K2/8/8/8/8/8 b - - 23 12",  // -M2
      "7k/5R2/8/6K1/8/8/8/8 w - - 22 12",  // +M3
      "8/5R1k/8/6K1/8/8/8/8 b - - 21 11",  // -M3
      "8/7k/5R2/6K1/8/8/8/8 w - - 20 11",  // +M4
      "8/6k1/5R2/6K1/8/8/8/8 b - - 19 10", // -M4
      "8/6k1/5R2/5K2/8/8/8/8 w - - 18 10", // +M5
      "8/7k/5R2/5K2/8/8/8/8 b - - 17 9",   // -M5
      "8/7k/5R2/4K3/8/8/8/8 w - - 16 9",   // +M6
      "8/6k1/5R2/4K3/8/8/8/8 b - - 15 8",  // -M6
      "8/6k1/7R/4K3/8/8/8/8 w - - 14 8",   // +M7
      "8/5k2/7R/4K3/8/8/8/8 b - - 13 7",   // -M7
      "8/5k2/7R/3K4/8/8/8/8 w - - 12 7",   // +M8
      "8/4k3/7R/3K4/8/8/8/8 b - - 11 6",   // -M8
      "8/4k3/7R/8/3K4/8/8/8 w - - 10 6",   // +M9
      "8/8/3k3R/8/3K4/8/8/8 b - - 9 5",    // -M9
      "8/8/3k4/7R/3K4/8/8/8 w - - 8 5",    // +M10
      "8/8/2k5/7R/3K4/8/8/8 b - - 7 4",    // -M10
      "8/8/2k5/7R/8/3K4/8/8 w - - 6 4",    // +M11
      "8/8/3k4/7R/8/3K4/8/8 b - - 5 3",    // -M11
      "8/8/3k4/7R/8/8/4K3/8 w - - 4 3",    // +M12
      "8/8/4k3/7R/8/8/4K3/8 b - - 3 2",    // -M12
      "8/8/4k3/7R/8/8/8/4K3 w - - 2 2",    // +M13
      "8/8/8/4k2R/8/8/8/4K3 b - - 1 1",    // -M13
      "8/8/8/4k3/8/8/8/4K2R w - - 0 1",    // +M14
    };

    static void MateScan()
    {
      var board = new Board();
      //board.SetFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"); // Startaufstellung

      //board.SetFEN("8/8/8/4k3/8/Q7/Q7/K7 w - - 0 1"); // Dame + Dame Mattsuche (Matt in 3)
      //board.SetFEN("8/8/8/4k3/8/Q7/R7/K7 w - - 0 1"); // Dame + Turm Mattsuche (Matt in 5)
      //board.SetFEN("8/8/8/4k3/8/R7/R7/K7 w - - 0 1"); // Turm + Turm Mattsuche (Matt in 7)
      //board.SetFEN("7k/5n2/8/8/8/8/5Q2/K7 w - - 0 1"); // Dame Mattsuche (Matt in 12)
      //board.SetFEN("8/5rK1/6R1/8/4k3/8/8/8 w - - 0 1"); // Turm Mattsuche (Matt in 15)
      //board.SetFEN("8/8/4k3/8/8/8/8/K2BB3 w - - 0 1"); // Läufer + Läufer Mattsuche (Matt in 17)
      //board.SetFEN("8/8/8/8/3k4/8/N7/KB6 w - - 0 1"); // Läufer + Springer Mattsuche (Matt in 31)
      //board.SetFEN("8/8/4k3/3bn3/8/4Q3/8/K7 w - - 0 1"); // Dame gegen Läufer + Springer Mattsuche (Matt in 39)
      //board.SetFEN("5k2/5P1P/4P3/pP6/P6q/3P2P1/2P5/K7 w - a6 0 1"); // Bauern-Test (Matt in 6)

      //board.SetFEN("8/8/8/8/3k4/8/NN6/KN6 w - - 0 1"); // drei Springer vorhanden (Matt in 18)
      //board.SetFEN("8/8/8/8/3k4/8/N7/KN6 w - - 0 1"); // nur zwei Springer vorhanden = Remis
      //board.SetFEN("8/8/8/8/3k4/8/N7/K7 w - - 0 1"); // nur ein Springer vorhanden = Remis
      //board.SetFEN("8/8/8/8/3k4/8/8/K7 w - - 0 1"); // keine Figur mehr vorhanden = Remis

      //var result = MateScanner.RunScan(board);

      for (int i = 0; i < MatesWithRook.Length; i++)
      {
        board.SetFEN(MatesWithRook[i]);
        var result = MateScanner.RunScan(board);
        Console.WriteLine("Check {0} = {1}", i, result.TxtInfo());
      }
    }

    /// <summary>
    /// TestTool program entry
    /// </summary>
    static void Main()
    {
      ConsoleHead("Test Tool: " + MainForm.FullName);

      MateScan();

      //BitmapTests.Run();
      //MemTest();
    }
  }
}
