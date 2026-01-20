using System.IO;
using SQLite.Attributes;
using UnityEngine;
using ChobiLib.Unity.SQLite.SecureDb;
using System.Threading.Tasks;
using System;

public class TestTable : AbsSecureDbContentData
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Value { get; set; }

    [Ignore]
    public int VVV { get; set; }

    public override string SecureDbContentDataId { get; set; }

    private int InnerInt { get; set; }

    public float FFF { get; }
}


public class SQLiteTest : AbsChobiSecureSQLiteMonoBehaviour
{
    const string dbFileName = "sqlite_test.db";

    private string _dbFilePath;
    public override string DbFilePath => _dbFilePath;

    public override int DbVersion => 1;

    private string _hSeedFilePath;
    protected override string HSeedFilePath => _hSeedFilePath;

    protected override async Task<string> LoadHKeyAsync()
    {
        return "";
    }

    protected override Type[] GetAdditionalTableSchemes() => new Type[]
    {
        typeof(TestTable),
    };

    void Awake()
    {
        _dbFilePath = Path.Join(Application.persistentDataPath, dbFileName);
        _hSeedFilePath = Path.Join(Application.persistentDataPath, "test_seed_file");

        Debug.Log(_dbFilePath);

        NoEncrypt = true;
        DeleteDbFile();
    }

    async Task Start()
    {
        await InitDb();

        var tt = new TestTable()
        {
            Value = "666",
        };
        Debug.Log(tt.ToJson());

        var cid = await WithTransactionAsyncInBackground(db =>
        {
            var d = db.InsertAbsSecureDbContentData(tt);
            return d.ContentId;
        });

        Debug.Log(cid);

        var reData = await WithTransactionAsyncInBackground(db =>
        {
            return db.LoadAbsSecureDbContentData<TestTable>(cid);
        });
        Debug.Log(reData);
        

        /*
        var test = new TTT();

        Debug.Log(JsonUtility.ToJson(test));
        Debug.Log(JsonConvert.SerializeObject(test));
        */


        /*
        var tt = new TestTable()
        {
            Value = "a",
            VVV = 6,
        };

        await InitDb();

        var scd = await WithTransactionAsyncInBackground(db =>
        {
            var scd = db.InsertAbsSecureDbContentData(tt);
            return scd;
        });

        Debug.Log($"content ID = {scd.ContentId}");

        var rData = await WithTransactionAsyncInBackground(db =>
        {
            return db.LoadAbsSecureDbContentData<TestTable>(scd.ContentId);
        });

        Debug.Log(rData);
        */

        /*
        var tt = typeof(TestTable);

        var props = tt.GetProperties();

        foreach (var p in props)
        {
            var isIgnore = p.IsDefined(typeof(SQLite.Attributes.IgnoreAttribute), true);
            Debug.Log($"{p.Name}: canRead = {p.CanRead}, canWrite = {p.CanWrite}, {isIgnore}");
        }
        */

        /*
        var data =  new TestTable();

        Debug.Log($"{data.ToJson()}");
        */

        /*
        Debug.Log($"isMainThread = {ChobiThreadInfo.IsInMainThread}");

        await Task.Run(() => Debug.Log($"isMainThread = {ChobiThreadInfo.IsInMainThread}"));
        */


        
        /*
        var obj = new TTT()
        {
            s = "aaaaa--"
        };
        Debug.Log($"obj = {obj}");
        
        
        
        var cData = SecureDbContentData.CreateContentDataFromSerializable(obj);
        Debug.Log(cData);

        var dData = cData.ConvertTo<TTT>();
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
