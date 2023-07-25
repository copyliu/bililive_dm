using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Bililive_dm_dd.Models;

namespace Bililive_dm_dd
{
    public partial class Tab4 : UserControl
    {
        public Tab4()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        
        public RoomContext Context1 => Statics.Contexts.FirstOrDefault();
        public RoomContext Context2 => Statics.Contexts.Skip(1).FirstOrDefault();
        public RoomContext Context3 => Statics.Contexts.Skip(2).FirstOrDefault();
        public RoomContext Context4 =>Statics. Contexts.Skip(3).FirstOrDefault();


    }
}