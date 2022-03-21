#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastBitmapLib;
using FastBitmapLib.Extras;
// ReSharper disable NotAccessedField.Local

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
    const int SubVersion = 93;

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
      mouseY = 0;
    }

    Bitmap background;

    /// <summary>
    /// load form
    /// </summary>
    void MainForm_Load(object sender, EventArgs e)
    {
      var chessPieces = new FastBitmap(DefaultChessPieces);
      chessPieces.ConvertGreenPixelsToAlpha();

      int w = chessPieces.width;
      int h = chessPieces.height;
      var distMap = DistanceTransform.GenerateMap(Enumerable.Range(0, w * h).Select(pos => (byte)(chessPieces.GetPixel32(pos % w, pos / w) >> 24)).ToArray(), w, h);
      for (int y = 0; y < h; y++)
      {
        for (int x = 0; x < w; x++)
        {
          uint opacity = (uint)Math.Max(0, 255 - Math.Pow(distMap[x + y * w], 0.3) * 18);
          if (opacity == 0) continue; // too far
          chessPieces.SetPixel(x, y, Color32.BlendFast(0xffcc00, chessPieces.GetPixel32(x, y), chessPieces.GetPixel32(x, y) >> 24) & 0xffffff | opacity << 24);
        }
      }

      background = chessPieces.ToGDIBitmap();
      DoubleBuffered = true;
      SetStyle(ControlStyles.ResizeRedraw, true);

      ReadConfig();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
      timer1_Tick(null, null);

      base.OnPaintBackground(e);
      var g = e.Graphics;
      var state = g.Save();

      g.TranslateTransform(mouseX, mouseY);
      if (mouseLeft) g.RotateTransform((float)(-360.0 / Width * (mouseX - Width / 2)));
      if (mouseRight) g.ScaleTransform((float)((Height - mouseY - 20) / 200.0), (float)((Height - mouseY - 20) / 200.0));
      g.DrawImage(background, -background.Width / 2, -background.Height / 2);

      g.Restore(state);
    }

    /// <summary>
    /// timer for background updates
    /// </summary>
    void timer1_Tick(object sender, EventArgs e)
    {
      uint l = (uint)Color.Black.ToArgb();
      uint r = (uint)Color.DarkRed.ToArgb();
      uint g = (uint)Color.Green.ToArgb();
      uint b = (uint)Color.DarkBlue.ToArgb();
      uint c;

      const int Slow = 100;

      int time = Environment.TickCount % (100 * Slow) * 256 / (25 * Slow);
      if (time < 256) c = FastBitmapOld.ColorBlend(l, r, time);
      else if (time < 512) c = FastBitmapOld.ColorBlend(r, g, time - 256);
      else if (time < 768) c = FastBitmapOld.ColorBlend(g, b, time - 512);
      else c = FastBitmapOld.ColorBlend(b, l, time - 768);

      BackColor = Color.FromArgb((int)c);

      Text = mouseX + ", " + mouseY + " - " + mouseLeft + ", " + mouseRight + " (" + mousePressedX + ", " + mousePressedY + ")" + (mouseLeft || mouseRight ? " dragging: " + (mouseX - mousePressedX) + ", " + (mouseY - mousePressedY) : "");
    }

    /// <summary>
    /// read the config-file
    /// </summary>
    void ReadConfig()
    {
      try
      {
        var cfg = File.ReadAllText("microconfig.cfg").Split('\t');
        int v = int.Parse(cfg[0]);
        if (v < 91) return;

        Left = int.Parse(cfg[1]);
        Top = int.Parse(cfg[2]);
        Width = int.Parse(cfg[3]);
        Height = int.Parse(cfg[4]);
      }
      catch
      {
      }
    }

    /// <summary>
    /// write the current config to config-file
    /// </summary>
    void WriteConfig()
    {
      try
      {
        File.WriteAllText("microconfig.cfg", string.Join("\t", SubVersion, Left, Top, Width, Height));
      }
      catch { }
    }

    /// <summary>
    /// closing-event
    /// </summary>
    void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      WriteConfig();
    }

    int mouseX, mouseY;
    bool mouseLeft, mouseRight;
    int mousePressedX, mousePressedY;

    /// <summary>
    /// press mouse-button
    /// </summary>
    void MainForm_MouseDown(object sender, MouseEventArgs e)
    {
      mouseX = e.X;
      mouseY = e.Y;
      if ((e.Button & MouseButtons.Left) == MouseButtons.Left && !mouseLeft)
      {
        mouseLeft = true;
        mousePressedX = e.X;
        mousePressedY = e.Y;
      }
      if ((e.Button & MouseButtons.Right) == MouseButtons.Right && !mouseRight)
      {
        mouseRight = true;
        mousePressedX = e.X;
        mousePressedY = e.Y;
      }
    }

    /// <summary>
    /// move mouse
    /// </summary>
    void MainForm_MouseMove(object sender, MouseEventArgs e)
    {
      mouseX = e.X;
      mouseY = e.Y;
      Invalidate();
    }

    /// <summary>
    /// release mouse-button
    /// </summary>
    void MainForm_MouseUp(object sender, MouseEventArgs e)
    {
      mouseX = e.X;
      mouseY = e.Y;

      if ((e.Button & MouseButtons.Left) == MouseButtons.Left) mouseLeft = false;
      if ((e.Button & MouseButtons.Right) == MouseButtons.Right) mouseRight = false;
    }
  }
}
