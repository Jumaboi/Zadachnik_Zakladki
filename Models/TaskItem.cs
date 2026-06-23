using System;

namespace MyNotesApp.Models;

public class TaskItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderAt { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.InProgress;

    // Совместимость со старым JSON, где был только bool IsCompleted.
    public bool IsCompleted
    {
        get => Status == TaskStatus.Completed;
        set
        {
            if (value) Status = TaskStatus.Completed;
        }
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(5);

    public string CreatedAtDisplay => $"Добавлено: {CreatedAt:dd.MM.yyyy HH:mm} (UTC+5)";
    public string DueDateDisplay => DueDate.HasValue ? $"Срок: {DueDate.Value:dd.MM.yyyy HH:mm}" : "Срок не задан";
    public string ReminderDisplay => ReminderAt.HasValue ? $"Напоминание: {ReminderAt.Value:dd.MM.yyyy HH:mm}" : "Без напоминания";
    public string StatusDisplay => Status switch
    {
        TaskStatus.InProgress => "В процессе",
        TaskStatus.Deferred => "Отложен",
        TaskStatus.Completed => "Выполнено",
        TaskStatus.Deleted => "Удален",
        _ => Status.ToString()
    };
}
