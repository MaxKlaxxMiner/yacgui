﻿using System.Drawing;
// ReSharper disable UnusedMember.Global
// ReSharper disable DoNotCallOverridableMethodsInConstructor
// ReSharper disable ClassCanBeSealed.Global

namespace FastBitmapLib
{
  /// <summary>
  /// Slow minimum Reference-Version of FastBitmap
  /// </summary>
  public class ReferenceBitmap64 : IFastBitmapSimple64
  {
    /// <summary>
    /// pixel-data
    /// </summary>
    readonly ulong[,] pixels;

    #region # // --- Constructors ---
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public ReferenceBitmap64(int width, int height, ulong backgroundColor = 0x0000000000000000)
      : base(width, height, backgroundColor)
    {
      pixels = new ulong[width, height];

      if (backgroundColor != 0x0000000000000000) Clear();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public ReferenceBitmap64(IFastBitmap bitmap, uint backgroundColor = 0x0000000000000000)
      : this(bitmap.width, bitmap.height, backgroundColor)
    {
      CopyFromBitmap(bitmap);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap to be used</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public ReferenceBitmap64(Bitmap bitmap, uint backgroundColor = 0x0000000000000000)
      : this(bitmap.Width, bitmap.Height, backgroundColor)
    {
      CopyFromGDIBitmap(bitmap);
    }
    #endregion

    #region # // --- SetPixel() / GetPixel() ---
    /// <summary>
    /// Set the pixel <see cref="Color64"/> at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color64">Pixel <see cref="Color64"/></param>
    public override void SetPixelUnsafe(int x, int y, ulong color64)
    {
      pixels[x, y] = color64;
    }

    /// <summary>
    /// Get the pixel <see cref="Color64"/> from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel <see cref="Color64"/></returns>
    public override ulong GetPixelUnsafe64(int x, int y)
    {
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
