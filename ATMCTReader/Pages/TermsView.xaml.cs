namespace ATMCTReader.Pages;

public partial class TermsView : ContentPage
{
	public TermsView()
	{
		InitializeComponent();
		DontShowAgain.IsChecked = !Preferences.Default.Get("show_terms", true);
	}

	void OnExitClicked(object sender, EventArgs args)
	{
		Application.Current?.Quit();
	}

	async void OnContinueClicked(object sender, EventArgs args)
	{
		Preferences.Default.Set("show_terms", !DontShowAgain.IsChecked);
		await Navigation.PopModalAsync();
	}
}