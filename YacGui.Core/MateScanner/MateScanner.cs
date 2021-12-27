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
      /// Bitmaske für das Ergebnis
      /// </summary>
      MaskResult = 0xc000,
      /// <summary>
      /// Ergebnis noch unbekannt
      /// </summary>
      Unknown = 0x0000,
      /// <summary>
      /// Weiß gewinnt in x Halbzügen
      /// </summary>
      WhiteWins = 0x8000,
      /// <summary>
      /// Schwarz gewinnt in x Halbzügen
      /// </summary>
      BlackWins = 0x4000,
      /// <summary>
      /// Remis wird in x Halbzügen erreicht
      /// </summary>
      Remis = WhiteWins | BlackWins,

      /// <summary>
      /// Bitmaske für die Anzahl der Halbzüge
      /// </summary>
      MaskHalfmoves = 0x3fff
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

      return new KeyValuePair<Result, Move>(Result.Unknown, default(Move));
    }
  }
}
