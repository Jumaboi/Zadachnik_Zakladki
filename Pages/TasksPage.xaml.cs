using Microsoft.Maui.Controls;
using MyNotesApp.Models;
using MyNotesApp.ViewModels;

namespace MyNotesApp.Pages;

public partial class TasksPage : ContentPage
{
    TasksViewModel vm;
    public TasksPage()
    {
        InitializeComponent();
        vm = BindingContext as TasksViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (vm != null) await vm.LoadAsync();
    }

    async void OnAddTaskClicked(object sender, System.EventArgs e)
    {
        var title = await DisplayPromptAsync("Новая задача", "Заголовок", maxLength: 100);
        if (string.IsNullOrWhiteSpace(title)) return;
        var desc = await DisplayPromptAsync("Новая задача", "Описание (необязательно)", maxLength: 200, keyboard: Keyboard.Text);
        var dateStr = await DisplayPromptAsync("Новая задача", "Дедлайн (YYYY-MM-DD HH:MM) или оставить пустым", placeholder: "2026-06-30 18:00");
        DateTime? due = null;
        if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, out var d)) due = d;
        await vm.AddTaskAsync(new TaskItem { Title = title, Description = desc, DueDate = due });
        // Простая анимация подтверждения добавления
        await this.FadeTo(0.98, 80);
        await this.FadeTo(1, 140);
    }

    async void OnCompleteClicked(object sender, System.EventArgs e)
    {
        if (!(sender is Button b) || b.BindingContext is not TaskItem t) return;
        await vm.ToggleCompleteAsync(t.Id);
    }
}
