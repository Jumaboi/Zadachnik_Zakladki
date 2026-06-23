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

    public async Task LoadAsync(string sortMode = "Новые сверху")
    {
        var list = await StorageService.LoadListAsync<Note>(FileName);
        var sorted = sortMode switch
        {
            "Старые сверху" => list.OrderBy(n => n.CreatedAt),
            "По заголовку" => list.OrderBy(n => n.Title),
            _ => list.OrderByDescending(n => n.CreatedAt)
        };

        Notes.Clear();
        foreach (var n in sorted) Notes.Add(n);
    }

    public async Task AddNoteAsync(Note note)
    {
        Notes.Insert(0, note);
        await SaveAsync();
    }

    public async Task UpdateNoteAsync(Note note)
    {
        var existing = Notes.FirstOrDefault(n => n.Id == note.Id);
        if (existing == null)
        {
            Notes.Insert(0, note);
        }
        else
        {
            var index = Notes.IndexOf(existing);
            Notes[index] = note;
        }

        await SaveAsync();
    }

    public async Task DeleteNoteAsync(string id)
    {
        var existing = Notes.FirstOrDefault(n => n.Id == id);
        if (existing != null)
        {
            Notes.Remove(existing);
            await SaveAsync();
        }
    }

    async Task SaveAsync()
    {
        await StorageService.SaveListAsync(FileName, Notes.ToList());
    }
}
