using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TDAAMCamEffect : MonoBehaviour
{
    public static TDAAMCamEffect Instance;
    private Camera _cam;

    private List<Material> _mats = new List<Material>();
    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.depthTextureMode |= DepthTextureMode.Depth;
    }
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance);
        }
    }
    public void AddEffect(Material material)
    {
        _mats.Add(material);
    }
    public void RemoveEffect(Material material)
    {
        _mats.Remove(material);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var buffer0 = RenderTexture.GetTemporary(source.width, source.height, 24);
        Graphics.Blit(source, buffer0);
        foreach (var effectMat in _mats)
        {
            var buffer1 = RenderTexture.GetTemporary(source.width, source.height, 24);
            Graphics.Blit(buffer0, buffer1, effectMat);
            RenderTexture.ReleaseTemporary(buffer0);
            buffer0 = buffer1;
        }
        Graphics.Blit(buffer0, destination);
        RenderTexture.ReleaseTemporary(buffer0);
    }
}
