using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlatformWellSync.Data;
using PlatformWellSync.Services;

const string Username = "user@aemenersol.com";
const string Password = "Test@123";
const string ConnectionString =
    @"Server=(localdb)\MSSQLLocalDB;Database=PlatformWellSyncDb;Trusted_Connection=True;MultipleActiveResultSets=true";

var services = new ServiceCollection();
services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});
services.AddDbContext<AppDbContext>(options => options.UseSqlServer(ConnectionString));
services.AddHttpClient<ApiService>();
services.AddScoped<SyncService>();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    logger.LogInformation("Setting up database...");

    // Drop and recreate to ensure tables are created fresh
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    logger.LogInformation("Database ready. Tables created.");
}

using (var scope = serviceProvider.CreateScope())
{
    var apiService = scope.ServiceProvider.GetRequiredService<ApiService>();
    var syncService = scope.ServiceProvider.GetRequiredService<SyncService>();

    var loggedIn = await apiService.LoginAsync(Username, Password);
    if (!loggedIn) { logger.LogError("Login failed. Exiting."); return; }

    logger.LogInformation("=== Syncing ACTUAL data ===");
    var actualData = await apiService.GetPlatformWellActualAsync();
    if (actualData.Count > 0)
        await syncService.SyncAsync(actualData);
    else
        logger.LogWarning("No actual data returned.");

    logger.LogInformation("=== Syncing DUMMY data (resilience test) ===");
    var dummyData = await apiService.GetPlatformWellDummyAsync();
    if (dummyData.Count > 0)
        await syncService.SyncAsync(dummyData);
    else
        logger.LogWarning("No dummy data returned.");

    logger.LogInformation("All done!");
}