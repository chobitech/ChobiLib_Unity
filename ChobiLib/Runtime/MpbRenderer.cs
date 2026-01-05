using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    [RequireComponent(typeof(Renderer))]
    public class MpbRenderer : MonoBehaviour
    {
        public Renderer Renderer { get; private set; }

        protected MaterialPropertyBlock PropertyBlock { get; private set; }


        public void ApplyProperty()
        {
            Renderer.SetPropertyBlock(PropertyBlock);
        }

        public void UpdateProperty(UnityAction<MaterialPropertyBlock> updater, bool applyProperty = false)
        {
            updater?.Invoke(PropertyBlock);

            if (applyProperty)
            {
                ApplyProperty();
            }
        }

        public void UpdateColor(string nameId, Color color, bool applyProperty = false)
        {
            UpdateProperty(m => m.SetColor(nameId, color), applyProperty);
        }

        public void UpdateFloat(string nameId, float f, bool applyProperty = false)
        {
            UpdateProperty(m => m.SetFloat(nameId, f), applyProperty);
        }

        public void UpdateVector(string nameId, Vector4 vec, bool applyProperty = false)
        {
            UpdateProperty(m => m.SetVector(nameId, vec), applyProperty);
        }

        public void UpdateBool(string nameId, bool b, bool applyProperty = false)
        {
            UpdateProperty(m => m.SetBool(nameId, b), applyProperty);
        }

        public void UpdateInt(string nameId, int i, bool applyProperty = false)
        {
            UpdateProperty(m => m.SetInt(nameId, i), applyProperty);
        }


        protected virtual void Awake()
        {
            Renderer = GetComponent<Renderer>();

            PropertyBlock = new();
            Renderer.GetPropertyBlock(PropertyBlock);
        }
    }
}