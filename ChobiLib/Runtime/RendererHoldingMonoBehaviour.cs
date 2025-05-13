using ChobiLib.Unity;
using UnityEngine;
using UnityEngine.Events;

public class RendererHoldingMonoBehaviour : SelfObjectHoldingMonoBehaviour
{
    [SerializeField]
    private new Renderer renderer;
    public Renderer Renderer => renderer;

    private Material material;
    public Material Material => material;

    public MaterialPropertyBlock PropertyBlock { get; private set; }

    public void UpdateMaterialProperty(UnityAction<MaterialPropertyBlock> updater)
    {
        updater(PropertyBlock);
        renderer.SetPropertyBlock(PropertyBlock);
    }


    protected override void Awake()
    {
        base.Awake();

        PropertyBlock = new();

        if (renderer != null)
        {
            material = renderer.material;
            renderer.GetPropertyBlock(PropertyBlock);
        }
    }

}
