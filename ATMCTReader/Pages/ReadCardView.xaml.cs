using ATMCTReader.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ATMCTReader.Pages;

public partial class ReadCardView : ContentPage
{
	public ReadCardView()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		WeakReferenceMessenger.Default.Send(new ValueChangedMessage<Card?>(null));
    }
}