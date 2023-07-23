using System.Windows;

namespace BililiveAudioCmtPlayer
{
    /// <summary>
    ///     MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Window
    {
        private readonly PluginDataContext _context;


        public MainPage(PluginDataContext context)
        {
            _context = context;
            InitializeComponent();
            DataContext = _context;
        }


        private void ClearClicked(object sender, RoutedEventArgs e)
        {
            _context.DataList.Clear();
        }
    }
}