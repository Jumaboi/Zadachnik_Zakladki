using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNotesApp.Models;

public class Note
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<ChecklistItem> Checklist { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string ChecklistPreview => Checklist.Count == 0
        ? string.Empty
        : string.Join("\n", Checklist.Take(3).Select(item => $"{(item.IsChecked ? "☑" : "☐")} {item.Text}"));
}
