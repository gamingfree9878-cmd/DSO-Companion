using System.Windows;
using DSOCompanion.Services;

namespace DSOCompanion;

public partial class LicenseWindow : Window
{
    private readonly LicenseService _licenseService;

    public LicenseWindow(LicenseService licenseService)
    {
        InitializeComponent();
        _licenseService = licenseService;
        HardwareIdBox.Text = _licenseService.HardwareId;
    }

    private void CopyHardwareId_OnClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_licenseService.HardwareId);
        StatusText.Text = "✓ HWID wurde kopiert.";
        StatusText.Foreground =
            System.Windows.Media.Brushes.LightGreen;
    }

    private void Activate_OnClick(object sender, RoutedEventArgs e)
    {
        if (_licenseService.Activate(ActivationCodeBox.Text))
        {
            StatusText.Text =
                $"✓ Lizenz gültig – {_licenseService.ExpiryText}\n" +
                $"Besitzer: {_licenseService.CurrentLicense?.Owner}\n" +
                $"Lizenz-ID: {_licenseService.MaskedLicenseId}";

            StatusText.Foreground =
                System.Windows.Media.Brushes.LightGreen;

            MessageBox.Show(
                "Die Lizenz wurde erfolgreich aktiviert.",
                "DSO Companion",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
            return;
        }

        StatusText.Text = "✗ " + _licenseService.LastError;
        StatusText.Foreground =
            System.Windows.Media.Brushes.LightCoral;
    }
}
