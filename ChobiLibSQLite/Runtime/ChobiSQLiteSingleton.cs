using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChobiLib.Unity.SQLite
{
    public class ChobiSQLiteSingleton : ChobiSQLite
    {
        public static ChobiSQLiteSingleton Instance { get; private set; }

        public static ChobiSQLiteSingleton GetInstance(
            string dbFilePath,
            int dbVersion = 1,
            string password = null,
            bool enableForeignKey = true,
            ISQLiteInitializer initializer = null,
            bool showDebugLog = true,
            bool reCreateDbFile = false
        )
        {
            if (reCreateDbFile && File.Exists(dbFilePath))
            {
                Instance?.Dispose();
                Instance = null;

                File.Delete(dbFilePath);
            }

            return Instance ??= new(dbFilePath, dbVersion, password, enableForeignKey, initializer, showDebugLog);
        }

        public static async Task<ChobiSQLiteSingleton> GetInstanceWithSQLiteKeyAsync(
            string dbFilePath,
            string hkAddr,
            string hsFileName,
            int dbVersion = 1,
            bool enableForeignKey = true,
            ISQLiteInitializer initializer = null,
            bool showDebugLog = true,
            bool reCreateDbFile = false,
            CancellationToken token = default
        )
        {
            token.ThrowIfCancellationRequested();
            var kData = await ChobiSQLiteKey.LoadKeyData(hkAddr, hsFileName, token);
            token.ThrowIfCancellationRequested();
            return GetInstance(dbFilePath, dbVersion, kData.GetKeyString(), enableForeignKey, initializer, showDebugLog, reCreateDbFile);
        }


        private ChobiSQLiteSingleton(string dbFilePath, int dbVersion = 1, string password = null, bool enableForeignKey = true, ISQLiteInitializer initializer = null, bool showDebugLog = true) : base(dbFilePath, dbVersion, password, enableForeignKey, initializer, showDebugLog)
        {
        }
    }
}