using System.Windows;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BililiveDebugPlugin
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Window
    {
        public PluginDataContext context = new PluginDataContext();

        public MainPage()
        {
            InitializeComponent();
            DataContext = context;
        }

        private void ListView_OnSelected(object sender, RoutedEventArgs e)
        {
            context.Selected = (ListView.SelectedItem as DMItem)?.Model;
        }

        private void ClearClicked(object sender, RoutedEventArgs e)
        {
            context.DataList.Clear();
        }
    }
}