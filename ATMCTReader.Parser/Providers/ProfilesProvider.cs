using System;
using ATMCTReader.Models;

namespace ATMCTReader.Parser.Providers;

public class ProfilesProvider : BaseProvider<Profile>
{
    public ProfilesProvider() : base("ATMCTReader.Parser.Data.profiles.json")
    {
    }

}
