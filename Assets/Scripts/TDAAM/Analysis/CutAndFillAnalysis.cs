using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolygonTool;
using SplitMesh;
using System.Threading.Tasks;

namespace TDAAM.Analysis
{
    public struct CutAndFillData
    {
        public float baseLevelHight;
        public float minLevelHight;
        public float maxLevelHight;
        public float totalArea;
        public float cutArea;
        public float fillArea;
        public float cutVolume;
        public float fillVolume;
        public float totalVolume;
        public CutAndFillData(float baseLevelHight, float minLevelHight, float maxLevelHight, float totalArea, float cutArea, float fillArea, float cutVolume, float fillVolume, float totalVolume)
        {
            this.baseLevelHight = baseLevelHight;
            this.minLevelHight = minLevelHight;
            this.maxLevelHight = maxLevelHight;
            this.totalArea = totalArea;
            this.cutArea = cutArea;
            this.fillArea = fillArea;
            this.cutVolume = cutVolume;
            this.fillVolume = fillVolume;
            this.totalVolume = totalVolume;
        }
    }
    public class CutAndFillAnalysis : MonoBehaviour
    {
        [SerializeField]
        private List<Vector3> startPoints = new List<Vector3>();
        private Material combineMaterial;
        private Vector3 tempPoint;
        private bool isStart = false;
        private int pointCount = 0;
        /// <summary>
        /// 刚被创建出来
        /// </summary>
        private bool isCreated = true;
        private float doubleClickTime = 0.3f;
        private float doubleClickTimer = 0.3f;
        private bool isAnalysisStart = false;
        private bool isMouseClickCompleted = false;
        private bool isGetHightPointCompleted = false;
        private bool isStartGetHightPoint = false;
        public event Action OnAnalysisCompleted;
        /// <summary>
        /// 基准面高程
        /// </summary>
        private float baseLevelHight = 0;
        /// <summary>
        /// 最低点的高
        /// </summary>
        public float minLevelHight = 0;
        /// <summary>
        /// 最高点的高
        /// </summary>
        public float maxLevelHight = 0;
        /// <summary>
        /// 挖填方总面积
        /// </summary>
        public float totalArea = 0;
        /// <summary>
        /// 挖方面积
        /// </summary>
        public float topArea = 0;
        /// <summary>
        /// 填方面积
        /// </summary>
        public float bottomArea = 0;
        private float totalVolume = 0;
        private float cutVolume = 0;
        /// <summary>
        /// 挖方体积
        /// </summary>
        public float digVolume = 0;
        private float baseVolume = 0;
        /// <summary>
        /// 填方体积
        /// </summary>
        public float fillVolume = 0;
        private event Action<float> OnBaseLevelHightChanged;
        public event Action<CutAndFillData> OnCalcalateDataCompleted;

        public float BaseLevelHight
        {
            set
            {
                //baseLevelHight = Mathf.Max(value, minLevelHight);
                baseLevelHight = value;
                splitPlane = new Plane(Vector3.up, new Vector3(0, baseLevelHight, 0));
                OnBaseLevelHightChanged?.Invoke(baseLevelHight);
            }
            get
            {
                return baseLevelHight;
            }
        }
        public Texture icon;

        private Plane splitPlane;
        private List<SplitObject> splitObjects = new List<SplitObject>();
        private float setScaleWithBaseHight;
        public void Init()
        {
            combineMaterial = GetComponent<MeshRenderer>().material;
            OnBaseLevelHightChanged += (baseLevelHight) =>
            {
                setScaleWithBaseHight = Mathf.InverseLerp(minLevelHight, maxLevelHight, baseLevelHight);
                if (setScaleWithBaseHight == 0) setScaleWithBaseHight = -0.001f;
                if (setScaleWithBaseHight == 1)
                {
                    if (maxLevelHight != minLevelHight)
                        setScaleWithBaseHight = (baseLevelHight - minLevelHight) / (maxLevelHight - minLevelHight);

                }
                transform.GetChild(0).localScale = new Vector3(1, setScaleWithBaseHight, 1);

            };
        }

        List<MeshInfo> meshInfos = new List<MeshInfo>();
        List<List<Vector3>> cutPoints_vertical = new List<List<Vector3>>();
        void Update()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                BaseLevelHight -= 0.01f;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                BaseLevelHight += 0.01f;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitCalculateVolumeAndArea();
                CreateMeshToCalculateVolumeAndArea();
            }
            if (!isMouseClickCompleted)
            {
                Handle_MousePoint();
            }
            else if (isMouseClickCompleted && !isStartGetHightPoint)
            {
                isStartGetHightPoint = true;
                CreateBoundsCube(startPoints);
                FormeshVertices_func();
            }
            else if (isGetHightPointCompleted && !isAnalysisStart)
            {
                isAnalysisStart = true;
                Analysis_Start();
                CreateSpecialMesh(startPoints);
                OnAnalysisCompleted?.Invoke();
            }

        }
        bool isInitCalculate = false;
        private void InitCalculateVolumeAndArea()
        {
            if (isInitCalculate) return;
            isInitCalculate = true;
            for (int i = 0; i < kAndBs.Count; i++)
            {
                cutPoints_vertical.Add(new List<Vector3>());
            }
            for (int i = 0; i < splitObjects.Count; i++)
            {
                splitObjects[i].Split_Vertical(kAndBs, out MeshInfo inSideMesh, out MeshInfo outSideMesh, cutPoints_vertical);
                meshInfos.Add(inSideMesh);
            }
            //绘制顶部
            var combineMesh = new MeshInfo();
            mesh_top = combineMesh.CombineMesh(meshInfos.ToArray());
            mesh_top.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            GameObject split_top = new GameObject("split_top");
            split_top.AddComponent<MeshFilter>().mesh = mesh_top;
            //split_top.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            splitTop_splitObject = split_top.AddComponent<SplitObject>();
            //求区域总面积
            totalArea = mesh_top.GetArea();
        }
        Mesh mesh_top;
        SplitObject splitTop_splitObject;
        private void CreateMeshToCalculateVolumeAndArea()
        {
            int currentLineIndex = 0;
            topArea = 0;
            bottomArea = 0;
            //绘制侧面
            List<Mesh> mesh_edge = new List<Mesh>();
            List<Mesh> mesh_edge_cut = new List<Mesh>();
            List<Vector3> edgePoints_copy = new List<Vector3>(edgePoints);
            List<int> edgeTriangleIndex_copy = new List<int>(edgeTriangleIndex);
            List<Vector3> mesh_edge_cutline = new List<Vector3>();
            float bottomHight = baseLevelHight < minLevelHight ? baseLevelHight - 10 : minLevelHight - 10;
            foreach (var cutLine in cutPoints_vertical)
            {
                var cutLine_copy = new List<Vector3>(cutLine);
                mesh_edge.Add(new Mesh());
                mesh_edge_cut.Add(new Mesh());
                List<int> mesh_triangles = new List<int>();
                int cutLineCount = cutLine_copy.Count;
                cutLine_copy.Sort((x, y) =>
                {

                    if (Vector3.Magnitude(x - startLinePoints[currentLineIndex]) < Vector3.Magnitude(y - startLinePoints[currentLineIndex]))
                        return -1;
                    else if (Vector3.Magnitude(x - startLinePoints[currentLineIndex]) == Vector3.Magnitude(y - startLinePoints[currentLineIndex]))
                        return 0;
                    else
                        return 1;
                });
                mesh_edge_cutline = new List<Vector3>(cutLine_copy);
                for (int i = 0; i < mesh_edge_cutline.Count; i++)
                {
                    if (mesh_edge_cutline[i].y > baseLevelHight)
                    {
                        mesh_edge_cutline[i] = new Vector3(mesh_edge_cutline[i].x, baseLevelHight, mesh_edge_cutline[i].z);
                    }
                }
                for (int i = 0; i < cutLineCount - 1; i = i + 2)
                {
                    cutLine_copy.Add(new Vector3(cutLine_copy[i].x, bottomHight, cutLine_copy[i].z));
                    cutLine_copy.Add(new Vector3(cutLine_copy[i + 1].x, bottomHight, cutLine_copy[i + 1].z));
                    mesh_edge_cutline.Add(new Vector3(cutLine_copy[i].x, bottomHight, cutLine_copy[i].z));
                    mesh_edge_cutline.Add(new Vector3(cutLine_copy[i + 1].x, bottomHight, cutLine_copy[i + 1].z));

                    mesh_triangles.Add(i);
                    mesh_triangles.Add(cutLine_copy.Count - 1);
                    mesh_triangles.Add(cutLine_copy.Count - 2);

                    mesh_triangles.Add(i + 1);
                    mesh_triangles.Add(cutLine_copy.Count - 1);
                    mesh_triangles.Add(i);
                }
                mesh_edge[mesh_edge.Count - 1].vertices = cutLine_copy.ToArray();
                mesh_edge[mesh_edge.Count - 1].triangles = mesh_triangles.ToArray();

                mesh_edge_cut[mesh_edge_cut.Count - 1].vertices = mesh_edge_cutline.ToArray();
                mesh_edge_cut[mesh_edge_cut.Count - 1].triangles = mesh_triangles.ToArray();

                currentLineIndex++;
            }
            //绘制一个在basehight的切面，等下要用
            Mesh mesh_top_plane = new Mesh();
            for (int i = 0; i < edgePoints_copy.Count; i++)
            {
                edgePoints_copy[i] = new Vector3(edgePoints_copy[i].x, baseLevelHight, edgePoints_copy[i].z);
            }
            mesh_top_plane.vertices = edgePoints_copy.ToArray();
            mesh_top_plane.triangles = edgeTriangleIndex_copy.ToArray();
            //绘制底部
            Mesh mesh_bottom_plane = new Mesh();
            mesh_bottom_plane.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            for (int i = 0; i < edgePoints_copy.Count; i++)
            {
                edgePoints_copy[i] = new Vector3(edgePoints_copy[i].x, bottomHight, edgePoints_copy[i].z);
            }
            //将三角形反面
            int tempIndex;
            for (int i = 0; i < edgeTriangleIndex_copy.Count; i = i + 3)
            {
                tempIndex = edgeTriangleIndex_copy[i + 1];
                edgeTriangleIndex_copy[i + 1] = edgeTriangleIndex_copy[i + 2];
                edgeTriangleIndex_copy[i + 2] = tempIndex;
            }
            mesh_bottom_plane.vertices = edgePoints_copy.ToArray();
            mesh_bottom_plane.triangles = edgeTriangleIndex_copy.ToArray();
            //绘制侧面plane
            int currentVerticePointsCount = edgePoints_copy.Count;
            //绘制侧面
            for (int i = 0; i < currentVerticePointsCount - 1; i++)
            {
                edgePoints_copy.Add(new Vector3(edgePoints_copy[i].x, baseLevelHight, edgePoints_copy[i].z));
                edgeTriangleIndex_copy.Add(i);
                edgeTriangleIndex_copy.Add(edgePoints_copy.Count - 1);
                edgeTriangleIndex_copy.Add(i + 1);
                edgePoints_copy.Add(new Vector3(edgePoints_copy[i + 1].x, baseLevelHight, edgePoints_copy[i + 1].z));
                edgeTriangleIndex_copy.Add(edgePoints_copy.Count - 2);
                edgeTriangleIndex_copy.Add(edgePoints_copy.Count - 1);
                edgeTriangleIndex_copy.Add(i + 1);
            }
            //补充最后一块侧面
            edgePoints_copy.Add(new Vector3(edgePoints_copy[currentVerticePointsCount - 1].x, baseLevelHight, edgePoints_copy[currentVerticePointsCount - 1].z));
            edgeTriangleIndex_copy.Add(currentVerticePointsCount - 1);
            edgeTriangleIndex_copy.Add(edgePoints_copy.Count - 1);
            edgeTriangleIndex_copy.Add(0);
            edgeTriangleIndex_copy.Add(edgePoints_copy.Count - 1);
            edgeTriangleIndex_copy.Add(currentVerticePointsCount);
            edgeTriangleIndex_copy.Add(0);
            Mesh mesh_edge_plane = new Mesh();
            mesh_edge_plane.vertices = edgePoints_copy.ToArray();
            mesh_edge_plane.triangles = edgeTriangleIndex_copy.ToArray();

            splitTop_splitObject.Split(splitPlane, baseLevelHight, out MeshInfo a, out MeshInfo b, out MeshInfo cut);
#if !Test_ShowMesh
            Destroy(splitTop_splitObject.gameObject);
#endif
            //绘制切过后的顶部
            Mesh mesh_top_cut = new Mesh();
            if (cut.vertices.Count != 0)
            {
                mesh_top_cut.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh_top_cut.vertices = cut.vertices.ToArray();
                mesh_top_cut.triangles = cut.triangles.ToArray();
#if Test_ShowMesh
				GameObject child_cut = new GameObject("child_cut");
				child_cut.AddComponent<MeshFilter>().mesh = mesh_top_cut;
				child_cut.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
#endif
            }
            CombineInstance[] coms = new CombineInstance[2 + mesh_edge.Count];
            coms[0].mesh = mesh_bottom_plane;
            coms[0].transform = transform.localToWorldMatrix;
            coms[1].mesh = mesh_top;
            coms[1].transform = transform.localToWorldMatrix;
            for (int i = 0; i < mesh_edge.Count; i++)
            {
                coms[2 + i].mesh = mesh_edge[i];
                coms[2 + i].transform = transform.localToWorldMatrix;
            }

            //合并顶部，侧边和底部
            Mesh mesh_volume = new Mesh();
            mesh_volume.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh_volume.CombineMeshes(coms);
            //求区域内总体积
            totalVolume = mesh_volume.GetVolume();
#if Test_ShowMesh
			GameObject volume = new GameObject("volume");
			volume.AddComponent<MeshFilter>().mesh = mesh_volume;
			volume.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
#endif

            //合并切过的顶部，切割侧边和底部
            coms[1].mesh = mesh_top_cut;
            for (int i = 0; i < mesh_edge_cut.Count; i++)
            {
                coms[2 + i].mesh = mesh_edge_cut[i];
                coms[2 + i].transform = transform.localToWorldMatrix;
            }
            Mesh mesh_volume_cut = new Mesh();
            mesh_volume_cut.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh_volume_cut.CombineMeshes(coms);
            //求切割后底部体积
            cutVolume = mesh_volume_cut.GetVolume();
#if Test_ShowMesh
			GameObject volume_cut = new GameObject("volume_cut");
			volume_cut.AddComponent<MeshFilter>().mesh = mesh_volume_cut;
			volume_cut.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
#endif

            //合并顶切片和侧面切片以及底部
            coms = new CombineInstance[2];
            coms[0].mesh = mesh_top_plane;
            coms[0].transform = transform.localToWorldMatrix;
            coms[1].mesh = mesh_edge_plane;
            coms[1].transform = transform.localToWorldMatrix;
            Mesh mesh_volume_plane = new Mesh();
            mesh_volume_plane.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh_volume_plane.CombineMeshes(coms);
            baseVolume = mesh_volume_plane.GetVolume();
#if Test_ShowMesh
			GameObject volume_plane = new GameObject("volume_plane");
			volume_plane.AddComponent<MeshFilter>().mesh = mesh_volume_plane;
			volume_plane.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
#endif


            if (a.vertices.Count != 0)
            {
                Mesh mesh_a = new Mesh();
                mesh_a.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh_a.vertices = a.vertices.ToArray();
                mesh_a.triangles = a.triangles.ToArray();
#if Test_ShowMesh

				GameObject child_a = new GameObject("child_a");
				child_a.AddComponent<MeshFilter>().mesh = mesh_a;
				child_a.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
#endif
                topArea = mesh_a.GetArea();

            }
            if (b.vertices.Count != 0)
            {
                Mesh mesh_b = new Mesh();
                mesh_b.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh_b.vertices = b.vertices.ToArray();
                mesh_b.triangles = b.triangles.ToArray();
#if Test_ShowMesh
				GameObject child_b = new GameObject("child_b");
				child_b.AddComponent<MeshFilter>().mesh = mesh_b;
				child_b.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
#endif
                bottomArea = totalArea - topArea;
            }
            Debug.Log("------------------------------------------");
            if (baseLevelHight >= maxLevelHight)
            {
                Debug.Log("baseHight > maxHight");
            }
            else if (baseLevelHight > minLevelHight && baseLevelHight < maxLevelHight)
            {
                Debug.Log("minHight < baseHight < maxHight");
            }
            else
            {
                //此时mesh_volume_cut 和 mesh_volume_plane 应该是一样的东西
                Debug.Log("baseHight < minHight");
            }
            digVolume = (totalVolume - cutVolume);
            fillVolume = (baseVolume - cutVolume < 0 ? 0 : baseVolume - cutVolume);
            OnCalcalateDataCompleted?.Invoke(new CutAndFillData(BaseLevelHight, minLevelHight, maxLevelHight, totalArea, topArea, bottomArea, digVolume, fillVolume, digVolume + fillVolume));
            Debug.Log("最高点：" + maxHightPoint.ToString("F2"));
            Debug.Log("最低点：" + minHightPoint.ToString("F2"));
            Debug.Log("基准高：" + baseLevelHight.ToString("F2"));
            Debug.Log("挖方面积为：" + topArea.ToString("F2") + "平方米");
            Debug.Log("填方面积为：" + bottomArea.ToString("F2") + "平方米");
            Debug.Log("挖填方总面积为：" + totalArea.ToString("F2") + "平方米");
            Debug.Log("挖方体积为：" + digVolume.ToString("F2") + "立方米");
            Debug.Log("填方体积为：" + fillVolume.ToString("F2") + "立方米");
            Debug.Log("填挖方总体积为：" + (digVolume + fillVolume).ToString("F2") + "立方米");
            Debug.Log("------------------------------------------");
            Debug.Log("基准高乘以填方面积：" + (baseLevelHight * bottomArea).ToString("F2"));
        }
        Vector3 maxHightPoint;
        Vector3 minHightPoint;
        /// <summary>
        /// 在画线区域内的地表点
        /// </summary>
        //private List<Vector3> surfacePointInArea = new List<Vector3>();
        List<List<bool>> currentHitObjectPointInSides = new List<List<bool>>();
        /// <summary>
        /// 对最小区域内的所有顶点遍历，找到符合画线区域内的顶点，对比找到最高点和最低点
        /// </summary>
        private async void FormeshVertices_func()
        {
            //runTime = Time.realtimeSinceStartup;
            maxHightPoint = new Vector3(0, -1000, 0);
            minHightPoint = new Vector3(0, 1000, 0);
            Vector3 transPoint;
            await Task.Run(() =>
            {
                foreach (var hitData in hitDatas)
                {
                    transPoint = hitData.Key;
                    List<bool> pointInSides = new List<bool>();
                    currentHitObjectPointInSides.Add(pointInSides);
                    foreach (var currentVertice in hitData.Value)
                    {
                        //将基于自身节点的位置的本地坐标转换为世界坐标
                        var currentTransPoint = transPoint + currentVertice;

                        if (PointIsInSidePolygon(currentTransPoint, kAndBs))
                        {
                            pointInSides.Add(true);
                            //surfacePointInArea.Add(currentTransPoint);
                            if (maxHightPoint.y < currentTransPoint.y)
                            {
                                maxHightPoint = currentTransPoint;
                            }
                            if (minHightPoint.y > currentTransPoint.y)
                            {
                                minHightPoint = currentTransPoint;
                            }
                        }
                        else pointInSides.Add(false);
                    }
                }
            });
            //Debug.LogError(Time.realtimeSinceStartup - runTime);
            bounds.Encapsulate(maxHightPoint);
            bounds.Encapsulate(minHightPoint);
            //Debug.DrawLine(bounds.center - new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z), bounds.center - new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z), bounds.center - new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z), bounds.center - new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z), bounds.center - new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z), bounds.center - new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z), bounds.center - new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z), bounds.center - new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z), bounds.center - new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z), bounds.center - new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z), bounds.center - new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z), bounds.center - new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z), Color.red, 100f);
            //Debug.DrawLine(bounds.center - new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z), bounds.center - new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z), Color.red, 100f);
            isGetHightPointCompleted = true;
        }
        List<KeyValuePair<Vector3, Vector3[]>> hitDatas = new List<KeyValuePair<Vector3, Vector3[]>>();
        private List<Vector4> kAndBs = new List<Vector4>();
        List<Vector3> startLinePoints = new List<Vector3>();
        Bounds bounds;
        /// <summary>
        /// 创建包围盒用来设置最小检测区域
        /// </summary>
        /// <param name="startPoints"></param>
        private void CreateBoundsCube(List<Vector3> startPoints)
        {
            bounds = new Bounds();
            for (int i = 0; i < startPoints.Count; i++)
            {
                bounds.Encapsulate(startPoints[i]);
            }

            var hitObjects = Physics.BoxCastAll(bounds.center + new Vector3(0, 1000f, 0), bounds.extents, Vector3.down);
            foreach (var hitObject in hitObjects)
            {
                splitObjects.Add(hitObject.transform.gameObject.AddComponent<SplitObject>());
                hitDatas.Add(new KeyValuePair<Vector3, Vector3[]>(hitObject.transform.position, hitObject.collider.GetComponent<MeshFilter>().mesh.vertices));
            }
            //计算所有包围点形成的包围线的直线方程和x区域
            float k;
            float b;
            float maxX;
            float minX;
            for (int i = 0; i < startPoints.Count; i++)
            {
                if (i == startPoints.Count - 1)
                {
                    if (GetLineKANDB_XZ_Axis(startPoints[i], startPoints[0], out k, out b, out maxX, out minX))
                    {
                        kAndBs.Add(new Vector4(k, b, maxX, minX));
                        startLinePoints.Add(startPoints[i]);
                        //linePlanes.Add(new Plane(startPoints[i], startPoints[0] - new Vector3(0, 10, 0), startPoints[0]));
                    }

                }
                else if (GetLineKANDB_XZ_Axis(startPoints[i], startPoints[i + 1], out k, out b, out maxX, out minX))
                {
                    kAndBs.Add(new Vector4(k, b, maxX, minX));
                    startLinePoints.Add(startPoints[i]);
                    //linePlanes.Add(new Plane(startPoints[i], startPoints[i + 1] - new Vector3(0, 10, 0), startPoints[i + 1]));
                }
            }

        }
        public static bool PointIsInSidePolygon(Vector3 point, List<Vector4> polygonLines)
        {
            int rightCount = 0;
            int leftCount = 0;
            for (int i = 0; i < polygonLines.Count; i++)
            {
                //点不在线的X区域内
                if (point.x > polygonLines[i].z || point.x < polygonLines[i].w)
                {
                    continue;
                }
                //点在线的左边还是右边,使用的是点是否在多边形内的算法
                if (point.x * polygonLines[i].x + polygonLines[i].y >= point.z)
                {
                    rightCount++;
                }
                else leftCount++;

            }
            if (leftCount % 2 != 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取画线区域每条边界线的KandB,以及各线的X方向最大和最小值
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="dstPoint"></param>
        /// <param name="k"></param>
        /// <param name="b"></param>
        /// <param name="maxX"></param>
        /// <param name="minX"></param>
        /// <returns></returns>
        public static bool GetLineKANDB_XZ_Axis(Vector3 startPoint, Vector3 dstPoint, out float k, out float b, out float maxX, out float minX)
        {
            if (dstPoint.x - startPoint.x >= -0.001f && dstPoint.x - startPoint.x <= 0.001f)
            {
                k = 100;
                b = dstPoint.x;
                maxX = dstPoint.x;
                minX = dstPoint.x;
                //maxY = 0;
                //minY = 0;
                return false;
            }
            k = (dstPoint.z - startPoint.z) / (dstPoint.x - startPoint.x);
            b = startPoint.z - k * startPoint.x;
            maxX = Mathf.Max(startPoint.x, dstPoint.x);
            minX = Mathf.Min(startPoint.x, dstPoint.x);
            //maxY = Mathf.Max(startPoint.y, dstPoint.y);
            //minY = Mathf.Max(startPoint.y, dstPoint.y);
            return true;
        }
        /// <summary>
        /// 根据最高点和最低点，计算出最高位置和最低位置，基准位置
        /// </summary>
        private void Analysis_Start()
        {
            baseLevelHight = (maxHightPoint.y + minHightPoint.y) / 2;
            splitPlane = new Plane(Vector3.up, new Vector3(0, baseLevelHight, 0));
            //baseLevelHight = maxHightPoint.y;
            maxLevelHight = maxHightPoint.y;
            minLevelHight = minHightPoint.y;
        }
        /// <summary>
        /// 处理鼠标点击获得相应世界坐标
        /// </summary>
        private void Handle_MousePoint()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit))
            {
                tempPoint = raycastHit.point;
                if (Input.GetMouseButtonDown(0) || isCreated)
                {
                    isCreated = false;
                    if (doubleClickTimer >= doubleClickTime)
                    {
                        doubleClickTimer = 0;
                        if (!isStart)
                        {
                            isStart = true;
                            combineMaterial.EnableKeyword("ProgramRun_On");
                        }
                        startPoints.Add(tempPoint);
                        pointCount++;
                    }
                    //双击鼠标
                    else if (tempPoint == startPoints[startPoints.Count - 1] && doubleClickTimer < doubleClickTime)
                    {
                        doubleClickTimer = 0;
                        if (pointCount <= 2)
                        {
                            pointCount = 0;
                            startPoints.Clear();
                            isStart = false;
                            combineMaterial.DisableKeyword("ProgramRun_On");
                        }
                        else isMouseClickCompleted = true;
                    }
                    //快速双击了鼠标但不是点击同一个地方
                    else
                    {
                        doubleClickTimer = 0;
                        if (!isStart)
                        {
                            isStart = true;
                            combineMaterial.EnableKeyword("ProgramRun_On");
                        }
                        startPoints.Add(tempPoint);
                        pointCount++;
                    }

                }
                else if (Input.GetMouseButtonDown(1) && startPoints.Count > 0)
                {
                    startPoints.RemoveAt(startPoints.Count - 1);
                    pointCount--;
                    if (startPoints.Count == 0)
                    {
                        combineMaterial.DisableKeyword("ProgramRun_On");
                        isStart = false;
                    }
                }
                Shader_DrawLine();
                doubleClickTimer += Time.deltaTime;
                if (doubleClickTimer >= 1) doubleClickTimer = doubleClickTime;
            }
        }
        private Vector4[] originPoints = new Vector4[1023];
        private Vector4[] dstPoints = new Vector4[1023];

        /// <summary>
        /// shader画点与点之间的地表线
        /// </summary>
        private void Shader_DrawLine()
        {
            if (!isStart) return;
            originPoints = new Vector4[1023];
            dstPoints = new Vector4[1023];

            for (int i = 0; i < pointCount - 1; i = i + 1)
            {
                originPoints[i] = startPoints[i];
                dstPoints[i] = startPoints[i + 1];
            }
            originPoints[pointCount - 1] = startPoints[0];
            dstPoints[pointCount - 1] = tempPoint;
            if (startPoints.Count - 1 != 0)
            {
                originPoints[pointCount] = startPoints[startPoints.Count - 1];
                dstPoints[pointCount] = tempPoint;
                combineMaterial.SetInt("_LineCount", pointCount + 1);
            }
            else combineMaterial.SetInt("_LineCount", pointCount);
            combineMaterial.SetVectorArray("_OriginPoints", originPoints);
            combineMaterial.SetVectorArray("_DstPoints", dstPoints);

        }
        /// <summary>
        /// 显示填挖区域的不规则平面
        /// </summary>
        Mesh mesh;
        Triangulation triangulation;
        Material cutAndFill_mat;

        List<Vector3> edgePoints;
        List<int> edgeTriangleIndex;
        //List<Vector3> verticePoints;
        //List<int> triangleIndex_list;
        /// <summary>
        /// 创建画线区域不规则mesh
        /// </summary>
        /// <param name="startPoints"></param>
        private void CreateSpecialMesh(List<Vector3> startPoints)
        {
            if (mesh) return;
            List<Vector3> verticePoints = new List<Vector3>();
            List<int> triangleIndex_list = new List<int>();
            GameObject childGo = new GameObject("CutAndFillIrregularPlane");
            childGo.transform.parent = transform;
            childGo.transform.localPosition = new Vector3(0, minLevelHight, 0);
            childGo.transform.localRotation = Quaternion.identity;
            childGo.transform.localScale = new Vector3(1, 0.5f, 1);
            mesh = new Mesh();
            var meshFilter = childGo.AddComponent<MeshFilter>();
            var meshRenderer = childGo.AddComponent<MeshRenderer>();
            cutAndFill_mat = new Material(Shader.Find("Unlit/CutAndFill_childShader"));
            cutAndFill_mat.color = new Color(0, 0, 1, 0.5f);
            meshRenderer.material = cutAndFill_mat;

            for (int i = 0; i < startPoints.Count; i++)
            {
                verticePoints.Add(new Vector3(startPoints[i].x, maxLevelHight, startPoints[i].z) - childGo.transform.localPosition);
            }
            edgePoints = new List<Vector3>(verticePoints);
            //使用多边形三角化其中一个算法，耳切法 https://github.com/yiwei151/PolygonTriangulation
            triangulation = new Triangulation(verticePoints);
            triangulation.SetCompareAxle(CompareAxle.Y);
            var triangleIndex = triangulation.GetTriangles();
            if (triangleIndex == null)
            {
                Debug.Log("不是简单多边形??");
                return;
            }

            //将三角形反面
            int tempIndex;
            for (int i = 0; i < triangleIndex.Length; i = i + 3)
            {
                tempIndex = triangleIndex[i + 1];
                triangleIndex[i + 1] = triangleIndex[i + 2];
                triangleIndex[i + 2] = tempIndex;
            }
            foreach (var index in triangleIndex)
            {
                triangleIndex_list.Add(index);
            }
            edgeTriangleIndex = new List<int>(triangleIndex_list);
            int currentVerticePointsCount = verticePoints.Count;
            //绘制侧面
            for (int i = 0; i < currentVerticePointsCount - 1; i++)
            {
                verticePoints.Add(new Vector3(verticePoints[i].x, minLevelHight, verticePoints[i].z) - childGo.transform.localPosition);
                triangleIndex_list.Add(i);
                triangleIndex_list.Add(i + 1);
                triangleIndex_list.Add(verticePoints.Count - 1);
                verticePoints.Add(new Vector3(verticePoints[i + 1].x, minLevelHight, verticePoints[i + 1].z) - childGo.transform.localPosition);
                triangleIndex_list.Add(verticePoints.Count - 2);
                triangleIndex_list.Add(i + 1);
                triangleIndex_list.Add(verticePoints.Count - 1);
            }
            //补充最后一块侧面
            verticePoints.Add(new Vector3(verticePoints[currentVerticePointsCount - 1].x, minLevelHight, verticePoints[currentVerticePointsCount - 1].z) - childGo.transform.localPosition);
            triangleIndex_list.Add(currentVerticePointsCount - 1);
            triangleIndex_list.Add(0);
            triangleIndex_list.Add(verticePoints.Count - 1);
            triangleIndex_list.Add(verticePoints.Count - 1);
            triangleIndex_list.Add(0);
            triangleIndex_list.Add(currentVerticePointsCount);

            mesh.vertices = verticePoints.ToArray();
            mesh.triangles = triangleIndex_list.ToArray();
            meshFilter.mesh = mesh;
            combineMaterial.EnableKeyword("AnalysisCompleted_On");
        }
        private Material lineMaterial;
        /// <summary>
        /// 创建画边框线的mat
        /// </summary>
        private void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                Shader shader = Shader.Find("Unlit/GLColorShader");
                lineMaterial = new Material(shader);
            }
        }
        /// <summary>
        /// 画填挖区域边框线
        /// </summary>
        private void OnRenderObject()
        {
            //return;
            if (!isMouseClickCompleted) return;
            CreateLineMaterial();
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            // Draw lines
            GL.Begin(GL.LINE_STRIP);
            GL.Color(new Color(0.5f, 1, 0.5f));
            foreach (var startPoint in startPoints)
            {
                GL.Vertex3(startPoint.x, baseLevelHight, startPoint.z);
            }
            GL.Vertex3(startPoints[0].x, baseLevelHight, startPoints[0].z);
            GL.End();

            GL.PopMatrix();
        }
        private void OnGUI()
        {
            //GUIStyle fontStyle = new GUIStyle();
            //fontStyle.normal.textColor = Color.green;
            //fontStyle.fontSize = 20;
            //Vector3 screenPoint;
            //Rect rect;
            //foreach (var cutPoint in cutPoints_vertical)
            //{
            //    foreach (var point in cutPoint)
            //    {
            //        screenPoint = Camera.main.WorldToScreenPoint(point);
            //        rect = new Rect(new Vector2(screenPoint.x - fontStyle.fontSize / 2, Screen.height - screenPoint.y - fontStyle.fontSize / 2), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
            //        GUI.Label(rect, icon, fontStyle);
            //    }

            //}
            //screenPoint = Camera.main.WorldToScreenPoint(maxHightPoint);
            //rect = new Rect(new Vector2(screenPoint.x - fontStyle.fontSize, Screen.height - screenPoint.y - fontStyle.fontSize), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
            //GUI.Label(rect, icon, fontStyle);

            //screenPoint = Camera.main.WorldToScreenPoint(minHightPoint);
            //rect = new Rect(new Vector2(screenPoint.x - fontStyle.fontSize, Screen.height - screenPoint.y - fontStyle.fontSize), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
            //GUI.Label(rect, icon, fontStyle);

            //foreach (var point in surfacePointInArea)
            //{
            //	screenPoint = Camera.main.WorldToScreenPoint(point);
            //	rect = new Rect(new Vector2(screenPoint.x - fontStyle.fontSize / 2, Screen.height - screenPoint.y - fontStyle.fontSize / 2), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
            //	GUI.Label(rect, icon, fontStyle);
            //}
            //--------------------------------------------

        }
    }
}
