using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BililiveAudioCmtPlayer
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Window
    {
        private readonly PluginDataContext _context;

     
        public MainPage(PluginDataContext context)
        {
            _context = context;
            InitializeComponent();
            this.DataContext = _context;
          
        }


        private void ClearClicked(object sender, RoutedEventArgs e)
        {
            this._context.DataList.Clear();
        }
       
    }
}
