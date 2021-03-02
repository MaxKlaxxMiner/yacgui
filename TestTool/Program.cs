#region # using *.*
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YacGui;
#endregion

namespace TestTool
{
  static partial class Program
  {
    static void Main(string[] args)
    {
      ConsoleHead("Test Tool: " + MainForm.FullName);

      ShowPicture(MainForm.DefaultChessPieces, mouseMove: (form, pos) => form.Text = "default chess pieces: " + pos);
    }
  }
}
