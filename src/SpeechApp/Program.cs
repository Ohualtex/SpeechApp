namespace SpeechApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using Stream iconResource = typeof(Program).Assembly.GetManifestResourceStream("SpeechApp.ico")!;
        using Icon icon = new(iconResource);
        using SpeechForm form = new() { Icon = icon };
        Application.Run(form);
    }
}
