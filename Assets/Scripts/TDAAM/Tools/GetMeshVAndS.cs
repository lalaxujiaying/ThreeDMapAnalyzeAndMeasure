using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表面积、体积的计算
/// </summary>
public static class MeshEx
{
    /// <summary>
    /// 获取表面积
    /// </summary>
    /// <param name="obj">带有MeshFilter的物体</param>
    /// <param name="callbackError">错误回调</param>
    /// <returns>表面积</returns>
    public static float GetArea(this Transform obj, Action callbackError = null)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        if (mesh == null)
        {
            Debug.LogWarning("There is no 'MeshFilter' component!");
            callbackError?.Invoke();
            return -1;
        }

        Vector3[] vertices = mesh.vertices;
        Vector3 lossyScale = obj.lossyScale;

        float area = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                area += CalculateTriangleArea(vertices[triangles[j]], vertices[triangles[j + 1]], vertices[triangles[j + 2]], lossyScale);
            }
        }

        return area;
    }
    public static float GetArea(this Mesh mesh, Action callbackError = null)
    {
        //Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        if (mesh == null)
        {
            Debug.LogWarning("There is no 'MeshFilter' component!");
            callbackError?.Invoke();
            return -1;
        }

        Vector3[] vertices = mesh.vertices;
        //Vector3 lossyScale = obj.lossyScale;

        float area = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                area += CalculateTriangleArea(vertices[triangles[j]], vertices[triangles[j + 1]], vertices[triangles[j + 2]], Vector3.one);
            }
        }

        return area;
    }
    /// <summary>
    /// 计算三角形面积
    /// </summary>
    /// <param name="point1">顶点1</param>
    /// <param name="point2">顶点2</param>
    /// <param name="point3">顶点3</param>
    /// <returns>面积</returns>
    private static float CalculateTriangleArea(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 lossyScale)
    {
        //计算缩放
        point1 = new Vector3(point1.x * lossyScale.x, point1.y * lossyScale.y, point1.z * lossyScale.z);
        point2 = new Vector3(point2.x * lossyScale.x, point2.y * lossyScale.y, point2.z * lossyScale.z);
        point3 = new Vector3(point3.x * lossyScale.x, point3.y * lossyScale.y, point3.z * lossyScale.z);

        //计算边长
        float l1 = (point2 - point1).magnitude;
        float l2 = (point3 - point2).magnitude;
        float l3 = (point1 - point3).magnitude;
        float p = (l1 + l2 + l3) * 0.5f;

        //计算面积  S=√[p(p-l1)(p-l2)(p-l3)]（p为半周长）
        return Mathf.Sqrt(p * (p - l1) * (p - l2) * (p - l3));
    }

    /// <summary>
    /// 获取体积
    /// </summary>
    /// <param name="obj">带有MeshFilter的物体</param>
    /// <param name="callbackError">错误回调</param>
    /// <returns></returns>
    public static float GetVolume(this Transform obj, Action callbackError = null)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        if (mesh == null)
        {
            Debug.LogWarning("There is no 'MeshFilter' component!");
            callbackError?.Invoke();
            return -1;
        }

        Vector3[] vertices = mesh.vertices;
        Vector3 lossyScale = obj.lossyScale;
        Vector3 o = GetCenter(vertices);
        float volume = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                volume += CalculateVolumeOfTriangle(vertices[triangles[j]], vertices[triangles[j + 1]], vertices[triangles[j + 2]], o, lossyScale);
            }
        }

        return Mathf.Abs(volume);
    }
    public static float GetVolume(this Mesh mesh, Action callbackError = null)
    {
        if (mesh == null)
        {
            Debug.LogWarning("There is no 'MeshFilter' component!");
            callbackError?.Invoke();
            return -1;
        }

        Vector3[] vertices = mesh.vertices;
        //Vector3 lossyScale = obj.lossyScale;
        Vector3 o = GetCenter(vertices);
        float volume = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                volume += CalculateVolumeOfTriangle(vertices[triangles[j]], vertices[triangles[j + 1]], vertices[triangles[j + 2]], o, Vector3.one);
            }
        }

        return Mathf.Abs(volume);
    }
    /// <summary>
    /// 获取中心点
    /// </summary>
    /// <param name="points">顶点</param>
    /// <returns>中心点</returns>
    private static Vector3 GetCenter(Vector3[] points)
    {
        Vector3 center = Vector3.zero;
        for (int i = 0; i < points.Length; i++)
        {
            center += points[i];
        }
        center = center / points.Length;
        return center;
    }

    /// <summary>
    /// 计算一个面和中心点组成三棱锥的体积
    /// </summary>
    /// <param name="point1">顶点1</param>
    /// <param name="point2">顶点2</param>
    /// <param name="point3">顶点3</param>
    /// <param name="center">中心点</param>
    /// <returns></returns>
    private static float CalculateVolumeOfTriangle(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 center, Vector3 lossyScale)
    {
        //计算缩放
        point1 = new Vector3(point1.x * lossyScale.x, point1.y * lossyScale.y, point1.z * lossyScale.z);
        point2 = new Vector3(point2.x * lossyScale.x, point2.y * lossyScale.y, point2.z * lossyScale.z);
        point3 = new Vector3(point3.x * lossyScale.x, point3.y * lossyScale.y, point3.z * lossyScale.z);

        //向量
        Vector3 v1 = point1 - center;
        Vector3 v2 = point2 - center;
        Vector3 v3 = point3 - center;

        //计算体积
        //首先我们求以这三个向量为邻棱的平行六面体的面积
        //那就是（a×b）·c的绝对值
        //然后四面体的体积是平行六面体的六分之一
        //因为四面体的底是平行六面体的一半,而且要多乘一个三分之一
        float v = Vector3.Dot(Vector3.Cross(v1, v2), v3) / 6f;
        return v;
    }
    public static void GetCutMeshPointByLine(this Mesh mesh, Transform transform, Vector3 beginPoint, Vector3 endPoint, ref List<Vector3> pos_a, ref List<Vector3> pos_b,
        Func<Vector3, Vector3, Vector3, bool> predicate = null)
    {
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i = i + 3)
        {
            List<Vector3> trianglePoints = new List<Vector3>();
            trianglePoints.Add(transform.TransformPoint(vertices[triangles[i]]));
            trianglePoints.Add(transform.TransformPoint(vertices[triangles[i + 1]]));
            trianglePoints.Add(transform.TransformPoint(vertices[triangles[i + 2]]));

            if (predicate != null && !predicate(trianglePoints[0], trianglePoints[1], trianglePoints[2])) break;

            if (LineIntersectTrangle(trianglePoints, new List<Vector3>() { beginPoint, endPoint }, out Vector3 tempPoint_a, out Vector3 tempPoint_b))
            {
                pos_a.Add(tempPoint_a);
                pos_b.Add(tempPoint_b);
            }
        }
    }
    public static void GetCutMeshPointByPlane(this Mesh mesh, Transform transform, Plane plane, ref List<Vector3> pos_a, ref List<Vector3> pos_b,
        Func<Vector3, Vector3, Vector3, bool> predicate = null)
    {
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;
        Vector3 point = transform.InverseTransformPoint(plane.normal * -plane.distance);
        Vector3 normal = transform.InverseTransformDirection(plane.normal);
        for (int i = 0; i < triangles.Length; i = i + 3)
        {
            var point1 = vertices[triangles[i]];
            var point2 = vertices[triangles[i + 1]];
            var point3 = vertices[triangles[i + 2]];

            if (predicate != null && !predicate(point1, point2, point3)) break;

            var left1 = Vector3.Dot(point1 - point, normal) >= 0f;
            var left2 = Vector3.Dot(point2 - point, normal) >= 0f;
            var left3 = Vector3.Dot(point3 - point, normal) >= 0f;

            if (left1 != left2 || left1 != left3 || left2 != left3)
            {
                Vector3 upPoint = default(Vector3);
                Vector3 downPoint1 = default(Vector3);
                Vector3 downPoint2 = default(Vector3);
                if (left1 == left2 && left1 != left3)
                {
                    upPoint = point3;
                    downPoint1 = point2;
                    downPoint2 = point1;
                }
                else if (left2 == left3 && left1 != left2)
                {
                    upPoint = point1;
                    downPoint1 = point3;
                    downPoint2 = point2;
                }
                //else
                else if (left1 == left3 && left2 != left1)
                {
                    upPoint = point2;
                    downPoint1 = point1;
                    downPoint2 = point3;
                }
                float topDot = Vector3.Dot(point - upPoint, normal);
                float aScale = Mathf.Clamp01(topDot / Vector3.Dot(downPoint1 - upPoint, normal));
                float bScale = Mathf.Clamp01(topDot / Vector3.Dot(downPoint2 - upPoint, normal));
                pos_a.Add(transform.TransformPoint(upPoint + (downPoint1 - upPoint) * aScale));
                pos_b.Add(transform.TransformPoint(upPoint + (downPoint2 - upPoint) * bScale));
            }
        }
    }
    /// <summary>
    /// 线段是否与三角形有交点
    /// </summary>
    /// <param name="tranglePoints"></param>
    /// <param name="linePoints"></param>
    /// <returns></returns>
    public static bool LineIntersectTrangle(List<Vector3> tranglePoints, List<Vector3> linePoints, out Vector3 pos_a, out Vector3 pos_b)
    {
        bool isIntersect = false;
        int trangleLineIndex1;
        int trangleLineIndex2;
        int intersectPointCount = 0;
        pos_a = default(Vector3);
        pos_b = default(Vector3);
        int[] sign1 = new int[3];
        int[] sign2 = new int[3];
        for (int i = 0; i < tranglePoints.Count; i++)
        {

            if (i == tranglePoints.Count - 1)
            {
                trangleLineIndex1 = i;
                trangleLineIndex2 = 0;
            }
            else
            {
                trangleLineIndex1 = i;
                trangleLineIndex2 = i + 1;
            }
            //三角形边与两个端点进行叉积如果符号相反，说明两个端点在这条边的左右两侧
            sign1[i] = (int)Mathf.Sign(Vector3.Cross(tranglePoints[trangleLineIndex2] - tranglePoints[trangleLineIndex1],
                linePoints[0] - tranglePoints[trangleLineIndex1]).y);
            sign2[i] = (int)Mathf.Sign(Vector3.Cross(tranglePoints[trangleLineIndex2] - tranglePoints[trangleLineIndex1],
                linePoints[1] - tranglePoints[trangleLineIndex1]).y);

            if (sign1[i] * sign2[i] == -1)
            {
                //还需要计算三角形两个顶点是否也同样在线段的两侧
                var lineSign1 = Mathf.Sign(Vector3.Cross(linePoints[0] - linePoints[1], linePoints[1] - tranglePoints[trangleLineIndex1]).y);
                var lineSign2 = Mathf.Sign(Vector3.Cross(linePoints[0] - linePoints[1], linePoints[1] - tranglePoints[trangleLineIndex2]).y);
                if (lineSign1 * lineSign2 == -1)
                {
                    //使用点积求投影根据投影比例求出交点位置
                    Vector3 lineNomal = Vector3.Cross(linePoints[1] - linePoints[0], Vector3.up);
                    float topDot = Vector3.Dot(linePoints[0] - tranglePoints[trangleLineIndex1], lineNomal);
                    Vector3 dir = tranglePoints[trangleLineIndex2] - tranglePoints[trangleLineIndex1];
                    float aScale = Mathf.Clamp01(topDot / Vector3.Dot(dir, lineNomal));
                    intersectPointCount++;
                    if (intersectPointCount == 1)
                    {
                        pos_a = tranglePoints[trangleLineIndex1] + dir * aScale;
                    }
                    else
                    {
                        pos_b = tranglePoints[trangleLineIndex1] + dir * aScale;
                    }
                    isIntersect = true;
                    continue;
                }
            }
        }
        //只有一个交点时，说明有一个端点在三角形内
        //使用三条边对两点进行叉积查看是否在内侧，求出哪个端点在三角形内
        if (intersectPointCount == 1)
        {
            if (sign1[0] == sign1[1] && sign1[1] == sign1[2])
            {
                pos_b = linePoints[0];
            }
            else if (sign2[0] == sign2[1] && sign2[1] == sign2[2])
            {
                pos_b = linePoints[1];
            }
        }
        return isIntersect;
    }
}
