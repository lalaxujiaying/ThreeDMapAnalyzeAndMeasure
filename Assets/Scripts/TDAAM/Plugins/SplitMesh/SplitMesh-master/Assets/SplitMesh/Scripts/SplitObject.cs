using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TDAAM.Analysis;
namespace SplitMesh
{
    /// <summary>
    /// 模型切割脚本
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    //[RequireComponent(typeof(MeshCollider))]
    public class SplitObject : MonoBehaviour
    {
        public bool fill = false;
        public Rect uvRange = Rect.MinMaxRect(0, 0, 1, 1);
        public Texture icon;
        public MeshInfo MeshInfo { get; set; }
        void Start()
        {
            if (MeshInfo == null)
            {
                MeshInfo = new MeshInfo(GetComponent<MeshFilter>().mesh);
            }
            icon = Resources.Load<Texture>("新消息红点");
        }
        public void UpdateMesh(params MeshInfo[] info)
        {
            CombineInstance[] coms = new CombineInstance[info.Length];
            for (int i = 0; i < info.Length; i++)
            {
                coms[i].mesh = info[i].GetMesh();
                coms[i].transform = Matrix4x4.identity;
            }
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(coms);
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            MeshInfo = new MeshInfo(mesh);
            MeshInfo.center = info[0].center;
            MeshInfo.size = info[0].size;
        }

        List<Vector3> cutPoints = new List<Vector3>();
        public void Split(Plane plane, float hight, out MeshInfo a, out MeshInfo b, out MeshInfo cut)
        {
            if (MeshInfo == null)
            {
                MeshInfo = new MeshInfo(GetComponent<MeshFilter>().mesh);
            }
            Vector3 point = transform.InverseTransformPoint(plane.normal * -plane.distance);
            Vector3 normal = transform.InverseTransformDirection(plane.normal);
            normal.Scale(transform.localScale);
            normal.Normalize();
            a = new MeshInfo();
            b = new MeshInfo();
            cut = new MeshInfo();
            cut.vertices = new List<Vector3>(MeshInfo.vertices);
            cut.triangles = new List<int>(MeshInfo.triangles);

            bool[] above = new bool[MeshInfo.vertices.Count];
            int[] newTriangles = new int[MeshInfo.vertices.Count];

            for (int i = 0; i < newTriangles.Length; i++)
            {
                Vector3 vert = MeshInfo.vertices[i];
                //顶点在切面上方且在画线区域内，才切割
                above[i] = Vector3.Dot(vert - point, normal) >= 0f;
                if (above[i])
                {
                    newTriangles[i] = a.vertices.Count;
                    a.vertices.Add(vert);

                    cut.vertices[i] = new Vector3(vert.x, hight, vert.z);
                }
                else
                {
                    newTriangles[i] = b.vertices.Count;
                    b.vertices.Add(vert);
                }
            }

            int triangleCount = MeshInfo.triangles.Count / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int _i0 = MeshInfo.triangles[i * 3];
                int _i1 = MeshInfo.triangles[i * 3 + 1];
                int _i2 = MeshInfo.triangles[i * 3 + 2];

                bool _a0 = above[_i0];
                bool _a1 = above[_i1];
                bool _a2 = above[_i2];
                if (_a0 && _a1 && _a2)
                {
                    a.triangles.Add(newTriangles[_i0]);
                    a.triangles.Add(newTriangles[_i1]);
                    a.triangles.Add(newTriangles[_i2]);
                }
                else if (!_a0 && !_a1 && !_a2)
                {
                    b.triangles.Add(newTriangles[_i0]);
                    b.triangles.Add(newTriangles[_i1]);
                    b.triangles.Add(newTriangles[_i2]);
                }
                else
                {
                    int up, down0, down1;
                    if (_a1 == _a2 && _a0 != _a1)
                    {
                        up = _i0;
                        down0 = _i1;
                        down1 = _i2;
                    }
                    else if (_a2 == _a0 && _a1 != _a2)
                    {
                        up = _i1;
                        down0 = _i2;
                        down1 = _i0;
                    }
                    else
                    {
                        up = _i2;
                        down0 = _i0;
                        down1 = _i1;
                    }

                    Vector3 pos0;
                    Vector3 pos1;

                    if (above[up])
                        SplitTriangle(a, b, point, normal, newTriangles, up, down0, down1, out pos0, out pos1);
                    else
                        SplitTriangle(b, a, point, normal, newTriangles, up, down0, down1, out pos1, out pos0);

                    cutPoints.Add(pos0);
                    cutPoints.Add(pos1);

                }
            }
            //cut = FastFillCutEdges(cutPoints, point, normal);
            //a.CombineVertices(0.001f);
            //a.center = MeshInfo.center;
            //a.size = MeshInfo.size;
            //b.CombineVertices(0.001f);
            //b.center = MeshInfo.center;
            //b.size = MeshInfo.size;


            //MeshInfo cut = FastFillCutEdges(cutPoints, point, normal);
            //if (b.vertices.Count != 0) Instantiate(gameObject).GetComponent<SplitObject>().UpdateMesh(b);
            //Instantiate(gameObject).GetComponent<SplitObject>().UpdateMesh(b, cut);
            //cut.Reverse();
            //Instantiate(gameObject).GetComponent<SplitObject>().UpdateMesh(a, cut);
            //Destroy(gameObject);
        }
        public void Split_Vertical(List<Vector4> kAndBs, out MeshInfo inSideMesh, out MeshInfo outSideMesh, List<List<Vector3>> cut_verticalPoints)
        {
            inSideMesh = new MeshInfo();
            outSideMesh = new MeshInfo();

            inSideMesh.localToWorldMatrix = transform.localToWorldMatrix;
            outSideMesh.localToWorldMatrix = transform.localToWorldMatrix;
            bool[] inSide = new bool[MeshInfo.vertices.Count];
            int[] newVerticesIndex = new int[MeshInfo.vertices.Count];

            for (int i = 0; i < newVerticesIndex.Length; i++)
            {
                Vector3 worldPoint = transform.TransformPoint(MeshInfo.vertices[i]);

                if (CutAndFillAnalysis.PointIsInSidePolygon(worldPoint, kAndBs))
                {
                    inSide[i] = true;
                }

                if (inSide[i])
                {
                    newVerticesIndex[i] = inSideMesh.vertices.Count;
                    inSideMesh.vertices.Add(MeshInfo.vertices[i]);
                    //inSideMesh.Add(MeshInfo.vertices[i], MeshInfo.uvs[i], MeshInfo.normals[i], MeshInfo.tangents[i]);
                }
                else
                {
                    newVerticesIndex[i] = outSideMesh.vertices.Count;
                    outSideMesh.vertices.Add(MeshInfo.vertices[i]);
                    //outSideMesh.Add(MeshInfo.vertices[i], MeshInfo.uvs[i], MeshInfo.normals[i], MeshInfo.tangents[i]);
                }

            }

            int triangleCount = MeshInfo.triangles.Count / 3;

            for (int i = 0; i < triangleCount; i++)
            {
                int _i0 = MeshInfo.triangles[i * 3];
                int _i1 = MeshInfo.triangles[i * 3 + 1];
                int _i2 = MeshInfo.triangles[i * 3 + 2];

                Vector3 cutDown0Point = Vector3.zero;
                Vector3 cutDown1Point = Vector3.zero;
                Vector3 cornerPoint = Vector3.zero;
                Vector4 firstLine = Vector4.zero;
                Vector4 secondLine = Vector4.zero;

                if (inSide[_i0] && inSide[_i1] && inSide[_i2])
                {
                    inSideMesh.triangles.Add(newVerticesIndex[_i0]);
                    inSideMesh.triangles.Add(newVerticesIndex[_i1]);
                    inSideMesh.triangles.Add(newVerticesIndex[_i2]);
                }
                else if (!inSide[_i0] && !inSide[_i1] && !inSide[_i2])
                {
                    outSideMesh.triangles.Add(newVerticesIndex[_i0]);
                    outSideMesh.triangles.Add(newVerticesIndex[_i1]);
                    outSideMesh.triangles.Add(newVerticesIndex[_i2]);
                }
                else
                {
                    int up, down0, down1;
                    if (inSide[_i1] == inSide[_i2] && inSide[_i0] != inSide[_i1])
                    {
                        up = _i0;
                        down0 = _i1;
                        down1 = _i2;
                    }
                    else if (inSide[_i2] == inSide[_i0] && inSide[_i1] != inSide[_i2])
                    {
                        up = _i1;
                        down0 = _i2;
                        down1 = _i0;
                    }
                    else
                    {
                        up = _i2;
                        down0 = _i0;
                        down1 = _i1;
                    }
                    float x, z, y;
                    Vector3 up_World = transform.TransformPoint(MeshInfo.vertices[up]);
                    Vector3 down0_World = transform.TransformPoint(MeshInfo.vertices[down0]);
                    Vector3 down1_World = transform.TransformPoint(MeshInfo.vertices[down1]);
                    var bool1 = CutAndFillAnalysis.GetLineKANDB_XZ_Axis(up_World, down0_World, out float k, out float b, out float maxX, out float minX);

                    var bool2 = CutAndFillAnalysis.GetLineKANDB_XZ_Axis(up_World, down1_World, out float k1, out float b1, out float maxX1, out float minX1);

                    float UpToDown0MaxY, UpToDown0MinY, UpToDown1MaxY, UpToDown1MinY;
                    UpToDown0MaxY = Mathf.Max(up_World.z, down0_World.z);
                    UpToDown0MinY = Mathf.Min(up_World.z, down0_World.z);
                    UpToDown1MaxY = Mathf.Max(up_World.z, down1_World.z);
                    UpToDown1MinY = Mathf.Min(up_World.z, down1_World.z);

                    //是否找到up-down那条线与画线区域的交点
                    bool isFindUpToDown0 = false;
                    bool isFindUpToDown1 = false;
                    //第几条线
                    int currentLineCount = 0;
                    int firstLineCount = 0;
                    foreach (var kAndB in kAndBs)
                    {
                        if (k != kAndB.x && !isFindUpToDown0 && bool1)
                        {
                            x = (b - kAndB.y) / (kAndB.x - k);
                            if (x <= maxX && x >= minX && x <= kAndB.z && x >= kAndB.w)
                            {
                                z = k * x + b;
                                y = (x - up_World.x) * (down0_World.y - up_World.y) / (down0_World.x - up_World.x) + up_World.y;
                                cutPoints.Add(new Vector3(x, y, z));
                                cutDown0Point = new Vector3(x, y, z);
                                cut_verticalPoints[currentLineCount].Add(cutDown0Point);
                                //Debug.DrawLine(up_World, down0_World, Color.red, 600f);
                                isFindUpToDown0 = true;
                            }
                        }
                        if (k1 != kAndB.x && !isFindUpToDown1 && bool2)
                        {
                            x = (b1 - kAndB.y) / (kAndB.x - k1);
                            if (x <= maxX1 && x >= minX1 && x <= kAndB.z && x >= kAndB.w)
                            {
                                z = k1 * x + b1;
                                y = (x - up_World.x) * (down1_World.y - up_World.y) / (down1_World.x - up_World.x) + up_World.y;
                                //Debug.DrawLine(up_World, down1_World, Color.red, 600f);
                                cutPoints.Add(new Vector3(x, y, z));
                                cutDown1Point = new Vector3(x, y, z);
                                cut_verticalPoints[currentLineCount].Add(cutDown1Point);
                                isFindUpToDown1 = true;
                            }
                        }
                        if (!bool1 && !isFindUpToDown0)
                        {
                            x = up_World.x;
                            z = kAndB.x * x + kAndB.y;
                            if (x <= kAndB.z && x >= kAndB.w && z <= UpToDown0MaxY && z >= UpToDown0MinY)
                            {
                                y = (z - up_World.z) * (down0_World.y - up_World.y) / (down0_World.z - up_World.z) + up_World.y;
                                cutPoints.Add(new Vector3(x, y, z));
                                cutDown0Point = new Vector3(x, y, z);
                                cut_verticalPoints[currentLineCount].Add(cutDown0Point);
                                //Debug.DrawLine(up_World, down0_World, Color.blue, 600f);
                                isFindUpToDown0 = true;
                            }
                        }
                        if (!bool2 && !isFindUpToDown1)
                        {
                            x = up_World.x;
                            z = kAndB.x * x + kAndB.y;
                            if (x <= kAndB.z && x >= kAndB.w && z <= UpToDown1MaxY && z >= UpToDown1MinY)
                            {
                                y = (z - up_World.z) * (down1_World.y - up_World.y) / (down1_World.z - up_World.z) + up_World.y;
                                cutPoints.Add(new Vector3(x, y, z));
                                //Debug.DrawLine(up_World, down1_World, Color.blue, 600f);
                                cutDown1Point = new Vector3(x, y, z);
                                cut_verticalPoints[currentLineCount].Add(cutDown1Point);
                                isFindUpToDown1 = true;
                            }
                        }
                        if (isFindUpToDown0 != isFindUpToDown1 && firstLine == Vector4.zero)
                        {
                            firstLine = kAndB;
                            firstLineCount = currentLineCount;
                        }
                        if (isFindUpToDown0 && isFindUpToDown1)
                        {
                            if (firstLine != Vector4.zero)
                            {
                                secondLine = kAndB;
                                if (firstLine.x - secondLine.x == 0) Debug.Log("什么意思");
                                float twoLineCrossX = (secondLine.y - firstLine.y) / (firstLine.x - secondLine.x);
                                float twoLineCrossZ = firstLine.x * twoLineCrossX + firstLine.y;

                                Plane plane = new Plane(up_World, down0_World, down1_World);
                                float twoLineCrossY = -plane.GetDistanceToPoint(new Vector3(twoLineCrossX, 0, twoLineCrossZ)) / Vector3.Dot(Vector3.up, plane.normal);
                                cornerPoint = new Vector3(twoLineCrossX, twoLineCrossY, twoLineCrossZ);
                                cut_verticalPoints[currentLineCount].Add(cornerPoint);
                                cut_verticalPoints[firstLineCount].Add(cornerPoint);

                                //cut_verticalPoints[currentLineCount].Add(new Vector3(twoLineCrossX, -20, twoLineCrossZ)) ;
                                //cut_verticalPoints[firstLineCount].Add(new Vector3(twoLineCrossX, -20, twoLineCrossZ));
                                cutPoints.Add(cornerPoint);
                            }
                            //Debug.DrawLine(down1_World, down0_World, Color.green, 600f);
                            break;
                        }
                        currentLineCount++;
                    }

                    if (!isFindUpToDown0 || !isFindUpToDown1)
                    {
                        Debug.Log("不能再到这里来了吧");
                        //Debug.Log(isFindUpToDown0);
                        //Debug.Log(isFindUpToDown1);
                    }
                    else
                    {
                        if (inSide[up])
                        {
                            inSideMesh.vertices.Add(transform.InverseTransformPoint(cutDown0Point));
                            inSideMesh.vertices.Add(transform.InverseTransformPoint(cutDown1Point));
                            inSideMesh.triangles.Add(inSideMesh.vertices.Count - 2);
                            inSideMesh.triangles.Add(inSideMesh.vertices.Count - 1);
                            inSideMesh.triangles.Add(newVerticesIndex[up]);
                        }
                        else
                        {
                            inSideMesh.vertices.Add(transform.InverseTransformPoint(cutDown0Point));
                            inSideMesh.vertices.Add(transform.InverseTransformPoint(cutDown1Point));
                            inSideMesh.triangles.Add(inSideMesh.vertices.Count - 1);
                            inSideMesh.triangles.Add(newVerticesIndex[down0]);
                            inSideMesh.triangles.Add(newVerticesIndex[down1]);

                            inSideMesh.triangles.Add(inSideMesh.vertices.Count - 1);
                            inSideMesh.triangles.Add(inSideMesh.vertices.Count - 2);
                            inSideMesh.triangles.Add(newVerticesIndex[down0]);
                        }
                        if (firstLine != Vector4.zero)
                        {
                            inSideMesh.vertices.Add(transform.InverseTransformPoint(cornerPoint));

                            if (inSide[up])
                            {
                                inSideMesh.triangles.Add(inSideMesh.vertices.Count - 3);
                                inSideMesh.triangles.Add(inSideMesh.vertices.Count - 1);
                                inSideMesh.triangles.Add(inSideMesh.vertices.Count - 2);
                            }
                            else
                            {
                                inSideMesh.triangles.Add(inSideMesh.vertices.Count - 3);
                                inSideMesh.triangles.Add(inSideMesh.vertices.Count - 2);
                                inSideMesh.triangles.Add(inSideMesh.vertices.Count - 1);
                            }
                        }
                    }

                }
            }
        }
        //private void OnGUI()
        //{
        //    GUIStyle fontStyle = new GUIStyle();
        //    fontStyle.normal.textColor = Color.green;
        //    fontStyle.fontSize = 20;
        //    Vector3 screenPoint;
        //    Rect rect;
        //    //Vector3 worldPoint;
        //    foreach (var cutPoint in cutPoints)
        //    {
        //        //worldPoint = transform.TransformPoint(cutPoint);
        //        screenPoint = Camera.main.WorldToScreenPoint(cutPoint);
        //        rect = new Rect(new Vector2(screenPoint.x - fontStyle.fontSize / 2, Screen.height - screenPoint.y - fontStyle.fontSize / 2), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
        //        GUI.Label(rect, icon, fontStyle);
        //    }
        //}
        bool SplitTriangle(MeshInfo top, MeshInfo bottom, Vector3 point, Vector3 normal, int[] newTriangles, int up, int down0, int down1, out Vector3 pos0, out Vector3 pos1)
        {
            Vector3 v0 = MeshInfo.vertices[up];
            Vector3 v1 = MeshInfo.vertices[down0];
            Vector3 v2 = MeshInfo.vertices[down1];
            float topDot = Vector3.Dot(point - v0, normal);
            float aScale = Mathf.Clamp01(topDot / Vector3.Dot(v1 - v0, normal));
            float bScale = Mathf.Clamp01(topDot / Vector3.Dot(v2 - v0, normal));
            Vector3 pos_a = v0 + (v1 - v0) * aScale;
            Vector3 pos_b = v0 + (v2 - v0) * bScale;


            int top_a = top.vertices.Count;
            top.vertices.Add(pos_a);
            int top_b = top.vertices.Count;
            top.vertices.Add(pos_b);
            top.triangles.Add(newTriangles[up]);
            top.triangles.Add(top_a);
            top.triangles.Add(top_b);

            int down_a = bottom.vertices.Count;
            bottom.vertices.Add(pos_a);
            int down_b = bottom.vertices.Count;
            bottom.vertices.Add(pos_b);

            bottom.triangles.Add(newTriangles[down0]);
            bottom.triangles.Add(newTriangles[down1]);
            bottom.triangles.Add(down_b);

            bottom.triangles.Add(newTriangles[down0]);
            bottom.triangles.Add(down_b);
            bottom.triangles.Add(down_a);

            pos0 = pos_a;
            pos1 = pos_b;
            return true;
        }
        MeshInfo FillCutEdges(List<Vector3> edges, Vector3 pos, Vector3 normal)
        {
            if (edges.Count < 3)
                throw new Exception("edges point less 3!");

            for (int i = 0; i < edges.Count; i++)
            {
                for (int j = i + 1; j < edges.Count; j++)
                    if ((edges[i] - edges[j]).sqrMagnitude < 1e-5f)
                    {
                        edges.RemoveAt(j);
                    }
            }

            Vector3 start = edges[0];
            Vector3 dir = Vector3.zero;
            for (int i = 1; i < edges.Count; i++)
            {
                if (dir == Vector3.zero)
                {
                    dir = edges[i] - start;
                }
            }
            dir.Normalize();
            int count = edges.Count - 1;
            for (int i = 2; i < count; i++)
            {
                Vector3 a = edges[i] - start;
                float angle = Vector3.Dot(a.normalized, dir);
                float dis = a.sqrMagnitude;
                if (dis < 1e-6f)
                    continue;

                bool change = false;

                for (int j = i + 1; j < edges.Count; j++)
                {
                    Vector3 b = edges[j] - start;
                    float _angle = Vector3.Dot(b.normalized, dir);
                    float _dis = b.sqrMagnitude;
                    bool next = _dis <= dis;
                    next = (angle - _angle < 0.001f) && (_angle > 0.9999f ? _dis < dis : _dis >= dis);
                    next = change ? false : next;
                    if (_angle - angle > 0.001f || next || _dis < 1e-6f)
                    {
                        if (_angle - angle > 0.001f)
                            change = true;
                        angle = _angle;
                        dis = _dis;
                        Vector3 temp;
                        temp = edges[i];
                        edges[i] = edges[j];
                        edges[j] = temp;
                    }
                }
            }

            Vector4 tangent = MeshInfo.CalculateTangent(normal);

            MeshInfo cutEdges = new MeshInfo();
            for (int i = 0; i < edges.Count; i++)
                cutEdges.Add(edges[i], Vector2.zero, normal, tangent);
            for (int i = 1; i < count; i++)
            {
                cutEdges.triangles.Add(0);
                cutEdges.triangles.Add(i);
                cutEdges.triangles.Add(i + 1);
            }

            cutEdges.center = MeshInfo.center;
            cutEdges.size = MeshInfo.size;
            cutEdges.MapperCube(uvRange);
            return cutEdges;
        }
        MeshInfo FastFillCutEdges(List<Vector3> edges, Vector3 pos, Vector3 normal)
        {
            if (edges.Count < 3)
                throw new Exception("edges point less 3!");

            for (int i = 0; i < edges.Count - 3; i++)
            {
                Vector3 t = edges[i + 1];
                Vector3 temp = edges[i + 3];
                for (int j = i + 2; j < edges.Count - 1; j += 2)
                {
                    if ((edges[j] - t).sqrMagnitude < 1e-6)
                    {
                        edges[j] = edges[i + 2];
                        edges[i + 3] = edges[j + 1];
                        edges[j + 1] = temp;
                        break;
                    }
                    if ((edges[j + 1] - t).sqrMagnitude < 1e-6)
                    {
                        edges[j + 1] = edges[i + 2];
                        edges[i + 3] = edges[j];
                        edges[j] = temp;
                        break;
                    }
                }
                edges.RemoveAt(i + 2);
            }
            edges.RemoveAt(edges.Count - 1);

            Vector4 tangent = MeshInfo.CalculateTangent(normal);

            MeshInfo cutEdges = new MeshInfo();
            for (int i = 0; i < edges.Count; i++)
                cutEdges.Add(edges[i], Vector2.zero, normal, tangent);
            int count = edges.Count - 1;
            for (int i = 1; i < count; i++)
            {
                cutEdges.triangles.Add(0);
                cutEdges.triangles.Add(i);
                cutEdges.triangles.Add(i + 1);
            }

            cutEdges.center = MeshInfo.center;
            cutEdges.size = MeshInfo.size;
            cutEdges.MapperCube(uvRange);
            return cutEdges;
        }
    }
}