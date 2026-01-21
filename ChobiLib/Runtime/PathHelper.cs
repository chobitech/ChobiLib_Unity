using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ChobiLib.Unity
{
    public static class PathHelper
    {
        public static string GetPathOnPersistentDataPath(string fileName, IEnumerable<string> subDirs = null, bool withCreating = true)
        {
            string dirPath = (subDirs != null) ? Path.Join(Application.persistentDataPath, subDirs.JoinToString("/")) : Application.persistentDataPath;
            
            if (withCreating)
            {
                Directory.CreateDirectory(dirPath);
            }
            return Path.Join(dirPath, fileName);
        }

        public static async Task<string> GetPathOnPersistentDataPathInAsync(string fileName, IEnumerable<string> subDirs = null, bool withCreating = true)
        {
            return await ChobiUnityThread.RunOnMainThreadAsync(() => GetPathOnPersistentDataPath(fileName, subDirs, withCreating));
        }
    }
}