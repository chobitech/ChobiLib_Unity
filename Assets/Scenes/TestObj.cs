using System.Collections;
using System.Security.Cryptography;
using System.Text;
using ChobiLib;
using ChobiLib.Unity;
using ChobiLib.Unity.Security;
using Unity.VisualScripting;
using UnityEngine;

public class TestObj : AsyncInitializeMonoBehaviour<TestObj>
{
    protected override IEnumerator InitializeRoutine()
    {
        yield break;
    }


    public Transform testCube;

    void Start()
    {
        /*
        IEnumerator testRotate()
        {
            var degree = 0f;

            var point = new Vector3(1, 1, 1);
            var axis = new Vector3(0.5f, 0.5f, -0.5f);

            while (true)
            {
                degree += 1f;
                degree %= 360f;
                testCube.RotateAround(point, axis, 1f);
                yield return null;
            }
        }

        StartCoroutine(testRotate());
        */

        Debug.Log("=== AES test ===");

        var str = "test, test, test!!!";
        var key = ChobiAES.GenerateRandomBytes(32);
        var iv = ChobiAES.GenerateRandomBytes(16);

        var strBytes = Encoding.UTF8.GetBytes(str);

        /*
        var encData = ChobiAes256.Encrypt(Encoding.UTF8.GetBytes(str), key, iv);
        Debug.Log($"key = {key.Length}, iv = {iv.Length}, encData = {encData.Length}");

        var decData = ChobiAes256.Decrypt(encData, key, iv);
        */

        /*        
        var encData = ChobiAES.EncryptAndCompress(Encoding.UTF8.GetBytes(str), key, iv);
        Debug.Log($"encData = {encData.Length}");

        var decData = ChobiAES.DecryptFromCompressed(encData);
        Debug.Log($"decData = {Encoding.UTF8.GetString(decData)}");
        */
    }
}
