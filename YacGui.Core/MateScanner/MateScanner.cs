using System;
using System.Collections.Generic;
using System.Linq;
using YacGui.Core.SimpleBoard;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantIfElseBlock
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable UnusedType.Local
// ReSharper disable UseObjectOrCollectionInitializer
#pragma warning disable 219

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
    public enum Result : ushort
    {
      /// <summary>
      /// Ergebnis unbekannt
      /// </summary>
      None = 0x0000,
      /// <summary>
      /// Bitmaske für das Ergebnis
      /// </summary>
      MaskResult = 0xf000,
      /// <summary>
      /// Bitmaske für die Marker "garantierte Gewinnmöglichkeit"
      /// </summary>
      MaskWins = 0xa000,
      /// <summary>
      /// Bitmaske für die Marker "keine Gewinnmöglichkeit"
      /// </summary>
      MaskCannotWin = 0x3000,
      /// <summary>
      /// Weiß gewinnt in x Halbzügen
      /// </summary>
      WhiteWins = 0x8000,
      /// <summary>
      /// Weiß kann nicht mehr gewinnen
      /// </summary>
      WhiteCannotWin = 0x4000,
      /// <summary>
      /// Schwarz gewinnt in x Halbzügen
      /// </summary>
      BlackWins = 0x2000,
      /// <summary>
      /// Schwarz kann nicht mehr gewinnen
      /// </summary>
      BlackCannotWin = 0x1000,
      /// <summary>
      /// Remis wird in x Halbzügen erreicht
      /// </summary>
      Remis = WhiteWins | BlackWins,

      /// <summary>
      /// Bitmaske für die Anzahl der Halbzüge
      /// </summary>
      MaskHalfmoves = 0x0fff
    }

    /// <summary>
    /// gibt das erreichbare Spielergebnis als lesbare Zeichenkette zurück
    /// </summary>
    /// <param name="result">Ergebnis, welches betroffen ist</param>
    /// <returns>lesbare Zeichenkette</returns>
    public static string TxtInfo(this Result result)
    {
      int halfmoves = (int)(result & Result.MaskHalfmoves);
      if ((result & Result.WhiteWins) == Result.WhiteWins)
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
      else if ((result & Result.BlackWins) == Result.BlackWins)
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
      return result.ToString();
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
    static Result CheckSimpleState(IBoard board)
    {
      if (!board.HasMoves) // sind keine Züge mehr möglich? = Matt oder Patt gefunden
      {
        if (board.IsMate()) // Matt wurde gesetzt
        {
          return board.WhiteMove
            ? Result.BlackWins | Result.WhiteCannotWin
            : Result.WhiteWins | Result.BlackCannotWin;
        }

        // Patt wurde erreicht = Remis
        return Result.WhiteCannotWin | Result.BlackCannotWin;
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
      var state = Result.None;
      if (materialWhite < 3) state |= Result.WhiteCannotWin;
      if (materialBlack < 3) state |= Result.BlackCannotWin;
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
      /// Prüfsumme der vorherigen Stellung
      /// </summary>
      public readonly ulong parentCrc;

      /// <summary>
      /// aktueller Ergebnis-Status der Stellung
      /// </summary>
      public Result state;

      /// <summary>
      /// merkt sich die aktuelle Stellung
      /// </summary>
      public readonly string fen;

      /// <summary>
      /// Konstruktor
      /// </summary>
      /// <param name="parentCrc">Prüfsumme der vorherigen Stellung</param>
      /// <param name="fen">FEN der aktuellen Stellung</param>
      /// <param name="state">aktueller Ergebnis-Status der Stellung</param>
      public HashElement(ulong parentCrc, Result state, string fen)
      {
        this.parentCrc = parentCrc;
        this.state = state;
        this.fen = fen;
      }

      /// <summary>
      /// gibt den Inhalt als lesbare Zeichenkette zurück
      /// </summary>
      /// <returns>lesbare Zeichenkette</returns>
      public override string ToString()
      {
        return new { parentCrc, state = state.TxtInfo(), fen }.ToString();
      }
    }
    #endregion

    static void ParentUpdate(ulong currentCrc, Dictionary<ulong, HashElement> hashTable, bool whiteMove)
    {
      var current = hashTable[currentCrc];

      var board = new Board();
      board.SetFEN(current.fen);

      var moves = board.GetMovesArray();
      var boardInfos = board.BoardInfos;

      var bestState = current.state;

      foreach (var move in moves)
      {
        board.DoMoveFast(move);

        ulong crc = board.GetChecksum();
        var next = hashTable[crc];

        if ((next.state & Result.MaskWins) != 0 || (next.state & Result.MaskCannotWin) == Result.MaskCannotWin)
        {
          throw new NotImplementedException();
        }
        else
        {
          return;
        }

        board.DoMoveBackward(move, boardInfos);
      }

      if (bestState != current.state)
      {
        throw new NotImplementedException();
      }
    }

    static void SubScan(IBoard board, Dictionary<ulong, HashElement> hashTable, List<string> nextFens)
    {
      var moves = board.GetMovesArray();
      var parentBoardInfos = board.BoardInfos;
      var parentCrc = board.GetChecksum();

      foreach (var move in moves)
      {
        board.DoMoveFast(move);

        ulong crc = board.GetChecksum();
        if (hashTable.ContainsKey(crc)) // Stellung schon bekannt? -> überspringen
        {
          board.DoMoveBackward(move, parentBoardInfos);
          continue;
        }

        var state = CheckSimpleState(board);

        hashTable.Add(crc, new HashElement(parentCrc, state, board.GetFEN()));

        if ((state & Result.MaskWins) != 0 || (state & Result.MaskCannotWin) == Result.MaskCannotWin) // Spielende wurde erreicht?
        {
          if ((state & Result.WhiteWins) == Result.WhiteWins) // Weiß hat Matt gesetzt
          {
            if (!board.WhiteMove)
            {
              var parent = hashTable[parentCrc];
              if (parent.state == Result.BlackCannotWin)
              {
                parent.state = Result.BlackCannotWin | Result.WhiteWins | (Result)1;
                hashTable[parentCrc] = parent;
              }
              else
              {
                throw new NotImplementedException();
              }
            }
            else
            {
              throw new NotImplementedException();
            }
          }
          else if ((state & Result.BlackWins) == Result.BlackWins) // Schwarz hat Matt gesetzt
          {
            throw new NotImplementedException();
          }
          else // Remis wurde erreicht
          {
            throw new NotImplementedException();
          }
        }

        nextFens.Add(board.GetFEN());

        board.DoMoveBackward(move, parentBoardInfos);
      }

      ParentUpdate(parentCrc, hashTable, board.WhiteMove);
    }

    /// <summary>
    /// führt eine Mattsuche durch und gibt den bestmöglichen Zug bei perfektem Spiel zurück (sofern nicht abgebrochen, sonst: Unknown)
    /// </summary>
    /// <param name="board">Spielbrett mit der Stellung, welche durchsucht werden soll</param>
    /// <param name="maxDepth">maximale Suchtiefe in Halbzügen</param>
    /// <param name="cancel">optionale Abbruchbedinung</param>
    /// <returns>Bestmöglicher Zug und entsprechendes Ergebnis</returns>
    public static Result RunScan(IBoard board, int maxDepth = 250, Func<bool> cancel = null)
    {
      if (cancel == null) cancel = () => false;
      var b = new Board();
      b.SetFEN(board.GetFEN());

      var state = CheckSimpleState(b);

      if ((state & Result.MaskWins) != 0 || (state & Result.MaskCannotWin) == Result.MaskCannotWin) // Spielende wurde jetzt schon erreicht?
      {
        return state;
      }

      var hashTable = new Dictionary<ulong, HashElement>();
      ulong baseCrc = b.GetChecksum();
      hashTable.Add(baseCrc, new HashElement(0, state, b.GetFEN()));
      var nextFens = new List<string> { b.GetFEN() };

      for (; maxDepth > 0; maxDepth--)
      {
        var scanFens = nextFens.ToArray();
        nextFens.Clear();
        foreach (var scanFen in scanFens)
        {
          b.SetFEN(scanFen);
          SubScan(b, hashTable, nextFens);
        }
        state = hashTable[baseCrc].state;

        if ((state & Result.MaskWins) != 0 || (state & Result.MaskCannotWin) == Result.MaskCannotWin) // Spielende wurde jetzt schon erreicht?
        {
          return state;
        }
      }

      return state;
    }
  }
}
