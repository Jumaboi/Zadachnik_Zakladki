using Microsoft.Maui.Controls;
using MyNotesApp.Models;
using MyNotesApp.ViewModels;

namespace MyNotesApp.Pages;

public partial class NotesPage : ContentPage
{
    readonly NotesViewModel? vm;

    public NotesPage()
    {
        InitializeComponent();
        vm = BindingContext as NotesViewModel;
        SortPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (vm != null) await vm.LoadAsync(SortPicker.SelectedItem?.ToString() ?? "Новые сверху");
    }

    void OnOpenAddNoteClicked(object sender, System.EventArgs e)
    {
        AddNoteOverlay.IsVisible = true;
    }

    void OnCloseAddNoteClicked(object sender, System.EventArgs e)
    {
        AddNoteOverlay.IsVisible = false;
    }

    async void OnAddNoteClicked(object sender, System.EventArgs e)
    {
        if (vm == null) return;
        var title = NoteTitleEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Нужен заголовок", "Введите заголовок заметки.", "OK");
            return;
        }

        var note = new Note
        {
            Title = title,
            Content = NoteContentEditor.Text?.Trim() ?? string.Empty,
            Checklist = (ChecklistEditor.Text ?? string.Empty)
                .Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(text => new ChecklistItem { Text = text.Trim() })
                .Where(item => !string.IsNullOrWhiteSpace(item.Text))
                .ToList()
        };

        await vm.AddNoteAsync(note);
        NoteTitleEntry.Text = string.Empty;
        NoteContentEditor.Text = string.Empty;
        ChecklistEditor.Text = string.Empty;
        AddNoteOverlay.IsVisible = false;
        await this.FadeToAsync(0.98, 80);
        await this.FadeToAsync(1, 140);
    }

    async void OnDeleteClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not Note note) return;
        var ok = await DisplayAlertAsync("Подтвердить", "Удалить заметку?", "Да", "Нет");
        if (!ok) return;
        await vm.DeleteNoteAsync(note.Id);
    }

    async void OnSortChanged(object sender, System.EventArgs e)
    {
        if (vm != null) await vm.LoadAsync(SortPicker.SelectedItem?.ToString() ?? "Новые сверху");
    }
}
