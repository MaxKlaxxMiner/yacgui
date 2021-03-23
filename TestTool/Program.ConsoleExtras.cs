#region # using *.*
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

// ReSharper disable UnusedMember.Local
#endregion

namespace TestTool
{
  /// <summary>
  /// Helper-Methods for the console
  /// </summary>
  public class ConsoleExtras
  {
    /// <summary>
    /// Display simple console header
    /// </summary>
    /// <param name="name">Name of the program</param>
    public static void ConsoleHead(string name)
    {
      string asterisks = string.Concat(Enumerable.Repeat(" *", name.Length / 2 + 2));

      Console.WriteLine();

      Console.WriteLine("  *" + asterisks + " *");
      Console.WriteLine("  *" + asterisks.Replace('*', ' ') + " *");

      Console.Write("  *  ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write(name);
      Console.ForegroundColor = ConsoleColor.Gray;
      if (name.Length % 2 == 0) Console.Write(' ');
      Console.WriteLine("  *");

      Console.WriteLine("  *" + asterisks.Replace('*', ' ') + " *");
      Console.WriteLine("  *" + asterisks + " *");
      Console.WriteLine();
    }

    /// <summary>
    /// Displays a picture
    /// </summary>
    /// <param name="image">Image to be drawn</param>
    /// <param name="title">Optional: Window-title</param>
    /// <param name="mouseMove">Optional: Callback mouse move event</param>
    /// <param name="backgroundColor">Optional: set the background-color (Only visible with transparent images)</param>
    /// <param name="runLoop">Optional: endless loop function</param>
    public static void ShowPicture(Image image, string title = "", Action<Form, Point> mouseMove = null, Color backgroundColor = default(Color), Action<Form, PictureBox> runLoop = null)
    {
      var pic = new PictureBox
      {
        Image = image,
        SizeMode = PictureBoxSizeMode.StretchImage,
        Dock = DockStyle.Fill
      };

      var form = new Form
      {
        ClientSize = image.Size,
        Text = title,
        BackColor = backgroundColor
      };

      form.KeyDown += (sender, e) =>
      {
        if (e.KeyCode == Keys.Escape) form.Close();
      };

      form.Controls.Add(pic);

      if (mouseMove != null)
      {
        pic.MouseMove += (sender, e) => mouseMove(form, new Point(e.X, e.Y));
      }

      if (runLoop != null)
      {
        bool formActive = true;
        form.FormClosed += (sender, e) => formActive = false;
        form.Show();

        while (formActive)
        {
          Thread.Sleep(0);
          runLoop(form, pic);
          Application.DoEvents();
        }
      }
      else
      {
        form.ShowDialog();
      }
    }
  }
}
