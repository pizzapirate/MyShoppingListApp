using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using ShoppingListApp.Models;

namespace ShoppingListApp;

public partial class MenuPage : ContentPage
{
    Grid selectedTable;
	public MenuPage()
	{   
		InitializeComponent();
		UpdateListOfTables();
    }
	private async void Btn_AddNewTable(object sender, EventArgs e)
	{
		string result = await DisplayPromptAsync("Create a new list", "Give it a name!");
        if (!string.IsNullOrEmpty(result))
        {
            App.ListTableRepository.Add(new ListTableEntry
            {
                Name = result
            });
            UpdateListOfTables();
        }
    }
	private void UpdateListOfTables()
	{	
        List<ListTableEntry> tempList = App.ListTableRepository.GetAll();
        tablesList.ItemsSource = tempList;
        if (tempList.Count <= 0)
        {
            GreetingLabel.Text = "Oh no! You don't have any lists to display!";
        } else { GreetingLabel.Text = "Please select a list to open!"; }
    }
    private void TableClicked(object sender, TappedEventArgs e)
    {
        selectedTable = sender as Grid;
        ListTableEntry table = selectedTable.BindingContext as ListTableEntry;
        App.SelectedTable = table.Id;
        // Open ListPage and display all items where tableId = app.selectedtable
        ActivityIndicator activityIndicator = selectedTable.FindByName<ActivityIndicator>("activityIndicator");
        activityIndicator.IsVisible = true;
        ListPage next = new();
        Navigation.PushAsync(next);
    }
    private ListTableEntry GetItemFromSender(object sender)
    {
        Button btn = sender as Button;
        Grid parentGrid = btn.Parent as Grid;
        ListTableEntry item = parentGrid.BindingContext as ListTableEntry;
        return item;
    }
    private async void Btn_DeleteTable(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Danger", "Are you sure you want to delete this list?", "Yes", "No");
        if (answer)
        {
            ListTableEntry table = GetItemFromSender(sender);
            App.ListTableRepository.Delete(table);
            UpdateListOfTables();
            // Display Toast or Snackbar telling user table deleted!
            CancellationTokenSource cancellationTokenSource = new();
            string text = "List deleted!";
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
    }
    private void Btn_About(object sender, EventArgs e)
    {
        AboutPage next = new();
        Navigation.PushAsync(next);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (selectedTable != null)
        {
            ActivityIndicator activityIndicator = selectedTable.FindByName<ActivityIndicator>("activityIndicator");
            activityIndicator.IsVisible = false;
        }
    }
}

