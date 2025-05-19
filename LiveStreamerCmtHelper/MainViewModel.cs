using System;
using System.Collections.ObjectModel;

namespace LiveStreamerCmtHelper;

public class MainLogItem
{
    public string LogName { get; set; }
    public DateTime LogTime { get; set; }
    public string LogContent { get; set; }
}
public class MainViewModel
{
    public ObservableCollection<MainLogItem> LogItems { get; set; }=new ObservableCollection<MainLogItem>();


    public MainViewModel()
    {
        LogItems.Add(new MainLogItem()
        {
            LogName = "1",
            LogTime = DateTime.Now,
            LogContent = "1"
        });
        LogItems.Add(new MainLogItem()
        {
            LogName = "2",
            LogTime = DateTime.Now,
            LogContent = "2"
        });
        LogItems.Add(new MainLogItem()
        {
            LogName = "3",
            LogTime = DateTime.Now,
            LogContent = "3"
        });
        LogItems.Add(new MainLogItem()
        {
            LogName = "4",
            LogTime = DateTime.Now,
            LogContent = "4"
        });
        LogItems.Add(new MainLogItem()
        {
            LogName = "5",
            LogTime = DateTime.Now,
            LogContent = "5"
        });
        LogItems.Add(new MainLogItem()
        {
            LogName = "6",
            LogTime = DateTime.Now,
            LogContent = "6"
        });
        LogItems.Add(new MainLogItem()
        {
            LogName = "7",
            LogTime = DateTime.Now,
            LogContent = "7"
        });
        
    }
}