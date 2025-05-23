/*
using System.IO;
using UnityEditor;

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

                string sourceDirPath = "Packages/com.chobitech.unity.localization/Templates";
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
*/
