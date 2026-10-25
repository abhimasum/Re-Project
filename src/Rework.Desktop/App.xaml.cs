namespace Rework.Desktop;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new Window(new MainPage()) { Title = "Rework Station", Width = 1280, Height = 800 };
}
