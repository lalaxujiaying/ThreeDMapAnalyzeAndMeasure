using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class InputConfig
{
	public enum KeyState
	{
		Down,
		Up,
		Keep
	}
	public KeyState keyState = KeyState.Down;
	public KeyCode keyCode;

	public bool GetKeyInput()
	{
		switch (keyState)
		{
			case KeyState.Down:
				return Input.GetKeyDown(keyCode);
			case KeyState.Up:
				return Input.GetKeyUp(keyCode);
			case KeyState.Keep:
				return Input.GetKey(keyCode);
		}
		return false;
	}
}
