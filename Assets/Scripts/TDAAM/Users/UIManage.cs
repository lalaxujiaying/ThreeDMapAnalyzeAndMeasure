using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDAAM.Analysis;
using UnityEngine.UI;
using TMPro;
using System;
using TDAAM.Analysis.Manager;
public class UIManage : MonoBehaviour
{
    [SerializeField]
    private GameObject CutAndFill_UI_Prefab;
    [SerializeField]
    private RectTransform CanvasRootTrans;
    [SerializeField]
    private CutAndFillManager scriptManager;

    private void Awake()
    {
        scriptManager.OnCreateAnalysisCompleted += (analysis) =>
        {
            analysis.OnCalcalateDataCompleted += (data) =>
            {
                ShowCutAndFillUI(data);
            };
            CutAndFill_UI_Prefab.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(
            () =>
            {
                analysis.BaseLevelHight = int.Parse(CutAndFill_UI_Prefab.transform.GetChild(4).GetComponentInChildren<TMP_InputField>().text);
            });
        };


    }
    public void ShowCutAndFillUI(CutAndFillData data)
    {
        //Debug.Log(data.maxLevelHight);
        //Debug.Log(data.minLevelHight);
        //Debug.Log(data.cutVolume);
        //Debug.Log(data.fillVolume);
        //Debug.Log(data.totalVolume);
        //Debug.Log(data.cutArea);
        //Debug.Log(data.fillArea);
        //Debug.Log(data.totalArea);
        //Debug.Log(data.totalArea);
        //Debug.Log(data.baseLevelHight);

        //var go = Instantiate(CutAndFill_UI_Prefab, CanvasRootTrans);
        TMP_Text[] output = CutAndFill_UI_Prefab.transform.GetChild(3).GetComponentsInChildren<TMP_Text>();
        Debug.Log(output.Length);
        output[0].text = data.maxLevelHight.ToString("F2") + "米";
        output[1].text = data.minLevelHight.ToString("F2") + "米";
        output[2].text = data.cutVolume.ToString("F2") + "立方米";
        output[3].text = data.fillVolume.ToString("F2") + "立方米";
        output[4].text = data.totalVolume.ToString("F2") + "立方米";
        output[5].text = data.cutArea.ToString("F2") + "平方米";
        output[6].text = data.fillArea.ToString("F2") + "平方米";
        output[7].text = data.totalArea.ToString("F2") + "平方米";
        output[8].text = data.totalArea.ToString("F2") + "平方米";
        var input = CutAndFill_UI_Prefab.transform.GetChild(4).GetComponentInChildren<TMP_InputField>();
        input.text = data.baseLevelHight.ToString("F2");

    }
    public void InputCutAndFillUI()
    {

    }
}
