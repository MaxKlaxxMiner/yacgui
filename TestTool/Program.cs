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
  class Program: ConsoleExtras
  {
    static void Main(string[] args)
    {
      ConsoleHead("Test Tool: " + MainForm.FullName);

      BitmapTests.Run();
    }
  }
}
