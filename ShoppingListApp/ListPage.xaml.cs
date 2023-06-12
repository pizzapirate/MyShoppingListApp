using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Core.Views;
using ShoppingListApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace ShoppingListApp;

public partial class ListPage : ContentPage, INotifyPropertyChanged
{
    private ShoppingListItem selectedReorderItem;
    private ObservableCollection<ShoppingListItem> viewData;
    public ObservableCollection<ShoppingListItem> ViewData
    {
        get { return viewData; }
        set { viewData = value; OnPropertyChanged(nameof(ViewData)); }
    }
    private bool isReorderModeActive;
    private bool isEditModeActive;
    public ListPage()
	{   
		InitializeComponent();
        ViewData = App.ShoppingListRepository.GetAll().Where(x => x.TableId == App.SelectedTable).ToObservableCollection();
        itemsList.ItemsSource = ViewData;
        ListName.Text = App.ListTableRepository.GetTableName(App.SelectedTable);
        this.BindingContext = this;
    }
    //private async Task InitializeDataAsync()
    //{
    //    var data = await App.ShoppingListRepository.GetAllAsync();
    //    ViewData = data.ToObservableCollection();
    //    itemsList.ItemsSource = ViewData;
    //    ListName.Text = App.ListTableRepository.GetTableName(App.SelectedTable);
    //}
    //-----------BASIC FUNCTIONS SECTION--------// // Performance isn't any better, but negates .NET MAUI shell animation. So, not good. 
    private async void Btn_AddItem(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Add a new item", "", "Save", "Cancel", "Enter item here");
        if (!string.IsNullOrEmpty(result))
        {   
            ShoppingListItem item = new ShoppingListItem { ItemName = result, TableId = App.SelectedTable};
            App.ShoppingListRepository.Add(item); // Add to DB
            // Update ViewData and itemList
            ViewData.Add(item);
        }
    }
    private void Btn_DeleteItem(object sender, EventArgs e)
    {   
        ShoppingListItem item = GetItemFromSender(sender);
        App.ShoppingListRepository.Delete(item);
        ViewData.Remove(item);
        //await RefreshItemsList();
    }
    private ShoppingListItem GetItemFromSender(object sender)
    {
        Grid parentGrid;
        ShoppingListItem item;

        if (sender is CheckBox checkBox)
        {
            parentGrid = (checkBox.Parent as Grid);
        }
        else if (sender is Button button)
        {
            parentGrid = (button.Parent as Grid);
        }
        else
        {
            // Handle the case if sender is neither CheckBox nor Button
            throw new InvalidOperationException("Invalid sender type.");
        }

        item = parentGrid.BindingContext as ShoppingListItem;
        return item;
    }
    private void LabelClicked(object sender, EventArgs e)
    {   
        if (isEditModeActive)
        {
            EditItem(GetItemFromSender(sender));
        }
        else
        {
            ShoppingListItem item = GetItemFromSender(sender);
            item.BasketStatus = !item.BasketStatus;
            // Update db
            App.ShoppingListRepository.Update(item);
            // Update ViewData
            int index = ViewData.IndexOf(item);
            if (index != -1)
            {
                ViewData[index] = item;
            }
        }
    }
    private void Btn_Menu(object sender, EventArgs e) // Navigates to the menu page
    {   
        Navigation.PopAsync();
    }
    //-----------EDIT SECTION-----------------//
    private async void Btn_Edit(object sender, EventArgs e)
    {   
        if (isReorderModeActive)
        {
            await CreateToast("Cannot edit whilst moving items!");
        }
        else
        {
            if (isEditModeActive)
            {
                ExitEditModeButton.IsVisible = false;
                AddItemButton.IsVisible = true;
                await CreateSnackbar("Edit mode deactivated.");
                isEditModeActive = false;
            }
            else
            {
                ExitEditModeButton.IsVisible = true;
                AddItemButton.IsVisible = false;
                await CreateSnackbar("Select an item to edit!");
                isEditModeActive = true;
            }
        }
    }
    private async void EditItem(ShoppingListItem item)
    {
        string result = await DisplayPromptAsync("Edit", "", "Save", "Cancel", item.ItemName);
        if (!string.IsNullOrEmpty(result))
        {   
            item.ItemName = result;
            // Update db
            App.ShoppingListRepository.Update(item);
            // Update ViewData
            int index = ViewData.IndexOf(item);
            if (index != -1)
            {
                ViewData[index] = item;
            }
        }
    }
    //-----------REORDER SECTION--------------//
    private async void Btn_Reorder(object sender, EventArgs e)
    {   
        if (isEditModeActive)
        {
            await CreateToast("Cannot move whilst editing items!");
        }
        else
        {
            // Display Toast
            if (isReorderModeActive)
            {
                if (selectedReorderItem != null)
                {
                    await CreateSnackbar("Changes saved.");
                }
                else { await CreateToast("No changes made."); }
                isReorderModeActive = false; SaveReorderedList(sender, e);
            }
            else { await CreateSnackbar("Drag an item to move it!"); isReorderModeActive = true; }
            //Display Buttons
            ReorderButtons.IsVisible = !ReorderButtons.IsVisible;
            AddItemButton.IsVisible = !AddItemButton.IsVisible;

            reorderItemsList.ItemsSource = ViewData;
            reorderItemsList.IsVisible = !reorderItemsList.IsVisible;
            itemsList.IsVisible = !itemsList.IsVisible;
        }
    }
    private void ReorderLabelClicked(object sender, EventArgs e)
    {
        //Grid buttonGridColumn;
        //Grid individualItemGrid;
        //if (selectedReorderItem is Button selectedItem) // Remove color styling if an item is selected
        //{   
        //    // For future, IF item is basketstatus = true, set color to basket status color
        //    buttonGridColumn = selectedItem.Parent as Grid;
        //    individualItemGrid = buttonGridColumn.Parent as Grid;
        //    individualItemGrid.BackgroundColor = Color.FromArgb("#6FFFA8"); // Minty Green
        //    selectedReorderItem = null;
        //}
        //if (sender != selectedReorderItem && sender is Button btn)
        //{
        //    buttonGridColumn = btn.Parent as Grid;
        //    individualItemGrid = buttonGridColumn.Parent as Grid;
        //    individualItemGrid.BackgroundColor = Color.FromArgb("#FD5564"); // Tinder Red
        //    selectedReorderItem = sender;
        //}
    } // NOT USED CURRENTLY / COMMENTED OUT
    private void SaveReorderedList(object sender, EventArgs e)
    {
        // delete all of the old stuff
        App.ListTableRepository.DeleteForReorder();
        // Update db
        App.ShoppingListRepository.AddRange(ViewData);
    }
    private void ItemDragStarting(object sender, DragStartingEventArgs e)
    {   // Change color of label, if u want
        DragGestureRecognizer dataGestureRecognizer = sender as DragGestureRecognizer;
        selectedReorderItem = dataGestureRecognizer.BindingContext as ShoppingListItem;
    }
    private void ItemDrop(object sender, DropEventArgs e)
    {
        DropGestureRecognizer dataGestureRecognizer = sender as DropGestureRecognizer;
        ShoppingListItem itemToBeDroppedOn = dataGestureRecognizer.BindingContext as ShoppingListItem;
        int dropIndex = ViewData.IndexOf(itemToBeDroppedOn);
        int selectedItemIndex = ViewData.IndexOf(selectedReorderItem);

        ViewData.RemoveAt(selectedItemIndex);
        ViewData.Insert(dropIndex, selectedReorderItem);
    }

    //-----------MISC SECTION----------------//
    private async Task CreateToast(string message)
    {
        // Display Toast or Snackbar telling user table deleted!
        CancellationTokenSource cancellationTokenSource = new();
        string text = message;
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }
    private async Task CreateSnackbar(string message)
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = Color.FromArgb("#ffa86f"),
            //ActionButtonTextColor = Colors.Yellow,
            CornerRadius = new CornerRadius(10),
            //Font = Font.SystemFontOfSize(14),
            //ActionButtonFont = Font.SystemFontOfSize(14),
            //CharacterSpacing = 0.5
        };

        string text = message;
        string actionButtonText = "Dismiss";
        //Action action = async () => await DisplayAlert("Snackbar ActionButton Tapped", "The user has tapped the Snackbar ActionButton", "OK");
        TimeSpan duration = TimeSpan.FromSeconds(3);

        var snackbar = Snackbar.Make(text, null, actionButtonText, duration, snackbarOptions);

        await snackbar.Show(cancellationTokenSource.Token);
    }
    protected override bool OnBackButtonPressed() // FOR HANDLING ANDROID BACK BUTTON
    {
        Navigation.PopAsync();
        return true;
    }
}
public class BooleanToStrikethroughConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {   
        if (value is bool isStrikethrough)
        {
            return isStrikethrough ? TextDecorations.Strikethrough : TextDecorations.None;
        }

        return TextDecorations.None;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
public class LabelColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Set the color based on the boolean value
            Color falseColor = Color.FromArgb("#6FFFA8"); // MintyGreen
            Color trueColor = Color.FromArgb("#429964"); // BasketStatus = true value. 

            return boolValue ? trueColor : falseColor;
        }

        // Return a default color if the value is not a boolean
        return Colors.Purple;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} // Used for changing label color when it is selected in move/reorder mode



