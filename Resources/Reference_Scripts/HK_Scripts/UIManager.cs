using System;
using System.Collections;
using System.Reflection;
using GlobalEnums;
using InControl;
using Language;
using Modding.Utils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000321 RID: 801
public class UIManager : MonoBehaviour
{
	// Token: 0x17000216 RID: 534
	// (get) Token: 0x060011A7 RID: 4519 RVA: 0x00052965 File Offset: 0x00050B65
	public bool IsFadingMenu
	{
		get
		{
			return this.isFadingMenu || Time.time < this.startMenuTime;
		}
	}

	// Token: 0x17000217 RID: 535
	// (get) Token: 0x060011A8 RID: 4520 RVA: 0x0005297E File Offset: 0x00050B7E
	public bool IsAnimatingMenus
	{
		get
		{
			return this.menuAnimationCounter > 0;
		}
	}

	// Token: 0x17000218 RID: 536
	// (get) Token: 0x060011A9 RID: 4521 RVA: 0x0005298C File Offset: 0x00050B8C
	public static UIManager instance
	{
		get
		{
			if (UIManager._instance == null)
			{
				UIManager._instance = UnityEngine.Object.FindObjectOfType<UIManager>();
				if (UIManager._instance == null)
				{
					return null;
				}
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(UIManager._instance.gameObject);
				}
			}
			return UIManager._instance;
		}
	}

	// Token: 0x060011AA RID: 4522 RVA: 0x000529DA File Offset: 0x00050BDA
	private void Awake()
	{
		this.orig_Awake();
		if (UIManager._instance != this)
		{
			return;
		}
		Action editMenus = UIManager._editMenus;
		if (editMenus != null)
		{
			editMenus();
		}
		this.hasCalledEditMenus = true;
	}

	// Token: 0x060011AB RID: 4523 RVA: 0x00052A07 File Offset: 0x00050C07
	public void SceneInit()
	{
		if (this == UIManager._instance)
		{
			this.SetupRefs();
		}
	}

	// Token: 0x060011AC RID: 4524 RVA: 0x00052A1C File Offset: 0x00050C1C
	private void Start()
	{
		this.orig_Start();
		if (UIManager._instance != this)
		{
			return;
		}
		GameObject gameObject = base.transform.Find("UICanvas/MainMenuScreen/TeamCherryLogo/Hidden_Dreams_Logo").gameObject;
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, gameObject.transform.parent);
		gameObject2.SetActive(true);
		Vector3 position = gameObject2.transform.position;
		gameObject2.transform.position = position - new Vector3(0.4f, 0.1f, 0f);
		gameObject.transform.position = position + new Vector3(0.6f, 0f, 0f);
		gameObject2.transform.localScale *= 0.1f;
		gameObject2.GetComponent<SpriteRenderer>().sprite = this.LoadImage();
	}

	// Token: 0x060011AD RID: 4525 RVA: 0x00052AEC File Offset: 0x00050CEC
	public void SetState(UIState newState)
	{
		if (this.gm == null)
		{
			this.gm = GameManager.instance;
		}
		if (newState != this.uiState)
		{
			if (this.uiState == UIState.PAUSED && newState == UIState.PLAYING)
			{
				this.UIClosePauseMenu();
			}
			else if (this.uiState == UIState.PLAYING && newState == UIState.PAUSED)
			{
				this.UIGoToPauseMenu();
			}
			else if (newState == UIState.INACTIVE)
			{
				this.DisableScreens();
			}
			else if (newState == UIState.MAIN_MENU_HOME)
			{
				if (Platform.Current.EngagementState == Platform.EngagementStates.Engaged)
				{
					this.didLeaveEngageMenu = true;
					this.UIGoToMainMenu();
				}
				else
				{
					this.UIGoToEngageMenu();
				}
			}
			else if (newState == UIState.LOADING)
			{
				this.DisableScreens();
			}
			else if (newState == UIState.PLAYING)
			{
				this.DisableScreens();
			}
			else if (newState == UIState.CUTSCENE)
			{
				this.DisableScreens();
			}
			this.uiState = newState;
			return;
		}
		if (newState == UIState.MAIN_MENU_HOME)
		{
			this.UIGoToMainMenu();
		}
	}

	// Token: 0x060011AE RID: 4526 RVA: 0x00052BB2 File Offset: 0x00050DB2
	private void SetMenuState(MainMenuState newState)
	{
		this.menuState = newState;
	}

	// Token: 0x060011AF RID: 4527 RVA: 0x00052BBC File Offset: 0x00050DBC
	private void SetupRefs()
	{
		this.gm = GameManager.instance;
		this.gs = this.gm.gameSettings;
		this.playerData = PlayerData.instance;
		this.ih = this.gm.inputHandler;
		if (this.gm.IsGameplayScene())
		{
			this.hero_ctrl = HeroController.instance;
		}
		if (this.gm.IsMenuScene() && this.gameTitle == null)
		{
			this.gameTitle = GameObject.Find("LogoTitle").GetComponent<SpriteRenderer>();
		}
		if (this.UICanvas.worldCamera == null)
		{
			this.UICanvas.worldCamera = GameCameras.instance.mainCamera;
		}
	}

	// Token: 0x060011B0 RID: 4528 RVA: 0x00052C71 File Offset: 0x00050E71
	public void SetUIStartState(GameState gameState)
	{
		if (gameState == GameState.MAIN_MENU)
		{
			this.SetState(UIState.MAIN_MENU_HOME);
			return;
		}
		if (gameState == GameState.LOADING)
		{
			this.SetState(UIState.LOADING);
			return;
		}
		if (gameState == GameState.ENTERING_LEVEL)
		{
			this.SetState(UIState.PLAYING);
			return;
		}
		if (gameState == GameState.PLAYING)
		{
			this.SetState(UIState.PLAYING);
			return;
		}
		if (gameState == GameState.CUTSCENE)
		{
			this.SetState(UIState.CUTSCENE);
		}
	}

	// Token: 0x060011B1 RID: 4529 RVA: 0x00052CAE File Offset: 0x00050EAE
	public IEnumerator ShowMainMenuHome()
	{
		this.ih.StopUIInput();
		this.SetMenuState(MainMenuState.MAIN_MENU);
		this.mainMenuScreen.alpha = 0f;
		this.ShowCanvas(this.UICanvas);
		this.mainMenuScreen.gameObject.SetActive(true);
		while (this.mainMenuScreen.alpha < 1f)
		{
			this.mainMenuScreen.alpha += Time.unscaledDeltaTime * this.MENU_FADE_SPEED;
			yield return null;
		}
		this.mainMenuScreen.alpha = 1f;
		this.mainMenuScreen.interactable = true;
		this.ih.StartUIInput();
		yield return null;
		this.mainMenuButtons.HighlightDefault(false);
		yield break;
	}

	// Token: 0x060011B2 RID: 4530 RVA: 0x00052CBD File Offset: 0x00050EBD
	private Coroutine StartMenuAnimationCoroutine(IEnumerator routine)
	{
		return base.StartCoroutine(this.StartMenuAnimationCoroutineWorker(routine));
	}

	// Token: 0x060011B3 RID: 4531 RVA: 0x00052CCC File Offset: 0x00050ECC
	private IEnumerator StartMenuAnimationCoroutineWorker(IEnumerator routine)
	{
		this.menuAnimationCounter++;
		yield return this.StartCoroutine(routine);
		this.menuAnimationCounter--;
		yield break;
	}

	// Token: 0x060011B4 RID: 4532 RVA: 0x00052CE2 File Offset: 0x00050EE2
	public void UIGoToOptionsMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToOptionsMenu());
	}

	// Token: 0x060011B5 RID: 4533 RVA: 0x00052CF1 File Offset: 0x00050EF1
	public void UILeaveOptionsMenu()
	{
		this.StartMenuAnimationCoroutine(this.LeaveOptionsMenu());
	}

	// Token: 0x060011B6 RID: 4534 RVA: 0x00052D00 File Offset: 0x00050F00
	public void UIExplicitSwitchUser()
	{
		this.slotOne.ClearCache();
		this.slotTwo.ClearCache();
		this.slotThree.ClearCache();
		this.slotFour.ClearCache();
		this.UIGoToEngageMenu();
	}

	// Token: 0x060011B7 RID: 4535 RVA: 0x00052D34 File Offset: 0x00050F34
	public void UIGoToEngageMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToEngageMenu());
	}

	// Token: 0x060011B8 RID: 4536 RVA: 0x00052D43 File Offset: 0x00050F43
	public void UIGoToNoSaveMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToNoSaveMenu());
	}

	// Token: 0x060011B9 RID: 4537 RVA: 0x00052D52 File Offset: 0x00050F52
	public void UIGoToMainMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToMainMenu());
	}

	// Token: 0x060011BA RID: 4538 RVA: 0x00052D61 File Offset: 0x00050F61
	public void UIGoToProfileMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToProfileMenu());
	}

	// Token: 0x060011BB RID: 4539 RVA: 0x00052D70 File Offset: 0x00050F70
	public void UIReturnToProfileMenu()
	{
		if (Platform.Current.IsSavingAllowedByEngagement)
		{
			this.UIGoToProfileMenu();
			return;
		}
		this.UIGoToMainMenu();
	}

	// Token: 0x060011BC RID: 4540 RVA: 0x00052D8C File Offset: 0x00050F8C
	public void UIMainStartGame()
	{
		if (Platform.Current.IsSavingAllowedByEngagement)
		{
			this.UIGoToProfileMenu();
			return;
		}
		this.gm.profileID = -1;
		if (this.gm.GetStatusRecordInt("RecPermadeathMode") == 1 || this.gm.GetStatusRecordInt("RecBossRushMode") == 1)
		{
			this.UIGoToPlayModeMenu();
			return;
		}
		this.StartNewGame(false, false);
	}

	// Token: 0x060011BD RID: 4541 RVA: 0x00052DED File Offset: 0x00050FED
	public void UIGoToControllerMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToControllerMenu());
	}

	// Token: 0x060011BE RID: 4542 RVA: 0x00052DFC File Offset: 0x00050FFC
	public void UIGoToRemapControllerMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToRemapControllerMenu());
	}

	// Token: 0x060011BF RID: 4543 RVA: 0x00052E0B File Offset: 0x0005100B
	public void UIGoToKeyboardMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToKeyboardMenu());
	}

	// Token: 0x060011C0 RID: 4544 RVA: 0x00052E1A File Offset: 0x0005101A
	public void UIGoToAudioMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToAudioMenu());
	}

	// Token: 0x060011C1 RID: 4545 RVA: 0x00052E29 File Offset: 0x00051029
	public void UIGoToVideoMenu(bool rollbackRes = false)
	{
		this.StartMenuAnimationCoroutine(this.GoToVideoMenu(rollbackRes));
	}

	// Token: 0x060011C2 RID: 4546 RVA: 0x00052E39 File Offset: 0x00051039
	public void UIGoToPauseMenu()
	{
		this.goToPauseMenuCo = this.StartMenuAnimationCoroutine(this.GoToPauseMenu());
	}

	// Token: 0x060011C3 RID: 4547 RVA: 0x00052E4D File Offset: 0x0005104D
	public void UIClosePauseMenu()
	{
		this.ih.StopUIInput();
		base.StartCoroutine(this.HideCurrentMenu());
		this.StartMenuAnimationCoroutine(this.FadeOutCanvasGroup(this.modalDimmer));
	}

	// Token: 0x060011C4 RID: 4548 RVA: 0x00052E7A File Offset: 0x0005107A
	public void UIClearPauseMenu()
	{
		this.pauseMenuAnimator.SetBool("clear", true);
	}

	// Token: 0x060011C5 RID: 4549 RVA: 0x00052E8D File Offset: 0x0005108D
	public void UnClearPauseMenu()
	{
		this.pauseMenuAnimator.SetBool("clear", false);
	}

	// Token: 0x060011C6 RID: 4550 RVA: 0x00052EA0 File Offset: 0x000510A0
	public void UIGoToOverscanMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToOverscanMenu());
	}

	// Token: 0x060011C7 RID: 4551 RVA: 0x00052EAF File Offset: 0x000510AF
	public void UIGoToBrightnessMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToBrightnessMenu());
	}

	// Token: 0x060011C8 RID: 4552 RVA: 0x00052EBE File Offset: 0x000510BE
	public void UIGoToGameOptionsMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToGameOptionsMenu());
	}

	// Token: 0x060011C9 RID: 4553 RVA: 0x00052ECD File Offset: 0x000510CD
	public void UIGoToAchievementsMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToAchievementsMenu());
	}

	// Token: 0x060011CA RID: 4554 RVA: 0x00052EDC File Offset: 0x000510DC
	public void UIGoToExtrasMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToExtrasMenu());
	}

	// Token: 0x060011CB RID: 4555 RVA: 0x00052EEB File Offset: 0x000510EB
	public void UIGoToExtrasContentMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToExtrasContentMenu());
	}

	// Token: 0x060011CC RID: 4556 RVA: 0x00052EFA File Offset: 0x000510FA
	public void UIShowQuitGamePrompt()
	{
		this.StartMenuAnimationCoroutine(this.GoToQuitGamePrompt());
	}

	// Token: 0x060011CD RID: 4557 RVA: 0x00052F09 File Offset: 0x00051109
	public void UIShowReturnMenuPrompt()
	{
		this.StartMenuAnimationCoroutine(this.GoToReturnMenuPrompt());
	}

	// Token: 0x060011CE RID: 4558 RVA: 0x00052F18 File Offset: 0x00051118
	public void UIShowResolutionPrompt(bool startTimer = false)
	{
		this.StartMenuAnimationCoroutine(this.GoToResolutionPrompt(startTimer));
	}

	// Token: 0x060011CF RID: 4559 RVA: 0x00052F28 File Offset: 0x00051128
	public void UILeaveExitToMenuPrompt()
	{
		this.StartMenuAnimationCoroutine(this.LeaveExitToMenuPrompt());
	}

	// Token: 0x060011D0 RID: 4560 RVA: 0x00052F37 File Offset: 0x00051137
	public void UIGoToPlayModeMenu()
	{
		this.StartMenuAnimationCoroutine(this.GoToPlayModeMenu());
	}

	// Token: 0x060011D1 RID: 4561 RVA: 0x00052F46 File Offset: 0x00051146
	public void UIReturnToMainMenu()
	{
		this.StartMenuAnimationCoroutine(this.ReturnToMainMenu());
	}

	// Token: 0x060011D2 RID: 4562 RVA: 0x00052F55 File Offset: 0x00051155
	public void UIGoToMenuCredits()
	{
		this.StartMenuAnimationCoroutine(this.GoToMenuCredits());
	}

	// Token: 0x060011D3 RID: 4563 RVA: 0x00052F64 File Offset: 0x00051164
	public void UIStartNewGame()
	{
		this.StartNewGame(false, false);
	}

	// Token: 0x060011D4 RID: 4564 RVA: 0x00052F6E File Offset: 0x0005116E
	public void UIStartNewGameContinue()
	{
		this.StartNewGame(this.permaDeath, this.bossRush);
	}

	// Token: 0x060011D5 RID: 4565 RVA: 0x00052F84 File Offset: 0x00051184
	public void StartNewGame(bool permaDeath = false, bool bossRush = false)
	{
		this.permaDeath = permaDeath;
		this.bossRush = bossRush;
		this.ih.StopUIInput();
		if (this.gs.overscanAdjusted == 1 && this.gs.brightnessAdjusted == 1)
		{
			this.uiAudioPlayer.PlayStartGame();
			this.gm.EnsureSaveSlotSpace(delegate(bool hasSpace)
			{
				if (hasSpace)
				{
					if (this.menuState == MainMenuState.SAVE_PROFILES)
					{
						this.StartCoroutine(this.HideSaveProfileMenu());
					}
					else
					{
						this.StartCoroutine(this.HideCurrentMenu());
					}
					this.uiAudioPlayer.PlayStartGame();
					this.gm.StartNewGame(permaDeath, bossRush);
					return;
				}
				this.ih.StartUIInput();
				SaveSlotButton saveSlotButton;
				switch (this.gm.profileID)
				{
				default:
					saveSlotButton = this.slotOne;
					break;
				case 2:
					saveSlotButton = this.slotTwo;
					break;
				case 3:
					saveSlotButton = this.slotThree;
					break;
				case 4:
					saveSlotButton = this.slotFour;
					break;
				}
				saveSlotButton.Select();
			});
			return;
		}
		if (this.gs.overscanAdjusted == 0)
		{
			this.UIGoToOverscanMenu();
			return;
		}
		if (this.gs.overscanAdjusted == 1 && this.gs.brightnessAdjusted == 0)
		{
			this.UIGoToBrightnessMenu();
		}
	}

	// Token: 0x060011D6 RID: 4566 RVA: 0x00053044 File Offset: 0x00051244
	public void ContinueGame()
	{
		this.ih.StopUIInput();
		this.uiAudioPlayer.PlayStartGame();
		if (MenuStyles.Instance)
		{
			MenuStyles.Instance.StopAudio();
		}
		if (this.menuState == MainMenuState.SAVE_PROFILES)
		{
			base.StartCoroutine(this.HideSaveProfileMenu());
		}
	}

	// Token: 0x060011D7 RID: 4567 RVA: 0x00053093 File Offset: 0x00051293
	public IEnumerator GoToEngageMenu()
	{
		if (this.ih == null)
		{
			this.ih = this.gm.inputHandler;
		}
		this.ih.StopUIInput();
		this.didLeaveEngageMenu = false;
		Platform.Current.ClearEngagement();
		if (this.menuState == MainMenuState.MAIN_MENU)
		{
			this.mainMenuScreen.interactable = false;
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.mainMenuScreen));
		}
		else if (this.menuState == MainMenuState.SAVE_PROFILES)
		{
			yield return this.StartCoroutine(this.HideSaveProfileMenu());
		}
		else
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		this.ih.StopUIInput();
		this.gameTitle.gameObject.SetActive(true);
		if (MenuStyles.Instance)
		{
			MenuStyles.Instance.UpdateTitle();
		}
		this.engageMenuScreen.gameObject.SetActive(true);
		this.StartCoroutine(this.FadeInSprite(this.gameTitle));
		this.subtitleFSM.SendEvent("FADE IN");
		this.engageMenuScreen.topFleur.ResetTrigger("hide");
		this.engageMenuScreen.topFleur.SetTrigger("show");
		this.engageMenuScreen.bottomFleur.ResetTrigger("hide");
		this.engageMenuScreen.bottomFleur.SetTrigger("show");
		this.StartCoroutine(this.FadeInCanvasGroup(this.engageMenuScreen.title));
		yield return this.StartCoroutine(this.FadeInCanvasGroup(this.engageMenuScreen.GetComponent<CanvasGroup>()));
		yield return null;
		this.SetMenuState(MainMenuState.ENGAGE_MENU);
		yield break;
	}

	// Token: 0x060011D8 RID: 4568 RVA: 0x000530A2 File Offset: 0x000512A2
	public IEnumerator GoToNoSaveMenu()
	{
		if (this.ih == null)
		{
			this.ih = this.gm.inputHandler;
		}
		this.ih.StopUIInput();
		yield return this.StartCoroutine(this.HideCurrentMenu());
		this.ih.StopUIInput();
		this.noSaveMenuScreen.gameObject.SetActive(true);
		yield return this.StartCoroutine(this.ShowMenu(this.noSaveMenuScreen));
		this.SetMenuState(MainMenuState.NO_SAVE_MENU);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011D9 RID: 4569 RVA: 0x000530B1 File Offset: 0x000512B1
	public IEnumerator GoToMainMenu()
	{
		if (this.ih == null)
		{
			this.ih = this.gm.inputHandler;
		}
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.OPTIONS_MENU || this.menuState == MainMenuState.ACHIEVEMENTS_MENU || this.menuState == MainMenuState.QUIT_GAME_PROMPT || this.menuState == MainMenuState.EXTRAS_MENU || this.menuState == MainMenuState.ENGAGE_MENU || this.menuState == MainMenuState.NO_SAVE_MENU || this.menuState == MainMenuState.PLAY_MODE_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		else if (this.menuState == MainMenuState.SAVE_PROFILES)
		{
			yield return this.StartCoroutine(this.HideSaveProfileMenu());
		}
		this.ih.StopUIInput();
		this.gameTitle.gameObject.SetActive(true);
		this.mainMenuScreen.gameObject.SetActive(true);
		if (MenuStyles.Instance)
		{
			MenuStyles.Instance.UpdateTitle();
		}
		this.StartCoroutine(this.FadeInSprite(this.gameTitle));
		this.subtitleFSM.SendEvent("FADE IN");
		yield return this.StartCoroutine(this.FadeInCanvasGroup(this.mainMenuScreen));
		this.mainMenuScreen.interactable = true;
		this.ih.StartUIInput();
		yield return null;
		this.mainMenuButtons.HighlightDefault(false);
		this.SetMenuState(MainMenuState.MAIN_MENU);
		yield break;
	}

	// Token: 0x060011DA RID: 4570 RVA: 0x000530C0 File Offset: 0x000512C0
	public IEnumerator GoToProfileMenu()
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.MAIN_MENU)
		{
			this.StartCoroutine(this.FadeOutSprite(this.gameTitle));
			this.subtitleFSM.SendEvent("FADE OUT");
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.mainMenuScreen));
		}
		else if (this.menuState == MainMenuState.PLAY_MODE_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
			this.ih.StopUIInput();
		}
		this.StartCoroutine(this.FadeInCanvasGroup(this.saveProfileScreen));
		this.saveProfileTopFleur.ResetTrigger("hide");
		this.saveProfileTopFleur.SetTrigger("show");
		this.StartCoroutine(this.FadeInCanvasGroup(this.saveProfileTitle));
		this.StartCoroutine(this.FadeInCanvasGroup(this.saveProfileScreen));
		this.StartCoroutine(this.PrepareSaveFilesInOrder());
		yield return new WaitForSeconds(0.165f);
		SaveSlotButton[] slotButtons = new SaveSlotButton[]
		{
			this.slotOne,
			this.slotTwo,
			this.slotThree,
			this.slotFour
		};
		int num;
		for (int i = 0; i < slotButtons.Length; i = num)
		{
			slotButtons[i].ShowRelevantModeForSaveFileState();
			yield return new WaitForSeconds(0.165f);
			num = i + 1;
		}
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.695f));
		this.StartCoroutine(this.FadeInCanvasGroup(this.saveProfileControls));
		this.ih.StartUIInput();
		yield return null;
		this.saveSlots.HighlightDefault(false);
		this.SetMenuState(MainMenuState.SAVE_PROFILES);
		yield break;
	}

	// Token: 0x060011DB RID: 4571 RVA: 0x000530CF File Offset: 0x000512CF
	protected IEnumerator PrepareSaveFilesInOrder()
	{
		SaveSlotButton[] slotButtons = new SaveSlotButton[]
		{
			this.slotOne,
			this.slotTwo,
			this.slotThree,
			this.slotFour
		};
		int num;
		for (int i = 0; i < slotButtons.Length; i = num)
		{
			SaveSlotButton slotButton = slotButtons[i];
			if (slotButton.saveFileState == SaveSlotButton.SaveFileStates.NotStarted)
			{
				slotButton.Prepare(this.gm, false);
				while (slotButton.saveFileState == SaveSlotButton.SaveFileStates.OperationInProgress)
				{
					yield return null;
				}
			}
			slotButton = null;
			num = i + 1;
		}
		yield break;
	}

	// Token: 0x060011DC RID: 4572 RVA: 0x000530DE File Offset: 0x000512DE
	public IEnumerator GoToOptionsMenu()
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.MAIN_MENU)
		{
			this.StartCoroutine(this.FadeOutSprite(this.gameTitle));
			this.subtitleFSM.SendEvent("FADE OUT");
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.mainMenuScreen));
		}
		else if (this.menuState == MainMenuState.AUDIO_MENU || this.menuState == MainMenuState.VIDEO_MENU || this.menuState == MainMenuState.GAMEPAD_MENU || this.menuState == MainMenuState.GAME_OPTIONS_MENU || this.menuState == MainMenuState.PAUSE_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		else if (this.menuState == MainMenuState.KEYBOARD_MENU)
		{
			if (this.uiButtonSkins.listeningKey != null)
			{
				this.uiButtonSkins.listeningKey.StopActionListening();
				this.uiButtonSkins.listeningKey.AbortRebind();
			}
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		yield return this.StartCoroutine(this.ShowMenu(this.optionsMenuScreen));
		this.SetMenuState(MainMenuState.OPTIONS_MENU);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011DD RID: 4573 RVA: 0x000530ED File Offset: 0x000512ED
	public IEnumerator GoToControllerMenu()
	{
		if (this.menuState == MainMenuState.OPTIONS_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		else if (this.menuState == MainMenuState.REMAP_GAMEPAD_MENU)
		{
			if (this.uiButtonSkins.listeningButton != null)
			{
				this.uiButtonSkins.listeningButton.StopActionListening();
				this.uiButtonSkins.listeningButton.AbortRebind();
			}
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		yield return this.StartCoroutine(this.ShowMenu(this.gamepadMenuScreen));
		this.SetMenuState(MainMenuState.GAMEPAD_MENU);
		yield break;
	}

	// Token: 0x060011DE RID: 4574 RVA: 0x000530FC File Offset: 0x000512FC
	public IEnumerator GoToRemapControllerMenu()
	{
		yield return this.StartCoroutine(this.HideCurrentMenu());
		this.StartCoroutine(this.ShowMenu(this.remapGamepadMenuScreen));
		yield return this.StartCoroutine(this.uiButtonSkins.ShowCurrentButtonMappings());
		this.SetMenuState(MainMenuState.REMAP_GAMEPAD_MENU);
		yield break;
	}

	// Token: 0x060011DF RID: 4575 RVA: 0x0005310B File Offset: 0x0005130B
	public IEnumerator GoToKeyboardMenu()
	{
		yield return this.StartCoroutine(this.HideCurrentMenu());
		this.StartCoroutine(this.ShowMenu(this.keyboardMenuScreen));
		yield return this.StartCoroutine(this.uiButtonSkins.ShowCurrentKeyboardMappings());
		this.SetMenuState(MainMenuState.KEYBOARD_MENU);
		yield break;
	}

	// Token: 0x060011E0 RID: 4576 RVA: 0x0005311A File Offset: 0x0005131A
	public IEnumerator GoToAudioMenu()
	{
		yield return this.StartCoroutine(this.HideCurrentMenu());
		yield return this.StartCoroutine(this.ShowMenu(this.audioMenuScreen));
		this.SetMenuState(MainMenuState.AUDIO_MENU);
		yield break;
	}

	// Token: 0x060011E1 RID: 4577 RVA: 0x00053129 File Offset: 0x00051329
	public IEnumerator GoToVideoMenu(bool rollbackRes = false)
	{
		if (this.menuState == MainMenuState.OPTIONS_MENU || this.menuState == MainMenuState.OVERSCAN_MENU || this.menuState == MainMenuState.BRIGHTNESS_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		else if (this.menuState == MainMenuState.RESOLUTION_PROMPT)
		{
			if (rollbackRes)
			{
				this.HideMenuInstant(this.resolutionPrompt);
				this.videoMenuScreen.gameObject.SetActive(true);
				this.videoMenuScreen.content.gameObject.SetActive(true);
				this.eventSystem.SetSelectedGameObject(null);
				this.resolutionOption.RollbackResolution();
			}
			else
			{
				yield return this.StartCoroutine(this.HideCurrentMenu());
			}
		}
		yield return this.StartCoroutine(this.ShowMenu(this.videoMenuScreen));
		this.SetMenuState(MainMenuState.VIDEO_MENU);
		yield break;
	}

	// Token: 0x060011E2 RID: 4578 RVA: 0x0005313F File Offset: 0x0005133F
	public IEnumerator GoToOverscanMenu()
	{
		if (this.menuState == MainMenuState.VIDEO_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
			this.overscanSetting.NormalMode();
		}
		else if (this.menuState == MainMenuState.SAVE_PROFILES)
		{
			yield return this.StartCoroutine(this.HideSaveProfileMenu());
			this.overscanSetting.DoneMode();
		}
		else if (this.menuState == MainMenuState.PLAY_MODE_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
			this.overscanSetting.DoneMode();
		}
		yield return this.StartCoroutine(this.ShowMenu(this.overscanMenuScreen));
		this.SetMenuState(MainMenuState.OVERSCAN_MENU);
		yield break;
	}

	// Token: 0x060011E3 RID: 4579 RVA: 0x0005314E File Offset: 0x0005134E
	public IEnumerator GoToBrightnessMenu()
	{
		if (this.menuState == MainMenuState.VIDEO_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
			this.brightnessSetting.NormalMode();
		}
		else if (this.menuState == MainMenuState.OVERSCAN_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
			this.brightnessSetting.DoneMode();
		}
		else if (this.menuState == MainMenuState.SAVE_PROFILES)
		{
			yield return this.StartCoroutine(this.HideSaveProfileMenu());
			this.brightnessSetting.DoneMode();
		}
		else if (this.menuState == MainMenuState.PLAY_MODE_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
			this.brightnessSetting.DoneMode();
		}
		yield return this.StartCoroutine(this.ShowMenu(this.brightnessMenuScreen));
		this.SetMenuState(MainMenuState.BRIGHTNESS_MENU);
		yield break;
	}

	// Token: 0x060011E4 RID: 4580 RVA: 0x0005315D File Offset: 0x0005135D
	public IEnumerator GoToGameOptionsMenu()
	{
		yield return this.StartCoroutine(this.HideCurrentMenu());
		yield return this.StartCoroutine(this.ShowMenu(this.gameOptionsMenuScreen));
		this.SetMenuState(MainMenuState.GAME_OPTIONS_MENU);
		yield break;
	}

	// Token: 0x060011E5 RID: 4581 RVA: 0x0005316C File Offset: 0x0005136C
	public IEnumerator GoToAchievementsMenu()
	{
		if (Platform.Current.HasNativeAchievementsDialog)
		{
			Platform.Current.ShowNativeAchievementsDialog();
			yield return null;
			this.mainMenuButtons.achievementsButton.Select();
			yield break;
		}
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.MAIN_MENU)
		{
			this.StartCoroutine(this.FadeOutSprite(this.gameTitle));
			this.subtitleFSM.SendEvent("FADE OUT");
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.mainMenuScreen));
		}
		else
		{
			Debug.LogError("Entering from this menu not implemented.");
		}
		this.achievementListRect.anchoredPosition = new Vector2(this.achievementListRect.anchoredPosition.x, 0f);
		yield return this.StartCoroutine(this.ShowMenu(this.achievementsMenuScreen));
		this.SetMenuState(MainMenuState.ACHIEVEMENTS_MENU);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011E6 RID: 4582 RVA: 0x0005317B File Offset: 0x0005137B
	public IEnumerator GoToExtrasMenu()
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.MAIN_MENU)
		{
			this.StartCoroutine(this.FadeOutSprite(this.gameTitle));
			this.subtitleFSM.SendEvent("FADE OUT");
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.mainMenuScreen));
		}
		else if (this.menuState == MainMenuState.EXTRAS_CONTENT_MENU)
		{
			yield return this.StartCoroutine(this.HideMenu(this.extrasContentMenuScreen));
		}
		else
		{
			Debug.LogError("Entering from this menu not implemented.");
		}
		yield return this.StartCoroutine(this.ShowMenu(this.extrasMenuScreen));
		this.SetMenuState(MainMenuState.EXTRAS_MENU);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011E7 RID: 4583 RVA: 0x0005318A File Offset: 0x0005138A
	public IEnumerator GoToExtrasContentMenu()
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.EXTRAS_MENU)
		{
			yield return this.StartCoroutine(this.HideMenu(this.extrasMenuScreen));
		}
		else
		{
			Debug.LogError("Entering from this menu not implemented.");
		}
		yield return this.StartCoroutine(this.ShowMenu(this.extrasContentMenuScreen));
		this.SetMenuState(MainMenuState.EXTRAS_CONTENT_MENU);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011E8 RID: 4584 RVA: 0x00053199 File Offset: 0x00051399
	public IEnumerator GoToQuitGamePrompt()
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.MAIN_MENU)
		{
			this.StartCoroutine(this.FadeOutSprite(this.gameTitle));
			this.subtitleFSM.SendEvent("FADE OUT");
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.mainMenuScreen));
		}
		else
		{
			Debug.LogError("Switching between these menus is not implemented.");
		}
		this.activePrompt = this.quitGamePrompt;
		yield return this.StartCoroutine(this.ShowMenu(this.quitGamePrompt));
		this.SetMenuState(MainMenuState.QUIT_GAME_PROMPT);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011E9 RID: 4585 RVA: 0x000531A8 File Offset: 0x000513A8
	public IEnumerator GoToReturnMenuPrompt()
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.PAUSE_MENU)
		{
			yield return this.StartCoroutine(this.HideCurrentMenu());
		}
		else
		{
			Debug.LogError("Switching between these menus is not implemented.");
		}
		this.activePrompt = this.quitGamePrompt;
		yield return this.StartCoroutine(this.ShowMenu(this.returnMainMenuPrompt));
		this.SetMenuState(MainMenuState.EXIT_PROMPT);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011EA RID: 4586 RVA: 0x000531B7 File Offset: 0x000513B7
	public IEnumerator GoToResolutionPrompt(bool startTimer = false)
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.VIDEO_MENU)
		{
			yield return this.StartCoroutine(this.HideMenu(this.videoMenuScreen));
		}
		else
		{
			Debug.LogError("Switching between these menus is not implemented.");
		}
		this.activePrompt = this.resolutionPrompt;
		yield return this.StartCoroutine(this.ShowMenu(this.resolutionPrompt));
		this.SetMenuState(MainMenuState.RESOLUTION_PROMPT);
		if (startTimer)
		{
			this.countdownTimer.StartTimer();
		}
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011EB RID: 4587 RVA: 0x000531CD File Offset: 0x000513CD
	public IEnumerator LeaveOptionsMenu()
	{
		yield return this.StartCoroutine(this.HideCurrentMenu());
		if (this.uiState == UIState.PAUSED)
		{
			this.UIGoToPauseMenu();
		}
		else
		{
			this.UIGoToMainMenu();
		}
		yield break;
	}

	// Token: 0x060011EC RID: 4588 RVA: 0x000531DC File Offset: 0x000513DC
	public IEnumerator LeaveExitToMenuPrompt()
	{
		yield return this.StartCoroutine(this.HideCurrentMenu());
		if (this.uiState == UIState.PAUSED)
		{
			this.UnClearPauseMenu();
		}
		yield break;
	}

	// Token: 0x060011ED RID: 4589 RVA: 0x000531EB File Offset: 0x000513EB
	public IEnumerator GoToPlayModeMenu()
	{
		this.ih.StopUIInput();
		if (this.menuState == MainMenuState.MAIN_MENU)
		{
			this.StartCoroutine(this.FadeOutSprite(this.gameTitle));
			this.subtitleFSM.SendEvent("FADE OUT");
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.mainMenuScreen));
		}
		else if (this.menuState == MainMenuState.SAVE_PROFILES)
		{
			yield return this.StartCoroutine(this.HideSaveProfileMenu());
		}
		yield return this.StartCoroutine(this.ShowMenu(this.playModeMenuScreen));
		this.SetMenuState(MainMenuState.PLAY_MODE_MENU);
		this.ih.StartUIInput();
		yield break;
	}

	// Token: 0x060011EE RID: 4590 RVA: 0x000531FA File Offset: 0x000513FA
	public IEnumerator ReturnToMainMenu()
	{
		this.ih.StopUIInput();
		bool calledBack = false;
		bool willSave = Platform.Current.IsSavingAllowedByEngagement;
		GameManager.ReturnToMainMenuSaveModes saveMode = GameManager.ReturnToMainMenuSaveModes.SaveAndCancelOnFail;
		if (!willSave)
		{
			saveMode = GameManager.ReturnToMainMenuSaveModes.SaveAndContinueOnFail;
		}
		this.StartCoroutine(this.gm.ReturnToMainMenu(saveMode, delegate(bool willComplete)
		{
			calledBack = true;
			if (!willComplete && willSave)
			{
				this.ih.StartUIInput();
				this.returnMainMenuPrompt.HighlightDefault();
				return;
			}
			this.StartCoroutine(this.HideCurrentMenu());
		}));
		while (!calledBack)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x060011EF RID: 4591 RVA: 0x00053209 File Offset: 0x00051409
	public IEnumerator GoToPauseMenu()
	{
		this.ih.StopUIInput();
		this.ignoreUnpause = true;
		if (this.uiState == UIState.PAUSED)
		{
			if (this.menuState == MainMenuState.OPTIONS_MENU || this.menuState == MainMenuState.EXIT_PROMPT)
			{
				yield return this.StartCoroutine(this.HideCurrentMenu());
			}
		}
		else
		{
			this.StartCoroutine(this.FadeInCanvasGroupAlpha(this.modalDimmer, 0.8f));
		}
		yield return this.StartCoroutine(this.ShowMenu(this.pauseMenuScreen));
		this.SetMenuState(MainMenuState.PAUSE_MENU);
		this.ih.StartUIInput();
		this.ignoreUnpause = false;
		yield break;
	}

	// Token: 0x060011F0 RID: 4592 RVA: 0x00053218 File Offset: 0x00051418
	public IEnumerator GoToMenuCredits()
	{
		this.ih.StopUIInput();
		yield return this.StartCoroutine(this.HideCurrentMenu());
		GameCameras.instance.cameraController.FadeOut(CameraFadeType.START_FADE);
		yield return new WaitForSeconds(2.5f);
		this.gm.LoadScene("Menu_Credits");
		yield break;
	}

	// Token: 0x060011F1 RID: 4593 RVA: 0x00053227 File Offset: 0x00051427
	public void ShowCutscenePrompt(CinematicSkipPopup.Texts text)
	{
		this.cinematicSkipPopup.Show(text);
	}

	// Token: 0x060011F2 RID: 4594 RVA: 0x00053235 File Offset: 0x00051435
	public void HideCutscenePrompt()
	{
		this.cinematicSkipPopup.Hide();
	}

	// Token: 0x060011F3 RID: 4595 RVA: 0x00052CE2 File Offset: 0x00050EE2
	public void ApplyAudioMenuSettings()
	{
		this.StartMenuAnimationCoroutine(this.GoToOptionsMenu());
	}

	// Token: 0x060011F4 RID: 4596 RVA: 0x00052CE2 File Offset: 0x00050EE2
	public void ApplyVideoMenuSettings()
	{
		this.StartMenuAnimationCoroutine(this.GoToOptionsMenu());
	}

	// Token: 0x060011F5 RID: 4597 RVA: 0x00052CE2 File Offset: 0x00050EE2
	public void ApplyControllerMenuSettings()
	{
		this.StartMenuAnimationCoroutine(this.GoToOptionsMenu());
	}

	// Token: 0x060011F6 RID: 4598 RVA: 0x00052DED File Offset: 0x00050FED
	public void ApplyRemapGamepadMenuSettings()
	{
		this.StartMenuAnimationCoroutine(this.GoToControllerMenu());
	}

	// Token: 0x060011F7 RID: 4599 RVA: 0x00052CE2 File Offset: 0x00050EE2
	public void ApplyKeyboardMenuSettings()
	{
		this.StartMenuAnimationCoroutine(this.GoToOptionsMenu());
	}

	// Token: 0x060011F8 RID: 4600 RVA: 0x00053242 File Offset: 0x00051442
	public void ApplyOverscanSettings(bool goToBrightness = false)
	{
		Debug.LogError("This function is now deprecated");
	}

	// Token: 0x060011F9 RID: 4601 RVA: 0x0005324E File Offset: 0x0005144E
	public void ApplyBrightnessSettings()
	{
		this.StartMenuAnimationCoroutine(this.GoToVideoMenu(false));
	}

	// Token: 0x060011FA RID: 4602 RVA: 0x00052CE2 File Offset: 0x00050EE2
	public void ApplyGameMenuSettings()
	{
		this.StartMenuAnimationCoroutine(this.GoToOptionsMenu());
	}

	// Token: 0x060011FB RID: 4603 RVA: 0x0005325E File Offset: 0x0005145E
	public void SaveOverscanSettings()
	{
		this.gs.SaveOverscanSettings();
	}

	// Token: 0x060011FC RID: 4604 RVA: 0x0005326B File Offset: 0x0005146B
	public void SaveBrightnessSettings()
	{
		this.gs.SaveBrightnessSettings();
	}

	// Token: 0x060011FD RID: 4605 RVA: 0x00053278 File Offset: 0x00051478
	public void DefaultAudioMenuSettings()
	{
		this.gs.ResetAudioSettings();
		this.RefreshAudioControls();
	}

	// Token: 0x060011FE RID: 4606 RVA: 0x0005328C File Offset: 0x0005148C
	public void DefaultVideoMenuSettings()
	{
		this.gs.ResetVideoSettings();
		Platform.Current.AdjustGraphicsSettings(this.gs);
		this.resolutionOption.ResetToDefaultResolution();
		this.fullscreenOption.UpdateSetting(this.gs.fullScreen);
		this.vsyncOption.UpdateSetting(this.gs.vSync);
		this.shadersOption.UpdateSetting((int)this.gs.shaderQuality);
		this.RefreshVideoControls();
	}

	// Token: 0x060011FF RID: 4607 RVA: 0x00053307 File Offset: 0x00051507
	public void DefaultGamepadMenuSettings()
	{
		this.ih.ResetDefaultControllerButtonBindings();
		this.uiButtonSkins.RefreshButtonMappings();
	}

	// Token: 0x06001200 RID: 4608 RVA: 0x0005331F File Offset: 0x0005151F
	public void DefaultKeyboardMenuSettings()
	{
		this.ih.ResetDefaultKeyBindings();
		this.uiButtonSkins.RefreshKeyMappings();
	}

	// Token: 0x06001201 RID: 4609 RVA: 0x00053337 File Offset: 0x00051537
	public void DefaultGameMenuSettings()
	{
		this.gs.ResetGameOptionsSettings();
		Platform.Current.AdjustGameSettings(this.gs);
		this.RefreshGameOptionsControls();
	}

	// Token: 0x06001202 RID: 4610 RVA: 0x0005335A File Offset: 0x0005155A
	public void LoadStoredSettings()
	{
		this.gs.LoadOverscanConfigured();
		this.gs.LoadBrightnessConfigured();
		this.LoadAudioSettings();
		this.LoadVideoSettings();
		this.LoadGameOptionsSettings();
	}

	// Token: 0x06001203 RID: 4611 RVA: 0x00053384 File Offset: 0x00051584
	private void LoadAudioSettings()
	{
		this.gs.LoadAudioSettings();
		this.RefreshAudioControls();
	}

	// Token: 0x06001204 RID: 4612 RVA: 0x00053397 File Offset: 0x00051597
	private void LoadVideoSettings()
	{
		this.gs.LoadVideoSettings();
		this.gs.LoadOverscanSettings();
		this.gs.LoadBrightnessSettings();
		Platform.Current.AdjustGraphicsSettings(this.gs);
		this.RefreshVideoControls();
	}

	// Token: 0x06001205 RID: 4613 RVA: 0x000533D0 File Offset: 0x000515D0
	private void LoadGameOptionsSettings()
	{
		this.gs.LoadGameOptionsSettings();
		Platform.Current.AdjustGameSettings(this.gs);
		this.RefreshGameOptionsControls();
	}

	// Token: 0x06001206 RID: 4614 RVA: 0x000533F3 File Offset: 0x000515F3
	private void LoadControllerSettings()
	{
		Debug.LogError("Not yet implemented.");
	}

	// Token: 0x06001207 RID: 4615 RVA: 0x000533FF File Offset: 0x000515FF
	private void RefreshAudioControls()
	{
		this.masterSlider.RefreshValueFromSettings();
		this.musicSlider.RefreshValueFromSettings();
		this.soundSlider.RefreshValueFromSettings();
	}

	// Token: 0x06001208 RID: 4616 RVA: 0x00053424 File Offset: 0x00051624
	private void RefreshVideoControls()
	{
		this.resolutionOption.RefreshControls();
		this.fullscreenOption.RefreshValueFromGameSettings(false);
		this.vsyncOption.RefreshValueFromGameSettings(true);
		this.overscanSetting.RefreshValueFromSettings();
		this.brightnessSetting.RefreshValueFromSettings();
		this.displayOption.RefreshControls();
		this.frameCapOption.RefreshValueFromGameSettings(true);
		this.particlesOption.RefreshValueFromGameSettings(false);
		this.shadersOption.RefreshValueFromGameSettings(false);
	}

	// Token: 0x06001209 RID: 4617 RVA: 0x00053499 File Offset: 0x00051699
	public void DisableFrameCapSetting()
	{
		if (this.frameCapOption)
		{
			this.frameCapOption.UpdateSetting(0);
			this.frameCapOption.RefreshValueFromGameSettings(false);
		}
	}

	// Token: 0x0600120A RID: 4618 RVA: 0x000534C0 File Offset: 0x000516C0
	public void DisableVsyncSetting()
	{
		if (this.vsyncOption)
		{
			this.vsyncOption.UpdateSetting(0);
			this.vsyncOption.RefreshValueFromGameSettings(false);
		}
	}

	// Token: 0x0600120B RID: 4619 RVA: 0x000534E7 File Offset: 0x000516E7
	private void RefreshKeyboardControls()
	{
		this.uiButtonSkins.RefreshKeyMappings();
	}

	// Token: 0x0600120C RID: 4620 RVA: 0x000534F4 File Offset: 0x000516F4
	private void RefreshGameOptionsControls()
	{
		this.languageSetting.RefreshControls();
		this.backerCreditsSetting.RefreshValueFromGameSettings(false);
		this.nativeAchievementsSetting.RefreshValueFromGameSettings(false);
		this.controllerRumbleSetting.RefreshValueFromGameSettings(true);
		this.nativeInputSetting.RefreshValueFromGameSettings(true);
	}

	// Token: 0x0600120D RID: 4621 RVA: 0x00053534 File Offset: 0x00051734
	public void RefreshAchievementsList()
	{
		AchievementsList achievementsList = this.gm.achievementHandler.achievementsList;
		int count = achievementsList.achievements.Count;
		if (this.menuAchievementsList.init)
		{
			for (int i = 0; i < count; i++)
			{
				Achievement achievement = achievementsList.achievements[i];
				MenuAchievement menuAchievement = this.menuAchievementsList.FindAchievement(achievement.key);
				if (menuAchievement != null)
				{
					this.UpdateMenuAchievementStatus(achievement, menuAchievement);
				}
				else
				{
					Debug.LogError("UI - Could not locate MenuAchievement " + achievement.key);
				}
			}
			return;
		}
		for (int j = 0; j < count; j++)
		{
			Achievement achievement2 = achievementsList.achievements[j];
			MenuAchievement menuAchievement2 = UnityEngine.Object.Instantiate<MenuAchievement>(this.menuAchievementsList.menuAchievementPrefab);
			menuAchievement2.transform.SetParent(this.achievementListRect.transform, false);
			menuAchievement2.name = achievement2.key;
			this.menuAchievementsList.AddMenuAchievement(menuAchievement2);
			this.UpdateMenuAchievementStatus(achievement2, menuAchievement2);
		}
		this.menuAchievementsList.MarkInit();
	}

	// Token: 0x0600120E RID: 4622 RVA: 0x0005363C File Offset: 0x0005183C
	private void UpdateMenuAchievementStatus(Achievement ach, MenuAchievement menuAch)
	{
		try
		{
			if (this.gm.IsAchievementAwarded(ach.key))
			{
				menuAch.icon.sprite = ach.earnedIcon;
				menuAch.icon.color = Color.white;
				menuAch.title.text = Language.Get(ach.localizedTitle, "Achievements");
				menuAch.text.text = Language.Get(ach.localizedText, "Achievements");
			}
			else if (ach.type == AchievementType.Normal)
			{
				menuAch.icon.sprite = ach.earnedIcon;
				menuAch.icon.color = new Color(0.57f, 0.57f, 0.57f, 0.57f);
				menuAch.title.text = Language.Get(ach.localizedTitle, "Achievements");
				menuAch.text.text = Language.Get(ach.localizedText, "Achievements");
			}
			else
			{
				menuAch.icon.sprite = this.hiddenIcon;
				menuAch.icon.color = new Color(0.57f, 0.57f, 0.57f, 0.57f);
				menuAch.title.text = Language.Get("HIDDEN_ACHIEVEMENT_TITLE", "Achievements");
				menuAch.text.text = Language.Get("HIDDEN_ACHIEVEMENT", "Achievements");
			}
		}
		catch (Exception)
		{
			if (ach.type == AchievementType.Normal)
			{
				menuAch.icon.sprite = ach.earnedIcon;
				menuAch.icon.color = new Color(0.57f, 0.57f, 0.57f, 0.57f);
				menuAch.title.text = Language.Get(ach.localizedTitle, "Achievements");
				menuAch.text.text = Language.Get(ach.localizedText, "Achievements");
			}
			else
			{
				menuAch.icon.sprite = this.hiddenIcon;
				menuAch.title.text = Language.Get("HIDDEN_ACHIEVEMENT_TITLE", "Achievements");
				menuAch.text.text = Language.Get("HIDDEN_ACHIEVEMENT", "Achievements");
			}
		}
	}

	// Token: 0x0600120F RID: 4623 RVA: 0x00053874 File Offset: 0x00051A74
	public void TogglePauseGame()
	{
		if (!this.ignoreUnpause)
		{
			this.togglePauseCo = base.StartCoroutine(this.gm.PauseGameToggleByMenu());
		}
	}

	// Token: 0x06001210 RID: 4624 RVA: 0x00053895 File Offset: 0x00051A95
	public void QuitGame()
	{
		this.ih.StopUIInput();
		this.StartMenuAnimationCoroutine(this.gm.QuitGame());
	}

	// Token: 0x06001211 RID: 4625 RVA: 0x000538B4 File Offset: 0x00051AB4
	public void FadeOutMenuAudio(float duration)
	{
		this.menuSilenceSnapshot.TransitionTo(duration);
	}

	// Token: 0x06001212 RID: 4626 RVA: 0x000538C2 File Offset: 0x00051AC2
	public void AudioGoToPauseMenu(float duration)
	{
		this.menuPauseSnapshot.TransitionTo(duration);
	}

	// Token: 0x06001213 RID: 4627 RVA: 0x000538D0 File Offset: 0x00051AD0
	public void AudioGoToGameplay(float duration)
	{
		this.gameplaySnapshot.TransitionTo(duration);
	}

	// Token: 0x06001214 RID: 4628 RVA: 0x000538E0 File Offset: 0x00051AE0
	public void ConfigureMenu()
	{
		if (this.mainMenuButtons != null)
		{
			this.mainMenuButtons.ConfigureNavigation();
		}
		if (this.gameMenuOptions != null)
		{
			this.gameMenuOptions.ConfigureNavigation();
		}
		if (this.videoMenuOptions != null)
		{
			this.videoMenuOptions.ConfigureNavigation();
		}
		if (this.uiState == UIState.MAIN_MENU_HOME)
		{
			if (this.slotOne != null)
			{
				this.slotOne.healthSlots.Awake();
			}
			if (this.slotTwo != null)
			{
				this.slotTwo.healthSlots.Awake();
			}
			if (this.slotThree != null)
			{
				this.slotThree.healthSlots.Awake();
			}
			if (this.slotFour != null)
			{
				this.slotFour.healthSlots.Awake();
			}
		}
	}

	// Token: 0x06001215 RID: 4629 RVA: 0x000539BC File Offset: 0x00051BBC
	public IEnumerator HideCurrentMenu()
	{
		if (this.menuState == MainMenuState.DYNAMIC_MENU)
		{
			Action beforeHideDynamicMenu = UIManager.BeforeHideDynamicMenu;
			if (beforeHideDynamicMenu != null)
			{
				beforeHideDynamicMenu();
			}
			return this.HideMenu(this.currentDynamicMenu);
		}
		return this.orig_HideCurrentMenu();
	}

	// Token: 0x06001216 RID: 4630 RVA: 0x000539EB File Offset: 0x00051BEB
	public IEnumerator ShowMenu(MenuScreen menu)
	{
		this.isFadingMenu = true;
		this.ih.StopUIInput();
		if (menu.screenCanvasGroup != null)
		{
			this.StartCoroutine(this.FadeInCanvasGroup(menu.screenCanvasGroup));
		}
		if (menu.title != null)
		{
			this.StartCoroutine(this.FadeInCanvasGroup(menu.title));
		}
		if (menu.topFleur != null)
		{
			yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
			menu.topFleur.ResetTrigger("hide");
			menu.topFleur.SetTrigger("show");
		}
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
		if (menu.content != null)
		{
			this.StartCoroutine(this.FadeInCanvasGroup(menu.content));
		}
		if (menu.controls != null)
		{
			this.StartCoroutine(this.FadeInCanvasGroup(menu.controls));
		}
		if (menu.bottomFleur != null)
		{
			menu.bottomFleur.ResetTrigger("hide");
			menu.bottomFleur.SetTrigger("show");
		}
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
		this.ih.StartUIInput();
		yield return null;
		menu.HighlightDefault();
		this.isFadingMenu = false;
		yield break;
	}

	// Token: 0x06001217 RID: 4631 RVA: 0x00053A01 File Offset: 0x00051C01
	public IEnumerator HideMenu(MenuScreen menu)
	{
		this.isFadingMenu = true;
		this.ih.StopUIInput();
		if (menu.title != null)
		{
			this.StartCoroutine(this.FadeOutCanvasGroup(menu.title));
		}
		if (menu.topFleur != null)
		{
			yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
			menu.topFleur.ResetTrigger("show");
			menu.topFleur.SetTrigger("hide");
		}
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
		if (menu.content != null)
		{
			this.StartCoroutine(this.FadeOutCanvasGroup(menu.content));
		}
		if (menu.controls != null)
		{
			this.StartCoroutine(this.FadeOutCanvasGroup(menu.controls));
		}
		if (menu.bottomFleur != null)
		{
			yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
			menu.bottomFleur.ResetTrigger("show");
			menu.bottomFleur.SetTrigger("hide");
		}
		yield return this.StartCoroutine(this.FadeOutCanvasGroup(menu.screenCanvasGroup));
		this.ih.StartUIInput();
		this.isFadingMenu = false;
		yield break;
	}

	// Token: 0x06001218 RID: 4632 RVA: 0x00053A18 File Offset: 0x00051C18
	public void HideMenuInstant(MenuScreen menu)
	{
		this.ih.StopUIInput();
		if (menu.title != null)
		{
			this.HideCanvasGroup(menu.title);
		}
		if (menu.topFleur != null)
		{
			menu.topFleur.ResetTrigger("show");
			menu.topFleur.SetTrigger("hide");
		}
		if (menu.content != null)
		{
			this.HideCanvasGroup(menu.content);
		}
		if (menu.controls != null)
		{
			this.HideCanvasGroup(menu.controls);
		}
		this.HideCanvasGroup(menu.screenCanvasGroup);
		this.ih.StartUIInput();
	}

	// Token: 0x06001219 RID: 4633 RVA: 0x00053AC3 File Offset: 0x00051CC3
	public IEnumerator HideSaveProfileMenu()
	{
		this.StartCoroutine(this.FadeOutCanvasGroup(this.saveProfileTitle));
		this.saveProfileTopFleur.ResetTrigger("show");
		this.saveProfileTopFleur.SetTrigger("hide");
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.165f));
		this.slotOne.HideSaveSlot();
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.165f));
		this.slotTwo.HideSaveSlot();
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.165f));
		this.slotThree.HideSaveSlot();
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.165f));
		this.slotFour.HideSaveSlot();
		yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.33f));
		yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.saveProfileControls));
		yield return this.StartCoroutine(this.FadeOutCanvasGroup(this.saveProfileScreen));
		yield break;
	}

	// Token: 0x0600121A RID: 4634 RVA: 0x00053AD4 File Offset: 0x00051CD4
	private void DisableScreens()
	{
		for (int i = 0; i < this.UICanvas.transform.childCount; i++)
		{
			if (!(this.UICanvas.transform.GetChild(i).name == "PauseMenuScreen"))
			{
				this.UICanvas.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
		if (this.achievementsPopupPanel)
		{
			this.achievementsPopupPanel.SetActive(true);
		}
	}

	// Token: 0x0600121B RID: 4635 RVA: 0x00053B53 File Offset: 0x00051D53
	private void ShowCanvas(Canvas canvas)
	{
		canvas.gameObject.SetActive(true);
	}

	// Token: 0x0600121C RID: 4636 RVA: 0x00053B61 File Offset: 0x00051D61
	private void HideCanvas(Canvas canvas)
	{
		canvas.gameObject.SetActive(false);
	}

	// Token: 0x0600121D RID: 4637 RVA: 0x00053B6F File Offset: 0x00051D6F
	public void ShowCanvasGroup(CanvasGroup cg)
	{
		cg.gameObject.SetActive(true);
		cg.interactable = true;
		cg.alpha = 1f;
	}

	// Token: 0x0600121E RID: 4638 RVA: 0x00053B8F File Offset: 0x00051D8F
	public void HideCanvasGroup(CanvasGroup cg)
	{
		cg.interactable = false;
		cg.alpha = 0f;
		cg.gameObject.SetActive(false);
	}

	// Token: 0x0600121F RID: 4639 RVA: 0x00053BAF File Offset: 0x00051DAF
	public IEnumerator FadeInCanvasGroup(CanvasGroup cg)
	{
		float loopFailsafe = 0f;
		cg.alpha = 0f;
		cg.gameObject.SetActive(true);
		while (cg.alpha < 1f)
		{
			cg.alpha += Time.unscaledDeltaTime * this.MENU_FADE_SPEED;
			loopFailsafe += Time.unscaledDeltaTime;
			if (cg.alpha >= 0.95f)
			{
				cg.alpha = 1f;
				break;
			}
			if (loopFailsafe >= 2f)
			{
				break;
			}
			yield return null;
		}
		cg.alpha = 1f;
		cg.interactable = true;
		cg.gameObject.SetActive(true);
		yield return null;
		yield break;
	}

	// Token: 0x06001220 RID: 4640 RVA: 0x00053BC5 File Offset: 0x00051DC5
	public IEnumerator FadeInCanvasGroupAlpha(CanvasGroup cg, float endAlpha)
	{
		float loopFailsafe = 0f;
		if (endAlpha > 1f)
		{
			endAlpha = 1f;
		}
		cg.alpha = 0f;
		cg.gameObject.SetActive(true);
		while (cg.alpha < endAlpha - 0.05f)
		{
			cg.alpha += Time.unscaledDeltaTime * this.MENU_FADE_SPEED;
			loopFailsafe += Time.unscaledDeltaTime;
			if (cg.alpha >= endAlpha - 0.05f)
			{
				cg.alpha = endAlpha;
				break;
			}
			if (loopFailsafe >= 2f)
			{
				break;
			}
			yield return null;
		}
		cg.alpha = endAlpha;
		cg.interactable = true;
		cg.gameObject.SetActive(true);
		yield return null;
		yield break;
	}

	// Token: 0x06001221 RID: 4641 RVA: 0x00053BE2 File Offset: 0x00051DE2
	public IEnumerator FadeOutCanvasGroup(CanvasGroup cg)
	{
		float loopFailsafe = 0f;
		cg.interactable = false;
		while (cg.alpha > 0.05f)
		{
			cg.alpha -= Time.unscaledDeltaTime * this.MENU_FADE_SPEED;
			loopFailsafe += Time.unscaledDeltaTime;
			if (cg.alpha <= 0.05f || loopFailsafe >= 2f)
			{
				break;
			}
			yield return null;
		}
		cg.alpha = 0f;
		cg.gameObject.SetActive(false);
		yield return null;
		yield break;
	}

	// Token: 0x06001222 RID: 4642 RVA: 0x00053BF8 File Offset: 0x00051DF8
	private IEnumerator FadeInSprite(SpriteRenderer sprite)
	{
		while (sprite.color.a < 1f)
		{
			sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a + Time.unscaledDeltaTime * this.MENU_FADE_SPEED);
			yield return null;
		}
		sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f);
		yield return null;
		yield break;
	}

	// Token: 0x06001223 RID: 4643 RVA: 0x00053C0E File Offset: 0x00051E0E
	private IEnumerator FadeOutSprite(SpriteRenderer sprite)
	{
		while (sprite.color.a > 0f)
		{
			sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a - Time.unscaledDeltaTime * this.MENU_FADE_SPEED);
			yield return null;
		}
		sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0f);
		yield return null;
		yield break;
	}

	// Token: 0x06001224 RID: 4644 RVA: 0x00053C24 File Offset: 0x00051E24
	private void EnableChildren(RectTransform parent)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			parent.GetChild(i).gameObject.SetActive(true);
		}
	}

	// Token: 0x06001225 RID: 4645 RVA: 0x00053C54 File Offset: 0x00051E54
	private void EnableChildren(Canvas parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			parent.transform.GetChild(i).gameObject.SetActive(true);
		}
	}

	// Token: 0x06001226 RID: 4646 RVA: 0x00053C90 File Offset: 0x00051E90
	private void DisableChildren(Canvas parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			parent.transform.GetChild(i).gameObject.SetActive(false);
		}
	}

	// Token: 0x06001227 RID: 4647 RVA: 0x00053CCC File Offset: 0x00051ECC
	private float GetAnimationClipLength(Animator animator, string clipName)
	{
		RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
		for (int i = 0; i < runtimeAnimatorController.animationClips.Length; i++)
		{
			if (runtimeAnimatorController.animationClips[i].name == clipName)
			{
				return runtimeAnimatorController.animationClips[i].length;
			}
		}
		return -1f;
	}

	// Token: 0x06001228 RID: 4648 RVA: 0x00053D1C File Offset: 0x00051F1C
	public void MakeMenuLean()
	{
		Debug.Log("Making UI menu lean.");
		if (this.saveProfileScreen)
		{
			UnityEngine.Object.Destroy(this.saveProfileScreen.gameObject);
			this.saveProfileScreen = null;
		}
		if (this.achievementsMenuScreen)
		{
			UnityEngine.Object.Destroy(this.achievementsMenuScreen.gameObject);
			this.achievementsMenuScreen = null;
		}
		if (!Platform.Current.WillDisplayGraphicsSettings)
		{
			if (this.videoMenuScreen)
			{
				UnityEngine.Object.Destroy(this.videoMenuScreen.gameObject);
				this.videoMenuScreen = null;
			}
			if (this.brightnessMenuScreen)
			{
				UnityEngine.Object.Destroy(this.brightnessMenuScreen.gameObject);
				this.brightnessMenuScreen = null;
			}
			if (this.overscanMenuScreen)
			{
				UnityEngine.Object.Destroy(this.overscanMenuScreen.gameObject);
				this.overscanMenuScreen = null;
			}
		}
	}

	// Token: 0x06001229 RID: 4649 RVA: 0x00053DF3 File Offset: 0x00051FF3
	public UIManager()
	{
		this.MENU_FADE_SPEED = 3.2f;
		base..ctor();
	}

	// Token: 0x17000219 RID: 537
	// (get) Token: 0x0600122C RID: 4652 RVA: 0x00053E1C File Offset: 0x0005201C
	// (set) Token: 0x0600122D RID: 4653 RVA: 0x00053E24 File Offset: 0x00052024
	public MenuScreen currentDynamicMenu { get; set; }

	// Token: 0x1400002C RID: 44
	// (add) Token: 0x0600122E RID: 4654 RVA: 0x00053E2D File Offset: 0x0005202D
	// (remove) Token: 0x0600122F RID: 4655 RVA: 0x00053E63 File Offset: 0x00052063
	public static event Action EditMenus
	{
		add
		{
			UIManager._editMenus = (Action)Delegate.Combine(UIManager._editMenus, value);
			if (UIManager._instance != null && UIManager._instance.hasCalledEditMenus)
			{
				value();
			}
		}
		remove
		{
			UIManager._editMenus = (Action)Delegate.Remove(UIManager._editMenus, value);
		}
	}

	// Token: 0x1400002D RID: 45
	// (add) Token: 0x06001230 RID: 4656 RVA: 0x00053E7C File Offset: 0x0005207C
	// (remove) Token: 0x06001231 RID: 4657 RVA: 0x00053EB0 File Offset: 0x000520B0
	public static event Action BeforeHideDynamicMenu;

	// Token: 0x06001232 RID: 4658 RVA: 0x00053EE4 File Offset: 0x000520E4
	public static UIManager orig_get_instance()
	{
		if (UIManager._instance == null)
		{
			UIManager._instance = UnityEngine.Object.FindObjectOfType<UIManager>();
			if (UIManager._instance == null)
			{
				Debug.LogError("Couldn't find a UIManager, make sure one exists in the scene.");
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(UIManager._instance.gameObject);
			}
		}
		return UIManager._instance;
	}

	// Token: 0x06001233 RID: 4659 RVA: 0x00053F3A File Offset: 0x0005213A
	private Sprite LoadImage()
	{
		return Assembly.GetExecutingAssembly().LoadEmbeddedSprite("Modding.logo.png", 100f);
	}

	// Token: 0x06001234 RID: 4660 RVA: 0x00053F50 File Offset: 0x00052150
	private void orig_Awake()
	{
		if (UIManager._instance == null)
		{
			UIManager._instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		else if (this != UIManager._instance)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.graphicRaycaster = base.GetComponentInChildren<GraphicRaycaster>();
	}

	// Token: 0x06001235 RID: 4661 RVA: 0x00053FA0 File Offset: 0x000521A0
	private void orig_Start()
	{
		if (this == UIManager._instance)
		{
			this.SetupRefs();
			if (this.gm.IsMenuScene())
			{
				this.startMenuTime = Time.time + 0.5f;
				GameCameras.instance.cameraController.FadeSceneIn();
				this.LoadStoredSettings();
				if (Platform.Current.AreAchievementsFetched)
				{
					this.RefreshAchievementsList();
				}
				else
				{
					Platform.AchievementsFetched += delegate()
					{
						this.RefreshAchievementsList();
					};
				}
				this.ConfigureMenu();
			}
			if (this.graphicRaycaster && InputHandler.Instance)
			{
				InputHandler.Instance.OnCursorVisibilityChange += delegate(bool isVisible)
				{
					this.graphicRaycaster.enabled = isVisible;
				};
			}
			if (Platform.Current.WillPreloadSaveFiles)
			{
				this.slotOne.Prepare(this.gm, false);
				this.slotTwo.Prepare(this.gm, false);
				this.slotThree.Prepare(this.gm, false);
				this.slotFour.Prepare(this.gm, false);
			}
		}
	}

	// Token: 0x06001236 RID: 4662 RVA: 0x000540A4 File Offset: 0x000522A4
	public IEnumerator orig_HideCurrentMenu()
	{
		this.isFadingMenu = true;
		MenuScreen menu;
		switch (this.menuState)
		{
		case MainMenuState.OPTIONS_MENU:
			menu = this.optionsMenuScreen;
			goto IL_268;
		case MainMenuState.GAMEPAD_MENU:
			menu = this.gamepadMenuScreen;
			this.gs.SaveGameOptionsSettings();
			goto IL_268;
		case MainMenuState.KEYBOARD_MENU:
			menu = this.keyboardMenuScreen;
			this.ih.SendKeyBindingsToGameSettings();
			this.gs.SaveKeyboardSettings();
			goto IL_268;
		case MainMenuState.AUDIO_MENU:
			menu = this.audioMenuScreen;
			this.gs.SaveAudioSettings();
			goto IL_268;
		case MainMenuState.VIDEO_MENU:
			menu = this.videoMenuScreen;
			this.gs.SaveVideoSettings();
			goto IL_268;
		case MainMenuState.EXIT_PROMPT:
			menu = this.returnMainMenuPrompt;
			goto IL_268;
		case MainMenuState.OVERSCAN_MENU:
			menu = this.overscanMenuScreen;
			goto IL_268;
		case MainMenuState.GAME_OPTIONS_MENU:
			menu = this.gameOptionsMenuScreen;
			this.gs.SaveGameOptionsSettings();
			goto IL_268;
		case MainMenuState.ACHIEVEMENTS_MENU:
			menu = this.achievementsMenuScreen;
			goto IL_268;
		case MainMenuState.QUIT_GAME_PROMPT:
			menu = this.quitGamePrompt;
			goto IL_268;
		case MainMenuState.RESOLUTION_PROMPT:
			menu = this.resolutionPrompt;
			goto IL_268;
		case MainMenuState.BRIGHTNESS_MENU:
			menu = this.brightnessMenuScreen;
			this.gs.SaveBrightnessSettings();
			goto IL_268;
		case MainMenuState.PAUSE_MENU:
			menu = this.pauseMenuScreen;
			goto IL_268;
		case MainMenuState.PLAY_MODE_MENU:
			menu = this.playModeMenuScreen;
			goto IL_268;
		case MainMenuState.EXTRAS_MENU:
			menu = this.extrasMenuScreen;
			goto IL_268;
		case MainMenuState.REMAP_GAMEPAD_MENU:
			menu = this.remapGamepadMenuScreen;
			if (this.uiButtonSkins.listeningButton != null)
			{
				this.uiButtonSkins.listeningButton.StopActionListening();
				this.uiButtonSkins.listeningButton.AbortRebind();
			}
			this.ih.SendButtonBindingsToGameSettings();
			this.gs.SaveGamepadSettings(this.ih.activeGamepadType);
			goto IL_268;
		case MainMenuState.ENGAGE_MENU:
			menu = this.engageMenuScreen;
			goto IL_268;
		case MainMenuState.NO_SAVE_MENU:
			menu = this.noSaveMenuScreen;
			goto IL_268;
		}
		yield break;
		IL_268:
		this.ih.StopUIInput();
		if (menu.title != null)
		{
			this.StartCoroutine(this.FadeOutCanvasGroup(menu.title));
			yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
		}
		if (menu.topFleur != null)
		{
			menu.topFleur.ResetTrigger("show");
			menu.topFleur.SetTrigger("hide");
			yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
		}
		if (menu.content != null)
		{
			this.StartCoroutine(this.FadeOutCanvasGroup(menu.content));
		}
		if (menu.controls != null)
		{
			this.StartCoroutine(this.FadeOutCanvasGroup(menu.controls));
		}
		if (menu.bottomFleur != null)
		{
			menu.bottomFleur.ResetTrigger("show");
			menu.bottomFleur.SetTrigger("hide");
			yield return this.StartCoroutine(this.gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
		}
		if (menu.screenCanvasGroup != null)
		{
			yield return this.StartCoroutine(this.FadeOutCanvasGroup(menu.screenCanvasGroup));
		}
		this.ih.StartUIInput();
		this.isFadingMenu = false;
		yield break;
	}

	// Token: 0x06001237 RID: 4663 RVA: 0x000540B3 File Offset: 0x000522B3
	public void UIGoToDynamicMenu(MenuScreen menu)
	{
		this.StartMenuAnimationCoroutine(this.GoToDynamicMenu(menu));
	}

	// Token: 0x06001238 RID: 4664 RVA: 0x000540C3 File Offset: 0x000522C3
	public IEnumerator GoToDynamicMenu(MenuScreen menu)
	{
		UIManager.<GoToDynamicMenu>d__25 <GoToDynamicMenu>d__ = new UIManager.<GoToDynamicMenu>d__25(0);
		<GoToDynamicMenu>d__.<>4__this = this;
		<GoToDynamicMenu>d__.menu = menu;
		return <GoToDynamicMenu>d__;
	}

	// Token: 0x06001239 RID: 4665 RVA: 0x000540D9 File Offset: 0x000522D9
	public void UILeaveDynamicMenu(MenuScreen to, MainMenuState state)
	{
		this.StartMenuAnimationCoroutine(this.LeaveDynamicMenu(to, state));
	}

	// Token: 0x0600123A RID: 4666 RVA: 0x000540EA File Offset: 0x000522EA
	public IEnumerator LeaveDynamicMenu(MenuScreen to, MainMenuState state)
	{
		UIManager.<LeaveDynamicMenu>d__27 <LeaveDynamicMenu>d__ = new UIManager.<LeaveDynamicMenu>d__27(0);
		<LeaveDynamicMenu>d__.<>4__this = this;
		<LeaveDynamicMenu>d__.to = to;
		<LeaveDynamicMenu>d__.state = state;
		return <LeaveDynamicMenu>d__;
	}

	// Token: 0x0600123B RID: 4667 RVA: 0x00054107 File Offset: 0x00052307
	public void UIPauseToDynamicMenu(MenuScreen to)
	{
		if (this.uiState != UIState.PAUSED)
		{
			this.StartMenuAnimationCoroutine(this.PauseToDynamicMenu(to));
			this.uiState = UIState.PAUSED;
		}
	}

	// Token: 0x0600123C RID: 4668 RVA: 0x00054127 File Offset: 0x00052327
	public IEnumerator PauseToDynamicMenu(MenuScreen to)
	{
		UIManager.<PauseToDynamicMenu>d__30 <PauseToDynamicMenu>d__ = new UIManager.<PauseToDynamicMenu>d__30(0);
		<PauseToDynamicMenu>d__.<>4__this = this;
		<PauseToDynamicMenu>d__.to = to;
		return <PauseToDynamicMenu>d__;
	}

	// Token: 0x04001165 RID: 4453
	private GameManager gm;

	// Token: 0x04001166 RID: 4454
	private GameSettings gs;

	// Token: 0x04001167 RID: 4455
	private HeroController hero_ctrl;

	// Token: 0x04001168 RID: 4456
	private PlayerData playerData;

	// Token: 0x04001169 RID: 4457
	private InputHandler ih;

	// Token: 0x0400116A RID: 4458
	public MenuAudioController uiAudioPlayer;

	// Token: 0x0400116B RID: 4459
	public HollowKnightInputModule inputModule;

	// Token: 0x0400116C RID: 4460
	[Space]
	public float MENU_FADE_SPEED;

	// Token: 0x0400116D RID: 4461
	private const float MENU_FADE_DELAY = 0.1f;

	// Token: 0x0400116E RID: 4462
	private const float MENU_MODAL_DIMMER_ALPHA = 0.8f;

	// Token: 0x0400116F RID: 4463
	private const float MENU_FADE_ALPHA_TOLERANCE = 0.05f;

	// Token: 0x04001170 RID: 4464
	private const float MENU_FADE_FAILSAFE = 2f;

	// Token: 0x04001171 RID: 4465
	[Header("State")]
	[Space(6f)]
	public UIState uiState;

	// Token: 0x04001172 RID: 4466
	public MainMenuState menuState;

	// Token: 0x04001173 RID: 4467
	[Header("Event System")]
	[Space(6f)]
	public EventSystem eventSystem;

	// Token: 0x04001174 RID: 4468
	[Header("Main Elements")]
	[Space(6f)]
	public Canvas UICanvas;

	// Token: 0x04001175 RID: 4469
	public CanvasGroup modalDimmer;

	// Token: 0x04001176 RID: 4470
	public CanvasScaler canvasScaler;

	// Token: 0x04001177 RID: 4471
	[Header("Menu Audio")]
	[Space(6f)]
	public AudioMixerSnapshot gameplaySnapshot;

	// Token: 0x04001178 RID: 4472
	public AudioMixerSnapshot menuSilenceSnapshot;

	// Token: 0x04001179 RID: 4473
	public AudioMixerSnapshot menuPauseSnapshot;

	// Token: 0x0400117A RID: 4474
	[Header("Main Menu")]
	[Space(6f)]
	public CanvasGroup mainMenuScreen;

	// Token: 0x0400117B RID: 4475
	public MainMenuOptions mainMenuButtons;

	// Token: 0x0400117C RID: 4476
	public SpriteRenderer gameTitle;

	// Token: 0x0400117D RID: 4477
	public PlayMakerFSM subtitleFSM;

	// Token: 0x0400117E RID: 4478
	[Header("Save Profile Menu")]
	[Space(6f)]
	public CanvasGroup saveProfileScreen;

	// Token: 0x0400117F RID: 4479
	public CanvasGroup saveProfileTitle;

	// Token: 0x04001180 RID: 4480
	public CanvasGroup saveProfileControls;

	// Token: 0x04001181 RID: 4481
	public Animator saveProfileTopFleur;

	// Token: 0x04001182 RID: 4482
	public PreselectOption saveSlots;

	// Token: 0x04001183 RID: 4483
	public SaveSlotButton slotOne;

	// Token: 0x04001184 RID: 4484
	public SaveSlotButton slotTwo;

	// Token: 0x04001185 RID: 4485
	public SaveSlotButton slotThree;

	// Token: 0x04001186 RID: 4486
	public SaveSlotButton slotFour;

	// Token: 0x04001187 RID: 4487
	public CheckpointSprite checkpointSprite;

	// Token: 0x04001188 RID: 4488
	[Header("Options Menu")]
	[Space(6f)]
	public MenuScreen optionsMenuScreen;

	// Token: 0x04001189 RID: 4489
	[Header("Game Options Menu")]
	[Space(6f)]
	public MenuScreen gameOptionsMenuScreen;

	// Token: 0x0400118A RID: 4490
	public GameMenuOptions gameMenuOptions;

	// Token: 0x0400118B RID: 4491
	public MenuLanguageSetting languageSetting;

	// Token: 0x0400118C RID: 4492
	public MenuSetting backerCreditsSetting;

	// Token: 0x0400118D RID: 4493
	public MenuSetting nativeAchievementsSetting;

	// Token: 0x0400118E RID: 4494
	public MenuSetting controllerRumbleSetting;

	// Token: 0x0400118F RID: 4495
	public MenuSetting nativeInputSetting;

	// Token: 0x04001190 RID: 4496
	[Header("Audio Menu")]
	[Space(6f)]
	public MenuScreen audioMenuScreen;

	// Token: 0x04001191 RID: 4497
	public MenuAudioSlider masterSlider;

	// Token: 0x04001192 RID: 4498
	public MenuAudioSlider musicSlider;

	// Token: 0x04001193 RID: 4499
	public MenuAudioSlider soundSlider;

	// Token: 0x04001194 RID: 4500
	[Header("Video Menu")]
	[Space(6f)]
	public MenuScreen videoMenuScreen;

	// Token: 0x04001195 RID: 4501
	public VideoMenuOptions videoMenuOptions;

	// Token: 0x04001196 RID: 4502
	public MenuResolutionSetting resolutionOption;

	// Token: 0x04001197 RID: 4503
	public ResolutionCountdownTimer countdownTimer;

	// Token: 0x04001198 RID: 4504
	public MenuSetting fullscreenOption;

	// Token: 0x04001199 RID: 4505
	public MenuSetting vsyncOption;

	// Token: 0x0400119A RID: 4506
	public MenuSetting particlesOption;

	// Token: 0x0400119B RID: 4507
	public MenuSetting shadersOption;

	// Token: 0x0400119C RID: 4508
	public MenuDisplaySetting displayOption;

	// Token: 0x0400119D RID: 4509
	public MenuSetting frameCapOption;

	// Token: 0x0400119E RID: 4510
	[Header("Controller Options Menu")]
	[Space(6f)]
	public MenuScreen gamepadMenuScreen;

	// Token: 0x0400119F RID: 4511
	public ControllerDetect controllerDetect;

	// Token: 0x040011A0 RID: 4512
	[Header("Controller Remap Menu")]
	[Space(6f)]
	public MenuScreen remapGamepadMenuScreen;

	// Token: 0x040011A1 RID: 4513
	[Header("Keyboard Options Menu")]
	[Space(6f)]
	public MenuScreen keyboardMenuScreen;

	// Token: 0x040011A2 RID: 4514
	[Header("Overscan Setting Menu")]
	[Space(6f)]
	public MenuScreen overscanMenuScreen;

	// Token: 0x040011A3 RID: 4515
	public OverscanSetting overscanSetting;

	// Token: 0x040011A4 RID: 4516
	[Header("Brightness Setting Menu")]
	[Space(6f)]
	public MenuScreen brightnessMenuScreen;

	// Token: 0x040011A5 RID: 4517
	public BrightnessSetting brightnessSetting;

	// Token: 0x040011A6 RID: 4518
	[Header("Achievements Menu")]
	[Space(6f)]
	public MenuScreen achievementsMenuScreen;

	// Token: 0x040011A7 RID: 4519
	public RectTransform achievementListRect;

	// Token: 0x040011A8 RID: 4520
	public MenuAchievementsList menuAchievementsList;

	// Token: 0x040011A9 RID: 4521
	public Sprite hiddenIcon;

	// Token: 0x040011AA RID: 4522
	public GameObject achievementsPopupPanel;

	// Token: 0x040011AB RID: 4523
	[Header("Extras Menu")]
	[Space(6f)]
	public MenuScreen extrasMenuScreen;

	// Token: 0x040011AC RID: 4524
	public MenuScreen extrasContentMenuScreen;

	// Token: 0x040011AD RID: 4525
	[Header("Play Mode Menu")]
	[Space(6f)]
	public MenuScreen playModeMenuScreen;

	// Token: 0x040011AE RID: 4526
	[Header("Pause Menu")]
	[Space(6f)]
	public Animator pauseMenuAnimator;

	// Token: 0x040011AF RID: 4527
	public MenuScreen pauseMenuScreen;

	// Token: 0x040011B0 RID: 4528
	[Header("Engage Menu")]
	[Space(6f)]
	public MenuScreen engageMenuScreen;

	// Token: 0x040011B1 RID: 4529
	public bool didLeaveEngageMenu;

	// Token: 0x040011B2 RID: 4530
	public MenuScreen noSaveMenuScreen;

	// Token: 0x040011B3 RID: 4531
	[Header("Prompts")]
	[Space(6f)]
	public MenuScreen quitGamePrompt;

	// Token: 0x040011B4 RID: 4532
	public MenuScreen returnMainMenuPrompt;

	// Token: 0x040011B5 RID: 4533
	public MenuScreen resolutionPrompt;

	// Token: 0x040011B6 RID: 4534
	[Header("Cinematics")]
	[SerializeField]
	private CinematicSkipPopup cinematicSkipPopup;

	// Token: 0x040011B7 RID: 4535
	[Header("Button Skins")]
	[Space(6f)]
	public UIButtonSkins uiButtonSkins;

	// Token: 0x040011B8 RID: 4536
	private bool clearSaveFile;

	// Token: 0x040011B9 RID: 4537
	private Selectable lastSelected;

	// Token: 0x040011BA RID: 4538
	private bool lastSubmitWasMouse;

	// Token: 0x040011BB RID: 4539
	private MenuScreen activePrompt;

	// Token: 0x040011BC RID: 4540
	private bool ignoreUnpause;

	// Token: 0x040011BD RID: 4541
	private bool isFadingMenu;

	// Token: 0x040011BE RID: 4542
	private float startMenuTime;

	// Token: 0x040011BF RID: 4543
	private const float startMenuDelay = 0.5f;

	// Token: 0x040011C0 RID: 4544
	private Coroutine togglePauseCo;

	// Token: 0x040011C1 RID: 4545
	private Coroutine goToPauseMenuCo;

	// Token: 0x040011C2 RID: 4546
	private Coroutine leavePauseMenuCo;

	// Token: 0x040011C3 RID: 4547
	private GraphicRaycaster graphicRaycaster;

	// Token: 0x040011C4 RID: 4548
	private int menuAnimationCounter;

	// Token: 0x040011C5 RID: 4549
	private static UIManager _instance;

	// Token: 0x040011C6 RID: 4550
	private bool permaDeath;

	// Token: 0x040011C7 RID: 4551
	private bool bossRush;

	// Token: 0x040011C9 RID: 4553
	private static Action _editMenus;

	// Token: 0x040011CA RID: 4554
	private bool hasCalledEditMenus;
}
