using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDAAM
{
    public enum NorthDir
    {
        Z_Axis,
        X_Axis,
        Reverse_Z_Axis,
        Reverse_X_Axis
    }
    public enum DistanceMeasureMode
    {
        [InspectorName("表面模式")]
        surface,
        [InspectorName("空间模式")]
        space,
        //投影模式
    }
}

