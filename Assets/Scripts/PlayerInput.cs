using System;
using UnityEngine;

/// <summary>Class detecting and transmitting inputs</summary>
public class PlayerInput : MonoBehaviour
{
	[Header("Settings")]
	public KeyCode pcLeft;
	public KeyCode pcRight;

	Action<float> HorizontalInputEvent;
	float pcHorizontalInput;

	void Awake()
	{
		pcHorizontalInput = 0;
	}

	void Update()
	{
		if(HorizontalInputEvent == null)
			return;

		if(Application.isMobilePlatform)
		{
			if(Input.touchCount >= 1)
				HorizontalInputEvent(Input.GetTouch(0).position.x / Screen.width);
		}
		else // testing mode
		{
			if(Input.GetKey(pcLeft))
				pcHorizontalInput -= 0.1f * Time.deltaTime;

			if(Input.GetKey(pcRight))
				pcHorizontalInput += 0.1f * Time.deltaTime;

			HorizontalInputEvent(pcHorizontalInput);
		}
	}

	public void SubscribeHorizontalInputEvent(Action<float> callback)
	{
		if(callback != null)
			HorizontalInputEvent += callback;
	}
}