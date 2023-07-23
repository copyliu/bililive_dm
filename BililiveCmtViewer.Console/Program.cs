// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using System.Threading.Tasks.Dataflow;
using BiliDMLibCore;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<AppCmd>();
return await app.RunAsync(args);


internal sealed class AppCmd : AsyncCommand<AppCmd.Settings>
{
    private const int NumberOfRows = 10;
    private static readonly Random _random = new();

    private static readonly string[] _exchanges =
    {
        "SGD", "SEK", "PLN",
        "MYR", "EUR", "USD",
        "AUD", "JPY", "CNH",
        "HKD", "CAD", "INR",
        "DKK", "GBP", "RUB",
        "NZD", "MXN", "IDR",
        "TWD", "THB", "VND"
    };

    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        // AnsiConsole.MarkupLine($"Total file size for [green]{searchPattern}[/] files in [green]{searchPath}[/]: [blue]{totalFileSize:N0}[/] bytes");
        // AnsiConsole.MarkupLine($"{settings.RoomId}");
        var c = await RoomConnecter.ConnectAsync(settings.RoomId);
        var table = new Table().Expand().BorderColor(Color.Grey);
        table.AddColumn("[yellow]活[/]");
        // table.AddColumn("[yellow]Source currency[/]");
        // table.AddColumn("[yellow]Destination currency[/]");
        // table.AddColumn("[yellow]Exchange rate[/]");

        await AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                // Continously update the table
                while (true)

                {
                    var item = await c.DanmakuSource.ReceiveAsync();
                    // More rows than we want?
                    if (table.Rows.Count > NumberOfRows)
                        // Remove the first one
                        table.Rows.RemoveAt(0);
                    // AnsiConsole.MarkupLine($"{item.UserName}:{item.CommentText}");
                    // Add a new row
                    table.AddRow($"{item.UserName}:{item.CommentText}");
                    // Refresh and wait for a while
                    ctx.Refresh();
                    await Task.Delay(100);
                }
            });
        return 0;
    }

    private static void AddExchangeRateRow(Table table)
    {
        var (source, destination, rate) = GetExchangeRate();
        table.AddRow(
            source, destination,
            _random.NextDouble() > 0.35D ? $"[green]{rate}[/]" : $"[red]{rate}[/]");
    }

    private static (string Source, string Destination, double Rate) GetExchangeRate()
    {
        var source = _exchanges[_random.Next(0, _exchanges.Length)];
        var dest = _exchanges[_random.Next(0, _exchanges.Length)];
        var rate = 200 / (_random.NextDouble() * 320 + 1);

        while (source == dest) dest = _exchanges[_random.Next(0, _exchanges.Length)];

        return (source, dest, rate);
    }

    public sealed class Settings : CommandSettings
    {
        [Description("房間號 長號")]
        [CommandArgument(0, "<RoomId>")]
        public int RoomId { get; init; }
    }
}