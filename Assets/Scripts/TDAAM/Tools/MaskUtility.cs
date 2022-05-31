using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class MaskUtility
{
    public static List<(T, bool)> GetMaskArray<T>(int maskValue, IEnumerable<T> maskObject)
    {
        List<(T, bool)> returnValue = new List<(T, bool)>();
        if (maskValue == -1)
        {
            foreach (var item in maskObject)
            {
                returnValue.Add((item, true));
            }
            return returnValue;
        }
        foreach (var item in maskObject)
        {
            if (maskValue % 2 == 1)
            {
                returnValue.Add((item, true));
            }
            else returnValue.Add((item, false));
            maskValue = maskValue >> 1;
        }
        return returnValue;
    }
}