using Dapper;
using MySqlConnector;
using System.Data.Common;
using static CS2MenuManager.API.Class.ConfigManager;

namespace CS2MenuManager.API.Class;

internal static class Database
{
    private static readonly string GlobalDatabaseConnectionString;
    internal static bool IsMYSQLSet => !string.IsNullOrEmpty(GlobalDatabaseConnectionString);

    static Database()
    {
        MySQL config = Config.MySQL;
        GlobalDatabaseConnectionString = new MySqlConnectionStringBuilder
        {
            Server = config.Host,
            Database = config.Name,
            UserID = config.User,
            Password = config.Pass,
            Port = config.Port,
            Pooling = true,
            MinimumPoolSize = 0,
            MaximumPoolSize = 640,
            ConnectionIdleTimeout = 30,
            AllowZeroDateTime = true
        }.ConnectionString;
    }

    public static async Task<MySqlConnection> ConnectAsync()
    {
        MySqlConnection connection = new(GlobalDatabaseConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static async Task CreateDatabaseAsync()
    {
        using DbConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS cs2_menu_manager (
                id INT AUTO_INCREMENT PRIMARY KEY,
                PositionX FLOAT NOT NULL,
                PositionY FLOAT NOT NULL,
                SteamID BIGINT UNSIGNED NOT NULL UNIQUE
            );
        ");
    }

    public static async Task<(float PositionX, float PositionY)> Select(ulong SteamID)
    {
        using DbConnection connection = await ConnectAsync();

        (float, float)? row = await connection.QueryFirstOrDefaultAsync<(float, float)?>(
            "SELECT PositionX, PositionY FROM cs2_menu_manager WHERE SteamID = @SteamID;",
            new { SteamID });

        if (row.HasValue)
            return row.Value;

        ResolutionManager.Resolution defaultResolution = ResolutionManager.GetDefaultResolution();
        await Insert(SteamID, defaultResolution.PositionX, defaultResolution.PositionY);
        return (defaultResolution.PositionX, defaultResolution.PositionY);
    }

    public static async Task Insert(ulong SteamID, float PositionX, float PositionY)
    {
        using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO cs2_menu_manager (PositionX, PositionY, SteamID)
            VALUES (@PositionX, @PositionY, @SteamID)
            ON DUPLICATE KEY UPDATE PositionX = VALUES(PositionX), PositionY = VALUES(PositionY);
        ", new { PositionX, PositionY, SteamID });
    }
}