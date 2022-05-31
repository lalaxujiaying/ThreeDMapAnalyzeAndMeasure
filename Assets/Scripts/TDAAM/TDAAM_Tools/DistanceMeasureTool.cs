using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TDAAM.Measure;
using TDAAM.Measure.Manager;
namespace TDAAM.Tool
{
    public class DistanceMeasureTool : TDAAM_Tool<DistanceMeasureTool>
    {
        private Dictionary<DistanceMeasureMode, Dictionary<string, ToolState>> toolBuffer = new Dictionary<DistanceMeasureMode, Dictionary<string, ToolState>>()
        {
            { DistanceMeasureMode.surface, new Dictionary<string, ToolState>() },
            { DistanceMeasureMode.space, new Dictionary<string, ToolState>() },
            //{ DistanceMeasureMode.投影模式, new Dictionary<string, ToolState>() },
        };

        public bool GetState(DistanceMeasureMode mode, string toolName, out ToolState toolState)
        {
            return toolBuffer[mode].TryGetValue(toolName, out toolState);
        }
        public string[] GetToolNames(DistanceMeasureMode mode)
        {
            return toolBuffer[mode].Keys.ToArray();
        }
        public bool Create(string toolName, NorthDir northDir, Color uiColor_Temp,
            Color uiColor_Confirm, Color lineColor_Temp, Color lineColor_Confirm,
            DistanceMeasureMode mode = DistanceMeasureMode.surface, float lineWidth = 1f, int uiSize = 20,
            Texture2D vertexPointTexture = null, bool isShowDefaultUI = false)
        {
            if (toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;
            GameObject go = null;
            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    DistanceMeasure_surfaceManage surfaceManage = TDAAM_Mono<DistanceMeasure_surfaceManage>.Create("[DistanceMeasureSurface]", toolName, out go);
                    surfaceManage.SetConfig(vertexPointTexture, northDir, lineColor_Temp, lineColor_Confirm, lineWidth, uiSize, isShowDefaultUI, uiColor_Temp, uiColor_Confirm);
                    toolBuffer[mode].Add(toolName, new ToolState(go, false, false, true));
                    break;
                case DistanceMeasureMode.space:

                    DistanceMeasure_spaceManage dmsManager = TDAAM_Mono<DistanceMeasure_spaceManage>.Create("[DistanceMeasureSpace]", toolName, out go);
                    dmsManager.SetConfig(vertexPointTexture, northDir, lineColor_Temp, lineColor_Confirm, lineWidth, uiSize, isShowDefaultUI, uiColor_Temp, uiColor_Confirm);
                    toolBuffer[mode].Add(toolName, new ToolState(go, false, false, true));
                    break;
                //case DistanceMeasureMode.投影模式:
                //    break;
                default:
                    break;
            }
            return true;
        }

        public bool Create(string toolName, DistanceMeasureMode mode = DistanceMeasureMode.surface,
            float lineWidth = 1f, int uiSize = 20, Texture2D vertexPointTexture = null, bool isShowDefaultUI = true)
        {
            return Create(toolName, NorthDir.Z_Axis, Color.red, Color.black, Color.green, Color.yellow, mode, lineWidth, uiSize, vertexPointTexture, isShowDefaultUI);

        }
        public bool Continue(string toolName, DistanceMeasureMode mode = DistanceMeasureMode.surface)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;
            if (!toolState.isStop || !toolState.isStart) return false;
            toolState.isStop = false;
            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    toolState.toolGo.GetComponent<DistanceMeasure_surfaceManage>().isStop = false;
                    break;
                case DistanceMeasureMode.space:
                    toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>().isStop = false;
                    break;
                //case DistanceMeasureMode.投影模式:
                //    break;
                default:
                    break;
            }
            return true;
        }
        public void Close(string toolName, DistanceMeasureMode mode = DistanceMeasureMode.surface)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return;
            if (!toolState.isStart) return;
            toolState.isStart = false;

            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    UnityEngine.Object.Destroy(toolState.toolGo);
                    toolBuffer[mode].Remove(toolName);
                    break;
                case DistanceMeasureMode.space:
                    UnityEngine.Object.Destroy(toolState.toolGo);
                    toolBuffer[mode].Remove(toolName);
                    break;
                //case DistanceMeasureMode.投影模式:
                //    break;
                default:
                    break;
            }
        }
        //public void Clear(string toolName, DistanceMeasureMode mode = DistanceMeasureMode.表面模式)
        //{
        //    if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return;
        //    if (!toolState.isStart) return;
        //    //if (toolState.isStop) Continue(toolName, mode);
        //    //if (toolState.isHide) Show(toolName, mode);

        //    switch (mode)
        //    {
        //        case DistanceMeasureMode.表面模式:
        //            toolState.toolGo.GetComponent<DistanceMeasure_surfaceManage>().Reset();
        //            break;
        //        case DistanceMeasureMode.空间模式:
        //            toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>().Reset();
        //            break;
        //        //case DistanceMeasureMode.投影模式:
        //        //    break;
        //        default:
        //            break;
        //    }
        //}
        public bool Hide(string toolName, DistanceMeasureMode mode = DistanceMeasureMode.surface)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;
            if (!toolState.isStop) Stop(toolName, mode);
            toolState.isHide = true;
            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    var surfaceManager = toolState.toolGo.GetComponent<DistanceMeasure_surfaceManage>();
                    var surfaceChild = surfaceManager.childScirpts;
                    for (int i = 0; i < surfaceChild.Count; i++) surfaceManager.HideChild(i);
                    break;
                case DistanceMeasureMode.space:
                    var spaceManager = toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>();
                    var spaceChild = spaceManager.childScirpts;
                    for (int i = 0; i < spaceChild.Count; i++) spaceManager.HideChild(i);
                    break;
                //case DistanceMeasureMode.投影模式:
                //    break;
                default:
                    break;
            }
            return true;
        }

        public bool Show(string toolName, DistanceMeasureMode mode = DistanceMeasureMode.surface)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;
            if (!toolState.isHide || !toolState.isStart) return false;
            toolState.isHide = false;

            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    var surfaceManager = toolState.toolGo.GetComponent<DistanceMeasure_surfaceManage>();
                    var surfaceChild = surfaceManager.childScirpts;
                    for (int i = 0; i < surfaceChild.Count; i++) surfaceManager.ShowChild(i);
                    break;
                case DistanceMeasureMode.space:
                    var spaceManager = toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>();
                    var spaceChild = spaceManager.childScirpts;
                    for (int i = 0; i < spaceChild.Count; i++) spaceManager.ShowChild(i);
                    break;
                //case DistanceMeasureMode.投影模式:
                //    break;
                default:
                    break;
            }
            return true;
        }

        public bool Stop(string toolName, DistanceMeasureMode mode = DistanceMeasureMode.surface)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;
            if (toolState.isStop || !toolState.isStart) return false;
            toolState.isStop = true;
            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    toolState.toolGo.GetComponent<DistanceMeasure_surfaceManage>().GetCurrentChild()?.OnDoubleClick();
                    toolState.toolGo.GetComponent<DistanceMeasure_surfaceManage>().isStop = true;
                    break;
                case DistanceMeasureMode.space:
                    toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>().GetCurrentChild()?.OnDoubleClick();
                    toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>().isStop = true;
                    break;
                //case DistanceMeasureMode.投影模式:
                //    break;
                default:
                    break;
            }
            return true;
        }
        public bool ChangedLineWidth(string toolName, DistanceMeasureMode mode, float lineWidth)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;

            switch (mode)
            {
                case DistanceMeasureMode.surface:

                    break;
                case DistanceMeasureMode.space:
                    var lineRenderers = toolState.toolGo.GetComponentsInChildren<LineRenderer>();
                    foreach (var line in lineRenderers) line.startWidth = lineWidth;
                    break;
                default:
                    break;
            }
            return true;
        }
        public bool ChangedLineColor(string toolName, DistanceMeasureMode mode, Color color)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;

            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    break;
                case DistanceMeasureMode.space:
                    var lineRenderers = toolState.toolGo.GetComponentsInChildren<LineRenderer>();
                    foreach (var line in lineRenderers) line.material.color = color;
                    break;
                default:
                    break;
            }

            return true;
        }
        public bool ChangedUISize(string toolName, DistanceMeasureMode mode, int uiSize)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;

            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    break;
                case DistanceMeasureMode.space:
                    var distances_space = toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>().GetComponentsInChildren<DistanceMeasure_space>();
                    foreach (var distance in distances_space) distance.showUISize = uiSize;
                    break;
                default:
                    break;
            }

            return true;
        }
        public bool ChangedUIColor(string toolName, DistanceMeasureMode mode, Color color)
        {
            if (!toolBuffer[mode].TryGetValue(toolName, out ToolState toolState)) return false;

            switch (mode)
            {
                case DistanceMeasureMode.surface:
                    break;
                case DistanceMeasureMode.space:
                    var distances_space = toolState.toolGo.GetComponent<DistanceMeasure_spaceManage>().GetComponentsInChildren<DistanceMeasure_space>();
                    foreach (var distance in distances_space) distance.uiColor_Confirm = color;
                    break;
                default:
                    break;
            }
            return true;
        }
    }

}
