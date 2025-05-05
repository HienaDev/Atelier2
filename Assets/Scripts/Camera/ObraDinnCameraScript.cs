using UnityEngine;

public class ObraDinnCameraScript : MonoBehaviour
{
    public Material ditherMath;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture main = RenderTexture.GetTemporary(820, 470);

        Graphics.Blit(source, main, ditherMath);
        Graphics.Blit(main, destination);

        RenderTexture.ReleaseTemporary(main);
    }
}