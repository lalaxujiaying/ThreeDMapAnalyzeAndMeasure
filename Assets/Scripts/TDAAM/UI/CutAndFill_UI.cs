using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutAndFill_UI
{
    public void ShowUI(Transform father)
    {
        GameObject go = new GameObject("CutAndFill_Canvas");
        go.transform.parent = father;
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = Camera.main;
        //var canvas_rectTrans = canvas.GetComponent<RectTransform>();
        //canvas_rectTrans.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        //canvas_rectTrans.localScale = Vector3.one;

    }
    public void HideUI()
    {

    }
}
