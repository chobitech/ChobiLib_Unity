using UnityEngine;

namespace ChobiLib.Unity
{
    public class SelfObjectHoldingMonoBehaviour : MonoBehaviour
    {
        public Transform SelfTransform { get; private set; }
        public RectTransform SelfRectTransform { get; private set; }
        public GameObject SelfGameObject { get; private set; }

        public Transform FindChild(string name) => SelfTransform.Find(name);

        public bool Active
        {
            get => SelfGameObject.activeSelf;
            set => SelfGameObject.SetActive(value);
        }

        protected virtual void Awake()
        {
            SelfTransform = transform;
            SelfRectTransform = GetComponent<RectTransform>();
            SelfGameObject = gameObject;
        }
    }
}