using BlankAtom.Database.Pipeline;
using NUnit.Framework;

namespace BA.xTest.src;

[TestFixture]
[TestOf(typeof(DatabaseMigrator))]
public class DatabaseMigratorTest
{

    [Test]
    public void MainTest()
    {
        ConnectionStringFactory stringFactory = new ConnectionStringFactory();
        string src = stringFactory.CreateConnectionString("mssql", "bg248", "ORAPS_SCSS_BS_Chongjian", "sa", "Oraps123");
        string des = stringFactory.CreateConnectionString("mssql", "bg248", "master", "sa", "Oraps123");
        // string src = "";
        // string des = "";

        DatasourceReaderFactory factory = new DatasourceReaderFactory();
        IDatasourceReader srcReader = factory.CreateReader("mssql", src);
        IDatasourceReader desReader = factory.CreateReader("mssql", des);

        DatasourceWriterFactory writerFactory = new DatasourceWriterFactory();
        IDatasourceWriter desWriter = writerFactory.CreateWriter("mssql", des);

        DatabaseMigrator migrator = new DatabaseMigrator(srcReader, desWriter);

        migrator.Migrate();

    }
}
