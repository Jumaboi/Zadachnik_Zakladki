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

    public async Task LoadAsync()
    {
        var list = await StorageService.LoadListAsync<Note>(FileName);
        Notes.Clear();
        foreach (var n in list.OrderByDescending(n => n.CreatedAt)) Notes.Add(n);
    }

    public async Task AddNoteAsync(Note note)
    {
        Notes.Insert(0, note);
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
