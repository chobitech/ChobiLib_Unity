using System.Collections;
using System.Security.Cryptography;
using ChobiLib;
using ChobiLib.Unity;
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
    }
}
