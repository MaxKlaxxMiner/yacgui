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
    const int SubVersion = 7;

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
      pictureBoxMain.Image = DefaultChessPieces;
    }

    /// <summary>
    /// "click"
    /// </summary>
    void pictureBoxMain_Click(object sender, EventArgs e)
    {
      if (pictureBoxMain.Image == DefaultChessPieces)
      {
        // draw alpha version
        var fastBitmap = new FastBitmap(DefaultChessPieces);
        fastBitmap.ConvertGreenPixelsToAlpha();
        pictureBoxMain.Image = fastBitmap.ToGDIBitmap();
        Text = FullName + ": alpha on";
      }
      else
      {
        // draw original image
        pictureBoxMain.Image = DefaultChessPieces;
        Text = FullName + ": alpha off";
      }
      pictureBoxMain.Refresh();
    }
  }
}
