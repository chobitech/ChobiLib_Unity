using System.IO;
using ChobiLib.Unity.SQLite;
using SQLite.Attributes;
using UnityEngine;

public class SQLiteTest : MonoBehaviour
{
    public class TestTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Value { get; set; }
    }

    void Start()
    {
        const string dbFileName = "sqlite_test.db";
        const string pw = "test, test, test---!!!";

        var path = Path.Join(Application.persistentDataPath, dbFileName);

        var db = new ChobiSQLite(path, 1, pw);

        db.WithTransaction(d =>
        {
            d.CreateTable<TestTable>();

            d.Insert(new TestTable() { Value = "test-----" });
        });

        db.WithTransaction(d =>
        {
            var data = d.Table<TestTable>().FirstOrDefault();
            Debug.Log(data?.Value);
        });


    }
}
