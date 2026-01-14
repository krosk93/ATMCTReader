using System;

namespace ATMCTReader.Triggers;

public class IdiomStateTrigger : StateTriggerBase
{
    public IdiomStateTrigger()
    {
    }

    public string Idiom
    {
        get => (string)GetValue(IdiomProperty);
        set => SetValue(IdiomProperty, value);
    }

    public static readonly BindableProperty IdiomProperty =
        BindableProperty.Create(nameof(Idiom), typeof(string), typeof(IdiomStateTrigger), string.Empty);

    static void OnIdiomChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        (bindable as IdiomStateTrigger)?.UpdateState();
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        UpdateState();
    }

    void UpdateState()
    {
        if(string.IsNullOrEmpty(Idiom))
            return;
        
        var idiom = DeviceIdiom.Create(Idiom);

        SetActive(idiom == DeviceInfo.Idiom);
    }
}
