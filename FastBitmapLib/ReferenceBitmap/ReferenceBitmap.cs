using FastBitmapLib.Extras;
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Slow minimum Reference-Version of FastBitmap
  /// </summary>
  public class ReferenceBitmap : IFastBitmap32
  {
    /// <summary>
    /// pixel-data
    /// </summary>
    readonly uint[,] pixels;

    #region # // --- Constructor ---
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public ReferenceBitmap(int width, int height, uint backgroundColor = 0x00000000)
      : base(width, height, backgroundColor)
    {
      pixels = new uint[width, height];

      if (backgroundColor != 0x00000000) Clear();
    }
    #endregion

    #region # // --- SetPixel() / GetPixel() ---
    /// <summary>
    /// Set the pixel <see cref="Color32"/> at a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color32">Pixel <see cref="Color32"/></param>
    public override void SetPixel(int x, int y, uint color32)
    {
      if (x < 0 || x >= width || y < 0 || y >= height) return;

      pixels[x, y] = color32;
    }

    /// <summary>
    /// Get the pixel <see cref="Color32"/> from a specific position
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel <see cref="Color32"/></returns>
    public override uint GetPixel32(int x, int y)
    {
      if (x < 0 || x >= width || y < 0 || y >= height) return backgroundColor;

      return pixels[x, y];
    }
    #endregion

    #region # // --- IDisposable ---
    /// <summary>
    /// Release unmanaged ressources
    /// </summary>
    public override void Dispose()
    {
      // pixels = null; // not needed
    }
    #endregion
  }
}
