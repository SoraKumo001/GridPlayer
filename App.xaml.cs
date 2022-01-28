using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GridPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }
    public class AppEventArgs
    {
        public string type = "";
        public double value = 0;
    }
    public delegate void AppEventHandler(object? sender, AppEventArgs e);
}
