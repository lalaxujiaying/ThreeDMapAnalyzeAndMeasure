using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDAAM.Measure;
namespace TDAAM.Measure.Manager
{
    public class DistanceMeasure_surfaceManage : TDAAM_Mono<DistanceMeasure_surfaceManage>
    {
        #region Unity面板输入
        [SerializeField, Tooltip("端点UI")]
        private Texture2D pointT2d;
        /// <summary>
        /// 定义北方向
        /// </summary>
        [SerializeField, Tooltip("定义北方向")]
        private NorthDir northDir = NorthDir.Z_Axis;
        /// <summary>
        /// 画线大小
        /// </summary>
        [SerializeField, Range(0, 1f), Tooltip("画线大小")]
        private float lineSize = 0.2f;
        /// <summary>
        /// UI大小
        /// </summary>
        [SerializeField, Range(5, 50), Tooltip("UI大小")]
        private int showUISize = 30;
        /// <summary>
        /// 待确认线段的颜色
        /// </summary>
        [SerializeField, Tooltip("待确认线段的颜色")]
        private Color lineColor_Temp = Color.green;
        /// <summary>
        /// 确认线段的颜色
        /// </summary>
        [SerializeField, Tooltip("确认线段的颜色")]
        private Color lineColor_Confirm = Color.yellow;

        [SerializeField, Tooltip("待确认UI的颜色")]
        private Color uiColor_Temp = Color.red;

        [SerializeField, Tooltip("确认UI的颜色")]
        private Color uiColor_Confirm = Color.black;

        [SerializeField, Tooltip("是否显示UI")]
        private bool isShowUI = false;

        [SerializeField, Tooltip("所有可显示效果的renderer")]
        private List<Renderer> canShowEffectRenderers = new List<Renderer>();

        [SerializeField, Tooltip("画线mat的模板")]
        private Material distanceSurfaceMat;
        [SerializeField, Tooltip("可在所有Renderer上绘制")]
        private bool isAllRender = true;
        #endregion

        #region 内部字段
        public List<GameObject> childScirpts = new List<GameObject>();
        private Vector3 downClickPoint;
        private bool isCompleted = true;
        public bool isStop = false;
        private List<GameObject> canShowEffectGo = new List<GameObject>();
        private Material lineMat_instance;

        private ComputeBuffer originPointsBuffer;
        private ComputeBuffer dstPointsBuffer;
        private List<Vector4> originPoints = new List<Vector4>();
        private List<Vector4> dstPoints = new List<Vector4>();
        #endregion

        private void Start()
        {
            if (!isAllRender)
            {
                foreach (var renderer in canShowEffectRenderers)
                {
                    canShowEffectGo.Add(renderer.gameObject);
                }
            }
            canShowEffectRenderers = null;
            distanceSurfaceMat = new Material(Shader.Find("DistanceSurfaceShader"));
            lineMat_instance = Instantiate(distanceSurfaceMat);
            //originPointsBuffer = new ComputeBuffer(0,0);
            //dstPointsBuffer = new ComputeBuffer(0,0);
            //添加临时线段
            originPoints.Add(new Vector4(0, 0, 0, 0));
            dstPoints.Add(new Vector4(0, 0, 0, 0));
            TDAAMCamEffect.Instance.AddEffect(lineMat_instance);
        }
        void Update()
        {
            if (isStop) return;
            if (Input.GetMouseButtonDown(0))
            {
                downClickPoint = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0) && isCompleted)
            {
                if (!MathfEx.CheckVecter3CompFromShpere(downClickPoint, Input.mousePosition)) return;
                //if (downClickPoint != Input.mousePosition) return;
                isCompleted = false;
                CreateScript();
            }

        }
        //public void Reset()
        //{
        //    isCompleted = true;
        //    foreach (var childScirpt in childScirpts)
        //    {
        //        Destroy(childScirpt);
        //    }
        //    childScirpts.Clear();
        //}
        public void DeleteEffect()
        {

        }
        public DistanceMeasure_surface GetCurrentChild()
        {
            return childScirpts[childScirpts.Count - 1].GetComponent<DistanceMeasure_surface>();
        }
        private void CreateScript()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                if (isAllRender || canShowEffectGo.Contains(hit.collider.gameObject))
                {
                    GameObject child = new GameObject();
                    childScirpts.Add(child);
                    child.transform.parent = transform;
                    var script = child.AddComponent<DistanceMeasure_surface>();
                    script.OnMeasureCompleted += (isStop) =>
                    {
                        this.isStop = isStop;
                        isCompleted = true;
                    };
                    script.pointT2d = pointT2d;
                    script.northDir = northDir;
                    script.lineColor_Temp = lineColor_Temp;
                    script.lineColor_Confirm = lineColor_Confirm;
                    script.lineSize = lineSize;
                    script.isCreate = true;
                    script.isShowUI = isShowUI;
                    script.showUISize = showUISize;
                    script.uiColor_Temp = uiColor_Temp;
                    script.uiColor_Confirm = uiColor_Confirm;

                    script.Init();

                    script.OnChangedPointAction += ChangedPointToLineMat;
                    script.OnAddPointAction += AddPointToLineMat;
                    script.OnRemovePointAction += RemovePointToLineMat;
                }
            }
        }
        public void SetConfig(Texture2D t2d, NorthDir northDir,
            Color lineColor_Temp, Color lineColor_Confirm, float lineWidth, int uiSize, bool isShowUI, Color uiColor_Temp, Color uiColor_Confirm)
        {
            pointT2d = t2d;
            lineSize = lineWidth;
            showUISize = uiSize;
            this.isShowUI = isShowUI;
            this.northDir = northDir;
            this.lineColor_Temp = lineColor_Temp;
            this.lineColor_Confirm = lineColor_Confirm;
            this.uiColor_Confirm = uiColor_Confirm;
            this.uiColor_Temp = uiColor_Temp;
        }
        public void HideChild(int childIndex)
        {
            childScirpts[childIndex].SetActive(false);
            TDAAMCamEffect.Instance.RemoveEffect(lineMat_instance);
        }
        public void ShowChild(int childIndex)
        {
            childScirpts[childIndex].SetActive(true);
            TDAAMCamEffect.Instance.AddEffect(lineMat_instance);
        }
        public float AddPointToLineMat(Vector3 originPoint, Vector3 dstPoint, int colorIndex)
        {
            originPoints.Add(new Vector4(originPoint.x, originPoint.y, originPoint.z, colorIndex));
            dstPoints.Add(dstPoint);

            originPointsBuffer?.Dispose();
            originPointsBuffer = new ComputeBuffer(originPoints.Count, sizeof(float) * 4);
            originPointsBuffer.SetData(originPoints);
            lineMat_instance.SetBuffer("_OriginPoints", originPointsBuffer);

            dstPointsBuffer?.Dispose();
            dstPointsBuffer = new ComputeBuffer(dstPoints.Count, sizeof(float) * 4);
            dstPointsBuffer.SetData(dstPoints);
            lineMat_instance.SetBuffer("_DstPoints", dstPointsBuffer);

            lineMat_instance.SetInt("_LineCount", dstPoints.Count);
            originPointsBuffer.Dispose();
            dstPointsBuffer.Dispose();
            //计算表面长度
            return GetSurfaceLength(originPoint, dstPoint);

        }
        public void ChangedPointToLineMat(int pointIndex, Vector3 originPoint, Vector3 dstPoint, int colorIndex)
        {
            originPoints[pointIndex] = new Vector4(originPoint.x, originPoint.y, originPoint.z, colorIndex);
            dstPoints[pointIndex] = dstPoint;

            originPointsBuffer?.Dispose();
            originPointsBuffer = new ComputeBuffer(originPoints.Count, sizeof(float) * 4);
            originPointsBuffer.SetData(originPoints);
            lineMat_instance.SetBuffer("_OriginPoints", originPointsBuffer);

            dstPointsBuffer?.Dispose();
            dstPointsBuffer = new ComputeBuffer(dstPoints.Count, sizeof(float) * 4);
            dstPointsBuffer.SetData(dstPoints);
            lineMat_instance.SetBuffer("_DstPoints", dstPointsBuffer);

            lineMat_instance.SetInt("_LineCount", dstPoints.Count);
        }
        public void RemovePointToLineMat(int pointIndex)
        {
            originPoints.RemoveAt(pointIndex);
            dstPoints.RemoveAt(pointIndex);

            originPointsBuffer?.Dispose();
            originPointsBuffer = new ComputeBuffer(originPoints.Count, sizeof(float) * 4);
            originPointsBuffer.SetData(originPoints);
            lineMat_instance.SetBuffer("_OriginPoints", originPointsBuffer);

            dstPointsBuffer?.Dispose();
            dstPointsBuffer = new ComputeBuffer(dstPoints.Count, sizeof(float) * 4);
            dstPointsBuffer.SetData(dstPoints);
            lineMat_instance.SetBuffer("_DstPoints", dstPointsBuffer);

            lineMat_instance.SetInt("_LineCount", dstPoints.Count);
        }

        public float GetSurfaceLength(Vector3 originPoint, Vector3 dstPoint)
        {
            List<Vector3> pos_a = new List<Vector3>();
            List<Vector3> pos_b = new List<Vector3>();
            originPoint = new Vector3(originPoint.x, 0, originPoint.z);
            dstPoint = new Vector3(dstPoint.x, 0, dstPoint.z);

            Vector3 tempCenter = (originPoint + dstPoint) / 2f;
            Vector3 centerPoint = new Vector3(tempCenter.x, 1000f, tempCenter.z);
            Vector3 dir = new Vector3(dstPoint.x - originPoint.x, 0, dstPoint.z - originPoint.z);
            float distance = Vector3.SqrMagnitude(dir);
            Vector3 size = new Vector3(0, 0.1f, distance);
            var hits = Physics.BoxCastAll(centerPoint, size / 2, Vector3.down, Quaternion.LookRotation(dir));

            foreach (var hit in hits)
            {
                if (isAllRender || canShowEffectGo.Contains(hit.collider.gameObject))
                {
                    var mesh = hit.collider.GetComponent<MeshFilter>().sharedMesh;
                    //Vector3 inNormal = Vector3.Cross(new Vector3(originPoint.x - dstPoint.x, 0, originPoint.z - dstPoint.z), Vector3.up);
                    //Plane plane = new Plane(inNormal, new Vector3(originPoint.x, 0, originPoint.z));
                    //mesh.GetCutMeshPointByPlane(hit.collider.transform, plane, ref pos_a, ref pos_b);
                    mesh.GetCutMeshPointByLine(hit.collider.transform, originPoint, dstPoint, ref pos_a, ref pos_b);
                }
            }
            float surfaceLength = 0;
            for (int i = 0; i < pos_a.Count; i++)
            {
                surfaceLength += Vector3.Distance(pos_a[i], pos_b[i]);
            }
            return surfaceLength;
        }
        //private void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{
        //    Graphics.Blit(source, destination, lineMat_instance);
        //}
    }
}