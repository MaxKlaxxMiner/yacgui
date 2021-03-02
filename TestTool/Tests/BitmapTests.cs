
using YacGui;

namespace TestTool
{
  /// <summary>
  /// Class for testing bitmap functions
  /// </summary>
  public class BitmapTests : ConsoleExtras
  {
    /// <summary>
    /// Run Bitmap-Tests
    /// </summary>
    public static void Run()
    {
      ShowPicture(MainForm.DefaultChessPieces, mouseMove: (form, pos) => form.Text = "default chess pieces: " + pos);
    }
  }
}
