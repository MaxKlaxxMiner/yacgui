using System.Drawing;
using FastBitmapLib.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastBitmapLib.Test.BitmapBasics
{
  [TestClass]
  public sealed class FastBitmap64_Test : FastBitmapTester
  {
    /// <summary>
    /// Create Testbitmap
    /// </summary>
    /// <param name="width">Width in Pixels</param>
    /// <param name="height">Height in Pixels</param>
    /// <param name="backgroundColor">Initial Background-Color</param>
    /// <returns>Created Bitmap</returns>
    public override IFastBitmap CreateBitmap(int width, int height, Color backgroundColor)
    {
      return new FastBitmap64(width, height, Color64.From(backgroundColor));
    }

    [TestMethod]
    public new void Test01_Constructor() { base.Test01_Constructor(); }

    [TestMethod]
    public new void Test02_Constructor_Background() { base.Test02_Constructor_Background(); }

    [TestMethod]
    public new void Test03_SetPixel32() { base.Test03_SetPixel32(); }

    [TestMethod]
    public new void Test04_GetPixel32() { base.Test04_GetPixel32(); }

    [TestMethod]
    public new void Test05_FillScanline32() { base.Test05_FillScanline32(); }
  }
}
