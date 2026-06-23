using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyNotesApp.Models;
using MyNotesApp.Services;

namespace MyNotesApp.ViewModels;

public class TasksViewModel
{
    const string FileName = "tasks.json";
    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public async Task LoadAsync()
    {
        var list = await StorageService.LoadListAsync<TaskItem>(FileName);
        Tasks.Clear();
        foreach (var t in list.OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate)) Tasks.Add(t);
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        Tasks.Add(task);
        await SaveAsync();
    }

    public async Task ToggleCompleteAsync(string id)
    {
        var existing = Tasks.FirstOrDefault(t => t.Id == id);
        if (existing != null)
        {
            existing.IsCompleted = !existing.IsCompleted;
            await SaveAsync();
            await LoadAsync();
        }
    }

    async Task SaveAsync()
    {
        await StorageService.SaveListAsync(FileName, Tasks.ToList());
    }
}
