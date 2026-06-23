using System;

namespace MyNotesApp.Models;

public class ChecklistItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public string DisplayText => IsChecked ? $"✓ {Text}" : $"○ {Text}";
}
