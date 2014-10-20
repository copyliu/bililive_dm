using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
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
    /// OptionDialog.xaml 的互動邏輯
    /// </summary>
    public partial class OptionDialog : Window
    {
        public OptionDialog()
        {
            this.InitializeComponent();
            this.Closed += OptionDialog_Closed;
            // 在此點下方插入建立物件所需的程式碼。
        }

        private void OptionDialog_Closed(object sender, EventArgs e)
        {
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                                            IsolatedStorageScope.Domain |
                                                                            IsolatedStorageScope.Assembly, null, null);
                System.Xml.Serialization.XmlSerializer settingsreader =
                    new System.Xml.Serialization.XmlSerializer(typeof (StoreModel));
                StreamWriter reader =
                    new StreamWriter(new IsolatedStorageFileStream("settings.xml", FileMode.Create, isoStore));
                settingsreader.Serialize(reader, (StoreModel) this.LayoutRoot.DataContext);
            }
            catch (Exception)
            {
            }
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow) (Application.Current.MainWindow)).Test_OnClick(null, null);
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            var a = (StoreModel) this.LayoutRoot.DataContext;
            a.FullOverlayEffect1 = DefaultStore.FullOverlayEffect1;
            a.FullOverlayFontsize = DefaultStore.FullOverlayFontsize;
            a.MainOverlayEffect1 = DefaultStore.MainOverlayEffect1;
            a.MainOverlayEffect2 = DefaultStore.MainOverlayEffect2;
            a.MainOverlayEffect3 = DefaultStore.MainOverlayEffect3;
            a.MainOverlayEffect4 = DefaultStore.MainOverlayEffect4;
            a.MainOverlayFontsize = DefaultStore.MainOverlayFontsize;
            a.MainOverlayWidth = DefaultStore.MainOverlayWidth;
            a.MainOverlayXoffset = DefaultStore.MainOverlayXoffset;
            a.MainOverlayYoffset = DefaultStore.MainOverlayYoffset;
        }
    }
}