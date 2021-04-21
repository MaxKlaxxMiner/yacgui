// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Local

namespace FastBitmapLib
{
  /// <summary>
  /// Compressed-Version of FastBitmap
  /// </summary>
  public class CompressedBitmap : IFastBitmapSimple32
  {
    readonly MiniMemoryManager data;
    readonly MiniMemoryManager.Entry[] dataIndex;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="backgroundColor">Optional: Background-Color, default: 100% transparency</param>
    public CompressedBitmap(int width, int height, uint backgroundColor = 0x00000000)
      : base(width, height, backgroundColor)
    {
      data = new MiniMemoryManager();
      dataIndex = new MiniMemoryManager.Entry[height + 1];
      dataIndex[height] = data.Alloc((uint)width * sizeof(uint) * 2 + 8); // last line = cache
    }

    /// <summary>
    /// Set the pixel color at a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <param name="color">Pixel color</param>
    public override void SetPixelUnsafe(int x, int y, uint color)
    {
    }

    /// <summary>
    /// Get the pixel color from a specific position (without boundary check)
    /// </summary>
    /// <param name="x">X-Pos (column)</param>
    /// <param name="y">Y-Pos (line)</param>
    /// <returns>Pixel color</returns>
    public override uint GetPixelUnsafe32(int x, int y)
    {
      return 0;
    }

    /// <summary>
    /// Release unmanaged ressources
    /// </summary>
    public override void Dispose()
    {
    }
  }
}
