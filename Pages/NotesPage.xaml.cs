using Microsoft.Maui.Controls;
using MyNotesApp.Models;
using MyNotesApp.ViewModels;

namespace MyNotesApp.Pages;

public partial class NotesPage : ContentPage
{
    NotesViewModel vm;
    public NotesPage()
    {
        InitializeComponent();
        vm = BindingContext as NotesViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (vm != null) await vm.LoadAsync();
    }

    async void OnAddNoteClicked(object sender, System.EventArgs e)
    {
        // Простой диалог для создания заметки
        var title = await DisplayPromptAsync("Новая заметка", "Заголовок", maxLength: 100);
        if (string.IsNullOrWhiteSpace(title)) return;
        var content = await DisplayPromptAsync("Новая заметка", "Содержимое", maxLength: 200, keyboard: Keyboard.Text);
        await vm.AddNoteAsync(new Note { Title = title, Content = content ?? string.Empty });
        // Небольшая анимация страницы при добавлении
        await this.FadeTo(0.98, 80);
        await this.FadeTo(1, 140);
    }

    async void OnDeleteClicked(object sender, System.EventArgs e)
    {
        if (!(sender is Button b) || b.BindingContext is not Note note) return;
        var ok = await DisplayAlert("Подтвердить", "Удалить заметку?", "Да", "Нет");
        if (!ok) return;
        await vm.DeleteNoteAsync(note.Id);
    }
}
