namespace Phonebook;

public partial class SplashScreenPage : ContentPage
{
    public SplashScreenPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(2500);

        if (Application.Current != null)
        {
            Application.Current.MainPage = new AppShell();
        }
    }
}