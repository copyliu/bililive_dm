using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BililiveDebugPlugin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Window
    {
        public PluginDataContext context=new PluginDataContext();
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = context;
        }

        private void ListView_OnSelected(object sender, RoutedEventArgs e)
        {
            this.context.Selected = (this.ListView.SelectedItem as DMItem)?.Model;

        }
    }
}
