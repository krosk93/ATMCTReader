using System;

namespace ATMCTReader.Models;

public class Profile : BaseModel
{
    private bool _empty = false;
    private string _name = "";
    public new string Name { 
        get
        {
            if(!string.IsNullOrWhiteSpace(_name) || _empty) return _name;
            else return Id.ToString();
        } 
        init
        {
            _name = value;
            if (string.IsNullOrWhiteSpace(_name)) _empty = true;
        }
    }
}
