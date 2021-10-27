using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays snake settings in shop</summary>
public class SettingsTicket : BaseBehaviour
{
	[Header("Scene references")]
	public Image snapshot;
	public TextMeshProUGUI settingsName;
	public GameObject selectedPicto, lockedPanel;
	public Button buyOrSelectButton;

	public void Init(SnakePieces settings, bool isUnlocked, bool isSelected, float width, Action buySettings, Action selectSettings, Func<bool> canBuy)
	{
		snapshot.sprite = settings.shopSnapshot;
		settingsName.text = settings.settingsName;

		lockedPanel.SetActive(!isUnlocked);
		selectedPicto.SetActive(isSelected);

		buyOrSelectButton.onClick.AddListener(() =>
		{
			if(lockedPanel.gameObject.activeSelf)
			{
				if(canBuy())
				{
					buySettings();

					lockedPanel.SetActive(false);
					selectedPicto.SetActive(true);
				}
			}
			else
			{
				selectSettings();
				selectedPicto.SetActive(true);
			}
		});

		GetComponent<RectTransform>().sizeDelta = new Vector2(width, width + 50);

		InitInternal();
	}

	public void Unselect()
	{
		if(!CheckInitialized())
			return;

		selectedPicto.SetActive(false);
	}
}