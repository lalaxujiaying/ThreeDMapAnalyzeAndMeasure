using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
namespace TDAAM.Analysis
{

    public class VisibleRangeAnalysis : MonoBehaviour
    {
        /// <summary>
        /// 线的颜色
        /// </summary>
        private enum LineColor
        {
            Red,
            Green
        }
        [SerializeField]
        /// <summary>
        /// 画一条线的时间
        /// </summary>
        private float drawLineAnimTime = 0.005f;
        /// <summary>
        /// 检测的圆半径
        /// </summary>
        [SerializeField]
        [Range(1, 30)]
        private float radius = 10f;
        [SerializeField]
        /// <summary>
        /// 检测中心点
        /// </summary>
        private Vector3 originPoint = Vector3.zero;
        /// <summary>
        /// 中心点的高
        /// </summary>
        //[Range(10f,100f)]
        public float hight = 100f;

        public float lineSizeMax = 0.1f;
        public float lineSizeMin = 0.0005f;

        /// <summary>
        /// 一条线有多少个检测点
        /// </summary>
        private float OneLineWithPoint = 100;
        /// <summary>
        /// 一个圆有多少条线
        /// </summary>
        public int allLintCount = 360;
        /// <summary>
        /// 画线的shader
        /// </summary>
        //[SerializeField]
        public Material material;
        /// <summary>
        /// 当前画线的目标点(用来做画线动画)
        /// </summary>
        private List<Vector4> dstPointArray = new List<Vector4>();
        /// <summary>
        /// 当前画线的起始点(用来做画线动画)
        /// </summary>
        private List<Vector4> originPointArray = new List<Vector4>();
        /// <summary>
        /// 画线目标点的最终结果
        /// </summary>
        [SerializeField]
        private List<Vector4> tempDstPointArray = new List<Vector4>();
        /// <summary>
        /// 画线起始点的最终结果
        /// </summary>
        [SerializeField]
        private List<Vector4> tempOriginPointArray = new List<Vector4>();
        private List<Vector4> staticDstPointArray = new List<Vector4>();
        private List<Vector4> staticOriginPointArray = new List<Vector4>();
        /// <summary>
        /// 传给shader使用（k,b,颜色,横竖还是斜着）
        /// </summary>
        [SerializeField]
        private Vector4[] verticalAndHorizontalAndKB = new Vector4[4096];
        /// <summary>
        /// 传给shader使用(maxX,minX,maxZ,minZ)(用来限制线的区间，画出线的端点)
        /// </summary>
        private Vector4[] maxAndmin = new Vector4[4096];
        [SerializeField]
        private float[] areaK;
        /// <summary>
        /// 存储所有划分区域的结束位置
        /// </summary>
        [SerializeField]
        private float[] areaLastCount;
        /// <summary>
        /// 存储所有划分区域的开始位置
        /// </summary>
        [SerializeField]
        private float[] areaFisrtCount;
        [SerializeField]
        private bool isPlayAnim = true;
        /// <summary>
        /// 画线的长度
        /// </summary>
        [SerializeField]
        [Range(0, 0.05f)]
        public float lineSize = 0.02f;
        /// <summary>
        /// 射线检测最高位置
        /// </summary>
        public float rayMaxHight = 200;
        /// <summary>
        /// 可以被射线检测的物体
        /// </summary>
        public List<Renderer> canRayObjects;
        private float LineSize
        {
            get
            {
                return lineSize;
            }
            set
            {
                if (lineSize != value)
                {
                    OnChangeLineSize?.Invoke(value);
                    lineSize = value;

                }
            }
        }
        /// <summary>
        /// 当改变线长时触发
        /// </summary>
        private Action<float> OnChangeLineSize;
        /// <summary>
        /// 控制是否循环传输数据到shader
        /// </summary>
        private bool isStartDrawLine = false;
        /// <summary>
        /// 一共画多少条线
        /// </summary>
        private int _lineCount = 0;
        /// <summary>
        /// 控制shader执行相应代码段
        /// </summary>
        private string shaderMacro = "Texture_On";
        /// <summary>
        /// 每个划分的区域大小
        /// </summary>
        private float eachAreaAngle;
        /// <summary>
        /// 设置划分的区域大小，经过测试划分区域在160-240之间性能最优
        /// </summary>
        private int areaCount;
        //staticLine_On
        //Texture_On
        public event Action OnAnimFinish;
        public event Action<float> OnCenterPointConfirmed;
        /// <summary>
        /// 什么都没有动，点击一下，点击第二下，动画结束
        /// </summary>
        private enum State
        {
            NotStart,
            ConfirmCenterPoint,
            ConfirmRadiusAndStart,
            FinishStartAnim
        }
        private State currentState;
        private LineRenderer lineRenderer;
        /// <summary>
        /// 物体是否都可以被射线检测
        /// </summary>
        public bool isAllRender = false;
        ComputeBuffer verticalAndHorizontalAndKBBuffer;
        ComputeBuffer maxAndMinBuffer;

        public void Init()
        {
            //areaCount = 360;
            //绘制中心线
            lineRenderer = GetComponent<LineRenderer>();
            Material lineMat = new Material(Shader.Find("describeLineShader"));
            lineMat.color = Color.blue + Color.green;
            lineRenderer.material = lineMat;

            currentState = State.NotStart;

            OnChangeLineSize += (size) =>
            {
                material.SetFloat("_LineSize", size);
            };

            material.SetFloat("_LineSize", lineSize);

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                ResetFunc();
                originPoint = hit.point + new Vector3(0, hight, 0);
                currentState = State.ConfirmCenterPoint;
                material.SetVector("_centerPoint", new Vector4(originPoint.x, 0, originPoint.z, 0));
                material.EnableKeyword("drawCircle_On");
                material.EnableKeyword("drawMouseLine_On");

                //绘制中心线
                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = 0.1f;
                lineRenderer.SetPosition(0, hit.point);
                lineRenderer.SetPosition(1, originPoint);
            }

        }
        void Update()
        {
            if (Input.GetMouseButtonDown(0) && currentState == State.ConfirmCenterPoint)
            {
                OnCenterPointConfirmed?.Invoke(radius);
                material.DisableKeyword("drawMouseLine_On");
                currentState = State.ConfirmRadiusAndStart;
                //VisibleRangeAnalysisStart();
                StartCoroutine(VisibleRangeAnalysisStart_Coroutine(ShaderStart));

            }
            if (currentState == State.ConfirmCenterPoint)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    radius = Vector2.Distance(new Vector2(originPoint.x, originPoint.z), new Vector2(hit.point.x, hit.point.z));

                    material.SetFloat("_radius", radius);
                    material.SetVector("_mouseWorldPoint", hit.point);
                }
            }

            ShaderUpdate();
            lodToLineSize();
        }

        public void ResetFunc()
        {
            DOTween.Kill("anim");
            areaLastCount = new float[areaCount];
            areaFisrtCount = new float[areaCount];
            dstPointArray.Clear();
            originPointArray.Clear();
            tempDstPointArray.Clear();
            tempOriginPointArray.Clear();
            staticOriginPointArray.Clear();
            staticDstPointArray.Clear();
            linePoints.Clear();
            verticalAndHorizontalAndKB = new Vector4[4096];
            maxAndmin = new Vector4[4096];
            _lineCount = 0;
            dotweenIndex = 0;
            isStartDrawLine = false;
            //material.DisableKeyword("for_On");
            material.DisableKeyword(shaderMacro);
            material.DisableKeyword("drawCircle_On");
            material.DisableKeyword("drawMouseLine_On");
            material.DisableKeyword("extraCircleLine_On");
            currentState = State.NotStart;

        }
        private void lodToLineSize()
        {
            LineSize = Vector3.Distance(Camera.main.transform.position, originPoint - new Vector3(0, hight, 0)) / 1500f;
            LineSize = Mathf.Clamp(LineSize, lineSizeMin, lineSizeMax);
        }
        private void ShaderStart()
        {
            //超过数量减少检测线重新开始
            if (tempOriginPointArray.Count > 4096)
            {
                //Debug.Log("画线数量超出4096 : " + tempOriginPointArray.Count);
                var tempOriginPoint = originPoint;
                //var tempAllLintCount = allLintCount;
                ResetFunc();
                allLintCount -= 40;
                material.SetVector("_centerPoint", new Vector4(tempOriginPoint.x, 0, tempOriginPoint.z, 0));
                material.EnableKeyword("drawCircle_On");

                //VisibleRangeAnalysisStart();
                //ShaderStart();
                StartCoroutine(VisibleRangeAnalysisStart_Coroutine(ShaderStart));
                return;
            }
            Debug.Log("动态一共画线数量为: " + allLintCount);
            isStartDrawLine = true;
            //将需要画的点传输给shader

            if (isPlayAnim)
            {
                originPointArray = tempOriginPointArray;
                for (int i = 0; i < tempOriginPointArray.Count; i++)
                {
                    dstPointArray.Add(new Vector4(tempOriginPointArray[i].x, tempOriginPointArray[i].y
                        , tempOriginPointArray[i].z, tempDstPointArray[i].w));
                }
                //根据距离设置时间
                var dis = Vector4.Distance(tempDstPointArray[dotweenIndex], tempOriginPointArray[dotweenIndex]);
                var currentPointAnimTime = dis * drawLineAnimTime / radius;
                DOTween.To(() => dstPointArray[dotweenIndex], x => dstPointArray[dotweenIndex] = x, tempDstPointArray[dotweenIndex], currentPointAnimTime).OnComplete(DotweenFunc).SetEase(Ease.Linear).SetId("anim");
                _lineCount++;
            }
            else
            {
                originPointArray = tempOriginPointArray;
                dstPointArray = tempDstPointArray;
            }
            areaCount = 360;
            //areaCount = allLintCount;
            eachAreaAngle = 90f / (allLintCount / 4);
            areaLastCount = new float[areaCount];
            areaFisrtCount = new float[areaCount];
            areaK = new float[areaCount / 4];
            //SetArea();
            //SetArea_New();
            DivideArea(allLintCount);
            material.SetInt("areaCount", allLintCount);
            material.SetFloatArray("areaK", areaK);
            material.SetFloatArray("areaLastCount", areaLastCount);
            material.SetFloatArray("areaFirstCount", areaFisrtCount);

            //material.SetVectorArray("verticalAndHorizontalAndKB", verticalAndHorizontalAndKB);
            //todo changebuffer

            verticalAndHorizontalAndKBBuffer = new ComputeBuffer(4096, sizeof(float) * 4);
            verticalAndHorizontalAndKBBuffer.SetData(verticalAndHorizontalAndKB);
            material.SetBuffer("verticalAndHorizontalAndKB", verticalAndHorizontalAndKBBuffer);

            material.SetFloat("_LineSize", lineSize);
            material.SetFloat("_radius", radius);
            //material.EnableKeyword("for_On");
            material.EnableKeyword(shaderMacro);
        }
        private void DivideArea(int areaCount)
        {
            int areaFisrtCountIndex = 0;
            verticalAndHorizontalAndKB = new Vector4[4096];
            for (int i = 0; i < originPointArray.Count; i++)
            {
                var offsetX = Mathf.Abs(tempDstPointArray[i].x - originPointArray[i].x);
                var offsetZ = Mathf.Abs(tempDstPointArray[i].z - originPointArray[i].z);

                var lineVertical = offsetX < 0.001f && offsetX > -0.001f ? true : false;
                var lineHorizontal = offsetZ < 0.001f && offsetZ > -0.001f ? true : false;
                int w = 0;
                float k = 0;
                float b = 0;
                if (lineHorizontal || lineVertical)
                {
                    if (lineHorizontal)
                    {
                        if (tempDstPointArray[i].x - originPointArray[i].x >= 0)
                        {
                            areaLastCount[areaCount - 1] = i;
                            if (areaFisrtCountIndex == 0)
                            {
                                areaFisrtCount[areaFisrtCountIndex] = 0;
                                areaFisrtCountIndex++;
                            }
                        }
                        else
                        {
                            var currentIndex = areaCount / 2 - 1;

                            areaLastCount[currentIndex] = i;
                            if (areaFisrtCountIndex <= currentIndex + 1)
                            {
                                areaFisrtCount[currentIndex + 1] = i;
                                areaFisrtCountIndex = currentIndex + 2;
                            }
                        }
                        w = 1;
                    }
                    else if (lineVertical)
                    {

                        if (tempDstPointArray[i].z - originPointArray[i].z >= 0)
                        {
                            var currentIndex = areaCount / 4 - 1;

                            areaLastCount[currentIndex] = i;
                            if (areaFisrtCountIndex <= currentIndex + 1)
                            {
                                areaFisrtCount[currentIndex + 1] = i;
                                areaFisrtCountIndex = currentIndex + 2;
                            }

                        }
                        else
                        {
                            var currentIndex = areaCount * 3 / 4 - 1;

                            areaLastCount[currentIndex] = i;
                            if (areaFisrtCountIndex <= currentIndex + 1)
                            {
                                areaFisrtCount[currentIndex + 1] = i;
                                areaFisrtCountIndex = currentIndex + 2;
                            }
                        }
                        w = 0;
                    }
                    k = 1;
                    b = 1;
                }
                else
                {
                    w = 2;
                    k = (tempDstPointArray[i].z - originPointArray[i].z) / (tempDstPointArray[i].x - originPointArray[i].x);
                    b = originPointArray[i].z - k * originPointArray[i].x;
                    if (tempDstPointArray[i].z - originPointArray[i].z >= 0)
                    {
                        for (int areaCountIndex = 0; areaCountIndex < areaCount / 4 - 1; areaCountIndex++)
                        {
                            if (k >= Mathf.Tan(areaCountIndex * eachAreaAngle * Mathf.Deg2Rad) - 0.001f && k <= Mathf.Tan((areaCountIndex + 1) * eachAreaAngle * Mathf.Deg2Rad) + 0.001f)
                            {

                                areaLastCount[areaCountIndex] = i;
                                if (areaFisrtCountIndex <= areaCountIndex)
                                {
                                    areaFisrtCount[areaCountIndex] = i;
                                    areaFisrtCountIndex = areaCountIndex + 1;
                                }
                            }

                        }
                        if (k >= Mathf.Tan((areaCount / 4 - 1) * eachAreaAngle * Mathf.Deg2Rad) - 0.001f)
                        {
                            var currentIndex = areaCount / 4 - 1;

                            areaLastCount[currentIndex] = i;
                            if (areaFisrtCountIndex <= currentIndex)
                            {
                                areaFisrtCount[currentIndex] = i;
                                areaFisrtCountIndex = currentIndex + 1;
                            }
                        }
                        if (k <= Mathf.Tan((areaCount / 4 + 1) * eachAreaAngle * Mathf.Deg2Rad) + 0.001f)
                        {
                            var currentIndex = areaCount / 4;

                            areaLastCount[currentIndex] = i;
                            if (areaFisrtCountIndex <= currentIndex + 1)
                            {
                                areaFisrtCount[currentIndex + 1] = i;
                                areaFisrtCountIndex = currentIndex + 2;
                            }
                        }
                        for (int areaCountIndex = areaCount / 4 + 1; areaCountIndex < areaCount / 2 - 1; areaCountIndex++)
                        {
                            if (k >= Mathf.Tan(areaCountIndex * eachAreaAngle * Mathf.Deg2Rad) - 0.001f && k <= Mathf.Tan((areaCountIndex + 1) * eachAreaAngle * Mathf.Deg2Rad) + 0.001f)
                            {

                                areaLastCount[areaCountIndex] = i;
                                if (areaFisrtCountIndex <= areaCountIndex + 1)
                                {
                                    areaFisrtCount[areaCountIndex + 1] = i;
                                    areaFisrtCountIndex = areaCountIndex + 2;
                                }
                            }


                        }
                    }
                    else
                    {
                        for (int areaCountIndex = areaCount / 2; areaCountIndex < areaCount * 3 / 4 - 1; areaCountIndex++)
                        {
                            if (k >= Mathf.Tan(areaCountIndex * eachAreaAngle * Mathf.Deg2Rad) - 0.001f && k <= Mathf.Tan((areaCountIndex + 1) * eachAreaAngle * Mathf.Deg2Rad) + 0.001f)
                            {

                                areaLastCount[areaCountIndex] = i;
                                if (areaFisrtCountIndex <= areaCountIndex)
                                {
                                    areaFisrtCount[areaCountIndex] = i;
                                    areaFisrtCountIndex = areaCountIndex + 1;
                                }
                            }

                        }
                        if (k >= Mathf.Tan((areaCount * 3 / 4 - 1) * eachAreaAngle * Mathf.Deg2Rad) - 0.001f)
                        {
                            var currentIndex = areaCount * 3 / 4 - 1;

                            areaLastCount[currentIndex] = i;
                            if (areaFisrtCountIndex <= currentIndex)
                            {
                                areaFisrtCount[currentIndex] = i;
                                areaFisrtCountIndex = currentIndex + 1;
                            }
                        }
                        if (k <= Mathf.Tan((areaCount * 3 / 4 + 1) * eachAreaAngle * Mathf.Deg2Rad) + 0.001f)
                        {
                            var currentIndex = areaCount * 3 / 4;

                            areaLastCount[currentIndex] = i;
                            if (areaFisrtCountIndex <= currentIndex + 1)
                            {
                                areaFisrtCount[currentIndex + 1] = i;
                                areaFisrtCountIndex = currentIndex + 2;
                            }
                        }
                        for (int areaCountIndex = areaCount * 3 / 4 + 1; areaCountIndex < areaCount - 1; areaCountIndex++)
                        {
                            if (k >= Mathf.Tan(areaCountIndex * eachAreaAngle * Mathf.Deg2Rad) - 0.001f && k <= Mathf.Tan((areaCountIndex + 1) * eachAreaAngle * Mathf.Deg2Rad) + 0.001f)
                            {
                                areaLastCount[areaCountIndex] = i;
                                if (areaFisrtCountIndex <= areaCountIndex + 1)
                                {
                                    areaFisrtCount[areaCountIndex + 1] = i;
                                    areaFisrtCountIndex = areaCountIndex + 2;
                                }
                            }

                        }

                    }
                }
                b = b < 0.001f && b > -0.001f ? 0 : b;
                verticalAndHorizontalAndKB[i] = new Vector4(k, b, tempDstPointArray[i].w, w);

            }
            for (int i = 0; i < areaCount / 4; i++)
            {
                areaK[i] = Mathf.Tan(i * eachAreaAngle * Mathf.Deg2Rad);
            }
        }

        private void ShaderUpdate()
        {
            if (isStartDrawLine && isPlayAnim)
            {
                maxAndmin = new Vector4[4096];
                for (int i = 0; i < originPointArray.Count; i++)
                {
                    var maxX = Mathf.Max(dstPointArray[i].x, originPointArray[i].x);
                    var minX = Mathf.Min(dstPointArray[i].x, originPointArray[i].x);
                    var maxZ = Mathf.Max(dstPointArray[i].z, originPointArray[i].z);
                    var minZ = Mathf.Min(dstPointArray[i].z, originPointArray[i].z);
                    maxX = maxX < 0.001f && maxX > -0.001f ? 0 : maxX;
                    minX = minX < 0.001f && minX > -0.001f ? 0 : minX;
                    maxZ = maxZ < 0.001f && maxZ > -0.001f ? 0 : maxZ;
                    minZ = minZ < 0.001f && minZ > -0.001f ? 0 : minZ;
                    maxAndmin[i] = new Vector4(maxX, minX, maxZ, minZ);

                }
                //todo changebuffer
                if (maxAndMinBuffer != null) maxAndMinBuffer.Dispose();
                maxAndMinBuffer = new ComputeBuffer(4096, sizeof(float) * 4);
                maxAndMinBuffer.SetData(maxAndmin);
                material.SetBuffer("maxAndmin", maxAndMinBuffer);

                //material.SetVectorArray("maxAndmin", maxAndmin);
                //material.SetInt("_lineCount", _lineCount);
                if (_lineCount == tempOriginPointArray.Count + 1)
                {
                    ShaderComplete();
                }

            }
            else if (isStartDrawLine && !isPlayAnim)
            {
                maxAndmin = new Vector4[4096];
                for (int i = 0; i < tempOriginPointArray.Count; i++)
                {
                    var maxX = Mathf.Max(tempDstPointArray[i].x, tempOriginPointArray[i].x);
                    var minX = Mathf.Min(tempDstPointArray[i].x, tempOriginPointArray[i].x);
                    var maxZ = Mathf.Max(tempDstPointArray[i].z, tempOriginPointArray[i].z);
                    var minZ = Mathf.Min(tempDstPointArray[i].z, tempOriginPointArray[i].z);
                    maxX = maxX < 0.001f && maxX > -0.001f ? 0 : maxX;
                    minX = minX < 0.001f && minX > -0.001f ? 0 : minX;
                    maxZ = maxZ < 0.001f && maxZ > -0.001f ? 0 : maxZ;
                    minZ = minZ < 0.001f && minZ > -0.001f ? 0 : minZ;
                    maxAndmin[i] = new Vector4(maxX, minX, maxZ, minZ);

                }
                material.SetVectorArray("maxAndmin", maxAndmin);
                _lineCount = tempOriginPointArray.Count + 1;
                ShaderComplete();
            }


        }

        //静态时的最多总线
        int staticLineCount;
        /// <summary>
        /// shader动画结束后的状态
        /// </summary>
        private void ShaderComplete()
        {
            isStartDrawLine = false;
            currentState = State.FinishStartAnim;
            //禁用掉脚本
            //enabled = false;
            //return;
            dstPointArray.Clear();
            originPointArray.Clear();
            tempDstPointArray.Clear();
            tempOriginPointArray.Clear();
            staticOriginPointArray.Clear();
            staticDstPointArray.Clear();
            verticalAndHorizontalAndKB = new Vector4[4096];
            maxAndmin = new Vector4[4096];
            allLintCount = staticLineCount;
            //VisibleRangeAnalysisStart();
            StartCoroutine(VisibleRangeAnalysisStart_Coroutine(() =>
            {
                Debug.Log("静态调整到" + allLintCount);
                //下面是优化shader
                originPointArray = staticOriginPointArray;
                tempDstPointArray = staticDstPointArray;

                eachAreaAngle = 90f / (allLintCount / 4);
                DivideArea(allLintCount);
                for (int i = 0; i < originPointArray.Count; i++)
                {
                    var maxX = Mathf.Max(tempDstPointArray[i].x, originPointArray[i].x);
                    var minX = Mathf.Min(tempDstPointArray[i].x, originPointArray[i].x);
                    var maxZ = Mathf.Max(tempDstPointArray[i].z, originPointArray[i].z);
                    var minZ = Mathf.Min(tempDstPointArray[i].z, originPointArray[i].z);
                    maxX = maxX < 0.001f && maxX > -0.001f ? 0 : maxX;
                    minX = minX < 0.001f && minX > -0.001f ? 0 : minX;
                    maxZ = maxZ < 0.001f && maxZ > -0.001f ? 0 : maxZ;
                    minZ = minZ < 0.001f && minZ > -0.001f ? 0 : minZ;
                    maxAndmin[i] = new Vector4(maxX, minX, maxZ, minZ);

                }
                material.SetInt("areaCount", areaCount);
                material.SetFloatArray("areaK", areaK);
                material.SetFloatArray("areaLastCount", areaLastCount);
                material.SetFloatArray("areaFirstCount", areaFisrtCount);
                //todo changebuffer

                if (maxAndMinBuffer != null) maxAndMinBuffer.Dispose();
                maxAndMinBuffer = new ComputeBuffer(tempDstPointArray.Count, sizeof(float) * 4);
                maxAndMinBuffer.SetData(maxAndmin, 0, 0, tempDstPointArray.Count);
                material.SetBuffer("maxAndmin", maxAndMinBuffer);

                if (verticalAndHorizontalAndKBBuffer != null) verticalAndHorizontalAndKBBuffer.Dispose();
                verticalAndHorizontalAndKBBuffer = new ComputeBuffer(tempDstPointArray.Count, sizeof(float) * 4);
                verticalAndHorizontalAndKBBuffer.SetData(verticalAndHorizontalAndKB, 0, 0, tempDstPointArray.Count);
                material.SetBuffer("verticalAndHorizontalAndKB", verticalAndHorizontalAndKBBuffer);

                //material.SetVectorArray("verticalAndHorizontalAndKB", verticalAndHorizontalAndKB);
                //material.SetVectorArray("maxAndmin", maxAndmin);
                //material.EnableKeyword("extraCircleLine_On");
            }));
            OnAnimFinish?.Invoke();
        }
        /// <summary>
        /// 添加需要画线的相关信息(shader会按照顺序画线)
        /// </summary>
        /// <param name="originPoint">起点</param>
        /// <param name="dstPoint">终点</param>
        /// <param name="lineColor">线的颜色</param>
        private void AddPointToDrawLine(Vector3 originPoint, Vector3 dstPoint, LineColor lineColor = LineColor.Red)
        {
            tempOriginPointArray.Add(new Vector4(originPoint.x, 0, originPoint.z, 0));
            tempDstPointArray.Add(new Vector4(dstPoint.x, 0, dstPoint.z, (int)lineColor));
            if (lineColor == LineColor.Green)
            {
                staticOriginPointArray.Add(new Vector4(originPoint.x, 0, originPoint.z, 0));
                staticDstPointArray.Add(new Vector4(dstPoint.x, 0, dstPoint.z, (int)lineColor));

            }
        }
        private void AddPointToDrawLine2(Vector3 dstPoint)
        {
            staticOriginPointArray.Add(new Vector4(originPoint.x, 0, originPoint.z, 0));
            staticDstPointArray.Add(new Vector4(dstPoint.x, 0, dstPoint.z, (int)LineColor.Red));
        }
        /* x = rcosθ */
        /* y = rsinθ */
        /// <summary>
        /// 存储检测到的所有点碰撞信息以及位置信息
        /// </summary>
        private List<Vector4> linePoints = new List<Vector4>();
        /// <summary>
        /// 第一次低于4096，获得最大staticOriginPointArray.count值
        /// </summary>
        bool isFristStaticTo4096;
        #region 这个会卡,后续改成使用协程
        private void VisibleRangeAnalysisStart()
        {
            float deg = 360 / (float)allLintCount;
            var distance = radius / OneLineWithPoint;
            int fisrtLineindex = 0;

            //for循环对所有需要检测的点进行射线碰撞检测,检测连通性
            for (int i = 0; i < allLintCount; i++)
            {
                linePoints.Clear();
                for (int j = 1; j < OneLineWithPoint + 1; j++)
                {
                    float x = distance * j * Mathf.Cos(i * deg * Mathf.Deg2Rad) + originPoint.x;
                    float z = distance * j * Mathf.Sin(i * deg * Mathf.Deg2Rad) + originPoint.z;
                    //先获取坐标,以圆的形式
                    var hits = Physics.RaycastAll(new Vector3(x, rayMaxHight, z), Vector3.down);
                    if (hits.Length > 0)
                    {
                        for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
                        {
                            if (!hits[hitIndex].collider.TryGetComponent(out Renderer renderer)) break;
                            if (canRayObjects.Contains(renderer))
                            {
                                var hit = hits[hitIndex];
                                //获得坐标后检测连通性
                                if (Physics.Linecast(originPoint, hit.point, out RaycastHit raycastHit))
                                {
                                    if (hit.point.x <= raycastHit.point.x + 0.001f && hit.point.x >= raycastHit.point.x - 0.001f)
                                    {
                                        if (hit.point.y <= raycastHit.point.y + 0.001f && hit.point.y >= raycastHit.point.y - 0.01f)
                                        {
                                            if (hit.point.z <= raycastHit.point.z + 0.001f && hit.point.z >= raycastHit.point.z - 0.001f)
                                            {
                                                linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Green));
                                            }
                                            else
                                            {
                                                linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Red));
                                            }
                                        }
                                        else
                                        {
                                            linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Red));
                                        }
                                        //Debug.Log("连通");
                                        //Debug.DrawLine(originPoint, hit.point, Color.green, 100);

                                    }
                                    else
                                    {
                                        //linePoints.Add(new Vector4(raycastHit.point.x, raycastHit.point.y, raycastHit.point.z, (int)LineColor.Green));
                                        linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Red));
                                        //Debug.Log("不连通");
                                        //Debug.DrawLine(originPoint, hit.point, Color.red, 100);
                                    }
                                }
                                else
                                {
                                    linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Green));
                                    //Debug.Log("连通");
                                    //Debug.DrawLine(originPoint, hit.point, Color.green, 100);

                                }
                                break;
                            }

                        }
                    }
                    else
                    {
                        break;
                        //Debug.Log("居然会检测不到");
                        //return;
                    }

                }

                //对所有检测点进行整合
                //需要画相同颜色的两条同一方向同一位置上的线，合并成一条
                Vector3 lastPoint = originPoint;
                for (int index = 0; index < linePoints.Count; index++)
                {

                    if (index != linePoints.Count - 1 && linePoints[index].w == linePoints[index + 1].w)
                    {
                        continue;
                    }
                    AddPointToDrawLine(lastPoint, linePoints[index], (LineColor)linePoints[index].w);
                    //Debug.DrawLine(lastPoint, linePoints[index], linePoints[index].w == 1 ?Color.green:Color.red, duration: 100f);
                    if (i == 0)
                    {
                        fisrtLineindex++;
                    }
                    lastPoint = linePoints[index];

                }
                float x2 = distance * OneLineWithPoint * Mathf.Cos(i * deg * Mathf.Deg2Rad) + originPoint.x;
                float z2 = distance * OneLineWithPoint * Mathf.Sin(i * deg * Mathf.Deg2Rad) + originPoint.z;
                AddPointToDrawLine2(new Vector3(x2, 0, z2));
            }
            for (int i = 0; i < fisrtLineindex; i++)
            {
                AddPointToDrawLine(tempOriginPointArray[i], tempDstPointArray[i], (LineColor)tempDstPointArray[i].w);
            }
            if (!isFristStaticTo4096 && staticOriginPointArray.Count <= 4096)
            {
                isFristStaticTo4096 = true;
                staticLineCount = allLintCount;
            }
        }
        #endregion
        IEnumerator VisibleRangeAnalysisStart_Coroutine(Action OnCoroutineCompeleted)
        {
            float deg = 360 / (float)allLintCount;
            var distance = radius / OneLineWithPoint;
            int fisrtLineindex = 0;

            //for循环对所有需要检测的点进行射线碰撞检测,检测连通性
            for (int i = 0; i < allLintCount; i++)
            {
                linePoints.Clear();
                for (int j = 1; j < OneLineWithPoint + 1; j++)
                {
                    float x = distance * j * Mathf.Cos(i * deg * Mathf.Deg2Rad) + originPoint.x;
                    float z = distance * j * Mathf.Sin(i * deg * Mathf.Deg2Rad) + originPoint.z;
                    //先获取坐标,以圆的形式
                    var hits = Physics.RaycastAll(new Vector3(x, rayMaxHight, z), Vector3.down);
                    if (hits.Length > 0)
                    {
                        for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
                        {
                            if (!hits[hitIndex].collider.TryGetComponent(out Renderer renderer)) break;
                            if (canRayObjects.Contains(renderer))
                            {
                                var hit = hits[hitIndex];
                                //获得坐标后检测连通性
                                if (Physics.Linecast(originPoint, hit.point, out RaycastHit raycastHit))
                                {
                                    if (hit.point.x <= raycastHit.point.x + 0.001f && hit.point.x >= raycastHit.point.x - 0.001f)
                                    {
                                        if (hit.point.y <= raycastHit.point.y + 0.001f && hit.point.y >= raycastHit.point.y - 0.01f)
                                        {
                                            if (hit.point.z <= raycastHit.point.z + 0.001f && hit.point.z >= raycastHit.point.z - 0.001f)
                                            {
                                                linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Green));
                                            }
                                            else
                                            {
                                                linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Red));
                                            }
                                        }
                                        else
                                        {
                                            linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Red));
                                        }
                                        //Debug.Log("连通");
                                        //Debug.DrawLine(originPoint, hit.point, Color.green, 100);

                                    }
                                    else
                                    {
                                        //linePoints.Add(new Vector4(raycastHit.point.x, raycastHit.point.y, raycastHit.point.z, (int)LineColor.Green));
                                        linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Red));
                                        //Debug.Log("不连通");
                                        //Debug.DrawLine(originPoint, hit.point, Color.red, 100);
                                    }
                                }
                                else
                                {
                                    linePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, (int)LineColor.Green));
                                    //Debug.Log("连通");
                                    //Debug.DrawLine(originPoint, hit.point, Color.green, 100);

                                }
                                break;
                            }

                        }
                    }
                    else
                    {
                        break;
                        //Debug.Log("居然会检测不到");
                        //return;
                    }
                }
                yield return null;
                //对所有检测点进行整合
                //需要画相同颜色的两条同一方向同一位置上的线，合并成一条
                Vector3 lastPoint = originPoint;
                for (int index = 0; index < linePoints.Count; index++)
                {

                    if (index != linePoints.Count - 1 && linePoints[index].w == linePoints[index + 1].w)
                    {
                        continue;
                    }
                    AddPointToDrawLine(lastPoint, linePoints[index], (LineColor)linePoints[index].w);
                    //Debug.DrawLine(lastPoint, linePoints[index], linePoints[index].w == 1 ?Color.green:Color.red, duration: 100f);
                    if (i == 0)
                    {
                        fisrtLineindex++;
                    }
                    lastPoint = linePoints[index];

                }
                float x2 = distance * OneLineWithPoint * Mathf.Cos(i * deg * Mathf.Deg2Rad) + originPoint.x;
                float z2 = distance * OneLineWithPoint * Mathf.Sin(i * deg * Mathf.Deg2Rad) + originPoint.z;
                AddPointToDrawLine2(new Vector3(x2, 0, z2));
                //todo 这里有一个BUG 最后一条默认红线有问题
            }


            for (int i = 0; i < fisrtLineindex; i++)
            {
                AddPointToDrawLine(tempOriginPointArray[i], tempDstPointArray[i], (LineColor)tempDstPointArray[i].w);
            }
            if (!isFristStaticTo4096 && staticOriginPointArray.Count <= 4096)
            {
                isFristStaticTo4096 = true;
                staticLineCount = allLintCount;
            }
            yield return null;
            OnCoroutineCompeleted?.Invoke();
        }
        int dotweenIndex = 0;
        private void DotweenFunc()
        {
            dotweenIndex++;
            _lineCount++;
            if (dotweenIndex >= tempDstPointArray.Count) return;
            //根据距离设置时间
            var dis = Vector4.Distance(tempDstPointArray[dotweenIndex], tempOriginPointArray[dotweenIndex]);
            var currentPointAnimTime = dis * drawLineAnimTime / radius;
            DOTween.To(() => dstPointArray[dotweenIndex], x => dstPointArray[dotweenIndex] = x, tempDstPointArray[dotweenIndex], currentPointAnimTime).OnComplete(DotweenFunc).SetEase(Ease.Linear).SetId("anim");
        }
        void OnGUI()
        {
            if (currentState == State.ConfirmCenterPoint)
            {
                GUIStyle fontStyle = new GUIStyle();
                fontStyle.normal.textColor = Color.yellow;
                fontStyle.fontSize = 40;
                GUI.Label(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 40, 500, 500), "半径：" + radius.ToString("f2") + "米", fontStyle);
            }
        }
    }

}
