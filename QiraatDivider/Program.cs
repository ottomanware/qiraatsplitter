using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;

namespace QiraatDivider
{
    public class Item
    {
        public string Text = "";
        public object Tag = null;

        public override string ToString()
        {
            return Text;
        }
    }

    public class ComboBoxItem
    {
        public object Value;
        public String Label;

        public ComboBoxItem()
        { }
        public ComboBoxItem(string Label, object Value)
        {
            this.Label = Label;
            this.Value = Value;
        }

        public override string ToString()
        {
            return Label;
        }
    }


    public static class Helper
    {
        public static string CultureName;
        private static ResourceManager locRm;

        static Helper()
        {
            string Culture = Properties.Settings.Default.Language;

            ChangeCulture(Culture);
        }



        public static string GetString(string Key)
        {
            return locRm.GetString(Key);

        }

        public static void ChangeCulture(string CultureName)
        {
            locRm = new ResourceManager(Assembly.GetExecutingAssembly().GetName().Name + "." + CultureName + ".Resources",
                Assembly.GetExecutingAssembly());
        }


    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
