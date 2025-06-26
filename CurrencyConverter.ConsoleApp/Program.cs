// See https://aka.ms/new-console-template for more information
using CurrencyConverter.Infrastructure.Services;
using Spectre.Console;

var converter = CurrencyConverterWithModelService.Instance;

// نمایش عنوان
AnsiConsole.MarkupLine("[bold yellow]💱 Welcome to Currency Converter 💱[/]");

// کانفیگ اولیه نرخ‌ها
converter.UpdateConfiguration(new List<(string, string, double)>
{
    ("USD", "CAD", 1.34),
    ("CAD", "GBP", 0.58),
    ("USD", "EUR", 0.86),
    ("EUR", "JPY", 156.28),
    ("GBP", "INR", 105.64),
    ("USD", "CAD", 1.30),
});

// دریافت ورودی‌ها
var from = AnsiConsole.Ask<string>("Enter [green]source currency[/] (e.g. USD):").ToUpperInvariant();
var to = AnsiConsole.Ask<string>("Enter [green]target currency[/] (e.g. EUR):").ToUpperInvariant();
var amount = AnsiConsole.Ask<double>("Enter [green]amount[/]:");

// تبدیل و نمایش نتیجه
try
{
    //var result = converter.Convert(from, to, amount);

    //var table = new Table();
    //table.AddColumn("[blue]From[/]");
    //table.AddColumn("[green]To[/]");
    //table.AddColumn("[yellow]Amount[/]");
    //table.AddColumn("[bold cyan]Result[/]");

    //table.AddRow(from, to, amount.ToString("F2"), result.ToString("F2"));

    //AnsiConsole.Write(table);

    AnsiConsole.Ask<string>("");
}
catch (Exception ex)
{
    AnsiConsole.MarkupLineInterpolated($"[bold red]❌ Error:[/] {ex.Message}");
}