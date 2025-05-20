using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public static class ChobiLibUnity
    {
        public static string GetPathInPersistentDataPath(string fileName, IEnumerable<string> subDirs = null, bool withCreating = true)
        {
            string dirPath = (subDirs != null) ? Path.Join(Application.persistentDataPath, subDirs.JoinToString("/")) : Application.persistentDataPath;
            if (withCreating)
            {
                Directory.CreateDirectory(dirPath);
            }
            return Path.Join(dirPath, fileName);
        }

        public static string GetCrossPlatformSavedataPath(string dataDirName = "savedata")
        {
            var userDirPath = Application.persistentDataPath;
            return Path.Combine(userDirPath, dataDirName);
        }
        

        public static IEnumerator ToRoutine(this UnityAction action)
        {
            action();
            yield break;
        }
    }
}