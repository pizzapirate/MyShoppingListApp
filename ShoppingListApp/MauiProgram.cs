using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ShoppingListApp.Repositories;

namespace ShoppingListApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		string dbPath = Path.Combine(FileSystem.AppDataDirectory, "shoppinglist.db");
		builder.Services.AddSingleton(s => ActivatorUtilities.CreateInstance<ShoppingListRepository>(s, dbPath));
        builder.Services.AddSingleton(s => ActivatorUtilities.CreateInstance<ListTableRepository>(s, dbPath));
        return builder.Build();
	}
}
