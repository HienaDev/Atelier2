using UnityEngine;

public class MultiShaderEffect : MonoBehaviour
{
    public Camera targetCamera; // The camera you want the effect on
    public Material effectMaterial1; // First shader effect
    public Material effectMaterial2; // Second shader effect

    private RenderTexture tempRenderTexture1;
    private RenderTexture tempRenderTexture2;

    void Start()
    {
        // Create temporary render textures matching the screen size
        tempRenderTexture1 = new RenderTexture(Screen.width, Screen.height, 16);
        tempRenderTexture2 = new RenderTexture(Screen.width, Screen.height, 16);

        // Set the target camera to render into the first texture
        if (targetCamera != null)
        {
            targetCamera.targetTexture = tempRenderTexture1;
        }
        else
        {
            Debug.LogError("Target Camera is not assigned!");
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (effectMaterial1 != null && effectMaterial2 != null)
        {
            // Apply first shader effect
            Graphics.Blit(tempRenderTexture1, tempRenderTexture2, effectMaterial1);

            // Apply second shader effect
            Graphics.Blit(tempRenderTexture2, dest, effectMaterial2);
        }
        else
        {
            Debug.LogError("Shader Materials are missing!");
            Graphics.Blit(tempRenderTexture1, dest); // Fallback: Just display the original render
        }
    }

    void OnDestroy()
    {
        if (targetCamera != null)
        {
            targetCamera.targetTexture = null;
        }

        if (tempRenderTexture1 != null)
        {
            tempRenderTexture1.Release();
            Destroy(tempRenderTexture1);
        }

        if (tempRenderTexture2 != null)
        {
            tempRenderTexture2.Release();
            Destroy(tempRenderTexture2);
        }
    }
}
