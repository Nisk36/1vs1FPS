using UnityEngine;

[ExecuteInEditMode]
public class ToonShader : MonoBehaviour
{

    [SerializeField] 
    private Material _material = null;

    private void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        if (_material == null) return;
        Graphics.Blit(source, dest, _material);
    }
}