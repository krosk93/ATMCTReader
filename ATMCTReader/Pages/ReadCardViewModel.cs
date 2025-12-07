using System;
using ATMCTReader.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ATMCTReader.Pages;

public partial class ReadCardViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _error;

    [ObservableProperty]
    private Card? _card;
}
