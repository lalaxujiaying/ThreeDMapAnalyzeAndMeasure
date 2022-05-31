using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace TDAAM.Tool.Editor
{
    public class MeasureWindow
    {
        DistanceMeasureWindow dsmWindow = new DistanceMeasureWindow();
        ITDAAM_Window currentWindow;
        public void OnDraw()
        {
            if (currentWindow == null)
            {
                if (GUILayout.Button("距离测量"))
                {
                    currentWindow = dsmWindow;
                    currentWindow.OnQuitAction += ResetTools;
                }
            }
            currentWindow?.OnDraw();
        }
        private void ResetTools()
        {
            currentWindow.OnQuitAction -= ResetTools;
            currentWindow = null;
        }
    }
}