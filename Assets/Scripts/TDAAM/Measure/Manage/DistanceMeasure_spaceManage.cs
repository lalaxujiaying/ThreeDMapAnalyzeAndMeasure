using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDAAM.Measure;
namespace TDAAM.Measure.Manager
{
    public class DistanceMeasure_spaceManage : TDAAM_Mono<DistanceMeasure_spaceManage>
    {
        #region Unity面板输入
        [SerializeField, Tooltip("端点UI")]
        private Texture2D pointT2d;
        /// <summary>
        /// 定义北方向
        /// </summary>
        [SerializeField, Tooltip("定义北方向")]
        private NorthDir northDir = NorthDir.Z_Axis;
        /// <summary>
        /// 画线大小
        /// </summary>
        [SerializeField, Range(0, 1f), Tooltip("画线大小")]
        private float lineSize = 0.2f;
        /// <summary>
        /// UI大小
        /// </summary>
        [SerializeField, Range(5, 50), Tooltip("UI大小")]
        private int showUISize = 30;
        /// <summary>
        /// 待确认线段的颜色
        /// </summary>
        [SerializeField, Tooltip("待确认线段的颜色")]
        private Color lineColor_Temp = Color.green;
        /// <summary>
        /// 确认线段的颜色
        /// </summary>
        [SerializeField, Tooltip("确认线段的颜色")]
        private Color lineColor_Confirm = Color.yellow;

        [SerializeField, Tooltip("待确认UI的颜色")]
        private Color uiColor_Temp = Color.red;

        [SerializeField, Tooltip("确认UI的颜色")]
        private Color uiColor_Confirm = Color.black;

        [SerializeField, Tooltip("是否显示UI")]
        private bool isShowUI = false;
        #endregion

        #region 内部字段
        public List<GameObject> childScirpts = new List<GameObject>();
        private Vector3 downClickPoint;
        private bool isCompleted = true;
        public bool isStop = false;
        #endregion
        void Update()
        {
            if (isStop) return;
            if (Input.GetMouseButtonDown(0))
            {
                downClickPoint = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0) && isCompleted)
            {
                if (!MathfEx.CheckVecter3CompFromShpere(downClickPoint, Input.mousePosition)) return;
                //if (downClickPoint != Input.mousePosition) return;
                isCompleted = false;
                CreateScript();
            }
        }
        public void Reset()
        {
            isCompleted = true;
            foreach (var childScirpt in childScirpts)
            {
                Destroy(childScirpt);
            }
            childScirpts.Clear();
        }
        public DistanceMeasure_space GetCurrentChild()
        {
            if (childScirpts.Count == 0) return null;
            return childScirpts[childScirpts.Count - 1].GetComponent<DistanceMeasure_space>();

        }
        private void CreateScript()
        {
            GameObject child = new GameObject();
            childScirpts.Add(child);
            child.transform.parent = transform;
            var script = child.AddComponent<DistanceMeasure_space>();
            script.OnMeasureCompleted += () =>
            {
                isCompleted = true;
            };
            script.pointT2d = pointT2d;
            script.northDir = northDir;
            script.lineColor_Temp = lineColor_Temp;
            script.lineColor_Confirm = lineColor_Confirm;
            script.lineSize = lineSize;
            script.isCreate = true;
            script.isShowUI = isShowUI;
            script.showUISize = showUISize;
            script.uiColor_Temp = uiColor_Temp;
            script.uiColor_Confirm = uiColor_Confirm;
            script.Init();
        }
        public void SetConfig(Texture2D t2d, NorthDir northDir,
            Color lineColor_Temp, Color lineColor_Confirm, float lineWidth, int uiSize, bool isShowUI, Color uiColor_Temp, Color uiColor_Confirm)
        {
            pointT2d = t2d;
            lineSize = lineWidth;
            showUISize = uiSize;
            this.isShowUI = isShowUI;
            this.northDir = northDir;
            this.lineColor_Temp = lineColor_Temp;
            this.lineColor_Confirm = lineColor_Confirm;
            this.uiColor_Confirm = uiColor_Confirm;
            this.uiColor_Temp = uiColor_Temp;
        }
        public void HideChild(int childIndex)
        {
            childScirpts[childIndex].SetActive(false);
        }
        public void ShowChild(int childIndex)
        {
            childScirpts[childIndex].SetActive(true);
        }
    }
}