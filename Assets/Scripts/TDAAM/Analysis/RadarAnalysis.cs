using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarAnalysis : MonoBehaviour
{
	[SerializeField]
	private float hight = 2f;
	[SerializeField]
	private float radius = 20f;
    [SerializeField]
    private int OneCircleLineCount = 360;
    [SerializeField]
    private List<Vector4> circleLine = new List<Vector4>();
    private List<int> eachCircleIndex = new List<int>();
	private Vector3 centerPoint;
	private enum LineColor
	{
        Green,
        Red
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
			{
				centerPoint = hit.point + new Vector3(0, hight, 0);
				RadarAnalysis_Start(centerPoint, radius, OneCircleLineCount);
			}

		}

	}
	private void RadarAnalysis_Start(Vector3 centerPoint,float radius,int OneCircleLineCount)
	{
        float currentRadius = radius;
		//float eachDegree = 360f / OneCircleLineCount;
		float eachDegree = 1;//(360 / 360)
		float radiusSplit = 90f / (OneCircleLineCount / 2);
		for (int currentCircleLineIndex = 0; currentCircleLineIndex < OneCircleLineCount / 2; currentCircleLineIndex++)
		{
			//float hight = radius / (OneCircleLineCount / 2) * currentCircleLineIndex;
			float hight = radius * Mathf.Sin(radiusSplit * currentCircleLineIndex * Mathf.Deg2Rad);
			currentRadius = Mathf.Sqrt(radius * radius - hight * hight);
			//circleLine.Clear();
			for (int currentDegreeIndex = 0; currentDegreeIndex < 360; currentDegreeIndex++)
			{
				Vector3 offset = new Vector3(currentRadius * Mathf.Cos(currentDegreeIndex * eachDegree * Mathf.Deg2Rad), hight, currentRadius * Mathf.Sin(currentDegreeIndex * eachDegree * Mathf.Deg2Rad));
				Vector3 dstPoint = centerPoint + offset;
				if (Physics.Linecast(centerPoint, dstPoint, out RaycastHit hit))
				{
					if (dstPoint.x >= hit.point.x - 0.001f && dstPoint.x <= hit.point.x + 0.001f)
					{
						if (dstPoint.y >= hit.point.y - 0.001f && dstPoint.y <= hit.point.y + 0.001f)
						{
							if (dstPoint.z >= hit.point.z - 0.001f && dstPoint.z <= hit.point.z + 0.001f)
							{
								circleLine.Add(new Vector4(dstPoint.x, dstPoint.y, dstPoint.z, (int)LineColor.Green));
							}
							else
							{
								circleLine.Add(new Vector4(dstPoint.x, dstPoint.y, dstPoint.z, (int)LineColor.Red));
							}
						}
						else
						{
							circleLine.Add(new Vector4(dstPoint.x, dstPoint.y, dstPoint.z, (int)LineColor.Red));
						}
					}
					else
					{
						circleLine.Add(new Vector4(dstPoint.x, dstPoint.y, dstPoint.z, (int)LineColor.Red));
					}
				}
				else
				{
					circleLine.Add(new Vector4(dstPoint.x, dstPoint.y, dstPoint.z, (int)LineColor.Green));
				}
			}
			eachCircleIndex.Add(circleLine.Count);
		}
    }


    private  Material lineMaterial;
	private void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			Shader shader = Shader.Find("Unlit/GLColorShader");
			lineMaterial = new Material(shader);
		}
	}
	private void OnRenderObject()
	{
		CreateLineMaterial();
		// Apply the line material
		lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

		// Draw lines
		//GL.Begin(GL.LINES);
		for (int i = 0; i < eachCircleIndex.Count; ++i)
		{
			GL.Begin(GL.LINE_STRIP);
			if (i == 0)
			{
				for (int j = 0; j < eachCircleIndex[i] - 1; j = j + 2)
				{
					if (circleLine[j + 1].w == 1) GL.Color(Color.red);
					else GL.Color(Color.green);

					GL.Vertex3(circleLine[j].x, circleLine[j].y, circleLine[j].z);
					GL.Vertex3(circleLine[j + 1].x, circleLine[j + 1].y, circleLine[j + 1].z);
				}

				if (circleLine[0].w == 1) GL.Color(Color.red);
				else GL.Color(Color.green);

				//GL.Vertex3(circleLine[eachCircleIndex[i] - 1].x, circleLine[eachCircleIndex[i] - 1].y, circleLine[eachCircleIndex[i] - 1].z);
				GL.Vertex3(circleLine[0].x, circleLine[0].y, circleLine[0].z);
			}
			else
			{
				for (int j = eachCircleIndex[i - 1]; j < eachCircleIndex[i] - 1; j = j + 2)
				{
					if (circleLine[j + 1].w == 1) GL.Color(Color.red);
					else GL.Color(Color.green);

					GL.Vertex3(circleLine[j].x, circleLine[j].y, circleLine[j].z);
					GL.Vertex3(circleLine[j + 1].x, circleLine[j + 1].y, circleLine[j + 1].z);
				}
				if (circleLine[eachCircleIndex[i - 1]].w == 1) GL.Color(Color.red);
				else GL.Color(Color.green);

				//GL.Vertex3(circleLine[eachCircleIndex[i] - 1].x, circleLine[eachCircleIndex[i] - 1].y, circleLine[eachCircleIndex[i] - 1].z);
				GL.Vertex3(circleLine[eachCircleIndex[i - 1]].x, circleLine[eachCircleIndex[i - 1]].y, circleLine[eachCircleIndex[i - 1]].z);
			}
			GL.End();

		}
		
		for (int i = 0; i < 360; i++)
		{
			GL.Begin(GL.LINE_STRIP);
			for (int j = 0; j < eachCircleIndex.Count; j++)
			{
				if (circleLine[j * 360 + i].w == 1) GL.Color(Color.red);
				else GL.Color(Color.green);

				GL.Vertex3(circleLine[j * 360 + i].x, circleLine[j * 360 + i].y, circleLine[j * 360 + i].z);
			}
			GL.Vertex3(centerPoint.x, centerPoint.y + radius, centerPoint.z);
			GL.End();
		}
		GL.PopMatrix();

    }
}
