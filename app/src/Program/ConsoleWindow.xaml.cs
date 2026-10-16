using System.Windows;
using NativeDnsOptimizer.Models;

namespace NativeDnsOptimizer;

public partial class ConsoleWindow : Window
{
    private readonly DnsServer? _winner;

    public ConsoleWindow(DnsServer? winner = null)
    {
        InitializeComponent();
        _winner = winner;

        if (_winner is not null)
        {
            ConsoleWinnerName.Text = $"{_winner.Name} ({_winner.PrimaryIp})";
            ConsolePrimaryText.Text = _winner.PrimaryIp;
            ConsoleSecondaryText.Text = string.IsNullOrWhiteSpace(_winner.SecondaryIp) ? _winner.PrimaryIp : _winner.SecondaryIp;
        }
    }

    private void CopyDnsBtn_Click(object sender, RoutedEventArgs e)
    {
        string primary = ConsolePrimaryText.Text;
        string secondary = ConsoleSecondaryText.Text;
        string clipboardText = $"Primary DNS: {primary}\nSecondary DNS: {secondary}";

        Clipboard.SetText(clipboardText);
        MessageBox.Show($"Copied to clipboard:\n\nPrimary: {primary}\nSecondary: {secondary}\n\nEnter these into your PlayStation or Xbox network settings.",
            "DNS IPs Copied", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
