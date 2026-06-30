using Microsoft.Maui.Controls;
using MyNotesApp.Models;
using MyNotesApp.ViewModels;

namespace MyNotesApp.Pages;

public partial class NotesPage : ContentPage
{
    readonly NotesViewModel vm;

    Picker? NotesFilterPickerControl => this.FindByName<Picker>("NotesFilterPicker");
    Picker? NotesSortPickerControl => this.FindByName<Picker>("NotesSortPicker");

    public NotesPage()
    {
        InitializeComponent();
        BindingContext = vm = new NotesViewModel();
        SyncPickers();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is NotesViewModel vm)
            await vm.LoadAsync();

        SyncPickers();
        Dispatcher.Dispatch(UpdateModeLabel);
    }

    void UpdateModeLabel()
    {
        if (BindingContext is not NotesViewModel vm)
            return;

        if (NotesModeLabel == null)
            return;

        Dispatcher.Dispatch(() =>
        {
            NotesModeLabel.Text = vm.CurrentFilter switch
            {
                "Удаленные" => "Удаленные заметки",
                "Все" => "Все заметки",
                _ => "Активные заметки"
            };
        });
    }

    async Task LoadNotesAsync(string? filter = null, string? sort = null)
    {
        if (vm == null) return;
        await vm.LoadAsync(filter, sort);
        SyncPickers();
        UpdateModeLabel();
    }


    async Task AnimateClickAsync(object sender)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleToAsync(0.94, 60);
            await element.ScaleToAsync(1, 100, Easing.CubicOut);
        }
    }

    async void OnToggleThemeClicked(object sender, System.EventArgs e)
    {
        await AnimateClickAsync(sender);
        Application.Current!.UserAppTheme = Application.Current.UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
    }

    async void OnOpenMenuClicked(object sender, System.EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleToAsync(0.92, 70);
            await element.ScaleToAsync(1, 100);
        }
        TopMenuOverlay.IsVisible = true;
    }
    void OnCloseMenuClicked(object sender, System.EventArgs e) => TopMenuOverlay.IsVisible = false;

    async void OnMenuActiveClicked(object sender, System.EventArgs e)
    {
        TopMenuOverlay.IsVisible = false;
        await LoadNotesAsync("Активные");
    }

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


    void SyncPickers()
    {
        var filterPicker = NotesFilterPickerControl;
        var sortPicker = NotesSortPickerControl;

        if (filterPicker != null && filterPicker.SelectedItem?.ToString() != vm.CurrentFilter)
            filterPicker.SelectedItem = vm.CurrentFilter;
        if (sortPicker != null && sortPicker.SelectedItem?.ToString() != vm.CurrentSortMode)
            sortPicker.SelectedItem = vm.CurrentSortMode;
    }

    async void OnNotesFilterChanged(object sender, System.EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is string filter && filter != vm.CurrentFilter)
            await LoadNotesAsync(filter);
    }

    async void OnNotesSortChanged(object sender, System.EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is string sort && sort != vm.CurrentSortMode)
            await LoadNotesAsync(sort: sort);
    }

    async void OnResetFiltersClicked(object sender, System.EventArgs e)
    {
        await LoadNotesAsync("Активные", "Новые сверху");
        await DisplayAlertAsync("Фильтры очищены", "Показаны активные заметки: новые сверху.", "OK");
    }

    async void OnShowActiveClicked(object sender, System.EventArgs e) => await LoadNotesAsync("Активные");
    async void OnShowDeletedClicked(object sender, System.EventArgs e) => await LoadNotesAsync("Удаленные");
    async void OnShowAllClicked(object sender, System.EventArgs e) => await LoadNotesAsync("Все");
    async void OnSortNewestClicked(object sender, System.EventArgs e) => await LoadNotesAsync(sort: "Новые сверху");
    async void OnSortOldestClicked(object sender, System.EventArgs e) => await LoadNotesAsync(sort: "Старые сверху");
    async void OnSortTitleClicked(object sender, System.EventArgs e) => await LoadNotesAsync(sort: "По заголовку");

    async void OnOpenAddNoteClicked(object sender, System.EventArgs e)
    {
        await AnimateClickAsync(sender);
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
        await DisplayAlertAsync("Заметка сохранена", "Заметка добавлена в активный список.", "OK");
        await this.FadeToAsync(0.98, 80);
        await this.FadeToAsync(1, 140);
    }

    async void OnChecklistClicked(object sender, TappedEventArgs e)
    {
        if (vm == null || sender is not BindableObject bindable || bindable.BindingContext is not ChecklistItem item) return;
        var note = vm.Notes.FirstOrDefault(n => n.Checklist.Any(i => i.Id == item.Id));
        if (note == null) return;
        await vm.ToggleChecklistItemAsync(note.Id, item.Id);
        await DisplayAlertAsync("Чек-лист обновлен", $"Пункт «{item.Text}» изменен.", "OK");
    }

    async void OnDeleteClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not Note note) return;
        await AnimateClickAsync(sender);
        if (note.IsDeleted)
        {
            await DisplayAlertAsync("Уже в удаленных", "Для полного удаления нажмите ✖.", "OK");
            return;
        }
        await vm.MoveToDeletedAsync(note.Id);
        UpdateModeLabel();
        await DisplayAlertAsync("Заметка удалена", "Заметка перенесена в удаленные.", "OK");
    }

    async void OnRestoreNoteClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not Note note) return;
        await AnimateClickAsync(sender);
        await vm.RestoreAsync(note.Id);
        UpdateModeLabel();
        await DisplayAlertAsync("Заметка восстановлена", "Заметка снова в активном списке.", "OK");
    }

    async void OnDeleteForeverClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not Note note) return;
        await AnimateClickAsync(sender);
        var ok = await DisplayAlertAsync("Удалить навсегда", "Окончательно удалить заметку без восстановления?", "Да", "Нет");
        if (!ok) return;
        await vm.DeleteForeverAsync(note.Id);
        UpdateModeLabel();
        await DisplayAlertAsync("Удалено", "Заметка окончательно удалена.", "OK");
    }
}
