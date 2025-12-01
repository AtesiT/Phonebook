namespace Phonebook;

public partial class SplashScreenPage : ContentPage {
    public SplashScreenPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Задержка для демонстрации заставки
        await Task.Delay(2500);

        // Переход на главное приложение
        if (Application.Current != null)
        {
            Application.Current.MainPage = new AppShell();
        }
    }
}