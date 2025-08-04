using System.Data.Common;
using Dapper;
using MySqlConnector;
using static CS2MenuManager.API.Class.ConfigManager;

namespace CS2MenuManager.API.Class;

internal static class Database
{
    private static readonly string GlobalDatabaseConnectionString;
    internal static readonly bool IsMYSQLSet;

    static Database()
    {
        List<string> credentials = [
            Config.MySQL.Host,
            Config.MySQL.Name,
            Config.MySQL.User,
            Config.MySQL.Pass
        ];

        if (credentials.Any(string.IsNullOrEmpty))
        {
            IsMYSQLSet = false;
            GlobalDatabaseConnectionString = string.Empty;
            return;
        }

        GlobalDatabaseConnectionString = new MySqlConnectionStringBuilder
        {
            Server = credentials[0],
            Database = credentials[1],
            UserID = credentials[2],
            Password = credentials[3],
            Port = Config.MySQL.Port,
            Pooling = true,
            MinimumPoolSize = 0,
            MaximumPoolSize = 640,
            ConnectionIdleTimeout = 30,
            AllowZeroDateTime = true
        }.ConnectionString;

        IsMYSQLSet = true;
    }

    private static async Task<MySqlConnection> ConnectAsync()
    {
        MySqlConnection connection = new(GlobalDatabaseConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static void CreateDatabase()
    {
        if (!IsMYSQLSet)
            return;

        Task.Run(CreateDatabaseAsync);
    }

    private static async Task CreateDatabaseAsync()
    {
        await using DbConnection connection = await ConnectAsync();

        await connection.ExecuteAsync("""

                                                  CREATE TABLE IF NOT EXISTS cs2_menu_manager (
                                                      id INT AUTO_INCREMENT PRIMARY KEY,
                                                      Menu VARCHAR(255),
                                                      SteamID BIGINT UNSIGNED NOT NULL UNIQUE
                                                  );
                                              
                                      """);
    }

    public static async Task<string?> SelectMenu(ulong SteamID)
    {
        await using DbConnection connection = await ConnectAsync();

        string? row = await connection.QueryFirstOrDefaultAsync<string?>(
            "SELECT Menu FROM cs2_menu_manager WHERE SteamID = @SteamID;",
            new { SteamID });

        return row;
    }

    public static async Task InsertMenu(ulong SteamID, string Menu)
    {
        await using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync("""

                                                  INSERT INTO cs2_menu_manager (Menu, SteamID)
                                                  VALUES (@Menu, @SteamID)
                                                  ON DUPLICATE KEY UPDATE Menu = VALUES(Menu);
                                      """
            , new { Menu, SteamID });
    }
}