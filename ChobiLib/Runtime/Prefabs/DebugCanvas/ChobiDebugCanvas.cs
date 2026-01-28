using ChobiLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public class ChobiDebugCanvas : MonoBehaviour
{
    public static ChobiDebugCanvas Instance { get; private set; }

    [SerializeField]
    private Vector2 resolution = new(1920, 1080);

    [SerializeField]
    private TMP_Text debugText;

    private CanvasScaler canvasScaler;

    public void ClearText()
    {
        debugText.text = "";
    }

    public void SetText(string text)
    {
        debugText.text = text;
    }

    public void AppendText(string text, string joint = "\n")
    {
        string t;
        if (debugText.text != null && debugText.text.IsNotEmpty())
        {
            t = joint + text;
        }
        else
        {
            t = text;
        }
        debugText.text += t;
    }

    void Awake()
    {
        Instance = this;

        canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = resolution;
    }
}
