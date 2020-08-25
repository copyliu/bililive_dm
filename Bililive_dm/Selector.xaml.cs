using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bililive_dm
{
    /// <summary>
    /// Selector.xaml 的交互逻辑
    /// </summary>
    public partial class Selector: Window
    {
        public List<KeyValuePair<string, ResourceDictionary>> Themes { get; } = new List<KeyValuePair<string, ResourceDictionary>>();

        void AddTheme(string theme, string variant = "NormalColor")
        {
            variant = theme + "." + variant;

            Themes.Add(new KeyValuePair<string, ResourceDictionary>(variant, new ResourceDictionary
            {
                Source =
                new Uri($"/PresentationFramework.{theme},Version=0.0.0.0,PublicKeyToken=31bf3856ad364e35;component/Themes/{variant}.xaml",
                UriKind.Relative)
            }));
        }

        public Selector()
        {
            AddTheme("Aero");
            AddTheme("Royale");
            AddTheme("Luna");
            AddTheme("Luna", "Homestead");
            AddTheme("Luna", "Metallic");

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public ResourceDictionary Select(Action<List<KeyValuePair<string, ResourceDictionary>>> init = null)
        {
            init?.Invoke(Themes);
            if (!ShowDialog().GetValueOrDefault()) return null;

            return (ResourceDictionary)list.SelectedValue ?? new ResourceDictionary();
        }

        private void list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button_Click(sender, e);
        }
    }
}
