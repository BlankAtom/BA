using System.Text.Json.Serialization;

public class ConnectionConfiguration
{
    public string ip { get; set; }
    public string? port { get; set; }
    public string catalog { get; set; }
    public string user { get; set; }
    public string word { get; set; }
    public DatasourceType type { get; set; }

    public ConnectionConfiguration()
    { }
}

[JsonConverter(typeof(JsonStringEnumConverter<DatasourceType>))]
public enum DatasourceType
{
    MSSQL = 1,
    MYSQL = 2,
    ORACLE = 3
}