using UnityEngine;

public class ObraDinnCameraScript : MonoBehaviour
{

    public Material ditherMath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture main = RenderTexture.GetTemporary(820, 470);

        Graphics.Blit(source, main, ditherMath);
        Graphics.Blit(main, destination);

        RenderTexture.ReleaseTemporary(main);
    }
}
