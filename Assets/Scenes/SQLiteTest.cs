using System.IO;
using SqlCipher4Unity3D;
using SQLite.Attributes;
using UnityEngine;
using ChobiLib.Unity.SQLite;
using ChobiLib.Unity.SQLite.SecureDb;
using System.Collections.Generic;

[System.Serializable]
public class TTT
{
    public string s;
}

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

    void Awake()
    {
        _dbFilePath = Path.Join(Application.persistentDataPath, dbFileName);

        DeleteDbFile();
    }

    async void Start()
    {
        /*
        var obj = new TTT()
        {
            s = "aaaaa--"
        };
        
        var cData = SecureDbContentDataDao.CreateContentData(obj);
        Debug.Log(cData);

        var dData = SecureDbContentDataDao.InstantiateFromContentData<TTT>(cData);
        Debug.Log(dData?.s);

        onAppPausedProcessInBackground += con =>
        {
            Debug.Log(con);
        };

        await WithTransactionAsyncInBackground(db =>
        {
            db.Insert(new TestTable() { Value = "aaaaaaaaa" });
        });

        onAppQuitProcessInBackground += db =>
        {
            Debug.Log($"onAppQuit");
        };
        */



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
