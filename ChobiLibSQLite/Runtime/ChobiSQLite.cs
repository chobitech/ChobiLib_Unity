using System;
using System.IO;
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


        public async Task<T> WithAsync<T>(Func<SQLiteConnection, T> func)
        {
            using (await _lock.LockAsync())
            {
                return await Task.Run(() =>
                {
                    return func(_con);
                });
            }
        }

        public async Task WithAsync(UnityAction<SQLiteConnection> action)
        {
            await WithAsync<object>(db =>
            {
                action?.Invoke(_con);
                return null;
            });
        }

        public T WithTransaction<T>(Func<SQLiteConnection, T> func)
        {
            using (_lock.Lock())
            {
                if (_con.IsInTransaction)
                {
                    return func(_con);
                }

                try
                {
                    _con.BeginTransaction();
                    var res = func(_con);
                    _con.Commit();
                    return res;
                }
                catch
                {
                    _con.Rollback();
                    throw;
                }
            }
        }

        public void WithTransaction(UnityAction<SQLiteConnection> action) => WithTransaction<object>(
            r =>
            {
                action(r);
                return null;
            }
        );


        public async Task<T> WithTransactionAsync<T>(Func<SQLiteConnection, T> func)
        {
            using (await _lock.LockAsync())
            {
                return await Task.Run(() =>
                {
                    if (_con.IsInTransaction)
                    {
                        return func(_con);
                    }
                    else
                    {
                        try
                        {
                            _con.BeginTransaction();
                            var res = func(_con);
                            _con.Commit();
                            return res;
                        }
                        catch
                        {
                            _con.Rollback();
                            throw;
                        }
                    }
                });
            }
        }

        public async Task WithTransactionAsync(UnityAction<SQLiteConnection> action)
        {
            await WithTransactionAsync<object>(db =>
            {
                action?.Invoke(_con);
                return null;
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
