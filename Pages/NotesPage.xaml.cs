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
        UpdateModeLabel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (vm != null) await vm.LoadAsync();
        UpdateModeLabel();
    }

    void UpdateModeLabel()
    {
        if (vm == null) return;
        NotesModeLabel.Text = vm.CurrentFilter switch
        {
            "Удаленные" => "Удаленные заметки",
            "Все" => "Все заметки",
            _ => "Активные заметки"
        };
    }

    async Task LoadNotesAsync(string? filter = null, string? sort = null)
    {
        if (vm == null) return;
        await vm.LoadAsync(filter, sort);
        UpdateModeLabel();
    }

    async void OnBackClicked(object sender, System.EventArgs e) => await Navigation.PopAsync();

    void OnToggleThemeClicked(object sender, System.EventArgs e)
    {
        Application.Current!.UserAppTheme = Application.Current.UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
    }

    void OnOpenMenuClicked(object sender, System.EventArgs e) => TopMenuOverlay.IsVisible = true;
    void OnCloseMenuClicked(object sender, System.EventArgs e) => TopMenuOverlay.IsVisible = false;

    async void OnMenuDeletedClicked(object sender, System.EventArgs e)
    {
        TopMenuOverlay.IsVisible = false;
        await LoadNotesAsync("Удаленные");
    }

    async void OnMenuSettingsClicked(object sender, System.EventArgs e)
    {
        TopMenuOverlay.IsVisible = false;
        await DisplayAlertAsync("Настройки", "Светлая/темная тема переключается кнопкой рядом с меню. Общие настройки можно расширить здесь позже.", "OK");
    }

    async void OnShowActiveClicked(object sender, System.EventArgs e) => await LoadNotesAsync("Активные");
    async void OnShowDeletedClicked(object sender, System.EventArgs e) => await LoadNotesAsync("Удаленные");
    async void OnShowAllClicked(object sender, System.EventArgs e) => await LoadNotesAsync("Все");
    async void OnSortNewestClicked(object sender, System.EventArgs e) => await LoadNotesAsync(sort: "Новые сверху");
    async void OnSortOldestClicked(object sender, System.EventArgs e) => await LoadNotesAsync(sort: "Старые сверху");
    async void OnSortTitleClicked(object sender, System.EventArgs e) => await LoadNotesAsync(sort: "По заголовку");

    void OnOpenAddNoteClicked(object sender, System.EventArgs e)
    {
        AddNoteOverlay.IsVisible = true;
    }

    void OnCloseAddNoteClicked(object sender, System.EventArgs e)
    {
        AddNoteOverlay.IsVisible = false;
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
        UpdateModeLabel();
        await this.FadeToAsync(0.98, 80);
        await this.FadeToAsync(1, 140);
    }

    async void OnChecklistClicked(object sender, TappedEventArgs e)
    {
        if (vm == null || sender is not BindableObject bindable || bindable.BindingContext is not ChecklistItem item) return;
        var note = vm.Notes.FirstOrDefault(n => n.Checklist.Any(i => i.Id == item.Id));
        if (note == null) return;
        await vm.ToggleChecklistItemAsync(note.Id, item.Id);
    }

    async void OnDeleteClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not Note note) return;
        if (note.IsDeleted)
        {
            await DisplayAlertAsync("Уже в удаленных", "Для полного удаления нажмите ✖.", "OK");
            return;
        }
        await vm.MoveToDeletedAsync(note.Id);
        UpdateModeLabel();
    }

    async void OnRestoreNoteClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not Note note) return;
        await vm.RestoreAsync(note.Id);
        UpdateModeLabel();
    }

    async void OnDeleteForeverClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not Note note) return;
        if (!note.IsDeleted)
        {
            await DisplayAlertAsync("Сначала в удаленные", "Сначала нажмите 🗑, потом в разделе «Удаленные» можно удалить окончательно.", "OK");
            return;
        }
        var ok = await DisplayAlertAsync("Удалить навсегда", "Окончательно удалить заметку без восстановления?", "Да", "Нет");
        if (!ok) return;
        await vm.DeleteForeverAsync(note.Id);
        UpdateModeLabel();
    }
}
