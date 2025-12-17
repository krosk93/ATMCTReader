using MauiIcons.Core;
using Microsoft.Maui.Controls.Shapes;

namespace ATMCTReader.Components;

public class CircleIcon : ContentView
{
	public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(Enum), typeof(CircleIcon), null);
	public Enum? Icon
	{
		get => (Enum)GetValue(CircleIcon.IconProperty);
		set => SetValue(CircleIcon.IconProperty, value);
	}

	public static readonly BindableProperty IconColorProperty = BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(CircleIcon), null);
	public Color IconColor
	{
		get => (Color)GetValue(CircleIcon.IconColorProperty);
		set => SetValue(CircleIcon.IconColorProperty, value);
	}

	public CircleIcon()
	{
		Content = new Border
		{
			WidthRequest = WidthRequest,
			HeightRequest = HeightRequest,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(WidthRequest / 2)
			},
			BackgroundColor = BackgroundColor,
			Content = new MauiIcon
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				IconSize = WidthRequest / 1.5d,
				IconColor = IconColor,
				Icon = Icon
			}
		};
	}
}