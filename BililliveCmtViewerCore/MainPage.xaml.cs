namespace BililliveCmtViewerCore;

public partial class MainPage : ContentPage
{
    private int count = 0;

    public MainPage()
    {
        InitializeComponent();
        var logs = new List<string> { DateTime.Now + " : 他啟動了" };
        for (var i = 0; i < 100; i++) logs.Add(DateTime.Now + " : 他啟動了" + i);
        mainLog.ItemsSource = logs;
    }
    /*
    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
    */
}