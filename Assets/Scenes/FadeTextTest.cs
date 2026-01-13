using ChobiLib.Unity;
using UnityEngine;

public class FadeTextTest : MonoBehaviour
{
    private FadingTMPText fadingTMPText;

    void Awake()
    {
        fadingTMPText = GetComponent<FadingTMPText>();
    }

    void Start()
    {
        fadingTMPText.StartTextFading(
            0.5f,
            0f,
            1f,
            true,
            2,
            0.1f,
            2f
        );
    }
}
