using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CamMove : MonoBehaviour
{
	[SerializeField]
	private float keyBoardSpeed = 1500;
    [SerializeField]
    private float viewSpeed = 2000;
	[SerializeField]
	private float scrollSpeed = 2000;
    private Vector2 mouseInput;
    private Vector2 dirInput;
	private float moveSpeed;
	private enum MoveState
	{
		mouseScroll,
		keyBoard
	}
	private MoveState moveState = MoveState.mouseScroll;

	private void Update()
	{
        SetInputData();
        transform.localPosition += transform.localRotation * new Vector3(dirInput.x, 0f, dirInput.y) * moveSpeed * Time.deltaTime;
        if(Input.GetMouseButton(1))
		{
			moveState = MoveState.keyBoard;
			transform.Rotate(Vector3.up, mouseInput.x * Time.deltaTime * viewSpeed, Space.World);
			transform.Rotate(Vector3.right, -mouseInput.y * Time.deltaTime * viewSpeed,Space.Self);
		}
		else if(Input.GetMouseButtonUp(1))
		{
			moveState = MoveState.mouseScroll;
		}
		if (Input.GetMouseButton(2))
		{
			transform.Translate( Quaternion.Euler(0,transform.localEulerAngles.y,0)* new Vector3(-mouseInput.x, 0, -mouseInput.y) * scrollSpeed *  2 * Time.deltaTime, Space.World);
		}
	}
    private void SetInputData()
	{
		mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		if(moveState == MoveState.keyBoard)
		{
			dirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
			moveSpeed = keyBoardSpeed;
		}
		else if(moveState == MoveState.mouseScroll)
		{
			dirInput = new Vector2(0, Input.GetAxis("Mouse ScrollWheel")).normalized;
			moveSpeed = scrollSpeed;
		}

	}
}
