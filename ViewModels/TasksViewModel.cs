using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyNotesApp.Models;
using MyNotesApp.Services;
using TaskStatus = MyNotesApp.Models.TaskStatus;

namespace MyNotesApp.ViewModels;

public class TasksViewModel
{
    const string FileName = "tasks.json";
    public ObservableCollection<TaskItem> Tasks { get; } = new();
    public string CurrentStatusFilter { get; private set; } = "Активные";
    public string CurrentSortMode { get; private set; } = "Сначала срочные";

    public async Task LoadAsync(string? statusFilter = null, string? sortMode = null)
    {
        if (!string.IsNullOrWhiteSpace(statusFilter)) CurrentStatusFilter = statusFilter;
        if (!string.IsNullOrWhiteSpace(sortMode)) CurrentSortMode = sortMode;

        var list = await StorageService.LoadListAsync<TaskItem>(FileName);
        var filtered = CurrentStatusFilter switch
        {
            "В процессе" => list.Where(t => t.Status == TaskStatus.InProgress),
            "Отложенные" => list.Where(t => t.Status == TaskStatus.Deferred),
            "Выполненные" => list.Where(t => t.Status == TaskStatus.Completed),
            "Удаленные" => list.Where(t => t.Status == TaskStatus.Deleted),
            "Все" => list,
            _ => list.Where(t => t.Status is TaskStatus.InProgress or TaskStatus.Deferred)
        };

        var sorted = CurrentSortMode switch
        {
            "Новые сверху" => filtered.OrderByDescending(t => t.CreatedAt),
            "Старые сверху" => filtered.OrderBy(t => t.CreatedAt),
            "По статусу" => filtered.OrderBy(t => t.Status).ThenBy(t => t.DueDate ?? DateTime.MaxValue),
            "По заголовку" => filtered.OrderBy(t => t.Title),
            _ => filtered.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ThenByDescending(t => t.CreatedAt)
        };

        Tasks.Clear();
        foreach (var t in sorted) Tasks.Add(t);
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        var all = await StorageService.LoadListAsync<TaskItem>(FileName);
        all.Add(task);
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync();
    }

    public async Task SetStatusAsync(string id, TaskStatus status)
    {
        var all = await StorageService.LoadListAsync<TaskItem>(FileName);
        var existing = all.FirstOrDefault(t => t.Id == id);
        if (existing == null) return;

        existing.Status = status;
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync();
    }

    public async Task DeleteForeverAsync(string id)
    {
        var all = await StorageService.LoadListAsync<TaskItem>(FileName);
        var existing = all.FirstOrDefault(t => t.Id == id);
        if (existing == null) return;

        all.Remove(existing);
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync();
    }
}
