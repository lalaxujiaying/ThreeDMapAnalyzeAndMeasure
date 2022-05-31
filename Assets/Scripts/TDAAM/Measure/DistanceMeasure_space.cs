using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TDAAM.Measure
{
    public class DistanceMeasure_space : MonoBehaviour
    {
        public event Action OnMeasureCompleted;
        public bool isShowUI;
        public bool isCreate = false;

        public NorthDir northDir;
        private List<Vector3> mousePoints = new List<Vector3>();
        public LineRenderer lineRenderer { get; private set; }
        public LineRenderer tempLineRenderer { get; private set; }

        public float lineSize = 0.2f;
        public Color lineColor_Temp;
        public Color lineColor_Confirm;
        public Color uiColor_Temp;
        public Color uiColor_Confirm;

        private bool isMeasureCompleted = false;
        private float doubleClickTimer;
        private float doubleClickTime = 0.3f;
        private Vector3 northVector = Vector3.zero;
        public Texture2D pointT2d;
        private bool isStart = false;

        public int showUISize = 30;
        public void Init()
        {
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            CreateLineMaterial();
            lineRenderer.startWidth = lineSize;
            CreateChildLineRenderer();
            lineRenderer.material = lineMaterial;
            lineMaterial.color = lineColor_Confirm;
            SetNorthDir();
            isStart = true;
        }

        void Update()
        {
            if (isStart)
            {
                VertexSnapping();
                MouseInput();
            }
        }
        bool isSnappingVertex = false;
        private void VertexSnapping()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (!isSnappingVertex) isSnappingVertex = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                isSnappingVertex = false;
            }

        }
        private void Reset()
        {
            mousePoints.Clear();
            distanceTwoPoints.Clear();
            tempPoint = Vector3.zero;
            tempDistance = 0;
            TotalLength = 0;
        }

        private void CreateChildLineRenderer()
        {
            GameObject childGo = new GameObject("tempLine");
            childGo.transform.parent = transform;
            tempLineRenderer = childGo.AddComponent<LineRenderer>();
            tempLineRenderer.startWidth = lineSize;
            var childLineMaterial = Instantiate(lineMaterial);
            tempLineRenderer.material = childLineMaterial;
            childLineMaterial.color = lineColor_Temp;

        }
        private Vector3 downClickPoint;

        [SerializeField]
        private List<float> distanceTwoPoints = new List<float>();
        private List<float> dirAngle = new List<float>();
        Vector3 tempPoint;
        float tempDistance;
        [SerializeField]
        float TotalLength;
        float tempDirAngle;
        private void MouseInput()
        {
            if (isMeasureCompleted) return;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit mouseHit))
            {
                if (isSnappingVertex)
                {
                    Mesh mesh = mouseHit.collider.GetComponent<MeshFilter>().mesh;
                    Vector3[] triangVerts = new Vector3[3];
                    try
                    {
                        triangVerts[0] = mouseHit.collider.transform.TransformPoint(mesh.vertices[mesh.triangles[mouseHit.triangleIndex * 3]]);
                        triangVerts[1] = mouseHit.collider.transform.TransformPoint(mesh.vertices[mesh.triangles[mouseHit.triangleIndex * 3 + 1]]);
                        triangVerts[2] = mouseHit.collider.transform.TransformPoint(mesh.vertices[mesh.triangles[mouseHit.triangleIndex * 3 + 2]]);

                    }
                    catch
                    {
                        Debug.Log(mouseHit.triangleIndex);
                    }



                    tempPoint = CheckClosePoint(mouseHit.point, triangVerts);

                }
                else tempPoint = mouseHit.point;
                if (mousePoints.Count > 0)
                {
                    tempDistance = Vector3.Distance(mousePoints[mousePoints.Count - 1], tempPoint);
                    var tempdir = new Vector3(tempPoint.x - mousePoints[mousePoints.Count - 1].x, 0, tempPoint.z - mousePoints[mousePoints.Count - 1].z);
                    tempDirAngle = Vector3.Angle(northVector, tempdir);
                    if (Vector3.Cross(northVector, tempdir).y < 0)
                    {
                        tempDirAngle = 360 - tempDirAngle;
                    }
                }
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    downClickPoint = tempPoint;
                }
                else if (Input.GetMouseButtonUp(0) || isCreate)
                {
                    if (!MathfEx.CheckVecter3CompFromShpere(downClickPoint, tempPoint, 0.1f) && !isCreate) return;
                    if (doubleClickTimer > doubleClickTime || isCreate)
                    {
                        //单击
                        isCreate = false;
                        if (mousePoints.Count > 0)
                        {
                            TotalLength += tempDistance;
                            distanceTwoPoints.Add(TotalLength);
                            dirAngle.Add(tempDirAngle);
                        }
                        mousePoints.Add(tempPoint);
                        lineRenderer.positionCount = mousePoints.Count;
                        lineRenderer.SetPosition(mousePoints.Count - 1, tempPoint);
                    }
                    else if (tempPoint == mousePoints[mousePoints.Count - 1])
                    {
                        //双击
                        OnDoubleClick();
                        return;
                    }
                    else
                    {
                        //单击
                        if (mousePoints.Count > 0)
                        {
                            TotalLength += tempDistance;
                            distanceTwoPoints.Add(TotalLength);
                            dirAngle.Add(tempDirAngle);
                        }
                        mousePoints.Add(tempPoint);
                        lineRenderer.positionCount = mousePoints.Count;
                        lineRenderer.SetPosition(mousePoints.Count - 1, tempPoint);
                    }
                    doubleClickTimer = 0;

                }
                else if (Input.GetMouseButtonUp(1))
                {
                    if (!MathfEx.CheckVecter3CompFromShpere(downClickPoint, tempPoint, 0.1f)) return;
                    if (mousePoints.Count != 0)
                    {
                        mousePoints.RemoveAt(mousePoints.Count - 1);
                    }
                    if (distanceTwoPoints.Count != 0)
                    {
                        if (distanceTwoPoints.Count == 1)
                        {
                            TotalLength = 0;
                        }
                        else TotalLength -= (distanceTwoPoints[distanceTwoPoints.Count - 1] - distanceTwoPoints[distanceTwoPoints.Count - 2]);
                        distanceTwoPoints.RemoveAt(distanceTwoPoints.Count - 1);
                    }
                    if (dirAngle.Count != 0)
                    {
                        dirAngle.RemoveAt(dirAngle.Count - 1);
                    }
                    lineRenderer.positionCount = mousePoints.Count;
                }
                if (mousePoints.Count > 0)
                {
                    tempLineRenderer.positionCount = 2;
                    tempLineRenderer.SetPosition(0, mousePoints[mousePoints.Count - 1]);
                    tempLineRenderer.SetPosition(1, tempPoint);
                }
                else tempLineRenderer.positionCount = 0;

                if (doubleClickTimer > 1) doubleClickTimer = 0.3f;
                doubleClickTimer += Time.deltaTime;
            }
        }
        private bool Vector3Comp(Vector3 vec_1, Vector3 vec_2, float similarLevel = 0.01f)
        {
            if (vec_1.x < vec_2.x + similarLevel && vec_1.x > vec_2.x - similarLevel)
            {
                if (vec_1.y < vec_2.y + similarLevel && vec_1.y > vec_2.y - similarLevel)
                {
                    if (vec_1.z < vec_2.z + similarLevel && vec_1.z > vec_2.z - similarLevel)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }
        private Vector3 CheckClosePoint(Vector3 centerPoint, params Vector3[] closePoints)
        {
            float minDistance = Vector3.SqrMagnitude(centerPoint - closePoints[0]);
            float tempDistance;
            int minIndex = 0;
            for (int i = 1; i < closePoints.Length; i++)
            {
                tempDistance = Vector3.SqrMagnitude(centerPoint - closePoints[i]);
                if (minDistance > tempDistance)
                {
                    minDistance = tempDistance;
                    minIndex = i;
                }
            }
            return closePoints[minIndex];
        }

        private void SetNorthDir()
        {
            switch (northDir)
            {
                case NorthDir.Z_Axis:
                    northVector = Vector3.forward;
                    break;
                case NorthDir.X_Axis:
                    northVector = Vector3.right;
                    break;
                case NorthDir.Reverse_Z_Axis:
                    northVector = -Vector3.forward;
                    break;
                case NorthDir.Reverse_X_Axis:
                    northVector = -Vector3.right;
                    break;

            }
        }
        private Material lineMaterial;
        private void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Line/LineRendererShader");
                lineMaterial = new Material(shader);
            }
        }
        void OnGUI()
        {
            if (!isShowUI) return;
            if (mousePoints.Count > 0)
            {
                GUIStyle fontStyle = new GUIStyle();
                fontStyle.normal.textColor = uiColor_Temp;
                fontStyle.fontSize = showUISize;
                Vector3 screenPoint;
                Rect rect;
                //画最新两点的长度和方位角
                if (!isMeasureCompleted)
                {
                    screenPoint = Camera.main.WorldToScreenPoint((tempPoint + mousePoints[mousePoints.Count - 1]) / 2);
                    rect = new Rect(new Vector2(screenPoint.x, Screen.height - screenPoint.y - fontStyle.fontSize), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
                    GUI.Label(rect, tempDistance.ToString("f2") + "米", fontStyle);

                    screenPoint = Camera.main.WorldToScreenPoint((tempPoint + mousePoints[mousePoints.Count - 1]) / 2);
                    rect = new Rect(new Vector2(screenPoint.x, Screen.height - screenPoint.y - fontStyle.fontSize * 2), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
                    GUI.Label(rect, "方位角：" + tempDirAngle.ToString("f2") + "°", fontStyle);
                }
                //画最终总长
                if (isMeasureCompleted) fontStyle.normal.textColor = uiColor_Confirm;
                screenPoint = Camera.main.WorldToScreenPoint(tempPoint);
                rect = new Rect(new Vector2(screenPoint.x, Screen.height - screenPoint.y - fontStyle.fontSize), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
                GUI.Label(rect, "空间总长: " + (TotalLength + tempDistance).ToString("f2") + "米", fontStyle);

                fontStyle.normal.textColor = uiColor_Confirm;

                screenPoint = Camera.main.WorldToScreenPoint(mousePoints[0]);
                rect = new Rect(new Vector2(screenPoint.x, Screen.height - screenPoint.y - fontStyle.fontSize), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
                GUI.Label(rect, "起点", fontStyle);
                //画总长
                for (int i = 0; i < distanceTwoPoints.Count; i++)
                {
                    if (isMeasureCompleted && i == distanceTwoPoints.Count - 1)
                    {
                        break;
                    }
                    screenPoint = Camera.main.WorldToScreenPoint(mousePoints[i + 1]);
                    rect = new Rect(new Vector2(screenPoint.x, Screen.height - screenPoint.y - fontStyle.fontSize), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
                    GUI.Label(rect, distanceTwoPoints[i].ToString("f2") + "米", fontStyle);
                }
                //画方位角
                for (int i = 0; i < dirAngle.Count; i++)
                {
                    screenPoint = Camera.main.WorldToScreenPoint((mousePoints[i] + mousePoints[i + 1]) / 2);
                    rect = new Rect(new Vector2(screenPoint.x, Screen.height - screenPoint.y - fontStyle.fontSize), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
                    GUI.Label(rect, "方位角：" + dirAngle[i].ToString("f2") + "°", fontStyle);
                }

                //画点的UI
                for (int i = 0; i < mousePoints.Count; i++)
                {
                    screenPoint = Camera.main.WorldToScreenPoint(mousePoints[i]);
                    rect = new Rect(new Vector2(screenPoint.x - fontStyle.fontSize / 2, Screen.height - screenPoint.y - fontStyle.fontSize / 2), new Vector2(fontStyle.fontSize, fontStyle.fontSize));
                    GUI.Label(rect, pointT2d, fontStyle);
                }
            }
        }
        public void OnDoubleClick()
        {
            //这一步为了设置最后一下就是最后点击鼠标的地方
            tempPoint = mousePoints[mousePoints.Count - 1];
            tempDistance = 0;

            tempLineRenderer.positionCount = 0;
            isMeasureCompleted = true;
            OnMeasureCompleted?.Invoke();
        }
    }
}