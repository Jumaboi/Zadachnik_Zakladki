using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MyNotesApp.Services;

public static class StorageService
{
    static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    static string GetFilePath(string filename)
    {
        return Path.Combine(FileSystem.AppDataDirectory, filename);
    }

    public static async Task<List<T>> LoadListAsync<T>(string filename)
    {
        var path = GetFilePath(filename);
        if (!File.Exists(path))
            return new List<T>();

        using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions) ?? new List<T>();
    }

    public static async Task SaveListAsync<T>(string filename, List<T> items)
    {
        var path = GetFilePath(filename);
        using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, items, _jsonOptions);
    }
}
