using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bililive_dm
{
    /// <summary>
    ///     Selector.xaml 的交互逻辑
    /// </summary>
    public partial class Selector : StyledWindow
    {
        private static readonly List<KeyValuePair<string, ResourceDictionary>> STATIC =
            new List<KeyValuePair<string, ResourceDictionary>>();

        static Selector()
        {
            addStaticTheme("Simple", null, (ResourceDictionary)Application.Current.Resources["Default"]);
            addStaticTheme("Aero2");
            addStaticTheme("Aero");
            addStaticTheme("Royale");
            addStaticTheme("Luna");
            addStaticTheme("Luna", "Homestead");
            addStaticTheme("Luna", "Metallic");
            addStaticTheme("Classic", null, (ResourceDictionary)Application.Current.Resources["Classic"]);
        }

        public Selector()
        {
            Themes = STATIC.ToList();
            InitializeComponent();
        }

        public List<KeyValuePair<string, ResourceDictionary>> Themes { get; }

        private ResourceDictionary selected => (ResourceDictionary)list.SelectedValue;

        private static void addStaticTheme(string theme, string variant = "NormalColor", ResourceDictionary dict = null)
        {
            variant = string.IsNullOrWhiteSpace(variant) ? theme : theme + "." + variant;

            STATIC.Add(new KeyValuePair<string, ResourceDictionary>(variant, dict ?? new ResourceDictionary
            {
                Source =
                    new Uri(
                        $"/PresentationFramework.{theme},Version=0.0.0.0,PublicKeyToken=31bf3856ad364e35;component/Themes/{variant}.xaml",
                        UriKind.Relative)
            }));
        }

        public event Action<ResourceDictionary> PreviewTheme;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public ResourceDictionary Select()
        {
            if (!ShowDialog().GetValueOrDefault()) return null;
            return selected ?? new ResourceDictionary();
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PreviewTheme?.Invoke(selected);
        }

        private void list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button_Click(sender, e);
        }

        private void list_Loaded(object sender, RoutedEventArgs e)
        {
            var li = (UIElement)list.ItemContainerGenerator.ContainerFromItem(list.SelectedItem);
            li?.Focus();
        }
    }
}