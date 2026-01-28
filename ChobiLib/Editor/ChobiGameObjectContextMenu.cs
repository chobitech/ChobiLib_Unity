using UnityEngine;
using UnityEditor;

public class ChobiGameObjectContextMenu
{
    [MenuItem("GameObject/chobitech/ChobiDebugCanvas", false, 10)]
    private static void CreateChobiDebugCanvas(MenuCommand menuCommand)
    {
        var path = "Packages/com.chobitech.unity/Runtime/Prefabs/DebugCanvas/ChobiDebugCanvas.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError("ChobiDebugCanvas prefab not found");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);

        Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);

        Selection.activeObject = instance;
    }
}
