using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;

namespace ChobiLib.Unity.Localization
{
    [InitializeOnLoad]
    public static class PostInstallProcess
    {
        private const string markerKey = "ChobiLocalization_Installed";

        static PostInstallProcess()
        {
            if (!SessionState.GetBool(markerKey, false))
            {
                SessionState.SetBool(markerKey, true);

                string locPackageName = "ChobiLocalization";
                var listRequest = Client.List();

                while (!listRequest.IsCompleted) { }

                var pInfo = listRequest.Result.FirstOrDefault(p => p.name == locPackageName);

                if (pInfo == null)
                {
                    return;
                }

                string sourceDirPath = pInfo.resolvedPath;
                string destDirPath = "Assets/ChobiLocalization";

                var srcDi = new DirectoryInfo(sourceDirPath);
                var destDi = new DirectoryInfo(destDirPath);

                if (!srcDi.Exists)
                {
                    return;
                }

                if (!destDi.Exists)
                {
                    destDi.Create();
                }

                foreach (var fInfo in srcDi.GetFiles())
                {
                    if (fInfo.Name.EndsWith(".meta"))
                    {
                        continue;
                    }
                    
                    string destPath = Path.Combine(destDi.FullName, fInfo.Name);
                    if (!File.Exists(destPath))
                    {
                        fInfo.CopyTo(destDirPath);
                    }
                }
            }
        }

    }
}
