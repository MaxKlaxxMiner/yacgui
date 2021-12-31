using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using YacGui.Core.SimpleBoard;
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantIfElseBlock
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace YacGui.Core
{
  /// <summary>
  /// Klasse zum Suchen von Matt-Varianten
  /// </summary>
  public static class MateScanner
  {
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

      for (int depth = 0; depth < maxDepth; depth++)
      {
        if (cancel()) break;

        var scanFens = nextFens.ToArray();
        nextFens.Clear();

        foreach (var scanFen in scanFens)
        {
          b.SetFEN(scanFen);
          ulong parentCrc = b.GetChecksum();
          var boardInfos = b.BoardInfos;
          var moves = b.GetMovesArray();
          ulong[] moveCrcs = new ulong[moves.Length];
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
              Debug.Assert(!hash.parentCrcs.Contains(parentCrc)); // übergeordnete Positionen sollten nicht doppelt vorhanden sein

              // --- neuer übergeordnete Position verknüpfen ---
              Array.Resize(ref hash.parentCrcs, hash.parentCrcs.Length + 1);
              hash.parentCrcs[hash.parentCrcs.Length - 1] = parentCrc;
            }
            else
            {
              state = CheckSimpleState(b);
              Debug.Assert(AllowedResults.Contains(state & ResultState.ResultMask));

              // --- ersten Hashtable-Eintrag Erstellen und Position für weitere Untersuchungen vormerken ---
              hash = new HashElement(parentCrc, state, b.GetFEN());
              hashTable.Add(crc, hash);
              nextFens.Add(hash.fen);
            }
            b.DoMoveBackward(moves[moveIndex], boardInfos);
          }

          UpdateParentStates(hashTable, moveCrcs, b.WhiteMove);
        }

        // --- Ende schon erreicht? ---
        state = hashTable[baseCrc].state;
        if ((state & ResultState.WinMask) != 0 || (state & ResultState.CannotWinMask) == ResultState.CannotWinMask) // Spielende wurde jetzt schon erreicht?
        {
          // todo: eventuell wird nicht immer das schnellste Matt gefunden
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
    /// berechnet das beste Ergebnis aus mehreren Zügmöglichkeiten (sofern möglich)
    /// </summary>
    /// <param name="hashTable">Hashtable, welche die Zwischenergebnisse enthält</param>
    /// <param name="moveCrcs">Prüfsummen der Positionen, welche durch die Züge erreichbar sind (müssen alle bereits in der Hashtable enthalten sein)</param>
    /// <param name="whiteMoves">gibt an, ob aus den Zügen das optimum für Weiß gesucht werden soll (sonst: Schwarz)</param>
    /// <returns>beste Ergebnis</returns>
    static ResultState BestState(Dictionary<ulong, HashElement> hashTable, ulong[] moveCrcs, bool whiteMoves)
    {
      var bestState = ResultState.None;
      for (int i = 0; i < moveCrcs.Length; i++)
      {
        var state = hashTable[moveCrcs[i]].state;
        if ((state & ResultState.WinMask) != 0 || (state & ResultState.CannotWinMask) == ResultState.CannotWinMask) state++; // wenn ein Spielende in Sicht ist, den Counter hoch zählen

        if (i == 0) // Ergebnis direkt nehmen, wenn es das Erste ist
        {
          bestState = state;
          continue;
        }
        if ((state & ResultState.ResultMask) == (bestState & ResultState.ResultMask)) // gleiches Ergebnis?
        {
          if (state < bestState) bestState = state; // kürzere Variante bevorzugen
          continue;
        }

        if (whiteMoves) // --- nach besten Weißen Zügen optimieren ---
        {
          switch (bestState & ResultState.ResultMask)
          {
            case ResultState.BlackCannotWin:                                          // bisher: Weiß [offen], Schwarz [kann nicht gewinnen]
            {
              if ((state & ResultState.WhiteCannotWin) == ResultState.WhiteCannotWin) //    neu: Weiß [kann nicht gewinnen] -> keine Verbesserung, da Weiß mit einem anderen Zug noch gewinnen könnte
              {
                continue;
              }
              if ((state & ResultState.WhiteWins) == ResultState.WhiteWins)           //    neu: Weiß [gewinnt] -> besser, da nun ein garantierter Gewinnweg bekannt ist
              {
                bestState = state;
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.WhiteWins | ResultState.BlackCannotWin:                  // bisher: Weiß [gewinnt], Schwarz [kann nicht gewinnen]
            {
              if ((state & ResultState.WhiteWins) == ResultState.None)                //    neu: Weiß [unbekannt] -> keine Verbesserung, da Weiß mit einem anderen Zug garantiert gewinnt
              {
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.CannotWinMask:                                           // bisher: Weiß [kann nicht gewinnen], Schwarz [kann nicht gewinnen]
            {
              if ((state & ResultState.WhiteCannotWin) == ResultState.None)           //    neu: Schwarz [unbekannt] -> keine Verbesserung zum Remis, da Schwarz noch gewinnen könnte
              {
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.WhiteCannotWin:                                          // bisher: Weiß [kann nicht gewinnen], Schwarz [offen]
            {
              if ((state & ResultState.WhiteCannotWin) == ResultState.WhiteCannotWin) //    neu: Schwarz [kann nicht gewinnen] -> besser, da Schwarz nicht mehr gewinnen kann
              {
                bestState = state;
                continue;
              }
              if ((state & ResultState.BlackWins) == ResultState.BlackWins)           //    neu: Schwarz [gewinnt] -> keine Verbesserung, da es noch Varianten ohne garantiertem Gewinn für Schwarz gibt
              {
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.WhiteCannotWin | ResultState.BlackWins:                  // bisher: Weiß [kann nicht gewinnen], Schwarz [gewinnt]
            {
              if ((state & ResultState.BlackWins) == ResultState.None)                //    neu: Schwarz [unbekannt] -> besser, da Schwarz zumindest nicht mehr garantiert gewinnt
              {
                bestState = state;
                continue;
              }
              throw new NotImplementedException();
            }
            default: throw new NotImplementedException();
          }
        }
        else // --- nach besten Schwarzen Zügen optimieren ---
        {
          switch (bestState & ResultState.ResultMask)
          {
            case ResultState.WhiteCannotWin:                                          // bisher: Weiß [kann nicht gewinnen], Schwarz [offen]
            {
              if ((state & ResultState.BlackCannotWin) == ResultState.BlackCannotWin) //    neu: Schwarz [kann nicht gewinnen] -> keine Verbesserung, da Schwarz mit einem anderen Zug noch gewinnen könnte
              {
                continue;
              }
              if ((state & ResultState.BlackWins) == ResultState.BlackWins)           //    neu: Schwarz [gewinnt] -> besser, da nun ein garantierter Gewinnweg bekannt ist
              {
                bestState = state;
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.BlackWins | ResultState.WhiteCannotWin:                  // bisher: Weiß [kann nicht gewinnen], Schwarz [gewinnt]
            {
              if ((state & ResultState.BlackWins) == ResultState.None)                //    neu: Schwarz [unbekannt] -> keine Verbesserung, da Schwarz mit einem anderen Zug garantiert gewinnt
              {
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.CannotWinMask:                                           // bisher: Weiß [kann nicht gewinnen], Schwarz [kann nicht gewinnen]
            {
              if ((state & ResultState.WhiteCannotWin) == ResultState.None)           //    neu: Weiß [unbekannt] -> keine Verbesserung zum Remis, da Weiß noch gewinnen könnte
              {
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.BlackCannotWin:                                          // bisher: Weiß [offen], Schwarz [kann nicht gewinnen]
            {
              if ((state & ResultState.WhiteCannotWin) == ResultState.WhiteCannotWin) //    neu: Weiß [kann nicht gewinnen] -> besser, da Weiß nicht mehr gewinnen kann
              {
                bestState = state;
                continue;
              }
              if ((state & ResultState.WhiteWins) == ResultState.WhiteWins)           //    neu: Weiß [gewinnt] -> keine Verbesserung, da es noch Varianten ohne garantiertem Gewinn für Weiß gibt
              {
                continue;
              }
              throw new NotImplementedException();
            }
            case ResultState.WhiteWins | ResultState.BlackCannotWin:                  // bisher: Weiß [gewinnt], Schwarz [kann nicht gewinnen]
            {
              if ((state & ResultState.WhiteWins) == ResultState.None)                //    neu: Weiß [unbekannt] -> besser, da Weiß zumindest nicht mehr garantiert gewinnt
              {
                bestState = state;
                continue;
              }
              throw new NotImplementedException();
            }
            default: throw new NotImplementedException();
          }
        }
      }

      Debug.Assert(AllowedResults.Contains(bestState & ResultState.ResultMask));

      return bestState;
    }

    /// <summary>
    /// übergeordnete Positionen in der Hashtable aktualisieren (sofern notwendig)
    /// </summary>
    /// <param name="hashTable">Hashtable, welche aktualisiert werden soll</param>
    /// <param name="moveCrcs">Prüfsummen der Positionen, welche erreichbar sind</param>
    /// <param name="whiteMoves">gibt an, ob aus den Zügen das optimum für Weiß gesucht werden soll (sonst: Schwarz)</param>
    static void UpdateParentStates(Dictionary<ulong, HashElement> hashTable, ulong[] moveCrcs, bool whiteMoves)
    {
      var bestState = BestState(hashTable, moveCrcs, whiteMoves);

      foreach (ulong moveCrc in moveCrcs)
      {
        var parentCrcs = hashTable[moveCrc].parentCrcs;
        foreach (ulong parentCrc in parentCrcs)
        {
          var hash = hashTable[parentCrc];
          while (hash.state != bestState) // sinnvolle Änderung erkannt?
          {
            hash.state = bestState;
            hashTable[parentCrc] = hash;
            UpdateParentStates(hashTable, hash.parentCrcs, !whiteMoves); // übergeordnete Positionen rekursiv ebenfalls aktualisieren
            bestState = BestState(hashTable, moveCrcs, whiteMoves);
          }
        }
      }
    }
  }
}
