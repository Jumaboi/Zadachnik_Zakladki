using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyNotesApp.Models;
using MyNotesApp.Services;

namespace MyNotesApp.ViewModels;

public class NotesViewModel
{
    const string FileName = "notes.json";
    public ObservableCollection<Note> Notes { get; } = new();
    public string CurrentFilter { get; private set; } = "Активные";
    public string CurrentSortMode { get; private set; } = "Новые сверху";

    public async Task LoadAsync(string? filter = null, string? sortMode = null)
    {
        if (!string.IsNullOrWhiteSpace(filter)) CurrentFilter = filter;
        if (!string.IsNullOrWhiteSpace(sortMode)) CurrentSortMode = sortMode;

        var list = await StorageService.LoadListAsync<Note>(FileName);
        var filtered = CurrentFilter switch
        {
            "Удаленные" => list.Where(n => n.IsDeleted),
            "Все" => list,
            _ => list.Where(n => !n.IsDeleted)
        };

        var sorted = CurrentSortMode switch
        {
            "Старые сверху" => filtered.OrderBy(n => n.CreatedAt),
            "По заголовку" => filtered.OrderBy(n => n.Title),
            _ => filtered.OrderByDescending(n => n.CreatedAt)
        };

        Notes.Clear();
        foreach (var n in sorted) Notes.Add(n);
    }

    public async Task AddNoteAsync(Note note)
    {
        var all = await StorageService.LoadListAsync<Note>(FileName);
        all.Add(note);
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync("Активные", CurrentSortMode);
    }

    public async Task ToggleChecklistItemAsync(string noteId, string itemId)
    {
        var all = await StorageService.LoadListAsync<Note>(FileName);
        var note = all.FirstOrDefault(n => n.Id == noteId);
        var item = note?.Checklist.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return;

        item.IsChecked = !item.IsChecked;
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync();
    }

    public async Task MoveToDeletedAsync(string id)
    {
        var all = await StorageService.LoadListAsync<Note>(FileName);
        var existing = all.FirstOrDefault(n => n.Id == id);
        if (existing == null) return;

        existing.IsDeleted = true;
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync();
    }

    public async Task RestoreAsync(string id)
    {
        var all = await StorageService.LoadListAsync<Note>(FileName);
        var existing = all.FirstOrDefault(n => n.Id == id);
        if (existing == null) return;

        existing.IsDeleted = false;
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync();
    }

    public async Task DeleteForeverAsync(string id)
    {
        var all = await StorageService.LoadListAsync<Note>(FileName);
        var existing = all.FirstOrDefault(n => n.Id == id);
        if (existing == null) return;

        all.Remove(existing);
        await StorageService.SaveListAsync(FileName, all);
        await LoadAsync();
    }
}
