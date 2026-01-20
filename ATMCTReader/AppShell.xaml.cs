using ATMCTReader.Pages;

namespace ATMCTReader;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("card/display", typeof(CardView));
		bool showTerms = Preferences.Default.Get("show_terms", true);
		if(showTerms)
			Navigation.PushModalAsync(new TermsView());
	}
}
