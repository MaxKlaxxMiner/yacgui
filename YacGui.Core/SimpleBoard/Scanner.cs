using System;
using System.Linq;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable RedundantIfElseBlock

namespace YacGui.Core.SimpleBoard
{
  public static class Scanner
  {
    #region # // --- Basis Funktionen ---
    static int PiecePoints(IBoard b)
    {
      int r = 0;
      for (var pos = 0; pos < IBoard.Width * IBoard.Height; pos++)
      {
        var p = b.GetField(pos);
        switch (p)
        {
          case Piece.BlackKing: if (pos == 2 || pos == 6) r -= 10; break;
          case Piece.WhiteKing: if (pos == 58 || pos == 62) r += 10; break;
          case Piece.WhiteQueen: r += 900; break;
          case Piece.BlackQueen: r -= 900; break;
          case Piece.WhiteRook: r += 500; break;
          case Piece.BlackRook: r -= 500; break;
          case Piece.WhiteBishop: r += 300; if (pos == 58 || pos == 61) r--; break;
          case Piece.BlackBishop: r -= 300; if (pos == 2 || pos == 5) r++; break;
          case Piece.WhiteKnight: r += 300; if (pos == 57 || pos == 62) r -= 2; break;
          case Piece.BlackKnight: r -= 300; if (pos == 1 || pos == 6) r += 2; break;
          case Piece.WhitePawn: r += 100; r += (6 - pos / IBoard.Width) * (6 - pos / IBoard.Width); break;
          case Piece.BlackPawn: r -= 100; r -= (pos / IBoard.Width - 1) * (pos / IBoard.Width - 1); break;
        }
      }
      return r;
    }

    static int EndCheck(IBoard b, int depth)
    {
      if (b.WhiteMove)
      {
        return b.IsChecked(b.GetKingPos(Piece.White), Piece.Black) ? -25000 - depth * 100 : 1000;
      }
      else
      {
        return b.IsChecked(b.GetKingPos(Piece.Black), Piece.White) ? 25000 + depth * 100 : -1000;
      }
    }
    #endregion

    #region # // --- ScanMovePointsAlphaBeta ---
    static int ScanMovePointsMaxInternal(IBoard b, int depth, int alpha, int beta)
    {
      var nextMoves = b.GetMovesArray();
      if (b.WhiteMove) Move.Sort(nextMoves, 0, nextMoves.Length); else Move.SortBackward(nextMoves, 0, nextMoves.Length);

      if (nextMoves.Length == 0) // keine weiteren Zugmöglichkeit?
      {
        return EndCheck(b, depth);
      }

      var lastBoardInfos = b.BoardInfos;
      int points = alpha;

      if (depth == 0)
      {
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          if (point > points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points >= beta) break;
        }
      }
      else
      {
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point = ScanMovePointsMinInternal(b, depth - 1, points, beta);
          if (point > points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points >= beta) break;
        }
      }
      return points;
    }

    static int ScanMovePointsMinInternal(IBoard b, int depth, int alpha, int beta)
    {
      var nextMoves = b.GetMovesArray();
      if (b.WhiteMove) Move.Sort(nextMoves, 0, nextMoves.Length); else Move.SortBackward(nextMoves, 0, nextMoves.Length);

      if (nextMoves.Length == 0) // keine weiteren Zugmöglichkeit?
      {
        return EndCheck(b, depth);
      }

      var lastBoardInfos = b.BoardInfos;
      int points = beta;

      if (depth == 0)
      {
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          if (point < points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points <= alpha) break;
        }
      }
      else
      {
        depth--;
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point = ScanMovePointsMaxInternal(b, depth, alpha, points);
          if (point < points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points <= alpha) break;
        }
      }
      return points;
    }

    public static int[] ScanMovePointsAlphaBeta(IBoard b, Move[] moves, int depth)
    {
      var result = new int[moves.Length];
      for (int i = 0; i < result.Length; i++) result[i] = b.WhiteMove ? int.MinValue : int.MaxValue;

      var lastBoardInfos = b.BoardInfos;
      var nextMoves = moves.ToArray();
      if (b.WhiteMove) Move.Sort(nextMoves, 0, nextMoves.Length); else Move.SortBackward(nextMoves, 0, nextMoves.Length);
      if (depth > 3)
      {
        var tmpPoints = ScanMovePointsAlphaBeta(b, moves, 3);
        nextMoves = Enumerable.Range(0, moves.Length).OrderBy(i => tmpPoints[i]).Select(i => moves[i]).ToArray();
        if (b.WhiteMove) nextMoves = nextMoves.Reverse().ToArray();
      }

      int points;
      if (b.WhiteMove)
      {
        points = int.MinValue;
        for (var i = 0; i < nextMoves.Length; i++)
        {
          var move = nextMoves[i];
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point;
          if (depth == 0)
          {
            point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          }
          else
          {
            point = ScanMovePointsMinInternal(b, depth - 1, points, int.MaxValue);
          }
          if (point > points)
          {
            if (i > 0) // Zug nach vorne sortieren?
            {
              for (int sort = i; sort > 0; sort--)
              {
                nextMoves[sort] = nextMoves[sort - 1];
              }
              nextMoves[0] = move;
            }
            points = point;
          }

          b.DoMoveBackward(move, lastBoardInfos);
        }
      }
      else
      {
        points = int.MaxValue;
        for (var i = 0; i < nextMoves.Length; i++)
        {
          var move = nextMoves[i];
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point;
          if (depth == 0)
          {
            point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          }
          else
          {
            point = ScanMovePointsMaxInternal(b, depth - 1, int.MinValue, points);
          }
          if (point < points)
          {
            if (i > 0) // Zug nach vorne sortieren?
            {
              for (int sort = i; sort > 0; sort--)
              {
                nextMoves[sort] = nextMoves[sort - 1];
              }
              nextMoves[0] = move;
            }
            points = point;
          }

          b.DoMoveBackward(move, lastBoardInfos);
        }
      }

      for (int i = 0; i < moves.Length; i++)
      {
        if (moves[i].ToString() == nextMoves[0].ToString())
        {
          result[i] = points;
        }
      }
      return result;
    }

    public static int[] ScanMovePointsAlphaBetaFull(IBoard b, Move[] moves, int depth)
    {
      var result = new int[moves.Length];

      var lastBoardInfos = b.BoardInfos;

      for (int i = 0; i < moves.Length; i++)
      {
        if (!b.DoMove(moves[i])) throw new Exception("invalid move?");

        if (b.WhiteMove)
        {
          result[i] = ScanMovePointsMaxInternal(b, depth - 1, int.MinValue, int.MaxValue);
        }
        else
        {
          result[i] = ScanMovePointsMinInternal(b, depth - 1, int.MinValue, int.MaxValue);
        }

        b.DoMoveBackward(moves[i], lastBoardInfos);
      }

      return result;
    }
    #endregion

    #region # // --- ScanMovePointsAlphaBetaDynamic ---
    static int ScanMovePointsMaxInternalDynamic(IBoard b, int depth, int alpha, int beta)
    {
      var nextMoves = b.GetMovesArray();
      if (b.WhiteMove) Move.Sort(nextMoves, 0, nextMoves.Length); else Move.SortBackward(nextMoves, 0, nextMoves.Length);

      if (nextMoves.Length == 0) // keine weiteren Zugmöglichkeit?
      {
        return EndCheck(b, depth);
      }

      var lastBoardInfos = b.BoardInfos;
      int points = alpha;

      if (depth == 0)
      {
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point;
          if (move.capturePiece != Piece.None)
          {
            point = ScanMovePointsMinInternalDynamic(b, 0, points, beta);
          }
          else
          {
            point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          }
          if (point > points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points >= beta) break;
        }
      }
      else
      {
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point = ScanMovePointsMinInternalDynamic(b, depth - 1, points, beta);
          if (point > points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points >= beta) break;
        }
      }
      return points;
    }

    static int ScanMovePointsMinInternalDynamic(IBoard b, int depth, int alpha, int beta)
    {
      var nextMoves = b.GetMovesArray();
      if (b.WhiteMove) Move.Sort(nextMoves, 0, nextMoves.Length); else Move.SortBackward(nextMoves, 0, nextMoves.Length);

      if (nextMoves.Length == 0) // keine weiteren Zugmöglichkeit?
      {
        return EndCheck(b, depth);
      }

      var lastBoardInfos = b.BoardInfos;
      int points = beta;

      if (depth == 0)
      {
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point;
          if (move.capturePiece != Piece.None)
          {
            point = ScanMovePointsMaxInternalDynamic(b, 0, alpha, points);
          }
          else
          {
            point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          }
          if (point < points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points <= alpha) break;
        }
      }
      else
      {
        depth--;
        foreach (var move in nextMoves)
        {
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point = ScanMovePointsMaxInternalDynamic(b, depth, alpha, points);
          if (point < points) points = point;

          b.DoMoveBackward(move, lastBoardInfos);

          if (points <= alpha) break;
        }
      }
      return points;
    }

    public static int[] ScanMovePointsAlphaBetaDynamic(IBoard b, Move[] moves, int depth)
    {
      var result = new int[moves.Length];
      for (int i = 0; i < result.Length; i++) result[i] = b.WhiteMove ? int.MinValue : int.MaxValue;

      var lastBoardInfos = b.BoardInfos;
      var nextMoves = moves.ToArray();
      if (b.WhiteMove) Move.Sort(nextMoves, 0, nextMoves.Length); else Move.SortBackward(nextMoves, 0, nextMoves.Length);
      if (depth > 3)
      {
        var tmpPoints = ScanMovePointsAlphaBetaDynamic(b, moves, 3);
        nextMoves = Enumerable.Range(0, moves.Length).OrderBy(i => tmpPoints[i]).Select(i => moves[i]).ToArray();
        if (b.WhiteMove) nextMoves = nextMoves.Reverse().ToArray();
      }

      int points;
      if (b.WhiteMove)
      {
        points = int.MinValue;
        for (var i = 0; i < nextMoves.Length; i++)
        {
          var move = nextMoves[i];
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point;
          if (depth == 0)
          {
            point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          }
          else
          {
            point = ScanMovePointsMinInternalDynamic(b, depth - 1, points, int.MaxValue);
          }
          if (point > points)
          {
            if (i > 0) // Zug nach vorne sortieren?
            {
              for (int sort = i; sort > 0; sort--)
              {
                nextMoves[sort] = nextMoves[sort - 1];
              }
              nextMoves[0] = move;
            }
            points = point;
          }

          b.DoMoveBackward(move, lastBoardInfos);
        }
      }
      else
      {
        points = int.MaxValue;
        for (var i = 0; i < nextMoves.Length; i++)
        {
          var move = nextMoves[i];
          if (!b.DoMove(move)) throw new Exception("invalid move?");

          int point;
          if (depth == 0)
          {
            point = b.HasMoves ? PiecePoints(b) : EndCheck(b, 0);
          }
          else
          {
            point = ScanMovePointsMaxInternalDynamic(b, depth - 1, int.MinValue, points);
          }
          if (point < points)
          {
            if (i > 0) // Zug nach vorne sortieren?
            {
              for (int sort = i; sort > 0; sort--)
              {
                nextMoves[sort] = nextMoves[sort - 1];
              }
              nextMoves[0] = move;
            }
            points = point;
          }

          b.DoMoveBackward(move, lastBoardInfos);
        }
      }

      for (int i = 0; i < moves.Length; i++)
      {
        if (moves[i].ToString() == nextMoves[0].ToString())
        {
          result[i] = points;
        }
      }
      return result;
    }

    public static int[] ScanMovePointsAlphaBetaDynamicFull(IBoard b, Move[] moves, int depth)
    {
      var result = new int[moves.Length];

      var lastBoardInfos = b.BoardInfos;

      for (int i = 0; i < moves.Length; i++)
      {
        if (!b.DoMove(moves[i])) throw new Exception("invalid move?");

        if (b.WhiteMove)
        {
          result[i] = ScanMovePointsMaxInternalDynamic(b, depth - 1, int.MinValue, int.MaxValue);
        }
        else
        {
          result[i] = ScanMovePointsMinInternalDynamic(b, depth - 1, int.MinValue, int.MaxValue);
        }

        b.DoMoveBackward(moves[i], lastBoardInfos);
      }

      return result;
    }
    #endregion
  }
}
