using System;

namespace ATMCTReader.Models;

public abstract class BaseModel
{
    private string _name = "";
    public int Id { get; init; }
    public string Name { 
        get => string.IsNullOrWhiteSpace(_name) ? Id.ToString() : _name;
        init
        {
            _name = value;
        }
    }
}
