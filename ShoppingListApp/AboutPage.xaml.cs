using System.Windows.Input;

namespace ShoppingListApp;

public partial class AboutPage : ContentPage
{
    public AboutPage()
	{
		InitializeComponent();
	}
    private async void Btn_GitHub(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("https://github.com/pizzapirate");
    }
    private void Btn_Back(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
    protected override bool OnBackButtonPressed() // FOR HANDLING ANDROID BACK BUTTON
    {
        Navigation.PopAsync();
        return true;
    }
}