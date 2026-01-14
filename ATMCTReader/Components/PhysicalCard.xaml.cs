namespace ATMCTReader.Components;

public partial class PhysicalCard : ContentView
{
	public static readonly BindableProperty AuthorityColorProperty = BindableProperty.Create(nameof(AuthorityColor), typeof(string), typeof(PhysicalCard), null);
	public string AuthorityColor
	{
		get => (string)GetValue(PhysicalCard.AuthorityColorProperty);
		set => SetValue(PhysicalCard.AuthorityColorProperty, value);
	}

	public static readonly BindableProperty OwnerNameProperty = BindableProperty.Create(nameof(OwnerName), typeof(string), typeof(PhysicalCard), null);
	public string OwnerName
	{
		get => (string)GetValue(PhysicalCard.OwnerNameProperty);
		set => SetValue(PhysicalCard.OwnerNameProperty, value);
	}

	public static readonly BindableProperty OwnerSurname1Property = BindableProperty.Create(nameof(OwnerSurname1), typeof(string), typeof(PhysicalCard), null);
	public string OwnerSurname1
	{
		get => (string)GetValue(PhysicalCard.OwnerSurname1Property);
		set => SetValue(PhysicalCard.OwnerSurname1Property, value);
	}

	public static readonly BindableProperty OwnerSurname2Property = BindableProperty.Create(nameof(OwnerSurname2), typeof(string), typeof(PhysicalCard), null);
	public string OwnerSurname2
	{
		get => (string)GetValue(PhysicalCard.OwnerSurname2Property);
		set => SetValue(PhysicalCard.OwnerSurname2Property, value);
	}

	public static readonly BindableProperty ProfileProperty = BindableProperty.Create(nameof(Profile), typeof(string), typeof(PhysicalCard), null);
	public string Profile
	{
		get => (string)GetValue(PhysicalCard.ProfileProperty);
		set => SetValue(PhysicalCard.ProfileProperty, value);
	}

	public static readonly BindableProperty AccountIdProperty = BindableProperty.Create(nameof(AccountId), typeof(string), typeof(PhysicalCard), null);
	public string AccountId
	{
		get => (string)GetValue(PhysicalCard.AccountIdProperty);
		set => SetValue(PhysicalCard.AccountIdProperty, value);
	}

	public static readonly BindableProperty CardIdProperty = BindableProperty.Create(nameof(CardId), typeof(string), typeof(PhysicalCard), null);
	public string CardId
	{
		get => (string)GetValue(PhysicalCard.CardIdProperty);
		set => SetValue(PhysicalCard.CardIdProperty, value);
	}

	public static readonly BindableProperty ExpireDateProperty = BindableProperty.Create(nameof(ExpireDate), typeof(DateTime?), typeof(PhysicalCard), null);
	public DateTime? ExpireDate
	{
		get => (DateTime)(GetValue(PhysicalCard.ExpireDateProperty) ?? DateTime.Now);
		set => SetValue(PhysicalCard.ExpireDateProperty, value);
	}

	public PhysicalCard()
	{
		InitializeComponent();
	}

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
		this.HeightRequest = width / 1.586;
    }
}