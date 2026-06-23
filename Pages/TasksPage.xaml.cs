using Microsoft.Maui.Controls;
using MyNotesApp.Models;
using MyNotesApp.ViewModels;
using TaskStatus = MyNotesApp.Models.TaskStatus;

namespace MyNotesApp.Pages;

public partial class TasksPage : ContentPage
{
    readonly TasksViewModel? vm;

    public TasksPage()
    {
        InitializeComponent();
        vm = BindingContext as TasksViewModel;
        UpdateModeLabel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    async Task ReloadAsync()
    {
        if (vm == null) return;
        await vm.LoadAsync();
        UpdateModeLabel();
    }


    void UpdateModeLabel()
    {
        if (vm == null) return;
        TasksModeLabel.Text = vm.CurrentStatusFilter switch
        {
            "Удаленные" => "Удаленные задачи",
            "Выполненные" => "Выполненные задачи",
            "Все" => "Все задачи",
            _ => "Активные задачи"
        };
    }

    async Task LoadTasksAsync(string? filter = null, string? sort = null)
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
        await LoadTasksAsync("Удаленные");
    }

    async void OnMenuSettingsClicked(object sender, System.EventArgs e)
    {
        TopMenuOverlay.IsVisible = false;
        await DisplayAlertAsync("Настройки", "Светлая/темная тема переключается кнопкой рядом с меню. Общие настройки можно расширить здесь позже.", "OK");
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

    void OnOpenAddTaskClicked(object sender, System.EventArgs e)
    {
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

    void OnOpenAddTaskClicked(object sender, System.EventArgs e)
    {
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
            due = DueDatePicker.Date.GetValueOrDefault(DateTime.UtcNow.AddHours(5).Date).Add((TimeSpan)DueTimePicker.Time);
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

        if (reminder.HasValue)
        {
            await DisplayAlertAsync("Напоминание сохранено", $"Напоминание привязано к {reminder:dd.MM.yyyy HH:mm}.", "OK");
        }

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
        await vm.SetStatusAsync(task.Id, status);
        UpdateModeLabel();
    }

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
    }
}
