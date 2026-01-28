using System.Collections;
using UnityEngine;

public class DebugCanvasTest : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);

    IEnumerator TestProcess()
    {
        var ct = 1;
        while (true)
        {
            ChobiDebugCanvas.Instance.AppendText($"test {ct}");
            ct++;
            yield return _waitForSeconds1;
        }
    }

    void Start()
    {
        StartCoroutine(TestProcess());
    }
}
