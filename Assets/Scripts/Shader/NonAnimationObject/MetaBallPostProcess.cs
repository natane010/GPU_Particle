using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaBallPostProcess : MonoBehaviour
{
    Material m_material;
    Camera m_camera;
    RenderTexture m_renderTexture;
    public void Init(Camera camera, Material material)
    {
        m_camera = camera;
        m_material = material;
        m_renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        m_camera.enabled = false;
        m_camera.targetTexture = m_renderTexture;
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        m_camera.Render();
        Graphics.Blit(m_renderTexture, source, m_material);
        Graphics.Blit(source, destination);
    }
}
