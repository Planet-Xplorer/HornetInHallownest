using System;
using System.Collections.Generic;
using System.Text;
using TeamCherry.BuildBot;
using TeamCherry.SharedUtils;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

// Token: 0x0200035F RID: 863
public class PerformanceHud : MonoBehaviour, IOnGUI
{
	// Token: 0x170002F6 RID: 758
	// (get) Token: 0x06001DE0 RID: 7648 RVA: 0x0008975F File Offset: 0x0008795F
	// (set) Token: 0x06001DE1 RID: 7649 RVA: 0x00089766 File Offset: 0x00087966
	public static PerformanceHud Shared { get; private set; }

	// Token: 0x06001DE2 RID: 7650 RVA: 0x0008976E File Offset: 0x0008796E
	public static void Init()
	{
		if (PerformanceHud.Shared != null)
		{
			return;
		}
		GameObject gameObject = new GameObject("PerformanceHud");
		PerformanceHud.Shared = gameObject.AddComponent<PerformanceHud>();
		PerformanceHud.Shared.DisplayState = PerformanceHud.DisplayStates.Hidden;
		Object.DontDestroyOnLoad(gameObject);
	}

	// Token: 0x06001DE3 RID: 7651 RVA: 0x000897A3 File Offset: 0x000879A3
	public static void ReInit()
	{
		if (PerformanceHud.Shared)
		{
			Object.Destroy(PerformanceHud.Shared.gameObject);
			PerformanceHud.Shared = null;
		}
		PerformanceHud.Init();
	}

	// Token: 0x170002F7 RID: 759
	// (get) Token: 0x06001DE4 RID: 7652 RVA: 0x000897CB File Offset: 0x000879CB
	// (set) Token: 0x06001DE5 RID: 7653 RVA: 0x000897D2 File Offset: 0x000879D2
	public static bool ShowVibration
	{
		get
		{
			return PerformanceHud._showVibrations;
		}
		set
		{
			if (PerformanceHud._showVibrations != value)
			{
				PerformanceHud._showVibrations = value;
				if (PerformanceHud.Shared != null)
				{
					PerformanceHud.Shared.UpdateDrawState();
				}
			}
		}
	}

	// Token: 0x170002F8 RID: 760
	// (get) Token: 0x06001DE6 RID: 7654 RVA: 0x000897F9 File Offset: 0x000879F9
	private GUIStyle RightAlignedStyle
	{
		get
		{
			if (this.rightAlignedStyle == null)
			{
				this.rightAlignedStyle = new GUIStyle(GUI.skin.label);
				this.rightAlignedStyle.alignment = TextAnchor.MiddleRight;
			}
			return this.rightAlignedStyle;
		}
	}

	// Token: 0x170002F9 RID: 761
	// (get) Token: 0x06001DE7 RID: 7655 RVA: 0x0008982A File Offset: 0x00087A2A
	private static int LineHeight
	{
		get
		{
			return Mathf.RoundToInt(24f * CheatManager.Multiplier);
		}
	}

	// Token: 0x170002FA RID: 762
	// (get) Token: 0x06001DE8 RID: 7656 RVA: 0x0008983C File Offset: 0x00087A3C
	// (set) Token: 0x06001DE9 RID: 7657 RVA: 0x00089844 File Offset: 0x00087A44
	public bool IsMonoGroupEnabled
	{
		get
		{
			return this.isMonoGroupEnabled;
		}
		set
		{
			this.isMonoGroupEnabled = value;
			if (this.monoGroup != null)
			{
				this.monoGroup.style.display = (value ? DisplayStyle.Flex : DisplayStyle.None);
			}
		}
	}

	// Token: 0x170002FB RID: 763
	// (get) Token: 0x06001DEA RID: 7658 RVA: 0x00089871 File Offset: 0x00087A71
	// (set) Token: 0x06001DEB RID: 7659 RVA: 0x0008987C File Offset: 0x00087A7C
	public PerformanceHud.DisplayStates DisplayState
	{
		get
		{
			return this.displayState;
		}
		set
		{
			if (this.displayState == value)
			{
				return;
			}
			PerformanceHud.DisplayStates displayStates = this.displayState;
			this.displayState = value;
			this.UpdateDrawState();
			if (this.displayState == PerformanceHud.DisplayStates.Hidden)
			{
				if (this.uiDoc)
				{
					this.uiDoc.visualTreeAsset = null;
					this.sceneLabel = null;
					this.cpuMemLabel = null;
					this.resolutionLabel = null;
					this.fpsLabel = null;
					this.profileLabel = null;
					this.monoGroup = null;
					this.gcLabel = null;
					this.heapLabel = null;
					this.fpsStringValues = null;
					this.lastScreenHeight = 0;
					this.lastScreenWidth = 0;
					this.lastScreenHeightScaled = 0;
					this.lastScreenWidthScaled = 0;
					this.previousFpsValue = 0;
					this.lastGcMode = null;
				}
				return;
			}
			if (displayStates != PerformanceHud.DisplayStates.Hidden)
			{
				return;
			}
			if (!this.uiDoc)
			{
				this.uiDoc = base.gameObject.AddComponent<UIDocument>();
				this.uiDoc.panelSettings = Object.Instantiate<PanelSettings>(Resources.Load<PanelSettings>("DebugPanelSettings"));
			}
			VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("PerformanceHud");
			StyleSheet styleSheet = Resources.Load<StyleSheet>("PerformanceHud");
			this.uiDoc.visualTreeAsset = visualTreeAsset;
			this.uiDoc.rootVisualElement.styleSheets.Add(styleSheet);
			VisualElement rootVisualElement = this.uiDoc.rootVisualElement;
			TextElement textElement = rootVisualElement.Q("revisionLabel", null);
			BuildMetadata embedded = BuildMetadata.Embedded;
			textElement.text = ((embedded != null) ? ("r" + embedded.Revision + " - " + embedded.MachineName) : "No Build Metadata");
			this.sceneLabel = rootVisualElement.Q("sceneLabel", null);
			this.cpuMemLabel = rootVisualElement.Q("cpuMemLabel", null);
			this.resolutionLabel = rootVisualElement.Q("resolutionLabel", null);
			this.fpsLabel = rootVisualElement.Q("fpsLabel", null);
			this.profileLabel = rootVisualElement.Q("profileLabel", null);
			this.monoGroup = rootVisualElement.Q("monoGroup", null);
			this.IsMonoGroupEnabled = this.IsMonoGroupEnabled;
			this.gcLabel = rootVisualElement.Q("gcLabel", null);
			this.UpdateGCMode(GarbageCollector.GCMode);
			this.heapLabel = rootVisualElement.Q("monoHeapLabel", null);
			this.UpdateAll();
			this.UpdateScene();
		}
	}

	// Token: 0x170002FC RID: 764
	// (get) Token: 0x06001DEC RID: 7660 RVA: 0x00089AB1 File Offset: 0x00087CB1
	// (set) Token: 0x06001DED RID: 7661 RVA: 0x00089ABC File Offset: 0x00087CBC
	public bool EnableProfileRecording
	{
		get
		{
			return this.isProfileRecordingEnabled;
		}
		set
		{
			if (value == this.isProfileRecordingEnabled)
			{
				return;
			}
			this.isProfileRecordingEnabled = value;
			if (value)
			{
				this.setPassCallRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 1, ProfilerRecorderOptions.Default);
				this.trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", 1, ProfilerRecorderOptions.Default);
			}
			else
			{
				this.setPassCallRecorder.Dispose();
				this.trianglesRecorder.Dispose();
			}
			this.UpdateResolution();
		}
	}

	// Token: 0x06001DEE RID: 7662 RVA: 0x00089B2C File Offset: 0x00087D2C
	protected void Awake()
	{
		this.frameCounter = 0;
		this.lastSecond = (int)Time.realtimeSinceStartup;
		this.framesColor = Color.gray;
		this.memoryContent = new GUIContent("N/A");
		this.loadReports = new List<PerformanceHud.LoadReport>();
		this.vibrationHudDrawer = new PerformanceHud.VibrationHudDrawer(this);
	}

	// Token: 0x06001DEF RID: 7663 RVA: 0x00089B80 File Offset: 0x00087D80
	protected void OnEnable()
	{
		GameManager.SceneTransitionBegan += this.GameManager_SceneTransitionBegan;
		SceneManager.activeSceneChanged += this.OnActiveSceneChanged;
		SceneManager.sceneLoaded += this.OnSceneLoaded;
		SceneManager.sceneUnloaded += this.OnSceneUnloaded;
		GarbageCollector.GCModeChanged += this.UpdateGCMode;
	}

	// Token: 0x06001DF0 RID: 7664 RVA: 0x00089BE4 File Offset: 0x00087DE4
	protected void OnDisable()
	{
		GameManager.SceneTransitionBegan -= this.GameManager_SceneTransitionBegan;
		SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
		SceneManager.sceneUnloaded -= this.OnSceneUnloaded;
		GarbageCollector.GCModeChanged -= this.UpdateGCMode;
		this.EnableProfileRecording = false;
	}

	// Token: 0x06001DF1 RID: 7665 RVA: 0x00089C4D File Offset: 0x00087E4D
	private void OnDestroy()
	{
		this.ToggleDraw(false);
	}

	// Token: 0x06001DF2 RID: 7666 RVA: 0x00089C58 File Offset: 0x00087E58
	protected void Update()
	{
		if (this.DisplayState == PerformanceHud.DisplayStates.Hidden)
		{
			return;
		}
		this.frameCounter++;
		int num = (int)Time.realtimeSinceStartup;
		if (num != this.lastSecond)
		{
			this.framesLastSecond = this.frameCounter;
			int num2 = this.framesLastSecond;
			Color color;
			if (num2 < 58)
			{
				if (num2 < 50)
				{
					color = Color.red;
				}
				else
				{
					color = Color.yellow;
				}
			}
			else
			{
				color = Color.green;
			}
			this.framesColor = color;
			this.lastSecond = num;
			this.frameCounter = 0;
			this.UpdateAll();
		}
		if (PerformanceHud.ShowVibration)
		{
			this.vibrationHudDrawer.Update();
		}
	}

	// Token: 0x06001DF3 RID: 7667 RVA: 0x00089CEE File Offset: 0x00087EEE
	private void ToggleDraw(bool draw)
	{
		if (this.isDrawing != draw)
		{
			this.isDrawing = draw;
			if (draw)
			{
				GUIDrawer.AddDrawer(this);
			}
			else
			{
				GUIDrawer.RemoveDrawer(this);
			}
			if (this.uiDoc)
			{
				this.uiDoc.enabled = draw;
			}
		}
	}

	// Token: 0x06001DF4 RID: 7668 RVA: 0x00089D2A File Offset: 0x00087F2A
	private void UpdateDrawState()
	{
		if (this.displayState == PerformanceHud.DisplayStates.Hidden)
		{
			this.ToggleDraw(PerformanceHud.ShowVibration);
			return;
		}
		this.ToggleDraw(true);
	}

	// Token: 0x06001DF5 RID: 7669 RVA: 0x00089D48 File Offset: 0x00087F48
	private void GameManager_SceneTransitionBegan(SceneLoad sceneLoad)
	{
		PerformanceHud.LoadReport loadReport = new PerformanceHud.LoadReport
		{
			Color = Color.white,
			Content = new GUIContent()
		};
		this.loadReports.Add(loadReport);
		while (this.loadReports.Count > 2)
		{
			this.loadReports.RemoveAt(0);
		}
		sceneLoad.FetchComplete += delegate()
		{
			PerformanceHud.UpdateSceneLoadRecordContent(sceneLoad, loadReport);
		};
		sceneLoad.ActivationComplete += delegate()
		{
			PerformanceHud.UpdateSceneLoadRecordContent(sceneLoad, loadReport);
		};
		sceneLoad.Complete += delegate()
		{
			PerformanceHud.UpdateSceneLoadRecordContent(sceneLoad, loadReport);
		};
		sceneLoad.StartCalled += delegate()
		{
			PerformanceHud.UpdateSceneLoadRecordContent(sceneLoad, loadReport);
		};
		sceneLoad.BossLoaded += delegate()
		{
			PerformanceHud.UpdateSceneLoadRecordContent(sceneLoad, loadReport);
		};
		sceneLoad.Finish += delegate()
		{
			PerformanceHud.UpdateSceneLoadRecordContent(sceneLoad, loadReport);
		};
		PerformanceHud.UpdateSceneLoadRecordContent(sceneLoad, loadReport);
	}

	// Token: 0x06001DF6 RID: 7670 RVA: 0x00089E4C File Offset: 0x0008804C
	private static void UpdateSceneLoadRecordContent(SceneLoad sceneLoad, PerformanceHud.LoadReport report)
	{
		StringBuilder tempStringBuilder = global::Helper.GetTempStringBuilder(sceneLoad.TargetSceneName);
		tempStringBuilder.Append(":    ");
		float num = 0f;
		for (int i = 0; i < 9; i++)
		{
			SceneLoad.Phases phase = (SceneLoad.Phases)i;
			float? duration = sceneLoad.GetDuration(phase);
			if (duration != null && duration.Value > Mathf.Epsilon)
			{
				tempStringBuilder.Append(phase.ToString());
				tempStringBuilder.Append(": ");
				tempStringBuilder.Append(duration.Value.ToString("0.00s"));
				tempStringBuilder.Append("    ");
				num += duration.Value;
			}
		}
		if (num > Mathf.Epsilon)
		{
			tempStringBuilder.Append("Total: ");
			tempStringBuilder.Append(num.ToString("0.00s"));
		}
		Color color;
		if (num <= 3.5f)
		{
			if (num <= 3f)
			{
				color = Color.white;
			}
			else
			{
				color = Color.yellow;
			}
		}
		else
		{
			color = Color.red;
		}
		report.Color = color;
		report.Content.text = tempStringBuilder.ToString();
	}

	// Token: 0x06001DF7 RID: 7671 RVA: 0x00089F63 File Offset: 0x00088163
	private void UpdateAll()
	{
		this.UpdateMemory();
		this.UpdateResolution();
	}

	// Token: 0x06001DF8 RID: 7672 RVA: 0x00089F74 File Offset: 0x00088174
	private void UpdateMemory()
	{
		double num = (double)GCManager.GetMemoryUsage() / 1024.0 / 1024.0;
		double num2 = (double)GCManager.GetMemoryTotal() / 1024.0 / 1024.0;
		double num3 = (double)((long)SystemInfo.systemMemorySize);
		if (this.cpuMemLabel != null)
		{
			this.cpuMemLabel.text = string.Format("CPU Mem.: {0:n} / {1:n} / {2:n}", num, num2, num3);
		}
		Label label = this.heapLabel;
		if (label != null && label.visible)
		{
			double num4 = (double)GCManager.GetMonoHeapUsage() / 1024.0 / 1024.0;
			double num5 = (double)GCManager.GetMonoHeapTotal() / 1024.0 / 1024.0;
			this.heapLabel.text = string.Format("Heap: {0:n} / {1:n} / {2:n}", num4, num5, GCManager.HeapUsageThreshold);
		}
	}

	// Token: 0x06001DF9 RID: 7673 RVA: 0x0008A068 File Offset: 0x00088268
	private void UpdateGCMode(GarbageCollector.Mode mode)
	{
		if (this.gcLabel != null)
		{
			GarbageCollector.Mode mode2 = mode;
			GarbageCollector.Mode? mode3 = this.lastGcMode;
			if (!(mode2 == mode3.GetValueOrDefault() & mode3 != null))
			{
				this.gcLabel.text = "GC: " + mode.ToString();
				this.lastGcMode = new GarbageCollector.Mode?(mode);
				return;
			}
		}
	}

	// Token: 0x06001DFA RID: 7674 RVA: 0x0008A0C7 File Offset: 0x000882C7
	private void OnActiveSceneChanged(Scene fromScene, Scene toScene)
	{
		this.UpdateScene();
	}

	// Token: 0x06001DFB RID: 7675 RVA: 0x0008A0CF File Offset: 0x000882CF
	private void OnSceneUnloaded(Scene arg0)
	{
		this.UpdateScene();
	}

	// Token: 0x06001DFC RID: 7676 RVA: 0x0008A0D7 File Offset: 0x000882D7
	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		this.UpdateScene();
	}

	// Token: 0x06001DFD RID: 7677 RVA: 0x0008A0E0 File Offset: 0x000882E0
	private void UpdateScene()
	{
		if (this.sceneInfoBuilder == null)
		{
			this.sceneInfoBuilder = new StringBuilder();
		}
		else
		{
			this.sceneInfoBuilder.Clear();
		}
		Scene activeScene = SceneManager.GetActiveScene();
		bool flag = false;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				if (flag)
				{
					this.sceneInfoBuilder.Append(" + ");
				}
				bool flag2 = false;
				if (sceneAt != activeScene)
				{
					this.sceneInfoBuilder.Append("<color=#808080>");
					flag2 = true;
				}
				this.sceneInfoBuilder.Append(sceneAt.name);
				flag = true;
				if (flag2)
				{
					this.sceneInfoBuilder.Append("</color>");
				}
			}
		}
		if (this.sceneLabel != null)
		{
			this.sceneLabel.text = this.sceneInfoBuilder.ToString();
		}
	}

	// Token: 0x06001DFE RID: 7678 RVA: 0x0008A1B4 File Offset: 0x000883B4
	private void UpdateResolution()
	{
		if (this.profileLabel != null)
		{
			if (this.isProfileRecordingEnabled)
			{
				this.profileLabel.text = string.Format("- SetPass Calls: {0}, Triangles: {1}", this.setPassCallRecorder.LastValue, this.trianglesRecorder.LastValue);
				this.profileLabel.style.display = DisplayStyle.Flex;
			}
			else
			{
				this.profileLabel.style.display = DisplayStyle.None;
			}
		}
		int width = Screen.width;
		int height = Screen.height;
		ScreenRes resolution = CameraRenderScaled.Resolution;
		if (this.resolutionLabel != null && (width != this.lastScreenWidth || height != this.lastScreenHeight || resolution.Width != this.lastScreenWidthScaled || resolution.Height != this.lastScreenHeightScaled))
		{
			this.resolutionLabel.text = string.Format("{0}x{1} [{2}x{3}]", new object[]
			{
				width,
				height,
				resolution.Width,
				resolution.Height
			});
			this.lastScreenWidth = width;
			this.lastScreenHeight = height;
			this.lastScreenWidthScaled = resolution.Width;
			this.lastScreenHeightScaled = resolution.Height;
			this.uiDoc.panelSettings.scale = CheatManager.Multiplier;
		}
		if (this.fpsLabel != null && this.framesLastSecond != this.previousFpsValue)
		{
			if (this.fpsStringValues == null)
			{
				this.fpsStringValues = new Dictionary<int, string>(60);
			}
			string text;
			if (!this.fpsStringValues.TryGetValue(this.framesLastSecond, out text))
			{
				text = (this.fpsStringValues[this.framesLastSecond] = this.framesLastSecond.ToString());
			}
			this.fpsLabel.text = text;
			this.fpsLabel.style.color = this.framesColor;
			this.previousFpsValue = this.framesLastSecond;
		}
	}

	// Token: 0x170002FD RID: 765
	// (get) Token: 0x06001DFF RID: 7679 RVA: 0x0008A39A File Offset: 0x0008859A
	public int GUIDepth
	{
		get
		{
			return 1;
		}
	}

	// Token: 0x06001E00 RID: 7680 RVA: 0x0008A3A0 File Offset: 0x000885A0
	public void DrawGUI()
	{
		if (PerformanceHud.ShowVibration)
		{
			this.vibrationHudDrawer.OnGUI();
		}
		this.rightLineIndex = 1;
		if (this.DisplayState != PerformanceHud.DisplayStates.Full)
		{
			return;
		}
		int num = this.IsMonoGroupEnabled ? 4 : 3;
		this.OnGUIFull(ref num);
		GUI.color = Color.white;
		PerformanceHud.LabelWithShadow(new GUIContent("Boost Mode: " + (CheatManager.BoostModeActive ? "Enabled" : "Disabled")), ref num);
		MazeController newestInstance = MazeController.NewestInstance;
		if (newestInstance)
		{
			PerformanceHud.LabelWithShadowRight(new GUIContent(string.Format("Incorrect Doors Left: {0}", newestInstance.IncorrectDoorsLeft)), ref this.rightLineIndex);
			PerformanceHud.LabelWithShadowRight(new GUIContent(string.Format("Correct Doors Left: {0}", newestInstance.CorrectDoorsLeft)), ref this.rightLineIndex);
			foreach (TransitionPoint transitionPoint in newestInstance.EnumerateCorrectDoors())
			{
				PerformanceHud.LabelWithShadowRight(new GUIContent("Correct Door: " + (transitionPoint ? transitionPoint.gameObject.name : "none")), ref this.rightLineIndex);
			}
		}
	}

	// Token: 0x06001E01 RID: 7681 RVA: 0x0008A4E0 File Offset: 0x000886E0
	private void OnGUIFull(ref int lineIndex)
	{
		GUI.color = Color.white;
		PerformanceHud.LabelWithShadow(this.memoryContent, ref lineIndex);
		foreach (PerformanceHud.LoadReport loadReport in this.loadReports)
		{
			GUI.color = loadReport.Color;
			PerformanceHud.LabelWithShadow(loadReport.Content, ref lineIndex);
		}
		if (GameManager.instance && GameManager.instance.sm)
		{
			CustomSceneManager sm = GameManager.instance.sm;
			string text = string.Format("Saturation: {0}, Adjusted: {1}", sm.saturation, sm.AdjustSaturation(sm.saturation));
			GUI.color = Color.white;
			PerformanceHud.LabelWithShadow(new GUIContent(text), ref lineIndex);
		}
		GameManager unsafeInstance = GameManager.UnsafeInstance;
		if (unsafeInstance)
		{
			PerformanceHud.LabelWithShadow(new GUIContent("MapZone: " + unsafeInstance.GetCurrentMapZone()), ref lineIndex);
		}
		GUI.color = Color.white;
		PerformanceHud.LabelWithShadow(new GUIContent("Interaction: " + (InteractManager.CanInteract ? "Enabled" : "Disabled") + ", Blocked by: " + (InteractManager.BlockingInteractable ? InteractManager.BlockingInteractable.gameObject.name : "None")), ref lineIndex);
	}

	// Token: 0x170002FE RID: 766
	// (get) Token: 0x06001E02 RID: 7682 RVA: 0x0008A63C File Offset: 0x0008883C
	public static float ScreenEdgePadding
	{
		get
		{
			return 5f * CheatManager.Multiplier;
		}
	}

	// Token: 0x06001E03 RID: 7683 RVA: 0x0008A649 File Offset: 0x00088849
	private static void LabelWithShadow(GUIContent content, ref int lineIndex)
	{
		lineIndex++;
		PerformanceHud.LabelWithShadow(new Rect(PerformanceHud.ScreenEdgePadding, (float)(Screen.height - PerformanceHud.LineHeight * lineIndex) - PerformanceHud.ScreenEdgePadding, (float)Screen.width - PerformanceHud.ScreenEdgePadding, (float)PerformanceHud.LineHeight), content);
	}

	// Token: 0x06001E04 RID: 7684 RVA: 0x0008A688 File Offset: 0x00088888
	private static void LabelWithShadowRight(GUIContent content, ref int lineIndex)
	{
		lineIndex++;
		Vector2 vector = CheatManager.LabelStyle.CalcSize(content);
		PerformanceHud.LabelWithShadow(new Rect((float)Screen.width - vector.x - PerformanceHud.ScreenEdgePadding, (float)(Screen.height - PerformanceHud.LineHeight * lineIndex) - PerformanceHud.ScreenEdgePadding, vector.x + 10f, (float)PerformanceHud.LineHeight), content);
	}

	// Token: 0x06001E05 RID: 7685 RVA: 0x0008A6EC File Offset: 0x000888EC
	private static void LabelWithShadow(Rect rect, GUIContent content)
	{
		GUIStyle labelStyle = CheatManager.LabelStyle;
		Vector2 vector = labelStyle.CalcSize(content);
		Color color = GUI.color;
		try
		{
			GUI.color = new Color(0f, 0f, 0f, 0.5f);
			GUI.DrawTexture(new Rect(rect.x, rect.y, vector.x, rect.height), Texture2D.whiteTexture);
			GUI.color = Color.black;
			GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), content, labelStyle);
			GUI.color = color;
			GUI.Label(new Rect(rect.x + 0f, rect.y + 0f, rect.width, rect.height), content, labelStyle);
		}
		finally
		{
			GUI.color = color;
		}
	}

	// Token: 0x04001D09 RID: 7433
	private int frameCounter;

	// Token: 0x04001D0A RID: 7434
	private int lastSecond;

	// Token: 0x04001D0B RID: 7435
	private int framesLastSecond;

	// Token: 0x04001D0C RID: 7436
	private Color framesColor;

	// Token: 0x04001D0D RID: 7437
	private UIDocument uiDoc;

	// Token: 0x04001D0E RID: 7438
	private Label sceneLabel;

	// Token: 0x04001D0F RID: 7439
	private Label cpuMemLabel;

	// Token: 0x04001D10 RID: 7440
	private Label resolutionLabel;

	// Token: 0x04001D11 RID: 7441
	private Label fpsLabel;

	// Token: 0x04001D12 RID: 7442
	private Label profileLabel;

	// Token: 0x04001D13 RID: 7443
	private VisualElement monoGroup;

	// Token: 0x04001D14 RID: 7444
	private Label gcLabel;

	// Token: 0x04001D15 RID: 7445
	private Label heapLabel;

	// Token: 0x04001D16 RID: 7446
	private int previousFpsValue;

	// Token: 0x04001D17 RID: 7447
	private Dictionary<int, string> fpsStringValues;

	// Token: 0x04001D18 RID: 7448
	private int lastScreenWidth;

	// Token: 0x04001D19 RID: 7449
	private int lastScreenHeight;

	// Token: 0x04001D1A RID: 7450
	private int lastScreenWidthScaled;

	// Token: 0x04001D1B RID: 7451
	private int lastScreenHeightScaled;

	// Token: 0x04001D1C RID: 7452
	private StringBuilder sceneInfoBuilder;

	// Token: 0x04001D1D RID: 7453
	private GUIContent memoryContent;

	// Token: 0x04001D1E RID: 7454
	private GarbageCollector.Mode? lastGcMode;

	// Token: 0x04001D1F RID: 7455
	private List<PerformanceHud.LoadReport> loadReports;

	// Token: 0x04001D20 RID: 7456
	private static bool _showVibrations;

	// Token: 0x04001D21 RID: 7457
	private bool isProfileRecordingEnabled;

	// Token: 0x04001D22 RID: 7458
	private ProfilerRecorder setPassCallRecorder;

	// Token: 0x04001D23 RID: 7459
	private ProfilerRecorder trianglesRecorder;

	// Token: 0x04001D24 RID: 7460
	private GUIStyle rightAlignedStyle;

	// Token: 0x04001D25 RID: 7461
	private bool isDrawing;

	// Token: 0x04001D26 RID: 7462
	private const int LINE_HEIGHT = 24;

	// Token: 0x04001D27 RID: 7463
	private PerformanceHud.VibrationHudDrawer vibrationHudDrawer;

	// Token: 0x04001D28 RID: 7464
	private int rightLineIndex;

	// Token: 0x04001D29 RID: 7465
	private PerformanceHud.DisplayStates displayState;

	// Token: 0x04001D2A RID: 7466
	private bool isMonoGroupEnabled = true;

	// Token: 0x04001D2B RID: 7467
	private const float SCREEN_EDGE_PADDING = 5f;

	// Token: 0x0200163E RID: 5694
	private class LoadReport
	{
		// Token: 0x04008A9C RID: 35484
		public Color Color;

		// Token: 0x04008A9D RID: 35485
		public GUIContent Content;
	}

	// Token: 0x0200163F RID: 5695
	public enum DisplayStates
	{
		// Token: 0x04008A9F RID: 35487
		Hidden,
		// Token: 0x04008AA0 RID: 35488
		Minimal,
		// Token: 0x04008AA1 RID: 35489
		Full
	}

	// Token: 0x02001640 RID: 5696
	private class VibrationHudDrawer
	{
		// Token: 0x17000E1F RID: 3615
		// (get) Token: 0x060089E2 RID: 35298 RVA: 0x00280AC5 File Offset: 0x0027ECC5
		public GUIContent VibrationsContent
		{
			get
			{
				return this.vibrationsContent;
			}
		}

		// Token: 0x060089E3 RID: 35299 RVA: 0x00280ACD File Offset: 0x0027ECCD
		public VibrationHudDrawer(PerformanceHud performanceHud)
		{
			this.performanceHud = performanceHud;
		}

		// Token: 0x060089E4 RID: 35300 RVA: 0x00280B04 File Offset: 0x0027ED04
		public void Update()
		{
			VibrationMixer mixer = VibrationManager.GetMixer();
			if (mixer != null)
			{
				for (int i = 0; i < mixer.PlayingEmissionCount; i++)
				{
					VibrationEmission playingEmission = mixer.GetPlayingEmission(i);
					if (this.activeEmissions.Add(playingEmission))
					{
						this.trackers.Add(new PerformanceHud.VibrationHudDrawer.VibrationTracker(playingEmission));
					}
				}
			}
			for (int j = this.trackers.Count - 1; j >= 0; j--)
			{
				PerformanceHud.VibrationHudDrawer.VibrationTracker vibrationTracker = this.trackers[j];
				if (!vibrationTracker.Update())
				{
					this.activeEmissions.Remove(vibrationTracker.Emission);
					this.trackers.RemoveAt(j);
				}
			}
		}

		// Token: 0x060089E5 RID: 35301 RVA: 0x00280BA0 File Offset: 0x0027EDA0
		public void OnGUI()
		{
			GUI.color = Color.white;
			for (int i = this.trackers.Count - 1; i >= 0; i--)
			{
				PerformanceHud.VibrationHudDrawer.VibrationTracker vibrationTracker = this.trackers[i];
				this.vibrationsContent.text = vibrationTracker.ToString();
				PerformanceHud.LabelWithShadowRight(this.vibrationsContent, ref this.performanceHud.rightLineIndex);
			}
			if (this.trackers.Count > 0)
			{
				this.performanceHud.rightLineIndex++;
			}
		}

		// Token: 0x04008AA2 RID: 35490
		private PerformanceHud performanceHud;

		// Token: 0x04008AA3 RID: 35491
		private const float DISPLAY_TIME = 5f;

		// Token: 0x04008AA4 RID: 35492
		private HashSet<VibrationEmission> activeEmissions = new HashSet<VibrationEmission>();

		// Token: 0x04008AA5 RID: 35493
		private List<PerformanceHud.VibrationHudDrawer.VibrationTracker> trackers = new List<PerformanceHud.VibrationHudDrawer.VibrationTracker>();

		// Token: 0x04008AA6 RID: 35494
		private GUIContent vibrationsContent = new GUIContent("");

		// Token: 0x02001C50 RID: 7248
		private class VibrationTracker
		{
			// Token: 0x170011D0 RID: 4560
			// (get) Token: 0x06009BF1 RID: 39921 RVA: 0x002B9F5F File Offset: 0x002B815F
			public VibrationEmission Emission
			{
				get
				{
					return this.emission;
				}
			}

			// Token: 0x06009BF2 RID: 39922 RVA: 0x002B9F67 File Offset: 0x002B8167
			public VibrationTracker(VibrationEmission emission)
			{
				this.emission = emission;
				this.timer = 5f;
			}

			// Token: 0x06009BF3 RID: 39923 RVA: 0x002B9F81 File Offset: 0x002B8181
			public bool Update()
			{
				if (this.Emission == null)
				{
					return false;
				}
				if (!this.Emission.IsPlaying)
				{
					this.timer -= Time.deltaTime;
				}
				return this.timer > 0f;
			}

			// Token: 0x06009BF4 RID: 39924 RVA: 0x002B9FB9 File Offset: 0x002B81B9
			public override string ToString()
			{
				if (this.emission == null)
				{
					return "Empty";
				}
				if (this.emission.IsPlaying)
				{
					return string.Format("{0}", this.emission);
				}
				return string.Format("{0} finished", this.emission);
			}

			// Token: 0x06009BF5 RID: 39925 RVA: 0x002B9FF8 File Offset: 0x002B81F8
			public override bool Equals(object obj)
			{
				if (obj == null || base.GetType() != obj.GetType())
				{
					return false;
				}
				PerformanceHud.VibrationHudDrawer.VibrationTracker vibrationTracker = (PerformanceHud.VibrationHudDrawer.VibrationTracker)obj;
				return object.Equals(this.Emission, vibrationTracker.Emission);
			}

			// Token: 0x06009BF6 RID: 39926 RVA: 0x002BA035 File Offset: 0x002B8235
			public override int GetHashCode()
			{
				VibrationEmission vibrationEmission = this.Emission;
				if (vibrationEmission == null)
				{
					return 0;
				}
				return vibrationEmission.GetHashCode();
			}

			// Token: 0x0400A142 RID: 41282
			private VibrationEmission emission;

			// Token: 0x0400A143 RID: 41283
			private float timer;
		}
	}
}
