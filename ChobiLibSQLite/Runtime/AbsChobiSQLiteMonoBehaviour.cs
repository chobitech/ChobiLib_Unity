using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChobiLib.Unity.SQLite;
using SQLite;
using UnityEngine;
using UnityEngine.Events;

public abstract class AbsChobiSQLiteMonoBehaviour : MonoBehaviour, ChobiSQLite.ISQLiteInitializer
{

    public abstract string DbFilePath { get; }
    public abstract int DbVersion { get; }

    public virtual bool EnableForeignKey => true;

    private SynchronizationContext _mainContext;

    public UnityAction<SQLiteConnection> onAppQuit;


    public abstract void OnCreate(SQLiteConnection con);

    private ChobiSQLite _db;
    public ChobiSQLite Db => _db ??= GenerateDb();

    public void DeleteDbFile()
    {
        _db?.Dispose();
        _db = null;

        var path = DbFilePath;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private ChobiSQLite GenerateDb() => new(DbFilePath, DbVersion, EnableForeignKey, this);


    protected virtual void Awake()
    {
        _mainContext = SynchronizationContext.Current;
    }

    public void RunOnMainThread(UnityAction action)
    {
        if (action != null)
        {
            _mainContext?.Post(_ => action?.Invoke(), null);
        }
    }

    private void RunOnMainThread<T>(T arg, UnityAction<T> action)
    {
        if (action != null)
        {
            _mainContext?.Post(_ => action(arg), null);
        }
    }



    public T With<T>(Func<SQLiteConnection, T> func) => Db.With(func);
    public void With(UnityAction<SQLiteConnection> action) => Db.With(action);

    public T WithTransaction<T>(Func<SQLiteConnection, T> func) => Db.WithTransaction(func);
    public void WithTransaction(UnityAction<SQLiteConnection> action) => Db.WithTransaction(action);

    /*
    public void WithAsync<T>(Func<SQLiteConnection, Task<T>> asyncFunc, UnityAction<T> onFinished = null)
    {
        Task.Run(async () =>
        {
            var result = await Db.WithAsync(asyncFunc);
            RunOnMainThread(result, onFinished);
        });
    }
    */
    public async Task<T> WithAsync<T>(Func<SQLiteConnection, T> func)
    {
        return await Db.WithAsync(func);
    }

    public async Task WithAsync(UnityAction<SQLiteConnection> action)
    {
        await Db.WithAsync(action);
    }

    public async Task<T> WithTransactionAsync<T>(Func<SQLiteConnection, T> func)
    {
        return await Db.WithTransactionAsync(func);
    }

    public async Task WithTransactionAsync(UnityAction<SQLiteConnection> action)
    {
        await Db.WithTransactionAsync(action);
    }

    protected virtual void OnApplicationQuit()
    {
        WithTransaction(db => onAppQuit?.Invoke(db));
        _db?.Dispose();
        _db = null;
    }
}