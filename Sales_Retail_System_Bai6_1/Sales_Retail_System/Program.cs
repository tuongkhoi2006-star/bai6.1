using System;
using System.Windows.Forms;
using Sales_Retail_System.GUI;

namespace Sales_Retail_System
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
