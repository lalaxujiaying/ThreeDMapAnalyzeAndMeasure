using UnityEditor;
using UnityEngine;
using TDAAM.Tool;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TDAAM.Tool.Editor
{
    public class DistanceMeasureWindow : ITDAAM_Window
    {

        private static DistanceMeasureMode mode = DistanceMeasureMode.space;
        private DistanceMeasureMode tempMode = DistanceMeasureMode.space;
        private static float lineWidth = 0.2f;
        private static int uiSize = 20;
        private static bool isShowDefaultUI = true;
        private static string toolName = "default";
        private static List<string> drawLineNames = new List<string>();
        private bool isCreateSuccess = true;
        private static int showLinesMask = -1;
        private static ToolState currentSelectToolState;
        private static int currentSelectIndex = -1;
        private int lastSelectIndex = -1;

        public override void OnDraw()
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("退出"))
                {
                    OnQuitAction?.Invoke();
                }

                GUILayout.Label("空间距离测量工具");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(40);
            #region 工具设置
            GUILayout.BeginVertical(GUI.skin.box);
            {
                toolName = EditorGUILayout.TextField("名称", toolName);
                EditorGUI.BeginChangeCheck();
                {
                    tempMode = (DistanceMeasureMode)EditorGUILayout.EnumPopup("模式", mode);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (currentSelectIndex >= 0)
                        AnalyzeAndMeasureTools.DistanceMeasure.Stop(drawLineNames[currentSelectIndex], mode);
                    mode = tempMode;
                    drawLineNames = new List<string>(AnalyzeAndMeasureTools.DistanceMeasure.GetToolNames(mode));
                    showLinesMask |= 1 << drawLineNames.Count - 1;
                    currentSelectIndex = drawLineNames.Count - 1;
                    if (currentSelectIndex >= 0)
                        AnalyzeAndMeasureTools.DistanceMeasure.GetState(mode, drawLineNames[currentSelectIndex], out currentSelectToolState);
                }
                toolName = ObjectNames.GetUniqueName(drawLineNames.ToArray(), toolName);
                lineWidth = EditorGUILayout.FloatField("线段大小", lineWidth);
                uiSize = EditorGUILayout.IntField("UI大小", uiSize);
                isShowDefaultUI = EditorGUILayout.Toggle("是否使用默认UI", isShowDefaultUI);

            }
            GUILayout.EndVertical();
            #endregion
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(30);
                if (GUILayout.Button(" 创建 ", GUILayout.ExpandWidth(false)))
                {
                    if (currentSelectIndex >= 0) AnalyzeAndMeasureTools.DistanceMeasure.Stop(drawLineNames[currentSelectIndex], mode);
                    drawLineNames.Add(toolName);
                    isCreateSuccess = AnalyzeAndMeasureTools.DistanceMeasure.Create(toolName, mode, lineWidth: lineWidth, uiSize: uiSize, isShowDefaultUI: isShowDefaultUI);
                    showLinesMask |= 1 << drawLineNames.Count - 1;
                    currentSelectIndex = drawLineNames.Count - 1;
                    AnalyzeAndMeasureTools.DistanceMeasure.GetState(mode, drawLineNames[currentSelectIndex], out currentSelectToolState);
                }
                if (!isCreateSuccess)
                {
                    EditorGUILayout.HelpBox("重复命名", MessageType.Warning);
                }

                GUILayout.Space(30);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(40);
            GUILayout.BeginVertical();
            {
                //显示选择的线段
                EditorGUILayout.LabelField("选择要显示的工具：");
                EditorGUI.BeginChangeCheck();
                {
                    if (drawLineNames.Count > 0)
                        showLinesMask = EditorGUILayout.MaskField(showLinesMask, drawLineNames.ToArray());
                }
                if (EditorGUI.EndChangeCheck())
                {
                    var selectDrawLines = MaskUtility.GetMaskArray(showLinesMask, drawLineNames);
                    foreach (var drawLine in selectDrawLines)
                    {
                        if (drawLine.Item2)
                        {
                            AnalyzeAndMeasureTools.DistanceMeasure.Show(drawLine.Item1, mode);
                        }
                        else
                        {
                            AnalyzeAndMeasureTools.DistanceMeasure.Hide(drawLine.Item1, mode);
                        }
                    }
                }
                //修改选择的线段
                EditorGUILayout.LabelField("当前操作的工具：");
                EditorGUI.BeginChangeCheck();
                {
                    lastSelectIndex = currentSelectIndex;
                    currentSelectIndex = EditorGUILayout.Popup(currentSelectIndex, drawLineNames.ToArray());
                }
                if (EditorGUI.EndChangeCheck())
                {
                    AnalyzeAndMeasureTools.DistanceMeasure.GetState(mode, drawLineNames[currentSelectIndex], out currentSelectToolState);
                    AnalyzeAndMeasureTools.DistanceMeasure.Stop(drawLineNames[lastSelectIndex], mode);
                    AnalyzeAndMeasureTools.DistanceMeasure.Continue(drawLineNames[currentSelectIndex], mode);
                }
            }
            GUILayout.EndVertical();
            GUILayout.Space(30);
            //显示选择的线段可使用的操作工具
            GUILayout.BeginVertical();
            {
                if (currentSelectToolState != null)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    {

                        if (!currentSelectToolState.isStop)
                        {
                            GUILayout.Space(30);
                            if (GUILayout.Button(" 锁定 ", GUILayout.ExpandWidth(false)) && !currentSelectToolState.isHide)
                            {
                                AnalyzeAndMeasureTools.DistanceMeasure.Stop(drawLineNames[currentSelectIndex], mode);
                            }
                        }
                        else
                        {
                            GUILayout.Space(30);
                            if (GUILayout.Button(" 解锁 ", GUILayout.ExpandWidth(false)) && !currentSelectToolState.isHide)
                            {
                                AnalyzeAndMeasureTools.DistanceMeasure.Continue(drawLineNames[currentSelectIndex], mode);
                            }
                        }
                        GUILayout.Space(10);
                        if (!currentSelectToolState.isHide)
                        {
                            if (GUILayout.Button(" 隐藏 ", GUILayout.ExpandWidth(false)))
                            {
                                currentSelectToolState.isHide =
                                    currentSelectToolState.isStop =
                                    AnalyzeAndMeasureTools.DistanceMeasure.Hide(drawLineNames[currentSelectIndex], mode);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(" 显示 ", GUILayout.ExpandWidth(false)))
                            {
                                AnalyzeAndMeasureTools.DistanceMeasure.Show(drawLineNames[currentSelectIndex], mode);
                            }
                        }
                        GUILayout.Space(10);
                        if (GUILayout.Button(" 删除 ", GUILayout.ExpandWidth(false)))
                        {
                            AnalyzeAndMeasureTools.DistanceMeasure.Close(drawLineNames[currentSelectIndex], mode);
                            drawLineNames = new List<string>(AnalyzeAndMeasureTools.DistanceMeasure.GetToolNames(mode));
                            currentSelectIndex = drawLineNames.Count - 1;
                            if (currentSelectIndex >= 0)
                                AnalyzeAndMeasureTools.DistanceMeasure.GetState(mode, drawLineNames[currentSelectIndex], out currentSelectToolState);
                            else currentSelectToolState = null;
                        }

                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }
    }
}