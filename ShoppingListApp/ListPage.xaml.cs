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
    private object selectedReorderItem;
    private ObservableCollection<ShoppingListItem> viewData;
    public ObservableCollection<ShoppingListItem> ViewData
    {
        get { return viewData; }
        set { viewData = value; OnPropertyChanged(nameof(ViewData)); }
    }
    private bool isReorderModeActive;
	public ListPage()
	{   
		InitializeComponent();
        ViewData = App.ShoppingListRepository.GetAll().Where(x => x.TableId == App.SelectedTable).ToObservableCollection();
        itemsList.ItemsSource = ViewData;
        this.BindingContext = this;
        ListName.Text = App.ListTableRepository.GetTableName(App.SelectedTable);
    }
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
        ShoppingListItem item = GetItemFromSender(sender);
        item.BasketStatus = !item.BasketStatus;
        App.ShoppingListRepository.Update(item);
        // Update ViewData
        int index = ViewData.IndexOf(item);
        if (index != -1)
        {
            ViewData[index] = item;
        }

        //await RefreshItemsList();
    }
    private void Btn_Menu(object sender, EventArgs e) // Navigates to the menu page
    {   
        Navigation.PopAsync();
    }
    private async void Btn_Complete(object sender, EventArgs e) // THIS BUTTON RESETS A LIST. 
    {
        //// Find out if there are any items that are not in the basket 
        //if (App.ShoppingListRepository.GetAll().Where(x => x.TableId == App.SelectedTable && x.BasketStatus == false).Any())
        //{
        //    string action = await DisplayActionSheet("What to do with items not ticked off?", "Cancel", null, "Delete", "Keep");
        //    switch(action)
        //    {
        //        case "Keep":
        //            // Delete all items where selectedtable and basketstatus = true
        //            App.ShoppingListRepository.DeleteBasketStatusTrueItems(App.SelectedTable);
        //            break;
        //        case "Delete":
        //            // delete all where selectedtable 
        //            App.ShoppingListRepository.DeleteEntireList(App.SelectedTable);
        //            break;
        //    }
        //}
        //else //  Else, if all items have basketstatus = true:
        //{
        //    App.ShoppingListRepository.DeleteEntireList(App.SelectedTable);
        //}
    }
    //-----------REORDER SECTION--------------//
    private async void Btn_Reorder(object sender, EventArgs e)
    {   
        // Display Toast
        if (isReorderModeActive) { await CreateToast("Move Mode Deactivated"); isReorderModeActive = false; }
        else { await CreateToast("Move Mode Activated"); isReorderModeActive = true; }
        //Display Buttons
        ReorderButtons.IsVisible = !ReorderButtons.IsVisible;
        AddItemButton.IsVisible = !AddItemButton.IsVisible;

        reorderItemsList.IsVisible = !reorderItemsList.IsVisible;
        itemsList.IsVisible = !itemsList.IsVisible;
        reorderItemsList.ItemsSource = ViewData;
    }
    private void ReorderLabelClicked(object sender, EventArgs e)
    {
        Grid buttonGridColumn;
        Grid individualItemGrid;
        if (selectedReorderItem is Button selectedItem) // Remove color styling if an item is selected
        {   
            // For future, IF item is basketstatus = true, set color to basket status color
            buttonGridColumn = selectedItem.Parent as Grid;
            individualItemGrid = buttonGridColumn.Parent as Grid;
            individualItemGrid.BackgroundColor = Color.FromArgb("#6FFFA8"); // Minty Green
            selectedReorderItem = null;
        }
        if (sender != selectedReorderItem && sender is Button btn)
        {
            buttonGridColumn = btn.Parent as Grid;
            individualItemGrid = buttonGridColumn.Parent as Grid;
            individualItemGrid.BackgroundColor = Color.FromArgb("#FD5564"); // Tinder Red
            selectedReorderItem = sender;
        }
    }
    private void ReorderDown(object sender, EventArgs e)
    {
        if (selectedReorderItem is Button)
        {
            ShoppingListItem selectedItem = GetItemFromSender(selectedReorderItem);
            MoveItemDown(selectedItem);
        }
    }
    private void ReorderUp(object sender, EventArgs e)
    {
        if (selectedReorderItem is Button)
        {
            ShoppingListItem selectedItem = GetItemFromSender(selectedReorderItem);
            MoveItemUp(selectedItem);
        }
    }
    private void MoveItemUp(ShoppingListItem selectedItem)
    {
        int selectedIndex = ViewData.IndexOf(selectedItem);
        if (selectedIndex != 0) // Check to see if it isn't already at top of list
        {
            ViewData.RemoveAt(selectedIndex);
            ViewData.Insert(selectedIndex - 1, selectedItem);
        }
    }
    private void MoveItemDown(ShoppingListItem selectedItem)
    {
        int selectedIndex = ViewData.IndexOf(selectedItem);
        if (selectedIndex != ViewData.Count - 1)
        {
            ViewData.RemoveAt(selectedIndex);
            ViewData.Insert(selectedIndex + 1, selectedItem);
        }
    }
    private void SaveReorderedList(object sender, EventArgs e)
    {
        // delete all of the old stuff
        App.ListTableRepository.DeleteForReorder();
        // Update db
        App.ShoppingListRepository.AddRange(ViewData);
        Btn_Reorder(sender, e);
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
    protected override bool OnBackButtonPressed() // FOR HANDLING ANDROID BACK BUTTON
    {
        Navigation.PopAsync();
        return true;
    }
    //public new event PropertyChangedEventHandler PropertyChanged;

    //protected override void OnPropertyChanged(string propertyName)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}
    //private void CheckBoxClicked(object sender, EventArgs e)
    //{
    //    ShoppingListItem item = GetItemFromSender(sender);
    //    item.BasketStatus = !item.BasketStatus;
    //    App.ShoppingListRepository.Update(item);
    //    RefreshItemsList();
    //    CheckBox cb = sender as CheckBox;
    //    cb.IsChecked = item.BasketStatus;
    //} // UNUSED, REMOVE
    //private void UpdateCheckedBox(object sender, EventArgs e)
    //{
    //    CheckBox cb = sender as CheckBox;
    //    ShoppingListItem item = GetItemFromSender(sender);
    //    item.BasketStatus = !item.BasketStatus;
    //    App.ShoppingListRepository.Update(item);
    //    RefreshItemsList();

    //    //if (cb.IsChecked == item.BasketStatus)
    //    //{
    //    //    // then good, we are good. 
    //    //}
    //    //else // if they are different!!! 
    //    //{
    //    //    item.BasketStatus = cb.IsChecked;
    //    //    App.ShoppingListRepository.Update(item);
    //    //    RefreshItemsList();
    //    //}
    //} // UNUSED, REMOVE 
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



