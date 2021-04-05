using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib.Test
{
  public static unsafe class Ex
  {
    /// <summary>
    /// Compare two FastBitmaps
    /// </summary>
    /// <param name="expected">Expected Bitmap</param>
    /// <param name="actual">Actual Bitmap to compare</param>
    /// <param name="name">Optional: Name of the test</param>
    public static void AreEqual(IFastBitmap expected, IFastBitmap actual, string name = "")
    {
      if (expected == null) throw new ArgumentNullException("expected");
      if (actual == null) throw new ArgumentNullException("actual");

      if (name != "") name += ": ";

      Assert.AreEqual(expected.width, actual.width);
      Assert.AreEqual(expected.height, actual.height);

      if (actual is IFastBitmap32) // 32-Bit Compare?
      {
        var lineExpected = new uint[actual.width];
        var lineActual = new uint[actual.width];

        fixed (uint* ptrExpected = lineExpected, ptrActual = lineActual)
        {
          for (int line = 0; line < actual.height; line++)
          {
            expected.ReadScanLineUnsafe(0, line, lineExpected.Length, ptrExpected);
            actual.ReadScanLineUnsafe(0, line, lineActual.Length, ptrActual);
            for (int i = 0; i < lineActual.Length; i++)
            {
              if (ptrActual[i] != ptrExpected[i])
              {
                Assert.Fail(name + "different Pixel at [" + i + ", " + line + "] - expected: 0x" + ptrExpected[i].ToString("x8") + ", actual: 0x" + ptrActual[i].ToString("x8"));
              }
            }
          }
        }
      }
      else // 64-Bit Compare
      {
        var lineExpected = new ulong[actual.width];
        var lineActual = new ulong[actual.width];

        fixed (ulong* ptrExpected = lineExpected, ptrActual = lineActual)
        {
          for (int line = 0; line < actual.height; line++)
          {
            expected.ReadScanLineUnsafe(0, line, lineExpected.Length, ptrExpected);
            actual.ReadScanLineUnsafe(0, line, lineActual.Length, ptrActual);
            for (int i = 0; i < lineActual.Length; i++)
            {
              if (ptrActual[i] != ptrExpected[i])
              {
                Assert.Fail(name + "different Pixel at [" + i + ", " + line + "] - expected: 0x" + ptrExpected[i].ToString("x16") + ", actual: 0x" + ptrActual[i].ToString("x16"));
              }
            }
          }
        }
      }
    }
  }
}
