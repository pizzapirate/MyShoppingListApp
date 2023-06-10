using ShoppingListApp.Repositories;

namespace ShoppingListApp;

public partial class App : Application
{	
	public static ShoppingListRepository ShoppingListRepository { get; set; }
	public static ListTableRepository ListTableRepository { get; set; }
	public static int SelectedTable { get; set; }
	public App(ShoppingListRepository shoppingListRepository, ListTableRepository listTableRepository)
	{
		InitializeComponent();

		ShoppingListRepository = shoppingListRepository;
		ListTableRepository = listTableRepository;
		MainPage = new AppShell();
	}
}
