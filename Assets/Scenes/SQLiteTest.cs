using System.IO;
using SqlCipher4Unity3D;
using SQLite.Attributes;
using UnityEngine;


public class TestTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Value { get; set; }
    }


public class SQLiteTest : AbsChobiSQLiteMonoBehaviour
{
    const string dbFileName = "sqlite_test.db";

    private string _dbFilePath;
    public override string DbFilePath => _dbFilePath;

    public override int DbVersion => 1;

    public override void OnCreate(SQLiteConnection con)
    {
        con.CreateTable<TestTable>();
    }

    protected override void Awake()
    {
        _dbFilePath = Path.Join(Application.persistentDataPath, dbFileName);

        DeleteDbFile();

        base.Awake();
    }

    void Start()
    {


        /*
        await Task.Run(async () =>
        {
            Debug.Log($"TR 1:");
            
            WithTransaction(db =>
            {
                db.Insert(new TestTable() { Value = "aaaaaa" });
            });
            Debug.Log($"TR 1 END");
        });
        */

        /*
        const string dbFileName = "sqlite_test.db";
        //const string pw = "test, test, test---!!!";

        var path = Path.Join(Application.persistentDataPath, dbFileName);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var db = new ChobiSQLite(path, 1);

        Task.Run(async () =>
        {
            await db.WithTransactionAsync(d =>
            {
                d.CreateTable<TestTable>();
                d.Insert(new TestTable() { Value = "test-----" });
            });

            await db.WithTransactionAsync(d =>
            {
                var data = d.Table<TestTable>().FirstOrDefault();
                Debug.Log(data?.Value);
            });

            Debug.Log("END");
        });
        */


    }
}
