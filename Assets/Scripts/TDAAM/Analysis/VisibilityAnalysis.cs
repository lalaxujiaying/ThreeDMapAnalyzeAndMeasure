using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDAAM.Analysis
{
    public class VisibilityAnalysis : MonoBehaviour
    {

        private Vector3 fisrtPoint = Vector3.zero;
        private Vector3 secondPoint = Vector3.zero;

        private bool isFirstClick = false;
        private bool isAanalysisCompleted = false;
        private void Start()
        {
            isFirstClick = true;
        }
        void Update()
        {

            VisibilityAnalysisStart();

        }
        private void VisibilityAnalysisStart()
        {
            if (isAanalysisCompleted) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) && isFirstClick)
                {
                    fisrtPoint = hit.point;
                    isFirstClick = false;
                }
                else if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && !isFirstClick)
                {
                    if (Physics.Linecast(fisrtPoint, hit.point, out RaycastHit raycastHit))
                    {
                        if (MathfEx.TwoPointApproximately(hit.point, raycastHit.point))
                        {
                            Debug.Log("中间连通");
                            Debug.DrawLine(fisrtPoint, hit.point, Color.green, 200f);
                        }
                        else
                        {
                            Debug.Log("中间不连通");
                            Debug.DrawLine(fisrtPoint, raycastHit.point, Color.green, 200f);
                            Debug.DrawLine(raycastHit.point, hit.point, Color.red, 200f);
                        }
                    }
                    else
                    {
                        Debug.Log("中间连通");
                        Debug.DrawLine(fisrtPoint, hit.point, Color.green, 200f);
                    }
                }
            }
        }
        private void ShowLine(Vector3 sourcePoint, Vector3 dstPoint, Color color, float lineSize)
        {

        }
    }
}
