using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using TDAAM.Analysis;
namespace TDAAM.Analysis.Manager
{
    public class VisibleRangeManager : TDAAM_Mono<VisibleRangeManager>
    {
        [SerializeField]
        private float rayMaxHight = 200;
        /// <summary>
        /// 所有的可视分析脚本
        /// </summary>
        private List<VisibleRangeAnalysis> analysisScripts = new List<VisibleRangeAnalysis>();
        /// <summary>
        /// 当前分析是否完成
        /// </summary>
        private bool isAnalysisFinish = true;
        /// <summary>
        /// 当前分析中心点的高
        /// </summary>
        [SerializeField]
        private float high = 10f;
        /// <summary>
        /// 当前分析总线段
        /// </summary>
        [SerializeField]
        private int allLintCount = 360;
        /// <summary>
        /// 当前分析每条线宽度
        /// </summary>
        [SerializeField]
        [Range(0.01f, 0.1f)]
        private float lineSize = 0.02f;


        [SerializeField]
        private float lineSizeMax = 0.1f;

        [SerializeField]
        private float lineSizeMin = 0.0005f;

        private Camera cam;
        /// <summary>
        /// 所有可执行可视分析的物体
        /// </summary>
        [SerializeField]
        private List<Renderer> visibleObjects_renderer = new List<Renderer>();

        //private List<GameObject> visibleObjects = new List<GameObject>();
        /// <summary>
        /// 将模板对象绘制成白色Mask的mat
        /// </summary>
        private Material drawWhiteObjectMat;
        /// <summary>
        /// 屏幕特效mat模板（没有数据）
        /// </summary>
        [SerializeField]
        private Material visibleTemplateMat;
        /// <summary>
        /// 所有需要显示的屏幕特效mat(拥有数据的)
        /// </summary>
        public List<Material> visibleEffectMats = new List<Material>();
        /// <summary>
        /// 每一块可视区域的存储，用来生成每一块区域的mask
        /// </summary>
        private List<Renderer> visibleAreas = new List<Renderer>();
        /// <summary>
        /// 使用最小覆盖绘制区域的对象，绘制出来的白色MaskRT
        /// </summary>
        private RenderTexture allObjectMaskRT;

        public bool isAllRender = false;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            drawWhiteObjectMat = new Material(Shader.Find("ScreenShader/DrawWhiteObject"));
            //visibleTemplateMat = new Material(Shader.Find("ScreenShader/VisibleRangeScreenShader"));
            cam.depthTextureMode |= DepthTextureMode.Depth;
            allObjectMaskRT = new RenderTexture(Screen.width, Screen.height, 24);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && isAnalysisFinish)
            {
                CreateVisibleRange();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isAnalysisFinish = true;

                for (int i = 0; i < analysisScripts.Count; i++)
                {
                    Destroy(visibleEffectMats[i]);
                    Destroy(analysisScripts[i].gameObject);
                }
                analysisScripts.Clear();
                visibleEffectMats.Clear();
                visibleAreas.Clear();
            }
        }
        private void CreateVisibleRange()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                if (isAllRender || visibleObjects_renderer.Contains(hit.collider.GetComponent<Renderer>()))
                {
                    isAnalysisFinish = false;
                    //创建分析脚本
                    GameObject go = new GameObject("visibleRange", typeof(MeshRenderer), typeof(LineRenderer));
                    go.transform.parent = transform;
                    go.transform.localScale = Vector3.one;
                    go.transform.localPosition = Vector3.zero;
                    var analysisScript = go.AddComponent<VisibleRangeAnalysis>();
                    analysisScripts.Add(analysisScript);
                    var visibleMat = Instantiate(visibleTemplateMat);
                    visibleEffectMats.Add(visibleMat);
                    //赋值数据
                    analysisScript.material = visibleMat;
                    analysisScript.hight = high;
                    analysisScript.allLintCount = allLintCount;
                    analysisScript.lineSize = lineSize;
                    analysisScript.canRayObjects = visibleObjects_renderer;
                    analysisScript.lineSizeMax = lineSizeMax;
                    analysisScript.lineSizeMin = lineSizeMin;
                    analysisScript.isAllRender = isAllRender;

                    analysisScript.OnCenterPointConfirmed += (radius) =>
                    {
                        var hits = Physics.SphereCastAll(new Vector3(hit.point.x, rayMaxHight, hit.point.z), radius, Vector3.down);
                        visibleAreas.Clear();
                        foreach (var hit in hits)
                        {
                            if (!hit.collider.TryGetComponent(out Renderer renderer)) break;
                            if (isAllRender || visibleObjects_renderer.Contains(renderer))
                            {
                                if (!visibleAreas.Contains(renderer))
                                {
                                    visibleAreas.Add(renderer);
                                }
                            }
                        }
                    };
                    analysisScript.OnAnimFinish += () =>
                    {
                        isAnalysisFinish = true;
                    };
                    //执行脚本
                    analysisScript.Init();
                }
            }
        }

        CommandBuffer commandBuffer = null;

        public VisibleRangeManager()
        {
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "ObjectStencilCB";
            commandBuffer.SetRenderTarget(allObjectMaskRT.colorBuffer, source.depthBuffer);
            commandBuffer.ClearRenderTarget(false, true, Color.clear);

            if (isAllRender) commandBuffer.ClearRenderTarget(false, true, Color.white);
            else if (!isAnalysisFinish) visibleObjects_renderer?.ForEach(x => commandBuffer.DrawRenderer(x, drawWhiteObjectMat));
            else visibleAreas?.ForEach(x => commandBuffer.DrawRenderer(x, drawWhiteObjectMat));

            RenderTexture buffer0 = RenderTexture.GetTemporary(source.width, source.height, 24);
            RenderTexture buffer1;

            commandBuffer.Blit(source, buffer0);

            foreach (var mat in visibleEffectMats)
            {
                buffer1 = RenderTexture.GetTemporary(source.width, source.height, 24);
                mat.SetTexture("_StencilBufferToColor", allObjectMaskRT);
                commandBuffer.Blit(buffer0, buffer1, mat);
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }
            commandBuffer.Blit(buffer0, destination);
            RenderTexture.ReleaseTemporary(buffer0);
            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();

        }
    }
}
