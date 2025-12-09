using ATMCTReader.Messages;
using CommunityToolkit.Mvvm.Messaging;

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
		WeakReferenceMessenger.Default.Send(new ReadCardRequestMessage());
    }
}