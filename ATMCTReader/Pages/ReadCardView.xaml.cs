using ATMCTReader.Messages;
using ATMCTReader.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ATMCTReader.Pages;

public partial class ReadCardView : ContentPage
{
	public ReadCardView()
	{
		InitializeComponent();
		WeakReferenceMessenger.Default.Register<ReadCardResultMessage>(this, async (r, m) => {
            if (m == null) return;
            if (m.Success && m.Card != null) {
				await Shell.Current.GoToAsync("display", new Dictionary<string, object> {{"card", m.Card}});
			}
        });
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		WeakReferenceMessenger.Default.Send(new ReadCardRequestMessage());
    }
}