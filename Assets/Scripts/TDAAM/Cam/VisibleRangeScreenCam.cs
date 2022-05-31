using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VisibleRangeScreenCam : MonoBehaviour
{
	public Material material;
	public Material currentMaterial;

	public Material objectStencilMat;

	private Camera cam;
	public RenderTexture objectStencilRT;

	public Renderer[] meshRenderers;
	public Material drawWhiteObjectMat;

	private void Awake()
	{
		cam = GetComponent<Camera>();
		drawWhiteObjectMat = new Material(Shader.Find("Test/DrawWhiteObject"));
	}
	private void OnEnable()
	{
		objectStencilRT = new RenderTexture(Screen.width,Screen.height,24);
		currentMaterial = Instantiate(material);
		cam.depthTextureMode |= DepthTextureMode.Depth;
		currentMaterial.SetTexture("_StencilBufferToColor", objectStencilRT);
	}
	CommandBuffer commandBuffer = null;
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		commandBuffer = new CommandBuffer();
		commandBuffer.name = "ObjectStencilCB";
		commandBuffer.SetRenderTarget(objectStencilRT);
		commandBuffer.ClearRenderTarget(true, true, Color.clear);
		commandBuffer.SetRenderTarget(objectStencilRT.colorBuffer, source.depthBuffer);
		if (meshRenderers != null)
		{
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				commandBuffer.DrawRenderer(meshRenderers[i], drawWhiteObjectMat);
			}
		}
		commandBuffer.Blit(source, destination, currentMaterial);
		Graphics.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Dispose();
	}

}
