using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bililive_dm
{
    public partial class WtfDanmakuWindow : Form, IDanmakuWindow
    {
        public WtfDanmakuWindow()
        {
            InitializeComponent();
        }

        private void WtfDanmakuWindow_Load(object sender, EventArgs e)
        {

        }

        void IDanmakuWindow.Initialize()
        {

        }

        void IDanmakuWindow.Terminate()
        {

        }

        void IDanmakuWindow.Show()
        {

        }

        void IDanmakuWindow.Close()
        {

        }

        void IDanmakuWindow.AddDanmaku(DanmakuType type, string comment, uint color)
        {

        }
    }
}
