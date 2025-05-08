using System.Security.Cryptography;
using ChobiLib;
using UnityEngine;

public class TestObj : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var curve = AnimationCurve.Linear(0, 1, 5, 3.6f);

        Debug.Log(curve.Evaluate(0));
        Debug.Log(curve.Evaluate(0.5f));
        Debug.Log(curve.Evaluate(1f));

        Debug.Log(curve.Evaluate(5f));
        Debug.Log(curve.Evaluate(-1f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
