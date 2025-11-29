using System;

namespace ATMCTReader.Models;

public class Owner
{
    public required string Name { get; init; }
    public required string FirstSurname { get; init; }
    public required string SecondSurname { get; init; }

    private string Concatenated => $"{Name} {FirstSurname} {SecondSurname}";

    public bool IsAnonymous => string.IsNullOrWhiteSpace(Concatenated);

    public override string ToString()
    {
        return Concatenated.Trim();
    }
}
