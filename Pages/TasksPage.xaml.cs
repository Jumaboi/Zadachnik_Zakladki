using Microsoft.Maui.Controls;
using MyNotesApp.Models;
using MyNotesApp.ViewModels;
using TaskStatus = MyNotesApp.Models.TaskStatus;

namespace MyNotesApp.Pages;

public partial class TasksPage : ContentPage
{
    readonly TasksViewModel vm;

    Picker? TaskFilterPickerControl => this.FindByName<Picker>("TaskFilterPicker");
    Picker? TaskSortPickerControl => this.FindByName<Picker>("TaskSortPicker");

    public TasksPage()
    {
        InitializeComponent();
        BindingContext = vm = new TasksViewModel();
        SyncPickers();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (vm != null)
            await vm.LoadAsync();

        SyncPickers();
        UpdateModeLabel();
    }

    async Task ReloadAsync()
    {
        if (vm == null) return;
        await vm.LoadAsync();
        UpdateModeLabel();
    }


    void UpdateModeLabel()
    {
        if (vm == null || TasksModeLabel == null)
            return;

        Dispatcher.Dispatch(() =>
        {
            TasksModeLabel.Text = vm.CurrentStatusFilter switch
            {
                "Удаленные" => "Удаленные задачи",
                "Выполненные" => "Выполненные задачи",
                "Все" => "Все задачи",
                _ => "Активные задачи"
            };
        });
    }

    async Task LoadTasksAsync(string? filter = null, string? sort = null)
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
        await LoadTasksAsync("Активные");
    }

    async void OnMenuCompletedClicked(object sender, System.EventArgs e)
    {
        TopMenuOverlay.IsVisible = false;
        await LoadTasksAsync("Выполненные");
    }

    async void OnMenuDeletedClicked(object sender, System.EventArgs e)
    {
        TopMenuOverlay.IsVisible = false;
        await LoadTasksAsync("Удаленные");
    }

    async void OnMenuSettingsClicked(object sender, System.EventArgs e)
    {
        TopMenuOverlay.IsVisible = false;
        await DisplayAlertAsync("Настройки", "Светлая/темная тема переключается кнопкой рядом с меню. Общие настройки можно расширить здесь позже.", "OK");
    }


    void SyncPickers()
    {
        var filterPicker = TaskFilterPickerControl;
        var sortPicker = TaskSortPickerControl;

        if (filterPicker != null && filterPicker.SelectedItem?.ToString() != vm.CurrentStatusFilter)
            filterPicker.SelectedItem = vm.CurrentStatusFilter;
        if (sortPicker != null && sortPicker.SelectedItem?.ToString() != vm.CurrentSortMode)
            sortPicker.SelectedItem = vm.CurrentSortMode;
    }

    async void OnTaskFilterChanged(object sender, System.EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is string filter && filter != vm.CurrentStatusFilter)
            await LoadTasksAsync(filter);
    }

    async void OnTaskSortChanged(object sender, System.EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is string sort && sort != vm.CurrentSortMode)
            await LoadTasksAsync(sort: sort);
    }

    async void OnResetFiltersClicked(object sender, System.EventArgs e)
    {
        await LoadTasksAsync("Активные", "Сначала срочные");
        await DisplayAlertAsync("Фильтры очищены", "Показаны активные задачи с сортировкой по срочности.", "OK");
    }

    async void OnFilterActiveClicked(object sender, System.EventArgs e) => await LoadTasksAsync("Активные");
    async void OnFilterInProgressClicked(object sender, System.EventArgs e) => await LoadTasksAsync("В процессе");
    async void OnFilterDeferredClicked(object sender, System.EventArgs e) => await LoadTasksAsync("Отложенные");
    async void OnFilterCompletedClicked(object sender, System.EventArgs e) => await LoadTasksAsync("Выполненные");
    async void OnFilterDeletedClicked(object sender, System.EventArgs e) => await LoadTasksAsync("Удаленные");
    async void OnFilterAllClicked(object sender, System.EventArgs e) => await LoadTasksAsync("Все");
    async void OnSortUrgentClicked(object sender, System.EventArgs e) => await LoadTasksAsync(sort: "Сначала срочные");
    async void OnSortNewestClicked(object sender, System.EventArgs e) => await LoadTasksAsync(sort: "Новые сверху");
    async void OnSortTitleClicked(object sender, System.EventArgs e) => await LoadTasksAsync(sort: "По заголовку");

    async void OnOpenAddTaskClicked(object sender, System.EventArgs e)
    {
        await AnimateClickAsync(sender);
        AddTaskOverlay.IsVisible = true;
    }

    void OnCloseAddTaskClicked(object sender, System.EventArgs e)
    {
        AddTaskOverlay.IsVisible = false;
    }

    void OnUseDueDateChanged(object sender, CheckedChangedEventArgs e)
    {
        DueDatePanel.IsVisible = e.Value;
        if (!e.Value) UseReminderCheckBox.IsChecked = false;
    }

    async void OnAddTaskClicked(object sender, System.EventArgs e)
    {
        if (vm == null) return;
        var title = TaskTitleEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Нужен заголовок", "Введите заголовок задачи.", "OK");
            return;
        }

        DateTime? due = null;
        DateTime? reminder = null;
        if (UseDueDateCheckBox.IsChecked)
        {
            TimeSpan dueTime = DueTimePicker.Time;
            due = DueDatePicker.Date.GetValueOrDefault(DateTime.UtcNow.AddHours(5).Date).Add(dueTime);
            if (UseReminderCheckBox.IsChecked) reminder = due;
        }

        await vm.AddTaskAsync(new TaskItem
        {
            Title = title,
            Description = TaskDescriptionEditor.Text?.Trim(),
            DueDate = due,
            ReminderAt = reminder,
            CreatedAt = DateTime.UtcNow.AddHours(5)
        });

        TaskTitleEntry.Text = string.Empty;
        TaskDescriptionEditor.Text = string.Empty;
        UseDueDateCheckBox.IsChecked = false;
        UseReminderCheckBox.IsChecked = false;
        DueDatePanel.IsVisible = false;
        AddTaskOverlay.IsVisible = false;

        await DisplayAlertAsync("Задача сохранена", reminder.HasValue ? $"Задача добавлена. Напоминание: {reminder:dd.MM.yyyy HH:mm}." : "Задача добавлена в активный список.", "OK");

        await this.FadeToAsync(0.98, 80);
        await this.FadeToAsync(1, 140);
    }

    async void OnSetInProgressClicked(object sender, System.EventArgs e) => await SetStatusFromButtonAsync(sender, TaskStatus.InProgress);

    async void OnSetDeferredClicked(object sender, System.EventArgs e) => await SetStatusFromButtonAsync(sender, TaskStatus.Deferred);

    async void OnSetCompletedClicked(object sender, System.EventArgs e) => await SetStatusFromButtonAsync(sender, TaskStatus.Completed);

    async void OnMoveToDeletedClicked(object sender, System.EventArgs e) => await SetStatusFromButtonAsync(sender, TaskStatus.Deleted);

    async Task SetStatusFromButtonAsync(object sender, TaskStatus status)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not TaskItem task) return;
        await AnimateClickAsync(sender);
        await vm.SetStatusAsync(task.Id, status);
        UpdateModeLabel();
        await DisplayAlertAsync("Статус изменен", $"Задача «{task.Title}» теперь: {StatusText(status)}.", "OK");
    }

    static string StatusText(TaskStatus status) => status switch
    {
        TaskStatus.InProgress => "в процессе",
        TaskStatus.Deferred => "отложена",
        TaskStatus.Completed => "выполнена",
        TaskStatus.Deleted => "в удаленных",
        _ => "активна"
    };

    async void OnDeleteForeverClicked(object sender, System.EventArgs e)
    {
        if (vm == null || sender is not Button b || b.BindingContext is not TaskItem task) return;
        if (task.Status != TaskStatus.Deleted)
        {
            await DisplayAlertAsync("Сначала в удаленные", "Чтобы удалить задачу навсегда, сначала переведите ее в статус «Удален». Потом откройте фильтр «Удаленные» и удалите окончательно.", "OK");
            return;
        }

        var ok = await DisplayAlertAsync("Удалить навсегда", "Окончательно удалить задачу без восстановления?", "Да", "Нет");
        if (!ok) return;
        await vm.DeleteForeverAsync(task.Id);
        UpdateModeLabel();
        await DisplayAlertAsync("Удалено", "Задача окончательно удалена.", "OK");
    }
}
