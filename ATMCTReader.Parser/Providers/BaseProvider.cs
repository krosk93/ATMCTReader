using System;
using System.Text.Json;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public abstract class BaseProvider<T> : IProvider<T> where T : BaseModel, new()
{
    private string ResourceName {get; init;}
    protected readonly Lazy<List<T>> _elements;

    public BaseProvider(string resourceName) {
        this.ResourceName = resourceName;
        _elements = new Lazy<List<T>>(Load);
    }

    private List<T> Load()
    {
        var assembly = typeof(BaseProvider<T>).Assembly;

            using var stream = assembly.GetManifestResourceStream(ResourceName)
                ?? throw new FileNotFoundException($"Recurs '{ResourceName}' no trobat.");

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            return JsonSerializer.Deserialize<List<T>>(json) 
                ?? throw new InvalidOperationException("JSON buit o invàlid.");
    }

    public T Get(int id)
    {
        return _elements.Value.FirstOrDefault(t => t.Id == id)
            ?? new T { Id = id, Name = id.ToString() };
    }

    public string GetName(int id)
    {
        return _elements.Value.FirstOrDefault(t => t.Id == id)?.Name ?? id.ToString();
    }
}
