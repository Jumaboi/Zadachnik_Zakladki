using Microsoft.Maui.Controls;

namespace MyNotesApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Простая анимация появления заголовка
        await TitleLabel.FadeToAsync(1, 400, Easing.CubicIn);
        await TitleLabel.ScaleToAsync(1.02, 200);
        await TitleLabel.ScaleToAsync(1, 120);
    }

    void OnLightThemeClicked(object sender, System.EventArgs e)
    {
        Application.Current!.UserAppTheme = AppTheme.Light;
    }

    void OnDarkThemeClicked(object sender, System.EventArgs e)
    {
        Application.Current!.UserAppTheme = AppTheme.Dark;
    }

    void OnSystemThemeClicked(object sender, System.EventArgs e)
    {
        Application.Current!.UserAppTheme = AppTheme.Unspecified;
    }

    async Task AnimateClickAsync(object sender)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleToAsync(0.96, 70);
            await element.ScaleToAsync(1, 110, Easing.CubicOut);
        }
    }

    async void OnNotesClicked(object sender, System.EventArgs e)
    {
        await AnimateClickAsync(sender);
        await Navigation.PushAsync(new Pages.NotesPage());
    }

    async void OnTasksClicked(object sender, System.EventArgs e)
    {
        await AnimateClickAsync(sender);
        await Navigation.PushAsync(new Pages.TasksPage());
    }
}
