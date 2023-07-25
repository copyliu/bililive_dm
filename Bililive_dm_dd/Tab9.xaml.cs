using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bililive_dm_dd.Models;

namespace Bililive_dm_dd
{
    /// <summary>
    /// Tab9.xaml 的交互逻辑
    /// </summary>
    public partial class Tab9 : UserControl
    {
        public Tab9()
        {
            InitializeComponent();
            this.DataContext = this;
        }
              
        public RoomContext Context1 => Statics.Contexts.FirstOrDefault();
        public RoomContext Context2 => Statics.Contexts.Skip(1).FirstOrDefault();
        public RoomContext Context3 => Statics.Contexts.Skip(2).FirstOrDefault();
        public RoomContext Context4 => Statics.Contexts.Skip(3).FirstOrDefault();
        public RoomContext Context5 => Statics.Contexts.Skip(4).FirstOrDefault();
        public RoomContext Context6 =>Statics. Contexts.Skip(5).FirstOrDefault();
        public RoomContext Context7 => Statics.Contexts.Skip(6).FirstOrDefault();
        public RoomContext Context8 =>Statics. Contexts.Skip(7).FirstOrDefault();
        public RoomContext Context9 => Statics.Contexts.Skip(8).FirstOrDefault();
        
    }
}
