using System;
using System.Text.Json;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public class CompaniesProvider : BaseProvider<Company>
{
    public CompaniesProvider() : base("ATMCTReader.Parser.Data.companies.json")
    {
    }
}
