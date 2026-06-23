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
        await TitleLabel.FadeTo(1, 400, Easing.CubicIn);
        await TitleLabel.ScaleTo(1.02, 200);
        await TitleLabel.ScaleTo(1, 120);
    }

    async void OnNotesClicked(object sender, System.EventArgs e)
    {
        await Navigation.PushAsync(new Pages.NotesPage());
    }

    async void OnTasksClicked(object sender, System.EventArgs e)
    {
        await Navigation.PushAsync(new Pages.TasksPage());
    }
}
