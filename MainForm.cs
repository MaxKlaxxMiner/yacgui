#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#endregion

namespace YacGui
{
  public sealed partial class MainForm : Form
  {
    /// <summary>
    /// main-version: 0 == alpha, 1 >= release
    /// </summary>
    const int MainVersion = 0;
    /// <summary>
    /// sub-version
    /// </summary>
    const int SubVersion = 11;

    /// <summary>
    /// get title name
    /// </summary>
    public static string FullName
    {
      get
      {
        return "YacGui - v" + MainVersion + "." + SubVersion.ToString("D3");
      }
    }

    /// <summary>
    /// cache of the Bitmap
    /// </summary>
    static Bitmap defaultChessPiecesCache;

    /// <summary>
    /// returns the standard chess pieces as an image (source: https://commons.wikimedia.org/wiki/Template:SVG_chess_pieces)
    /// </summary>
    public static Bitmap DefaultChessPieces
    {
      get
      {
        return defaultChessPiecesCache ?? (defaultChessPiecesCache = Properties.Resources.ChessPieces);
      }
    }

    /// <summary>
    /// constructor
    /// </summary>
    public MainForm()
    {
      InitializeComponent();

      Text = FullName;
    }

    /// <summary>
    /// load form
    /// </summary>
    void MainForm_Load(object sender, EventArgs e)
    {
      var fastBitmap = new FastBitmap(DefaultChessPieces);
      fastBitmap.ConvertGreenPixelsToAlpha();

      var distMap = DistanceTransform.GenerateMap(fastBitmap.pixels.Select(x => (byte)(x >> 24)).ToArray(), fastBitmap.width, fastBitmap.height);
      for (int i = 0; i < distMap.Length; i++)
      {
        uint opacity = (uint)Math.Max(0, 255 - Math.Pow(distMap[i], 0.3) * 18);
        if (opacity == 0) continue; // too far
        fastBitmap.pixels[i] = FastBitmap.ColorBlend(0xffcc00, fastBitmap.pixels[i], fastBitmap.pixels[i] >> 24) & 0xffffff | opacity << 24;
      }

      pictureBoxMain.Image = fastBitmap.ToGDIBitmap();
    }
  }
}
