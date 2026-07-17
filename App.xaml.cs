using System.IO;
using System.Windows;
using System.Windows.Threading;
using DSOCompanion.Services;

namespace DSOCompanion;

public partial class App : Application
{
    public static LicenseService LicenseService { get; } = new();

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!LicenseService.HasValidLicense())
        {
            LicenseWindow activationWindow =
                new(LicenseService);

            bool? activated = activationWindow.ShowDialog();

            if (activated != true)
            {
                Shutdown();
                return;
            }
        }

        MainWindow mainWindow = new();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "DSOCompanion");

            Directory.CreateDirectory(folder);
            string logPath = Path.Combine(folder, "crash.log");

            File.AppendAllText(
                logPath,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{e.Exception}\n\n");

            MessageBox.Show(
                "Es ist ein Fehler aufgetreten.\n\n" +
                "Fehlerprotokoll:\n" + logPath + "\n\n" +
                e.Exception.Message,
                "DSO Companion – Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }
        catch
        {
            e.Handled = true;
        }
    }
}
