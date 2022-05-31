using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDAAM;
public class TestTDAAMTools : MonoBehaviour
{
    private void Start()
    {
        AnalyzeAndMeasureTools.CutAndFillAnalysis.Create("TestApi");

    }
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    AnalyzeAndMeasureTools.DistanceMeasure.Create("TestApi", DistanceMeasureMode.space);
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    Debug.Log(AnalyzeAndMeasureTools.DistanceMeasure.ChangedUIColor("TestApi", DistanceMeasureMode.surface, Color.blue));
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    Debug.Log(AnalyzeAndMeasureTools.DistanceMeasure.ChangedLineColor("TestApi", DistanceMeasureMode.surface, Color.green));
        //}
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    Debug.Log(AnalyzeAndMeasureTools.DistanceMeasure.ChangedLineWidth("TestApi", DistanceMeasureMode.surface, 0.5f));
        //}
    }
}
