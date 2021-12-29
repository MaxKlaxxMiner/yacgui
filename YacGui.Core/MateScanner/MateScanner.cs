using System;
using System.Collections.Generic;
using System.Linq;
using YacGui.Core.SimpleBoard;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace YacGui.Core
{
  /// <summary>
  /// Klasse zum Suchen von Matt-Varianten
  /// </summary>
  public static class MateScanner
  {
    [Flags]
    public enum Result : ushort
    {
      /// <summary>
      /// Ergebnis unbekannt
      /// </summary>
      Unknown = 0x0000,
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
    /// Positionsangaben des Schachbrettes sortiert um schneller (abwechselnd) weiße und schwarze Figuren schneller auf dem Brett zu finden
    /// </summary>
    static readonly int[] MaterialCheckPosOrder = Enumerable.Range(0, IBoard.Width * IBoard.Height).Select(x => x % 2 == 0 ? x / 2 : IBoard.Width * IBoard.Height - 1 - x / 2).ToArray();

    /// <summary>
    /// prüft den Status des Schachbrettes (ob z.B. bereits Matt gesetzt wurde)
    /// </summary>
    /// <param name="board">Spielbrett, welches geprüft werden soll</param>
    /// <param name="checkMaterial">optional: prüft zusätzlich, ob noch genug Material zum Mattsetzen vorhanden ist</param>
    /// <returns>einfaches Zwischenergebnis, ob ein Matt, Patt oder einfaches Remis erkannt wurde</returns>
    static Result CheckSimpleState(IBoard board, bool checkMaterial = false)
    {
      if (!board.HasMoves) // sind keine Züge mehr möglich? (Matt oder Patt gefunden)
      {
        if (board.IsMate()) // wurde Matt gesetzt?
        {
          return board.WhiteMove
            ? Result.BlackWins | Result.WhiteCannotWin
            : Result.WhiteWins | Result.BlackCannotWin;
        }

        // Patt = Remis
        return Result.WhiteCannotWin | Result.BlackCannotWin;
      }

      if (checkMaterial)
      {
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
        var state = Result.Unknown;
        if (materialWhite < 3) state |= Result.WhiteCannotWin;
        if (materialBlack < 3) state |= Result.BlackCannotWin;
        return state;
      }

      return Result.Unknown;
    }

    /// <summary>
    /// führt eine Mattsuche durch und gibt den bestmöglichen Zug bei perfektem Spiel zurück (sofern nicht abgebrochen, sonst: Unknown)
    /// </summary>
    /// <param name="board">Spielbrett mit der Stellung, welche durchsucht werden soll</param>
    /// <param name="maxDepth">maximale Suchtiefe in Halbzügen</param>
    /// <param name="cancel">optionale Abbruchbedinung</param>
    /// <returns>Bestmöglicher Zug und entsprechendes Ergebnis</returns>
    public static KeyValuePair<Result, Move> RunScan(IBoard board, int maxDepth = 250, Func<bool> cancel = null)
    {
      if (cancel == null) cancel = () => false;
      var b = new Board();
      b.SetFEN(board.GetFEN());

      var state = CheckSimpleState(board, true);

      if ((state & Result.MaskWins) != 0 || (state & Result.MaskCannotWin) == Result.MaskCannotWin) // Spielende wurde bereits erreicht?
      {
        return new KeyValuePair<Result, Move>(state, default(Move));
      }

      return new KeyValuePair<Result, Move>(state, default(Move));
    }
  }
}
