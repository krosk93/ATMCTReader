using System;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public interface IProvider<T> where T : BaseModel, new()
{
    T Get(int id);
    string GetName(int id);
}
