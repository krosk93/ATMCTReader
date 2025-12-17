using System;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public class AuthoritiesProvider: BaseProvider<Authority>
{
    public AuthoritiesProvider() : base("ATMCTReader.Parser.Data.authorities.json")
    {
    }
}
