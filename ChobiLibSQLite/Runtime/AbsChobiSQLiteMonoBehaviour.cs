using System.Threading;
using System.Threading.Tasks;
using SqlCipher4Unity3D;

namespace ChobiLib.Unity.SQLite
{
    public abstract class AbsChobiSQLiteMonoBehaviour : AbsChobiSQLiteHolderBehaviour, ChobiSQLite.ISQLiteInitializer
    {

        public abstract string DbFilePath { get; }
        public abstract int DbVersion { get; }

        public virtual string DbPassword { get; } = null;

        public virtual bool EnableForeignKey => true;

        protected override async Task<ChobiSQLite> OpenChobiSQLite(CancellationToken token = default)
        {
            return new ChobiSQLite(DbFilePath, DbVersion, DbPassword, EnableForeignKey, this, ShowDebugLog);
        }

        public virtual void OnCreate(SQLiteConnection con) {}

        public virtual void OnUpgrade(SQLiteConnection con, int oldVersion, int newVersion) {}

        public virtual void OnOpen(SQLiteConnection connection) {}

        public override void DeleteDbFile(string dbFilePath = null)
        {
            base.DeleteDbFile(DbFilePath ?? dbFilePath);
        }
    }
}