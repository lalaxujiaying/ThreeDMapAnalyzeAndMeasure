using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 直线模型
/// </summary>
public class StraightLine
{
    private (Vector2, Vector2) lineExpression;
    private Vector2 ab;
    private float abLength;
    public StraightLine(Vector2 aPoint, Vector2 bPoint)
    {
        lineExpression = (aPoint, bPoint - aPoint);
        ab = lineExpression.Item2 - lineExpression.Item1;
        abLength = Vector2.SqrMagnitude(ab);
    }
    public bool PointOnLine(Vector2 pPoint, float distance = 0)
    {
        return Distance(pPoint) <= distance;
    }
    public bool PointOnLineSegment(Vector2 pPoint, float distance = 0)
    {
        Vector2 ap = pPoint - lineExpression.Item1;
        float apLength = Vector2.SqrMagnitude(ap);
        float dotValue = Vector2.Dot(ap, ab);
        return dotValue >= 0 && dotValue <= apLength * abLength && Distance(pPoint) <= distance;
    }
    public float Distance(Vector2 pPoint)
    {
        Vector2 ap = pPoint - lineExpression.Item1;
        return Mathf.Abs(ap.Cross(ab)) / abLength;
    }
    /// <summary>
    /// 计算点是否在线段的左边
    /// </summary>
    /// <param name="pPoint"></param>
    /// <returns></returns>
    public bool LeftOfTheLine(Vector2 pPoint)
    {
        Vector2 ap = pPoint - lineExpression.Item1;
        return ap.Cross(ab) < 0;
    }
}
public static class MathfEx
{
    public static float Cross(this Vector2 v1, Vector2 v2)
    {
        // return Cross(Vector3,Vector3).z;
        return v1.x * v2.y - v1.y * v2.x;
    }
    public static bool CheckVecter3CompFromShpere(Vector3 vec_1, Vector3 vec_2, float closeValue = 0.1f)
    {
        if (closeValue < 0) return false;
        if (Vector3.SqrMagnitude(vec_1 - vec_2) <= closeValue * closeValue) return true;
        else return false;
    }
    /// <summary>
    /// 判断两个点是否相同
    /// </summary>
    /// <param name="sourcePoint"></param>
    /// <param name="dstPoint"></param>
    /// <param name="similarity"></param>
    /// <returns></returns>
    public static bool TwoPointApproximately(Vector3 sourcePoint, Vector3 dstPoint, float similarity = 0.01f)
    {
        if (sourcePoint.x < dstPoint.x + similarity && sourcePoint.x > dstPoint.x - similarity)
        {
            if (sourcePoint.y < dstPoint.y + similarity && sourcePoint.y > dstPoint.y - similarity)
            {
                if (sourcePoint.z < dstPoint.z + similarity && sourcePoint.z > dstPoint.z - similarity)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }
        else return false;
    }
}
