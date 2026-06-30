using Microsoft.Maui.Controls;

namespace MyNotesApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navigationPage = new NavigationPage(new MainPage());
        navigationPage.BarBackgroundColor = Color.FromArgb("#334155");
        navigationPage.BarTextColor = Colors.White;

        return new Window(navigationPage);
    }
}
