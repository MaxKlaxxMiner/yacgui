using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using YacGui.Core.SimpleBoard;
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantIfElseBlock
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace YacGui.Core
{
  /// <summary>
  /// Klasse zum Suchen von Matt-Varianten
  /// </summary>
  public static class MateScanner
  {
    /// <summary>
    /// Debug-Anzeigen (0 - 4)
    /// </summary>
    static readonly int VerboseDebug = 1;

    #region # public enum Result : ushort // bewertetes Ergebnis, welche eine Stellung annehmen kann
    /// <summary>
    /// bewertetes Ergebnis, welche eine Stellung annehmen kann
    /// </summary>
    [Flags]
    public enum ResultState : ushort
    {
      /// <summary>
      /// Ergebnis unbekannt
      /// </summary>
      None = 0x0000,

      /// <summary>
      /// gesamte Status-Maske
      /// </summary>
      ResultMask = WinMask | CannotWinMask,

      /// <summary>
      /// Bitmaske für die Marker "garantierte Gewinnmöglichkeit"
      /// </summary>
      WinMask = WhiteWins | BlackWins,
      /// <summary>
      /// Weiß gewinnt in x Halbzügen
      /// </summary>
      WhiteWins = 0x8000,
      /// <summary>
      /// Schwarz gewinnt in x Halbzügen
      /// </summary>
      BlackWins = 0x4000,

      /// <summary>
      /// Bitmaske für die Marker "keine Gewinnmöglichkeit" - Remis in x Halbzügen
      /// </summary>
      CannotWinMask = WhiteCannotWin | BlackCannotWin,
      /// <summary>
      /// Weiß kann nicht mehr gewinnen
      /// </summary>
      WhiteCannotWin = 0x2000,
      /// <summary>
      /// Schwarz kann nicht mehr gewinnen
      /// </summary>
      BlackCannotWin = 0x1000,

      /// <summary>
      /// Bitmaske für die Anzahl der Halbzüge
      /// </summary>
      MaskHalfmoves = 0x0fff
    }

    /// <summary>
    /// gibt das erreichbare Spielergebnis als lesbare Zeichenkette zurück
    /// </summary>
    /// <param name="state">Ergebnis, welches betroffen ist</param>
    /// <returns>lesbare Zeichenkette</returns>
    public static string TxtInfo(this ResultState state)
    {
      int halfmoves = (int)(state & ResultState.MaskHalfmoves);
      if ((state & ResultState.WhiteWins) == ResultState.WhiteWins)
      {
        if (halfmoves % 2 == 0)
        {
          return "-M" + halfmoves / 2;
        }
        else
        {
          return "+M" + (halfmoves + 1) / 2;
        }
      }
      else if ((state & ResultState.BlackWins) == ResultState.BlackWins)
      {
        if (halfmoves % 2 == 0)
        {
          return "+M" + halfmoves / 2;
        }
        else
        {
          return "-M" + (halfmoves + 1) / 2;
        }
      }
      else if ((state & ResultState.CannotWinMask) == ResultState.CannotWinMask)
      {
        return "=R" + (halfmoves + 1) / 2;
      }
      return state.ToString();
    }
    #endregion

    #region # static Result CheckSimpleState(IBoard board, bool checkMaterial = false) // prüft den Status des Schachbrettes (ob z.B. bereits Matt gesetzt wurde)
    /// <summary>
    /// Positionsangaben des Schachbrettes sortiert um schneller (abwechselnd) weiße und schwarze Figuren schneller auf dem Brett zu finden
    /// </summary>
    static readonly int[] MaterialCheckPosOrder = Enumerable.Range(0, IBoard.Width * IBoard.Height).Select(x => x % 2 == 0 ? x / 2 : IBoard.Width * IBoard.Height - 1 - x / 2).ToArray();

    /// <summary>
    /// prüft den Status des Schachbrettes (ob z.B. bereits Matt gesetzt wurde)
    /// </summary>
    /// <param name="board">Spielbrett, welches geprüft werden soll</param>
    /// <returns>einfaches Zwischenergebnis, ob ein Matt, Patt oder einfaches Remis erkannt wurde</returns>
    static ResultState CheckSimpleState(IBoard board)
    {
      if (!board.HasMoves) // sind keine Züge mehr möglich? = Matt oder Patt gefunden
      {
        if (board.IsMate()) // Matt wurde gesetzt
        {
          return board.WhiteMove
            ? ResultState.BlackWins | ResultState.WhiteCannotWin
            : ResultState.WhiteWins | ResultState.BlackCannotWin;
        }

        // Patt wurde erreicht = Remis
        return ResultState.WhiteCannotWin | ResultState.BlackCannotWin;
      }

      // --- Material zu Mattsetzen prüfen ---
      int materialWhite = 0;
      int materialBlack = 0;
      for (int i = 0; i < MaterialCheckPosOrder.Length; i++)
      {
        var piece = board.GetField(MaterialCheckPosOrder[i]);
        if (piece == Piece.None) continue;
        switch (piece)
        {
          case Piece.WhiteQueen:
          case Piece.WhiteRook:
          case Piece.WhitePawn: materialWhite += 3; if (materialBlack > 2) i = IBoard.Width * IBoard.Height; break;
          case Piece.WhiteBishop: materialWhite += 2; if (materialBlack > 2 && materialWhite > 2) i = IBoard.Width * IBoard.Height; break;
          case Piece.WhiteKnight: materialWhite++; if (materialBlack > 2 && materialWhite > 2) i = IBoard.Width * IBoard.Height; break;

          case Piece.BlackQueen:
          case Piece.BlackRook:
          case Piece.BlackPawn: materialBlack += 3; if (materialWhite > 2) i = IBoard.Width * IBoard.Height; break;
          case Piece.BlackBishop: materialBlack += 2; if (materialWhite > 2 && materialBlack > 2) i = IBoard.Width * IBoard.Height; break;
          case Piece.BlackKnight: materialBlack++; if (materialWhite > 2) i = IBoard.Width * IBoard.Height; break;
        }
      }
      var state = ResultState.None;
      if (materialWhite < 3) state |= ResultState.WhiteCannotWin;
      if (materialBlack < 3) state |= ResultState.BlackCannotWin;
      return state;
    }
    #endregion

    #region # struct HashElement // Daten-Struktur der Hashtable
    /// <summary>
    /// Daten-Struktur der Hashtable
    /// </summary>
    struct HashElement
    {
      /// <summary>
      /// Prüfsummen der vorherigen Stellungen
      /// </summary>
      public ulong[] parentCrcs;

      /// <summary>
      /// aktuelles Ergebnis der Stellung
      /// </summary>
      public ResultState state;

      /// <summary>
      /// merkt sich die aktuelle Stellung
      /// </summary>
      public string fen;

      /// <summary>
      /// Konstruktor
      /// </summary>
      /// <param name="parentCrc">Prüfsumme der vorherigen Stellung</param>
      /// <param name="fen">FEN der aktuellen Stellung</param>
      /// <param name="state">aktueller Ergebnis-Status der Stellung</param>
      public HashElement(ulong parentCrc, ResultState state, string fen)
      {
        parentCrcs = new[] { parentCrc };
        this.state = state;
        this.fen = fen;
      }

      /// <summary>
      /// gibt den Inhalt als lesbare Zeichenkette zurück
      /// </summary>
      /// <returns>lesbare Zeichenkette</returns>
      public override string ToString()
      {
        return new { state = state.TxtInfo(), parentCrcs = string.Join(", ", parentCrcs), fen }.ToString();
      }
    }
    #endregion

    /// <summary>
    /// führt eine Mattsuche durch und gibt den bestmöglichen Zug bei perfektem Spiel zurück (sofern nicht abgebrochen, sonst: Unknown)
    /// </summary>
    /// <param name="board">Spielbrett mit der Stellung, welche durchsucht werden soll</param>
    /// <param name="maxDepth">maximale Suchtiefe in Halbzügen</param>
    /// <param name="cancel">optionale Abbruchbedinung</param>
    /// <returns>Bestmöglicher Zug und entsprechendes Ergebnis</returns>
    public static ResultState RunScan(IBoard board, int maxDepth = 250, Func<bool> cancel = null)
    {
      if (cancel == null) cancel = () => false;
      var b = new Board();
      b.SetFEN(board.GetFEN());

      var state = CheckSimpleState(b);
      Debug.Assert(AllowedResults.Contains(state));

      if ((state & ResultState.WinMask) != 0 || (state & ResultState.CannotWinMask) == ResultState.CannotWinMask) // Spielende wurde jetzt schon erreicht?
      {
        return state;
      }

      ulong baseCrc = b.GetChecksum();
      var hashTable = new Dictionary<ulong, HashElement> { { baseCrc, new HashElement(0, state, b.GetFEN()) { parentCrcs = new ulong[0] } } };
      var nextFens = new HashSet<string> { b.GetFEN() };
      if (VerboseDebug > 0)
      {
        parentUpdates = 0;
        Display(hashTable, baseCrc, b.WhiteMove, "init", false);
      }

      for (int depth = 0; depth < maxDepth; depth++)
      {
        if (cancel()) break;

        if (VerboseDebug > 0)
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine("\r\n   --- Depth: " + (depth + 1) + ", Nodes: " + nextFens.Count.ToString("N0") + ", Hash: " + hashTable.Count.ToString("N0") + ", Parent-Updates: " + parentUpdates.ToString("N0") + " ---\r\n");
          Console.ForegroundColor = ConsoleColor.Gray;
          Thread.Sleep(VerboseDebug > 1 ? 1000 : 250);
        }

        var scanFens = nextFens.ToArray();
        nextFens.Clear();

        for (int scanIndex = 0; scanIndex < scanFens.Length; scanIndex++)
        {
          b.SetFEN(scanFens[scanIndex]);
          ulong scanCrc = b.GetChecksum();
          var boardInfos = b.BoardInfos;
          var moves = b.GetMovesArray();
          var moveCrcs = new ulong[moves.Length];
          for (var moveIndex = 0; moveIndex < moves.Length; moveIndex++)
          {
            b.DoMoveFast(moves[moveIndex]);
            ulong crc = b.GetChecksum();
            moveCrcs[moveIndex] = crc;
            HashElement hash;
            if (hashTable.TryGetValue(crc, out hash))
            {
              state = hash.state;
              Debug.Assert(AllowedResults.Contains(state & ResultState.ResultMask));
              Debug.Assert(!hash.parentCrcs.Contains(scanCrc)); // übergeordnete Positionen sollten nicht doppelt vorhanden sein

              // --- neuer übergeordnete Position verknüpfen ---
              Array.Resize(ref hash.parentCrcs, hash.parentCrcs.Length + 1);
              hash.parentCrcs[hash.parentCrcs.Length - 1] = scanCrc;
              hashTable[crc] = hash;
              if (VerboseDebug > 2) Display(hashTable, crc, b.WhiteMove, "add parent, move " + (moveIndex + 1) + "/" + moves.Length + " (" + moves[moveIndex] + ") - " + (scanIndex + 1) + "/" + scanFens.Length, VerboseDebug > 3);
            }
            else
            {
              state = CheckSimpleState(b);
              Debug.Assert(AllowedResults.Contains(state & ResultState.ResultMask));

              // --- ersten Hashtable-Eintrag Erstellen ---
              hash = new HashElement(scanCrc, state, b.GetFEN());
              hashTable.Add(crc, hash);
              if (VerboseDebug > 2) Display(hashTable, crc, b.WhiteMove, "insert hash, move " + (moveIndex + 1) + "/" + moves.Length + " (" + moves[moveIndex] + ") - " + (scanIndex + 1) + "/" + scanFens.Length, VerboseDebug > 3);

              // --- Position für weitere Untersuchungen vormerken, sofern noch kein eindeutiges Ergebnis bekannt ist ---
              if ((state & ResultState.WinMask) == 0 && (state & ResultState.CannotWinMask) != ResultState.CannotWinMask)
              {
                nextFens.Add(hash.fen);
              }
            }
            b.DoMoveBackward(moves[moveIndex], boardInfos);
          }

          UpdateParentStates(hashTable, scanCrc, moveCrcs, b);
        }

        // --- Ende schon erreicht? ---
        state = hashTable[baseCrc].state;
        if ((state & ResultState.WinMask) != 0 || (state & ResultState.CannotWinMask) == ResultState.CannotWinMask) // Spielende wurde erreicht?
        {
          // todo: eventuell wurde nicht die kürzeste Variante gefunden
          return state;
        }
      }

      return state;
    }

    /// <summary>
    /// erlaubte Zustände von (Zwischen-) Ergebnissen, muss vor mit <see cref="ResultState.ResultMask"/> maskiert werden
    /// </summary>
    static readonly HashSet<ResultState> AllowedResults = new HashSet<ResultState>
    {
      ResultState.None,                                   // Weiß [offen],               Schwarz [offen]
      ResultState.BlackCannotWin,                         // Weiß [offen],               Schwarz [kann nicht gewinnen]
      ResultState.WhiteCannotWin,                         // Weiß [kann nicht gewinnen], Schwarz [offen]
      ResultState.CannotWinMask,                          // Weiß [kann nicht gewinnen], Schwarz [kann nicht gewinnen]   ->  1/2 - 1/2
      ResultState.WhiteWins | ResultState.BlackCannotWin, // Weiß [gewinnt],             Schwarz [kann nicht gewinnen]   ->    1 - 0
      ResultState.BlackWins | ResultState.WhiteCannotWin, // Weiß [kann nicht gewinnen], Schwarz [gewinnt]               ->    0 - 1
    };

    /// <summary>
    /// vergleicht zwei Ergbnisse und gibt das bessere für Weiß zurück
    /// </summary>
    /// <param name="s1">erstes Ergebnis</param>
    /// <param name="s2">zweiten Ergebnis</param>
    /// <returns>besseres Ergebnis</returns>
    static ResultState CompareStateWhite(ResultState s1, ResultState s2)
    {
      switch (s1 & ResultState.ResultMask)
      {
        case ResultState.None: return s2;                                        // s1 = N/A, s2 = etwas anderes   -> s2 besser

        case ResultState.CannotWinMask:
        {
          switch (s2 & ResultState.ResultMask)
          {
            case ResultState.WhiteCannotWin: return s1;                          // s1 = Remis, s2 = kein Gewinn   -> s1 besser
            case ResultState.BlackWins | ResultState.WhiteCannotWin: return s1;  // s1 = Remis, s2 = Verlust       -> s1 besser
            case ResultState.BlackCannotWin: return s2;                          // s1 = Remis, s2 = Unbekannt     -> s2 besser
            case ResultState.WhiteWins | ResultState.BlackCannotWin: return s2;  // s1 = Remis, s2 = Gewinn        -> s2 besser
            case ResultState.CannotWinMask: return s1 < s2 ? s1 : s2;            // s1 = Remis, s2 = Remis         -> kürzere Variante bevorzugen
            default: throw new NotSupportedException();
          }
        }

        case ResultState.WhiteCannotWin:
        {
          switch (s2 & ResultState.ResultMask)
          {
            case ResultState.CannotWinMask: return s2;                           // s1 = kein Gewinn, s2 = Remis   -> s2 besser
            case ResultState.BlackWins | ResultState.WhiteCannotWin: return s1;  // s1 = kein Gewinn, s2 = Verlust -> s1 besser
            default: throw new NotSupportedException();
          }
        }

        case ResultState.WhiteCannotWin | ResultState.BlackWins:
        {
          switch (s2 & ResultState.ResultMask)
          {
            case ResultState.WhiteCannotWin: return s2;                          // s1 = Verlust, s2 = kein Gewinn -> s2 besser
            case ResultState.CannotWinMask: return s2;                           // s1 = Verlust, s2 = Remis       -> s2 besser
            case ResultState.BlackWins | ResultState.WhiteCannotWin: return s1 > s2 ? s1 : s2; // s1 = Verlust, s2 = Verlust -> längere Variante bevorzugen
            default: throw new NotSupportedException();
          }
        }

        case ResultState.BlackCannotWin:
        {
          switch (s2 & ResultState.ResultMask)
          {
            case ResultState.CannotWinMask: return s1;                           // s1 = unbekannt, s2 = Remis     -> s1 besser
            case ResultState.WhiteWins | ResultState.BlackCannotWin: return s2;  // s1 = unbekannt, s2 = Gewinn    -> s2 besser
            default: throw new NotSupportedException();
          }
        }

        case ResultState.WhiteWins | ResultState.BlackCannotWin:
        {
          switch (s2 & ResultState.ResultMask)
          {
            case ResultState.BlackCannotWin: return s1;                          // s1 = Gewinn, s2 = unbekannt    -> s1 besser
            case ResultState.CannotWinMask: return s1;                           // s1 = Gewinn, s2 = Remis        -> s1 besser
            case ResultState.WhiteWins | ResultState.BlackCannotWin: return s1 < s2 ? s1 : s2; // s1 = Gewinn, s2 = Gewinn -> kürzere Variante bevorzugen
            default: throw new NotSupportedException();
          }
        }

        default: throw new NotSupportedException();
      }
    }

    /// <summary>
    /// wechselt den Status zwischen Weiß und Schwarz
    /// </summary>
    /// <param name="s">Status, welcher gewechselt werden soll</param>
    /// <returns>fertig gewechselter Status</returns>
    static ResultState SwapWhiteBlackState(ResultState s)
    {
      return (s & ResultState.MaskHalfmoves) |
            ((s & ResultState.WhiteCannotWin) == ResultState.WhiteCannotWin ? ResultState.BlackCannotWin : ResultState.None) |
            ((s & ResultState.BlackCannotWin) == ResultState.BlackCannotWin ? ResultState.WhiteCannotWin : ResultState.None) |
            ((s & ResultState.WhiteWins) == ResultState.WhiteWins ? ResultState.BlackWins : ResultState.None) |
            ((s & ResultState.BlackWins) == ResultState.BlackWins ? ResultState.WhiteWins : ResultState.None);
    }

    /// <summary>
    /// vergleicht zwei Ergbnisse und gibt das bessere zurück
    /// </summary>
    /// <param name="s1">erstes Ergebnis</param>
    /// <param name="s2">zweiten Ergebnis</param>
    /// <param name="whiteMove">gibt an, ob für Weiß optimiert werden soll</param>
    /// <returns>besseres Ergebnis</returns>
    static ResultState CompareState(ResultState s1, ResultState s2, bool whiteMove)
    {
      if (s1 == s2) return s1; // kein Unterschied erkannt

      if (whiteMove) // --- nach Weißen Zügen optimieren ---
      {
        return CompareStateWhite(s1, s2);
      }
      else // --- nach Schwarzen Zügen optimieren ---
      {
        return SwapWhiteBlackState(CompareStateWhite(SwapWhiteBlackState(s1), SwapWhiteBlackState(s2)));
      }
    }

    /// <summary>
    /// berechnet das beste Ergebnis aus mehreren Zügmöglichkeiten (sofern möglich)
    /// </summary>
    /// <param name="hashTable">Hashtable, welche die Zwischenergebnisse enthält</param>
    /// <param name="currentCrc">Prüfsumme der aktuellen Stellung</param>
    /// <param name="moveCrcs">Prüfsummen der Positionen, welche durch die Züge erreichbar sind (müssen alle bereits in der Hashtable enthalten sein)</param>
    /// <param name="whiteMoves">gibt an, ob aus den Zügen das optimum für Weiß gesucht werden soll (sonst: Schwarz)</param>
    /// <returns>beste Ergebnis</returns>
    static ResultState BestState(Dictionary<ulong, HashElement> hashTable, ulong currentCrc, ulong[] moveCrcs, bool whiteMoves)
    {
      var bestState = ResultState.None;
      foreach (var moveCrc in moveCrcs)
      {
        var state = hashTable[moveCrc].state;
        if ((state & ResultState.WinMask) != 0 || (state & ResultState.CannotWinMask) == ResultState.CannotWinMask) state++; // wenn ein Spielende in Sicht ist, den Counter hoch zählen

        bestState = CompareState(bestState, state, whiteMoves); // mit bester Variante vergleichen und ggf. übernehmen
      }

      if (bestState == ResultState.None) bestState = hashTable[currentCrc].state;

      Debug.Assert(AllowedResults.Contains(bestState & ResultState.ResultMask));

      return bestState;
    }

    /// <summary>
    /// zeigt den Hash-Inhalt einer bestimmten Stellung an
    /// </summary>
    /// <param name="hashTable">Hashtable, welche alle Positionen enthält</param>
    /// <param name="crc">Prüfsumme der aktuellen Stellung</param>
    /// <param name="whiteMoves">Ist Weiß am Zug?</param>
    /// <param name="name">optionaler Name</param>
    /// <param name="readline">gibt an, ob auf die Enter-Taste gewartet wird</param>
    static void Display(Dictionary<ulong, HashElement> hashTable, ulong crc, bool whiteMoves, string name, bool readline = true)
    {
      var hash = hashTable[crc];
      Console.WriteLine();
      BoardTools.PrintBoard(hash.fen);
      Console.WriteLine();
      Console.WriteLine("    " + hash.fen.Split(' ')[1].ToUpper() + (whiteMoves ? "W" : "B") + " " + hash.state.TxtInfo() + ": " + (hash.state & ResultState.ResultMask) + " - " + (int)(hash.state & ResultState.MaskHalfmoves) + " (" + name + ")");
      if (readline) Console.ReadLine(); else Console.WriteLine();
    }

    /// <summary>
    /// merkt sich die Anzahl der übergeordneten Updates
    /// </summary>
    static long parentUpdates;

    /// <summary>
    /// übergeordnete Positionen in der Hashtable aktualisieren (sofern notwendig)
    /// </summary>
    /// <param name="hashTable">Hashtable, welche aktualisiert werden soll</param>
    /// <param name="currentCrc">Prüfsumme der aktuellen Stellung</param>
    /// <param name="moveCrcs">Prüfsummen der Positionen, welche erreichbar sind</param>
    /// <param name="board">Schachbrett zur weiteren Verwendung</param>
    static void UpdateParentStates(Dictionary<ulong, HashElement> hashTable, ulong currentCrc, ulong[] moveCrcs, IBoard board)
    {
      bool whiteMoves = board.WhiteMove; // gibt an, ob aus den Zügen das optimum für Weiß gesucht werden soll (sonst: Schwarz)
      if (VerboseDebug > 2)
      {
        Console.WriteLine("\r\n    --- Best Scan ---");
        for (int m = 0; m < moveCrcs.Length; m++)
        {
          Display(hashTable, moveCrcs[m], !whiteMoves, "scan best " + (m + 1) + "/" + moveCrcs.Length, false);
        }
      }
      var bestState = BestState(hashTable, currentCrc, moveCrcs, whiteMoves);
      if (VerboseDebug > 1)
      {
        Display(hashTable, currentCrc, whiteMoves, "Best from " + moveCrcs.Length, false);
        Console.WriteLine("    Best [" + currentCrc + "]: " + bestState.TxtInfo() + ": " + (bestState & ResultState.ResultMask) + " - " + (int)(bestState & ResultState.MaskHalfmoves));
      }

      var hash = hashTable[currentCrc];
      while (hash.state != bestState) // Änderung erkannt? -> übergeordnete Knoten ebenfalls aktualisieren
      {
        hash.state = bestState;
        hashTable[currentCrc] = hash;
        if (VerboseDebug > 0)
        {
          parentUpdates++;
          if (VerboseDebug > 1 || parentUpdates % 1000 == 0 || parentUpdates == 1)
          {
            Console.Title = "Hash: " + hashTable.Count.ToString("N0") + ", Updates: " + parentUpdates.ToString("N0");
          }
        }

        foreach (var parentCrc in hash.parentCrcs)
        {
          var parentHash = hashTable[parentCrc];
          board.SetFEN(parentHash.fen);

          var boardInfos = board.BoardInfos;
          var moves = board.GetMovesArray();
          var nextCrcs = new ulong[moves.Length];
          for (var moveIndex = 0; moveIndex < moves.Length; moveIndex++)
          {
            board.DoMoveFast(moves[moveIndex]);
            nextCrcs[moveIndex] = board.GetChecksum();
            Debug.Assert(hashTable.ContainsKey(nextCrcs[moveIndex])); // alle untergeordneten Züge sollten bereits in der Hashtable enthalten sein
            board.DoMoveBackward(moves[moveIndex], boardInfos);
          }

          UpdateParentStates(hashTable, parentCrc, nextCrcs, board);
        }

        // --- weitere Updates (bei Loops) ---
        hash = hashTable[currentCrc];
        bestState = BestState(hashTable, currentCrc, moveCrcs, whiteMoves);
        if (hash.state != bestState)
        {
          throw new NotImplementedException("check");
        }
      }
    }
  }
}
