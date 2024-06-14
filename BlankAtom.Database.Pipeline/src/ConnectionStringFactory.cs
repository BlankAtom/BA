namespace BlankAtom.Database.Pipeline;

public class ConnectionStringFactory
{
    public string CreateConnectionString(string databaseType, string server, string database, string userId, string password)
    {
        switch (databaseType.ToLower())
        {
            case "mysql":
                return $"Server={server};Database={database};Uid={userId};Pwd={password};";
            case "mssql":
                return $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;";
            default:
                throw new ArgumentException($"Unsupported database type: {databaseType}");
        }
    }
}
