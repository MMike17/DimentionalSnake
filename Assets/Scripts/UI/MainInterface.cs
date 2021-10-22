using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Manages what is displayed on the interface during the main menu</summary>
[RequireComponent(typeof(Animator))]
public class MainInterface : BaseInterface
{
	const string FACEBOOK_URL = "https://www.facebook.com/ketchappgames";

	[Header("Settings")]
	public Sprite hasSoundSprite;
	public Sprite noSoundSprite;

	[Header("Scene references")]
	public Button shopButton;
	public Button facebookButton, stopSoundButton, playButton;
	public Image soundButtonPicto;

	bool hasSound;

	public void Init(bool initialSoundState, Action openShop, Action startGame, Action<bool> setSoundState)
	{
		hasSound = initialSoundState;

		soundButtonPicto.sprite = hasSound ? hasSoundSprite : noSoundSprite;
		setSoundState(hasSound);

		playButton.onClick.AddListener(() => startGame());
		shopButton.onClick.AddListener(() => openShop());
		facebookButton.onClick.AddListener(() => Application.OpenURL(FACEBOOK_URL));
		stopSoundButton.onClick.AddListener(() =>
		{
			hasSound = !hasSound;

			soundButtonPicto.sprite = hasSound ? hasSoundSprite : noSoundSprite;
			setSoundState(hasSound);
		});
	}
}