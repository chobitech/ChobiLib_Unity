using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental;
using UnityEditor.Localization.Plugins.XLIFF.V20;

namespace ChobiLib.Unity
{
    public static class ChobiUnityThreadProcessAdder
    {
        [MenuItem("Tools/ChobiLib/Add UnityThreadProcess")]
        public static void AddUnityTHreadProcess()
        {
            var procObj = GameObject.FindAnyObjectByType<ChobiUnityThreadProcess>() ?? new GameObject("ChobiUnityThreadProcess", typeof(ChobiUnityThreadProcess)).GetComponent<ChobiUnityThreadProcess>();
            var script = MonoScript.FromMonoBehaviour(procObj);

            if (script != null)
            {
                var order = MonoImporter.GetExecutionOrder(script);
                if (order == 0)
                {
                    MonoImporter.SetExecutionOrder(script, -100);
                }
            }
        }
    }
}