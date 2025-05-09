using UnityEngine;

public class ChobiMonoBehaviour : MonoBehaviour
{
    public Transform SelfTransform { get; private set; }
    public RectTransform SelfRectTransform { get; private set; }

    protected virtual void Awake()
    {
        SelfTransform = transform;
        SelfRectTransform = GetComponent<RectTransform>();
    }
}
