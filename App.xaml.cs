using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace DSOCompanion;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DSOCompanion");

            Directory.CreateDirectory(folder);

            string logPath = Path.Combine(folder, "crash.log");

            File.AppendAllText(
                logPath,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{e.Exception}\n\n");

            MessageBox.Show(
                "Es ist ein Fehler aufgetreten. Das Programm bleibt geöffnet.\n\n" +
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
