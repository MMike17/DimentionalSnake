using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Manages background switching for bonus levels</summary>
public class BackgroundManager : BaseBehaviour
{
	[Header("Settings")]
	public int memorySize;

	[Header("Scene references")]
	public Sprite[] backgrounds;
	public Image backgroundUI;

	List<int> lastSelectedBGs;

	public void Init()
	{
		lastSelectedBGs = new List<int>();

		InitInternal();
	}

	public void PickNewBackground()
	{
		if(!CheckInitialized())
			return;

		// pick index
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < backgrounds.Length; i++)
			availableIndexes.Add(i);

		lastSelectedBGs.ForEach(item => availableIndexes.Remove(item));

		int index = availableIndexes[Random.Range(0, availableIndexes.Count)];
		lastSelectedBGs.Add(index);

		// keep memory size consistent
		if(lastSelectedBGs.Count > memorySize)
			lastSelectedBGs.RemoveAt(0);

		// set background
		backgroundUI.sprite = backgrounds[index];
	}
}