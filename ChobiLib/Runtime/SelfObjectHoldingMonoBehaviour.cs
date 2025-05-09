using UnityEngine;

public class SelfObjectHoldingMonoBehaviour : MonoBehaviour
{
    public Transform SelfTransform { get; private set; }
    public RectTransform SelfRectTransform { get; private set; }
    public GameObject SelfGameObject { get; private set; }

    protected virtual void Awake()
    {
        SelfTransform = transform;
        SelfRectTransform = GetComponent<RectTransform>();
        SelfGameObject = gameObject;
    }
}
