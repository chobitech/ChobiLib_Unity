using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{

    [RequireComponent(typeof(Renderer))]
    public class ChobiRendererAndMaterialProperty : MonoBehaviour
    {
        public Renderer SelfRenderer { get; private set; }

        public MaterialPropertyBlock PropertyBlock { get; private set; }

        public void ApplyToMaterial()
        {
            SelfRenderer.SetPropertyBlock(PropertyBlock);
        }

        public void WithApplyToMaterial(
            UnityAction<MaterialPropertyBlock> updater,
            bool updateMaterialProperty = false
        )
        {
            updater?.Invoke(PropertyBlock);
            if (updateMaterialProperty)
            {
                ApplyToMaterial();
            }
        }

        protected virtual void Awake()
        {
            PropertyBlock = new();
            SelfRenderer = GetComponent<Renderer>();
            SelfRenderer.SetPropertyBlock(PropertyBlock);
        }

    }
}