using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace LiveStreamerCmtHelper;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainLog.DataContext = new MainViewModel();
    }
}