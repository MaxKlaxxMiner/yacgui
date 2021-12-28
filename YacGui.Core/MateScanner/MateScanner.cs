using System;
using System.Collections.Generic;
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
            ? Result.WhiteWins | Result.BlackCannotWin
            : Result.BlackWins | Result.WhiteCannotWin;
        }

        // Patt = Remis
        return Result.WhiteCannotWin | Result.BlackCannotWin;
      }

      if (checkMaterial)
      {
        throw new NotImplementedException();
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

      var state = CheckSimpleState(board);

      return new KeyValuePair<Result, Move>(Result.Unknown, default(Move));
    }
  }
}
