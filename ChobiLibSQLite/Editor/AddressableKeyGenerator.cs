using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;


public static class AddressableKeyGenerator
{
    public const int KeySizeByte = 32;
    public const string AKey = "sk.asset";

    [MenuItem("Tools/ChobiLib Unity SecureDB/HKey")]
    public static void Generate()
    {
        var keyBytes = ChobiLib.ChobiLib.GenerateRandomBytes(KeySizeByte);

        var kB64 = System.Convert.ToBase64String(keyBytes);

        var aPath = EditorUtility.SaveFilePanelInProject(
            "Save HKey",
            "sk",
            "asset",
            "Select the folder to save HKey file"
        );

        if (aPath == null)
        {
            return;
        }

        if (AssetDatabase.AssetPathExists(aPath))
        {
            AssetDatabase.DeleteAsset(aPath);
        }

        var tAsset = new TextAsset(kB64);

        AssetDatabase.CreateAsset(tAsset, aPath);
        AssetDatabase.Refresh();

        var guid = AssetDatabase.AssetPathToGUID(aPath);

        var aSettings = AddressableAssetSettingsDefaultObject.Settings;
        var aGroup = aSettings.DefaultGroup;
        var aEntry = aSettings.CreateOrMoveEntry(guid, aGroup);
        aEntry.address = AKey;

        aSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, aEntry, true);
        AssetDatabase.SaveAssets();
    }

    
}
