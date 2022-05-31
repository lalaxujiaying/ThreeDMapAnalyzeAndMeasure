using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace TDAAM.Tool.Editor
{
    public class TDAAM_Window : EditorWindow
    {
        [MenuItem("TDAAMTools/TDAAM_Window")]
        public static void OpenWindow()
        {
            GetWindow<TDAAM_Window>().Show();
        }
        //[MenuItem("TDAAMTools/TDAAM_Window",validate = true)]
        //public static bool OpenWindowVerify()
        //{
        //	return Application.isPlaying;
        //}
        private enum TDAAMMode
        {
            Measure,
            Analysis
        }
        private TDAAMMode currentMode;

        //private DistanceMeasureWindow distanceMeasureWindow = new DistanceMeasureWindow();
        private MeasureWindow measureWindow = new MeasureWindow();
        private AnalyzeWindow analyzeWindow = new AnalyzeWindow();
        private void OnGUI()
        {
            currentMode = (TDAAMMode)GUILayout.Toolbar((int)currentMode, Enum.GetNames(typeof(TDAAMMode)));

            //if (!Application.isPlaying)
            //{
            //    EditorGUILayout.HelpBox("需要运行", MessageType.Warning);
            //}
            //EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            {
                switch (currentMode)
                {
                    case TDAAMMode.Measure:
                        measureWindow.OnDraw();
                        break;
                    case TDAAMMode.Analysis:
                        analyzeWindow.OnDraw();
                        break;
                }
            }
            //EditorGUI.EndDisabledGroup();

        }
    }
}