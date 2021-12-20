﻿using System.Diagnostics;
using YacGui.Core.SimpleBoard;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantIfElseBlock

namespace YacGui.Core
{
  /// <summary>
  /// Klasse zum Berechnen von 64-Bit Prüfsummen
  /// </summary>
  public static class Crc64
  {
    /// <summary>
    /// Crc64 Startwert
    /// </summary>
    public const ulong Start = 0xcbf29ce484222325u;
    /// <summary>
    /// Crc64 Multiplikator
    /// </summary>
    public const ulong Mul = 0x100000001b3;

    /// <summary>
    /// aktualisiert die Prüfsumme
    /// </summary>
    /// <param name="crc64">ursprünglicher Crc64-Wert</param>
    /// <param name="value">Datenwert, welcher einberechnet werden soll</param>
    /// <returns>neuer Crc64-Wert</returns>
    public static ulong Crc64Update(this ulong crc64, int value)
    {
      return (crc64 ^ (uint)value) * Mul;
    }

    /// <summary>
    /// aktualisiert die Prüfsumme
    /// </summary>
    /// <param name="crc64">ursprünglicher Crc64-Wert</param>
    /// <param name="value">Datenwert, welcher einberechnet werden soll</param>
    /// <returns>neuer Crc64-Wert</returns>
    public static ulong Crc64Update(this ulong crc64, bool value)
    {
      if (value) return (crc64 ^ 1) * Mul; else return crc64 * Mul;
    }

    /// <summary>
    /// aktualisiert die Prüfsumme
    /// </summary>
    /// <param name="crc64">ursprünglicher Crc64-Wert</param>
    /// <param name="values">Datenwert, welcher einberechnet werden soll</param>
    /// <returns>neuer Crc64-Wert</returns>
    public static unsafe ulong Crc64Update(this ulong crc64, Piece[] values)
    {
      //for (int i = 0; i < values.Length; i++)
      //{
      //  crc64 = (crc64 ^ (byte)values[i]) * Mul;
      //}
      //return crc64;

      Debug.Assert(values.Length == 16 * sizeof(uint));
      fixed (Piece* ptr = values)
      {
        for (int i = 0; i < 16; i++)
        {
          crc64 = (crc64 ^ ((uint*)ptr)[i]) * Mul;
        }
        return crc64;
      }
    }
  }
}
