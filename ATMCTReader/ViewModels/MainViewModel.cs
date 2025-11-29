using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATMCTReader.ViewModels;

public class MainViewModel : ObservableObject
{
    int count = 0;
    
    public string CounterBtnText
    {
        get
        {
            if (count > 0) return $"Clicked {count} times";
            else return "Click me";
        }
    }
    public ICommand CounterCommand {get; }

    public MainViewModel()
    {
        CounterCommand = new RelayCommand(IncrementCounter);
    }
    
    private void IncrementCounter()
    {
        count++;
        OnPropertyChanged(nameof(CounterBtnText));
    }

}
