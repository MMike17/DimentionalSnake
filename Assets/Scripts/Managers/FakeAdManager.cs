using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Classed used to pop fade ads on screen</summary>
public class FakeAdManager : BaseBehaviour
{
	[Header("Scene references")]
	public GameObject fakeAdPanel;
	public Button closeButton;

	public void Init()
	{
		InitInternal();
	}

	public void PopAd(Action onAdDone)
	{
		if(!CheckInitialized())
			return;

		fakeAdPanel.gameObject.SetActive(true);

		closeButton.onClick.RemoveAllListeners();
		closeButton.onClick.AddListener(() =>
		{
			if(onAdDone != null)
				onAdDone();

			fakeAdPanel.gameObject.SetActive(false);
		});
	}
}