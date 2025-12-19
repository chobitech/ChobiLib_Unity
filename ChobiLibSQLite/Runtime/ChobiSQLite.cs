using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity.SQLite
{
    public class ChobiSQLite : IDisposable
    {
        public interface ISQLiteInitializer
        {
            void OnCreate(SQLiteConnection connection) { }
            public virtual void OnUpgrade(SQLiteConnection con, int oldVersion, int newVersion) { }

            public virtual void OnOpen(SQLiteConnection connection) { }
        }

        public static string GetDbPathInPersistentData(string fileName) => Path.Join(Application.persistentDataPath, fileName);

        private SQLiteConnection _con;

        public readonly string dbFilePath;
        public readonly int dbVersion;

        private readonly AsyncLock _lock = new();

        public ChobiSQLite(string dbFilePath, int dbVersion, string password = null, bool enableForeignKey = true, ISQLiteInitializer initializer = null)
        {
            this.dbFilePath = dbFilePath;
            this.dbVersion = dbVersion;

            using (_lock.Lock())
            {
                _con = new SQLiteConnection(
                    databasePath: dbFilePath,
                    password: password
                );

                if (enableForeignKey)
                {
                    _con.Execute("PRAGMA foreign_keys = ON;");
                }

                
                

                var currentVer = _con.ExecuteScalar<int>("PRAGMA user_version;");

                var execOnCreate = currentVer == 0;
                var isSameVersion = currentVer == dbVersion;

                if (!isSameVersion)
                {
                    _con.Execute($"PRAGMA user_version = {dbVersion};");
                }

                if (initializer != null)
                {
                    initializer.OnOpen(_con);

                    if (execOnCreate)
                    {
                        initializer.OnCreate(_con);
                    }

                    if (!isSameVersion)
                    {
                        initializer.OnUpgrade(_con, currentVer, dbVersion);
                    }

                }
            }
        }

        public T With<T>(Func<SQLiteConnection, T> func)
        {
            using (_lock.Lock())
            {
                return func(_con);
            }
        }

        public void With(UnityAction<SQLiteConnection> action) => With<object>(r =>
        {
            action(r);
            return null;
        });

        private async Task<T> InnerAsyncFunc<T>(Func<SQLiteConnection, T> func)
        {
            using (await _lock.LockAsync())
            {
                if (Thread.CurrentThread.IsThreadPoolThread)
                {
                    return func(_con);
                }
                else
                {
                    return await Task.Run(() => func(_con));
                }
            }
        }

        private async Task InnerAsyncAction(UnityAction<SQLiteConnection> action)
        {
            await InnerAsyncFunc(c =>
            {
                action(c);
                return false;
            });
        }

        public async Task<T> WithAsync<T>(Func<SQLiteConnection, T> func)
        {
            return await InnerAsyncFunc(func);
        }

        public async Task WithAsync(UnityAction<SQLiteConnection> action)
        {
            await InnerAsyncAction(action);
        }

        private T InnerTransaction<T>(Func<SQLiteConnection, T> func)
        {
            if (_con.IsInTransaction)
            {
                return func(_con);
            }

            try
            {
                _con.BeginTransaction();
                var res = (func != null) ? func(_con) : default;
                _con.Commit();
                return res;
            }
            catch
            {
                _con.Rollback();
                throw;
            }
        }

        public T WithTransaction<T>(Func<SQLiteConnection, T> func)
        {
            using (_lock.Lock())
            {
                return InnerTransaction(func);
            }
        }

        public void WithTransaction(UnityAction<SQLiteConnection> action) => WithTransaction(
            c =>
            {
                action?.Invoke(c);
                return false;
            }
        );


        public async Task<T> WithTransactionAsync<T>(Func<SQLiteConnection, T> func)
        {
            return await InnerAsyncFunc(_ =>
            {
                return InnerTransaction(func);
            });
        }

        public async Task WithTransactionAsync(UnityAction<SQLiteConnection> action)
        {
            await WithTransactionAsync(db =>
            {
                action?.Invoke(_con);
                return false;
            });
        }
        

        public void Dispose()
        {
            using (_lock.Lock())
            {
                _con?.Close();
                _con?.Dispose();
                _con = null;
            }
        }
    }
}
