using System.IO;
using SQLite.Attributes;
using UnityEngine;
using ChobiLib.Unity.SQLite.SecureDb;
using System.Threading.Tasks;
using System;
using System.Threading;
using ChobiLib.Unity.SQLite;

[Serializable]
public class STData
{
    public string s;
}

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

    protected override async Task<string> LoadHKeyAsync(CancellationToken token = default)
    {
        return await ChobiSQLiteKey.LoadHKeyFromTextAsset("sk.asset");
    }

    protected override Type[] GetAdditionalTableSchemes() => new Type[]
    {
        typeof(TestTable),
    };

    void Awake()
    {
        _dbFilePath = Path.Join(Application.persistentDataPath, dbFileName);
        _hSeedFilePath = Path.Join(Application.persistentDataPath, "test_seed_file");

        NoEncrypt = false;
        DeleteDbFile();
    }

    async Task Start()
    {
        //await InitDb();

        var vacuumed = await WithTransactionAsyncInBackgroundThread(db => db.VacuumUnusedSecureDbContentData<TestTable>());
        Debug.Log($"v = {vacuumed}");

        /*
        var stData = new STData()
        {
            s = "vffff"
        };

        var scd = await WithTransactionAsyncInBackground(db =>
        {
            return db.InsertSerializableAsSecureDbContentData(stData);
        });

        stData.s = "aaa";

        var res = await WithTransactionAsyncInBackground(db =>
        {
            return db.UpdateSerializableAsSecureDbContentData(stData, scd.ContentId);
        });
        Debug.Log(res);
        */


        var tt = new TestTable()
        {
            Value = "666",
        };
        Debug.Log(tt.ToJson());

        var cid = await WithTransactionAsyncInBackgroundThread(db =>
        {
            var d = db.InsertAbsSecureDbContentData(tt);
            return d.ContentId;
        });

        Debug.Log(cid);

        var reData = await WithTransactionAsyncInBackgroundThread(db =>
        {
            return db.LoadAbsSecureDbContentData<TestTable>(cid);
        });
        Debug.Log(reData);
    }
}
