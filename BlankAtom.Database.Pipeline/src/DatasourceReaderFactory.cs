namespace BlankAtom.Database.Pipeline;

public class DatasourceReaderFactory
{
    public IDatasourceReader CreateReader(string databaseType, string connectionString)
    {
        switch (databaseType.ToLower())
        {
            case "mysql":
                return new MySqlDatasourceReader(connectionString);
            case "mssql":
                return new MSSQLDataSourceReader(connectionString);
            default:
                throw new ArgumentException($"Unsupported database type: {databaseType}");
        }
    }
}

public class DatasourceWriterFactory
{
    public IDatasourceWriter CreateWriter(string databaseType, string connectionString)
    {
        switch (databaseType.ToLower())
        {
            case "mysql":
                return new MySqlDatasourceWriter(connectionString);
            case "mssql":
                return new MSSqlDatasourceWriter(connectionString);
            default:
                throw new ArgumentException($"Unsupported database type: {databaseType}");
        }
    }
}
