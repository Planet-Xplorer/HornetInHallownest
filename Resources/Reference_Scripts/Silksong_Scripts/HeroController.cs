using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GlobalEnums;
using GlobalSettings;
using HutongGames.PlayMaker;
using TeamCherry.NestedFadeGroup;
using TeamCherry.SharedUtils;
using UnityEngine;

// Token: 0x0200018D RID: 397
public class HeroController : MonoBehaviour, ITagDamageTakerOwner
{
	// Token: 0x1700015B RID: 347
	// (get) Token: 0x06000D06 RID: 3334 RVA: 0x0003A0C2 File Offset: 0x000382C2
	private float CurrentNailChargeTime
	{
		get
		{
			if (!this.NailChargeTimeQuickTool.IsEquipped)
			{
				return this.NAIL_CHARGE_TIME;
			}
			return this.NAIL_CHARGE_TIME_QUICK;
		}
	}

	// Token: 0x1700015C RID: 348
	// (get) Token: 0x06000D07 RID: 3335 RVA: 0x0003A0DE File Offset: 0x000382DE
	private float CurrentNailChargeBeginTime
	{
		get
		{
			if (!this.NailChargeTimeQuickTool.IsEquipped)
			{
				return this.NAIL_CHARGE_BEGIN_TIME;
			}
			return this.NAIL_CHARGE_BEGIN_TIME_QUICK;
		}
	}

	// Token: 0x1700015D RID: 349
	// (get) Token: 0x06000D08 RID: 3336 RVA: 0x0003A0FA File Offset: 0x000382FA
	// (set) Token: 0x06000D09 RID: 3337 RVA: 0x0003A102 File Offset: 0x00038302
	public HeroLockStates HeroLockState { get; private set; }

	// Token: 0x1700015E RID: 350
	// (get) Token: 0x06000D0A RID: 3338 RVA: 0x0003A10B File Offset: 0x0003830B
	private bool CanTurn
	{
		get
		{
			return !this.cState.downSpikeBouncing && !this.queuedAutoThrowTool && !this.animCtrl.IsTurnBlocked();
		}
	}

	// Token: 0x1700015F RID: 351
	// (get) Token: 0x06000D0B RID: 3339 RVA: 0x0003A132 File Offset: 0x00038332
	// (set) Token: 0x06000D0C RID: 3340 RVA: 0x0003A13A File Offset: 0x0003833A
	public float fallTimer { get; private set; }

	// Token: 0x17000160 RID: 352
	// (get) Token: 0x06000D0D RID: 3341 RVA: 0x0003A143 File Offset: 0x00038343
	public HeroAudioController AudioCtrl
	{
		get
		{
			return this.audioCtrl;
		}
	}

	// Token: 0x17000161 RID: 353
	// (get) Token: 0x06000D0E RID: 3342 RVA: 0x0003A14B File Offset: 0x0003834B
	public Bounds Bounds
	{
		get
		{
			return this.col2d.bounds;
		}
	}

	// Token: 0x17000162 RID: 354
	// (get) Token: 0x06000D0F RID: 3343 RVA: 0x0003A158 File Offset: 0x00038358
	// (set) Token: 0x06000D10 RID: 3344 RVA: 0x0003A160 File Offset: 0x00038360
	public PlayMakerFSM proxyFSM { get; private set; }

	// Token: 0x17000163 RID: 355
	// (get) Token: 0x06000D11 RID: 3345 RVA: 0x0003A169 File Offset: 0x00038369
	// (set) Token: 0x06000D12 RID: 3346 RVA: 0x0003A171 File Offset: 0x00038371
	public HeroNailImbuement NailImbuement { get; private set; }

	// Token: 0x17000164 RID: 356
	// (get) Token: 0x06000D13 RID: 3347 RVA: 0x0003A17A File Offset: 0x0003837A
	public bool IsSprintMasterActive
	{
		get
		{
			return this.sprintMasterActiveBool.Value;
		}
	}

	// Token: 0x17000165 RID: 357
	// (get) Token: 0x06000D14 RID: 3348 RVA: 0x0003A187 File Offset: 0x00038387
	// (set) Token: 0x06000D15 RID: 3349 RVA: 0x0003A18F File Offset: 0x0003838F
	public bool IsGravityApplied { get; private set; }

	// Token: 0x17000166 RID: 358
	// (get) Token: 0x06000D16 RID: 3350 RVA: 0x0003A198 File Offset: 0x00038398
	// (set) Token: 0x06000D17 RID: 3351 RVA: 0x0003A1A0 File Offset: 0x000383A0
	public TransitionPoint sceneEntryGate { get; private set; }

	// Token: 0x17000167 RID: 359
	// (get) Token: 0x06000D18 RID: 3352 RVA: 0x0003A1A9 File Offset: 0x000383A9
	public Vector2[] PositionHistory
	{
		get
		{
			return this.positionHistory;
		}
	}

	// Token: 0x1400001F RID: 31
	// (add) Token: 0x06000D19 RID: 3353 RVA: 0x0003A1B4 File Offset: 0x000383B4
	// (remove) Token: 0x06000D1A RID: 3354 RVA: 0x0003A1EC File Offset: 0x000383EC
	public event Action<float> FrostAmountUpdated;

	// Token: 0x14000020 RID: 32
	// (add) Token: 0x06000D1B RID: 3355 RVA: 0x0003A224 File Offset: 0x00038424
	// (remove) Token: 0x06000D1C RID: 3356 RVA: 0x0003A25C File Offset: 0x0003845C
	public event Action OnDoubleJumped;

	// Token: 0x17000168 RID: 360
	// (get) Token: 0x06000D1D RID: 3357 RVA: 0x0003A291 File Offset: 0x00038491
	// (set) Token: 0x06000D1E RID: 3358 RVA: 0x0003A299 File Offset: 0x00038499
	public bool ForceWalkingSound
	{
		get
		{
			return this.forceWalkingSound;
		}
		set
		{
			this.forceWalkingSound = value;
		}
	}

	// Token: 0x17000169 RID: 361
	// (get) Token: 0x06000D1F RID: 3359 RVA: 0x0003A2A2 File Offset: 0x000384A2
	// (set) Token: 0x06000D20 RID: 3360 RVA: 0x0003A2AA File Offset: 0x000384AA
	public bool ForceRunningSound { get; set; }

	// Token: 0x1700016A RID: 362
	// (get) Token: 0x06000D21 RID: 3361 RVA: 0x0003A2B3 File Offset: 0x000384B3
	// (set) Token: 0x06000D22 RID: 3362 RVA: 0x0003A2BB File Offset: 0x000384BB
	public bool IsStunned { get; set; }

	// Token: 0x1700016B RID: 363
	// (get) Token: 0x06000D23 RID: 3363 RVA: 0x0003A2C4 File Offset: 0x000384C4
	// (set) Token: 0x06000D24 RID: 3364 RVA: 0x0003A2CC File Offset: 0x000384CC
	private NailSlash SlashComponent
	{
		get
		{
			return this._slashComponent;
		}
		set
		{
			if (this.cState.downAttacking && this._slashComponent != null && this._slashComponent != value)
			{
				this._slashComponent.CancelAttack();
			}
			this._slashComponent = value;
		}
	}

	// Token: 0x14000021 RID: 33
	// (add) Token: 0x06000D25 RID: 3365 RVA: 0x0003A30C File Offset: 0x0003850C
	// (remove) Token: 0x06000D26 RID: 3366 RVA: 0x0003A340 File Offset: 0x00038540
	public static event HeroController.HeroSetDelegate OnHeroInstanceSet;

	// Token: 0x14000022 RID: 34
	// (add) Token: 0x06000D27 RID: 3367 RVA: 0x0003A374 File Offset: 0x00038574
	// (remove) Token: 0x06000D28 RID: 3368 RVA: 0x0003A3AC File Offset: 0x000385AC
	public event HeroController.HeroInPosition preHeroInPosition;

	// Token: 0x14000023 RID: 35
	// (add) Token: 0x06000D29 RID: 3369 RVA: 0x0003A3E4 File Offset: 0x000385E4
	// (remove) Token: 0x06000D2A RID: 3370 RVA: 0x0003A41C File Offset: 0x0003861C
	public event HeroController.HeroInPosition heroInPosition;

	// Token: 0x14000024 RID: 36
	// (add) Token: 0x06000D2B RID: 3371 RVA: 0x0003A454 File Offset: 0x00038654
	// (remove) Token: 0x06000D2C RID: 3372 RVA: 0x0003A48C File Offset: 0x0003868C
	public event HeroController.HeroInPosition heroInPositionDelayed;

	// Token: 0x14000025 RID: 37
	// (add) Token: 0x06000D2D RID: 3373 RVA: 0x0003A4C4 File Offset: 0x000386C4
	// (remove) Token: 0x06000D2E RID: 3374 RVA: 0x0003A4FC File Offset: 0x000386FC
	public event Action OnTakenDamage;

	// Token: 0x14000026 RID: 38
	// (add) Token: 0x06000D2F RID: 3375 RVA: 0x0003A534 File Offset: 0x00038734
	// (remove) Token: 0x06000D30 RID: 3376 RVA: 0x0003A56C File Offset: 0x0003876C
	public event HeroController.DamageTakenDelegate OnTakenDamageExtra;

	// Token: 0x14000027 RID: 39
	// (add) Token: 0x06000D31 RID: 3377 RVA: 0x0003A5A4 File Offset: 0x000387A4
	// (remove) Token: 0x06000D32 RID: 3378 RVA: 0x0003A5DC File Offset: 0x000387DC
	public event Action OnDeath;

	// Token: 0x14000028 RID: 40
	// (add) Token: 0x06000D33 RID: 3379 RVA: 0x0003A614 File Offset: 0x00038814
	// (remove) Token: 0x06000D34 RID: 3380 RVA: 0x0003A64C File Offset: 0x0003884C
	public event Action OnHazardDeath;

	// Token: 0x14000029 RID: 41
	// (add) Token: 0x06000D35 RID: 3381 RVA: 0x0003A684 File Offset: 0x00038884
	// (remove) Token: 0x06000D36 RID: 3382 RVA: 0x0003A6BC File Offset: 0x000388BC
	public event Action OnHazardRespawn;

	// Token: 0x1400002A RID: 42
	// (add) Token: 0x06000D37 RID: 3383 RVA: 0x0003A6F4 File Offset: 0x000388F4
	// (remove) Token: 0x06000D38 RID: 3384 RVA: 0x0003A72C File Offset: 0x0003892C
	public event Action<Vector2> BeforeApplyConveyorSpeed;

	// Token: 0x1400002B RID: 43
	// (add) Token: 0x06000D39 RID: 3385 RVA: 0x0003A764 File Offset: 0x00038964
	// (remove) Token: 0x06000D3A RID: 3386 RVA: 0x0003A79C File Offset: 0x0003899C
	public event Action FlippedSprite;

	// Token: 0x1400002C RID: 44
	// (add) Token: 0x06000D3B RID: 3387 RVA: 0x0003A7D4 File Offset: 0x000389D4
	// (remove) Token: 0x06000D3C RID: 3388 RVA: 0x0003A80C File Offset: 0x00038A0C
	public event Action HeroLeavingScene;

	// Token: 0x1700016C RID: 364
	// (get) Token: 0x06000D3D RID: 3389 RVA: 0x0003A841 File Offset: 0x00038A41
	public HeroController.WarriorCrestStateInfo WarriorState
	{
		get
		{
			return this.warriorState;
		}
	}

	// Token: 0x1700016D RID: 365
	// (get) Token: 0x06000D3E RID: 3390 RVA: 0x0003A849 File Offset: 0x00038A49
	public HeroController.ReaperCrestStateInfo ReaperState
	{
		get
		{
			return this.reaperState;
		}
	}

	// Token: 0x1700016E RID: 366
	// (get) Token: 0x06000D3F RID: 3391 RVA: 0x0003A851 File Offset: 0x00038A51
	public HeroController.HunterUpgCrestStateInfo HunterUpgState
	{
		get
		{
			return this.hunterUpgState;
		}
	}

	// Token: 0x1700016F RID: 367
	// (get) Token: 0x06000D40 RID: 3392 RVA: 0x0003A859 File Offset: 0x00038A59
	// (set) Token: 0x06000D41 RID: 3393 RVA: 0x0003A861 File Offset: 0x00038A61
	public HeroController.WandererCrestStateInfo WandererState { get; set; }

	// Token: 0x17000170 RID: 368
	// (get) Token: 0x06000D42 RID: 3394 RVA: 0x0003A86A File Offset: 0x00038A6A
	public bool IsUsingQuickening
	{
		get
		{
			return this.quickeningTimeLeft > 0f;
		}
	}

	// Token: 0x17000171 RID: 369
	// (get) Token: 0x06000D43 RID: 3395 RVA: 0x0003A879 File Offset: 0x00038A79
	// (set) Token: 0x06000D44 RID: 3396 RVA: 0x0003A881 File Offset: 0x00038A81
	public bool IsInLifebloodState { get; private set; }

	// Token: 0x17000172 RID: 370
	// (get) Token: 0x06000D45 RID: 3397 RVA: 0x0003A88A File Offset: 0x00038A8A
	public bool IsWandererLucky
	{
		get
		{
			return !this.cState.isMaggoted && Gameplay.WandererCrest.IsEquipped && this.playerData.silk >= 9;
		}
	}

	// Token: 0x17000173 RID: 371
	// (get) Token: 0x06000D46 RID: 3398 RVA: 0x0003A8BB File Offset: 0x00038ABB
	// (set) Token: 0x06000D47 RID: 3399 RVA: 0x0003A8C3 File Offset: 0x00038AC3
	public int PoisonHealthCount { get; private set; }

	// Token: 0x17000174 RID: 372
	// (get) Token: 0x06000D48 RID: 3400 RVA: 0x0003A8CC File Offset: 0x00038ACC
	public Rigidbody2D Body
	{
		get
		{
			return this.rb2d;
		}
	}

	// Token: 0x17000175 RID: 373
	// (get) Token: 0x06000D49 RID: 3401 RVA: 0x0003A8D4 File Offset: 0x00038AD4
	public bool HasAnimationControl
	{
		get
		{
			return this.animCtrl.controlEnabled;
		}
	}

	// Token: 0x17000176 RID: 374
	// (get) Token: 0x06000D4A RID: 3402 RVA: 0x0003A8E1 File Offset: 0x00038AE1
	public HeroAnimationController AnimCtrl
	{
		get
		{
			return this.animCtrl;
		}
	}

	// Token: 0x17000177 RID: 375
	// (get) Token: 0x06000D4B RID: 3403 RVA: 0x0003A8E9 File Offset: 0x00038AE9
	public static HeroController instance
	{
		get
		{
			return HeroController.SilentInstance;
		}
	}

	// Token: 0x17000178 RID: 376
	// (get) Token: 0x06000D4C RID: 3404 RVA: 0x0003A8F0 File Offset: 0x00038AF0
	public static HeroController SilentInstance
	{
		get
		{
			if (HeroController.lastUpdate != CustomPlayerLoop.FixedUpdateCycle)
			{
				HeroController.lastUpdate = CustomPlayerLoop.FixedUpdateCycle;
				if (HeroController._instance == null)
				{
					HeroController._instance = Object.FindObjectOfType<HeroController>();
					if (HeroController._instance && Application.isPlaying)
					{
						HeroController.HeroSetDelegate onHeroInstanceSet = HeroController.OnHeroInstanceSet;
						if (onHeroInstanceSet != null)
						{
							onHeroInstanceSet(HeroController._instance);
						}
						HeroUtility.Reset();
						Object.DontDestroyOnLoad(HeroController._instance.gameObject);
					}
					else
					{
						HeroController.lastUpdate = -1;
					}
				}
			}
			return HeroController._instance;
		}
	}

	// Token: 0x17000179 RID: 377
	// (get) Token: 0x06000D4D RID: 3405 RVA: 0x0003A973 File Offset: 0x00038B73
	public static HeroController UnsafeInstance
	{
		get
		{
			return HeroController._instance;
		}
	}

	// Token: 0x1700017A RID: 378
	// (get) Token: 0x06000D4E RID: 3406 RVA: 0x0003A97A File Offset: 0x00038B7A
	public HeroControllerConfig Config
	{
		get
		{
			if (this.CurrentConfigGroup == null)
			{
				return null;
			}
			return this.CurrentConfigGroup.Config;
		}
	}

	// Token: 0x1700017B RID: 379
	// (get) Token: 0x06000D4F RID: 3407 RVA: 0x0003A991 File Offset: 0x00038B91
	// (set) Token: 0x06000D50 RID: 3408 RVA: 0x0003A999 File Offset: 0x00038B99
	public HeroController.ConfigGroup CurrentConfigGroup { get; private set; }

	// Token: 0x1700017C RID: 380
	// (get) Token: 0x06000D51 RID: 3409 RVA: 0x0003A9A2 File Offset: 0x00038BA2
	public SpriteFlash SpriteFlash
	{
		get
		{
			return this.spriteFlash;
		}
	}

	// Token: 0x1700017D RID: 381
	// (get) Token: 0x06000D52 RID: 3410 RVA: 0x0003A9AA File Offset: 0x00038BAA
	public Vector2 TagDamageEffectPos
	{
		get
		{
			return Vector2.zero;
		}
	}

	// Token: 0x1700017E RID: 382
	// (get) Token: 0x06000D53 RID: 3411 RVA: 0x0003A9B1 File Offset: 0x00038BB1
	public bool IsRefillSoundsSuppressed
	{
		get
		{
			return this.refillSoundSuppressFramesLeft > 0;
		}
	}

	// Token: 0x1700017F RID: 383
	// (get) Token: 0x06000D54 RID: 3412 RVA: 0x0003A9BC File Offset: 0x00038BBC
	// (set) Token: 0x06000D55 RID: 3413 RVA: 0x0003A9C4 File Offset: 0x00038BC4
	public bool ForceClampTerminalVelocity { get; set; }

	// Token: 0x17000180 RID: 384
	// (get) Token: 0x06000D56 RID: 3414 RVA: 0x0003A9D0 File Offset: 0x00038BD0
	private int CriticalHealthValue
	{
		get
		{
			float num = 1f;
			if (Gameplay.BarbedWireTool.Status.IsEquipped)
			{
				num *= Gameplay.BarbedWireDamageTakenMultiplier;
			}
			return Mathf.FloorToInt(num);
		}
	}

	// Token: 0x06000D57 RID: 3415 RVA: 0x0003AA02 File Offset: 0x00038C02
	private void OnValidate()
	{
		ArrayForEnumAttribute.EnsureArraySize<RandomAudioClipTable>(ref this.footStepTables, typeof(EnvironmentTypes));
	}

	// Token: 0x06000D58 RID: 3416 RVA: 0x0003AA1C File Offset: 0x00038C1C
	private void Awake()
	{
		this.OnValidate();
		if (HeroController._instance == null)
		{
			HeroController._instance = this;
			HeroController.HeroSetDelegate onHeroInstanceSet = HeroController.OnHeroInstanceSet;
			if (onHeroInstanceSet != null)
			{
				onHeroInstanceSet(HeroController._instance);
			}
			Object.DontDestroyOnLoad(this);
			HeroUtility.Reset();
		}
		else if (this != HeroController._instance)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		EventRegister spoolAppearedRegister = EventRegister.GetRegisterGuaranteed(base.gameObject, "SPOOL APPEARED");
		Action temp = null;
		temp = delegate()
		{
			this.hasSilkSpoolAppeared = true;
			spoolAppearedRegister.ReceivedEvent -= temp;
		};
		spoolAppearedRegister.ReceivedEvent += temp;
		this.SetupGameRefs();
		this.SetupPools();
		this.rageModeEffect = this.SpawnChildPrefab<NestedFadeGroupTimedFader>(this.rageModeEffectPrefab);
		this.reaperModeEffect = this.SpawnChildPrefab<NestedFadeGroupTimedFader>(this.reaperModeEffectPrefab);
		this.spawnedSilkAcid = this.SpawnChildPrefab(this.silkAcidPrefab);
		this.spawnedVoidAcid = this.SpawnChildPrefab(this.voidAcidPrefab);
		this.spawnedFrostEnter = this.SpawnChildPrefab(this.frostEnterPrefab);
		this.spawnedHeatEnter = this.SpawnChildPrefab(this.heatEnterPrefab);
		this.spawnedMaggotEnter = this.SpawnChildPrefab(this.maggotEnterPrefab);
		this.spawnedLuckyDiceShieldEffect = this.SpawnChildPrefab(this.luckyDiceShieldEffectPrefab);
		this.spawnedLavaBellRechargeEffect = this.SpawnChildPrefab(this.lavaBellRechargeEffectPrefab);
		HeroController.ConfigGroup[] array = this.configs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Setup();
		}
		array = this.specialConfigs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Setup();
		}
		this.UpdateConfig();
		EventRegister.GetRegisterGuaranteed(base.gameObject, "TOOL EQUIPS CHANGED").ReceivedEvent += this.ResetAllCrestState;
		this.ResetAllCrestState();
		this.tagDamageTaker = TagDamageTaker.Add(base.gameObject, this);
		this.bumpChecker = CharacterBumpCheck.Add(base.gameObject, 8448, this.rb2d, this.col2d, () => this.cState.facingRight);
		this.OnDeath += CurrencyObjectBase.ProcessHeroDeath;
		this.wallClingEffect.SetActive(false);
	}

	// Token: 0x06000D59 RID: 3417 RVA: 0x0003AC42 File Offset: 0x00038E42
	private void OnDestroy()
	{
		if (HeroController._instance == this)
		{
			HeroController._instance = null;
		}
		if (this.heroPhysicsPusher)
		{
			Object.Destroy(this.heroPhysicsPusher);
		}
		this.damageEnemiesList.Clear();
	}

	// Token: 0x06000D5A RID: 3418 RVA: 0x0003AC7A File Offset: 0x00038E7A
	private GameObject SpawnChildPrefab(GameObject prefab)
	{
		if (!prefab)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(prefab);
		Transform transform = gameObject.transform;
		transform.SetParent(this.transform);
		transform.localPosition = Vector3.zero;
		gameObject.gameObject.SetActive(false);
		return gameObject;
	}

	// Token: 0x06000D5B RID: 3419 RVA: 0x0003ACB4 File Offset: 0x00038EB4
	private T SpawnChildPrefab<T>(T prefab) where T : Component
	{
		if (!prefab)
		{
			return default(!!0);
		}
		T t = Object.Instantiate<T>(prefab);
		Transform transform = t.transform;
		transform.SetParent(this.transform);
		transform.localPosition = Vector3.zero;
		t.gameObject.SetActive(false);
		return t;
	}

	// Token: 0x06000D5C RID: 3420 RVA: 0x0003AD10 File Offset: 0x00038F10
	private void Start()
	{
		this.playerData = PlayerData.instance;
		this.ui = UIManager.instance;
		if (this.fsm_thornCounter == null)
		{
			this.fsm_thornCounter = FSMUtility.LocateFSM(this.transform.Find("Charm Effects").gameObject, "Thorn Counter");
		}
		if (this.dashBurst == null)
		{
			this.dashBurst = FSMUtility.GetFSM(this.transform.Find("Effects").Find("Dash Burst").gameObject);
		}
		if (this.spellControl == null)
		{
			this.spellControl = FSMUtility.LocateFSM(base.gameObject, "Spell Control");
		}
		if (this.gm.IsGameplayScene())
		{
			this.isGameplayScene = true;
			this.vignette.enabled = true;
			this.SendHeroInPosition(false);
			this.FinishedEnteringScene(true, false);
			if (GameManager.instance.profileID == 0)
			{
				this.AddSilk(this.playerData.CurrentSilkMax, false);
			}
		}
		else
		{
			this.isGameplayScene = false;
			this.transform.SetPositionY(-50000f);
			this.vignette.enabled = false;
			this.AffectedByGravity(false);
		}
		if (this.sprintFSM)
		{
			this.sprintFSMIsQuickening = this.sprintFSM.FsmVariables.GetFsmBool("Is Quickening Active");
			this.toolsFSMIsQuickening = this.toolsFSM.FsmVariables.GetFsmBool("Is Quickening Active");
			this.sprintMasterActiveBool = this.sprintFSM.FsmVariables.FindFsmBool("Sprintmaster Active");
			this.sprintSpeedAddFloat = this.sprintFSM.FsmVariables.FindFsmFloat("Add Speed");
		}
		this.CharmUpdate();
		if (this.acidDeathPrefab)
		{
			ObjectPool.CreatePool(this.acidDeathPrefab, 1, false);
		}
		if (this.spikeDeathPrefab)
		{
			ObjectPool.CreatePool(this.spikeDeathPrefab, 1, false);
		}
		if (this.zapDeathPrefab)
		{
			ObjectPool.CreatePool(this.zapDeathPrefab, 1, false);
		}
		DeliveryQuestItem.BreakTimedNoEffects();
		this.SetupDeliveryItems();
		if (this.umbrellaFSM != null)
		{
			this.isUmbrellaActive = this.umbrellaFSM.FsmVariables.FindFsmBool("Is Active");
		}
		this.damageEnemiesList.AddRange(base.GetComponentsInChildren<DamageEnemies>(true));
	}

	// Token: 0x06000D5D RID: 3421 RVA: 0x0003AF4E File Offset: 0x0003914E
	private void SendPreHeroInPosition(bool forceDirect)
	{
		this.isHeroInPrePosition = true;
		HeroController.HeroInPosition heroInPosition = this.preHeroInPosition;
		if (heroInPosition == null)
		{
			return;
		}
		heroInPosition(forceDirect);
	}

	// Token: 0x06000D5E RID: 3422 RVA: 0x0003AF68 File Offset: 0x00039168
	private void SendHeroInPosition(bool forceDirect)
	{
		if (!this.isHeroInPrePosition)
		{
			this.SendPreHeroInPosition(forceDirect);
		}
		this.isHeroInPosition = true;
		this.animCtrl.waitingToEnter = false;
		this.gm.cameraCtrl.ResetStartTimer();
		if (this.heroInPosition != null)
		{
			this.heroInPosition(false);
		}
		if (this.heroInPositionDelayed != null)
		{
			this.heroInPositionDelayed(false);
		}
		this.vignetteFSM.SendEvent("RESET");
	}

	// Token: 0x06000D5F RID: 3423 RVA: 0x0003AFE0 File Offset: 0x000391E0
	private void UpdateConfig()
	{
		if (this.crestConfig)
		{
			HeroControllerConfig y = this.crestConfig;
			HeroController.ConfigGroup configGroup = null;
			foreach (HeroController.ConfigGroup configGroup2 in this.configs)
			{
				if (!(configGroup2.Config != y))
				{
					configGroup = configGroup2;
					break;
				}
			}
			HeroController.ConfigGroup overrideGroup = null;
			foreach (HeroController.ConfigGroup configGroup3 in this.specialConfigs)
			{
				if (!(configGroup3.Config != y) && (!(configGroup3.Config == Gameplay.WarriorCrest.HeroConfig) || this.warriorState.IsInRageMode))
				{
					overrideGroup = configGroup3;
					break;
				}
			}
			this.SetConfigGroup(configGroup, overrideGroup);
			return;
		}
		this.SetConfigGroup(this.configs[0], null);
	}

	// Token: 0x06000D60 RID: 3424 RVA: 0x0003B0AC File Offset: 0x000392AC
	private void SetConfigGroup(HeroController.ConfigGroup configGroup, HeroController.ConfigGroup overrideGroup)
	{
		if (configGroup == null)
		{
			Debug.LogError("configGroup was null!");
			return;
		}
		HeroController.ConfigGroup currentConfigGroup = this.CurrentConfigGroup;
		HeroControllerConfig heroControllerConfig = (currentConfigGroup != null) ? currentConfigGroup.Config : null;
		this.CurrentConfigGroup = configGroup;
		HeroControllerConfig config = configGroup.Config;
		if (config != heroControllerConfig)
		{
			if (heroControllerConfig != null && !config.ForceBareInventory && heroControllerConfig.ForceBareInventory)
			{
				CollectableItemManager.ApplyHiddenItems();
				this.gm.gameMap.SetupMap(false);
			}
			ToolItemManager.ReportAllBoundAttackToolsUpdated();
			this.animCtrl.SetHeroControllerConfig(config);
			if (currentConfigGroup != null && currentConfigGroup.ActiveRoot)
			{
				currentConfigGroup.ActiveRoot.SetActive(false);
			}
			if (configGroup.ActiveRoot)
			{
				configGroup.ActiveRoot.SetActive(true);
			}
		}
		this.normalSlash = ((overrideGroup != null && overrideGroup.NormalSlash) ? overrideGroup.NormalSlash : configGroup.NormalSlash);
		this.normalSlashDamager = ((overrideGroup != null && overrideGroup.NormalSlashDamager) ? overrideGroup.NormalSlashDamager : configGroup.NormalSlashDamager);
		this.alternateSlash = ((overrideGroup != null && overrideGroup.AlternateSlash) ? overrideGroup.AlternateSlash : configGroup.AlternateSlash);
		this.alternateSlashDamager = ((overrideGroup != null && overrideGroup.AlternateSlashDamager) ? overrideGroup.AlternateSlashDamager : configGroup.AlternateSlashDamager);
		this.upSlash = ((overrideGroup != null && overrideGroup.UpSlash) ? overrideGroup.UpSlash : configGroup.UpSlash);
		this.upSlashDamager = ((overrideGroup != null && overrideGroup.UpSlashDamager) ? overrideGroup.UpSlashDamager : configGroup.UpSlashDamager);
		this.altUpSlash = ((overrideGroup != null && overrideGroup.AltUpSlash) ? overrideGroup.AltUpSlash : configGroup.AltUpSlash);
		this.altUpSlashDamager = ((overrideGroup != null && overrideGroup.AltUpSlashDamager) ? overrideGroup.AltUpSlashDamager : configGroup.AltUpSlashDamager);
		this.downSpike = ((overrideGroup != null && overrideGroup.Downspike) ? overrideGroup.Downspike : configGroup.Downspike);
		this.downSlash = ((overrideGroup != null && overrideGroup.DownSlash) ? overrideGroup.DownSlash : configGroup.DownSlash);
		this.downSlashDamager = ((overrideGroup != null && overrideGroup.DownSlashDamager) ? overrideGroup.DownSlashDamager : configGroup.DownSlashDamager);
		this.altDownSpike = ((overrideGroup != null && overrideGroup.AltDownspike) ? overrideGroup.AltDownspike : configGroup.AltDownspike);
		this.altDownSlash = ((overrideGroup != null && overrideGroup.AltDownSlash) ? overrideGroup.AltDownSlash : configGroup.AltDownSlash);
		this.altDownSlashDamager = ((overrideGroup != null && overrideGroup.AltDownSlashDamager) ? overrideGroup.AltDownSlashDamager : configGroup.AltDownSlashDamager);
		this.wallSlash = ((overrideGroup != null && overrideGroup.WallSlash) ? overrideGroup.WallSlash : configGroup.WallSlash);
		this.wallSlashDamager = ((overrideGroup != null && overrideGroup.WallSlashDamager) ? overrideGroup.WallSlashDamager : configGroup.WallSlashDamager);
		if (config != heroControllerConfig)
		{
			FSMUtility.SendEventToGameObject(base.gameObject, "HC CONFIG UPDATED", false);
		}
	}

	// Token: 0x06000D61 RID: 3425 RVA: 0x0003B3C4 File Offset: 0x000395C4
	public void SceneInit()
	{
		if (this != HeroController._instance)
		{
			return;
		}
		if (!this.gm)
		{
			this.gm = GameManager.instance;
		}
		if (this.gm.IsGameplayScene())
		{
			this.isGameplayScene = true;
			HeroBox.Inactive = false;
			this.damageMode = DamageMode.FULL_DAMAGE;
		}
		else
		{
			this.isGameplayScene = false;
			this.acceptingInput = false;
			this.SetState(ActorStates.no_input);
			this.transform.SetPositionY(-50000f);
			this.vignette.enabled = false;
			this.AffectedByGravity(false);
			this.rb2d.linearVelocity = Vector2.zero;
		}
		this.transform.SetPositionZ(0.004f);
		this.SetWalkZone(false);
		this.ResetUpdraft();
		this.ResetTauntEffects();
		this.cState.invulnerable = false;
		this.cState.evading = false;
		this.cState.whipLashing = false;
		this.cState.isTriggerEventsPaused = false;
	}

	// Token: 0x06000D62 RID: 3426 RVA: 0x0003B4B8 File Offset: 0x000396B8
	private void Update()
	{
		if (Time.frameCount % 10 == 0)
		{
			this.Update10();
		}
		this.current_velocity = this.rb2d.linearVelocity;
		if (this.transitionState == HeroTransitionState.WAITING_TO_TRANSITION)
		{
			this.FallCheck();
		}
		this.FailSafeChecks();
		if (this.hero_state == ActorStates.running && !this.cState.dashing && !this.cState.backDashing && !this.controlReqlinquished)
		{
			if (this.cState.inWalkZone)
			{
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN, true);
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_SPRINT, true);
				this.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_WALK, true);
			}
			else
			{
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK, true);
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_SPRINT, true);
				this.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_RUN, true);
			}
			Vector2 linearVelocity = this.rb2d.linearVelocity;
			bool flag = (linearVelocity.x < -0.1f || linearVelocity.x > 0.1f) && !this.cState.inWalkZone;
			if (this.runEffect && !flag)
			{
				this.runEffect.Stop();
				this.runEffect = null;
			}
			if (!this.runEffect && flag)
			{
				this.runEffect = this.runEffectPrefab.Spawn<RunEffects>();
				this.runEffect.transform.SetParent(base.gameObject.transform, false);
				this.runEffect.StartEffect(true, false);
			}
		}
		else
		{
			if (this.ForceWalkingSound)
			{
				this.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_WALK, true);
			}
			else
			{
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK, true);
			}
			if (this.ForceRunningSound)
			{
				this.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_RUN, true);
			}
			else
			{
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN, true);
			}
			if (this.runEffect)
			{
				this.runEffect.Stop();
				this.runEffect = null;
			}
			if ((this.cState.isSprinting || this.cState.isBackScuttling) && this.cState.onGround)
			{
				this.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_SPRINT, true);
			}
			else
			{
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_SPRINT, true);
			}
		}
		if (this.hero_state == ActorStates.dash_landing)
		{
			this.dashLandingTimer += Time.deltaTime;
			if (this.dashLandingTimer > this.DOWN_DASH_RECOVER_TIME)
			{
				this.BackOnGround(true);
				this.FinishedDashing(true);
			}
		}
		if (this.hero_state == ActorStates.hard_landing)
		{
			this.hardLandingTimer += Time.deltaTime;
			if (this.hardLandingTimer > this.HARD_LANDING_TIME)
			{
				this.SetState(ActorStates.grounded);
				this.BackOnGround(false);
			}
		}
		else if (this.hero_state == ActorStates.no_input)
		{
			this.LookForInput();
			if (this.cState.recoiling)
			{
				if (this.recoilTimer < this.RECOIL_DURATION)
				{
					this.recoilTimer += Time.deltaTime;
				}
				else
				{
					this.StartRevengeWindow();
					this.CancelDamageRecoil();
					if ((this.prev_hero_state == ActorStates.idle || this.prev_hero_state == ActorStates.running) && !this.CheckTouchingGround())
					{
						this.cState.onGround = false;
						this.SetState(ActorStates.airborne);
					}
					else
					{
						this.SetState(ActorStates.previous);
					}
				}
			}
		}
		else if (this.hero_state != ActorStates.no_input)
		{
			this.LookForInput();
			if (this.cState.recoiling)
			{
				this.cState.recoiling = false;
				this.AffectedByGravity(true);
			}
			if (this.cState.attacking && !this.cState.dashing)
			{
				this.attack_time += Time.deltaTime;
				if (this.attack_time >= this.attackDuration)
				{
					this.ResetAttacks(true);
					this.animCtrl.StopAttack();
				}
			}
			if (this.cState.isToolThrowing)
			{
				this.toolThrowTime += Time.deltaTime;
				if (this.toolThrowTime >= this.toolThrowDuration || (this.queuedAutoThrowTool && this.toolThrowTime >= 0.15f))
				{
					bool flag2 = this.queuedAutoThrowTool;
					this.ThrowToolEnd();
					if (flag2)
					{
						this.ThrowTool(true);
					}
				}
			}
			if (this.cState.bouncing)
			{
				if (this.bounceTimer < this.BOUNCE_TIME)
				{
					this.bounceTimer += Time.deltaTime;
				}
				else
				{
					this.CancelBounce();
					this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, 0f);
				}
			}
			if (this.cState.shroomBouncing && this.current_velocity.y <= 0f)
			{
				this.cState.shroomBouncing = false;
			}
			if (this.cState.shuttleCock)
			{
				if (this.cState.attacking || (this.cState.falling && !this.cState.jumping) || this.cState.downSpikeAntic)
				{
					this.ShuttleCockCancel();
					if (this.cState.falling && this.inputHandler.inputActions.Dash.IsPressed)
					{
						this.sprintFSM.SendEvent("TRY SPRINT");
					}
				}
				if (this.controlReqlinquished)
				{
					this.ShuttleCockCancel();
				}
				this.shuttlecockTime += Time.deltaTime;
				this.shuttlecockTimeResetTimer = 0.1f;
			}
			else if (this.shuttlecockTimeResetTimer > 0f)
			{
				this.shuttlecockTimeResetTimer -= Time.deltaTime;
			}
			else if (this.shuttlecockTime > 0f)
			{
				this.shuttlecockTime = 0f;
			}
			if (this.hero_state == ActorStates.idle && !this.controlReqlinquished && !this.IsPaused() && !this.IsInputBlocked() && this.CanAttack())
			{
				if (this.lastLookDirection == 0)
				{
					if (this.inputHandler.inputActions.Up.IsPressed || this.inputHandler.inputActions.RsUp.IsPressed)
					{
						this.lastLookDirection = 1;
					}
					else if (this.inputHandler.inputActions.Down.IsPressed || this.inputHandler.inputActions.RsDown.IsPressed)
					{
						this.lastLookDirection = 2;
					}
				}
				else if (this.lastLookDirection == 1)
				{
					if (this.inputHandler.inputActions.Up.IsPressed || this.inputHandler.inputActions.RsUp.IsPressed)
					{
						this.lastLookDirection = 1;
					}
					else
					{
						this.lastLookDirection = 0;
					}
				}
				else if (this.lastLookDirection == 2)
				{
					if (this.inputHandler.inputActions.Down.IsPressed || this.inputHandler.inputActions.RsDown.IsPressed)
					{
						this.lastLookDirection = 2;
					}
					else
					{
						this.lastLookDirection = 0;
					}
				}
				if (this.lastLookDirection == 1)
				{
					this.cState.lookingDown = false;
					this.cState.lookingDownAnim = false;
					if (this.lookDelayTimer >= this.LOOK_DELAY || (this.inputHandler.inputActions.RsUp.IsPressed && !this.cState.jumping && !this.cState.dashing))
					{
						this.cState.lookingUp = true;
					}
					else
					{
						this.lookDelayTimer += Time.deltaTime;
					}
					if (this.lookDelayTimer >= this.LOOK_ANIM_DELAY || this.inputHandler.inputActions.RsUp.IsPressed)
					{
						this.cState.lookingUpAnim = true;
					}
					else
					{
						this.cState.lookingUpAnim = false;
					}
				}
				else if (this.lastLookDirection == 2)
				{
					this.cState.lookingUp = false;
					this.cState.lookingUpAnim = false;
					if (this.lookDownBlocked)
					{
						this.cState.lookingDown = false;
					}
					else
					{
						if (this.lookDelayTimer >= this.LOOK_DELAY || (this.inputHandler.inputActions.RsDown.IsPressed && !this.cState.jumping && !this.cState.dashing))
						{
							this.cState.lookingDown = true;
						}
						else
						{
							this.lookDelayTimer += Time.deltaTime;
						}
						if (this.lookDelayTimer >= this.LOOK_ANIM_DELAY || this.inputHandler.inputActions.RsDown.IsPressed)
						{
							this.cState.lookingDownAnim = true;
						}
						else
						{
							this.cState.lookingDownAnim = false;
						}
					}
				}
				else
				{
					this.ResetLook();
					this.lookDownBlocked = false;
				}
			}
		}
		this.LookForQueueInput();
		if (this.cState.wallSliding)
		{
			if (this.airDashed)
			{
				this.airDashed = false;
			}
			if (this.doubleJumped)
			{
				this.doubleJumped = false;
			}
			if (this.cState.onGround)
			{
				this.FlipSprite();
				this.CancelWallsliding();
			}
			else if (!this.cState.touchingWall)
			{
				this.FlipSprite();
				this.CancelWallsliding();
			}
			else if (!this.CanContinueWallSlide())
			{
				this.CancelWallsliding();
			}
			if (this.cState.wallSliding)
			{
				if (!this.playedMantisClawClip)
				{
					this.audioSource.PlayOneShot(this.mantisClawClip, 1f);
					this.playedMantisClawClip = true;
				}
				bool wallClinging = this.cState.wallClinging;
				this.cState.wallClinging = (!NoWallClingRegion.IsWallClingBlocked && this.wallClingCooldownTimer <= 0f && Gameplay.WallClingTool.Status.IsEquipped && ((this.wallSlidingL && this.move_input < 0f) || (this.wallSlidingR && this.move_input > 0f)));
				if (this.cState.wallClinging)
				{
					if (!wallClinging)
					{
						this.wallClingEffect.SetActive(false);
						this.wallClingEffect.SetActive(true);
					}
					this.wallslideClipTimer = 0f;
					this.audioCtrl.StopSound(HeroSounds.WALLSLIDE, true);
					this.playingWallslideClip = false;
					this.AffectedByGravity(false);
				}
				else
				{
					if (wallClinging)
					{
						this.wallClingCooldownTimer = this.WALLCLING_COOLDOWN;
					}
					if (!this.playingWallslideClip)
					{
						if (this.wallslideClipTimer <= this.WALLSLIDE_CLIP_DELAY)
						{
							this.wallslideClipTimer += Time.deltaTime;
						}
						else
						{
							this.wallslideClipTimer = 0f;
							this.audioCtrl.PlaySound(HeroSounds.WALLSLIDE, true);
							this.vibrationCtrl.StartWallSlide();
							this.playingWallslideClip = true;
						}
					}
				}
			}
		}
		else
		{
			if (this.playedMantisClawClip)
			{
				this.playedMantisClawClip = false;
			}
			if (this.playingWallslideClip)
			{
				this.audioCtrl.StopSound(HeroSounds.WALLSLIDE, true);
				this.playingWallslideClip = false;
			}
			if (this.wallslideClipTimer > 0f)
			{
				this.wallslideClipTimer = 0f;
			}
			if (this.wallStickTimer > 0f)
			{
				this.wallStickTimer = 0f;
			}
			if (this.wallSlashing)
			{
				this.CancelAttack();
			}
		}
		if (this.attack_cooldown > 0f)
		{
			this.attack_cooldown -= Time.deltaTime;
		}
		if (this.throwToolCooldown > 0f)
		{
			this.throwToolCooldown -= Time.deltaTime;
		}
		if (this.dashCooldownTimer > 0f)
		{
			this.dashCooldownTimer -= Time.deltaTime;
		}
		if (this.shadowDashTimer > 0f)
		{
			this.shadowDashTimer -= Time.deltaTime;
			if (this.shadowDashTimer <= 0f)
			{
				this.spriteFlash.FlashShadowRecharge();
			}
		}
		if (this.harpoonDashCooldown > 0f)
		{
			if (this.cState.onGround)
			{
				this.harpoonDashCooldown = 0f;
			}
			this.harpoonDashCooldown -= Time.deltaTime;
		}
		if (this.wallClingCooldownTimer > 0f)
		{
			this.wallClingCooldownTimer -= Time.deltaTime;
		}
		if (this.cState.downSpikeRecovery)
		{
			if (this.cState.onGround)
			{
				if (this.downSpikeRecoveryTimer >= this.DOWNSPIKE_LAND_RECOVERY_TIME)
				{
					this.cState.downSpikeRecovery = false;
				}
			}
			else if (this.downSpikeRecoveryTimer >= this.Config.DownspikeRecoveryTime)
			{
				this.cState.downSpikeRecovery = false;
			}
			this.downSpikeRecoveryTimer += Time.deltaTime;
		}
		if (this.cState.downSpikeAntic)
		{
			if (this.downSpikeTimer >= this.Config.DownSpikeAnticTime)
			{
				if (this.Config.DownspikeBurstEffect)
				{
					this.downspikeBurstPrefab.SetActive(true);
				}
				this.downSpikeTimer = 0f;
				this.cState.downSpikeAntic = false;
				this.cState.downSpiking = true;
				this.allowAttackCancellingDownspikeRecovery = false;
				this.currentDownspike.StartSlash();
			}
			else
			{
				this.downSpikeTimer += Time.deltaTime;
			}
		}
		if (this.refillSoundSuppressFramesLeft > 0)
		{
			this.refillSoundSuppressFramesLeft--;
		}
		if (this.evadingDidClash && !this.cState.evading)
		{
			this.evadingDidClash = false;
		}
		if (!this.gm.isPaused && !this.playerData.isInventoryOpen)
		{
			if (this.animCtrl.IsPlayingUpdraftAnim)
			{
				this.audioCtrl.PlaySound(HeroSounds.UPDRAFT_IDLE, false);
			}
			else
			{
				this.audioCtrl.StopSound(HeroSounds.UPDRAFT_IDLE, true);
			}
			if (this.animCtrl.IsPlayingWindyAnim)
			{
				this.audioCtrl.PlaySound(HeroSounds.WINDY_IDLE, false);
			}
			else
			{
				this.audioCtrl.StopSound(HeroSounds.WINDY_IDLE, true);
			}
			if (this.inputHandler.inputActions.Attack.IsPressed && this.CanNailCharge())
			{
				this.cState.nailCharging = true;
				this.nailChargeTimer += Time.deltaTime;
			}
			else if (this.cState.nailCharging || this.nailChargeTimer != 0f)
			{
				this.CancelNailCharge();
			}
			float currentNailChargeTime = this.CurrentNailChargeTime;
			if (this.cState.nailCharging && this.nailChargeTimer > this.CurrentNailChargeBeginTime && !this.artChargeEffect.activeSelf && this.nailChargeTimer < currentNailChargeTime)
			{
				this.artChargeEffect.SetActive(true);
				this.audioCtrl.PlaySound(HeroSounds.NAIL_ART_CHARGE, true);
			}
			if (this.artChargeEffect.activeSelf && (!this.cState.nailCharging || this.nailChargeTimer > currentNailChargeTime))
			{
				this.StopNailChargeEffects();
			}
			if (!this.artChargedEffect.activeSelf && this.nailChargeTimer >= currentNailChargeTime)
			{
				this.artChargedEffect.SetActive(true);
				this.artChargedEffectAnim.PlayFromFrame(0);
				GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
				this.audioSource.PlayOneShot(this.nailArtChargeComplete, 1f);
				this.audioCtrl.PlaySound(HeroSounds.NAIL_ART_READY, true);
				this.spriteFlash.flashFocusGet();
				this.cState.nailCharging = true;
				GameCameras.instance.cameraController.ScreenFlash(new Color(1f, 1f, 1f, 0.3f));
			}
			if (this.artChargedEffect.activeSelf && (this.nailChargeTimer < currentNailChargeTime || !this.cState.nailCharging))
			{
				this.artChargedEffect.SetActive(false);
				this.audioCtrl.StopSound(HeroSounds.NAIL_ART_READY, true);
			}
			if (this.gm.GameState == GameState.CUTSCENE)
			{
				this.ResetAllCrestStateMinimal(false);
			}
			else if (this.gm.GameState == GameState.PLAYING)
			{
				if (!this.isSilkRegenBlocked && this.hasSilkSpoolAppeared && !this.cState.dead)
				{
					if (this.silkRegenDelayLeft > 0f)
					{
						this.silkRegenDelayLeft -= Time.deltaTime;
						if (this.silkRegenDelayLeft <= 0f)
						{
							this.StartSilkRegen();
						}
					}
					if (this.silkRegenDurationLeft > 0f)
					{
						this.silkRegenDurationLeft -= Time.deltaTime;
						if (this.silkRegenDurationLeft <= 0f)
						{
							this.DoSilkRegen();
						}
					}
				}
				if (this.warriorState.IsInRageMode)
				{
					this.warriorState.RageEffectTimeLeft = this.warriorState.RageEffectTimeLeft - Time.deltaTime;
					if (this.warriorState.RageEffectTimeLeft <= 0f)
					{
						this.ResetWarriorRageEffect();
					}
					this.warriorState.RageHealTimeLeft = this.warriorState.RageHealTimeLeft - Time.deltaTime;
					if (this.warriorState.RageHealTimeLeft <= 0f)
					{
						this.ResetWarriorCrestState();
					}
				}
				if (this.reaperState.IsInReaperMode)
				{
					this.reaperState.ReaperModeDurationLeft = this.reaperState.ReaperModeDurationLeft - Time.deltaTime;
					if (this.reaperState.ReaperModeDurationLeft <= 0f)
					{
						this.ResetReaperCrestState();
					}
				}
				if (this.silkPartsTimeLeft > 0f)
				{
					this.silkPartsTimeLeft -= Time.deltaTime;
					if (this.silkPartsTimeLeft <= 0f)
					{
						this.playerData.silkParts = 0;
						this.gm.gameCams.silkSpool.RefreshSilk();
					}
				}
				if (this.IsUsingQuickening)
				{
					this.quickeningTimeLeft -= Time.deltaTime;
					if (this.quickeningTimeLeft <= 0f || this.playerData.atBench || ToolItemManager.ActiveState != ToolsActiveStates.Active)
					{
						this.StopQuickening();
					}
				}
				if (this.parryInvulnTimer > 0f)
				{
					this.parryInvulnTimer -= Time.deltaTime;
				}
				if (this.revengeWindowTimer > 0f)
				{
					this.revengeWindowTimer -= Time.deltaTime;
				}
				if (this.wandererDashComboWindowTimer > 0f)
				{
					this.wandererDashComboWindowTimer -= Time.deltaTime;
				}
				bool flag3 = !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.isBinding;
				this.TickFrostEffect(flag3);
				if (flag3)
				{
					this.TickSilkEat(this.playerData.silk > 0 && this.cState.isMaggoted && !this.cState.swimming, ref this.maggotedSilkTracker, SilkSpool.SilkUsingFlags.Maggot, SilkSpool.SilkTakeSource.Normal, this.MAGGOTED_SILK_EAT_DELAY_FIRST, this.MAGGOTED_SILK_EAT_DELAY, this.MAGGOTED_SILK_EAT_DURATION);
				}
				if (!this.cState.dead && this.lavaBellCooldownTimeLeft > 0f)
				{
					if (Gameplay.LavaBellTool.Status.IsEquipped)
					{
						float num = Gameplay.LavaBellCooldownTime - this.lavaBellCooldownTimeLeft;
						this.lavaBellCooldownTimeLeft -= Time.deltaTime;
						float num2 = Gameplay.LavaBellCooldownTime - this.lavaBellCooldownTimeLeft;
						float num3 = Gameplay.LavaBellCooldownTime - 1.25f;
						if (num < num3 && num2 >= num3)
						{
							this.spawnedLavaBellRechargeEffect.SetActive(true);
							EventRegister.SendEvent(EventRegisterEvents.LavaBellRecharging, null);
						}
						if (this.lavaBellCooldownTimeLeft <= 0f)
						{
							this.spriteFlash.FlashLavaBellRecharge();
						}
					}
					else
					{
						this.ResetLavaBell();
					}
				}
				this.tagDamageTaker.Tick(true);
				if (this.silkTauntEffectTimeLeft > 0f)
				{
					this.silkTauntEffectTimeLeft -= Time.deltaTime;
				}
				if (this.ringTauntEffectTimeLeft > 0f)
				{
					this.ringTauntEffectTimeLeft -= Time.deltaTime;
				}
				if (InteractManager.BlockingInteractable == null)
				{
					this.TickDeliveryItems();
				}
				if (this.runningWaterEffect)
				{
					if (this.enviroRegionListener.CurrentEnvironmentType == EnvironmentTypes.RunningWater && this.cState.onGround)
					{
						if (!this.runningWaterEffect.isPlaying)
						{
							if (this.areaEffectTint)
							{
								this.areaEffectTint.DoTint();
							}
							this.runningWaterEffect.Play(true);
						}
					}
					else if (this.runningWaterEffect.isPlaying)
					{
						this.runningWaterEffect.Stop(true);
					}
				}
			}
		}
		if ((this.gm.isPaused || this.playerData.isInventoryOpen) && !this.inputHandler.inputActions.Attack.IsPressed)
		{
			this.CancelNailCharge();
		}
		if (!NoClamberRegion.IsClamberBlocked && !this.cState.onGround && (!this.controlReqlinquished || this.allowMantle) && !this.cState.attacking && !this.cState.upAttacking && !this.cState.downAttacking && !this.cState.wallSliding && !this.cState.doubleJumping && !this.cState.downSpikeBouncing && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && !this.cState.recoiling && this.gm.GameState == GameState.PLAYING && this.rb2d.linearVelocity.y < 5f && !SlideSurface.IsHeroInside && !this.CheckTouchingGround() && (((this.inputHandler.inputActions.Left.IsPressed || (this.velocity_prev.x < -0.1f && this.inputHandler.inputActions.Dash.IsPressed)) && this.CheckStillTouchingWall(CollisionSide.left, false, false)) || ((this.inputHandler.inputActions.Right.IsPressed || (this.velocity_prev.x > 0.1f && this.inputHandler.inputActions.Dash.IsPressed)) && this.CheckStillTouchingWall(CollisionSide.right, false, false))))
		{
			if (!this.wallLocked && !this.cState.dashing)
			{
				if (this.inputHandler.inputActions.Left.IsPressed)
				{
					this.FaceLeft();
				}
				else if (this.inputHandler.inputActions.Right.IsPressed)
				{
					this.FaceRight();
				}
			}
			this.mantleFSM.SendEvent("AIR MANTLE");
		}
	}

	// Token: 0x06000D63 RID: 3427 RVA: 0x0003CA24 File Offset: 0x0003AC24
	public void SetIsMaggoted(bool value)
	{
		bool isMaggoted = this.cState.isMaggoted;
		this.cState.isMaggoted = value;
		if (value)
		{
			if (!this.spawnedMaggotEffect && this.maggotEffectPrefab)
			{
				this.spawnedMaggotEffect = this.maggotEffectPrefab.Spawn<PlayParticleEffects>();
				this.spawnedMaggotEffect.transform.SetParent(this.transform, true);
				this.spawnedMaggotEffect.transform.SetLocalPosition2D(Vector2.zero);
				this.spawnedMaggotEffect.PlayParticleSystems();
			}
			if (!this.spriteFlash.IsFlashing(true, this.maggotedFlash))
			{
				this.maggotedFlash = this.spriteFlash.FlashingMaggot();
			}
			StatusVignette.AddStatus(StatusVignette.StatusTypes.Maggoted);
			if (!isMaggoted)
			{
				Effects.BeginMaggotedSound.SpawnAndPlayOneShot(this.transform.position, null);
			}
			if (this.spawnedMaggotEnter)
			{
				this.spawnedMaggotEnter.SetActive(false);
				this.spawnedMaggotEnter.SetActive(true);
			}
			this.ResetAllCrestState();
		}
		else
		{
			if (this.spawnedMaggotEffect)
			{
				this.spawnedMaggotEffect.StopParticleSystems();
				this.spawnedMaggotEffect = null;
			}
			if (this.spriteFlash.IsFlashing(true, this.maggotedFlash))
			{
				this.spriteFlash.CancelRepeatingFlash(this.maggotedFlash);
				this.maggotedFlash = default(SpriteFlash.FlashHandle);
			}
			if (isMaggoted)
			{
				StatusVignette.RemoveStatus(StatusVignette.StatusTypes.Maggoted);
			}
		}
		EventRegister.SendEvent(EventRegisterEvents.MaggotCheck, null);
	}

	// Token: 0x06000D64 RID: 3428 RVA: 0x0003CB8C File Offset: 0x0003AD8C
	public void AddToMaggotCharmTimer(float delta)
	{
		if (this.playerData.MaggotCharmHits >= 3)
		{
			return;
		}
		this.maggotCharmTimer += delta;
		float maggotCharmHealthLossTime = Gameplay.MaggotCharmHealthLossTime;
		if (this.maggotCharmTimer < maggotCharmHealthLossTime)
		{
			return;
		}
		this.maggotCharmTimer %= maggotCharmHealthLossTime;
		this.playerData.MaggotCharmHits++;
		EventRegister.SendEvent(EventRegisterEvents.MaggotCheck, null);
	}

	// Token: 0x06000D65 RID: 3429 RVA: 0x0003CBF4 File Offset: 0x0003ADF4
	public void DidMaggotCharmHit()
	{
		this.maggotCharmTimer = 0f;
		this.playerData.MaggotCharmHits++;
		EventRegister.SendEvent(EventRegisterEvents.MaggotCheck, null);
		GameObject maggotCharmHitSinglePrefab = Gameplay.MaggotCharmHitSinglePrefab;
		if (maggotCharmHitSinglePrefab)
		{
			Vector3 position = this.transform.position;
			position.z = maggotCharmHitSinglePrefab.transform.position.z;
			maggotCharmHitSinglePrefab.Spawn(position);
		}
	}

	// Token: 0x06000D66 RID: 3430 RVA: 0x0003CC64 File Offset: 0x0003AE64
	private void TickFrostEffect(bool shouldTickInto)
	{
		if (this.cState.dead && !this.cState.isFrostDeath)
		{
			return;
		}
		float num = this.cState.isFrostDeath ? 100f : this.GetTotalFrostSpeed();
		bool flag = this.playerData.hasDoubleJump;
		if (num > Mathf.Epsilon)
		{
			if (!shouldTickInto)
			{
				return;
			}
			num *= 1f;
			if (this.Config.ForceBareInventory)
			{
				num *= 2f;
				flag = false;
			}
		}
		else
		{
			num -= 100f;
		}
		float num2 = num / 100f;
		float value = this.frostAmount + num2 * Time.deltaTime;
		if (flag)
		{
			this.frostAmount = Mathf.Clamp(value, 0f, 0.035f);
			Action<float> frostAmountUpdated = this.FrostAmountUpdated;
			if (frostAmountUpdated != null)
			{
				frostAmountUpdated(0f);
			}
		}
		else
		{
			this.frostAmount = Mathf.Clamp01(value);
			Action<float> frostAmountUpdated2 = this.FrostAmountUpdated;
			if (frostAmountUpdated2 != null)
			{
				frostAmountUpdated2(this.frostAmount);
			}
		}
		StatusVignette.SetFrostVignetteAmount(this.frostAmount);
		if (this.cState.dead)
		{
			return;
		}
		bool isFrosted = this.cState.isFrosted;
		this.cState.isFrosted = (!flag && this.frostAmount >= 1f - Mathf.Epsilon);
		bool flag2 = this.isInFrostRegion;
		this.isInFrostRegion = (num2 > Mathf.Epsilon);
		bool inFrostRegion = this.cState.inFrostRegion;
		this.cState.inFrostRegion = (!flag && this.isInFrostRegion);
		if (this.cState.isFrosted != isFrosted || this.cState.inFrostRegion != inFrostRegion)
		{
			EventRegister.SendEvent(EventRegisterEvents.FrostUpdateHealth, null);
		}
		if (this.cState.inFrostRegion)
		{
			if (!this.spriteFlash.IsFlashing(true, this.frostRegionFlash))
			{
				this.frostRegionFlash = this.spriteFlash.FlashingFrosted();
			}
			if (!this.frostedEffect.IsAlive())
			{
				this.frostedEffect.PlayParticleSystems();
			}
		}
		else
		{
			if (this.spriteFlash.IsFlashing(true, this.frostRegionFlash))
			{
				this.spriteFlash.CancelRepeatingFlash(this.frostRegionFlash);
				this.frostRegionFlash = default(SpriteFlash.FlashHandle);
			}
			if (this.frostedEffect.IsAlive())
			{
				this.frostedEffect.StopParticleSystems();
			}
		}
		if (this.isInFrostRegion)
		{
			if (!flag2 && this.spawnedFrostEnter)
			{
				this.spawnedFrostEnter.SetActive(true);
			}
		}
		else if (flag2 && num < this.gm.sm.FrostSpeed - 100f - Mathf.Epsilon && this.spawnedHeatEnter)
		{
			this.spawnedHeatEnter.SetActive(true);
		}
		if (this.cState.isFrosted)
		{
			if (!this.spriteFlash.IsFlashing(true, this.frostAnticFlash))
			{
				this.frostAnticFlash = this.spriteFlash.FlashingFrostAntic();
			}
			if (!this.frostedAudioLoop.isPlaying || this.frostedFadeOutRoutine != null)
			{
				if (this.frostedFadeOutRoutine != null)
				{
					base.StopCoroutine(this.frostedFadeOutRoutine);
					this.frostedFadeOutRoutine = null;
				}
				this.frostedEnterAudio.SpawnAndPlayOneShot(Audio.DefaultUIAudioSourcePrefab, this.frostedAudioLoop.transform.position, null);
				this.frostedAudioLoop.volume = 1f;
				this.frostedAudioLoop.Play();
			}
			this.frostDamageTimer += Time.deltaTime;
			if (this.frostDamageTimer >= 1.75f)
			{
				this.frostDamageTimer %= 1.75f;
				this.TakeFrostDamage(1);
				this.spriteFlash.Flash(new Color(0.6f, 0.8f, 1f), 0.9f, 0.1f, 0.01f, 0.1f, 0f, false, 0, 0, false);
				return;
			}
		}
		else
		{
			this.frostDamageTimer -= Time.deltaTime;
			if (this.frostDamageTimer < 0f)
			{
				this.frostDamageTimer = 0f;
			}
			if (this.spriteFlash.IsFlashing(true, this.frostAnticFlash))
			{
				this.spriteFlash.CancelRepeatingFlash(this.frostAnticFlash);
				this.frostAnticFlash = default(SpriteFlash.FlashHandle);
			}
			if (this.frostedAudioLoop.isPlaying && this.frostedFadeOutRoutine == null)
			{
				this.frostedFadeOutRoutine = this.StartTimerRoutine(0f, this.frostedAudioLoopFadeOut, delegate(float t)
				{
					this.frostedAudioLoop.volume = 1f - t;
				}, null, delegate
				{
					this.frostedAudioLoop.Stop();
					this.frostedFadeOutRoutine = null;
				}, false);
			}
		}
	}

	// Token: 0x06000D67 RID: 3431 RVA: 0x0003D0B8 File Offset: 0x0003B2B8
	public void SpawnDeliveryItemEffect(DeliveryQuestItem deliveryQuestItem)
	{
		if (deliveryQuestItem == null)
		{
			return;
		}
		if (!this.spawnedDeliveryEffects.ContainsKey(deliveryQuestItem))
		{
			GameObject value = null;
			if (deliveryQuestItem.HeroLoopEffect)
			{
				value = deliveryQuestItem.HeroLoopEffect.Spawn(this.transform, Vector3.zero);
			}
			this.spawnedDeliveryEffects[deliveryQuestItem] = value;
		}
	}

	// Token: 0x06000D68 RID: 3432 RVA: 0x0003D110 File Offset: 0x0003B310
	public void RemoveDeliveryItemEffect(DeliveryQuestItem deliveryQuestItem)
	{
		if (deliveryQuestItem == null)
		{
			return;
		}
		GameObject gameObject;
		if (this.spawnedDeliveryEffects.TryGetValue(deliveryQuestItem, out gameObject))
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			this.spawnedDeliveryEffects.Remove(deliveryQuestItem);
		}
	}

	// Token: 0x06000D69 RID: 3433 RVA: 0x0003D154 File Offset: 0x0003B354
	public void SetupDeliveryItems()
	{
		if (this.currentTimedDeliveries == null)
		{
			this.currentTimedDeliveries = new List<HeroController.DeliveryTimer>();
		}
		this.currentTimedDeliveries.Clear();
		foreach (DeliveryQuestItem.ActiveItem activeItem in DeliveryQuestItem.GetActiveItems())
		{
			float chunkDuration = activeItem.Item.GetChunkDuration(activeItem.MaxCount);
			if (chunkDuration > Mathf.Epsilon)
			{
				this.SpawnDeliveryItemEffect(activeItem.Item);
				this.currentTimedDeliveries.Add(new HeroController.DeliveryTimer
				{
					Item = activeItem,
					TimeLeft = chunkDuration
				});
			}
		}
		this.CleanSpawnedDeliveryEffects();
	}

	// Token: 0x06000D6A RID: 3434 RVA: 0x0003D208 File Offset: 0x0003B408
	private void TickDeliveryItems()
	{
		if (this.currentTimedDeliveries == null)
		{
			return;
		}
		for (int i = this.currentTimedDeliveries.Count - 1; i >= 0; i--)
		{
			HeroController.DeliveryTimer deliveryTimer = this.currentTimedDeliveries[i];
			deliveryTimer.TimeLeft -= Time.deltaTime;
			if (deliveryTimer.TimeLeft > 0f)
			{
				this.currentTimedDeliveries[i] = deliveryTimer;
			}
			else
			{
				deliveryTimer.Item.CurrentCount = deliveryTimer.Item.Item.CollectedAmount;
				DeliveryQuestItem.TakeHitForItem(deliveryTimer.Item, false);
			}
		}
	}

	// Token: 0x06000D6B RID: 3435 RVA: 0x0003D297 File Offset: 0x0003B497
	public IEnumerable<HeroController.DeliveryTimer> GetDeliveryTimers()
	{
		if (this.currentTimedDeliveries == null)
		{
			return Enumerable.Empty<HeroController.DeliveryTimer>();
		}
		return this.currentTimedDeliveries;
	}

	// Token: 0x06000D6C RID: 3436 RVA: 0x0003D2B0 File Offset: 0x0003B4B0
	private float GetTotalFrostSpeed()
	{
		if (CheatManager.IsFrostDisabled)
		{
			return 0f;
		}
		float num = this.gm.sm.FrostSpeed;
		foreach (FrostRegion frostRegion in FrostRegion.FrostRegions)
		{
			if (frostRegion.IsInside)
			{
				num += frostRegion.FrostSpeed;
			}
		}
		if (this.NailImbuement.CurrentElement == NailElements.Fire)
		{
			num *= 0.7f;
		}
		if (this.warriorState.IsInRageMode)
		{
			num *= 0.9f;
		}
		if (Gameplay.WispLanternTool.Status.IsEquipped)
		{
			num *= 0.8f;
		}
		return num;
	}

	// Token: 0x06000D6D RID: 3437 RVA: 0x0003D370 File Offset: 0x0003B570
	private void TickSilkEat(bool shouldEatSilk, ref HeroController.SilkEatTracker tracker, SilkSpool.SilkUsingFlags usingType, SilkSpool.SilkTakeSource takeSource, float firstDelay, float delay, float duration)
	{
		if (shouldEatSilk)
		{
			if (tracker.EatDurationLeft <= 0f)
			{
				if (tracker.EatDelayLeft <= 0f)
				{
					tracker.EatDelayLeft = firstDelay;
				}
				else
				{
					tracker.EatDelayLeft -= Time.deltaTime;
				}
				if (tracker.EatDelayLeft <= 0f)
				{
					tracker.EatDurationLeft = duration;
					SilkSpool.Instance.AddUsing(usingType, 1);
				}
			}
			else
			{
				if (tracker.EatSilkCount != this.playerData.silk)
				{
					tracker.EatDurationLeft = duration;
				}
				else
				{
					tracker.EatDurationLeft -= Time.deltaTime;
				}
				if (tracker.EatDurationLeft <= 0f)
				{
					SilkSpool.Instance.RemoveUsing(usingType, 1);
					this.TakeSilk(1, takeSource);
					tracker.EatDurationLeft = 0f;
					tracker.EatDelayLeft = ((this.playerData.silk > 0) ? delay : 0f);
				}
			}
		}
		else
		{
			if (tracker.EatDurationLeft > 0f)
			{
				SilkSpool.Instance.RemoveUsing(usingType, 1);
			}
			tracker.EatDelayLeft = 0f;
			tracker.EatDurationLeft = 0f;
		}
		tracker.EatSilkCount = this.playerData.silk;
	}

	// Token: 0x06000D6E RID: 3438 RVA: 0x0003D49C File Offset: 0x0003B69C
	public void ShuttleCockCancel()
	{
		if (this.cState.shuttleCock && !this.cState.onGround)
		{
			this.AddExtraAirMoveVelocity(new HeroController.DecayingVelocity
			{
				Velocity = new Vector2(this.shuttlecockSpeed, 0f),
				Decay = 5f,
				CancelOnTurn = true,
				SkipBehaviour = HeroController.DecayingVelocity.SkipBehaviours.WhileMoving
			});
		}
		this.ShuttleCockCancelInert();
	}

	// Token: 0x06000D6F RID: 3439 RVA: 0x0003D50B File Offset: 0x0003B70B
	private void ShuttleCockCancelInert()
	{
		this.cState.shuttleCock = false;
		if (this.shuttleCockJumpAudio)
		{
			this.shuttleCockJumpAudio.Stop();
		}
		this.vibrationCtrl.StopShuttlecock();
	}

	// Token: 0x06000D70 RID: 3440 RVA: 0x0003D53C File Offset: 0x0003B73C
	private void FixedUpdate()
	{
		if (this.cState.recoilingLeft || this.cState.recoilingRight)
		{
			if (this.recoilStepsLeft > 0)
			{
				this.recoilStepsLeft--;
			}
			else
			{
				this.CancelRecoilHorizontal();
			}
		}
		for (int i = this.extraAirMoveVelocities.Count - 1; i >= 0; i--)
		{
			HeroController.DecayingVelocity decayingVelocity = this.extraAirMoveVelocities[i];
			decayingVelocity.Velocity -= decayingVelocity.Velocity * (decayingVelocity.Decay * Time.deltaTime);
			if (decayingVelocity.Velocity.magnitude < 0.01f)
			{
				this.extraAirMoveVelocities.RemoveAt(i);
			}
			else
			{
				this.extraAirMoveVelocities[i] = decayingVelocity;
			}
		}
		if (this.cState.dead)
		{
			this.rb2d.linearVelocity = new Vector2(0f, 0f);
		}
		this.UpdateSteepSlopes();
		if ((this.hero_state == ActorStates.hard_landing && !this.cState.onConveyor) || this.hero_state == ActorStates.dash_landing)
		{
			this.ResetMotion(false);
			this.UpdateEdgeAdjust();
		}
		else if (this.hero_state == ActorStates.no_input)
		{
			this.didCheckEdgeAdjust = false;
			if (this.cState.transitioning)
			{
				if (this.transitionState == HeroTransitionState.EXITING_SCENE)
				{
					if (this.transition_vel.y > Mathf.Epsilon || (this.cState.onGround && this.transition_vel.magnitude > Mathf.Epsilon))
					{
						this.AffectedByGravity(false);
					}
					if (!this.stopWalkingOut)
					{
						this.rb2d.linearVelocity = new Vector2(this.transition_vel.x, this.transition_vel.y + this.rb2d.linearVelocity.y);
					}
				}
				else if (this.transitionState == HeroTransitionState.ENTERING_SCENE)
				{
					this.rb2d.linearVelocity = this.transition_vel;
					if (this.transition_vel.x > 0f)
					{
						this.CheckForBump(CollisionSide.right);
					}
					else if (this.transition_vel.x < 0f)
					{
						this.CheckForBump(CollisionSide.left);
					}
				}
				else if (this.transitionState == HeroTransitionState.DROPPING_DOWN)
				{
					this.rb2d.linearVelocity = new Vector2(this.transition_vel.x, this.rb2d.linearVelocity.y);
				}
			}
			else if (this.cState.recoiling)
			{
				this.AffectedByGravity(false);
				this.rb2d.linearVelocity = this.recoilVector;
			}
		}
		else if (this.hero_state != ActorStates.no_input)
		{
			if (this.cState.transitioning)
			{
				return;
			}
			this.DoMovement(this.acceptingInput);
			if ((this.cState.lookingUp || this.cState.lookingDown) && Mathf.Abs(this.move_input) > 0.6f)
			{
				this.ResetLook();
			}
			if (this.cState.jumping && !this.cState.dashing && !this.cState.isSprinting)
			{
				this.Jump();
			}
			if (this.cState.doubleJumping)
			{
				this.DoubleJump();
			}
			if (this.cState.dashing)
			{
				this.Dash();
			}
			if (this.cState.downSpiking)
			{
				this.Downspike();
			}
			if (this.cState.floating)
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.FLOAT_SPEED);
				this.ResetHardLandingTimer();
			}
			if (this.cState.casting)
			{
				if (this.cState.castRecoiling)
				{
					if (this.cState.facingRight)
					{
						this.rb2d.linearVelocity = new Vector2(-this.CAST_RECOIL_VELOCITY, 0f);
					}
					else
					{
						this.rb2d.linearVelocity = new Vector2(this.CAST_RECOIL_VELOCITY, 0f);
					}
				}
				else
				{
					this.rb2d.linearVelocity = Vector2.zero;
				}
			}
			if (this.cState.bouncing)
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.BOUNCE_VELOCITY);
			}
			bool shroomBouncing = this.cState.shroomBouncing;
			if (this.cState.downSpikeBouncing)
			{
				if (this.downspike_rebound_steps <= this.DOWNSPIKE_REBOUND_STEPS)
				{
					if (this.downspike_rebound_xspeed != 0f)
					{
						this.rb2d.linearVelocity = new Vector2(this.downspike_rebound_xspeed, this.rb2d.linearVelocity.y);
					}
					this.downspike_rebound_steps++;
				}
				else
				{
					this.cState.downSpikeBouncing = false;
				}
			}
			if (this.wallJumpChainStepsLeft > 0)
			{
				this.wallJumpChainStepsLeft--;
			}
			if (this.wallLocked)
			{
				if (this.wallJumpedR)
				{
					this.rb2d.linearVelocity = new Vector2(this.currentWalljumpSpeed, this.rb2d.linearVelocity.y);
				}
				else if (this.wallJumpedL)
				{
					this.rb2d.linearVelocity = new Vector2(-this.currentWalljumpSpeed, this.rb2d.linearVelocity.y);
				}
				this.wallLockSteps++;
				if (this.wallLockSteps > this.WJLOCK_STEPS_LONG)
				{
					this.wallLocked = false;
					this.wallJumpChainStepsLeft = this.WJLOCK_CHAIN_STEPS;
				}
				this.currentWalljumpSpeed -= this.walljumpSpeedDecel;
			}
			if (this.cState.wallSliding)
			{
				HeroActions inputActions = this.inputHandler.inputActions;
				bool flag = false;
				if (this.wallSlidingL)
				{
					if (inputActions.Right.IsPressed && !inputActions.Left.IsPressed)
					{
						flag = true;
					}
				}
				else if (this.wallSlidingR && inputActions.Left.IsPressed && !inputActions.Right.IsPressed)
				{
					flag = true;
				}
				if (flag)
				{
					this.wallUnstickSteps++;
				}
				else
				{
					this.wallUnstickSteps = 0;
				}
				if (this.wallUnstickSteps >= this.WALL_STICKY_STEPS)
				{
					this.FlipSprite();
					this.CancelWallsliding();
				}
				if (this.wallSlidingL)
				{
					if (!this.CheckStillTouchingWall(CollisionSide.left, false, true))
					{
						this.FlipSprite();
						this.CancelWallsliding();
					}
				}
				else if (this.wallSlidingR && !this.CheckStillTouchingWall(CollisionSide.right, false, true))
				{
					this.FlipSprite();
					this.CancelWallsliding();
				}
			}
			if (this.hero_state == ActorStates.running)
			{
				if (this.move_input > 0f)
				{
					this.CheckForBump(CollisionSide.right);
				}
				else if (this.move_input < 0f)
				{
					this.CheckForBump(CollisionSide.left);
				}
			}
		}
		if (this.cState.downSpikeAntic && this.Config.DownspikeThrusts && this.downSpikeTimer < this.Config.DownSpikeAnticTime)
		{
			this.rb2d.linearVelocity *= this.DOWNSPIKE_ANTIC_DECELERATION;
		}
		float maxFallVelocity = this.GetMaxFallVelocity();
		if (this.rb2d.linearVelocity.y < -maxFallVelocity && (this.ForceClampTerminalVelocity || this.cState.isBackSprinting || this.cState.isBackScuttling || (!this.controlReqlinquished && !this.cState.shadowDashing && !this.cState.spellQuake)))
		{
			this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, -maxFallVelocity);
		}
		if (this.jumpQueuing)
		{
			this.jumpQueueSteps++;
		}
		if (this.doubleJumpQueuing)
		{
			this.doubleJumpQueueSteps++;
		}
		if (this.dashQueuing)
		{
			this.dashQueueSteps++;
		}
		if (this.attackQueuing)
		{
			this.attackQueueSteps++;
		}
		if (this.harpoonQueuing)
		{
			this.harpoonQueueSteps++;
		}
		if (this.toolThrowQueueing)
		{
			this.toolThrowQueueSteps++;
		}
		if (this.doMaxSilkRegen)
		{
			this.maxSilkRegenTimer += Time.deltaTime;
			if (this.maxSilkRegenTimer >= 0.03f)
			{
				if (this.playerData.CurrentSilkRegenMax - this.playerData.silk > 0)
				{
					this.maxSilkRegenTimer %= 0.03f;
					this.AddSilk(1, false);
				}
				if (this.playerData.CurrentSilkRegenMax - this.playerData.silk <= 0)
				{
					this.maxSilkRegenTimer = 0f;
					this.doMaxSilkRegen = false;
				}
			}
		}
		if (this.cState.superDashOnWall && !this.cState.onConveyorV)
		{
			this.rb2d.linearVelocity = new Vector3(0f, 0f);
		}
		if (this.cState.onConveyor && (this.cState.onGround || this.cState.isSprinting || this.hero_state == ActorStates.hard_landing))
		{
			if (this.cState.freezeCharge || this.hero_state == ActorStates.hard_landing || (this.controlReqlinquished && !this.cState.isSprinting && !this.cState.isBackScuttling && !this.cState.isBackSprinting))
			{
				this.rb2d.linearVelocity = new Vector3(0f, 0f);
			}
			if (this.BeforeApplyConveyorSpeed != null)
			{
				this.BeforeApplyConveyorSpeed(this.rb2d.linearVelocity);
			}
			Vector2 linearVelocity = this.rb2d.linearVelocity;
			this.rb2d.linearVelocity = new Vector2(linearVelocity.x + this.conveyorSpeed, linearVelocity.y);
		}
		if (this.cState.inConveyorZone)
		{
			if (this.cState.freezeCharge || this.hero_state == ActorStates.hard_landing)
			{
				this.rb2d.linearVelocity = new Vector3(0f, 0f);
			}
			float num = this.conveyorSpeed;
			if (this.cState.isSprinting)
			{
				num *= 2f;
			}
			Vector2 position = this.rb2d.position;
			position.x += num * Time.fixedDeltaTime;
			this.rb2d.position = position;
		}
		if (this.cState.shuttleCock)
		{
			if (this.cState.onGround && this.rb2d.linearVelocity.y <= 0f)
			{
				this.ShuttleCockCancel();
				if (this.inputHandler.inputActions.Dash.IsPressed)
				{
					this.sprintFSM.SendEvent("TRY SPRINT");
				}
				else
				{
					this.ForceSoftLanding();
				}
			}
			else
			{
				this.rb2d.linearVelocity = new Vector3(this.shuttlecockSpeed, this.rb2d.linearVelocity.y);
			}
		}
		if (!this.didAirHang && !this.cState.onGround && this.rb2d.linearVelocity.y < 0f)
		{
			if (!this.controlReqlinquished && this.transitionState == HeroTransitionState.WAITING_TO_TRANSITION)
			{
				this.StartAirHang();
			}
			this.didAirHang = true;
		}
		if (this.cState.onGround)
		{
			if (this.didAirHang)
			{
				this.didAirHang = false;
			}
			if (this.airDashed)
			{
				this.airDashed = false;
			}
		}
		if (this.rb2d.gravityScale < this.DEFAULT_GRAVITY && !this.controlReqlinquished && this.hero_state != ActorStates.no_input && !this.cState.wallSliding)
		{
			this.rb2d.gravityScale += this.AIR_HANG_ACCEL * Time.deltaTime;
		}
		if (this.rb2d.gravityScale < this.DEFAULT_GRAVITY && !this.inputHandler.inputActions.Jump.IsPressed && !this.controlReqlinquished && this.hero_state != ActorStates.no_input && !this.cState.wallSliding)
		{
			this.rb2d.gravityScale = this.DEFAULT_GRAVITY;
		}
		if (this.rb2d.gravityScale > this.DEFAULT_GRAVITY)
		{
			this.rb2d.gravityScale = this.DEFAULT_GRAVITY;
		}
		this.velocity_crt = this.rb2d.linearVelocity;
		this.velocity_prev = this.velocity_crt;
		if ((this.tryShove || (this.cState.falling && this.cState.wasOnGround)) && ((!this.cState.onGround && this.velocity_crt == Vector2.zero && this.hero_state != ActorStates.dash_landing) || (this.dashingDown && !this.onFlatGround)) && !this.cState.hazardRespawning && !this.cState.transitioning && !this.playerData.atBench)
		{
			this.ShoveOff();
		}
		this.tryShove = false;
		this.onFlatGround = false;
		if (this.landingBufferSteps > 0)
		{
			this.landingBufferSteps--;
		}
		if (this.ledgeBufferSteps > 0)
		{
			this.ledgeBufferSteps--;
		}
		if (this.sprintBufferSteps > 0)
		{
			this.sprintBufferSteps--;
		}
		if (this.shuttleCockJumpSteps > 0)
		{
			this.shuttleCockJumpSteps--;
		}
		if (this.headBumpSteps > 0)
		{
			this.headBumpSteps--;
		}
		if (this.jumpReleaseQueueSteps > 0)
		{
			this.jumpReleaseQueueSteps--;
		}
		if (this.cState.downspikeInvulnerabilitySteps > 0)
		{
			this.cState.downspikeInvulnerabilitySteps--;
		}
		this.positionHistory[1] = this.positionHistory[0];
		this.positionHistory[0] = this.transform.position;
		this.cState.wasOnGround = this.cState.onGround;
	}

	// Token: 0x06000D71 RID: 3441 RVA: 0x0003E2BC File Offset: 0x0003C4BC
	private void ShoveOff()
	{
		float num = Mathf.Sign(this.transform.localScale.x) * 0.2f;
		Vector3 position = this.transform.position;
		this.transform.position = new Vector3(position.x + num, position.y, position.z);
	}

	// Token: 0x06000D72 RID: 3442 RVA: 0x0003E315 File Offset: 0x0003C515
	public void UpdateMoveInput()
	{
		this.move_input = this.inputHandler.inputActions.MoveVector.Vector.x;
	}

	// Token: 0x06000D73 RID: 3443 RVA: 0x0003E338 File Offset: 0x0003C538
	public void DoMovement(bool useInput)
	{
		if (CheatManager.IsOpen)
		{
			this.move_input = 0f;
		}
		if (this.cState.backDashing || this.cState.dashing)
		{
			return;
		}
		this.Move(this.move_input, useInput);
		this.UpdateEdgeAdjust();
		if ((!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.wallSliding && !this.wallLocked && !this.cState.shuttleCock && !this.cState.isToolThrowing && this.TrySetCorrectFacing(false))
		{
			if (this.Config.CanTurnWhileSlashing)
			{
				this.CancelAttackNotSlash(false);
			}
			else
			{
				this.CancelAttack(false);
			}
		}
		this.DoRecoilMovement();
	}

	// Token: 0x06000D74 RID: 3444 RVA: 0x0003E400 File Offset: 0x0003C600
	public void DoRecoilMovement()
	{
		if (this.cState.recoilingLeft)
		{
			Vector2 linearVelocity = this.rb2d.linearVelocity;
			this.rb2d.linearVelocity = ((linearVelocity.x - this.recoilVelocity > -this.recoilVelocity) ? new Vector2(-this.recoilVelocity, linearVelocity.y) : new Vector2(linearVelocity.x - this.recoilVelocity, linearVelocity.y));
		}
		if (this.cState.recoilingRight)
		{
			Vector2 linearVelocity2 = this.rb2d.linearVelocity;
			this.rb2d.linearVelocity = ((linearVelocity2.x + this.recoilVelocity < this.recoilVelocity) ? new Vector2(this.recoilVelocity, linearVelocity2.y) : new Vector2(linearVelocity2.x + this.recoilVelocity, linearVelocity2.y));
		}
	}

	// Token: 0x06000D75 RID: 3445 RVA: 0x0003E4D7 File Offset: 0x0003C6D7
	public void ConveyorReset()
	{
		this.cState.inConveyorZone = false;
		this.conveyorSpeed = 0f;
		this.cState.onConveyor = false;
		this.cState.onConveyorV = false;
	}

	// Token: 0x06000D76 RID: 3446 RVA: 0x0003E508 File Offset: 0x0003C708
	public void SetBlockSteepSlopes(bool blocked)
	{
		this.blockSteepSlopes = blocked;
		if (blocked)
		{
			this.cState.isTouchingSlopeLeft = false;
			this.cState.isTouchingSlopeRight = false;
		}
	}

	// Token: 0x06000D77 RID: 3447 RVA: 0x0003E52C File Offset: 0x0003C72C
	private static bool IsSteepSlopeRayHitting(Vector2 heroCentre, Vector2 heroMin, Vector2 heroMax, float rayPadding, int layerMask, bool checkLeft, out RaycastHit2D hit)
	{
		Vector2 origin = new Vector2(heroCentre.x, heroMin.y + Physics2D.defaultContactOffset);
		Vector2 direction = checkLeft ? Vector2.left : Vector2.right;
		float distance = (checkLeft ? (heroCentre.x - heroMin.x) : (heroMax.x - heroCentre.x)) + rayPadding;
		int num = Physics2D.RaycastNonAlloc(origin, direction, HeroController._rayHitStore, distance, layerMask);
		bool flag = false;
		RaycastHit2D raycastHit2D = default(RaycastHit2D);
		hit = default(RaycastHit2D);
		for (int i = 0; i < num; i++)
		{
			hit = HeroController._rayHitStore[i];
			Collider2D collider = hit.collider;
			if (!collider.isTrigger && SteepSlope.IsSteepSlope(collider))
			{
				if (!flag || hit.distance < raycastHit2D.distance)
				{
					raycastHit2D = hit;
				}
				flag = true;
			}
			HeroController._rayHitStore[i] = default(RaycastHit2D);
		}
		return flag;
	}

	// Token: 0x06000D78 RID: 3448 RVA: 0x0003E614 File Offset: 0x0003C814
	private void UpdateSteepSlopes()
	{
		if (this.blockSteepSlopes)
		{
			return;
		}
		this.cState.isTouchingSlopeLeft = false;
		this.cState.isTouchingSlopeRight = false;
		ActorStates actorStates = this.hero_state;
		bool flag = actorStates == ActorStates.running || actorStates == ActorStates.airborne;
		Vector2 linearVelocity = this.rb2d.linearVelocity;
		Bounds bounds = this.col2d.bounds;
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Vector3 center = bounds.center;
		float rayPadding = Mathf.Abs(linearVelocity.x) * Time.fixedDeltaTime + 0.05f;
		RaycastHit2D raycastHit2D;
		bool flag2;
		if (HeroController.IsSteepSlopeRayHitting(center, min, max, rayPadding, 33562880, true, out raycastHit2D))
		{
			flag2 = true;
		}
		else
		{
			if (!HeroController.IsSteepSlopeRayHitting(center, min, max, rayPadding, 33562880, false, out raycastHit2D))
			{
				return;
			}
			flag2 = false;
		}
		Vector2 normal = raycastHit2D.normal;
		if (normal.y < 0f)
		{
			return;
		}
		this.cState.isTouchingSlopeLeft = flag2;
		this.cState.isTouchingSlopeRight = !flag2;
		if (!flag)
		{
			return;
		}
		float f = normal.y / normal.x;
		float num = Physics2D.gravity.y * Mathf.Abs(f) * 4f;
		linearVelocity.y += num * Time.fixedDeltaTime;
		this.rb2d.linearVelocity = linearVelocity;
		if (this.cState.shuttleCock)
		{
			this.ShuttleCockCancelInert();
		}
		this.ResetHardLandingTimer();
	}

	// Token: 0x06000D79 RID: 3449 RVA: 0x0003E79C File Offset: 0x0003C99C
	private void UpdateEdgeAdjust()
	{
		if (this.cState.onGround && (this.hero_state == ActorStates.idle || this.hero_state == ActorStates.hard_landing))
		{
			if (!this.didCheckEdgeAdjust)
			{
				if (this.cState.falling && !this.controlReqlinquished && this.rb2d.linearVelocity.y == 0f && !this.wallSlidingL && !this.wallSlidingR)
				{
					return;
				}
				this.DoEdgeAdjust();
				this.didCheckEdgeAdjust = true;
				return;
			}
		}
		else
		{
			this.didCheckEdgeAdjust = false;
		}
	}

	// Token: 0x06000D7A RID: 3450 RVA: 0x0003E824 File Offset: 0x0003CA24
	public bool GetWillThrowTool(bool reportFailure)
	{
		AttackToolBinding binding;
		if (this.inputHandler.inputActions.Up.IsPressed)
		{
			this.willThrowTool = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Up, ToolEquippedReadSource.Active, out binding);
		}
		else if (this.inputHandler.inputActions.Down.IsPressed)
		{
			this.willThrowTool = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Down, ToolEquippedReadSource.Active, out binding);
		}
		else
		{
			this.willThrowTool = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Neutral, ToolEquippedReadSource.Active, out binding);
		}
		return this.willThrowTool && this.CanThrowTool(this.willThrowTool, binding, reportFailure);
	}

	// Token: 0x06000D7B RID: 3451 RVA: 0x0003E8B0 File Offset: 0x0003CAB0
	private bool CanThrowTool(ToolItem tool, AttackToolBinding binding, bool reportFailure)
	{
		ToolItemType type = tool.Type;
		if (type == ToolItemType.Red)
		{
			if (!tool.IsEmpty || (tool.UsableWhenEmpty && !tool.UsableWhenEmptyPrevented))
			{
				int silkRequired = tool.Usage.SilkRequired;
				if (silkRequired <= 0 || this.playerData.silk >= silkRequired)
				{
					return true;
				}
				EventRegister.SendEvent(EventRegisterEvents.BindFailedNotEnough, null);
			}
			if (reportFailure)
			{
				ToolItemManager.ReportBoundAttackToolFailed(binding);
			}
			return false;
		}
		if (type != ToolItemType.Skill)
		{
			return false;
		}
		if (this.playerData.silk >= this.playerData.SilkSkillCost)
		{
			return true;
		}
		if (reportFailure)
		{
			ToolItemManager.ReportBoundAttackToolFailed(binding);
			EventRegister.SendEvent(EventRegisterEvents.BindFailedNotEnough, null);
		}
		return false;
	}

	// Token: 0x06000D7C RID: 3452 RVA: 0x0003E951 File Offset: 0x0003CB51
	public void SetToolCooldown(float cooldown)
	{
		this.canThrowTime = Time.timeAsDouble + (double)cooldown;
	}

	// Token: 0x06000D7D RID: 3453 RVA: 0x0003E964 File Offset: 0x0003CB64
	private void ThrowTool(bool isAutoThrow)
	{
		if (!isAutoThrow && Time.timeAsDouble < this.canThrowTime)
		{
			return;
		}
		if (!this.willThrowTool)
		{
			return;
		}
		AttackToolBinding? attackToolBinding = ToolItemManager.GetAttackToolBinding(this.willThrowTool);
		if (attackToolBinding == null)
		{
			return;
		}
		this.inputHandler.inputActions.QuickCast.ClearInputState();
		ToolItem.UsageOptions usage = this.willThrowTool.Usage;
		ToolItemsData.Data savedData = this.willThrowTool.SavedData;
		bool isEmpty = this.willThrowTool.IsEmpty;
		if (!this.CanThrowTool(this.willThrowTool, attackToolBinding.Value, true))
		{
			return;
		}
		if (this.cState.isToolThrowing)
		{
			this.ThrowToolEnd();
		}
		this.ResetLook();
		this.cState.recoiling = false;
		this.cState.floating = false;
		this.CancelAttack(true);
		if (!isAutoThrow && this.TrySetCorrectFacing(true) && this.cState.wallSliding)
		{
			this.FlipSprite();
		}
		if (this.willThrowTool.Type == ToolItemType.Skill)
		{
			if (this.skillEventTarget.IsEventValid(usage.FsmEventName, true).Value)
			{
				EventRegister.SendEvent(EventRegisterEvents.FsmCancel, null);
				this.skillEventTarget.SendEvent(usage.FsmEventName);
				return;
			}
			Debug.LogErrorFormat(this, "Fsm Skill Tool Event {0} is invalid", new object[]
			{
				usage.FsmEventName
			});
			return;
		}
		else
		{
			if (!usage.ThrowPrefab && !usage.IsNonBlockingEvent)
			{
				if (this.toolEventTarget.IsEventValid(usage.FsmEventName, true).Value)
				{
					if (this.cState.wallSliding)
					{
						this.FlipSprite();
						this.CancelWallsliding();
					}
					string activeStateName = this.toolEventTarget.ActiveStateName;
					this.toolEventTarget.SendEventSafe("TAKE CONTROL");
					this.toolEventTarget.SendEvent(usage.FsmEventName);
					if (this.toolEventTarget.ActiveStateName != activeStateName)
					{
						this.DidUseAttackTool(savedData);
						return;
					}
				}
				else
				{
					Debug.LogErrorFormat(this, "Fsm Tool Event {0} is invalid", new object[]
					{
						usage.FsmEventName
					});
				}
				return;
			}
			this.DidUseAttackTool(savedData);
			this.cState.isToolThrowing = true;
			this.cState.toolThrowCount++;
			this.cState.throwingToolVertical = this.willThrowTool.Usage.ThrowAnimVerticalDirection;
			this.toolThrowDuration = ((this.cState.throwingToolVertical > 0) ? this.animCtrl.GetClipDuration("ToolThrow Up") : this.animCtrl.GetClipDuration("ToolThrow Q"));
			this.toolThrowTime = 0f;
			this.DidAttack();
			this.throwToolCooldown = usage.ThrowCooldown;
			this.attackAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
			if (usage.ThrowPrefab)
			{
				Transform transform;
				Vector2 v;
				float num;
				float direction;
				if (this.cState.wallSliding)
				{
					transform = this.toolThrowWallPoint;
					v = Vector2.right;
					num = -1f;
					direction = (float)(this.cState.facingRight ? 180 : 0);
				}
				else
				{
					transform = this.toolThrowPoint;
					v = Vector2.left;
					num = 1f;
					direction = (float)(this.cState.facingRight ? 0 : 180);
				}
				if (this.toolThrowClosePoint)
				{
					ref Vector3 position = transform.position;
					Vector3 v2 = this.toolThrowClosePoint.TransformDirection(v);
					Vector3 position2 = this.toolThrowClosePoint.position;
					float length = Mathf.Abs(position.x - position2.x);
					if (global::Helper.IsRayHittingNoTriggers(position2, v2, length, 8448))
					{
						transform = this.toolThrowClosePoint;
					}
				}
				bool flag = isAutoThrow && usage.UseAltForQuickSling;
				Vector2 v3 = flag ? usage.ThrowOffsetAlt : usage.ThrowOffset;
				if (this.cState.wallSliding)
				{
					v3.x *= -1f;
				}
				v3.y += Random.Range(-0.1f, 0.1f);
				GameObject gameObject = usage.ThrowPrefab.Spawn(transform.TransformPoint(v3));
				if (usage.ScaleToHero)
				{
					Vector3 localScale = gameObject.transform.localScale;
					float num2 = this.cState.facingRight ? (-num) : num;
					localScale.x = num2 * usage.ThrowPrefab.transform.localScale.x;
					if (usage.FlipScale)
					{
						localScale.x = -localScale.x;
					}
					gameObject.transform.localScale = localScale;
					if (usage.SetDamageDirection)
					{
						DamageEnemies component = gameObject.GetComponent<DamageEnemies>();
						if (component)
						{
							component.SetDirection(direction);
						}
					}
				}
				Vector2 vector = (flag ? usage.ThrowVelocityAlt : usage.ThrowVelocity).MultiplyElements(new float?(num), null);
				if (vector.magnitude > Mathf.Epsilon)
				{
					Rigidbody2D component2 = gameObject.GetComponent<Rigidbody2D>();
					if (component2)
					{
						if (!this.cState.facingRight)
						{
							vector.x *= -1f;
						}
						if (Mathf.Abs(vector.y) > Mathf.Epsilon)
						{
							float magnitude = vector.magnitude;
							vector = (vector.normalized.DirectionToAngle() + Random.Range(-2f, 2f)).AngleToDirection() * magnitude;
						}
						component2.linearVelocity = vector;
					}
				}
				this.vibrationCtrl.PlayToolThrow();
			}
			else
			{
				this.toolEventTarget.SendEventSafe(usage.FsmEventName);
			}
			this.willThrowTool.OnWasUsed(isEmpty);
			bool flag2 = Gameplay.QuickSlingTool.Status.IsEquipped && !ToolItemManager.IsCustomToolOverride;
			if (flag2)
			{
				this.quickSlingAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
			}
			if (!isAutoThrow && flag2 && this.CanThrowTool(this.willThrowTool, attackToolBinding.Value, false))
			{
				this.queuedAutoThrowTool = true;
				return;
			}
			this.willThrowTool = null;
			this.queuedAutoThrowTool = false;
			return;
		}
	}

	// Token: 0x06000D7E RID: 3454 RVA: 0x0003EF54 File Offset: 0x0003D154
	private void DidUseAttackTool(ToolItemsData.Data toolData)
	{
		if (!this.willThrowTool.IsCustomUsage && toolData.AmountLeft > 0)
		{
			toolData.AmountLeft--;
			this.willThrowTool.SavedData = toolData;
			ToolItemLimiter.ReportToolUsed(this.willThrowTool);
		}
		AttackToolBinding value = ToolItemManager.GetAttackToolBinding(this.willThrowTool).Value;
		ToolItemManager.ReportBoundAttackToolUsed(value);
		ToolItemManager.ReportBoundAttackToolUpdated(value);
	}

	// Token: 0x06000D7F RID: 3455 RVA: 0x0003EFB8 File Offset: 0x0003D1B8
	private void ThrowToolEnd()
	{
		if (!this.cState.isToolThrowing)
		{
			return;
		}
		this.cState.isToolThrowing = false;
		this.queuedAutoThrowTool = false;
		this.animCtrl.StopToolThrow();
	}

	// Token: 0x06000D80 RID: 3456 RVA: 0x0003EFE6 File Offset: 0x0003D1E6
	public void QueueLifeBloodStateQuick()
	{
		if (!this.IsInLifebloodState)
		{
			this.queuedLifeBloodState = true;
		}
	}

	// Token: 0x06000D81 RID: 3457 RVA: 0x0003EFF7 File Offset: 0x0003D1F7
	public void EnterLifebloodStateQuick()
	{
		this.queuedLifeBloodState = false;
		EventRegister.SendEvent(EventRegisterEvents.FsmCancel, null);
		this.toolEventTarget.SendEventSafe("TAKE CONTROL");
		this.toolEventTarget.SendEvent("OVERBLUE QUICK");
	}

	// Token: 0x06000D82 RID: 3458 RVA: 0x0003F02C File Offset: 0x0003D22C
	public bool TrySetCorrectFacing(bool force = false)
	{
		if (!this.CanTurn && !force)
		{
			return false;
		}
		if (this.move_input > 0f && !this.cState.facingRight)
		{
			this.FlipSprite();
			return true;
		}
		if (this.move_input < 0f && this.cState.facingRight)
		{
			this.FlipSprite();
			return true;
		}
		return false;
	}

	// Token: 0x06000D83 RID: 3459 RVA: 0x0003F08C File Offset: 0x0003D28C
	private void DoEdgeAdjust()
	{
		if (SlideSurface.IsHeroInside)
		{
			return;
		}
		if (this.dashingDown)
		{
			return;
		}
		float edgeAdjustX = EdgeAdjustHelper.GetEdgeAdjustX(this.col2d, this.cState.facingRight, 0.25f, 0f);
		if (edgeAdjustX != 0f)
		{
			this.rb2d.MovePosition(this.rb2d.position + new Vector2(edgeAdjustX, 0f));
		}
	}

	// Token: 0x06000D84 RID: 3460 RVA: 0x0003F0FC File Offset: 0x0003D2FC
	private void Update10()
	{
		if (this.isGameplayScene)
		{
			this.OutOfBoundsCheck();
		}
		float scaleX = this.transform.GetScaleX();
		if (scaleX < -1f)
		{
			this.transform.SetScaleX(-1f);
		}
		if (scaleX > 1f)
		{
			this.transform.SetScaleX(1f);
		}
		if (!this.controlReqlinquished && Math.Abs(this.transform.position.z - 0.004f) >= Mathf.Epsilon)
		{
			this.transform.SetPositionZ(0.004f);
		}
	}

	// Token: 0x06000D85 RID: 3461 RVA: 0x0003F18C File Offset: 0x0003D38C
	private void LateUpdate()
	{
		if (!this.cState.wallSliding || this.cState.onConveyorV)
		{
			return;
		}
		Vector2 vector = this.rb2d.linearVelocity;
		if (this.cState.wallClinging)
		{
			if (vector.y < -10f)
			{
				vector.y = -10f;
			}
			if (vector.y < 0f)
			{
				vector.y += this.WALLCLING_DECEL * Time.deltaTime;
			}
			else if (vector.y > 0f)
			{
				vector.y = 0f;
			}
			this.wallStickTimer = 0f;
			this.wallStickStartVelocity = 0f;
		}
		else if (this.wallStickTimer < this.WALLSLIDE_STICK_TIME)
		{
			if (this.wallStickStartVelocity > 0f)
			{
				this.wallStickStartVelocity += this.WALLSLIDE_ACCEL * Time.deltaTime;
			}
			vector.y = Mathf.Max(this.wallStickStartVelocity, 0f);
			this.wallStickTimer += Time.deltaTime;
		}
		else
		{
			vector = new Vector3(vector.x, vector.y + this.WALLSLIDE_ACCEL * Time.deltaTime);
		}
		this.rb2d.linearVelocity = vector;
	}

	// Token: 0x06000D86 RID: 3462 RVA: 0x0003F2CF File Offset: 0x0003D4CF
	private void OnLevelUnload()
	{
		if (this.transform.parent != null)
		{
			this.SetHeroParent(null);
		}
	}

	// Token: 0x06000D87 RID: 3463 RVA: 0x0003F2EB File Offset: 0x0003D4EB
	private void OnDisable()
	{
		if (this.gm != null)
		{
			this.gm.UnloadingLevel -= this.OnLevelUnload;
		}
		this.ReattachHeroLight();
	}

	// Token: 0x06000D88 RID: 3464 RVA: 0x0003F318 File Offset: 0x0003D518
	private void Move(float moveDirection, bool useInput)
	{
		if (this.cState.onGround)
		{
			this.SetState(ActorStates.grounded);
		}
		if (this.cState.downSpikeRecovery && this.cState.onGround)
		{
			moveDirection = 0f;
		}
		if (this.cState.isTouchingSlopeLeft && moveDirection < 0f)
		{
			moveDirection = 0f;
		}
		else if (this.cState.isTouchingSlopeRight && moveDirection > 0f)
		{
			moveDirection = 0f;
		}
		Vector2 vector = this.rb2d.linearVelocity;
		if (useInput && !this.cState.wallSliding)
		{
			if (this.cState.inWalkZone && this.cState.onGround)
			{
				vector.x = moveDirection * this.GetWalkSpeed();
			}
			else
			{
				vector.x = moveDirection * this.GetRunSpeed();
			}
		}
		foreach (HeroController.DecayingVelocity decayingVelocity in this.extraAirMoveVelocities)
		{
			switch (decayingVelocity.SkipBehaviour)
			{
			case HeroController.DecayingVelocity.SkipBehaviours.None:
				break;
			case HeroController.DecayingVelocity.SkipBehaviours.WhileMoving:
				if (Math.Abs(this.move_input) > Mathf.Epsilon)
				{
					continue;
				}
				break;
			case HeroController.DecayingVelocity.SkipBehaviours.WhileMovingForward:
				if (this.cState.facingRight)
				{
					if (this.move_input > Mathf.Epsilon)
					{
						continue;
					}
				}
				else if (this.move_input < Mathf.Epsilon)
				{
					continue;
				}
				break;
			case HeroController.DecayingVelocity.SkipBehaviours.WhileMovingBackward:
				if (this.cState.facingRight)
				{
					if (this.move_input < Mathf.Epsilon)
					{
						continue;
					}
				}
				else if (this.move_input > Mathf.Epsilon)
				{
					continue;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			vector += decayingVelocity.Velocity;
		}
		this.rb2d.linearVelocity = vector;
	}

	// Token: 0x06000D89 RID: 3465 RVA: 0x0003F4D8 File Offset: 0x0003D6D8
	private void Jump()
	{
		if (this.jump_steps <= this.JUMP_STEPS)
		{
			if (this.isDashStabBouncing)
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.Config.DashStabBounceJumpSpeed);
			}
			else
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.useUpdraftExitJumpSpeed ? this.JUMP_SPEED_UPDRAFT_EXIT : this.JUMP_SPEED);
			}
			this.jump_steps++;
			this.jumped_steps++;
			this.ledgeBufferSteps = 0;
			this.sprintBufferSteps = 0;
			this.syncBufferSteps = false;
			return;
		}
		this.CancelJump();
	}

	// Token: 0x06000D8A RID: 3466 RVA: 0x0003F59C File Offset: 0x0003D79C
	private void DoubleJump()
	{
		if (this.cState.inUpdraft && this.CanFloat(true))
		{
			this.CancelDoubleJump();
			this.fsm_brollyControl.SendEvent("FORCE UPDRAFT ENTER");
			return;
		}
		if (this.doubleJump_steps <= this.DOUBLE_JUMP_RISE_STEPS + this.DOUBLE_JUMP_FALL_STEPS)
		{
			if (this.doubleJump_steps > this.DOUBLE_JUMP_FALL_STEPS)
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.JUMP_SPEED * 1.1f);
			}
			this.doubleJump_steps++;
		}
		else
		{
			this.CancelDoubleJump();
		}
		if (this.cState.onGround)
		{
			this.CancelDoubleJump();
		}
	}

	// Token: 0x06000D8B RID: 3467 RVA: 0x0003F64E File Offset: 0x0003D84E
	public void SetSlashComponent(NailSlash nailSlash)
	{
		this.SlashComponent = nailSlash;
	}

	// Token: 0x06000D8C RID: 3468 RVA: 0x0003F658 File Offset: 0x0003D858
	private void DoAttack()
	{
		this.queuedWallJumpInterrupt = false;
		this.ResetLook();
		if (this.cState.dashing || this.dashingDown)
		{
			this.CancelDash(true);
		}
		this.cState.recoiling = false;
		if (this.inputHandler.inputActions.Up.IsPressed)
		{
			this.Attack(AttackDirection.upward);
			return;
		}
		if (!this.inputHandler.inputActions.Down.IsPressed)
		{
			this.Attack(AttackDirection.normal);
			return;
		}
		if (this.allowAttackCancellingDownspikeRecovery || !this.cState.onGround)
		{
			this.Attack(AttackDirection.downward);
			return;
		}
		this.Attack(AttackDirection.normal);
	}

	// Token: 0x06000D8D RID: 3469 RVA: 0x0003F6FC File Offset: 0x0003D8FC
	public void IncrementAttackCounter()
	{
		this.cState.attackCount++;
	}

	// Token: 0x06000D8E RID: 3470 RVA: 0x0003F714 File Offset: 0x0003D914
	private void Attack(AttackDirection attackDir)
	{
		this.TrySetCorrectFacing(true);
		if (this.WarriorState.IsInRageMode)
		{
			this.warriorRageAttackAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
		}
		else
		{
			this.attackAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
		}
		this.cState.floating = false;
		this.IncrementAttackCounter();
		this.UpdateConfig();
		if (Time.timeSinceLevelLoad - this.altAttackTime > this.Config.AttackRecoveryTime + this.ALT_ATTACK_RESET || attackDir != this.prevAttackDir)
		{
			this.cState.altAttack = false;
		}
		if (attackDir != AttackDirection.downward || this.Config.DownSlashType != HeroControllerConfig.DownSlashTypes.DownSpike)
		{
			this.cState.attacking = true;
			this.attackDuration = this.Config.AttackDuration;
			if (this.IsUsingQuickening)
			{
				this.attackDuration *= 1f / this.Config.QuickAttackSpeedMult;
			}
		}
		bool flag = true;
		if (this.wandererDashComboWindowTimer > 0f && this.playerData.CurrentCrestID == "Wanderer")
		{
			this.sprintFSM.SendEvent("WANDERER DASH COMBO");
			return;
		}
		if ((this.cState.wallSliding || this.cState.wallScrambling) && attackDir == AttackDirection.normal && !Gameplay.ToolmasterCrest.IsEquipped)
		{
			if (this.Config.WallSlashSlowdown)
			{
				this.rb2d.SetVelocity(new float?(this.rb2d.linearVelocity.x), new float?(this.rb2d.linearVelocity.y / 2f));
				if (this.rb2d.linearVelocity.y < -5f)
				{
					this.rb2d.SetVelocity(new float?(this.rb2d.linearVelocity.x), new float?((float)-5));
				}
			}
			this.wallSlashing = true;
			this.SlashComponent = this.wallSlash;
			this.currentSlashDamager = this.wallSlashDamager;
		}
		else
		{
			if ((this.cState.wallSliding || this.cState.wallScrambling) && attackDir == AttackDirection.normal && Gameplay.ToolmasterCrest.IsEquipped)
			{
				this.FlipSprite();
				this.CancelWallsliding();
			}
			this.wallSlashing = false;
			switch (attackDir)
			{
			case AttackDirection.normal:
				if (this.cState.altAttack)
				{
					this.SlashComponent = this.alternateSlash;
					this.currentSlashDamager = this.alternateSlashDamager;
					this.cState.altAttack = false;
				}
				else
				{
					this.SlashComponent = this.normalSlash;
					this.currentSlashDamager = this.normalSlashDamager;
					if (this.alternateSlash)
					{
						this.cState.altAttack = true;
					}
				}
				break;
			case AttackDirection.upward:
				if (this.cState.wallSliding)
				{
					this.rb2d.MovePosition(this.rb2d.position + new Vector2(this.cState.facingRight ? -0.8f : 0.8f, 0f));
				}
				this.AttackCancelWallSlide();
				if (this.cState.altAttack)
				{
					this.SlashComponent = this.altUpSlash;
					this.currentSlashDamager = this.altUpSlashDamager;
					this.cState.altAttack = false;
				}
				else
				{
					this.SlashComponent = this.upSlash;
					this.currentSlashDamager = this.upSlashDamager;
					if (this.altUpSlash)
					{
						this.cState.altAttack = true;
					}
				}
				this.cState.upAttacking = true;
				break;
			case AttackDirection.downward:
				this.AttackCancelWallSlide();
				this.DownAttack(ref flag);
				break;
			default:
				throw new ArgumentOutOfRangeException("attackDir", attackDir, null);
			}
		}
		if (flag)
		{
			if (this.cState.wallSliding || this.cState.wallScrambling)
			{
				if (this.cState.facingRight)
				{
					this.currentSlashDamager.SetDirection(180f);
				}
				else
				{
					this.currentSlashDamager.SetDirection(0f);
				}
			}
			else if (attackDir == AttackDirection.normal && this.cState.facingRight)
			{
				this.currentSlashDamager.SetDirection(0f);
			}
			else if (attackDir == AttackDirection.normal && !this.cState.facingRight)
			{
				this.currentSlashDamager.SetDirection(180f);
			}
			else if (attackDir == AttackDirection.upward)
			{
				this.currentSlashDamager.SetDirection(90f);
			}
			else
			{
				this.currentSlashDamager.SetDirection(270f);
			}
			this.altAttackTime = Time.timeSinceLevelLoad;
			this.SlashComponent.StartSlash();
			this.DidAttack();
		}
		this.prevAttackDir = attackDir;
	}

	// Token: 0x06000D8F RID: 3471 RVA: 0x0003FBA2 File Offset: 0x0003DDA2
	private void AttackCancelWallSlide()
	{
		if (!this.cState.wallSliding)
		{
			return;
		}
		this.FlipSprite();
		this.CancelWallsliding();
	}

	// Token: 0x06000D90 RID: 3472 RVA: 0x0003FBBE File Offset: 0x0003DDBE
	public void QueueCancelDownAttack()
	{
		this.tryCancelDownSlash = true;
		this._slashComponent = this.downSlash;
	}

	// Token: 0x06000D91 RID: 3473 RVA: 0x0003FBD4 File Offset: 0x0003DDD4
	private void DownAttack(ref bool isSlashing)
	{
		this.CancelQueuedBounces();
		switch (this.Config.DownSlashType)
		{
		case HeroControllerConfig.DownSlashTypes.DownSpike:
		{
			isSlashing = false;
			if (this.transform.localScale.x > 0f)
			{
				this.downSpikeHorizontalSpeed = -this.Config.DownspikeSpeed;
			}
			else
			{
				this.downSpikeHorizontalSpeed = this.Config.DownspikeSpeed;
			}
			Vector2 linearVelocity = this.rb2d.linearVelocity;
			if (this.Config.DownspikeThrusts)
			{
				this.RelinquishControl();
				linearVelocity.y = Mathf.Clamp(linearVelocity.y, this.DOWNSPIKE_ANTIC_CLAMP_VEL_Y.Start, this.DOWNSPIKE_ANTIC_CLAMP_VEL_Y.End);
				this.AffectedByGravity(false);
			}
			else
			{
				this.CancelJump();
				if (linearVelocity.y > 0f)
				{
					linearVelocity.y = 0f;
				}
			}
			this.cState.downSpikeAntic = true;
			this.downSpikeTimer = 0f;
			if (this.cState.altAttack)
			{
				this.currentDownspike = this.altDownSpike;
				this.cState.altAttack = false;
			}
			else
			{
				this.currentDownspike = this.downSpike;
				if (this.altDownSpike)
				{
					this.cState.altAttack = true;
				}
			}
			this.rb2d.linearVelocity = linearVelocity;
			this.DidAttack();
			return;
		}
		case HeroControllerConfig.DownSlashTypes.Slash:
			if (this.cState.altAttack)
			{
				this.SlashComponent = this.altDownSlash;
				this.currentSlashDamager = this.altDownSlashDamager;
				this.cState.altAttack = false;
			}
			else
			{
				this.SlashComponent = this.downSlash;
				this.currentSlashDamager = this.downSlashDamager;
				if (this.altDownSlash)
				{
					this.cState.altAttack = true;
				}
			}
			this.cState.downAttacking = true;
			return;
		case HeroControllerConfig.DownSlashTypes.Custom:
			isSlashing = false;
			this.crestAttacksFSM.SendEvent(this.Config.DownSlashEvent);
			this.DidAttack();
			return;
		default:
			throw new NotImplementedException();
		}
	}

	// Token: 0x06000D92 RID: 3474 RVA: 0x0003FDC4 File Offset: 0x0003DFC4
	private void DidAttack()
	{
		float num = this.Config.AttackDuration;
		this.attack_cooldown = (this.IsUsingQuickening ? this.Config.QuickAttackCooldownTime : this.Config.AttackCooldownTime);
		if (this.attack_cooldown < num)
		{
			this.attack_cooldown = num;
		}
	}

	// Token: 0x06000D93 RID: 3475 RVA: 0x0003FE14 File Offset: 0x0003E014
	private void Dash()
	{
		if (this.CanWallSlide())
		{
			this.BeginWallSlide(false);
			return;
		}
		this.AffectedByGravity(false);
		HeroActions inputActions = this.inputHandler.inputActions;
		bool wasDashingDown = this.dashingDown;
		if (this.cState.onGround)
		{
			this.dashingDown = false;
		}
		this.cState.mantleRecovery = false;
		if (this.dashingDown)
		{
			this.cState.falling = true;
		}
		if (this.dash_timer <= 0f && (!this.dashingDown || !inputActions.Dash.IsPressed))
		{
			this.FinishedDashing(wasDashingDown);
			return;
		}
		float dash_SPEED = this.DASH_SPEED;
		if (this.dashingDown)
		{
			float maxFallVelocity = this.GetMaxFallVelocity();
			this.rb2d.linearVelocity = new Vector2(0f, -maxFallVelocity);
		}
		else
		{
			this.heroBox.HeroBoxAirdash();
			if (this.cState.facingRight)
			{
				this.rb2d.linearVelocity = new Vector2(dash_SPEED, 0f);
				this.CheckForBump(CollisionSide.right);
			}
			else
			{
				this.rb2d.linearVelocity = new Vector2(-dash_SPEED, 0f);
				this.CheckForBump(CollisionSide.left);
			}
		}
		this.dash_timer -= Time.deltaTime;
		this.dash_time += Time.deltaTime;
	}

	// Token: 0x06000D94 RID: 3476 RVA: 0x0003FF51 File Offset: 0x0003E151
	private void BackDash()
	{
	}

	// Token: 0x06000D95 RID: 3477 RVA: 0x0003FF54 File Offset: 0x0003E154
	private void Downspike()
	{
		if (this.downSpikeTimer > this.Config.DownSpikeTime)
		{
			this.FinishDownspike();
			return;
		}
		if (this.Config.DownspikeThrusts)
		{
			this.rb2d.linearVelocity = new Vector2(this.downSpikeHorizontalSpeed, -this.Config.DownspikeSpeed);
		}
		this.downSpikeTimer += Time.deltaTime;
	}

	// Token: 0x06000D96 RID: 3478 RVA: 0x0003FFBC File Offset: 0x0003E1BC
	private void StartAirHang()
	{
		this.rb2d.gravityScale = this.AIR_HANG_GRAVITY;
	}

	// Token: 0x06000D97 RID: 3479 RVA: 0x0003FFD0 File Offset: 0x0003E1D0
	public void FaceRight()
	{
		this.cState.facingRight = true;
		Vector3 localScale = this.transform.localScale;
		if (localScale.x < 0f)
		{
			return;
		}
		localScale.x = -1f;
		this.transform.localScale = localScale;
		this.ChangedFacing();
	}

	// Token: 0x06000D98 RID: 3480 RVA: 0x00040024 File Offset: 0x0003E224
	public void FaceLeft()
	{
		this.cState.facingRight = false;
		Vector3 localScale = this.transform.localScale;
		if (localScale.x > 0f)
		{
			return;
		}
		localScale.x = 1f;
		this.transform.localScale = localScale;
		this.ChangedFacing();
	}

	// Token: 0x06000D99 RID: 3481 RVA: 0x00040075 File Offset: 0x0003E275
	private void ChangedFacing()
	{
		this.airDashEffect.SetActive(false);
		Action flippedSprite = this.FlippedSprite;
		if (flippedSprite == null)
		{
			return;
		}
		flippedSprite();
	}

	// Token: 0x06000D9A RID: 3482 RVA: 0x00040093 File Offset: 0x0003E293
	public void SetBackOnGround()
	{
		this.BackOnGround(false);
	}

	// Token: 0x06000D9B RID: 3483 RVA: 0x0004009C File Offset: 0x0003E29C
	public void SetStartWithWallslide()
	{
		this.startWithWallslide = true;
	}

	// Token: 0x06000D9C RID: 3484 RVA: 0x000400A8 File Offset: 0x0003E2A8
	public bool TryFsmCancelToWallSlide()
	{
		if (!this.CanStartWithWallSlide())
		{
			return false;
		}
		if (!this.CheckStillTouchingWall(this.cState.facingRight ? CollisionSide.right : CollisionSide.left, false, true))
		{
			return false;
		}
		if (this.controlReqlinquished)
		{
			EventRegister.SendEvent(EventRegisterEvents.FsmCancel, null);
			this.RegainControl();
			this.StartAnimationControl();
		}
		this.ForceTouchingWall();
		this.BeginWallSlide(false);
		return true;
	}

	// Token: 0x06000D9D RID: 3485 RVA: 0x00040109 File Offset: 0x0003E309
	public void SetStartWithShuttlecock()
	{
		this.startWithShuttlecock = true;
	}

	// Token: 0x06000D9E RID: 3486 RVA: 0x00040112 File Offset: 0x0003E312
	public void SetStartWithJump()
	{
		this.startWithJump = true;
	}

	// Token: 0x06000D9F RID: 3487 RVA: 0x0004011B File Offset: 0x0003E31B
	public void SetStartWithWallJump()
	{
		this.startWithWallJump = true;
	}

	// Token: 0x06000DA0 RID: 3488 RVA: 0x00040124 File Offset: 0x0003E324
	public void SetStartWithTinyJump()
	{
		this.startWithTinyJump = true;
	}

	// Token: 0x06000DA1 RID: 3489 RVA: 0x0004012D File Offset: 0x0003E32D
	public void SetStartWithFlipJump()
	{
		this.startWithFlipJump = true;
		this.SetStartFromMantle();
		this.SetStartWithFullJump();
	}

	// Token: 0x06000DA2 RID: 3490 RVA: 0x00040144 File Offset: 0x0003E344
	public void SilkChargeEnd()
	{
		this.parryInvulnTimer = this.INVUL_TIME_SILKDASH;
		if (this.cState.onGround)
		{
			return;
		}
		this.AddExtraAirMoveVelocity(new HeroController.DecayingVelocity
		{
			Velocity = new Vector2(this.rb2d.linearVelocity.x, 0f),
			Decay = 4f,
			CancelOnTurn = true,
			SkipBehaviour = HeroController.DecayingVelocity.SkipBehaviours.WhileMoving
		});
		this.animCtrl.SetPlaySilkChargeEnd();
	}

	// Token: 0x06000DA3 RID: 3491 RVA: 0x000401C4 File Offset: 0x0003E3C4
	public void HarpoonDashEnd()
	{
		if (this.cState.onGround)
		{
			return;
		}
		this.AddExtraAirMoveVelocity(new HeroController.DecayingVelocity
		{
			Velocity = new Vector2(this.rb2d.linearVelocity.x, 0f),
			Decay = 4f,
			CancelOnTurn = true,
			SkipBehaviour = HeroController.DecayingVelocity.SkipBehaviours.WhileMoving
		});
		this.animCtrl.SetPlaySilkChargeEnd();
	}

	// Token: 0x06000DA4 RID: 3492 RVA: 0x00040236 File Offset: 0x0003E436
	public void SetStartWithAnyJump()
	{
		this.startWithAnyJump = true;
	}

	// Token: 0x06000DA5 RID: 3493 RVA: 0x0004023F File Offset: 0x0003E43F
	public void SetStartWithFullJump()
	{
		this.startWithFullJump = true;
	}

	// Token: 0x06000DA6 RID: 3494 RVA: 0x00040248 File Offset: 0x0003E448
	public void SetStartWithBackflipJump()
	{
		this.startWithBackflipJump = true;
		this.SetStartFromMantle();
		this.SetStartWithFullJump();
	}

	// Token: 0x06000DA7 RID: 3495 RVA: 0x0004025D File Offset: 0x0003E45D
	public void SetStartWithBrolly()
	{
		this.startWithBrolly = true;
	}

	// Token: 0x06000DA8 RID: 3496 RVA: 0x00040266 File Offset: 0x0003E466
	public void SetStartWithDoubleJump()
	{
		this.startWithDoubleJump = true;
	}

	// Token: 0x06000DA9 RID: 3497 RVA: 0x0004026F File Offset: 0x0003E46F
	public void SetStartWithWallsprintLaunch()
	{
		this.startWithWallsprintLaunch = true;
	}

	// Token: 0x06000DAA RID: 3498 RVA: 0x00040278 File Offset: 0x0003E478
	public void SetStartWithDash()
	{
		this.startWithDash = true;
	}

	// Token: 0x06000DAB RID: 3499 RVA: 0x00040281 File Offset: 0x0003E481
	public void SetStartWithDashKeepFacing()
	{
		this.startWithDash = true;
		this.dashCurrentFacing = true;
	}

	// Token: 0x06000DAC RID: 3500 RVA: 0x00040291 File Offset: 0x0003E491
	public void SetStartWithAttack()
	{
		this.startWithAttack = true;
	}

	// Token: 0x06000DAD RID: 3501 RVA: 0x0004029A File Offset: 0x0003E49A
	public void SetStartWithToolThrow()
	{
		this.startWithToolThrow = true;
		this.GetWillThrowTool(true);
	}

	// Token: 0x06000DAE RID: 3502 RVA: 0x000402AC File Offset: 0x0003E4AC
	public void SetStartWithDashStabBounce()
	{
		if (this.Config.ForceShortDashStabBounce)
		{
			this.startWithDownSpikeBounceShort = true;
			this.attack_cooldown = 0.1f;
		}
		else
		{
			if (global::Helper.IsRayHittingNoTriggers(this.rb2d.position, Vector2.up, 3.5f, 8448))
			{
				this.startWithDownSpikeBounceShort = true;
			}
			else
			{
				this.startWithDownSpikeBounce = true;
			}
			this.attack_cooldown = 0.15f;
		}
		this.isDashStabBouncing = true;
	}

	// Token: 0x06000DAF RID: 3503 RVA: 0x0004031D File Offset: 0x0003E51D
	public void SetStartWithDownSpikeBounce()
	{
		this.startWithDownSpikeBounce = true;
		this.attack_cooldown = 0.15f;
	}

	// Token: 0x06000DB0 RID: 3504 RVA: 0x00040331 File Offset: 0x0003E531
	public void SetStartWithDownSpikeBounceSlightlyShort()
	{
		this.startWithDownSpikeBounceSlightlyShort = true;
		this.attack_cooldown = 0.15f;
		this.dashCooldownTimer = 0.1f;
	}

	// Token: 0x06000DB1 RID: 3505 RVA: 0x00040350 File Offset: 0x0003E550
	public void SetStartWithDownSpikeBounceShort()
	{
		this.startWithDownSpikeBounceShort = true;
	}

	// Token: 0x06000DB2 RID: 3506 RVA: 0x00040359 File Offset: 0x0003E559
	public void ResetAnimationDownspikeBounce()
	{
		this.animCtrl.ResetDownspikeBounce();
	}

	// Token: 0x06000DB3 RID: 3507 RVA: 0x00040366 File Offset: 0x0003E566
	public void SetStartWithDownSpikeEnd()
	{
		this.startWithDownSpikeEnd = true;
	}

	// Token: 0x06000DB4 RID: 3508 RVA: 0x0004036F File Offset: 0x0003E56F
	public void SetStartWithBalloonBounce()
	{
		this.startWithBalloonBounce = true;
	}

	// Token: 0x06000DB5 RID: 3509 RVA: 0x00040378 File Offset: 0x0003E578
	public void SetStartWithHarpoonBounce()
	{
		this.startWithHarpoonBounce = true;
		this.attack_cooldown = 0.15f;
	}

	// Token: 0x06000DB6 RID: 3510 RVA: 0x0004038C File Offset: 0x0003E58C
	public void CancelQueuedBounces()
	{
		this.startWithBalloonBounce = false;
		this.startWithDownSpikeBounce = false;
		this.startWithDownSpikeBounceShort = false;
		this.startWithDownSpikeBounceSlightlyShort = false;
	}

	// Token: 0x06000DB7 RID: 3511 RVA: 0x000403AC File Offset: 0x0003E5AC
	public void SetStartWithWitchSprintBounce()
	{
		this.startWithDownSpikeBounce = true;
		this.attack_cooldown = 0.15f;
		if (this.transform.localScale.x > 0f)
		{
			this.downspike_rebound_xspeed = this.DOWNSPIKE_REBOUND_SPEED;
			return;
		}
		this.downspike_rebound_xspeed = -this.DOWNSPIKE_REBOUND_SPEED;
	}

	// Token: 0x06000DB8 RID: 3512 RVA: 0x000403FC File Offset: 0x0003E5FC
	public void SetStartWithUpdraftExit()
	{
		this.startWithUpdraftExit = true;
	}

	// Token: 0x06000DB9 RID: 3513 RVA: 0x00040405 File Offset: 0x0003E605
	public void SetStartWithScrambleLeap()
	{
		this.startWithScrambleLeap = true;
	}

	// Token: 0x06000DBA RID: 3514 RVA: 0x0004040E File Offset: 0x0003E60E
	public void SetStartWithRecoilBack()
	{
		this.startWithRecoilBack = true;
	}

	// Token: 0x06000DBB RID: 3515 RVA: 0x00040417 File Offset: 0x0003E617
	public void SetStartWithRecoilBackLong()
	{
		this.startWithRecoilBackLong = true;
	}

	// Token: 0x06000DBC RID: 3516 RVA: 0x00040420 File Offset: 0x0003E620
	public void SetStartWithWhipPullRecoil()
	{
		this.startWithWhipPullRecoil = true;
	}

	// Token: 0x06000DBD RID: 3517 RVA: 0x00040429 File Offset: 0x0003E629
	public void SetSuperDashExit()
	{
		this.exitedSuperDashing = true;
	}

	// Token: 0x06000DBE RID: 3518 RVA: 0x00040432 File Offset: 0x0003E632
	public void SetQuakeExit()
	{
		this.exitedQuake = true;
	}

	// Token: 0x06000DBF RID: 3519 RVA: 0x0004043B File Offset: 0x0003E63B
	public void SetTakeNoDamage()
	{
		this.takeNoDamage = true;
	}

	// Token: 0x06000DC0 RID: 3520 RVA: 0x00040444 File Offset: 0x0003E644
	public void EndTakeNoDamage()
	{
		this.takeNoDamage = false;
	}

	// Token: 0x06000DC1 RID: 3521 RVA: 0x0004044D File Offset: 0x0003E64D
	public void SetStartFromMantle()
	{
		this.startFromMantle = true;
	}

	// Token: 0x06000DC2 RID: 3522 RVA: 0x00040456 File Offset: 0x0003E656
	public void SetStartFromReaperUpperslash()
	{
		this.jump_steps = this.JUMP_STEPS;
		this.animCtrl.SetPlayDashUpperRecovery();
	}

	// Token: 0x06000DC3 RID: 3523 RVA: 0x0004046F File Offset: 0x0003E66F
	public void SetHeroParent(Transform newParent)
	{
		if (this.transform.parent == newParent)
		{
			return;
		}
		this.transform.parent = newParent;
		if (newParent == null)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	// Token: 0x06000DC4 RID: 3524 RVA: 0x000404A5 File Offset: 0x0003E6A5
	public void SetBlockFsmMove(bool blocked)
	{
		this.blockFsmMove = blocked;
	}

	// Token: 0x06000DC5 RID: 3525 RVA: 0x000404B0 File Offset: 0x0003E6B0
	public void IsSwimming()
	{
		if (!this.cState.swimming && this.GetTotalFrostSpeed() > Mathf.Epsilon)
		{
			this.tagDamageTaker.AddDamageTagToStack(this.frostWaterDamageTag, 0);
			StatusVignette.AddStatus(StatusVignette.StatusTypes.InFrostWater);
			GameCameras.instance.cameraController.ScreenFlashFrostStart();
		}
		this.cState.downSpikeRecovery = false;
		this.cState.swimming = true;
		this.dashingDown = false;
		this.CancelFallEffects();
	}

	// Token: 0x06000DC6 RID: 3526 RVA: 0x00040523 File Offset: 0x0003E723
	public void AddFrost(float percentage)
	{
		this.frostAmount += percentage / 100f;
	}

	// Token: 0x06000DC7 RID: 3527 RVA: 0x00040539 File Offset: 0x0003E739
	public void SetFrostAmount(float amount)
	{
		this.frostAmount = amount;
	}

	// Token: 0x06000DC8 RID: 3528 RVA: 0x00040544 File Offset: 0x0003E744
	public void NotSwimming()
	{
		if (!this.cState.swimming)
		{
			return;
		}
		this.cState.swimming = false;
		StatusVignette.RemoveStatus(StatusVignette.StatusTypes.InFrostWater);
		TimerGroup damageCooldownTimer = this.frostWaterDamageTag.DamageCooldownTimer;
		bool flag = damageCooldownTimer && !damageCooldownTimer.HasEnded;
		this.tagDamageTaker.RemoveDamageTagFromStack(this.frostWaterDamageTag, !flag);
	}

	// Token: 0x06000DC9 RID: 3529 RVA: 0x000405A7 File Offset: 0x0003E7A7
	public bool GetAirdashed()
	{
		return this.airDashed;
	}

	// Token: 0x06000DCA RID: 3530 RVA: 0x000405AF File Offset: 0x0003E7AF
	public bool GetCanAirDashCancel()
	{
		return this.playerData.hasDash && !this.airDashed;
	}

	// Token: 0x06000DCB RID: 3531 RVA: 0x000405C9 File Offset: 0x0003E7C9
	public void EnableRenderer()
	{
		this.renderer.enabled = true;
	}

	// Token: 0x06000DCC RID: 3532 RVA: 0x000405D7 File Offset: 0x0003E7D7
	public void ResetAirMoves()
	{
		if (!this.cState.doubleJumping)
		{
			this.doubleJumped = false;
		}
		this.airDashed = false;
	}

	// Token: 0x06000DCD RID: 3533 RVA: 0x000405F4 File Offset: 0x0003E7F4
	public void SetConveyorSpeed(float speed)
	{
		this.conveyorSpeed = speed;
	}

	// Token: 0x06000DCE RID: 3534 RVA: 0x000405FD File Offset: 0x0003E7FD
	public void EnterWithoutInput(bool flag)
	{
		this.enterWithoutInput = flag;
	}

	// Token: 0x06000DCF RID: 3535 RVA: 0x00040606 File Offset: 0x0003E806
	public void SetDarkness(int darkness)
	{
	}

	// Token: 0x06000DD0 RID: 3536 RVA: 0x00040608 File Offset: 0x0003E808
	public void CancelHeroJump()
	{
		if (this.cState.jumping || this.cState.doubleJumping)
		{
			this.CancelJump();
			this.CancelDoubleJump();
			if (this.rb2d.linearVelocity.y > 0f)
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, 0f);
			}
		}
	}

	// Token: 0x06000DD1 RID: 3537 RVA: 0x00040677 File Offset: 0x0003E877
	public void StartHarpoonDashCooldown()
	{
		this.harpoonDashCooldown = 0.16f;
	}

	// Token: 0x06000DD2 RID: 3538 RVA: 0x00040684 File Offset: 0x0003E884
	public void StartHarpoonDashCooldownShort()
	{
		this.harpoonDashCooldown = 0.1f;
	}

	// Token: 0x06000DD3 RID: 3539 RVA: 0x00040691 File Offset: 0x0003E891
	public void CharmUpdate()
	{
		this.playerData.maxHealth = this.playerData.maxHealthBase;
		this.playerData.MaxHealth();
		this.UpdateBlueHealth();
	}

	// Token: 0x06000DD4 RID: 3540 RVA: 0x000406BA File Offset: 0x0003E8BA
	public void UpdateBlueHealth()
	{
		this.playerData.healthBlue = 0;
	}

	// Token: 0x06000DD5 RID: 3541 RVA: 0x000406C8 File Offset: 0x0003E8C8
	public void HitMaxBlueHealth()
	{
		if (!this.IsInLifebloodState)
		{
			this.IsInLifebloodState = true;
			if (this.queuedLifeBloodState && this.gm.QueuedBlueHealth == 0)
			{
				this.EnterLifebloodStateQuick();
			}
		}
	}

	// Token: 0x06000DD6 RID: 3542 RVA: 0x000406F4 File Offset: 0x0003E8F4
	public void HitMaxBlueHealthBurst()
	{
		EventRegister.SendEvent(EventRegisterEvents.CharmIndicatorCheck, null);
		this.CharmUpdate();
		EventRegister.SendEvent(EventRegisterEvents.HealthUpdate, null);
	}

	// Token: 0x06000DD7 RID: 3543 RVA: 0x00040714 File Offset: 0x0003E914
	public float GetMaxFallVelocity()
	{
		float result = this.MAX_FALL_VELOCITY;
		if (Gameplay.WeightedAnkletTool.Status.IsEquipped)
		{
			result = this.MAX_FALL_VELOCITY_WEIGHTED;
		}
		return result;
	}

	// Token: 0x06000DD8 RID: 3544 RVA: 0x00040741 File Offset: 0x0003E941
	public void ResetLifebloodState()
	{
		if (!this.IsInLifebloodState)
		{
			return;
		}
		this.IsInLifebloodState = false;
		EventRegister.SendEvent(EventRegisterEvents.CharmIndicatorCheck, null);
	}

	// Token: 0x06000DD9 RID: 3545 RVA: 0x00040760 File Offset: 0x0003E960
	public void checkEnvironment()
	{
		if (this.enviroRegionListener)
		{
			this.enviroRegionListener.Refresh(true);
		}
		EnvironmentTypes environmentType = this.playerData.environmentType;
		try
		{
			this.audioCtrl.SetFootstepsTable(this.footStepTables[(int)environmentType]);
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	// Token: 0x06000DDA RID: 3546 RVA: 0x000407BC File Offset: 0x0003E9BC
	public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
	{
		this.playerData.SetBenchRespawn(spawnMarker, sceneName, spawnType, facingRight);
	}

	// Token: 0x06000DDB RID: 3547 RVA: 0x000407CE File Offset: 0x0003E9CE
	public void SetBenchRespawn(RespawnMarker spawnMarker, string sceneName, int spawnType)
	{
		this.playerData.SetBenchRespawn(spawnMarker, sceneName, spawnType);
	}

	// Token: 0x06000DDC RID: 3548 RVA: 0x000407DE File Offset: 0x0003E9DE
	public void SetHazardRespawn(Vector3 position, bool facingRight)
	{
		this.playerData.SetHazardRespawn(position, facingRight);
	}

	// Token: 0x06000DDD RID: 3549 RVA: 0x000407ED File Offset: 0x0003E9ED
	public void AddGeo(int amount)
	{
		CurrencyManager.AddGeo(amount);
	}

	// Token: 0x06000DDE RID: 3550 RVA: 0x000407F5 File Offset: 0x0003E9F5
	public void ToZero()
	{
		CurrencyManager.ToZero();
	}

	// Token: 0x06000DDF RID: 3551 RVA: 0x000407FC File Offset: 0x0003E9FC
	public void AddGeoQuietly(int amount)
	{
		CurrencyManager.AddGeoQuietly(amount);
	}

	// Token: 0x06000DE0 RID: 3552 RVA: 0x00040804 File Offset: 0x0003EA04
	public void AddGeoToCounter(int amount)
	{
		CurrencyManager.AddGeoToCounter(amount);
	}

	// Token: 0x06000DE1 RID: 3553 RVA: 0x0004080C File Offset: 0x0003EA0C
	public void TakeGeo(int amount)
	{
		CurrencyManager.TakeGeo(amount);
	}

	// Token: 0x06000DE2 RID: 3554 RVA: 0x00040814 File Offset: 0x0003EA14
	public void AddShards(int amount)
	{
		CurrencyManager.AddShards(amount);
	}

	// Token: 0x06000DE3 RID: 3555 RVA: 0x0004081C File Offset: 0x0003EA1C
	public void TakeShards(int amount)
	{
		CurrencyManager.TakeShards(amount);
	}

	// Token: 0x06000DE4 RID: 3556 RVA: 0x00040824 File Offset: 0x0003EA24
	public void AddCurrency(int amount, CurrencyType type, bool showCounter = true)
	{
		CurrencyManager.ChangeCurrency(amount, type, showCounter);
	}

	// Token: 0x06000DE5 RID: 3557 RVA: 0x0004082E File Offset: 0x0003EA2E
	public void TakeCurrency(int amount, CurrencyType type, bool showCounter = true)
	{
		CurrencyManager.ChangeCurrency(-amount, type, showCounter);
	}

	// Token: 0x06000DE6 RID: 3558 RVA: 0x00040839 File Offset: 0x0003EA39
	public int GetCurrencyAmount(CurrencyType type)
	{
		return CurrencyManager.GetCurrencyAmount(type);
	}

	// Token: 0x06000DE7 RID: 3559 RVA: 0x00040841 File Offset: 0x0003EA41
	public void TempStoreCurrency()
	{
		CurrencyManager.TempStoreCurrency();
	}

	// Token: 0x06000DE8 RID: 3560 RVA: 0x00040848 File Offset: 0x0003EA48
	public void RestoreTempStoredCurrency()
	{
		CurrencyManager.RestoreTempStoredCurrency();
	}

	// Token: 0x06000DE9 RID: 3561 RVA: 0x0004084F File Offset: 0x0003EA4F
	public void UpdateGeo()
	{
	}

	// Token: 0x06000DEA RID: 3562 RVA: 0x00040854 File Offset: 0x0003EA54
	public bool CanInput()
	{
		return this.acceptingInput && !this.IsPaused() && this.transitionState == HeroTransitionState.WAITING_TO_TRANSITION && !this.gm.RespawningHero && !this.cState.hazardRespawning && !this.cState.hazardDeath;
	}

	// Token: 0x06000DEB RID: 3563 RVA: 0x000408A3 File Offset: 0x0003EAA3
	public bool IsPaused()
	{
		return this.gm.isPaused || this.playerData.isInventoryOpen;
	}

	// Token: 0x06000DEC RID: 3564 RVA: 0x000408C0 File Offset: 0x0003EAC0
	public bool IsHunterCrestEquipped()
	{
		return this.playerData.CurrentCrestID == "Hunter" || this.playerData.CurrentCrestID == "Hunter_v2" || this.playerData.CurrentCrestID == "Hunter_v3";
	}

	// Token: 0x06000DED RID: 3565 RVA: 0x00040912 File Offset: 0x0003EB12
	public bool IsArchitectCrestEquipped()
	{
		return this.playerData.CurrentCrestID == "Toolmaster";
	}

	// Token: 0x06000DEE RID: 3566 RVA: 0x00040929 File Offset: 0x0003EB29
	public bool IsShamanCrestEquipped()
	{
		return this.playerData.CurrentCrestID == "Spell";
	}

	// Token: 0x06000DEF RID: 3567 RVA: 0x00040940 File Offset: 0x0003EB40
	public void FlipSprite()
	{
		this.cState.facingRight = !this.cState.facingRight;
		Vector3 localScale = this.transform.localScale;
		localScale.x *= -1f;
		this.transform.localScale = localScale;
		for (int i = this.extraAirMoveVelocities.Count - 1; i >= 0; i--)
		{
			if (this.extraAirMoveVelocities[i].CancelOnTurn)
			{
				this.extraAirMoveVelocities.RemoveAt(i);
			}
		}
		this.ChangedFacing();
	}

	// Token: 0x06000DF0 RID: 3568 RVA: 0x000409CB File Offset: 0x0003EBCB
	public void RefreshFacing()
	{
		this.cState.facingRight = (this.transform.lossyScale.x < 0f);
	}

	// Token: 0x06000DF1 RID: 3569 RVA: 0x000409EF File Offset: 0x0003EBEF
	public void RefreshScale()
	{
		if (this.cState.facingRight)
		{
			this.FaceRight();
			return;
		}
		this.FaceLeft();
	}

	// Token: 0x06000DF2 RID: 3570 RVA: 0x00040A0B File Offset: 0x0003EC0B
	public void NeedleArtRecovery()
	{
		this.attack_cooldown = 0.35f;
	}

	// Token: 0x06000DF3 RID: 3571 RVA: 0x00040A18 File Offset: 0x0003EC18
	public void CrestAttackRecovery()
	{
		this.attack_cooldown = 0.2f;
	}

	// Token: 0x06000DF4 RID: 3572 RVA: 0x00040A25 File Offset: 0x0003EC25
	public void NailParry()
	{
		this.parryInvulnTimer = this.INVUL_TIME_PARRY;
		this.sprintFSM.SendEvent("NAIL CLASH");
		this.parriedAttack = this.cState.attackCount;
	}

	// Token: 0x06000DF5 RID: 3573 RVA: 0x00040A54 File Offset: 0x0003EC54
	public bool IsParrying()
	{
		return this.IsParryingActive() || this.parryInvulnTimer > 0f;
	}

	// Token: 0x06000DF6 RID: 3574 RVA: 0x00040A6D File Offset: 0x0003EC6D
	public bool IsParryingActive()
	{
		return this.cState.parrying || this.cState.parryAttack;
	}

	// Token: 0x06000DF7 RID: 3575 RVA: 0x00040A8C File Offset: 0x0003EC8C
	public void NailParryRecover()
	{
		if (this.parriedAttack != this.cState.attackCount)
		{
			return;
		}
		this.attackDuration = 0f;
		this.attack_cooldown = 0f;
		this.throwToolCooldown = 0f;
		this.CancelAttackNotDownspikeBounce();
		if (this.cState.onGround)
		{
			this.animCtrl.SetPlayRunToIdle();
		}
	}

	// Token: 0x06000DF8 RID: 3576 RVA: 0x00040AEC File Offset: 0x0003ECEC
	public void QuakeInvuln()
	{
		this.parryInvulnTimer = this.INVUL_TIME_QUAKE;
	}

	// Token: 0x06000DF9 RID: 3577 RVA: 0x00040AFA File Offset: 0x0003ECFA
	public void CrossStitchInvuln()
	{
		this.parryInvulnTimer = this.INVUL_TIME_CROSS_STITCH;
	}

	// Token: 0x06000DFA RID: 3578 RVA: 0x00040B08 File Offset: 0x0003ED08
	public void StartRevengeWindow()
	{
		this.revengeWindowTimer = this.REVENGE_WINDOW_TIME;
	}

	// Token: 0x06000DFB RID: 3579 RVA: 0x00040B16 File Offset: 0x0003ED16
	public void StartWandererDashComboWindow()
	{
		this.wandererDashComboWindowTimer = this.DASHCOMBO_WINDOW_TIME;
	}

	// Token: 0x06000DFC RID: 3580 RVA: 0x00040B24 File Offset: 0x0003ED24
	public void ForceSoftLanding()
	{
		this.animCtrl.playLanding = true;
		this.PlaySoftLandingEffect();
		this.BackOnGround(false);
	}

	// Token: 0x06000DFD RID: 3581 RVA: 0x00040B3F File Offset: 0x0003ED3F
	public void PlaySoftLandingEffect()
	{
		this.softLandingEffectPrefab.Spawn(this.transform.position);
	}

	// Token: 0x06000DFE RID: 3582 RVA: 0x00040B58 File Offset: 0x0003ED58
	public void TakeQuickDamage(int damageAmount, bool playEffects)
	{
		this.TakeQuickDamage(damageAmount, playEffects, false);
	}

	// Token: 0x06000DFF RID: 3583 RVA: 0x00040B64 File Offset: 0x0003ED64
	public void TakeQuickDamage(int damageAmount, bool playEffects, bool allowFracturedMaskBreak)
	{
		if (Time.timeAsDouble - this.lastHazardRespawnTime <= 0.5)
		{
			this.lastHazardRespawnTime = Time.timeAsDouble + 1.0;
			Debug.LogError("<b>Possible hazard death loop</b>, canceling quick damage.", this);
			return;
		}
		this.DoSpecialDamage(damageAmount, playEffects, "DAMAGE", false, allowFracturedMaskBreak, false, false);
	}

	// Token: 0x06000E00 RID: 3584 RVA: 0x00040BBA File Offset: 0x0003EDBA
	public void TakeQuickDamageSimple(int damageAmount)
	{
		this.TakeQuickDamage(damageAmount, true);
	}

	// Token: 0x06000E01 RID: 3585 RVA: 0x00040BC4 File Offset: 0x0003EDC4
	public void TakeFrostDamage(int damageAmount)
	{
		this.DoSpecialDamage(damageAmount, true, "FROST DAMAGE", true, true, true, true);
		GameCameras.instance.cameraController.ScreenFlashFrostDamage();
		TimerGroup damageCooldownTimer = this.frostWaterDamageTag.DamageCooldownTimer;
		if (damageCooldownTimer)
		{
			damageCooldownTimer.ResetTimer();
		}
	}

	// Token: 0x06000E02 RID: 3586 RVA: 0x00040C0B File Offset: 0x0003EE0B
	public void TakeChompDamage()
	{
		this.DoSpecialDamage(1, true, "DAMAGE", true, true, true, false);
	}

	// Token: 0x06000E03 RID: 3587 RVA: 0x00040C1E File Offset: 0x0003EE1E
	public bool ApplyTagDamage(DamageTag.DamageTagInstance damageTagInstance)
	{
		if (damageTagInstance.specialDamageType == DamageTag.SpecialDamageTypes.Frost)
		{
			this.TakeFrostDamage(damageTagInstance.amount);
		}
		else
		{
			this.DoSpecialDamage(damageTagInstance.amount, true, "DAMAGE", true, true, true, false);
		}
		return true;
	}

	// Token: 0x06000E04 RID: 3588 RVA: 0x00040C50 File Offset: 0x0003EE50
	private void DoSpecialDamage(int damageAmount, bool playEffects, string damageEvent, bool canDie, bool allowFracturedMaskBreak, bool justTakeHealth, bool isFrostDamage)
	{
		object obj = !this.takeNoDamage && CheatManager.Invincibility == CheatManager.InvincibilityStates.Off && ToolItemManager.ActiveState != ToolsActiveStates.Cutscene && !this.cState.transitioning;
		this.DoMossToolHit();
		object obj2 = obj;
		if (obj2 != null)
		{
			if (!isFrostDamage)
			{
				ToolItem barbedWireTool = Gameplay.BarbedWireTool;
				if (barbedWireTool && barbedWireTool.IsEquipped)
				{
					damageAmount = Mathf.FloorToInt((float)damageAmount * Gameplay.BarbedWireDamageTakenMultiplier);
				}
			}
			this.playerData.TakeHealth(damageAmount, this.IsInLifebloodState, allowFracturedMaskBreak);
			if (justTakeHealth)
			{
				EventRegister.SendEvent(EventRegisterEvents.HealthUpdate, null);
			}
			else
			{
				this.HeroDamaged();
			}
			this.ResetHunterUpgCrestState();
			DeliveryQuestItem.TakeHit(damageAmount);
		}
		if (playEffects)
		{
			this.damageEffectFSM.SendEvent(damageEvent);
			this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT, true);
			if (isFrostDamage)
			{
				this.woundFrostAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
			}
		}
		EventRegister.SendEvent(EventRegisterEvents.HeroDamagedExtra, null);
		if ((obj2 & canDie) != null)
		{
			if (this.playerData.health == 0)
			{
				this.deathAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
				base.StartCoroutine(this.Die(false, isFrostDamage));
				return;
			}
			this.DoBindReminder();
		}
		EventRegister.SendEvent(EventRegisterEvents.HealthUpdate, null);
	}

	// Token: 0x06000E05 RID: 3589 RVA: 0x00040D7D File Offset: 0x0003EF7D
	public void CriticalDamage()
	{
		if (this.takeNoDamage || CheatManager.Invincibility != CheatManager.InvincibilityStates.Off || ToolItemManager.ActiveState == ToolsActiveStates.Cutscene)
		{
			return;
		}
		DeliveryQuestItem.BreakAll();
		this.playerData.healthBlue = 0;
		this.TakeHealth(this.playerData.health - 1);
	}

	// Token: 0x06000E06 RID: 3590 RVA: 0x00040DBC File Offset: 0x0003EFBC
	public void CheckParry(DamageHero damageHero)
	{
		if (damageHero == null)
		{
			return;
		}
		HazardType hazardType = damageHero.hazardType;
		if (hazardType == HazardType.ENEMY || hazardType == HazardType.EXPLOSION)
		{
			if (this.damageMode == DamageMode.HAZARD_ONLY)
			{
				return;
			}
			if (this.cState.shadowDashing)
			{
				return;
			}
			GameObject gameObject = damageHero.gameObject;
			if (this.cState.evading && gameObject.layer != 11)
			{
				if (!this.evadingDidClash)
				{
					this.evadingDidClash = true;
					GameObject scuttleEvadeEffect = Gameplay.ScuttleEvadeEffect;
					scuttleEvadeEffect.Spawn(this.transform.position, scuttleEvadeEffect.transform.localRotation).transform.localScale = scuttleEvadeEffect.transform.localScale.MultiplyElements(this.transform.localScale);
				}
				return;
			}
			if (this.cState.whipLashing && gameObject.layer == 11)
			{
				return;
			}
			if (this.cState.downspikeInvulnerabilitySteps > 0 && hazardType == HazardType.ENEMY && (!damageHero || !damageHero.noBounceCooldown))
			{
				return;
			}
			if (this.parryInvulnTimer > 0f && hazardType == HazardType.ENEMY)
			{
				return;
			}
			if (this.cState.parrying && hazardType == HazardType.ENEMY)
			{
				if (gameObject.transform.position.x > this.transform.position.x)
				{
					this.FaceRight();
				}
				else
				{
					this.FaceLeft();
				}
				this.silkSpecialFSM.SendEvent("PARRIED");
				this.cState.parrying = false;
				this.cState.parryAttack = true;
				return;
			}
		}
	}

	// Token: 0x06000E07 RID: 3591 RVA: 0x00040F2C File Offset: 0x0003F12C
	public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
	{
		if ((damagePropertyFlags & DamagePropertyFlags.Self) != DamagePropertyFlags.None && InteractManager.BlockingInteractable != null)
		{
			return;
		}
		if (damageAmount > 0)
		{
			switch (hazardType)
			{
			case HazardType.SPIKES:
			case HazardType.ACID:
				damageAmount = 1;
				goto IL_83;
			case HazardType.LAVA:
				damageAmount = 2;
				goto IL_83;
			case HazardType.COAL:
			case HazardType.COAL_SPIKES:
				damageAmount = 1;
				goto IL_83;
			case HazardType.ZAP:
				damageAmount = 1;
				goto IL_83;
			case HazardType.STEAM:
				damageAmount = 2;
				goto IL_83;
			case HazardType.RESPAWN_PIT:
				damageAmount = 0;
				goto IL_83;
			}
			if ((damagePropertyFlags & DamagePropertyFlags.Flame) != DamagePropertyFlags.None)
			{
				damageAmount = 2;
			}
			if ((damagePropertyFlags & DamagePropertyFlags.Void) != DamagePropertyFlags.None)
			{
				damageAmount = 2;
			}
			IL_83:
			if (BossSceneController.IsBossScene)
			{
				int bossLevel = BossSceneController.Instance.BossLevel;
				if (bossLevel != 1)
				{
					if (bossLevel == 2)
					{
						damageAmount = 9999;
					}
				}
				else
				{
					damageAmount *= 2;
				}
			}
			if (!this.cState.hazardDeath && hazardType != HazardType.ENEMY && hazardType != HazardType.NON_HAZARD && hazardType != HazardType.EXPLOSION && Time.timeAsDouble - this.lastHazardRespawnTime <= 0.5)
			{
				if (!string.IsNullOrEmpty(this.gm.entryGateName))
				{
					foreach (TransitionPoint transitionPoint in TransitionPoint.TransitionPoints)
					{
						if (!(transitionPoint.name != this.gm.entryGateName))
						{
							this.playerData.SetHazardRespawn(transitionPoint.respawnMarker);
							break;
						}
					}
					using (List<RespawnMarker>.Enumerator enumerator2 = RespawnMarker.Markers.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							RespawnMarker respawnMarker = enumerator2.Current;
							if (!(respawnMarker.name != this.gm.entryGateName))
							{
								this.playerData.SetHazardRespawn(respawnMarker.transform.position, respawnMarker.respawnFacingRight);
								break;
							}
						}
						goto IL_1FD;
					}
				}
				List<TransitionPoint> transitionPoints = TransitionPoint.TransitionPoints;
				if (transitionPoints.Count > 0)
				{
					TransitionPoint transitionPoint2 = transitionPoints[Random.Range(0, transitionPoints.Count)];
					this.playerData.SetHazardRespawn(transitionPoint2.respawnMarker);
				}
				IL_1FD:
				this.doingHazardRespawn = true;
				this.SetState(ActorStates.no_input);
				this.gm.HazardRespawn();
				return;
			}
			if (this.CanTakeDamage() || ((this.cState.Invulnerable || this.parryInvulnTimer > 0f) && !this.cState.hazardDeath && !this.playerData.isInvincible && CheatManager.Invincibility != CheatManager.InvincibilityStates.FullInvincible && hazardType != HazardType.ENEMY && hazardType != HazardType.NON_HAZARD && hazardType != HazardType.EXPLOSION && !HeroInvincibilitySource.IsActive) || hazardType == HazardType.SINK)
			{
				DamageHero damageHero = go ? go.GetComponent<DamageHero>() : null;
				RandomAudioClipTable randomAudioClipTable = this.woundAudioTable;
				if (hazardType == HazardType.ENEMY || hazardType == HazardType.EXPLOSION)
				{
					if (this.damageMode == DamageMode.HAZARD_ONLY)
					{
						return;
					}
					if (this.cState.shadowDashing)
					{
						return;
					}
					if (this.cState.evading && go.layer != 11)
					{
						if (!this.evadingDidClash)
						{
							this.evadingDidClash = true;
							GameObject scuttleEvadeEffect = Gameplay.ScuttleEvadeEffect;
							scuttleEvadeEffect.Spawn(this.transform.position, scuttleEvadeEffect.transform.localRotation).transform.localScale = scuttleEvadeEffect.transform.localScale.MultiplyElements(this.transform.localScale);
						}
						return;
					}
					if (this.cState.whipLashing && go.layer == 11)
					{
						return;
					}
					if (this.cState.downspikeInvulnerabilitySteps > 0 && hazardType == HazardType.ENEMY && (!damageHero || !damageHero.noBounceCooldown))
					{
						return;
					}
					if (this.parryInvulnTimer > 0f && hazardType == HazardType.ENEMY)
					{
						return;
					}
					if (this.cState.parrying && hazardType == HazardType.ENEMY)
					{
						if (go.transform.position.x > this.transform.position.x)
						{
							this.FaceRight();
						}
						else
						{
							this.FaceLeft();
						}
						this.silkSpecialFSM.SendEvent("PARRIED");
						this.cState.parrying = false;
						this.cState.parryAttack = true;
						return;
					}
					if (this.cState.parryAttack)
					{
						return;
					}
					if (this.WillDoBellBindHit())
					{
						this.bellBindFSM.SendEvent("HIT");
						damageAmount = 0;
						randomAudioClipTable = null;
					}
				}
				ToolItem barbedWireTool = Gameplay.BarbedWireTool;
				if (barbedWireTool && barbedWireTool.IsEquipped)
				{
					damageAmount = Mathf.FloorToInt((float)damageAmount * Gameplay.BarbedWireDamageTakenMultiplier);
				}
				if ((hazardType == HazardType.SPIKES || hazardType == HazardType.COAL_SPIKES) && this.cState.downTravelling && go.GetComponent<TinkEffect>())
				{
					return;
				}
				if (Gameplay.LuckyDiceTool.IsEquipped && hazardType == HazardType.ENEMY)
				{
					int luckyDiceShieldThreshold = HeroController.GetLuckyDiceShieldThreshold(this.luckyDiceShieldedHits);
					bool flag = Random.Range(1, 100) <= luckyDiceShieldThreshold;
					if (CheatManager.SuperLuckyDice)
					{
						flag = true;
					}
					if (flag)
					{
						this.luckyDiceShieldedHits = 0;
						this.spawnedLuckyDiceShieldEffect.SetActive(false);
						this.spawnedLuckyDiceShieldEffect.SetActive(true);
						damageAmount = 0;
					}
					else
					{
						this.luckyDiceShieldedHits++;
					}
				}
				else
				{
					this.luckyDiceShieldedHits = 0;
				}
				VibrationMixer mixer = VibrationManager.GetMixer();
				if (mixer != null)
				{
					mixer.StopAllEmissionsWithTag("heroAction");
				}
				this.SetAllowNailChargingWhileRelinquished(false);
				bool flag2 = false;
				if (damageAmount > 0)
				{
					this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT, true);
					this.tagDamageTaker.RemoveDamageTagFromStack(this.acidDamageTag, false);
					if ((damagePropertyFlags & DamagePropertyFlags.Acid) != DamagePropertyFlags.None)
					{
						this.tagDamageTaker.AddDamageTagToStack(this.acidDamageTag, 0);
					}
					bool flag3 = hazardType == HazardType.LAVA || (damagePropertyFlags & DamagePropertyFlags.Flame) > DamagePropertyFlags.None;
					if (flag3)
					{
						if (damageAmount > 1)
						{
							if (this.IsLavaBellActive())
							{
								flag2 = true;
								damageAmount /= 2;
								this.UseLavaBell();
								StatusVignette.AddTempStatus(StatusVignette.TempStatusTypes.FlameDamageLavaBell);
							}
							else
							{
								GameObject gameObject = this.afterDamageEffectsPrefab.Spawn(this.transform.position);
								if (gameObject)
								{
									gameObject.SetActiveChildren(false);
									Transform transform = gameObject.transform.Find("Flame");
									if (transform)
									{
										transform.gameObject.SetActive(true);
									}
								}
								StatusVignette.AddTempStatus(StatusVignette.TempStatusTypes.FlameDamage);
							}
						}
						else
						{
							StatusVignette.AddTempStatus(StatusVignette.TempStatusTypes.FlameDamage);
						}
					}
					if ((damagePropertyFlags & DamagePropertyFlags.SilkAcid) == DamagePropertyFlags.SilkAcid && this.spawnedSilkAcid)
					{
						this.spawnedSilkAcid.SetActive(false);
						this.spawnedSilkAcid.SetActive(true);
					}
					if ((damagePropertyFlags & DamagePropertyFlags.Void) != DamagePropertyFlags.None)
					{
						this.ActivateVoidAcid();
					}
					if (!this.takeNoDamage && (CheatManager.Invincibility == CheatManager.InvincibilityStates.Off || CheatManager.Invincibility == CheatManager.InvincibilityStates.PreventDeath) && !this.gm.sm.HeroKeepHealth && ToolItemManager.ActiveState != ToolsActiveStates.Cutscene)
					{
						this.playerData.TakeHealth(damageAmount, this.IsInLifebloodState, (damagePropertyFlags & DamagePropertyFlags.MidChain) == DamagePropertyFlags.None);
					}
					this.SendHeroDamagedEvent(new HeroController.DamageInfo(hazardType));
					this.DoMossToolHit();
					this.ResetHunterUpgCrestState();
					if (this.warriorState.IsInRageMode)
					{
						this.warriorState.RageEffectTimeLeft = this.warriorState.RageEffectTimeLeft - Gameplay.WarriorRageDamagedRemoveTime;
						this.warriorState.RageHealTimeLeft = this.warriorState.RageHealTimeLeft - Gameplay.WarriorRageDamagedRemoveTime;
					}
					if (this.reaperState.IsInReaperMode)
					{
						this.ResetReaperCrestState();
					}
					DeliveryQuestItem.TakeHit(damageAmount);
					if (damageAmount <= 1)
					{
						this.takeHitSingleEffectPrefab.Spawn(this.transform.position);
					}
					else
					{
						this.takeHitDoubleEffectPrefab.Spawn(this.transform.position);
						randomAudioClipTable = this.woundHeavyAudioTable;
						if ((damagePropertyFlags & DamagePropertyFlags.Void) != DamagePropertyFlags.None)
						{
							this.takeHitDoubleBlackThreadEffectPrefab.Spawn(this.transform.position);
						}
						else if (flag3)
						{
							this.takeHitDoubleFlameEffectPrefab.Spawn(this.transform.position);
						}
					}
					if (Gameplay.ThiefCharmTool.IsEquipped)
					{
						float randomValue = Gameplay.ThiefCharmGeoLoss.GetRandomValue();
						int num = Mathf.CeilToInt((float)this.playerData.geo * randomValue);
						int num2 = Gameplay.ThiefCharmGeoLossCap.GetRandomValue(true);
						if (damageAmount > 1)
						{
							num *= 2;
							num2 = Mathf.FloorToInt((float)num2 * 1.5f);
						}
						if (num > num2)
						{
							num = num2;
						}
						if (Random.Range(0f, 1f) <= Gameplay.ThiefCharmGeoLossLooseChance)
						{
							int randomValue2 = Gameplay.ThiefCharmGeoLossLooseAmount.GetRandomValue(true);
							num += randomValue2;
							FlingUtils.SpawnAndFling(new FlingUtils.Config
							{
								Prefab = Gameplay.SmallGeoPrefab,
								AmountMin = randomValue2,
								AmountMax = randomValue2,
								SpeedMin = 10f,
								SpeedMax = 40f,
								AngleMin = 65f,
								AngleMax = 115f
							}, this.transform, Vector3.zero, null, -1f);
						}
						this.TakeGeo(num);
						GameObject thiefCharmHeroHitPrefab = Gameplay.ThiefCharmHeroHitPrefab;
						if (thiefCharmHeroHitPrefab)
						{
							Vector3 position = this.transform.position;
							position.z = thiefCharmHeroHitPrefab.transform.position.z;
							thiefCharmHeroHitPrefab.Spawn(position);
						}
					}
					if (damageHero)
					{
						damageHero.SendHeroDamagedEvent();
					}
				}
				this.HeroDamaged();
				this.CancelAttack();
				this.RegainControl();
				this.StartAnimationControl();
				if (this.cState.wallSliding)
				{
					this.cState.wallSliding = false;
					this.cState.wallClinging = false;
					this.vibrationCtrl.StopWallSlide();
					this.heroBox.HeroBoxNormal();
				}
				if (this.cState.touchingWall)
				{
					this.cState.touchingWall = false;
				}
				if (this.cState.recoilingLeft || this.cState.recoilingRight)
				{
					this.CancelRecoilHorizontal();
				}
				if (this.cState.bouncing)
				{
					this.CancelBounce();
					this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, 0f);
				}
				if (this.cState.shroomBouncing)
				{
					this.CancelBounce();
					this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, 0f);
				}
				if (this.cState.nailCharging || this.nailChargeTimer != 0f)
				{
					this.CancelNailCharge();
				}
				if (this.playerData.health == 0)
				{
					this.deathAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
					if (this.audioSource && this.deathImpactClip)
					{
						this.audioSource.PlayOneShot(this.deathImpactClip, 1f);
					}
					base.StartCoroutine(this.Die((damagePropertyFlags & DamagePropertyFlags.NonLethal) > DamagePropertyFlags.None, false));
					return;
				}
				this.DoBindReminder();
				if (flag2)
				{
					GameObject prefab = this.lavaBellEffectPrefab;
					Vector3 position2 = this.transform.position;
					float? z = new float?(this.lavaBellEffectPrefab.transform.position.z);
					prefab.Spawn(position2.Where(null, null, z));
				}
				switch (hazardType)
				{
				case HazardType.SPIKES:
					base.StartCoroutine(this.DieFromHazard(HazardType.SPIKES, (go != null) ? go.transform.rotation.z : 0f));
					return;
				case HazardType.ACID:
					base.StartCoroutine(this.DieFromHazard(HazardType.ACID, 0f));
					return;
				case HazardType.LAVA:
					base.StartCoroutine(this.DieFromHazard(HazardType.LAVA, 0f));
					return;
				case HazardType.PIT:
					base.StartCoroutine(this.DieFromHazard(HazardType.PIT, 0f));
					return;
				case HazardType.COAL:
					base.StartCoroutine(this.DieFromHazard(HazardType.COAL, 0f));
					return;
				case HazardType.ZAP:
					base.StartCoroutine(this.DieFromHazard(HazardType.ZAP, 0f));
					return;
				case HazardType.SINK:
					base.StartCoroutine(this.DieFromHazard(HazardType.SINK, 0f));
					return;
				case HazardType.STEAM:
					base.StartCoroutine(this.DieFromHazard(HazardType.STEAM, 0f));
					return;
				case HazardType.RESPAWN_PIT:
					base.StartCoroutine(this.DieFromHazard(HazardType.PIT, 0f));
					return;
				case HazardType.COAL_SPIKES:
					base.StartCoroutine(this.DieFromHazard(HazardType.COAL_SPIKES, (go != null) ? go.transform.rotation.z : 0f));
					return;
				}
				if (randomAudioClipTable)
				{
					randomAudioClipTable.SpawnAndPlayOneShot(this.transform.position, false);
				}
				if (this.vibrationCtrl)
				{
					this.vibrationCtrl.PlayHeroDamage();
				}
				if (!this.cState.recoiling)
				{
					this.recoilRoutine = base.StartCoroutine(this.StartRecoil(damageSide, damageAmount));
					return;
				}
			}
		}
		else
		{
			DamageHero component = go.GetComponent<DamageHero>();
			if (component && component.AlwaysSendDamaged)
			{
				component.SendHeroDamagedEvent();
			}
		}
	}

	// Token: 0x06000E08 RID: 3592 RVA: 0x00041B7C File Offset: 0x0003FD7C
	public void CheckDeathCatch()
	{
		if (this.cState.dead || this.playerData.health > 0)
		{
			return;
		}
		base.StartCoroutine(this.Die(false, false));
	}

	// Token: 0x06000E09 RID: 3593 RVA: 0x00041BA9 File Offset: 0x0003FDA9
	public void ActivateVoidAcid()
	{
		if (!this.spawnedVoidAcid)
		{
			return;
		}
		this.spawnedVoidAcid.SetActive(false);
		this.spawnedVoidAcid.SetActive(true);
	}

	// Token: 0x06000E0A RID: 3594 RVA: 0x00041BD1 File Offset: 0x0003FDD1
	public bool IsLavaBellActive()
	{
		return Gameplay.LavaBellTool.Status.IsEquipped && this.lavaBellCooldownTimeLeft <= 0f;
	}

	// Token: 0x06000E0B RID: 3595 RVA: 0x00041BF6 File Offset: 0x0003FDF6
	public void UseLavaBell()
	{
		this.lavaBellCooldownTimeLeft = Gameplay.LavaBellCooldownTime;
		EventRegister.SendEvent(EventRegisterEvents.LavaBellUsed, null);
	}

	// Token: 0x06000E0C RID: 3596 RVA: 0x00041C0E File Offset: 0x0003FE0E
	private static int GetLuckyDiceShieldThreshold(int hits)
	{
		switch (hits)
		{
		case 0:
			return 0;
		case 1:
			return 2;
		case 2:
			return 4;
		case 3:
			return 6;
		case 4:
			return 8;
		default:
			return 10;
		}
	}

	// Token: 0x06000E0D RID: 3597 RVA: 0x00041C38 File Offset: 0x0003FE38
	public float GetLuckModifier()
	{
		float num = 1f;
		if (Gameplay.LuckyDiceTool.Status.IsEquipped)
		{
			num += 0.1f;
		}
		return num;
	}

	// Token: 0x06000E0E RID: 3598 RVA: 0x00041C65 File Offset: 0x0003FE65
	public void DamageSelf(int amount)
	{
		this.TakeDamage(base.gameObject, this.cState.facingRight ? CollisionSide.right : CollisionSide.left, amount, HazardType.ENEMY, DamagePropertyFlags.None);
	}

	// Token: 0x06000E0F RID: 3599 RVA: 0x00041C87 File Offset: 0x0003FE87
	public bool CanTryHarpoonDash()
	{
		return Time.timeAsDouble > this.HARPOON_DASH_TIME;
	}

	// Token: 0x06000E10 RID: 3600 RVA: 0x00041C98 File Offset: 0x0003FE98
	private void HeroDamaged()
	{
		this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
		this.CancelAttack();
		this.ResetTauntEffects();
		this.CancelFallEffects();
		this.HARPOON_DASH_TIME = Time.timeAsDouble + 0.10000000149011612;
		this.frostDamageTimer = 0f;
		if (this.vibrationCtrl)
		{
			this.vibrationCtrl.PlayHeroDamage();
		}
	}

	// Token: 0x06000E11 RID: 3601 RVA: 0x00041D00 File Offset: 0x0003FF00
	public void SendHeroDamagedEvent()
	{
		this.SendHeroDamagedEvent(default(HeroController.DamageInfo));
	}

	// Token: 0x06000E12 RID: 3602 RVA: 0x00041D1C File Offset: 0x0003FF1C
	public void SendHeroDamagedEvent(HeroController.DamageInfo damageInfo)
	{
		Action onTakenDamage = this.OnTakenDamage;
		if (onTakenDamage != null)
		{
			onTakenDamage();
		}
		HeroController.DamageTakenDelegate onTakenDamageExtra = this.OnTakenDamageExtra;
		if (onTakenDamageExtra == null)
		{
			return;
		}
		onTakenDamageExtra(damageInfo);
	}

	// Token: 0x06000E13 RID: 3603 RVA: 0x00041D40 File Offset: 0x0003FF40
	private void HeroRespawned()
	{
		this.playerData.disableSaveQuit = false;
		this.proxyFSM.SendEvent("HeroCtrl-Respawned");
		this.CancelAttack();
	}

	// Token: 0x06000E14 RID: 3604 RVA: 0x00041D64 File Offset: 0x0003FF64
	private void DoBindReminder()
	{
		if (this.playerData.SeenBindPrompt)
		{
			return;
		}
		if (this.IsRefillSoundsSuppressed)
		{
			return;
		}
		if (this.playerData.health < 3 && this.playerData.silk >= 9)
		{
			EventRegister.SendEvent(EventRegisterEvents.ReminderBind, null);
			this.playerData.SeenBindPrompt = true;
		}
	}

	// Token: 0x06000E15 RID: 3605 RVA: 0x00041DBC File Offset: 0x0003FFBC
	public bool WillDoBellBindHit()
	{
		return this.WillDoBellBindHit(false);
	}

	// Token: 0x06000E16 RID: 3606 RVA: 0x00041DC8 File Offset: 0x0003FFC8
	public bool WillDoBellBindHit(bool sendEvent)
	{
		if (!this.bellBindFSM)
		{
			return false;
		}
		FsmBool fsmBool = this.bellBindFSM.FsmVariables.FindFsmBool("Is Shielding");
		bool flag = fsmBool != null && fsmBool.Value;
		if (flag && sendEvent)
		{
			this.bellBindFSM.SendEvent("HIT");
		}
		return flag;
	}

	// Token: 0x06000E17 RID: 3607 RVA: 0x00041E1C File Offset: 0x0004001C
	private void DoMossToolHit()
	{
		if (this.playerData.health <= 0 || this.playerData.silk >= this.playerData.CurrentSilkMax)
		{
			return;
		}
		if (!this.<DoMossToolHit>g__DoHitForMossTool|890_0(Gameplay.MossCreep1Tool, ref this.mossCreep1Hits, 1) && !this.<DoMossToolHit>g__DoHitForMossTool|890_0(Gameplay.MossCreep2Tool, ref this.mossCreep2Hits, 2))
		{
			return;
		}
		if (this.mossDamageEffectPrefab)
		{
			this.mossDamageEffectPrefab.Spawn(this.transform, Vector3.zero, this.mossDamageEffectPrefab.transform.rotation);
		}
	}

	// Token: 0x06000E18 RID: 3608 RVA: 0x00041EAD File Offset: 0x000400AD
	public string GetEntryGateName()
	{
		if (this.sceneEntryGate != null)
		{
			return this.sceneEntryGate.name;
		}
		return "";
	}

	// Token: 0x06000E19 RID: 3609 RVA: 0x00041ECE File Offset: 0x000400CE
	public float GetShuttlecockTime()
	{
		return this.shuttlecockTime;
	}

	// Token: 0x06000E1A RID: 3610 RVA: 0x00041ED6 File Offset: 0x000400D6
	public void SilkGain()
	{
		this.AddSilk(1, false);
	}

	// Token: 0x06000E1B RID: 3611 RVA: 0x00041EE0 File Offset: 0x000400E0
	public void SilkGain(HitInstance hitInstance)
	{
		switch (hitInstance.SilkGeneration)
		{
		case HitSilkGeneration.Full:
			this.AddSilk(1, false);
			return;
		case HitSilkGeneration.FirstHit:
			if (hitInstance.IsFirstHit)
			{
				this.AddSilk(1, false);
				return;
			}
			break;
		case HitSilkGeneration.None:
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	// Token: 0x06000E1C RID: 3612 RVA: 0x00041F28 File Offset: 0x00040128
	public void NailHitEnemy(HealthManager enemyHealth, HitInstance hitInstance)
	{
		if (this.cState.isMaggoted)
		{
			return;
		}
		if (!hitInstance.RageHit)
		{
			if ((Gameplay.HunterCrest2.IsEquipped || Gameplay.HunterCrest3.IsEquipped) && hitInstance.SilkGeneration != HitSilkGeneration.None && !enemyHealth.DoNotGiveSilk)
			{
				this.hunterUpgState.CurrentMeterHits = this.hunterUpgState.CurrentMeterHits + 1;
			}
			return;
		}
		if (enemyHealth.ShouldIgnore(HealthManager.IgnoreFlags.RageHeal))
		{
			return;
		}
		if (this.warriorState.LastHealAttack == this.cState.attackCount && hitInstance.IsFirstHit)
		{
			return;
		}
		this.warriorState.LastHealAttack = this.cState.attackCount;
		if (this.warriorState.RageModeHealCount >= this.GetRageModeHealCap() || this.playerData.health >= this.playerData.CurrentMaxHealth)
		{
			return;
		}
		int num = this.warriorState.RageModeHealCount;
		this.warriorState.RageModeHealCount = this.warriorState.RageModeHealCount + 1;
		this.AddHealth(1);
		Effects.RageHitHealthEffectPrefab.Spawn(enemyHealth.transform.position);
		this.spriteFlash.flashFocusHeal();
		this.RestartWarriorRageEffect();
		IReadOnlyList<float> warriorRageHitAddTimePerHit = Gameplay.WarriorRageHitAddTimePerHit;
		if (num >= warriorRageHitAddTimePerHit.Count)
		{
			num = warriorRageHitAddTimePerHit.Count - 1;
		}
		float num2 = warriorRageHitAddTimePerHit[num];
		if (num2 <= Mathf.Epsilon)
		{
			return;
		}
		this.warriorState.RageEffectTimeLeft = this.warriorState.RageEffectTimeLeft + num2;
		this.warriorState.RageHealTimeLeft = this.warriorState.RageHealTimeLeft + num2;
	}

	// Token: 0x06000E1D RID: 3613 RVA: 0x00042088 File Offset: 0x00040288
	public int GetRageModeHealCap()
	{
		if (Gameplay.MultibindTool.IsEquipped)
		{
			return 4;
		}
		return 3;
	}

	// Token: 0x06000E1E RID: 3614 RVA: 0x00042099 File Offset: 0x00040299
	public int GetWitchHealCap()
	{
		if (!Gameplay.MultibindTool.IsEquippedHud)
		{
			return 3;
		}
		return 4;
	}

	// Token: 0x06000E1F RID: 3615 RVA: 0x000420AA File Offset: 0x000402AA
	public int GetReaperPayout()
	{
		if (!this.reaperState.IsInReaperMode)
		{
			return 0;
		}
		return Probability.GetRandomItemByProbability<Probability.ProbabilityInt, int>(this.reaperBundleDrops, null);
	}

	// Token: 0x06000E20 RID: 3616 RVA: 0x000420C7 File Offset: 0x000402C7
	private void ResetAllCrestStateMinimal(bool clearHunterState)
	{
		this.ResetWarriorCrestState();
		this.ResetReaperCrestState();
		if (clearHunterState)
		{
			this.ResetHunterUpgCrestState();
		}
		this.ResetWandererCrestState();
	}

	// Token: 0x06000E21 RID: 3617 RVA: 0x000420E4 File Offset: 0x000402E4
	public void ResetAllCrestState()
	{
		this.ResetAllCrestState(!ToolItemManager.IsChangingActiveState);
	}

	// Token: 0x06000E22 RID: 3618 RVA: 0x000420F4 File Offset: 0x000402F4
	public void ResetAllCrestState(bool clearHunterState)
	{
		this.ResetAllCrestStateMinimal(clearHunterState);
		ToolCrest crestByName = ToolItemManager.GetCrestByName(PlayerData.instance.CurrentCrestID);
		if (crestByName != null)
		{
			this.crestConfig = crestByName.HeroConfig;
		}
		this.UpdateConfig();
	}

	// Token: 0x06000E23 RID: 3619 RVA: 0x00042133 File Offset: 0x00040333
	private void ResetWarriorRageEffect()
	{
		bool isInRageMode = this.warriorState.IsInRageMode;
		if (isInRageMode && this.rageModeEffect)
		{
			this.rageModeEffect.Fade();
		}
		if (isInRageMode)
		{
			StatusVignette.RemoveStatus(StatusVignette.StatusTypes.InRageMode);
			EventRegister.SendEvent(EventRegisterEvents.WarriorRageEnded, null);
		}
	}

	// Token: 0x06000E24 RID: 3620 RVA: 0x0004216E File Offset: 0x0004036E
	private void ResetWarriorCrestState()
	{
		this.ResetWarriorRageEffect();
		this.warriorState = default(HeroController.WarriorCrestStateInfo);
	}

	// Token: 0x06000E25 RID: 3621 RVA: 0x00042182 File Offset: 0x00040382
	private void ResetReaperCrestState()
	{
		if (this.reaperState.IsInReaperMode && this.reaperModeEffect)
		{
			this.reaperModeEffect.Fade();
		}
		this.reaperState = default(HeroController.ReaperCrestStateInfo);
	}

	// Token: 0x06000E26 RID: 3622 RVA: 0x000421B5 File Offset: 0x000403B5
	public void SetSilkPartsTimeLeft(float delay)
	{
		this.silkPartsTimeLeft = delay;
	}

	// Token: 0x06000E27 RID: 3623 RVA: 0x000421BE File Offset: 0x000403BE
	private void ResetHunterUpgCrestState()
	{
		this.hunterUpgState = default(HeroController.HunterUpgCrestStateInfo);
	}

	// Token: 0x06000E28 RID: 3624 RVA: 0x000421CC File Offset: 0x000403CC
	private void ResetWandererCrestState()
	{
		this.WandererState = default(HeroController.WandererCrestStateInfo);
	}

	// Token: 0x06000E29 RID: 3625 RVA: 0x000421E8 File Offset: 0x000403E8
	public void AddSilk(int amount, bool heroEffect)
	{
		this.AddSilk(amount, heroEffect, SilkSpool.SilkAddSource.Normal);
	}

	// Token: 0x06000E2A RID: 3626 RVA: 0x000421F3 File Offset: 0x000403F3
	public void ReduceOdours(int amount)
	{
		this.playerData.ReduceOdours(amount);
	}

	// Token: 0x06000E2B RID: 3627 RVA: 0x00042201 File Offset: 0x00040401
	public void AddSilk(int amount, bool heroEffect, SilkSpool.SilkAddSource source)
	{
		this.AddSilk(amount, heroEffect, source, false);
	}

	// Token: 0x06000E2C RID: 3628 RVA: 0x00042210 File Offset: 0x00040410
	public void AddSilk(int amount, bool heroEffect, SilkSpool.SilkAddSource source, bool forceCanBindEffect)
	{
		int silk = this.playerData.silk;
		this.playerData.AddSilk(amount);
		this.ResetSilkRegen();
		if (heroEffect)
		{
			this.spriteFlash.flashFocusHeal();
		}
		int num = Gameplay.WitchCrest.IsEquipped ? 9 : 9;
		if ((forceCanBindEffect || silk < num) && this.playerData.silk >= num && !this.IsRefillSoundsSuppressed && ToolItemManager.ActiveState == ToolsActiveStates.Active && this.renderer.enabled)
		{
			this.canBindEffect.SetActive(false);
			this.canBindEffect.SetActive(true);
		}
		SilkSpool silkSpool = GameCameras.instance.silkSpool;
		silkSpool.RefreshSilk(source, SilkSpool.SilkTakeSource.Normal);
		if (this.playerData.silk == this.playerData.CurrentSilkMax || (this.playerData.silk == this.playerData.CurrentSilkMax - 1 && this.playerData.silkParts > 0))
		{
			this.mossCreep1Hits = 0;
			this.mossCreep2Hits = 0;
			silkSpool.SetMossState(0, 1);
		}
		this.DoBindReminder();
	}

	// Token: 0x06000E2D RID: 3629 RVA: 0x00042318 File Offset: 0x00040518
	public void RefillSilkToMax()
	{
		int num = this.playerData.CurrentSilkMax - this.playerData.silk;
		if (num > 0)
		{
			this.AddSilk(num, false, SilkSpool.SilkAddSource.Normal);
		}
	}

	// Token: 0x06000E2E RID: 3630 RVA: 0x0004234C File Offset: 0x0004054C
	public void RefreshSilk()
	{
		int num = this.playerData.silk - this.playerData.CurrentSilkMax;
		if (num > 0)
		{
			this.TakeSilk(num);
		}
	}

	// Token: 0x06000E2F RID: 3631 RVA: 0x0004237C File Offset: 0x0004057C
	public void AddFinalMaxSilk()
	{
		this.AddSilk(1, false);
	}

	// Token: 0x06000E30 RID: 3632 RVA: 0x00042388 File Offset: 0x00040588
	public void AddSilkParts(int amount)
	{
		if (this.playerData.silk >= this.playerData.CurrentSilkMax)
		{
			return;
		}
		this.playerData.silkParts += amount;
		if (this.playerData.silkParts >= 3)
		{
			this.playerData.silkParts = 0;
			this.AddSilk(1, false);
			return;
		}
		this.ResetSilkRegen();
		GameCameras.instance.silkSpool.RefreshSilk();
	}

	// Token: 0x06000E31 RID: 3633 RVA: 0x000423F9 File Offset: 0x000405F9
	public void AddSilkParts(int amount, bool heroEffect)
	{
		this.AddSilkParts(amount);
		if (heroEffect)
		{
			this.spriteFlash.flashFocusGet();
		}
	}

	// Token: 0x06000E32 RID: 3634 RVA: 0x00042410 File Offset: 0x00040610
	public void TakeSilk(int amount)
	{
		this.TakeSilk(amount, SilkSpool.SilkTakeSource.Normal);
	}

	// Token: 0x06000E33 RID: 3635 RVA: 0x0004241C File Offset: 0x0004061C
	public void TakeSilk(int amount, SilkSpool.SilkTakeSource source)
	{
		if (ToolItemManager.ActiveState == ToolsActiveStates.Cutscene)
		{
			return;
		}
		this.playerData.TakeSilk(amount);
		if (this.playerData.silk < 0)
		{
			this.playerData.silk = 0;
		}
		GameCameras.instance.silkSpool.RefreshSilk(SilkSpool.SilkAddSource.Normal, source);
		this.ResetSilkRegen();
	}

	// Token: 0x06000E34 RID: 3636 RVA: 0x0004246F File Offset: 0x0004066F
	public void ClearSpoolMossChunks()
	{
		this.mossCreep1Hits = 0;
		this.mossCreep2Hits = 0;
		GameCameras.instance.silkSpool.SetMossState(0, 1);
	}

	// Token: 0x06000E35 RID: 3637 RVA: 0x00042490 File Offset: 0x00040690
	public void MaxRegenSilk()
	{
		if (this.playerData.CurrentSilkRegenMax - this.playerData.silk <= 0)
		{
			return;
		}
		this.doMaxSilkRegen = true;
		this.maxSilkRegenTimer = 0f;
	}

	// Token: 0x06000E36 RID: 3638 RVA: 0x000424C0 File Offset: 0x000406C0
	public void MaxRegenSilkInstant()
	{
		int num = this.playerData.CurrentSilkRegenMax - this.playerData.silk;
		if (num <= 0)
		{
			return;
		}
		this.AddSilk(num, false);
	}

	// Token: 0x06000E37 RID: 3639 RVA: 0x000424F4 File Offset: 0x000406F4
	private void StartSilkRegen()
	{
		int currentSilkRegenMax = this.playerData.CurrentSilkRegenMax;
		if (this.playerData.silk < currentSilkRegenMax && this.playerData.silk < this.playerData.CurrentSilkMax)
		{
			float num = this.SILK_REGEN_DURATION;
			float num2 = this.FIRST_SILK_REGEN_DURATION;
			if (Gameplay.WhiteRingTool.IsEquipped)
			{
				num *= Gameplay.WhiteRingSilkRegenTimeMultiplier;
				num2 *= Gameplay.WhiteRingSilkRegenTimeMultiplier;
			}
			this.silkRegenDurationLeft = ((this.playerData.silk > 0 && !this.isNextSilkRegenUpgraded) ? num : num2);
			SilkSpool.Instance.SetRegen(1, this.isNextSilkRegenUpgraded);
			return;
		}
		this.ResetSilkRegen();
	}

	// Token: 0x06000E38 RID: 3640 RVA: 0x00042595 File Offset: 0x00040795
	private void DoSilkRegen()
	{
		this.AddSilk(1, false);
		EventRegister.SendEvent(EventRegisterEvents.RegeneratedSilkChunk, null);
		this.isNextSilkRegenUpgraded = false;
	}

	// Token: 0x06000E39 RID: 3641 RVA: 0x000425B4 File Offset: 0x000407B4
	private void ResetSilkRegen()
	{
		float num = this.SILK_REGEN_DELAY;
		float num2 = this.FIRST_SILK_REGEN_DELAY;
		if (Gameplay.WhiteRingTool.IsEquipped)
		{
			num *= Gameplay.WhiteRingSilkRegenTimeMultiplier;
			num2 *= Gameplay.WhiteRingSilkRegenTimeMultiplier;
		}
		this.silkRegenDelayLeft = ((this.playerData.silk > 0 && !this.isNextSilkRegenUpgraded) ? num : num2);
		this.silkRegenDurationLeft = 0f;
		SilkSpool instance = SilkSpool.Instance;
		if (instance)
		{
			instance.SetRegen(0, this.isNextSilkRegenUpgraded);
		}
	}

	// Token: 0x06000E3A RID: 3642 RVA: 0x00042634 File Offset: 0x00040834
	public void SetSilkRegenBlocked(bool isBlocked)
	{
		bool flag = false;
		if (this.isSilkRegenBlocked || isBlocked)
		{
			this.ResetSilkRegen();
			flag = true;
		}
		this.isSilkRegenBlocked = isBlocked;
		if (!isBlocked && !flag && this.silkRegenDelayLeft == 0f && this.silkRegenDurationLeft == 0f)
		{
			this.StartSilkRegen();
		}
	}

	// Token: 0x06000E3B RID: 3643 RVA: 0x00042682 File Offset: 0x00040882
	public void SetSilkRegenBlockedSilkHeart(bool isBlocked)
	{
		if (!isBlocked)
		{
			this.isNextSilkRegenUpgraded = true;
		}
		this.SetSilkRegenBlocked(isBlocked);
		if (isBlocked)
		{
			this.TakeSilk(999);
			this.AddSilk(this.playerData.CurrentSilkRegenMax - 1, false);
		}
	}

	// Token: 0x06000E3C RID: 3644 RVA: 0x000426B7 File Offset: 0x000408B7
	public void UpdateSilkCursed()
	{
		this.ResetSilkRegen();
		EventRegister.SendEvent(EventRegisterEvents.SilkCursedUpdate, null);
	}

	// Token: 0x06000E3D RID: 3645 RVA: 0x000426CA File Offset: 0x000408CA
	public void AddHealth(int amount)
	{
		this.playerData.AddHealth(amount);
		EventRegister.SendEvent(EventRegisterEvents.HeroHealed, null);
	}

	// Token: 0x06000E3E RID: 3646 RVA: 0x000426E4 File Offset: 0x000408E4
	public void RefillHealthToMax()
	{
		int num = this.playerData.CurrentMaxHealth - this.playerData.health;
		if (num > 0)
		{
			this.AddHealth(num);
		}
	}

	// Token: 0x06000E3F RID: 3647 RVA: 0x00042714 File Offset: 0x00040914
	public void SuppressRefillSound(int frames)
	{
		this.refillSoundSuppressFramesLeft = frames;
	}

	// Token: 0x06000E40 RID: 3648 RVA: 0x0004271D File Offset: 0x0004091D
	public void RefillAll()
	{
		this.SuppressRefillSound(2);
		this.RefillHealthToMax();
		this.RefillSilkToMax();
	}

	// Token: 0x06000E41 RID: 3649 RVA: 0x00042732 File Offset: 0x00040932
	public void RefillSilkToMaxSilent()
	{
		this.SuppressRefillSound(2);
		this.RefillSilkToMax();
	}

	// Token: 0x06000E42 RID: 3650 RVA: 0x00042744 File Offset: 0x00040944
	public void BindCompleted()
	{
		if (Gameplay.WarriorCrest.IsEquipped)
		{
			if (!this.warriorState.IsInRageMode)
			{
				this.AddFrost(-15f);
				StatusVignette.AddStatus(StatusVignette.StatusTypes.InRageMode);
			}
			this.warriorState = default(HeroController.WarriorCrestStateInfo);
			this.warriorState.IsInRageMode = true;
			this.warriorState.RageEffectTimeLeft = Gameplay.WarriorRageDuration;
			this.warriorState.RageHealTimeLeft = Gameplay.WarriorRageDuration + Gameplay.WarriorRageGraceTime;
			this.RestartWarriorRageEffect();
			BrightnessEffect component = this.gm.cameraCtrl.GetComponent<BrightnessEffect>();
			component.ExtraEffectFadeTo(1.15f, 1.15f, 0f, 0f);
			component.ExtraEffectFadeTo(1f, 1f, 2f, 0.3f);
			EventRegister.SendEvent(EventRegisterEvents.WarriorRageStarted, null);
		}
		if (Gameplay.ReaperCrest.IsEquipped)
		{
			this.reaperState.IsInReaperMode = true;
			this.reaperState.ReaperModeDurationLeft = Gameplay.ReaperModeDuration;
			if (this.reaperModeEffect)
			{
				this.reaperModeEffect.gameObject.SetActive(false);
				this.reaperModeEffect.gameObject.SetActive(true);
			}
		}
	}

	// Token: 0x06000E43 RID: 3651 RVA: 0x00042867 File Offset: 0x00040A67
	private void RestartWarriorRageEffect()
	{
		if (this.rageModeEffect)
		{
			this.rageModeEffect.gameObject.SetActive(false);
			this.rageModeEffect.gameObject.SetActive(true);
		}
	}

	// Token: 0x06000E44 RID: 3652 RVA: 0x00042898 File Offset: 0x00040A98
	public void BindInterrupted()
	{
		this.mossCreep1Hits = 0;
		this.mossCreep2Hits = 0;
		GameCameras.instance.silkSpool.SetMossState(0, 1);
		this.playerData.silkParts = 0;
	}

	// Token: 0x06000E45 RID: 3653 RVA: 0x000428C5 File Offset: 0x00040AC5
	public void TakeHealth(int amount)
	{
		this.playerData.TakeHealth(amount, this.IsInLifebloodState, true);
		this.HeroDamaged();
	}

	// Token: 0x06000E46 RID: 3654 RVA: 0x000428E0 File Offset: 0x00040AE0
	public void MaxHealth()
	{
		this.ResetLifebloodState();
		this.proxyFSM.SendEvent("HeroCtrl-MaxHealth");
		this.playerData.MaxHealth();
		this.UpdateBlueHealth();
		EventRegister.SendEvent(EventRegisterEvents.HeroHealedToMax, null);
		this.ResetAllCrestState();
		this.ResetMaggotCharm();
	}

	// Token: 0x06000E47 RID: 3655 RVA: 0x00042920 File Offset: 0x00040B20
	public void MaxHealthKeepBlue()
	{
		int healthBlue = this.playerData.healthBlue;
		this.playerData.MaxHealth();
		this.UpdateBlueHealth();
		this.playerData.healthBlue = healthBlue;
		EventRegister.SendEvent(EventRegisterEvents.HeroHealed, null);
	}

	// Token: 0x06000E48 RID: 3656 RVA: 0x00042964 File Offset: 0x00040B64
	public void AddToMaxHealth(int amount)
	{
		this.playerData.AddToMaxHealth(amount);
		this.gm.QueueAchievement("FIRST_MASK");
		int current = (this.playerData.maxHealthBase - 5) * 4;
		this.gm.QueueAchievementProgress("ALL_MASKS", current, 20);
	}

	// Token: 0x06000E49 RID: 3657 RVA: 0x000429B0 File Offset: 0x00040BB0
	public void AddToMaxSilk(int amount)
	{
		this.playerData.silkMax += amount;
		this.gm.QueueAchievement("FIRST_SILK_SPOOL");
		int current = (this.playerData.silkMax - 9) * 2;
		this.gm.QueueAchievementProgress("ALL_SILK_SPOOLS", current, 18);
	}

	// Token: 0x06000E4A RID: 3658 RVA: 0x00042A04 File Offset: 0x00040C04
	public void AddToMaxSilkRegen(int amount)
	{
		this.playerData.silkRegenMax += amount;
		this.gm.QueueAchievementProgress("ALL_SILK_HEARTS", this.playerData.silkRegenMax, 3);
	}

	// Token: 0x06000E4B RID: 3659 RVA: 0x00042A38 File Offset: 0x00040C38
	public bool IsHealthCritical()
	{
		ToolItem fracturedMaskTool = Gameplay.FracturedMaskTool;
		return (!fracturedMaskTool || !fracturedMaskTool.IsEquipped || fracturedMaskTool.SavedData.AmountLeft <= 0) && this.playerData.health + this.playerData.healthBlue <= this.CriticalHealthValue;
	}

	// Token: 0x06000E4C RID: 3660 RVA: 0x00042A90 File Offset: 0x00040C90
	public void DownspikeBounce(bool harpoonRecoil, HeroSlashBounceConfig bounceConfig = null)
	{
		this.animCtrl.ResetDownspikeBounce();
		this.cState.jumping = true;
		this.cState.bouncing = false;
		this.downspike_rebound_steps = 0;
		this.cState.downSpikeBouncing = true;
		this.allowAttackCancellingDownspikeRecovery = false;
		this.ResetAirMoves();
		this.downspike_rebound_xspeed = 0f;
		if (bounceConfig == null)
		{
			bounceConfig = HeroSlashBounceConfig.Default;
		}
		this.jump_steps = bounceConfig.JumpSteps;
		this.jumped_steps = bounceConfig.JumpedSteps;
		this.jumpQueueSteps = 0;
		this.downspikeBurstPrefab.SetActive(false);
		bool standardRecovery = !harpoonRecoil;
		this.FinishDownspike(standardRecovery);
		this.BecomeAirborne();
	}

	// Token: 0x06000E4D RID: 3661 RVA: 0x00042B38 File Offset: 0x00040D38
	public void DownspikeBounceSlightlyShort(bool harpoonRecoil)
	{
		this.cState.jumping = true;
		this.downspike_rebound_steps = 0;
		this.cState.downSpikeBouncing = true;
		this.cState.downSpikeBouncingShort = true;
		this.allowAttackCancellingDownspikeRecovery = false;
		this.ResetAirMoves();
		if (harpoonRecoil)
		{
			if (this.transform.localScale.x > 0f)
			{
				this.downspike_rebound_xspeed = this.DOWNSPIKE_REBOUND_SPEED;
			}
			else
			{
				this.downspike_rebound_xspeed = -this.DOWNSPIKE_REBOUND_SPEED;
			}
		}
		else
		{
			this.downspike_rebound_xspeed = 0f;
		}
		this.jump_steps = 5;
		this.jumped_steps = -20;
		this.jumpQueueSteps = 0;
		this.downspikeBurstPrefab.SetActive(false);
		this.FinishDownspike();
		Vector2 linearVelocity = this.rb2d.linearVelocity;
		linearVelocity.y = 0f;
		this.rb2d.linearVelocity = linearVelocity;
	}

	// Token: 0x06000E4E RID: 3662 RVA: 0x00042C0C File Offset: 0x00040E0C
	public void DownspikeBounceShort(bool harpoonRecoil)
	{
		this.cState.jumping = true;
		this.downspike_rebound_steps = 0;
		this.cState.downSpikeBouncing = true;
		this.cState.downSpikeBouncingShort = true;
		this.allowAttackCancellingDownspikeRecovery = false;
		this.ResetAirMoves();
		if (harpoonRecoil)
		{
			if (this.transform.localScale.x > 0f)
			{
				this.downspike_rebound_xspeed = this.DOWNSPIKE_REBOUND_SPEED;
			}
			else
			{
				this.downspike_rebound_xspeed = -this.DOWNSPIKE_REBOUND_SPEED;
			}
		}
		else
		{
			this.downspike_rebound_xspeed = 0f;
		}
		this.jump_steps = 7;
		this.jumped_steps = -20;
		this.jumpQueueSteps = 0;
		this.downspikeBurstPrefab.SetActive(false);
		this.FinishDownspike();
	}

	// Token: 0x06000E4F RID: 3663 RVA: 0x00042CBC File Offset: 0x00040EBC
	public bool Bounce()
	{
		if (!this.cState.bouncing && !this.cState.shroomBouncing && !this.controlReqlinquished)
		{
			this.doubleJumped = false;
			this.airDashed = false;
			this.cState.bouncing = true;
			return true;
		}
		return false;
	}

	// Token: 0x06000E50 RID: 3664 RVA: 0x00042D08 File Offset: 0x00040F08
	public void BounceShort()
	{
		if (this.Bounce())
		{
			this.bounceTimer = this.BOUNCE_TIME * 0.5f;
		}
	}

	// Token: 0x06000E51 RID: 3665 RVA: 0x00042D24 File Offset: 0x00040F24
	public void BounceHigh()
	{
		if (!this.cState.bouncing && !this.controlReqlinquished)
		{
			this.doubleJumped = false;
			this.airDashed = false;
			this.cState.bouncing = true;
			this.bounceTimer = -0.03f;
			this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.BOUNCE_VELOCITY);
		}
	}

	// Token: 0x06000E52 RID: 3666 RVA: 0x00042D94 File Offset: 0x00040F94
	public void ShroomBounce()
	{
		this.doubleJumped = false;
		this.airDashed = false;
		this.cState.bouncing = false;
		this.cState.shroomBouncing = true;
		this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.SHROOM_BOUNCE_VELOCITY);
	}

	// Token: 0x06000E53 RID: 3667 RVA: 0x00042DED File Offset: 0x00040FED
	public void RecoilLeft()
	{
		if (this.CanRecoil())
		{
			this.Recoil(false, false);
		}
	}

	// Token: 0x06000E54 RID: 3668 RVA: 0x00042DFF File Offset: 0x00040FFF
	public void RecoilRight()
	{
		if (this.CanRecoil())
		{
			this.Recoil(true, false);
		}
	}

	// Token: 0x06000E55 RID: 3669 RVA: 0x00042E11 File Offset: 0x00041011
	public void RecoilRightLong()
	{
		if (this.CanRecoil())
		{
			this.Recoil(true, true);
		}
	}

	// Token: 0x06000E56 RID: 3670 RVA: 0x00042E23 File Offset: 0x00041023
	public void RecoilLeftLong()
	{
		if (this.CanRecoil())
		{
			this.Recoil(false, true);
		}
	}

	// Token: 0x06000E57 RID: 3671 RVA: 0x00042E38 File Offset: 0x00041038
	private bool CanRecoil()
	{
		return this.CanCustomRecoil() && ((this.cState.recoilingDrill || (!this.cState.recoilingLeft && !this.cState.recoilingRight)) && (!this.controlReqlinquished || this.allowRecoilWhileRelinquished)) && InteractManager.BlockingInteractable == null;
	}

	// Token: 0x06000E58 RID: 3672 RVA: 0x00042E93 File Offset: 0x00041093
	public bool CanCustomRecoil()
	{
		return Time.timeAsDouble >= this.recoilAllowTime;
	}

	// Token: 0x06000E59 RID: 3673 RVA: 0x00042EA8 File Offset: 0x000410A8
	public void PreventRecoil(float duration)
	{
		double num = Time.timeAsDouble + (double)duration;
		if (num > this.recoilAllowTime)
		{
			this.recoilAllowTime = num;
		}
	}

	// Token: 0x06000E5A RID: 3674 RVA: 0x00042ECE File Offset: 0x000410CE
	public void AllowRecoil()
	{
		this.recoilAllowTime = 0.0;
	}

	// Token: 0x06000E5B RID: 3675 RVA: 0x00042EE0 File Offset: 0x000410E0
	private void Recoil(bool isRight, bool isLong)
	{
		int steps = this.RECOIL_HOR_STEPS;
		ToolBase weightedAnkletTool = Gameplay.WeightedAnkletTool;
		ToolBase witchCrest = Gameplay.WitchCrest;
		ToolCrest cursedCrest = Gameplay.CursedCrest;
		bool flag = false;
		if (witchCrest.IsEquipped || cursedCrest.IsEquipped)
		{
			flag = true;
		}
		if (weightedAnkletTool.IsEquipped)
		{
			if (flag)
			{
				steps = 0;
			}
			else
			{
				steps = Gameplay.WeightedAnkletRecoilSteps;
			}
		}
		else if (flag)
		{
			steps = Gameplay.WitchCrestRecoilSteps;
		}
		if (isLong)
		{
			this.Recoil(isRight, steps, this.RECOIL_HOR_VELOCITY_LONG);
			this.ResetAttacks(true);
			return;
		}
		this.Recoil(isRight, steps, this.RECOIL_HOR_VELOCITY);
	}

	// Token: 0x06000E5C RID: 3676 RVA: 0x00042F60 File Offset: 0x00041160
	public void DrillDash(bool isRight)
	{
		float x = this.rb2d.linearVelocity.x;
		if (x < 6f && x > -6f)
		{
			this.Recoil(isRight, this.RECOIL_HOR_STEPS, this.RECOIL_HOR_VELOCITY_DRILLDASH);
		}
		else
		{
			this.Recoil(isRight, this.RECOIL_HOR_STEPS, this.RECOIL_HOR_VELOCITY_DRILLDASH * 0.6f);
		}
		this.cState.recoilingDrill = true;
	}

	// Token: 0x06000E5D RID: 3677 RVA: 0x00042FC8 File Offset: 0x000411C8
	public void DrillPull(bool isRight)
	{
		this.CancelRecoilHorizontal();
		this.Recoil(isRight, this.RECOIL_HOR_STEPS, this.RECOIL_HOR_VELOCITY);
	}

	// Token: 0x06000E5E RID: 3678 RVA: 0x00042FE3 File Offset: 0x000411E3
	public void Recoil(int steps, float velocity)
	{
		this.Recoil(!this.cState.facingRight, steps, velocity);
	}

	// Token: 0x06000E5F RID: 3679 RVA: 0x00042FFC File Offset: 0x000411FC
	public void Recoil(bool isRight, int steps, float velocity)
	{
		if (this.cState.dashing || this.dashingDown)
		{
			this.CancelDash(true);
		}
		this.recoilStepsLeft = steps;
		this.cState.recoilingRight = isRight;
		this.cState.recoilingLeft = !isRight;
		this.cState.recoilingDrill = false;
		this.cState.wallJumping = false;
		this.wallJumpedL = false;
		this.wallJumpedR = false;
		this.recoilVelocity = velocity;
		this.rb2d.linearVelocity = new Vector2(isRight ? velocity : (-velocity), this.rb2d.linearVelocity.y);
	}

	// Token: 0x06000E60 RID: 3680 RVA: 0x0004309C File Offset: 0x0004129C
	public void ChargeSlashRecoilRight()
	{
		if (!this.Config.ChargeSlashRecoils)
		{
			return;
		}
		this.SetAllowRecoilWhileRelinquished(true);
		this.RecoilRight();
	}

	// Token: 0x06000E61 RID: 3681 RVA: 0x000430B9 File Offset: 0x000412B9
	public void ChargeSlashRecoilLeft()
	{
		if (!this.Config.ChargeSlashRecoils)
		{
			return;
		}
		this.SetAllowRecoilWhileRelinquished(true);
		this.RecoilLeft();
	}

	// Token: 0x06000E62 RID: 3682 RVA: 0x000430D8 File Offset: 0x000412D8
	public void RecoilDown()
	{
		this.CancelJump();
		bool flag = Gameplay.WitchCrest.IsEquipped || Gameplay.CursedCrest.IsEquipped || Gameplay.WeightedAnkletTool.IsEquipped;
		if ((!Gameplay.WitchCrest.IsEquipped && !Gameplay.CursedCrest.IsEquipped) || !Gameplay.WeightedAnkletTool.IsEquipped)
		{
			if (flag)
			{
				if (this.rb2d.linearVelocity.y > this.RECOIL_DOWN_VELOCITY && !this.controlReqlinquished)
				{
					this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.rb2d.linearVelocity.y / 2f);
					return;
				}
			}
			else if (this.rb2d.linearVelocity.y > this.RECOIL_DOWN_VELOCITY && !this.controlReqlinquished)
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.RECOIL_DOWN_VELOCITY);
			}
		}
	}

	// Token: 0x06000E63 RID: 3683 RVA: 0x000431DE File Offset: 0x000413DE
	public void ForceHardLanding()
	{
		if (!this.cState.onGround)
		{
			this.cState.willHardLand = true;
		}
	}

	// Token: 0x06000E64 RID: 3684 RVA: 0x000431FC File Offset: 0x000413FC
	public void EnterUpdraft(float speed)
	{
		this.updraftsEntered++;
		this.cState.inUpdraft = true;
		if (this.updraftsEntered == 1)
		{
			this.fsm_brollyControl.FsmVariables.FindFsmFloat("Target Updraft Speed").Value = speed;
			this.fsm_brollyControl.SendEventSafe("UPDRAFT ENTER");
		}
	}

	// Token: 0x06000E65 RID: 3685 RVA: 0x00043257 File Offset: 0x00041457
	public void ExitUpdraft()
	{
		this.updraftsEntered--;
		if (this.updraftsEntered <= 0)
		{
			this.updraftsEntered = 0;
			this.cState.inUpdraft = false;
			this.fsm_brollyControl.SendEventSafe("UPDRAFT EXIT");
		}
	}

	// Token: 0x06000E66 RID: 3686 RVA: 0x00043293 File Offset: 0x00041493
	public void ResetUpdraft()
	{
		this.updraftsEntered = 0;
		this.cState.inUpdraft = false;
		this.fsm_brollyControl.SendEventSafe("UPDRAFT EXIT");
	}

	// Token: 0x06000E67 RID: 3687 RVA: 0x000432B8 File Offset: 0x000414B8
	public void AllowMantle(bool allow)
	{
		this.allowMantle = allow;
	}

	// Token: 0x06000E68 RID: 3688 RVA: 0x000432C4 File Offset: 0x000414C4
	public void EnterSceneDreamGate()
	{
		this.IgnoreInputWithoutReset();
		this.ResetMotion(true);
		this.airDashed = false;
		this.doubleJumped = false;
		this.ResetHardLandingTimer();
		this.ResetAttacksDash();
		this.AffectedByGravity(false);
		this.sceneEntryGate = null;
		this.SetState(ActorStates.no_input);
		this.transitionState = HeroTransitionState.WAITING_TO_ENTER_LEVEL;
		this.vignetteFSM.SendEvent("RESET");
		this.SendHeroInPosition(false);
		this.FinishedEnteringScene(true, false);
	}

	// Token: 0x06000E69 RID: 3689 RVA: 0x00043333 File Offset: 0x00041533
	public IEnumerator EnterScene(TransitionPoint enterGate, float delayBeforeEnter, bool forceCustomFade = false, Action onEnd = null, bool enterSkip = false)
	{
		enterGate.PrepareEntry();
		this.animCtrl.waitingToEnter = true;
		while (GameManager.IsCollectingGarbage)
		{
			yield return null;
		}
		float num = Platform.Current.EnterSceneWait;
		if (CheatManager.SceneEntryWait >= 0f)
		{
			num = CheatManager.SceneEntryWait;
		}
		if (num > 0f)
		{
			yield return new WaitForSecondsRealtime(num);
		}
		this.animCtrl.waitingToEnter = false;
		this.cState.fakeHurt = false;
		EventRegister.SendEvent(EventRegisterEvents.FsmCancel, null);
		this.ElevatorReset();
		this.ConveyorReset();
		this.IgnoreInputWithoutReset();
		bool flag = this.dashingDown;
		this.ResetMotion(false);
		if (flag)
		{
			this.dashingDown = true;
			this.HeroDash(true);
		}
		this.airDashed = false;
		this.doubleJumped = false;
		this.ResetHardLandingTimer();
		this.ResetAttacksDash();
		this.AffectedByGravity(false);
		this.sceneEntryGate = enterGate;
		this.SetState(ActorStates.no_input);
		this.transitionState = HeroTransitionState.WAITING_TO_ENTER_LEVEL;
		this.renderer.enabled = true;
		enterGate.BeforeEntry();
		if (!this.cState.transitioning)
		{
			this.cState.transitioning = true;
		}
		this.gatePosition = enterGate.GetGatePosition();
		this.proxyFSM.SendEvent("HeroCtrl-EnteringScene");
		if (this.exitedSuperDashing)
		{
			this.superJumpFSM.SendEventSafe("PRE ENTER SUPERJUMPING");
		}
		SilkSpool.ResumeSilkAudio();
		if (this.gatePosition == GatePosition.top)
		{
			this.cState.onGround = false;
			this.enteringVertically = true;
			this.exitedSuperDashing = false;
			this.exitedSprinting = false;
			float x2 = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y2 = enterGate.transform.position.y + enterGate.entryOffset.y;
			this.transform.SetPosition2D(x2, y2);
			this.SendHeroInPosition(false);
			yield return base.StartCoroutine(this.EnterHeroSubFadeUp(enterGate, forceCustomFade, delayBeforeEnter, enterSkip));
			if (this.exitedQuake)
			{
				this.IgnoreInput();
				this.proxyFSM.SendEvent("HeroCtrl-EnterQuake");
				yield return new WaitForSeconds(0.25f);
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.rb2d.linearVelocity = new Vector2(0f, this.SPEED_TO_ENTER_SCENE_DOWN);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				this.transitionState = HeroTransitionState.DROPPING_DOWN;
				this.AffectedByGravity(true);
				if (enterGate.hardLandOnExit)
				{
					this.cState.willHardLand = true;
				}
				yield return new WaitForSeconds(0.33f);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				if (this.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
				{
					this.FinishedEnteringScene(true, false);
				}
			}
		}
		else if (this.gatePosition == GatePosition.bottom)
		{
			this.cState.onGround = false;
			this.enteringVertically = true;
			this.exitedSprinting = false;
			if (enterGate.alwaysEnterRight)
			{
				this.FaceRight();
			}
			if (enterGate.alwaysEnterLeft)
			{
				this.FaceLeft();
			}
			float x = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y = enterGate.transform.position.y + enterGate.entryOffset.y + 3f;
			this.transform.SetPosition2D(x, y);
			this.SendHeroInPosition(false);
			yield return base.StartCoroutine(this.EnterHeroSubFadeUp(enterGate, forceCustomFade, delayBeforeEnter, enterSkip));
			if (this.exitedSuperDashing)
			{
				this.IgnoreInput();
				this.superJumpFSM.SendEventSafe("ENTER SUPERJUMPING");
				Debug.Log("Entering Super Jumping");
				yield return null;
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.transition_vel = (this.cState.facingRight ? new Vector2(this.SPEED_TO_ENTER_SCENE_HOR, this.SPEED_TO_ENTER_SCENE_UP) : new Vector2(-this.SPEED_TO_ENTER_SCENE_HOR, this.SPEED_TO_ENTER_SCENE_UP));
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				this.transform.SetPosition2D(x, y);
				yield return new WaitForSeconds(this.TIME_TO_ENTER_SCENE_BOT);
				this.transition_vel = new Vector2(this.rb2d.linearVelocity.x, 0f);
				this.AffectedByGravity(true);
				this.transitionState = HeroTransitionState.DROPPING_DOWN;
			}
		}
		else if (this.gatePosition == GatePosition.left)
		{
			yield return base.StartCoroutine(this.EnterHeroSubHorizontal(enterGate, forceCustomFade, delayBeforeEnter, enterSkip, 1f));
		}
		else if (this.gatePosition == GatePosition.right)
		{
			yield return base.StartCoroutine(this.EnterHeroSubHorizontal(enterGate, forceCustomFade, delayBeforeEnter, enterSkip, -1f));
		}
		else if (this.gatePosition == GatePosition.door)
		{
			if (enterGate.alwaysEnterRight)
			{
				this.FaceRight();
			}
			if (enterGate.alwaysEnterLeft)
			{
				this.FaceLeft();
			}
			this.cState.falling = false;
			this.cState.onGround = true;
			if (enterGate.dontWalkOutOfDoor)
			{
				this.ForceWalkingSound = false;
			}
			else
			{
				this.ForceWalkingSound = true;
			}
			this.enteringVertically = false;
			this.SetState(ActorStates.idle);
			this.SetState(ActorStates.no_input);
			this.exitedSuperDashing = false;
			this.exitedSprinting = false;
			this.animCtrl.PlayClip("Idle");
			this.transform.SetPosition2D(this.FindGroundPoint(enterGate.transform.position, false));
			this.SendHeroInPosition(false);
			yield return null;
			yield return new WaitForFixedUpdate();
			yield return base.StartCoroutine(this.EnterHeroSubFadeUp(enterGate, forceCustomFade, delayBeforeEnter, enterSkip));
			if (enterGate.dontWalkOutOfDoor)
			{
				yield return new WaitForSeconds(0.33f);
			}
			else
			{
				this.animCtrl.PlayClip("Exit Door To Idle");
				yield return new WaitForTk2dAnimatorClipFinish(this.animCtrl.animator, delegate(tk2dSpriteAnimator _, tk2dSpriteAnimationClip _)
				{
					this.animCtrl.SetPlayRunToIdle();
				});
			}
			this.FinishedEnteringScene(true, false);
		}
		if (onEnd != null)
		{
			onEnd();
		}
		yield break;
	}

	// Token: 0x06000E6A RID: 3690 RVA: 0x00043367 File Offset: 0x00041567
	private IEnumerator EnterHeroSubHorizontal(TransitionPoint enterGate, bool forceCustomFade, float delayBeforeEnter, bool enterSkip, float direction)
	{
		this.cState.falling = false;
		this.cState.onGround = true;
		this.canSoftLand = false;
		this.enteringVertically = false;
		this.exitedSuperDashing = false;
		this.SetState(ActorStates.no_input);
		if (this.enterWithoutInput || this.exitedSuperDashing || this.exitedQuake || this.exitedSprinting)
		{
			this.IgnoreInput();
		}
		this.ForceWalkingSound = true;
		Vector3 position = enterGate.transform.position;
		float x = position.x + enterGate.entryOffset.x;
		float y = this.FindGroundPointY(x + direction, position.y, false);
		this.ResetVelocity();
		this.extraAirMoveVelocities.Clear();
		this.PreventSoftLand(1f);
		this.transform.SetPosition2D(x, y);
		this.SendPreHeroInPosition(true);
		yield return null;
		this.transform.SetPosition2D(x, y);
		this.SendHeroInPosition(true);
		if (direction > 0f)
		{
			this.FaceRight();
		}
		else
		{
			this.FaceLeft();
		}
		this.animCtrl.ResetAll();
		this.renderer.enabled = false;
		yield return base.StartCoroutine(this.EnterHeroSubFadeUp(enterGate, forceCustomFade, delayBeforeEnter, enterSkip));
		this.renderer.enabled = true;
		this.cState.falling = false;
		this.cState.onGround = true;
		this.canSoftLand = false;
		this.PreventSoftLand(1f);
		this.SetState(ActorStates.no_input);
		if (this.skipNormalEntry)
		{
			this.transition_vel = new Vector2(this.GetRunSpeed() * direction, 0f);
			this.transitionState = HeroTransitionState.ENTERING_SCENE;
			yield return new WaitForSeconds(0.33f);
			this.FinishedEnteringScene(true, true);
		}
		else if (enterSkip)
		{
			this.transitionState = HeroTransitionState.ENTERING_SCENE;
			float x2 = this.GetRunSpeed() * direction * 0.33f;
			this.rb2d.MovePosition(this.rb2d.position + new Vector2(x2, 0f));
			this.FinishedEnteringScene(true, true);
			this.animCtrl.StartControl();
			this.ForceWalkingSound = false;
		}
		else if (this.exitedSprinting)
		{
			this.IgnoreInput();
			this.sprintFSM.SendEventSafe("ENTER SPRINTING");
			yield return null;
			this.FinishedEnteringScene(true, false);
		}
		else
		{
			this.transition_vel = new Vector2(this.GetRunSpeed() * direction, 0f);
			this.transitionState = HeroTransitionState.ENTERING_SCENE;
			this.ForceRunningSound = true;
			this.ForceWalkingSound = false;
			yield return new WaitForSeconds(0.33f);
			this.SetState(ActorStates.running);
			this.FinishedEnteringScene(true, true);
		}
		yield break;
	}

	// Token: 0x06000E6B RID: 3691 RVA: 0x0004339B File Offset: 0x0004159B
	private IEnumerator EnterHeroSubFadeUp(TransitionPoint enterGate, bool forceCustomFade, float delayBeforeEnter, bool enterSkip)
	{
		yield return new WaitForSeconds(0.165f);
		float sceneFadeUpPadding = this.GetSceneFadeUpPadding();
		if (sceneFadeUpPadding > 0f)
		{
			yield return new WaitForSeconds(sceneFadeUpPadding);
		}
		if (!enterGate.customFade && !forceCustomFade)
		{
			while (CameraController.IsPositioningCamera && this.gm.cameraCtrl.StartLockedTimer > 0f)
			{
				yield return null;
			}
			this.gm.FadeSceneIn();
		}
		if (enterSkip)
		{
			yield break;
		}
		if (delayBeforeEnter > 0f)
		{
			yield return new WaitForSeconds(delayBeforeEnter);
		}
		if (enterGate.entryDelay > 0f)
		{
			yield return new WaitForSeconds(enterGate.entryDelay);
		}
		yield break;
	}

	// Token: 0x06000E6C RID: 3692 RVA: 0x000433C7 File Offset: 0x000415C7
	private float GetSceneFadeUpPadding()
	{
		return 0f;
	}

	// Token: 0x06000E6D RID: 3693 RVA: 0x000433D0 File Offset: 0x000415D0
	public static void MoveIfNotInDontDestroyOnLoad(GameObject obj)
	{
		if (obj.scene.name == "DontDestroyOnLoad")
		{
			return;
		}
		if (obj.transform.parent != null)
		{
			obj.transform.SetParent(null);
		}
		Object.DontDestroyOnLoad(obj);
	}

	// Token: 0x06000E6E RID: 3694 RVA: 0x0004341D File Offset: 0x0004161D
	private void ResetHeroInPositionState()
	{
		this.isHeroInPosition = false;
		this.isHeroInPrePosition = false;
	}

	// Token: 0x06000E6F RID: 3695 RVA: 0x00043430 File Offset: 0x00041630
	public void LeaveScene(GatePosition? gate = null)
	{
		this.ResetHeroInPositionState();
		this.SetBlockSteepSlopes(false);
		HeroController.MoveIfNotInDontDestroyOnLoad(base.gameObject);
		this.updraftsEntered = 0;
		this.IgnoreInputWithoutReset();
		this.ResetHardLandingTimer();
		this.SetState(ActorStates.no_input);
		this.SetDamageMode(DamageMode.NO_DAMAGE);
		this.transitionState = HeroTransitionState.EXITING_SCENE;
		this.CancelFallEffects();
		this.tilemapTestActive = false;
		this.SetHeroParent(null);
		this.StopTilemapTest();
		if (gate != null)
		{
			switch (gate.Value)
			{
			case GatePosition.top:
				this.transition_vel = new Vector2(0f, this.MIN_JUMP_SPEED);
				this.cState.onGround = false;
				break;
			case GatePosition.right:
			{
				float x = this.rb2d.linearVelocity.x;
				this.transition_vel = new Vector2(this.GetRunSpeed(), 0f);
				if (x > this.transition_vel.x)
				{
					this.transition_vel = new Vector2(x, 0f);
				}
				if (this.cState.onGround)
				{
					this.ForceRunningSound = true;
				}
				break;
			}
			case GatePosition.left:
			{
				float x = this.rb2d.linearVelocity.x;
				this.transition_vel = new Vector2(-this.GetRunSpeed(), 0f);
				if (x < this.transition_vel.x)
				{
					this.transition_vel = new Vector2(x, 0f);
				}
				if (this.cState.onGround)
				{
					this.ForceRunningSound = true;
				}
				break;
			}
			case GatePosition.bottom:
				this.transition_vel = Vector2.zero;
				this.cState.onGround = false;
				break;
			}
		}
		this.cState.transitioning = true;
	}

	// Token: 0x06000E70 RID: 3696 RVA: 0x000435D2 File Offset: 0x000417D2
	public IEnumerator BetaLeave(EndBeta betaEndTrigger)
	{
		if (!this.playerData.betaEnd)
		{
			this.endBeta = betaEndTrigger;
			this.IgnoreInput();
			this.playerData.disablePause = true;
			this.SetState(ActorStates.no_input);
			this.ResetInput();
			this.tilemapTestActive = false;
			yield return new WaitForSeconds(0.66f);
			GameObject.Find("Beta Ender").GetComponent<SimpleSpriteFade>().FadeIn();
			this.ResetMotion(true);
			yield return new WaitForSeconds(1.25f);
			this.playerData.betaEnd = true;
		}
		yield break;
	}

	// Token: 0x06000E71 RID: 3697 RVA: 0x000435E8 File Offset: 0x000417E8
	public IEnumerator BetaReturn()
	{
		this.rb2d.linearVelocity = new Vector2(this.GetRunSpeed(), 0f);
		if (!this.cState.facingRight)
		{
			this.FlipSprite();
		}
		GameObject.Find("Beta Ender").GetComponent<SimpleSpriteFade>().FadeOut();
		this.animCtrl.PlayClip("Run");
		yield return new WaitForSeconds(1.4f);
		this.SetState(ActorStates.grounded);
		this.SetStartingMotionState();
		this.AcceptInput();
		this.playerData.betaEnd = false;
		this.playerData.disablePause = false;
		this.tilemapTestActive = true;
		if (this.endBeta != null)
		{
			this.endBeta.Reactivate();
		}
		yield break;
	}

	// Token: 0x06000E72 RID: 3698 RVA: 0x000435F7 File Offset: 0x000417F7
	public IEnumerator Respawn(Transform spawnPoint)
	{
		HeroController.<>c__DisplayClass983_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		bool wasDead = this.cState.dead;
		this.playerData = PlayerData.instance;
		this.playerData.disablePause = true;
		base.gameObject.layer = 9;
		this.renderer.enabled = true;
		this.heroBox.HeroBoxNormal();
		this.rb2d.isKinematic = false;
		this.ResetLook();
		this.cState.dead = false;
		this.cState.isFrostDeath = false;
		this.cState.onGround = true;
		this.cState.falling = false;
		this.cState.hazardDeath = false;
		this.cState.recoiling = false;
		this.enteringVertically = false;
		this.airDashed = false;
		this.doubleJumped = false;
		this.startFromMantle = false;
		if (wasDead)
		{
			this.CharmUpdate();
			this.MaxHealth();
		}
		this.ResetMotion(true);
		this.ResetHardLandingTimer();
		this.ResetAttacks(true);
		this.ResetInput();
		if (wasDead)
		{
			this.CharmUpdate();
		}
		this.ResetLavaBell();
		if (wasDead)
		{
			EventRegister.SendEvent("MAGGOT RESET", null);
			this.ClearEffects();
			if (this.frostedEffect.IsAlive())
			{
				this.frostedEffect.StopParticleSystems();
				this.frostedEffect.ClearParticleSystems();
			}
		}
		SilkSpool.ResumeSilkAudio();
		this.ResetSilkRegen();
		if (wasDead)
		{
			SilkSpool silkSpool = GameCameras.instance.silkSpool;
			silkSpool.RemoveUsing(SilkSpool.SilkUsingFlags.Maggot, 1);
			silkSpool.RefreshSilk();
		}
		bool flag = this.playerData.tempRespawnType == 0;
		this.playerData.ResetTempRespawn();
		PlayMakerFSM benchFSM = (spawnPoint && spawnPoint.gameObject.activeInHierarchy) ? FSMUtility.LocateFSM(spawnPoint.gameObject, "Bench Control") : null;
		if (spawnPoint != null)
		{
			if (!spawnPoint.gameObject.activeInHierarchy)
			{
				Transform transform = spawnPoint.Find("Alt Respawn Point");
				if (transform)
				{
					spawnPoint = transform;
				}
			}
			this.transform.SetPosition2D(this.FindGroundPoint(spawnPoint.transform.position, false));
			if (benchFSM != null)
			{
				FSMUtility.GetVector3(benchFSM, "Adjust Vector");
			}
		}
		CS$<>8__locals1.respawnMarker = spawnPoint.GetComponent<RespawnMarker>();
		this.TickFrostEffect(false);
		this.playerData.ResetCutsceneBools();
		this.playerData.isInvincible = false;
		if (!flag && this.playerData.respawnType == 1 && benchFSM != null)
		{
			this.AffectedByGravity(false);
			benchFSM.FsmVariables.GetFsmBool("RespawnResting").Value = true;
			yield return new WaitForEndOfFrame();
			this.<Respawn>g__SetFacingForSpawnPoint|983_0(ref CS$<>8__locals1);
			this.SendHeroInPosition(false);
			this.HeroRespawned();
			this.FinishedEnteringScene(true, false);
			benchFSM.SendEvent("RESPAWN");
		}
		else
		{
			yield return new WaitForEndOfFrame();
			yield return null;
			this.IgnoreInput();
			this.<Respawn>g__SetFacingForSpawnPoint|983_0(ref CS$<>8__locals1);
			this.SendHeroInPosition(false);
			if (!CS$<>8__locals1.respawnMarker || !CS$<>8__locals1.respawnMarker.customWakeUp)
			{
				this.StopAnimationControl();
				this.controlReqlinquished = true;
				if (CS$<>8__locals1.respawnMarker && CS$<>8__locals1.respawnMarker.customFadeDuration.IsEnabled)
				{
					this.animCtrl.PlayClipForced("Prostrate");
					yield return new WaitForSeconds(CS$<>8__locals1.respawnMarker.customFadeDuration.Value);
				}
				float clipLength = this.animCtrl.GetClipDuration("Wake Up Ground");
				this.animCtrl.PlayClipForced("Wake Up Ground");
				tk2dSpriteAnimationClip clip = this.animCtrl.animator.CurrentClip;
				if (clip != null)
				{
					while (this.animCtrl.animator.IsPlaying(clip))
					{
						if (clipLength <= 0f)
						{
							break;
						}
						yield return null;
						clipLength -= Time.deltaTime;
					}
				}
				else
				{
					yield return new WaitForSeconds(clipLength);
				}
				this.SetState(ActorStates.grounded);
				this.StartAnimationControl();
				this.controlReqlinquished = false;
				GameCameras.instance.HUDIn();
				clip = null;
			}
			if (CS$<>8__locals1.respawnMarker)
			{
				CS$<>8__locals1.respawnMarker.RespawnedHere();
			}
			this.HeroRespawned();
			this.FinishedEnteringScene(true, false);
		}
		if (wasDead && this.playerData.HasSeenGeo)
		{
			if (this.playerData.geo <= 0)
			{
				CurrencyCounter.ToZero(CurrencyType.Money);
			}
			else
			{
				CurrencyCounter.ToValue(this.playerData.geo, CurrencyType.Money);
			}
		}
		yield break;
	}

	// Token: 0x06000E73 RID: 3699 RVA: 0x00043610 File Offset: 0x00041810
	public void HazardRespawnReset()
	{
		this.cState.hazardDeath = false;
		this.cState.onGround = true;
		this.cState.hazardRespawning = true;
		this.ResetMotion(true);
		this.ResetHardLandingTimer();
		this.ResetAttacks(true);
		this.ResetInput();
		this.cState.recoiling = false;
		this.enteringVertically = false;
		this.airDashed = false;
		this.doubleJumped = false;
		this.cState.lookingUp = false;
		this.cState.lookingDown = false;
		base.gameObject.layer = 9;
		this.renderer.enabled = true;
		this.heroBox.HeroBoxNormal();
		this.IsStunned = false;
		EventRegister.SendEvent(EventRegisterEvents.HazardRespawnReset, null);
	}

	// Token: 0x06000E74 RID: 3700 RVA: 0x000436CA File Offset: 0x000418CA
	public void ResetShuttlecock()
	{
		this.startWithShuttlecock = false;
		this.sprintBufferSteps = 0;
		this.syncBufferSteps = false;
	}

	// Token: 0x06000E75 RID: 3701 RVA: 0x000436E1 File Offset: 0x000418E1
	public IEnumerator HazardRespawn()
	{
		this.doingHazardRespawn = true;
		this.SetState(ActorStates.no_input);
		this.lastHazardRespawnTime = Time.timeAsDouble;
		this.ResetLook();
		Vector2 respawnPos;
		if (!this.TryFindGroundPoint(out respawnPos, this.playerData.hazardRespawnLocation, true))
		{
			string entryGateName = this.gm.entryGateName;
			if (!string.IsNullOrEmpty(entryGateName))
			{
				TransitionPoint transitionPoint = TransitionPoint.TransitionPoints.FirstOrDefault((TransitionPoint p) => p.gameObject.name == entryGateName);
				if (transitionPoint != null)
				{
					this.transform.SetPosition2D(new Vector2(-500f, -500f));
					this.HazardRespawnReset();
					this.gm.BeginSceneTransition(new GameManager.SceneLoadInfo
					{
						SceneName = transitionPoint.targetScene,
						EntryGateName = transitionPoint.entryPoint
					});
					yield break;
				}
			}
		}
		this.StartInvulnerable(this.INVUL_TIME * 2f + 0.3f);
		Vector2 posBeforeRespawn = this.transform.position;
		this.transform.SetPosition2D(respawnPos);
		this.rb2d.isKinematic = false;
		this.HazardRespawnReset();
		yield return null;
		this.transform.SetPosition2D(respawnPos);
		this.rb2d.linearVelocity = Vector2.zero;
		yield return new WaitForEndOfFrame();
		switch (this.playerData.hazardRespawnFacing)
		{
		case HazardRespawnMarker.FacingDirection.None:
			if (posBeforeRespawn.x < respawnPos.x)
			{
				this.FaceLeft();
			}
			else
			{
				this.FaceRight();
			}
			break;
		case HazardRespawnMarker.FacingDirection.Left:
			this.FaceLeft();
			break;
		case HazardRespawnMarker.FacingDirection.Right:
			this.FaceRight();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		this.SendHeroInPosition(false);
		if (this.OnHazardRespawn != null)
		{
			this.OnHazardRespawn();
		}
		yield return new WaitForSeconds(0.3f);
		GCManager.Collect();
		yield return null;
		this.gm.screenFader_fsm.SendEventSafe("HAZARD RESPAWN");
		this.proxyFSM.SendEvent("HeroCtrl-HazardRespawned");
		tk2dSpriteAnimationClip clip = this.animCtrl.GetClip("Hazard Respawn");
		yield return base.StartCoroutine(this.animCtrl.animator.PlayAnimWait(clip, null));
		this.cState.hazardRespawning = false;
		this.rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
		this.FinishedEnteringScene(false, false);
		yield break;
	}

	// Token: 0x06000E76 RID: 3702 RVA: 0x000436F0 File Offset: 0x000418F0
	public bool GetState(string stateName)
	{
		return this.cState.GetState(stateName);
	}

	// Token: 0x06000E77 RID: 3703 RVA: 0x000436FE File Offset: 0x000418FE
	public bool GetCState(string stateName)
	{
		return this.cState.GetState(stateName);
	}

	// Token: 0x06000E78 RID: 3704 RVA: 0x0004370C File Offset: 0x0004190C
	public void SetCState(string stateName, bool value)
	{
		this.cState.SetState(stateName, value);
	}

	// Token: 0x06000E79 RID: 3705 RVA: 0x0004371B File Offset: 0x0004191B
	public static bool CStateExists(string stateName)
	{
		return HeroControllerStates.CStateExists(stateName);
	}

	// Token: 0x06000E7A RID: 3706 RVA: 0x00043723 File Offset: 0x00041923
	public void ResetHardLandingTimer()
	{
		this.cState.willHardLand = false;
		this.hardLandingTimer = 0f;
		this.fallTimer = 0f;
		this.hardLanded = false;
	}

	// Token: 0x06000E7B RID: 3707 RVA: 0x0004374E File Offset: 0x0004194E
	public void CancelSuperDash()
	{
	}

	// Token: 0x06000E7C RID: 3708 RVA: 0x00043750 File Offset: 0x00041950
	private void CancelDownSpike()
	{
		this.cState.downSpikeAntic = false;
		this.startWithDownSpikeBounce = false;
		this.startWithDownSpikeBounceShort = false;
		this.startWithDownSpikeBounceSlightlyShort = false;
		if (this.cState.downSpiking)
		{
			this.FinishDownspike();
		}
	}

	// Token: 0x06000E7D RID: 3709 RVA: 0x00043786 File Offset: 0x00041986
	public void CancelDownSpikeBounces()
	{
		this.startWithDownSpikeBounce = false;
		this.startWithDownSpikeBounceShort = false;
		this.startWithDownSpikeBounceSlightlyShort = false;
		this.cState.downSpikeBouncing = false;
		this.cState.downSpikeBouncingShort = false;
	}

	// Token: 0x06000E7E RID: 3710 RVA: 0x000437B8 File Offset: 0x000419B8
	public void RelinquishControlNotVelocity()
	{
		HeroController.ControlVersion++;
		this.CancelDownSpike();
		if (!this.controlReqlinquished)
		{
			this.prev_hero_state = ActorStates.idle;
			this.ResetInput();
			this.ResetMotionNotVelocity();
			this.SetState(ActorStates.no_input);
			this.IgnoreInput();
			this.controlReqlinquished = true;
			this.controlRelinquishedFrame = Time.frameCount;
			this.ResetLook();
			this.ResetAttacks(true);
			this.CancelJump();
			this.touchingWallL = false;
			this.touchingWallR = false;
			this.touchingWallObj = null;
		}
	}

	// Token: 0x17000181 RID: 385
	// (get) Token: 0x06000E7F RID: 3711 RVA: 0x00043839 File Offset: 0x00041A39
	// (set) Token: 0x06000E80 RID: 3712 RVA: 0x00043840 File Offset: 0x00041A40
	public static int ControlVersion { get; private set; }

	// Token: 0x06000E81 RID: 3713 RVA: 0x00043848 File Offset: 0x00041A48
	public void RelinquishControl()
	{
		HeroController.ControlVersion++;
		this.RemoveUnlockRequest(HeroLockStates.ControlLocked);
		this.CancelDownSpike();
		if ((!this.controlReqlinquished || this.acceptingInput) && !this.cState.dead)
		{
			this.ResetInput();
			this.ResetMotion(true);
			this.IgnoreInput();
			this.controlReqlinquished = true;
			this.controlRelinquishedFrame = Time.frameCount;
			this.ResetLook();
			this.CancelAttack();
			this.CancelJump();
			this.touchingWallL = false;
			this.touchingWallR = false;
			this.touchingWallObj = null;
		}
	}

	// Token: 0x06000E82 RID: 3714 RVA: 0x000438D7 File Offset: 0x00041AD7
	public void RegainControl()
	{
		this.RegainControl(true);
	}

	// Token: 0x06000E83 RID: 3715 RVA: 0x000438E0 File Offset: 0x00041AE0
	public void RegainControl(bool allowInput)
	{
		if (this.CheckAndRequestUnlock(HeroLockStates.ControlLocked))
		{
			return;
		}
		this.RemoveUnlockRequest(HeroLockStates.ControlLocked);
		InteractManager.BlockingInteractable = null;
		this.enteringVertically = false;
		this.regainControlJumpQueued = false;
		this.cState.willHardLand = false;
		this.hardLandingTimer = 0f;
		if (this.cState.onGround)
		{
			this.fallTimer = 0f;
		}
		this.hardLanded = false;
		this.SetBlockFsmMove(false);
		this.audioCtrl.BlockFootstepAudio = false;
		this.heroBox.HeroBoxNormal();
		NoClamberRegion.RefreshInside();
		this.AcceptInput();
		this.inputHandler.ForceDreamNailRePress = true;
		if (!this.cState.hazardDeath && !this.cState.hazardRespawning)
		{
			this.hero_state = ActorStates.idle;
		}
		this.allowRecoilWhileRelinquished = false;
		this.recoilZeroVelocity = false;
		if (this.controlReqlinquished && !this.cState.dead)
		{
			this.AffectedByGravity(true);
			this.controlReqlinquished = false;
			this.SetStartingMotionState();
			if (this.startWithWallslide)
			{
				this.startWithWallslide = false;
				this.ForceTouchingWall();
				this.BeginWallSlide(false);
			}
			else if (this.startWithShuttlecock)
			{
				this.startWithShuttlecock = false;
				this.HeroJump(false);
				this.OnShuttleCockJump();
			}
			else if (this.startWithTinyJump)
			{
				this.HeroJump();
				this.cState.onGround = false;
				this.doubleJumpQueuing = false;
				this.startWithTinyJump = false;
				this.jump_steps = this.JUMP_STEPS;
				this.jumped_steps = this.JUMP_STEPS_MIN;
			}
			else if (this.startWithBrolly)
			{
				if (this.playerData.hasBrolly)
				{
					this.StartFloat();
				}
				this.startWithBrolly = false;
			}
			else if (this.startWithDoubleJump)
			{
				if (this.playerData.hasDoubleJump && !this.doubleJumped)
				{
					this.DoDoubleJump();
					this.doubleJumpQueuing = false;
				}
				else if (this.playerData.hasBrolly)
				{
					this.StartFloat();
				}
				this.startWithDoubleJump = false;
			}
			else if (this.startWithJump)
			{
				this.HeroJumpNoEffect();
				this.doubleJumpQueuing = false;
				this.startWithJump = false;
				this.startWithTinyJump = false;
			}
			else if (this.startWithAnyJump)
			{
				if (this.CanJump())
				{
					this.HeroJump();
				}
				else if (this.CanDoubleJump(true))
				{
					this.DoubleJump();
				}
				else if (this.CanFloat(true))
				{
					this.StartFloat();
				}
				this.startWithAnyJump = false;
			}
			else if (this.startWithWallsprintLaunch)
			{
				this.HeroJump();
				this.cState.onGround = false;
				this.doubleJumpQueuing = false;
				this.startWithWallsprintLaunch = false;
				this.jumped_steps = -7;
			}
			else if (this.startWithFullJump || this.startWithFlipJump || this.startWithBackflipJump)
			{
				if (this.startFromMantle)
				{
					this.startFromMantle = false;
					this.cState.mantleRecovery = true;
					this.TrySetCorrectFacing(true);
				}
				if (this.startWithBackflipJump)
				{
					this.HeroJumpNoEffect();
					this.jump_steps = 0;
					this.FlipSprite();
					this.animCtrl.SetPlayBackflip();
					if (this.backflipPuffPrefab)
					{
						this.backflipPuffPrefab.SetActive(false);
						this.backflipPuffPrefab.SetActive(true);
					}
					this.audioCtrl.PlaySound(HeroSounds.WALLJUMP, true);
					this.gruntAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
				}
				else if (this.startWithFlipJump)
				{
					this.HeroJumpNoEffect();
					this.animCtrl.SetPlayMantleCancel();
				}
				else
				{
					this.HeroJump(false);
				}
				this.cState.onGround = false;
				this.doubleJumpQueuing = false;
				this.startWithFullJump = false;
				this.startWithFlipJump = false;
				this.startWithBackflipJump = false;
			}
			else if (this.startWithWallJump)
			{
				this.startWithWallJump = false;
				this.TryDoWallJumpFromMove();
			}
			else if (this.startWithDash)
			{
				this.HeroDash(false);
				this.doubleJumpQueuing = false;
				this.startWithDash = false;
				this.dashCurrentFacing = false;
			}
			else if (this.startWithAttack)
			{
				if (this.cState.wallScrambling)
				{
					this.ForceTouchingWall();
					this.BeginWallSlide(false);
				}
				this.DoAttack();
				this.doubleJumpQueuing = false;
				this.startWithAttack = false;
			}
			else if (this.CanStartWithThrowTool())
			{
				if (this.cState.wallScrambling)
				{
					this.ForceTouchingWall();
					this.BeginWallSlide(false);
				}
				this.doubleJumpQueuing = false;
				this.startWithToolThrow = false;
				this.ThrowTool(false);
			}
			else if (this.startWithDownSpikeBounce)
			{
				this.startWithDownSpikeBounce = false;
				this.DownspikeBounce(false, null);
			}
			else if (this.startWithDownSpikeBounceSlightlyShort)
			{
				this.startWithDownSpikeBounceSlightlyShort = false;
				this.DownspikeBounceSlightlyShort(false);
			}
			else if (this.startWithDownSpikeBounceShort)
			{
				this.startWithDownSpikeBounceShort = false;
				this.DownspikeBounceShort(false);
			}
			else if (this.startWithDownSpikeEnd)
			{
				this.startWithDownSpikeEnd = false;
				this.FinishDownspike();
				this.CancelJump();
				this.rb2d.linearVelocity = Vector2.zero;
			}
			else if (this.startWithHarpoonBounce)
			{
				this.startWithHarpoonBounce = false;
				this.DownspikeBounce(true, null);
			}
			else if (this.startWithBalloonBounce)
			{
				this.useUpdraftExitJumpSpeed = true;
				this.TrySetCorrectFacing(true);
				this.DownspikeBounce(false, null);
				this.allowAttackCancellingDownspikeRecovery = true;
				this.startWithBalloonBounce = false;
			}
			else if (this.startWithUpdraftExit)
			{
				this.startWithUpdraftExit = false;
				this.useUpdraftExitJumpSpeed = true;
				this.TrySetCorrectFacing(true);
				this.DownspikeBounce(false, null);
			}
			else if (this.startWithScrambleLeap)
			{
				this.startWithScrambleLeap = false;
				this.cState.mantleRecovery = true;
			}
			else if (this.startFromMantle)
			{
				this.startFromMantle = false;
				this.cState.mantleRecovery = true;
			}
			else
			{
				this.cState.touchingWall = false;
				this.touchingWallL = false;
				this.touchingWallR = false;
				this.touchingWallObj = null;
				if (allowInput)
				{
					if (this.CanJump() && this.inputHandler.GetWasButtonPressedQueued(HeroActionButton.JUMP, true))
					{
						this.HeroJump();
					}
					else if (this.jumpPressedWhileRelinquished)
					{
						this.regainControlJumpQueued = true;
					}
				}
			}
			if (this.startWithRecoilBack)
			{
				if (this.transform.localScale.x < 0f)
				{
					this.RecoilLeft();
				}
				else
				{
					this.RecoilRight();
				}
				this.startWithRecoilBack = false;
			}
			if (this.startWithRecoilBackLong)
			{
				if (this.transform.localScale.x < 0f)
				{
					this.RecoilLeftLong();
				}
				else
				{
					this.RecoilRightLong();
				}
				this.startWithRecoilBackLong = false;
			}
			if (this.startWithWhipPullRecoil)
			{
				if (this.transform.localScale.x < 0f)
				{
					this.RecoilRight();
				}
				else
				{
					this.RecoilLeft();
				}
				this.startWithWhipPullRecoil = false;
			}
		}
		this.jumpPressedWhileRelinquished = false;
		this.IsStunned = false;
	}

	// Token: 0x06000E84 RID: 3716 RVA: 0x00043F60 File Offset: 0x00042160
	private void OnShuttleCockJump()
	{
		this.shuttleCockJumpSteps = 2;
		this.jumped_steps = -5;
		this.cState.shuttleCock = true;
		this.dashCooldownTimer = 0.1f;
		if (this.transform.localScale.x < 0f)
		{
			this.shuttlecockSpeed = this.SHUTTLECOCK_SPEED;
		}
		else
		{
			this.shuttlecockSpeed = -this.SHUTTLECOCK_SPEED;
		}
		this.doubleJumpQueuing = false;
		this.startWithJump = false;
		if (this.shuttleCockJumpAudio)
		{
			this.shuttleCockJumpAudio.Play();
		}
		this.vibrationCtrl.StartShuttlecock();
		if (this.shuttleCockJumpEffectPrefab)
		{
			GameObject gameObject = this.shuttleCockJumpEffectPrefab.Spawn();
			Vector3 localScale = this.shuttleCockJumpEffectPrefab.transform.localScale;
			Vector3 localScale2 = this.transform.localScale;
			gameObject.transform.localScale = new Vector3(localScale.x * -localScale2.x, localScale.y * localScale2.y, localScale.z);
			gameObject.transform.position = this.transform.position;
		}
	}

	// Token: 0x06000E85 RID: 3717 RVA: 0x00044071 File Offset: 0x00042271
	public void PreventCastByDialogueEnd()
	{
		this.preventCastByDialogueEndTimer = 0.3f;
	}

	// Token: 0x06000E86 RID: 3718 RVA: 0x0004407E File Offset: 0x0004227E
	public bool CanDoFsmMove()
	{
		return !this.blockFsmMove;
	}

	// Token: 0x06000E87 RID: 3719 RVA: 0x00044089 File Offset: 0x00042289
	public bool IsHardLanding()
	{
		return this.hardLanded || this.hero_state == ActorStates.hard_landing;
	}

	// Token: 0x06000E88 RID: 3720 RVA: 0x000440A0 File Offset: 0x000422A0
	public bool CanCast()
	{
		return this.CanDoFsmMove() && (!this.gm.isPaused && !this.cState.dashing && this.hero_state != ActorStates.no_input && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.recoiling && !this.cState.recoilFrozen && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.CanInput() && this.preventCastByDialogueEndTimer <= 0f);
	}

	// Token: 0x06000E89 RID: 3721 RVA: 0x0004416C File Offset: 0x0004236C
	public bool CanBind()
	{
		return this.CanDoFsmMove() && !this.IsHardLanding() && !this.IsInputBlocked() && ToolItemManager.ActiveState != ToolsActiveStates.Cutscene && (!this.gm.isPaused && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.recoiling && !this.cState.transitioning && !this.cState.recoilFrozen && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.dead && this.Config.CanBind && this.CanDoFSMCancelMove() && !ManagerSingleton<HeroChargeEffects>.Instance.IsCharging);
	}

	// Token: 0x06000E8A RID: 3722 RVA: 0x00044263 File Offset: 0x00042463
	public bool CanDoFSMCancelMove()
	{
		return this.CanInput() || this.cState.isInCancelableFSMMove;
	}

	// Token: 0x06000E8B RID: 3723 RVA: 0x0004427C File Offset: 0x0004247C
	public bool CanDoSpecial()
	{
		return this.CanDoFsmMove() && (!this.gm.isPaused && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.recoiling && !this.cState.recoilFrozen && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.CanDoFSMCancelMove() && this.preventCastByDialogueEndTimer <= 0f);
	}

	// Token: 0x06000E8C RID: 3724 RVA: 0x0004433C File Offset: 0x0004253C
	public bool CanNailArt()
	{
		if (!this.CanDoFsmMove())
		{
			return false;
		}
		if (!this.cState.transitioning && (this.hero_state != ActorStates.no_input || this.allowNailChargingWhileRelinquished) && !this.cState.attacking && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.nailChargeTimer >= this.CurrentNailChargeTime)
		{
			this.nailChargeTimer = 0f;
			return true;
		}
		this.nailChargeTimer = 0f;
		return false;
	}

	// Token: 0x06000E8D RID: 3725 RVA: 0x000443BF File Offset: 0x000425BF
	public bool CanQuickMap()
	{
		return this.CanQuickMap(false);
	}

	// Token: 0x06000E8E RID: 3726 RVA: 0x000443C8 File Offset: 0x000425C8
	public bool CanQuickMapBench()
	{
		return this.CanQuickMap(true);
	}

	// Token: 0x06000E8F RID: 3727 RVA: 0x000443D4 File Offset: 0x000425D4
	private bool CanQuickMap(bool onBench)
	{
		return (onBench || (!this.controlReqlinquished && this.hero_state != ActorStates.no_input)) && !this.IsInputBlocked() && (!this.gm.isPaused && !this.playerData.disablePause && !this.playerData.disableInventory && this.hero_state != ActorStates.hard_landing && this.playerData.HasAnyMap && !HeroController.IsLostInSlab(this.gm.gameMap) && !this.cState.onConveyor && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.recoiling && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && (this.cState.onGround || onBench)) && (onBench || this.CanInput());
	}

	// Token: 0x06000E90 RID: 3728 RVA: 0x0004450C File Offset: 0x0004270C
	private static bool IsLostInSlab(GameMap gameMap)
	{
		return CollectableItemManager.IsInHiddenMode() && !gameMap.HasAnyMapForZone(MapZone.THE_SLAB);
	}

	// Token: 0x06000E91 RID: 3729 RVA: 0x00044521 File Offset: 0x00042721
	public static bool HasNoMap(GameMap gameMap)
	{
		return HeroController.IsLostInSlab(gameMap) || gameMap.IsLostInAbyssPreMap();
	}

	// Token: 0x06000E92 RID: 3730 RVA: 0x00044534 File Offset: 0x00042734
	public bool CanInspect()
	{
		return !this.gm.isPaused && !this.cState.dashing && this.hero_state != ActorStates.no_input && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.recoiling && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && this.cState.onGround && this.CanInput();
	}

	// Token: 0x06000E93 RID: 3731 RVA: 0x000445F4 File Offset: 0x000427F4
	public bool CanBackDash()
	{
		return !this.gm.isPaused && !this.cState.dashing && this.hero_state != ActorStates.no_input && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.preventBackDash && !this.cState.backDashCooldown && !this.controlReqlinquished && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.cState.onGround;
	}

	// Token: 0x06000E94 RID: 3732 RVA: 0x000446B4 File Offset: 0x000428B4
	public bool CanPlayNeedolin()
	{
		return !this.playerData.isInventoryOpen && (!this.hardLanded && !this.gm.isPaused && this.hero_state != ActorStates.no_input && !this.cState.dashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && ((!this.controlReqlinquished && this.cState.onGround) || this.playerData.atBench) && !this.cState.hazardDeath && this.rb2d.linearVelocity.y > -0.1f && !this.cState.hazardRespawning && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.HasNeedolin());
	}

	// Token: 0x06000E95 RID: 3733 RVA: 0x000447AF File Offset: 0x000429AF
	public bool HasNeedolin()
	{
		return this.playerData.hasNeedolin && this.Config.CanPlayNeedolin;
	}

	// Token: 0x06000E96 RID: 3734 RVA: 0x000447CC File Offset: 0x000429CC
	public bool CanInteract()
	{
		ActorStates actorStates = this.hero_state;
		return (actorStates == ActorStates.idle || actorStates == ActorStates.running || actorStates == ActorStates.grounded) && this.CanTakeControl() && this.cState.onGround;
	}

	// Token: 0x06000E97 RID: 3735 RVA: 0x00044800 File Offset: 0x00042A00
	public bool CanTakeControl()
	{
		return this.CanInput() && this.hero_state != ActorStates.no_input && !this.gm.isPaused && !this.cState.dashing && !this.cState.backDashing && !this.cState.attacking && !this.controlReqlinquished && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning;
	}

	// Token: 0x06000E98 RID: 3736 RVA: 0x000448A4 File Offset: 0x00042AA4
	public bool CanOpenInventory()
	{
		return !this.gm.isPaused && !this.gm.RespawningHero && !this.IsInputBlocked() && ((!this.cState.recoiling && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.isInCutsceneMovement && !this.playerData.disablePause && !this.playerData.disableInventory && (this.CanInput() || this.controlReqlinquished) && InteractManager.BlockingInteractable == null && !GenericMessageCanvas.IsActive) || (this.playerData.atBench && !this.playerData.disableInventory));
	}

	// Token: 0x06000E99 RID: 3737 RVA: 0x00044975 File Offset: 0x00042B75
	public void SetDamageMode(int invincibilityType)
	{
		switch (invincibilityType)
		{
		case 0:
			this.damageMode = DamageMode.FULL_DAMAGE;
			return;
		case 1:
			this.damageMode = DamageMode.HAZARD_ONLY;
			return;
		case 2:
			this.damageMode = DamageMode.NO_DAMAGE;
			return;
		default:
			return;
		}
	}

	// Token: 0x06000E9A RID: 3738 RVA: 0x000449A1 File Offset: 0x00042BA1
	public void SetDamageModeFSM(int invincibilityType)
	{
		switch (invincibilityType)
		{
		case 0:
			this.damageMode = DamageMode.FULL_DAMAGE;
			return;
		case 1:
			this.damageMode = DamageMode.HAZARD_ONLY;
			return;
		case 2:
			this.damageMode = DamageMode.NO_DAMAGE;
			return;
		default:
			return;
		}
	}

	// Token: 0x06000E9B RID: 3739 RVA: 0x000449CD File Offset: 0x00042BCD
	public void ResetQuakeDamage()
	{
		if (this.damageMode == DamageMode.HAZARD_ONLY)
		{
			this.damageMode = DamageMode.FULL_DAMAGE;
		}
	}

	// Token: 0x06000E9C RID: 3740 RVA: 0x000449DF File Offset: 0x00042BDF
	public void SetDamageMode(DamageMode newDamageMode)
	{
		this.damageMode = newDamageMode;
		this.playerData.isInvincible = (newDamageMode == DamageMode.NO_DAMAGE);
	}

	// Token: 0x17000182 RID: 386
	// (get) Token: 0x06000E9D RID: 3741 RVA: 0x000449F7 File Offset: 0x00042BF7
	// (set) Token: 0x06000E9E RID: 3742 RVA: 0x000449FF File Offset: 0x00042BFF
	public int AnimationControlVersion { get; private set; }

	// Token: 0x06000E9F RID: 3743 RVA: 0x00044A08 File Offset: 0x00042C08
	public void StopAnimationControl()
	{
		this.RemoveUnlockRequest(HeroLockStates.AnimationLocked);
		int animationControlVersion = this.AnimationControlVersion;
		this.AnimationControlVersion = animationControlVersion + 1;
		this.animCtrl.StopControl();
	}

	// Token: 0x06000EA0 RID: 3744 RVA: 0x00044A37 File Offset: 0x00042C37
	public int StopAnimationControlVersioned()
	{
		this.StopAnimationControl();
		return this.AnimationControlVersion;
	}

	// Token: 0x06000EA1 RID: 3745 RVA: 0x00044A45 File Offset: 0x00042C45
	public void StartAnimationControl()
	{
		if (this.CheckAndRequestUnlock(HeroLockStates.AnimationLocked))
		{
			return;
		}
		this.RemoveUnlockRequest(HeroLockStates.AnimationLocked);
		this.animCtrl.StartControl();
	}

	// Token: 0x06000EA2 RID: 3746 RVA: 0x00044A63 File Offset: 0x00042C63
	public void StartAnimationControl(int version)
	{
		if (this.AnimationControlVersion != version)
		{
			return;
		}
		this.StartAnimationControl();
	}

	// Token: 0x06000EA3 RID: 3747 RVA: 0x00044A75 File Offset: 0x00042C75
	public void StartAnimationControlRunning()
	{
		this.animCtrl.StartControlRunning();
	}

	// Token: 0x06000EA4 RID: 3748 RVA: 0x00044A82 File Offset: 0x00042C82
	public void StartAnimationControlToIdle()
	{
		if (this.cState.onGround)
		{
			this.animCtrl.StartControlToIdle(false);
			return;
		}
		this.animCtrl.StartControl();
	}

	// Token: 0x06000EA5 RID: 3749 RVA: 0x00044AA9 File Offset: 0x00042CA9
	public void StartAnimationControlToIdleForcePlay()
	{
		if (this.cState.onGround)
		{
			this.animCtrl.StartControlToIdle(true);
			return;
		}
		this.animCtrl.StartControl();
	}

	// Token: 0x06000EA6 RID: 3750 RVA: 0x00044AD0 File Offset: 0x00042CD0
	public void IgnoreInput()
	{
		if (this.acceptingInput)
		{
			this.acceptingInput = false;
			this.ResetInput();
		}
	}

	// Token: 0x06000EA7 RID: 3751 RVA: 0x00044AE7 File Offset: 0x00042CE7
	public void IgnoreInputWithoutReset()
	{
		if (this.acceptingInput)
		{
			this.acceptingInput = false;
		}
	}

	// Token: 0x06000EA8 RID: 3752 RVA: 0x00044AF8 File Offset: 0x00042CF8
	public void AcceptInput()
	{
		if (!this.isHeroInPosition)
		{
			return;
		}
		if (this.cState.transitioning)
		{
			return;
		}
		if (this.cState.hazardRespawning)
		{
			return;
		}
		this.acceptingInput = true;
	}

	// Token: 0x06000EA9 RID: 3753 RVA: 0x00044B26 File Offset: 0x00042D26
	public void Pause()
	{
		this.PauseInput();
		this.PauseAudio();
		this.JumpReleased();
		this.cState.isPaused = true;
	}

	// Token: 0x06000EAA RID: 3754 RVA: 0x00044B46 File Offset: 0x00042D46
	public void UnPause()
	{
		this.cState.isPaused = false;
		this.UnPauseAudio();
		this.UnPauseInput();
	}

	// Token: 0x06000EAB RID: 3755 RVA: 0x00044B60 File Offset: 0x00042D60
	public void NearBench(bool isNearBench)
	{
		this.cState.nearBench = isNearBench;
	}

	// Token: 0x06000EAC RID: 3756 RVA: 0x00044B6E File Offset: 0x00042D6E
	public void SetWalkZone(bool inWalkZone)
	{
		this.cState.inWalkZone = inWalkZone;
	}

	// Token: 0x06000EAD RID: 3757 RVA: 0x00044B7C File Offset: 0x00042D7C
	public void ResetState()
	{
		this.cState.Reset();
		this.heroBox.HeroBoxNormal();
	}

	// Token: 0x06000EAE RID: 3758 RVA: 0x00044B94 File Offset: 0x00042D94
	public void StopPlayingAudio()
	{
		this.audioCtrl.StopAllSounds();
	}

	// Token: 0x06000EAF RID: 3759 RVA: 0x00044BA1 File Offset: 0x00042DA1
	public void PauseAudio()
	{
		this.audioCtrl.PauseAllSounds();
	}

	// Token: 0x06000EB0 RID: 3760 RVA: 0x00044BAE File Offset: 0x00042DAE
	public void UnPauseAudio()
	{
		this.audioCtrl.UnPauseAllSounds();
	}

	// Token: 0x06000EB1 RID: 3761 RVA: 0x00044BBB File Offset: 0x00042DBB
	private void PauseInput()
	{
		if (this.acceptingInput)
		{
			this.acceptingInput = false;
		}
		this.lastInputState = new Vector2(this.move_input, this.vertical_input);
	}

	// Token: 0x06000EB2 RID: 3762 RVA: 0x00044BE4 File Offset: 0x00042DE4
	private void UnPauseInput()
	{
		if (!this.controlReqlinquished)
		{
			Vector2 vector = this.lastInputState;
			if (this.inputHandler.inputActions.Right.IsPressed)
			{
				this.move_input = this.lastInputState.x;
			}
			else if (this.inputHandler.inputActions.Left.IsPressed)
			{
				this.move_input = this.lastInputState.x;
			}
			else
			{
				this.rb2d.linearVelocity = new Vector2(0f, this.rb2d.linearVelocity.y);
				this.move_input = 0f;
			}
			this.vertical_input = this.lastInputState.y;
			this.acceptingInput = true;
		}
	}

	// Token: 0x06000EB3 RID: 3763 RVA: 0x00044C9F File Offset: 0x00042E9F
	public void SetCanSoftLand()
	{
		if (!this.canSoftLand)
		{
			this.canSoftLand = true;
			this.softLandTime = Time.frameCount + 1;
		}
	}

	// Token: 0x06000EB4 RID: 3764 RVA: 0x00044CBD File Offset: 0x00042EBD
	public bool TrySpawnSoftLandingPrefab()
	{
		if (this.CanSoftLand())
		{
			this.SpawnSoftLandingPrefab();
			return true;
		}
		return false;
	}

	// Token: 0x06000EB5 RID: 3765 RVA: 0x00044CD0 File Offset: 0x00042ED0
	public bool CanSoftLand()
	{
		return this.canSoftLand && Time.frameCount > this.softLandTime && this.cState.onGround && Time.time > this.preventSoftLandTimer;
	}

	// Token: 0x06000EB6 RID: 3766 RVA: 0x00044D03 File Offset: 0x00042F03
	public void PreventSoftLand(float duration)
	{
		this.canSoftLand = false;
		this.preventSoftLandTimer = Time.time + duration;
	}

	// Token: 0x06000EB7 RID: 3767 RVA: 0x00044D19 File Offset: 0x00042F19
	public void SpawnSoftLandingPrefab()
	{
		this.canSoftLand = false;
		this.softLandingEffectPrefab.Spawn(this.transform.position);
		this.vibrationCtrl.PlaySoftLand();
	}

	// Token: 0x06000EB8 RID: 3768 RVA: 0x00044D44 File Offset: 0x00042F44
	public void AffectedByGravity(bool gravityApplies)
	{
		if (gravityApplies && this.CheckAndRequestUnlock(HeroLockStates.GravityLocked))
		{
			return;
		}
		this.RemoveUnlockRequest(HeroLockStates.GravityLocked);
		this.IsGravityApplied = gravityApplies;
		if (this.rb2d.gravityScale > Mathf.Epsilon && !gravityApplies)
		{
			this.prevGravityScale = this.rb2d.gravityScale;
			this.rb2d.gravityScale = 0f;
			return;
		}
		if (this.rb2d.gravityScale <= Mathf.Epsilon && gravityApplies)
		{
			this.rb2d.gravityScale = this.prevGravityScale;
			this.prevGravityScale = 0f;
		}
	}

	// Token: 0x06000EB9 RID: 3769 RVA: 0x00044DD8 File Offset: 0x00042FD8
	public void TryRestoreGravity()
	{
		this.AffectedByGravity(true);
	}

	// Token: 0x06000EBA RID: 3770 RVA: 0x00044DE1 File Offset: 0x00042FE1
	public void ResetGravity()
	{
		this.rb2d.gravityScale = this.DEFAULT_GRAVITY;
	}

	// Token: 0x06000EBB RID: 3771 RVA: 0x00044DF4 File Offset: 0x00042FF4
	public void ResetVelocity()
	{
		this.rb2d.linearVelocity = Vector2.zero;
	}

	// Token: 0x06000EBC RID: 3772 RVA: 0x00044E06 File Offset: 0x00043006
	public void AddInputBlocker(object blocker)
	{
		this.inputBlockers.Add(blocker);
	}

	// Token: 0x06000EBD RID: 3773 RVA: 0x00044E15 File Offset: 0x00043015
	public void RemoveInputBlocker(object blocker)
	{
		this.inputBlockers.Remove(blocker);
	}

	// Token: 0x06000EBE RID: 3774 RVA: 0x00044E24 File Offset: 0x00043024
	public bool IsInputBlocked()
	{
		if (CheatManager.IsOpen)
		{
			return true;
		}
		foreach (object obj in this.inputBlockers)
		{
			Object @object = obj as Object;
			if (@object != null)
			{
				if (@object == null)
				{
					continue;
				}
			}
			else if (obj == null)
			{
				continue;
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000EBF RID: 3775 RVA: 0x00044E98 File Offset: 0x00043098
	private bool IsPressingOnlyDown()
	{
		return this.inputHandler.inputActions.Down.IsPressed && !this.inputHandler.inputActions.Right.IsPressed && !this.inputHandler.inputActions.Left.IsPressed;
	}

	// Token: 0x06000EC0 RID: 3776 RVA: 0x00044EF0 File Offset: 0x000430F0
	private void LookForInput()
	{
		if (this.IsInputBlocked() || this.gm.GameState != GameState.PLAYING)
		{
			this.move_input = 0f;
			this.vertical_input = 0f;
			return;
		}
		if (this.gm.isPaused || !this.isGameplayScene)
		{
			return;
		}
		if (this.inputHandler.inputActions.SuperDash.WasPressed && this.IsPressingOnlyDown() && this.CanSuperJump())
		{
			if (this.controlReqlinquished)
			{
				EventRegister.SendEvent(EventRegisterEvents.FsmCancel, null);
				this.RegainControl();
				this.StartAnimationControlToIdle();
			}
			this.superJumpFSM.SendEventSafe("DO MOVE");
		}
		if (!this.acceptingInput)
		{
			return;
		}
		this.UpdateMoveInput();
		this.vertical_input = this.inputHandler.inputActions.MoveVector.Vector.y;
		this.FilterInput();
		if (this.CanWallSlide() && !this.cState.attacking)
		{
			this.BeginWallSlide(true);
		}
		HeroActions inputActions = this.inputHandler.inputActions;
		if (this.cState.wallSliding && inputActions.Down.WasPressed && (!this.touchingWallL || !inputActions.Left.IsPressed) && (!this.touchingWallR || !inputActions.Right.IsPressed))
		{
			this.CancelWallsliding();
			this.FlipSprite();
		}
		if (this.wallLocked && this.wallJumpedL && inputActions.Right.IsPressed && this.wallLockSteps >= this.WJLOCK_STEPS_SHORT)
		{
			this.wallLocked = false;
		}
		if (this.wallLocked && this.wallJumpedR && inputActions.Left.IsPressed && this.wallLockSteps >= this.WJLOCK_STEPS_SHORT)
		{
			this.wallLocked = false;
		}
		if (this.inputHandler.inputActions.Jump.WasReleased)
		{
			if (this.jumpReleaseQueueingEnabled)
			{
				this.jumpReleaseQueueSteps = this.JUMP_RELEASE_QUEUE_STEPS;
				this.jumpReleaseQueuing = true;
			}
			if (this.cState.floating)
			{
				this.cState.floating = false;
			}
		}
		if (!this.inputHandler.inputActions.Jump.IsPressed)
		{
			this.JumpReleased();
		}
		if (!this.inputHandler.inputActions.Dash.IsPressed)
		{
			if (this.cState.preventDash && !this.cState.dashCooldown)
			{
				this.cState.preventDash = false;
			}
			this.dashQueuing = false;
		}
		if (!this.inputHandler.inputActions.Attack.IsPressed)
		{
			this.attackQueuing = false;
		}
		if (!this.inputHandler.inputActions.SuperDash.IsPressed)
		{
			this.harpoonQueuing = false;
		}
		if (!this.inputHandler.inputActions.QuickCast.IsPressed)
		{
			this.toolThrowQueueing = false;
		}
	}

	// Token: 0x06000EC1 RID: 3777 RVA: 0x000451AD File Offset: 0x000433AD
	private void ForceTouchingWall()
	{
		this.touchingWallL = !this.cState.facingRight;
		this.touchingWallR = this.cState.facingRight;
		this.cState.touchingWall = true;
	}

	// Token: 0x06000EC2 RID: 3778 RVA: 0x000451E0 File Offset: 0x000433E0
	private void BeginWallSlide(bool requireInput)
	{
		if (this.cState.wallSliding)
		{
			return;
		}
		bool flag = false;
		if (this.touchingWallL && (!requireInput || this.inputHandler.inputActions.Left.IsPressed))
		{
			this.wallSlidingL = true;
			this.wallSlidingR = false;
			this.FaceLeft();
			flag = true;
		}
		if (this.touchingWallR && (!requireInput || this.inputHandler.inputActions.Right.IsPressed))
		{
			this.wallSlidingL = false;
			this.wallSlidingR = true;
			this.FaceRight();
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (this.touchingWallObj)
		{
			FSMUtility.SendEventUpwards(this.touchingWallObj, "HERO WALLSLIDE");
		}
		if (this.cState.shuttleCock)
		{
			this.ShuttleCockCancelInert();
			float num;
			if (this.jump_steps > 0)
			{
				num = 1f - (float)this.jump_steps / (float)this.JUMP_STEPS;
				if (num < 0.55f)
				{
					num = 0.55f;
				}
			}
			else
			{
				num = 0.55f;
			}
			this.wallStickStartVelocity = this.WALLSLIDE_SHUTTLECOCK_VEL * num;
		}
		else
		{
			this.wallStickStartVelocity = 0f;
		}
		this.CancelJump();
		if (this.cState.dashing)
		{
			this.CancelDash(true);
		}
		this.airDashed = false;
		this.doubleJumped = false;
		this.cState.wallSliding = true;
		this.AffectedByGravity(false);
		this.cState.willHardLand = false;
		this.extraAirMoveVelocities.Clear();
		this.wallslideDustPrefab.emission.enabled = true;
		this.dashCooldownTimer = 0f;
		this.CancelFallEffects();
		this.heroBox.HeroBoxWallSlide();
	}

	// Token: 0x06000EC3 RID: 3779 RVA: 0x00045374 File Offset: 0x00043574
	public void WallKickoff()
	{
		float num;
		if (this.cState.wallSliding)
		{
			num = 10f;
			this.FlipSprite();
		}
		else
		{
			if (!this.cState.touchingWall)
			{
				return;
			}
			num = 8f;
			if (this.cState.facingRight)
			{
				if (this.touchingWallR)
				{
					this.FlipSprite();
				}
			}
			else if (this.touchingWallL)
			{
				this.FlipSprite();
			}
		}
		this.rb2d.linearVelocity = new Vector2(this.cState.facingRight ? num : (-num), this.rb2d.linearVelocity.y);
	}

	// Token: 0x06000EC4 RID: 3780 RVA: 0x0004540F File Offset: 0x0004360F
	private void TryDoWallJumpFromMove()
	{
		if (!this.CanWallJump(false))
		{
			return;
		}
		this.ForceTouchingWall();
		this.DoWallJump();
	}

	// Token: 0x06000EC5 RID: 3781 RVA: 0x00045428 File Offset: 0x00043628
	private void LookForQueueInput()
	{
		if (this.IsInputBlocked())
		{
			return;
		}
		if (InteractManager.BlockingInteractable != null)
		{
			return;
		}
		if (this.IsPaused() || !this.isGameplayScene)
		{
			return;
		}
		if (this.inputHandler.inputActions.SuperDash.WasPressed && (!this.IsPressingOnlyDown() || !this.CanSuperJump()))
		{
			if (this.CanHarpoonDash())
			{
				this.IncrementAttackCounter();
				this.startWithHarpoonBounce = false;
				this.harpoonDashFSM.SendEventSafe("DO MOVE");
				this.harpoonQueuing = false;
			}
			else
			{
				this.harpoonQueueSteps = 0;
				this.harpoonQueuing = true;
			}
		}
		else if (this.inputHandler.inputActions.SuperDash.IsPressed && this.harpoonQueueSteps <= this.HARPOON_QUEUE_STEPS && this.CanHarpoonDash() && this.harpoonQueuing)
		{
			this.IncrementAttackCounter();
			this.startWithHarpoonBounce = false;
			this.harpoonDashFSM.SendEventSafe("DO MOVE");
			this.harpoonQueuing = false;
		}
		if (this.acceptingInput && this.queuedWallJumpInterrupt)
		{
			this.TryDoWallJumpFromMove();
		}
		else if (this.inputHandler.inputActions.Jump.WasPressed)
		{
			if (this.acceptingInput && this.CanWallJump(true))
			{
				this.DoWallJump();
			}
			else if (this.acceptingInput && this.CanJump())
			{
				this.HeroJump();
			}
			else if (this.acceptingInput && !this.wallLocked && !this.cState.swimming && this.CanDoubleJump(true))
			{
				this.DoDoubleJump();
			}
			else if (this.acceptingInput && this.CanInfiniteAirJump())
			{
				this.CancelJump();
				this.audioCtrl.PlaySound(HeroSounds.JUMP, true);
				this.ResetLook();
				this.cState.jumping = true;
				this.ResetAttacks(false);
			}
			else if (this.acceptingInput && !this.wallLocked && this.CanFloat(true) && !SlideSurface.IsInJumpGracePeriod)
			{
				this.StartFloat();
			}
			else if (!this.TryQueueWallJumpInterrupt())
			{
				this.jumpQueueSteps = 0;
				this.jumpQueuing = true;
				if (!this.cState.swimming && !this.cState.jumping && !SlideSurface.IsHeroSliding)
				{
					this.doubleJumpQueueSteps = 0;
					this.doubleJumpQueuing = true;
				}
			}
		}
		if (this.inputHandler.inputActions.Dash.WasPressed)
		{
			if (this.acceptingInput && this.CanDash())
			{
				this.HeroDashPressed();
			}
			else
			{
				this.dashQueueSteps = 0;
				this.dashQueuing = true;
			}
		}
		if (this.inputHandler.inputActions.Attack.WasPressed)
		{
			if (this.acceptingInput && this.CanAttack())
			{
				this.DoAttack();
			}
			else
			{
				this.attackQueueSteps = 0;
				this.attackQueuing = true;
			}
		}
		if (this.inputHandler.inputActions.QuickCast.WasPressed)
		{
			if (this.acceptingInput && this.CanThrowTool(false))
			{
				if (this.GetWillThrowTool(true))
				{
					this.ThrowTool(false);
				}
			}
			else
			{
				this.toolThrowQueueSteps = 0;
				this.toolThrowQueueing = true;
			}
		}
		if (this.acceptingInput)
		{
			if (this.inputHandler.inputActions.Jump.IsPressed)
			{
				if (this.jumpQueueSteps <= this.JUMP_QUEUE_STEPS && this.jumpQueuing && this.CanJump())
				{
					this.HeroJump();
				}
				else if (this.doubleJumpQueueSteps <= this.DOUBLE_JUMP_QUEUE_STEPS && this.doubleJumpQueuing && this.CanDoubleJump(true))
				{
					if (this.cState.onGround)
					{
						this.HeroJump();
					}
					else
					{
						this.DoDoubleJump();
					}
				}
				else if (this.regainControlJumpQueued && this.CanFloat(true))
				{
					this.StartFloat();
				}
				if (this.CanSwim() && this.hero_state != ActorStates.airborne)
				{
					this.SetState(ActorStates.airborne);
				}
			}
			if (this.inputHandler.inputActions.Dash.IsPressed && this.dashQueueSteps <= this.DASH_QUEUE_STEPS && this.dashQueuing && this.CanDash())
			{
				this.HeroDashPressed();
			}
			if (this.inputHandler.inputActions.Attack.IsPressed && this.attackQueueSteps <= this.ATTACK_QUEUE_STEPS && this.CanAttack() && this.attackQueuing)
			{
				this.DoAttack();
			}
			if (this.inputHandler.inputActions.QuickCast.IsPressed && this.toolThrowQueueing && this.toolThrowQueueSteps <= this.TOOLTHROW_QUEUE_STEPS && this.CanThrowTool())
			{
				this.ThrowTool(false);
			}
			this.regainControlJumpQueued = false;
			return;
		}
		if (this.inputHandler.inputActions.Jump.WasPressed && Time.frameCount != this.controlRelinquishedFrame)
		{
			this.jumpPressedWhileRelinquished = true;
			return;
		}
		if (!this.inputHandler.inputActions.Jump.IsPressed)
		{
			this.jumpPressedWhileRelinquished = false;
		}
	}

	// Token: 0x06000EC6 RID: 3782 RVA: 0x000458DC File Offset: 0x00043ADC
	public void ResetInputQueues()
	{
		this.jumpQueuing = false;
		this.jumpQueueSteps = 0;
		this.doubleJumpQueuing = false;
		this.doubleJumpQueueSteps = 0;
		this.dashQueuing = false;
		this.dashQueueSteps = 0;
		this.attackQueuing = false;
		this.attackQueueSteps = 0;
		this.toolThrowQueueing = false;
		this.toolThrowQueueSteps = 0;
		this.harpoonQueuing = false;
		this.harpoonQueueSteps = 0;
	}

	// Token: 0x06000EC7 RID: 3783 RVA: 0x0004593D File Offset: 0x00043B3D
	private void HeroJump()
	{
		this.HeroJump(true);
	}

	// Token: 0x06000EC8 RID: 3784 RVA: 0x00045946 File Offset: 0x00043B46
	private void ResetStartWithJumps()
	{
		this.startWithAnyJump = false;
		this.startWithShuttlecock = false;
		this.startWithDoubleJump = false;
		this.startWithFlipJump = false;
		this.startWithBackflipJump = false;
		this.startWithFullJump = false;
	}

	// Token: 0x06000EC9 RID: 3785 RVA: 0x00045972 File Offset: 0x00043B72
	public void PreventShuttlecock()
	{
		this.noShuttlecockTime = Time.timeAsDouble + 0.30000001192092896;
	}

	// Token: 0x06000ECA RID: 3786 RVA: 0x0004598C File Offset: 0x00043B8C
	private void HeroJump(bool checkSprint)
	{
		this.animCtrl.UpdateWallScramble();
		this.cState.downSpikeRecovery = false;
		this.ResetStartWithJumps();
		if (checkSprint && (this.sprintBufferSteps > 0 || this.cState.dashing || this.cState.isSprinting) && Time.timeAsDouble > this.noShuttlecockTime)
		{
			this.OnShuttleCockJump();
		}
		Vector3 position = this.transform.position;
		this.jumpEffectPrefab.Spawn(position).Play(base.gameObject, this.rb2d.linearVelocity, Vector3.zero);
		this.audioCtrl.PlaySound(HeroSounds.JUMP, true);
		this.gruntAudioTable.SpawnAndPlayOneShot(position, false);
		this.ResetLook();
		this.CancelDoubleJump();
		this.animCtrl.ResetPlaying();
		this.cState.recoiling = false;
		this.ClearJumpInputState();
		this.cState.jumping = true;
		this.jumpQueueSteps = 0;
		this.jumped_steps = 0;
		this.dashCooldownTimer = 0.05f;
		this.isDashStabBouncing = false;
		this.sprintBufferSteps = 0;
		this.syncBufferSteps = false;
		this.doubleJumpQueuing = false;
		if (this.cState.attacking && this.attack_time >= this.Config.AttackRecoveryTime)
		{
			this.CancelAttack();
		}
		this.OnHeroJumped();
		this.BecomeAirborne();
		this.ResetHardLandingTimer();
	}

	// Token: 0x06000ECB RID: 3787 RVA: 0x00045ADF File Offset: 0x00043CDF
	private void OnHeroJumped()
	{
		if (this.cState.onGround || this.cState.wallClinging || this.cState.wallSliding)
		{
			this.airDashed = false;
			this.doubleJumped = false;
			this.allowAttackCancellingDownspikeRecovery = false;
		}
	}

	// Token: 0x06000ECC RID: 3788 RVA: 0x00045B20 File Offset: 0x00043D20
	private void BecomeAirborne()
	{
		bool onGround = this.cState.onGround;
		this.animCtrl.ResetPlays();
		if (onGround)
		{
			this.SetCanSoftLand();
		}
		this.cState.onGround = false;
		this.SetState(ActorStates.airborne);
		this.animCtrl.UpdateState(this.hero_state);
		if (onGround || this.CheckTouchingGround())
		{
			Vector2 linearVelocity = this.rb2d.linearVelocity;
			if (linearVelocity.y < 0f)
			{
				this.rb2d.linearVelocity = new Vector2(linearVelocity.x, 0f);
			}
			this.rb2d.position += new Vector2(0f, Physics2D.defaultContactOffset * 2f);
		}
	}

	// Token: 0x06000ECD RID: 3789 RVA: 0x00045BD9 File Offset: 0x00043DD9
	private void HeroJumpNoEffect()
	{
		this.ClearJumpInputState();
		this.ResetLook();
		this.jump_steps = 5;
		this.cState.jumping = true;
		this.jumpQueueSteps = 0;
		this.jumped_steps = 0;
		this.OnHeroJumped();
	}

	// Token: 0x06000ECE RID: 3790 RVA: 0x00045C0E File Offset: 0x00043E0E
	public void ClearActionsInputState()
	{
		this.inputHandler.inputActions.ClearInputState();
	}

	// Token: 0x06000ECF RID: 3791 RVA: 0x00045C20 File Offset: 0x00043E20
	public void ClearJumpInputState()
	{
		this.inputHandler.inputActions.Jump.ClearInputState();
		this.jumpPressedWhileRelinquished = false;
	}

	// Token: 0x06000ED0 RID: 3792 RVA: 0x00045C40 File Offset: 0x00043E40
	private void DoWallJump()
	{
		this.queuedWallJumpInterrupt = false;
		if (this.wallPuffPrefab)
		{
			if (this.wallPuffPrefab.activeInHierarchy)
			{
				FSMUtility.SendEventToGameObject(this.wallPuffPrefab, "DEACTIVATE", false);
			}
			this.wallPuffPrefab.SetActive(true);
			bool flag = this.matchScale != null;
			if (!flag)
			{
				this.matchScale = this.wallPuffPrefab.GetComponent<MatchXScaleSignOnEnable>();
				flag = (this.matchScale != null);
			}
			if (flag)
			{
				if (this.wallSlidingR || this.touchingWallR)
				{
					this.matchScale.SetTargetSign(false);
				}
				else if (this.wallSlidingL || this.touchingWallL)
				{
					this.matchScale.SetTargetSign(true);
				}
			}
		}
		if (!this.playedMantisClawClip)
		{
			this.audioSource.PlayOneShot(this.mantisClawClip, 1f);
			this.playedMantisClawClip = true;
		}
		this.audioCtrl.PlaySound(HeroSounds.WALLJUMP, false);
		this.vibrationCtrl.PlayWallJump();
		if (this.touchingWallL)
		{
			this.FaceRight();
			this.wallJumpedR = true;
			this.wallJumpedL = false;
		}
		else if (this.touchingWallR)
		{
			this.FaceLeft();
			this.wallJumpedR = false;
			this.wallJumpedL = true;
		}
		if (this.touchingWallObj)
		{
			FSMUtility.SendEventUpwards(this.touchingWallObj, "HERO WALLSLIDE");
		}
		this.CancelWallsliding();
		this.cState.touchingWall = false;
		this.touchingWallL = false;
		this.touchingWallR = false;
		this.touchingWallObj = null;
		this.airDashed = false;
		this.doubleJumped = false;
		this.ShuttleCockCancel();
		this.cState.mantleRecovery = false;
		this.currentWalljumpSpeed = this.WJ_KICKOFF_SPEED;
		this.walljumpSpeedDecel = (this.WJ_KICKOFF_SPEED - this.GetRunSpeed()) / (float)this.WJLOCK_STEPS_LONG;
		this.cState.jumping = true;
		this.wallLockSteps = 0;
		this.wallLocked = true;
		this.jumpQueueSteps = 0;
		this.jumpQueuing = false;
		this.jumped_steps = 5;
		this.doubleJumpQueuing = false;
		this.animCtrl.SetWallJumped();
	}

	// Token: 0x06000ED1 RID: 3793 RVA: 0x00045E3C File Offset: 0x0004403C
	private void DoDoubleJump()
	{
		if (SlideSurface.IsInJumpGracePeriod)
		{
			return;
		}
		if (this.cState.inUpdraft && this.CanFloat(true))
		{
			this.fsm_brollyControl.SendEvent("FORCE UPDRAFT ENTER");
			return;
		}
		if (this.shuttleCockJumpSteps > 0)
		{
			return;
		}
		if (this.cState.dashing && this.dashingDown)
		{
			this.FinishedDashing(true);
		}
		if (this.cState.jumping)
		{
			this.Jump();
		}
		this.doubleJumpEffectPrefab.Spawn(this.transform, Vector3.zero);
		if (Gameplay.BrollySpikeTool.IsEquipped)
		{
			GameObject gameObject = Gameplay.BrollySpikeObject_dj.Spawn(this.transform);
			gameObject.transform.Translate(0f, 0f, -0.001f);
			gameObject.transform.Rotate(0f, 0f, -10f);
		}
		this.vibrationCtrl.PlayDoubleJump();
		if (this.audioSource && this.doubleJumpClip)
		{
			this.audioSource.PlayOneShot(this.doubleJumpClip, 1f);
		}
		Vector2 linearVelocity = this.rb2d.linearVelocity;
		if (linearVelocity.y < -this.MAX_FALL_VELOCITY_DJUMP)
		{
			this.rb2d.linearVelocity = new Vector2(linearVelocity.x, -this.MAX_FALL_VELOCITY_DJUMP);
		}
		this.ShuttleCockCancel();
		this.ResetLook();
		this.startWithDownSpikeBounceShort = false;
		this.cState.downSpikeBouncingShort = false;
		this.startWithDownSpikeBounce = false;
		this.cState.downSpikeBouncing = false;
		this.cState.jumping = false;
		this.cState.doubleJumping = true;
		this.cState.downSpikeRecovery = false;
		this.animCtrl.AllowDoubleJumpReEntry();
		if (this.jumped_steps < this.JUMP_STEPS_MIN)
		{
			this.jumped_steps = this.JUMP_STEPS_MIN;
		}
		this.doubleJump_steps = 0;
		this.doubleJumped = true;
		this.ResetHardLandingTimer();
		Action onDoubleJumped = this.OnDoubleJumped;
		if (onDoubleJumped == null)
		{
			return;
		}
		onDoubleJumped();
	}

	// Token: 0x06000ED2 RID: 3794 RVA: 0x00046029 File Offset: 0x00044229
	public void SetBlockFootstepAudio(bool blockFootStep)
	{
		if (this.audioCtrl != null)
		{
			this.audioCtrl.BlockFootstepAudio = blockFootStep;
		}
	}

	// Token: 0x06000ED3 RID: 3795 RVA: 0x00046045 File Offset: 0x00044245
	private void StartFloat()
	{
		this.CancelQueuedBounces();
		this.umbrellaFSM.SendEvent("FLOAT");
	}

	// Token: 0x06000ED4 RID: 3796 RVA: 0x00046060 File Offset: 0x00044260
	public void DoHardLanding()
	{
		if (this.cState.hazardRespawning || this.cState.hazardDeath)
		{
			return;
		}
		if (this.cState.dashing)
		{
			this.CancelDash(true);
		}
		this.sprintFSM.SendEvent("HARD LANDING");
		this.BackOnGround(false);
		this.AffectedByGravity(true);
		this.ResetInput();
		this.SetState(ActorStates.hard_landing);
		this.CancelAttack();
		this.hardLanded = true;
		this.DoHardLandingEffect();
	}

	// Token: 0x06000ED5 RID: 3797 RVA: 0x000460DA File Offset: 0x000442DA
	public void DoHardLandingEffect()
	{
		this.DoHardLandingEffectNoHit();
		DeliveryQuestItem.TakeHit();
	}

	// Token: 0x06000ED6 RID: 3798 RVA: 0x000460E7 File Offset: 0x000442E7
	public void DoHardLandingEffectNoHit()
	{
		this.audioCtrl.PlaySound(HeroSounds.HARD_LANDING, true);
		this.hardLandingEffectPrefab.Spawn(this.transform.position);
	}

	// Token: 0x06000ED7 RID: 3799 RVA: 0x00046110 File Offset: 0x00044310
	private void HeroDashPressed()
	{
		ToolItem scuttleCharmTool = Gameplay.ScuttleCharmTool;
		if (this.inputHandler.inputActions.Down.IsPressed && !this.inputHandler.inputActions.Left.IsPressed && !this.inputHandler.inputActions.Right.IsPressed && scuttleCharmTool.IsEquipped)
		{
			this.dashQueueSteps = 0;
			this.ResetAttacksDash();
			this.CancelBounce();
			this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN, true);
			this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK, true);
			this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_SPRINT, true);
			this.ResetLook();
			this.lookDownBlocked = true;
			this.gruntAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
			this.toolEventTarget.SendEventSafe("TAKE CONTROL");
			this.toolEventTarget.SendEvent("SCUTTLE");
			return;
		}
		if (this.CanWallScramble())
		{
			this.wallScrambleFSM.enabled = true;
			this.wallScrambleFSM.SendEvent("SCRAMBLE");
			return;
		}
		this.HeroDash(false);
	}

	// Token: 0x06000ED8 RID: 3800 RVA: 0x0004622C File Offset: 0x0004442C
	private void HeroDash(bool startAlreadyDashing)
	{
		HeroActions inputActions = this.inputHandler.inputActions;
		this.ResetAttacksDash();
		this.CancelBounce();
		this.CancelHeroJump();
		this.ShuttleCockCancelInert();
		this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN, true);
		this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK, true);
		this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_SPRINT, true);
		this.startWithDash = false;
		if (!startAlreadyDashing)
		{
			bool flag = !this.cState.onGround;
			this.audioCtrl.PlaySound(HeroSounds.DASH, !flag);
			this.gruntAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
			this.dashingDown = (inputActions.Down.IsPressed && !this.cState.onGround && !inputActions.Left.IsPressed && !inputActions.Right.IsPressed);
			if (flag)
			{
				this.vibrationCtrl.PlayAirDash();
			}
		}
		this.ResetLook();
		if (this.dashingDown && this.cState.jumping && this.jump_steps == 0)
		{
			this.dashingDown = false;
		}
		if (this.dashingDown)
		{
			this.onFlatGround = true;
			this.tryShove = false;
		}
		if (this.cState.onGround && !this.dashingDown)
		{
			this.dash_timer = this.DASH_TIME;
			this.cState.airDashing = false;
			this.dash_time = 0f;
		}
		else
		{
			if (startAlreadyDashing)
			{
				this.dash_timer = 0f;
			}
			else
			{
				this.dash_timer = (this.dashingDown ? this.DOWN_DASH_TIME : this.AIR_DASH_TIME);
				this.dash_time = 0f;
			}
			this.cState.airDashing = true;
			this.airDashed = true;
		}
		this.cState.recoiling = false;
		if (this.cState.wallSliding)
		{
			this.FlipSprite();
		}
		else if (!this.dashCurrentFacing)
		{
			if (inputActions.Right.IsPressed && !inputActions.Left.IsPressed)
			{
				this.FaceRight();
			}
			else if (inputActions.Left.IsPressed && !inputActions.Right.IsPressed)
			{
				this.FaceLeft();
			}
		}
		this.cState.dashing = true;
		this.dashQueueSteps = 0;
		this.wallLocked = false;
		if (!startAlreadyDashing)
		{
			this.StartDashEffect();
			if (this.cState.onGround && !this.cState.shadowDashing)
			{
				Transform transform = this.transform;
				Vector3 localScale = transform.localScale;
				this.dashEffect = this.backDashPrefab.Spawn(transform.position);
				this.dashEffect.transform.localScale = new Vector3(localScale.x * -1f, localScale.y, localScale.z);
				this.dashEffect.Play(base.gameObject);
			}
		}
		this.SetDashCooldownTimer();
		this.sprintFSM.SendEvent("DASHED");
	}

	// Token: 0x06000ED9 RID: 3801 RVA: 0x000464FD File Offset: 0x000446FD
	public void SetDashCooldownTimer()
	{
		this.dashCooldownTimer = this.DASH_COOLDOWN;
	}

	// Token: 0x06000EDA RID: 3802 RVA: 0x0004650C File Offset: 0x0004470C
	public void StartDashEffect()
	{
		if (this.cState.wallSliding)
		{
			this.walldashKickoffEffect.SetActive(false);
			this.walldashKickoffEffect.SetActive(true);
			return;
		}
		float num = 1f;
		if (this.IsUsingQuickening)
		{
			num += 0.25f;
		}
		if (Gameplay.SprintmasterTool.IsEquipped)
		{
			num += 0.25f;
		}
		if (!Mathf.Approximately(this.sprintSpeedAddFloat.Value, 0f))
		{
			num += 0.25f;
		}
		if (this.cState.onGround)
		{
			this.dashBurstPrefab.transform.localScale = new Vector3(-num, num, num);
			this.dashBurstPrefab.SetActive(false);
			this.dashBurstPrefab.SetActive(true);
			return;
		}
		if (this.dashingDown)
		{
			this.airDashEffect.transform.SetLocalRotation2D(90f);
		}
		else
		{
			this.airDashEffect.transform.SetLocalRotation2D(0f);
		}
		this.airDashEffect.transform.localScale = new Vector3(num, num, num);
		this.airDashEffect.SetActive(false);
		this.airDashEffect.SetActive(true);
	}

	// Token: 0x06000EDB RID: 3803 RVA: 0x0004662E File Offset: 0x0004482E
	private void StartFallRumble()
	{
		this.fallRumble = true;
		this.audioCtrl.PlaySound(HeroSounds.FALLING, false);
		GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = true;
	}

	// Token: 0x06000EDC RID: 3804 RVA: 0x00046669 File Offset: 0x00044869
	public bool IsOnWall()
	{
		return this.cState.wallSliding || this.cState.wallClinging || this.cState.wallScrambling;
	}

	// Token: 0x06000EDD RID: 3805 RVA: 0x00046692 File Offset: 0x00044892
	private bool CanExitNoInput()
	{
		return !this.doingHazardRespawn && !this.cState.dead;
	}

	// Token: 0x06000EDE RID: 3806 RVA: 0x000466AC File Offset: 0x000448AC
	private void SetState(ActorStates newState)
	{
		if (this.hero_state == ActorStates.no_input && !this.CanExitNoInput())
		{
			return;
		}
		switch (newState)
		{
		case ActorStates.grounded:
			newState = ((Mathf.Abs(this.move_input) > Mathf.Epsilon) ? ActorStates.running : ActorStates.idle);
			this.heroBox.HeroBoxNormal();
			break;
		case ActorStates.idle:
		case ActorStates.running:
		case ActorStates.airborne:
			if (!this.cState.wallSliding && !this.cState.wallClinging)
			{
				this.heroBox.HeroBoxNormal();
			}
			break;
		case ActorStates.previous:
			newState = this.prev_hero_state;
			break;
		}
		if (newState != this.hero_state)
		{
			this.prev_hero_state = this.hero_state;
			this.hero_state = newState;
			this.animCtrl.UpdateState(newState);
		}
	}

	// Token: 0x06000EDF RID: 3807 RVA: 0x00046774 File Offset: 0x00044974
	private void FinishedEnteringScene(bool setHazardMarker = true, bool preventRunBob = false)
	{
		if (this.isEnteringFirstLevel)
		{
			this.isEnteringFirstLevel = false;
		}
		else
		{
			this.playerData.disablePause = false;
		}
		this.animCtrl.waitingToEnter = false;
		Vector3 position = this.transform.position;
		this.doingHazardRespawn = false;
		this.cState.transitioning = false;
		this.transitionState = HeroTransitionState.WAITING_TO_TRANSITION;
		this.stopWalkingOut = false;
		this.ForceRunningSound = false;
		this.ForceWalkingSound = false;
		if (this.exitedSuperDashing || this.exitedQuake || this.exitedSprinting)
		{
			this.controlReqlinquished = true;
			this.IgnoreInput();
		}
		else
		{
			this.SetStartingMotionState(preventRunBob);
			this.AffectedByGravity(true);
		}
		if (setHazardMarker)
		{
			if (this.gm.startedOnThisScene || this.sceneEntryGate == null)
			{
				this.playerData.SetHazardRespawn(position, this.cState.facingRight);
			}
			else if (!this.sceneEntryGate.nonHazardGate)
			{
				this.playerData.SetHazardRespawn(this.sceneEntryGate.respawnMarker);
			}
		}
		if (this.exitedQuake)
		{
			this.SetDamageMode(DamageMode.HAZARD_ONLY);
		}
		else
		{
			this.SetDamageMode(DamageMode.FULL_DAMAGE);
		}
		if (this.enterWithoutInput || this.exitedSuperDashing || this.exitedQuake || this.exitedSprinting)
		{
			this.enterWithoutInput = false;
		}
		else
		{
			this.AcceptInput();
		}
		this.SetSilkRegenBlocked(false);
		this.gm.FinishedEnteringScene();
		this.ResetSceneExitedStates();
		this.positionHistory[0] = position;
		this.positionHistory[1] = position;
		this.tilemapTestActive = true;
		if (this.sceneEntryGate)
		{
			this.sceneEntryGate.AfterEntry();
		}
		InteractManager.SetEnabledDelay(0.5f);
		this.skipNormalEntry = false;
	}

	// Token: 0x06000EE0 RID: 3808 RVA: 0x00046927 File Offset: 0x00044B27
	public void ResetSceneExitedStates()
	{
		this.exitedSuperDashing = false;
		this.exitedQuake = false;
		this.exitedSprinting = false;
	}

	// Token: 0x06000EE1 RID: 3809 RVA: 0x0004693E File Offset: 0x00044B3E
	private IEnumerator Die(bool nonLethal, bool frostDeath)
	{
		if (this.hazardRespawnRoutine != null)
		{
			base.StopCoroutine(this.hazardRespawnRoutine);
			this.hazardRespawnRoutine = null;
		}
		this.ResetSilkRegen();
		this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK, true);
		this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN, true);
		Action onDeath = this.OnDeath;
		if (onDeath != null)
		{
			onDeath();
		}
		DeliveryQuestItem.BreakAll();
		if (this.cState.dead)
		{
			yield break;
		}
		EventRegister.SendEvent(EventRegisterEvents.HeroDeath, null);
		EventRegister.SendEvent(EventRegisterEvents.FsmCancel, null);
		this.playerData.disablePause = true;
		this.boundsChecking = false;
		this.updraftsEntered = 0;
		this.StopTilemapTest();
		this.cState.onConveyor = false;
		this.cState.onConveyorV = false;
		this.rb2d.linearVelocity = new Vector2(0f, 0f);
		this.CancelRecoilHorizontal();
		bool flag = this.gm.IsMemoryScene();
		if (flag)
		{
			nonLethal = true;
		}
		if (nonLethal)
		{
			if (!string.IsNullOrEmpty(this.playerData.nonLethalRespawnMarker))
			{
				this.playerData.tempRespawnMarker = this.playerData.nonLethalRespawnMarker;
				this.playerData.nonLethalRespawnMarker = null;
			}
			if (this.playerData.nonLethalRespawnType != 0)
			{
				this.playerData.tempRespawnType = this.playerData.nonLethalRespawnType;
				this.playerData.nonLethalRespawnType = 0;
			}
			if (!string.IsNullOrEmpty(this.playerData.nonLethalRespawnScene))
			{
				this.playerData.tempRespawnScene = this.playerData.nonLethalRespawnScene;
				this.playerData.nonLethalRespawnScene = null;
			}
		}
		else if (this.playerData.permadeathMode == PermadeathModes.On)
		{
			this.playerData.permadeathMode = PermadeathModes.Dead;
		}
		this.AffectedByGravity(false);
		HeroBox.Inactive = true;
		this.cState.falling = false;
		this.rb2d.isKinematic = true;
		this.SetState(ActorStates.no_input);
		this.cState.dead = true;
		this.cState.isFrostDeath = frostDeath;
		this.ResetMotion(true);
		this.ResetHardLandingTimer();
		this.renderer.enabled = false;
		base.gameObject.layer = 2;
		this.heroBox.HeroBoxOff();
		GameObject gameObject = this.GetHeroDeathPrefab(nonLethal, flag, frostDeath);
		if (this.vibrationCtrl)
		{
			this.vibrationCtrl.PlayHeroDeath();
		}
		GameObject gameObject2 = gameObject.Spawn();
		gameObject2.transform.position = this.transform.position;
		gameObject2.transform.localScale = this.transform.localScale.MultiplyElements(gameObject.transform.localScale);
		gameObject2.SetActive(true);
		tk2dSpriteAnimator component = gameObject2.GetComponent<tk2dSpriteAnimator>();
		if (component)
		{
			component.Library = this.animCtrl.animator.Library;
		}
		if (!nonLethal)
		{
			HeroCorpseMarkerProxy instance = HeroCorpseMarkerProxy.Instance;
			if (instance)
			{
				this.playerData.HeroCorpseScene = instance.TargetSceneName;
				this.playerData.HeroCorpseMarkerGuid = instance.TargetGuid;
				this.playerData.HeroDeathScenePos = instance.TargetScenePos;
			}
			else
			{
				Vector3 position = this.transform.position;
				this.playerData.HeroCorpseScene = this.gm.GetSceneNameString();
				HeroCorpseMarker closest = HeroCorpseMarker.GetClosest(position);
				if (closest)
				{
					this.playerData.HeroCorpseMarkerGuid = closest.Guid.ToByteArray();
					this.playerData.HeroDeathScenePos = closest.Position;
				}
				else
				{
					this.playerData.HeroCorpseMarkerGuid = null;
					this.playerData.HeroDeathScenePos = position;
				}
			}
			tk2dTileMap tilemap = this.gm.tilemap;
			this.playerData.HeroDeathSceneSize = new Vector2((float)tilemap.width, (float)tilemap.height);
			this.gm.gameMap.PositionCompassAndCorpse();
			this.playerData.IsSilkSpoolBroken = true;
			this.playerData.HeroCorpseType = HeroDeathCocoonTypes.Normal;
			int num = this.playerData.geo;
			bool isEquipped = Gameplay.DeadPurseTool.IsEquipped;
			if (isEquipped)
			{
				int num2 = Mathf.RoundToInt((float)num * Gameplay.DeadPurseHoldPercent);
				num -= num2;
				this.playerData.geo = num2;
			}
			else
			{
				this.playerData.geo = 0;
			}
			this.playerData.HeroCorpseMoneyPool = Mathf.RoundToInt((float)num);
			if (this.playerData.IsAnyCursed)
			{
				this.playerData.HeroCorpseType |= HeroDeathCocoonTypes.Cursed;
			}
			if (isEquipped && this.playerData.HeroCorpseMoneyPool >= 10)
			{
				this.playerData.HeroCorpseType |= HeroDeathCocoonTypes.Rosaries;
			}
		}
		this.playerData.silk = 0;
		this.playerData.silkParts = 0;
		GameCameras.instance.silkSpool.RefreshSilk();
		this.ClearSpoolMossChunks();
		EventRegister.SendEvent("TOOL EQUIPS CHANGED", null);
		float deathWait = frostDeath ? 5.1f : this.DEATH_WAIT;
		HeroDeathSequence component2 = gameObject2.GetComponent<HeroDeathSequence>();
		if (component2 != null)
		{
			deathWait = component2.DeathWait;
		}
		yield return null;
		base.StartCoroutine(this.gm.PlayerDead(deathWait));
		if (!frostDeath)
		{
			yield return new WaitForSeconds(2.45f);
			this.frostAmount = 0f;
			StatusVignette.SetFrostVignetteAmount(this.frostAmount);
		}
		yield break;
	}

	// Token: 0x06000EE2 RID: 3810 RVA: 0x0004695C File Offset: 0x00044B5C
	private GameObject GetHeroDeathPrefab(bool nonLethal, bool inMemoryScene, bool isFrostDamage)
	{
		if (inMemoryScene && this.heroDeathMemoryPrefab)
		{
			return this.heroDeathMemoryPrefab;
		}
		if (nonLethal && this.heroDeathNonLethalPrefab)
		{
			return this.heroDeathNonLethalPrefab;
		}
		if (this.playerData.IsAnyCursed && this.heroDeathCursedPrefab)
		{
			return this.heroDeathCursedPrefab;
		}
		if (isFrostDamage && this.heroDeathFrostPrefab)
		{
			return this.heroDeathFrostPrefab;
		}
		return this.heroDeathPrefab;
	}

	// Token: 0x06000EE3 RID: 3811 RVA: 0x000469D5 File Offset: 0x00044BD5
	private void ElevatorReset()
	{
		this.SetHeroParent(null);
		if (this.rb2d.interpolation != RigidbodyInterpolation2D.Interpolate)
		{
			this.rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
		}
	}

	// Token: 0x06000EE4 RID: 3812 RVA: 0x000469F8 File Offset: 0x00044BF8
	private IEnumerator DieFromHazard(HazardType hazardType, float angle)
	{
		this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK, true);
		this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN, true);
		if (this.cState.hazardDeath)
		{
			yield break;
		}
		this.CancelDamageRecoil();
		this.playerData.disablePause = true;
		this.ElevatorReset();
		if (this.OnHazardDeath != null)
		{
			this.OnHazardDeath();
		}
		this.StopTilemapTest();
		this.SetState(ActorStates.no_input);
		this.cState.hazardDeath = true;
		this.ResetMotion(true);
		this.ResetHardLandingTimer();
		this.AffectedByGravity(false);
		this.renderer.enabled = false;
		base.gameObject.layer = 2;
		this.heroBox.HeroBoxOff();
		EventRegister.SendEvent(EventRegisterEvents.HeroHazardDeath, null);
		if (this.vibrationCtrl)
		{
			this.vibrationCtrl.PlayHeroHazardDeath();
		}
		float hazardDeathWait = 0f;
		if (hazardType == HazardType.SPIKES)
		{
			GameObject gameObject = this.spikeDeathPrefab.Spawn();
			gameObject.transform.position = this.transform.position;
			FSMUtility.SetFloat(gameObject.GetComponent<PlayMakerFSM>(), "Spike Direction", angle * 57.29578f);
		}
		else if (hazardType == HazardType.COAL_SPIKES)
		{
			GameObject gameObject2 = this.coalSpikeDeathPrefab.Spawn();
			gameObject2.transform.position = this.transform.position;
			FSMUtility.SetFloat(gameObject2.GetComponent<PlayMakerFSM>(), "Spike Direction", angle * 57.29578f);
		}
		else if (hazardType == HazardType.ACID)
		{
			GameObject gameObject3 = this.acidDeathPrefab.Spawn();
			gameObject3.transform.position = this.transform.position;
			gameObject3.transform.localScale = this.transform.localScale;
		}
		else if (hazardType == HazardType.LAVA)
		{
			GameObject gameObject4 = this.lavaDeathPrefab.Spawn();
			gameObject4.transform.position = this.transform.position;
			gameObject4.transform.localScale = this.transform.localScale;
			DeliveryQuestItem.BreakAll();
		}
		else if (hazardType == HazardType.COAL)
		{
			GameObject gameObject5 = this.coalDeathPrefab.Spawn();
			gameObject5.transform.position = this.transform.position;
			gameObject5.transform.localScale = this.transform.localScale;
		}
		else if (hazardType == HazardType.ZAP)
		{
			this.zapDeathPrefab.Spawn().transform.position = this.transform.position;
			hazardDeathWait = 0.5f;
		}
		else if (hazardType == HazardType.SINK)
		{
			GameObject gameObject6 = this.sinkDeathPrefab.Spawn();
			gameObject6.transform.position = this.transform.position;
			gameObject6.transform.localScale = this.transform.localScale;
		}
		else if (hazardType == HazardType.STEAM)
		{
			GameObject gameObject7 = this.steamDeathPrefab.Spawn();
			gameObject7.transform.position = this.transform.position;
			gameObject7.transform.localScale = this.transform.localScale;
		}
		if (hazardType == HazardType.LAVA)
		{
			this.hazardDamageAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
		}
		else if (hazardType == HazardType.RESPAWN_PIT || hazardType == HazardType.PIT)
		{
			this.pitFallAudioTable.SpawnAndPlayOneShot2D(this.transform.position, false);
		}
		else
		{
			this.woundAudioTable.SpawnAndPlayOneShot(this.transform.position, false);
		}
		yield return null;
		this.hazardRespawnRoutine = base.StartCoroutine(this.gm.PlayerDeadFromHazard(hazardDeathWait));
		yield break;
	}

	// Token: 0x06000EE5 RID: 3813 RVA: 0x00046A15 File Offset: 0x00044C15
	private IEnumerator StartRecoil(CollisionSide impactSide, int damageAmount)
	{
		if (!this.cState.recoiling)
		{
			float num = this.RECOIL_VELOCITY;
			float num2 = this.INVUL_TIME;
			ToolItem weightedAnkletTool = Gameplay.WeightedAnkletTool;
			if (weightedAnkletTool && weightedAnkletTool.IsEquipped)
			{
				num *= Gameplay.WeightedAnkletDmgKnockbackMult;
				num2 *= Gameplay.WeightedAnkletDmgInvulnMult;
			}
			this.playerData.disablePause = true;
			this.ResetMotion(true);
			this.AffectedByGravity(false);
			if (impactSide == CollisionSide.left)
			{
				this.recoilVector = new Vector2(num, num * 0.5f);
				if (this.cState.facingRight)
				{
					this.FlipSprite();
				}
			}
			else if (impactSide == CollisionSide.right)
			{
				this.recoilVector = new Vector2(-num, num * 0.5f);
				if (!this.cState.facingRight)
				{
					this.FlipSprite();
				}
			}
			else
			{
				this.recoilVector = Vector2.zero;
			}
			this.SetState(ActorStates.no_input);
			this.cState.recoilFrozen = true;
			this.cState.onGround = false;
			this.cState.wasOnGround = false;
			this.ledgeBufferSteps = 0;
			this.sprintBufferSteps = 0;
			this.syncBufferSteps = false;
			if (damageAmount > 0)
			{
				this.damageEffectFSM.SendEvent("DAMAGE");
			}
			this.StartInvulnerable(num2);
			yield return this.takeDamageCoroutine = base.StartCoroutine(this.gm.FreezeMoment(this.DAMAGE_FREEZE_DOWN, this.DAMAGE_FREEZE_WAIT, this.DAMAGE_FREEZE_UP, this.DAMAGE_FREEZE_SPEED, null));
			this.cState.recoilFrozen = false;
			this.cState.recoiling = true;
			if (!this.cState.dead)
			{
				this.renderer.enabled = true;
			}
			this.playerData.disablePause = false;
		}
		this.recoilRoutine = null;
		yield break;
	}

	// Token: 0x06000EE6 RID: 3814 RVA: 0x00046A34 File Offset: 0x00044C34
	private void StartInvulnerable(float duration)
	{
		if (this.hazardInvulnRoutine == null)
		{
			this.cState.invulnerable = true;
			this.invulnerableFreezeDuration = this.DAMAGE_FREEZE_DOWN;
			this.invulnerableDuration = duration;
			this.hazardInvulnRoutine = base.StartCoroutine(this.Invulnerable());
			return;
		}
		if (this.invulnerableFreezeDuration + this.invulnerableDuration < duration + this.DAMAGE_FREEZE_DOWN)
		{
			if (this.invulnerableFreezeDuration > 0f)
			{
				this.invulnerableFreezeDuration = this.DAMAGE_FREEZE_DOWN;
			}
			else
			{
				duration += this.DAMAGE_FREEZE_DOWN;
			}
			this.invulnerableDuration = duration;
		}
	}

	// Token: 0x06000EE7 RID: 3815 RVA: 0x00046ABE File Offset: 0x00044CBE
	private IEnumerator Invulnerable()
	{
		this.cState.invulnerable = true;
		while (this.invulnerableFreezeDuration > 0f)
		{
			yield return null;
			this.invulnerableFreezeDuration -= Time.deltaTime;
		}
		this.invPulse.StartInvulnerablePulse();
		while (this.invulnerableDuration > 0f)
		{
			yield return null;
			this.invulnerableDuration -= Time.deltaTime;
		}
		this.invPulse.StopInvulnerablePulse();
		this.cState.invulnerable = false;
		this.cState.recoiling = false;
		this.hazardInvulnRoutine = null;
		yield break;
	}

	// Token: 0x06000EE8 RID: 3816 RVA: 0x00046ACD File Offset: 0x00044CCD
	public void AddInvulnerabilitySource(object source)
	{
		this.cState.AddInvulnerabilitySource(source);
	}

	// Token: 0x06000EE9 RID: 3817 RVA: 0x00046ADB File Offset: 0x00044CDB
	public void RemoveInvulnerabilitySource(object source)
	{
		this.cState.RemoveInvulnerabilitySource(source);
	}

	// Token: 0x06000EEA RID: 3818 RVA: 0x00046AE9 File Offset: 0x00044CE9
	private IEnumerator FirstFadeIn()
	{
		yield return new WaitForSeconds(0.25f);
		this.gm.FadeSceneIn();
		this.fadedSceneIn = true;
		yield break;
	}

	// Token: 0x06000EEB RID: 3819 RVA: 0x00046AF8 File Offset: 0x00044CF8
	private void FallCheck()
	{
		if (this.rb2d.linearVelocity.y <= Mathf.Epsilon && !this.CheckTouchingGround())
		{
			this.cState.falling = true;
			this.cState.wallJumping = false;
			this.LeftGround(this.hero_state != ActorStates.no_input);
			if ((!this.controlReqlinquished || this.cState.isSprinting || (this.cState.isBinding && Gameplay.SpellCrest.IsEquipped)) && this.rb2d.linearVelocity.y <= -this.MAX_FALL_VELOCITY && !this.cState.wallSliding)
			{
				this.fallTimer += Time.deltaTime;
			}
			else
			{
				this.fallTimer = 0f;
			}
			if (this.fallTimer > this.BIG_FALL_TIME)
			{
				if (!this.cState.willHardLand)
				{
					this.cState.willHardLand = true;
				}
				if (!this.fallRumble)
				{
					this.StartFallRumble();
				}
			}
			if (this.fallCheckFlagged)
			{
				this.fallCheckFlagged = false;
				return;
			}
		}
		else
		{
			this.cState.falling = false;
			this.fallTimer = 0f;
			if (this.fallCheckFlagged)
			{
				this.fallCheckFlagged = false;
			}
			if (this.fallRumble)
			{
				this.CancelFallEffects();
			}
		}
	}

	// Token: 0x06000EEC RID: 3820 RVA: 0x00046C40 File Offset: 0x00044E40
	private void OutOfBoundsCheck()
	{
		if (this.isGameplayScene)
		{
			Vector2 vector = this.transform.position;
			if ((vector.y < -60f || vector.y > this.gm.sceneHeight + 60f || vector.x < -60f || vector.x > this.gm.sceneWidth + 60f) && !this.cState.dead)
			{
				bool flag = this.boundsChecking;
			}
		}
	}

	// Token: 0x06000EED RID: 3821 RVA: 0x00046CC8 File Offset: 0x00044EC8
	private void ConfirmOutOfBounds()
	{
		if (this.boundsChecking)
		{
			Vector2 vector = this.transform.position;
			if (vector.y < -60f || vector.y > this.gm.sceneHeight + 60f || vector.x < -60f || vector.x > this.gm.sceneWidth + 60f)
			{
				if (!this.cState.dead)
				{
					this.rb2d.linearVelocity = Vector2.zero;
					Debug.LogFormat("Pos: {0} Transition State: {1}", new object[]
					{
						this.transform.position,
						this.transitionState
					});
					return;
				}
			}
			else
			{
				this.boundsChecking = false;
			}
		}
	}

	// Token: 0x06000EEE RID: 3822 RVA: 0x00046D94 File Offset: 0x00044F94
	private void FailSafeChecks()
	{
		if (this.hero_state == ActorStates.hard_landing)
		{
			this.hardLandFailSafeTimer += Time.deltaTime;
			if (this.hardLandFailSafeTimer > this.HARD_LANDING_TIME + 0.3f)
			{
				this.SetState(ActorStates.grounded);
				this.BackOnGround(false);
				this.hardLandFailSafeTimer = 0f;
			}
		}
		else
		{
			this.hardLandFailSafeTimer = 0f;
		}
		if (this.cState.hazardDeath)
		{
			this.hazardDeathTimer += Time.deltaTime;
			if (this.hazardDeathTimer > this.HAZARD_DEATH_CHECK_TIME && this.hero_state != ActorStates.no_input)
			{
				this.ResetMotion(true);
				this.AffectedByGravity(false);
				this.SetState(ActorStates.no_input);
				this.hazardDeathTimer = 0f;
			}
		}
		else
		{
			this.hazardDeathTimer = 0f;
		}
		if (this.rb2d.linearVelocity.y == 0f && !this.cState.onGround && !this.cState.falling && !this.cState.jumping && !this.cState.dashing && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.hero_state != ActorStates.no_input)
		{
			if (this.CheckTouchingGround())
			{
				this.floatingBufferTimer += Time.deltaTime;
				if (this.floatingBufferTimer > this.FLOATING_CHECK_TIME)
				{
					if (this.cState.recoiling)
					{
						this.CancelDamageRecoil();
					}
					this.BackOnGround(false);
					this.floatingBufferTimer = 0f;
					return;
				}
			}
			else
			{
				this.floatingBufferTimer = 0f;
			}
		}
	}

	// Token: 0x06000EEF RID: 3823 RVA: 0x00046F24 File Offset: 0x00045124
	public Transform LocateSpawnPoint()
	{
		string b;
		if (!string.IsNullOrEmpty(this.playerData.tempRespawnMarker))
		{
			b = this.playerData.tempRespawnMarker;
		}
		else if (!string.IsNullOrEmpty(this.gm.LastSceneLoad.SceneLoadInfo.EntryGateName))
		{
			b = this.gm.LastSceneLoad.SceneLoadInfo.EntryGateName;
		}
		else
		{
			b = this.playerData.respawnMarkerName;
		}
		foreach (RespawnMarker respawnMarker in RespawnMarker.Markers)
		{
			if (respawnMarker.name == b && respawnMarker.gameObject.activeInHierarchy)
			{
				return respawnMarker.transform;
			}
		}
		foreach (RespawnMarker respawnMarker2 in RespawnMarker.Markers)
		{
			if (respawnMarker2.name == b)
			{
				return respawnMarker2.transform;
			}
		}
		foreach (RespawnMarker respawnMarker3 in Object.FindObjectsByType<RespawnMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if (respawnMarker3.name == b)
			{
				return respawnMarker3.transform;
			}
		}
		return null;
	}

	// Token: 0x06000EF0 RID: 3824 RVA: 0x00047088 File Offset: 0x00045288
	private void CancelJump()
	{
		this.cState.jumping = false;
		this.jumpReleaseQueuing = false;
		this.jump_steps = 0;
		this.useUpdraftExitJumpSpeed = false;
		this.wallLocked = false;
	}

	// Token: 0x06000EF1 RID: 3825 RVA: 0x000470B2 File Offset: 0x000452B2
	private void CancelDoubleJump()
	{
		this.cState.doubleJumping = false;
		this.doubleJump_steps = 0;
	}

	// Token: 0x06000EF2 RID: 3826 RVA: 0x000470C8 File Offset: 0x000452C8
	private void CancelDash(bool sendSprintEvent = true)
	{
		if (this.cState.shadowDashing)
		{
			this.cState.shadowDashing = false;
		}
		this.cState.dashing = false;
		this.dashQueuing = false;
		this.heroBox.HeroBoxNormal();
		this.dashingDown = false;
		this.cState.airDashing = false;
		this.dash_timer = 0f;
		this.AffectedByGravity(true);
		this.StopDashEffect();
		if (sendSprintEvent)
		{
			this.sprintFSM.SendEvent("CANCEL SPRINT");
		}
	}

	// Token: 0x06000EF3 RID: 3827 RVA: 0x0004714A File Offset: 0x0004534A
	public void StopDashEffect()
	{
	}

	// Token: 0x06000EF4 RID: 3828 RVA: 0x0004714C File Offset: 0x0004534C
	private void CancelWallsliding()
	{
		this.wallslideDustPrefab.emission.enabled = false;
		if (this.cState.wallSliding)
		{
			this.cState.wallSliding = false;
			this.cState.wallClinging = false;
			this.vibrationCtrl.StopWallSlide();
			this.heroBox.HeroBoxNormal();
		}
		this.AffectedByGravity(true);
		this.cState.touchingWall = false;
		this.wallSlidingL = false;
		this.wallSlidingR = false;
		this.touchingWallL = false;
		this.touchingWallR = false;
		this.touchingWallObj = null;
	}

	// Token: 0x06000EF5 RID: 3829 RVA: 0x000471DE File Offset: 0x000453DE
	private void CancelBackDash()
	{
		this.cState.backDashing = false;
	}

	// Token: 0x06000EF6 RID: 3830 RVA: 0x000471EC File Offset: 0x000453EC
	private void CancelDownAttack()
	{
		if (this.cState.downAttacking)
		{
			this.ResetAttacks(true);
		}
	}

	// Token: 0x06000EF7 RID: 3831 RVA: 0x00047202 File Offset: 0x00045402
	public void CancelAttack()
	{
		this.CancelAttack(true);
	}

	// Token: 0x06000EF8 RID: 3832 RVA: 0x0004720B File Offset: 0x0004540B
	public void CancelAttack(bool resetNailCharge)
	{
		this.CancelAttackNotDownspikeBounce(resetNailCharge);
		if (this.cState.downSpikeBouncing)
		{
			this.cState.downSpikeBouncing = false;
		}
	}

	// Token: 0x06000EF9 RID: 3833 RVA: 0x0004722D File Offset: 0x0004542D
	public void CancelDownspike()
	{
		if (this.currentDownspike)
		{
			this.currentDownspike.CancelAttack();
		}
	}

	// Token: 0x06000EFA RID: 3834 RVA: 0x00047247 File Offset: 0x00045447
	public void CancelDownspikeAndDamage()
	{
		if (this.currentDownspike)
		{
			this.currentDownspike.CancelAttackAndDamage();
		}
	}

	// Token: 0x06000EFB RID: 3835 RVA: 0x00047261 File Offset: 0x00045461
	public void CancelAttackNotDownspikeBounce()
	{
		this.CancelAttackNotDownspikeBounce(true);
	}

	// Token: 0x06000EFC RID: 3836 RVA: 0x0004726A File Offset: 0x0004546A
	public void CancelAttackNotDownspikeBounce(bool resetNailCharge)
	{
		if (this.cState.attacking && this.SlashComponent != null)
		{
			this.SlashComponent.CancelAttack(false);
		}
		this.CancelAttackNotSlash(resetNailCharge);
	}

	// Token: 0x06000EFD RID: 3837 RVA: 0x0004729C File Offset: 0x0004549C
	private void CancelAttackNotSlash(bool resetNailCharge)
	{
		this.ResetAttacks(resetNailCharge);
		if (this.cState.downSpikeAntic)
		{
			this.cState.downSpikeAntic = false;
			if (!this.cState.downSpiking)
			{
				this.FinishDownspike();
			}
		}
		if (this.cState.downSpiking)
		{
			this.FinishDownspike();
		}
		if (this.cState.isToolThrowing)
		{
			this.ThrowToolEnd();
		}
	}

	// Token: 0x06000EFE RID: 3838 RVA: 0x00047304 File Offset: 0x00045504
	private void CancelBounce()
	{
		this.cState.bouncing = false;
		this.cState.shroomBouncing = false;
		this.bounceTimer = 0f;
		this.cState.downSpikeBouncing = false;
		this.cState.downSpikeBouncingShort = false;
		this.CancelQueuedBounces();
	}

	// Token: 0x06000EFF RID: 3839 RVA: 0x00047352 File Offset: 0x00045552
	public void CancelRecoilHorizontal()
	{
		this.cState.recoilingLeft = false;
		this.cState.recoilingRight = false;
		this.cState.recoilingDrill = false;
		this.recoilStepsLeft = 0;
	}

	// Token: 0x06000F00 RID: 3840 RVA: 0x00047380 File Offset: 0x00045580
	public void CancelDamageRecoil()
	{
		if (this.recoilRoutine != null)
		{
			base.StopCoroutine(this.recoilRoutine);
			this.recoilRoutine = null;
			this.cState.recoilFrozen = false;
			if (!this.cState.dead)
			{
				this.renderer.enabled = true;
			}
			this.playerData.disablePause = false;
		}
		this.cState.recoiling = false;
		this.recoilTimer = 0f;
		this.ResetMotion(true);
		this.AffectedByGravity(true);
		this.SetDamageMode(DamageMode.FULL_DAMAGE);
	}

	// Token: 0x06000F01 RID: 3841 RVA: 0x00047408 File Offset: 0x00045608
	public void CancelDamageRecoilSimple()
	{
		if (this.recoilRoutine != null)
		{
			base.StopCoroutine(this.recoilRoutine);
			this.recoilRoutine = null;
			this.cState.recoilFrozen = false;
			if (!this.cState.dead)
			{
				this.renderer.enabled = true;
			}
			this.playerData.disablePause = false;
		}
		this.cState.recoiling = false;
		this.recoilTimer = 0f;
	}

	// Token: 0x06000F02 RID: 3842 RVA: 0x00047478 File Offset: 0x00045678
	private void CancelFallEffects()
	{
		this.fallRumble = false;
		this.audioCtrl.StopSound(HeroSounds.FALLING, true);
		GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = false;
	}

	// Token: 0x06000F03 RID: 3843 RVA: 0x000474B4 File Offset: 0x000456B4
	private void ResetAttacksShared()
	{
		this.cState.attacking = false;
		this.cState.upAttacking = false;
		if (this.cState.downAttacking || this.tryCancelDownSlash)
		{
			this.cState.downAttacking = false;
			this.tryCancelDownSlash = false;
			if (this.SlashComponent)
			{
				this.SlashComponent.CancelAttack();
			}
		}
		this.attack_time = 0f;
	}

	// Token: 0x06000F04 RID: 3844 RVA: 0x00047524 File Offset: 0x00045724
	private void ResetAttacks(bool resetNailCharge = true)
	{
		this.ResetAttacksShared();
		this.wallSlashing = false;
		this.isDashStabBouncing = false;
		if (!this.allowNailChargingWhileRelinquished && resetNailCharge)
		{
			this.CancelNailCharge();
		}
		if (!this.queuedAutoThrowTool || resetNailCharge)
		{
			this.ThrowToolEnd();
		}
	}

	// Token: 0x06000F05 RID: 3845 RVA: 0x00047560 File Offset: 0x00045760
	private void StopNailChargeEffects()
	{
		this.artChargeEffect.SetActive(false);
		this.audioCtrl.StopSound(HeroSounds.NAIL_ART_CHARGE, true);
	}

	// Token: 0x06000F06 RID: 3846 RVA: 0x0004757C File Offset: 0x0004577C
	private void CancelNailCharge()
	{
		this.StopNailChargeEffects();
		this.cState.nailCharging = false;
		this.nailChargeTimer = 0f;
	}

	// Token: 0x06000F07 RID: 3847 RVA: 0x0004759B File Offset: 0x0004579B
	private void ResetAttacksDash()
	{
		this.ResetAttacksShared();
		this.ThrowToolEnd();
	}

	// Token: 0x06000F08 RID: 3848 RVA: 0x000475AC File Offset: 0x000457AC
	private void ResetMotion(bool resetNailCharge = true)
	{
		this.CancelDownAttack();
		this.CancelJump();
		this.CancelDoubleJump();
		this.CancelDash(true);
		this.CancelBackDash();
		this.CancelBounce();
		this.CancelRecoilHorizontal();
		this.CancelWallsliding();
		this.cState.floating = false;
		this.cState.downSpiking = false;
		this.ShuttleCockCancel();
		this.ResetShuttlecock();
		this.rb2d.linearVelocity = Vector2.zero;
		this.transition_vel = Vector2.zero;
		this.wallLocked = false;
		this.queuedWallJumpInterrupt = false;
		this.startWithRecoilBack = false;
		this.startWithWhipPullRecoil = false;
		this.extraAirMoveVelocities.Clear();
		if (!this.allowNailChargingWhileRelinquished && resetNailCharge)
		{
			this.nailChargeTimer = 0f;
		}
		this.ResetGravity();
	}

	// Token: 0x06000F09 RID: 3849 RVA: 0x00047670 File Offset: 0x00045870
	private void ResetMotionNotVelocity()
	{
		this.CancelJump();
		this.ShuttleCockCancel();
		this.CancelDoubleJump();
		this.CancelDash(true);
		this.CancelBackDash();
		this.CancelBounce();
		this.CancelRecoilHorizontal();
		this.CancelWallsliding();
		this.transition_vel = Vector2.zero;
		this.wallLocked = false;
		this.ResetGravity();
	}

	// Token: 0x06000F0A RID: 3850 RVA: 0x000476C6 File Offset: 0x000458C6
	public void ResetLook()
	{
		this.cState.lookingUp = false;
		this.cState.lookingDown = false;
		this.cState.lookingUpAnim = false;
		this.cState.lookingDownAnim = false;
		this.lookDelayTimer = 0f;
	}

	// Token: 0x06000F0B RID: 3851 RVA: 0x00047703 File Offset: 0x00045903
	private void ResetInput()
	{
		this.move_input = 0f;
		this.vertical_input = 0f;
	}

	// Token: 0x06000F0C RID: 3852 RVA: 0x0004771B File Offset: 0x0004591B
	public bool CheckAndRequestUnlock(HeroLockStates lockStates)
	{
		if (this.IsBlocked(lockStates))
		{
			this.AddUnlockRequest(lockStates);
			return true;
		}
		return false;
	}

	// Token: 0x06000F0D RID: 3853 RVA: 0x00047730 File Offset: 0x00045930
	public bool IsBlocked(HeroLockStates lockStates)
	{
		return lockStates != HeroLockStates.None && (this.HeroLockState & lockStates) == lockStates;
	}

	// Token: 0x06000F0E RID: 3854 RVA: 0x00047742 File Offset: 0x00045942
	public void AddLockStates(HeroLockStates lockStates)
	{
		this.HeroLockState |= lockStates;
	}

	// Token: 0x06000F0F RID: 3855 RVA: 0x00047754 File Offset: 0x00045954
	public void RemoveLockStates(HeroLockStates lockStates)
	{
		this.HeroLockState &= ~lockStates;
		if (lockStates.HasFlag(HeroLockStates.AnimationLocked) && this.unlockRequests.HasFlag(HeroLockStates.AnimationLocked))
		{
			this.StartAnimationControl();
		}
		if (lockStates.HasFlag(HeroLockStates.ControlLocked) && this.unlockRequests.HasFlag(HeroLockStates.ControlLocked))
		{
			this.RegainControl();
		}
		if (lockStates.HasFlag(HeroLockStates.GravityLocked) && this.unlockRequests.HasFlag(HeroLockStates.GravityLocked))
		{
			this.AffectedByGravity(true);
		}
	}

	// Token: 0x06000F10 RID: 3856 RVA: 0x00047804 File Offset: 0x00045A04
	public void SetLockStates(HeroLockStates lockStates)
	{
		this.HeroLockState = lockStates;
	}

	// Token: 0x06000F11 RID: 3857 RVA: 0x0004780D File Offset: 0x00045A0D
	public void AddUnlockRequest(HeroLockStates lockStates)
	{
		this.unlockRequests |= lockStates;
	}

	// Token: 0x06000F12 RID: 3858 RVA: 0x0004781D File Offset: 0x00045A1D
	public void RemoveUnlockRequest(HeroLockStates lockStates)
	{
		this.unlockRequests &= ~lockStates;
	}

	// Token: 0x06000F13 RID: 3859 RVA: 0x00047830 File Offset: 0x00045A30
	private void BackOnGround(bool force = false)
	{
		this.cState.willHardLand = false;
		this.hardLandingTimer = 0f;
		this.hardLanded = false;
		this.sprintBufferSteps = 0;
		this.syncBufferSteps = false;
		if (this.cState.onGround && !force)
		{
			return;
		}
		if (this.landingBufferSteps <= 0 && this.isHeroInPosition)
		{
			this.landingBufferSteps = this.LANDING_BUFFER_STEPS;
			if (!this.hardLanded && !this.cState.superDashing && (!this.controlReqlinquished || this.airDashed))
			{
				this.SpawnSoftLandingPrefab();
			}
		}
		this.cState.falling = false;
		this.fallTimer = 0f;
		this.dashLandingTimer = 0f;
		this.cState.floating = false;
		this.doFullJump = false;
		this.jump_steps = 0;
		this.extraAirMoveVelocities.Clear();
		if (this.hero_state != ActorStates.no_input)
		{
			if (this.cState.doubleJumping)
			{
				this.HeroJump();
			}
			this.SetState(ActorStates.grounded);
			if (!this.SlashComponent || (!this.SlashComponent.IsSlashOut && !this.SlashComponent.IsStartingSlash))
			{
				this.ResetAttacks(false);
			}
		}
		this.cState.onGround = true;
		this.airDashed = false;
		this.doubleJumped = false;
		this.allowAttackCancellingDownspikeRecovery = false;
	}

	// Token: 0x06000F14 RID: 3860 RVA: 0x0004797C File Offset: 0x00045B7C
	private void JumpReleased()
	{
		if (this.rb2d.linearVelocity.y > 0f && this.jumped_steps >= this.JUMP_STEPS_MIN && !this.cState.shroomBouncing && !this.doFullJump)
		{
			if (this.jumpReleaseQueueingEnabled)
			{
				if (this.jumpReleaseQueuing && this.jumpReleaseQueueSteps <= 0)
				{
					this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.rb2d.linearVelocity.y * 0.5f);
					this.CancelJump();
				}
			}
			else
			{
				this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, this.rb2d.linearVelocity.y * 0.5f);
				this.CancelJump();
			}
		}
		this.jumpQueuing = false;
		this.doubleJumpQueuing = false;
	}

	// Token: 0x06000F15 RID: 3861 RVA: 0x00047A73 File Offset: 0x00045C73
	public void SetDoFullJump()
	{
		this.doFullJump = true;
	}

	// Token: 0x06000F16 RID: 3862 RVA: 0x00047A7C File Offset: 0x00045C7C
	public void FinishedDashing(bool wasDashingDown)
	{
		if (this.cState.dashing)
		{
			this.audioCtrl.AllowFootstepsGrace();
		}
		this.CancelDash(false);
		this.AffectedByGravity(true);
		this.animCtrl.FinishedDash();
		this.proxyFSM.SendEvent("HeroCtrl-DashEnd");
		if (this.cState.onGround)
		{
			if (wasDashingDown)
			{
				this.sprintFSM.SendEvent("CANCEL SPRINT");
				return;
			}
			if (!this.IsInputBlocked())
			{
				this.sprintFSM.SendEvent("TRY SPRINT");
				return;
			}
		}
		else if (wasDashingDown)
		{
			this.sprintFSM.SendEvent("CANCEL SPRINT");
			this.animCtrl.SetDownDashEnded();
			if (this.audioSource && this.downDashCancelClip)
			{
				this.audioSource.PlayOneShot(this.downDashCancelClip, 1f);
				return;
			}
		}
		else
		{
			this.ResetHardLandingTimer();
			if (!this.IsInputBlocked())
			{
				this.sprintFSM.SendEvent("TRY SPRINT");
			}
		}
	}

	// Token: 0x06000F17 RID: 3863 RVA: 0x00047B71 File Offset: 0x00045D71
	public void FinishDownspike()
	{
		this.FinishDownspike(true);
	}

	// Token: 0x06000F18 RID: 3864 RVA: 0x00047B7C File Offset: 0x00045D7C
	public void FinishDownspike(bool standardRecovery)
	{
		this.cState.downSpiking = false;
		if (standardRecovery)
		{
			this.downSpikeRecoveryTimer = 0f;
		}
		else
		{
			this.downSpikeRecoveryTimer = 0.1f;
		}
		if (!this.cState.floating)
		{
			this.cState.downSpikeRecovery = true;
			this.ResetGravity();
			this.RegainControl();
			this.crestAttacksFSM.SendEvent("CREST ATTACK CANCEL");
			if (!this.startWithBalloonBounce)
			{
				this.rb2d.linearVelocity = new Vector3(this.rb2d.linearVelocity.x, -10f, 0f);
			}
		}
		if (this.currentDownspike)
		{
			this.currentDownspike.CancelAttack();
		}
	}

	// Token: 0x06000F19 RID: 3865 RVA: 0x00047C34 File Offset: 0x00045E34
	private void SetStartingMotionState()
	{
		this.SetStartingMotionState(false);
	}

	// Token: 0x06000F1A RID: 3866 RVA: 0x00047C40 File Offset: 0x00045E40
	private void SetStartingMotionState(bool preventRunDip)
	{
		if (this.IsInputBlocked() || (this.gm.GameState != GameState.PLAYING && this.gm.GameState != GameState.ENTERING_LEVEL))
		{
			this.move_input = 0f;
		}
		else
		{
			this.move_input = ((this.acceptingInput || preventRunDip) ? this.inputHandler.inputActions.MoveVector.X : 0f);
		}
		this.FilterInput();
		this.extraAirMoveVelocities.Clear();
		this.cState.touchingWall = false;
		if (this.CheckTouchingGround() && !this.startWithFullJump)
		{
			if (!this.cState.onGround && this.cState.isSprinting)
			{
				this.SetState(ActorStates.airborne);
				this.BackOnGround(false);
			}
			this.cState.onGround = true;
			this.allowAttackCancellingDownspikeRecovery = false;
			this.SetState(ActorStates.grounded);
			if (this.enteringVertically)
			{
				this.enteringVertically = false;
				if (this.playerData.bindCutscenePlayed)
				{
					this.SpawnSoftLandingPrefab();
					this.animCtrl.playLanding = true;
				}
			}
		}
		else
		{
			this.cState.onGround = false;
			this.SetState(ActorStates.airborne);
		}
		this.animCtrl.UpdateState(this.hero_state);
	}

	// Token: 0x06000F1B RID: 3867 RVA: 0x00047D70 File Offset: 0x00045F70
	private void TileMapTest()
	{
		if (!this.tilemapTestActive || this.cState.jumping)
		{
			return;
		}
		Vector2 vector = this.transform.position;
		Vector2 direction = new Vector2(this.positionHistory[0].x - vector.x, this.positionHistory[0].y - vector.y);
		float magnitude = direction.magnitude;
		if (!global::Helper.IsRayHittingNoTriggers(vector, direction, magnitude, 8448))
		{
			return;
		}
		this.ResetMotion(true);
		this.rb2d.linearVelocity = Vector2.zero;
		if (this.cState.spellQuake)
		{
			this.spellControl.SendEvent("Hero Landed");
			this.transform.SetPosition2D(this.positionHistory[1]);
		}
		this.tilemapTestActive = false;
		this.tilemapTestCoroutine = base.StartCoroutine(this.TilemapTestPause());
	}

	// Token: 0x06000F1C RID: 3868 RVA: 0x00047E57 File Offset: 0x00046057
	private IEnumerator TilemapTestPause()
	{
		yield return new WaitForSeconds(0.1f);
		this.tilemapTestActive = true;
		yield break;
	}

	// Token: 0x06000F1D RID: 3869 RVA: 0x00047E66 File Offset: 0x00046066
	private void StopTilemapTest()
	{
		if (this.tilemapTestCoroutine != null)
		{
			base.StopCoroutine(this.tilemapTestCoroutine);
			this.tilemapTestActive = false;
		}
	}

	// Token: 0x06000F1E RID: 3870 RVA: 0x00047E84 File Offset: 0x00046084
	[Obsolete]
	public bool TryDoTerrainThunk(AttackDirection attackDir, Collider2D thunkAgainst, Vector2 effectPoint, bool doHeroRecoil)
	{
		if (!thunkAgainst)
		{
			return false;
		}
		if (thunkAgainst.isTrigger)
		{
			return false;
		}
		bool flag = false;
		bool flag2;
		TerrainThunkUtils.GetThunkProperties(thunkAgainst.gameObject, out flag2, ref flag);
		if (flag2)
		{
			this.nailTerrainImpactEffectPrefab.Spawn(effectPoint, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
			if (doHeroRecoil)
			{
				if (attackDir == AttackDirection.normal)
				{
					if (this.cState.facingRight)
					{
						this.RecoilLeft();
					}
					else
					{
						this.RecoilRight();
					}
				}
				else if (attackDir == AttackDirection.upward)
				{
					this.RecoilDown();
				}
			}
		}
		else if (flag && doHeroRecoil)
		{
			if (attackDir == AttackDirection.normal)
			{
				if (this.cState.facingRight)
				{
					this.RecoilLeft();
				}
				else
				{
					this.RecoilRight();
				}
			}
			else if (attackDir == AttackDirection.upward)
			{
				this.RecoilDown();
			}
		}
		return true;
	}

	// Token: 0x06000F1F RID: 3871 RVA: 0x00047F4A File Offset: 0x0004614A
	private void ResetFixedUpdateCaches()
	{
		this.leftCache.Reset();
		this.rightCache.Reset();
	}

	// Token: 0x06000F20 RID: 3872 RVA: 0x00047F64 File Offset: 0x00046164
	public bool CheckStillTouchingWall(CollisionSide side, bool checkTop = false, bool checkNonSliders = true)
	{
		WallTouchCache.HitInfo top;
		WallTouchCache.HitInfo mid;
		WallTouchCache.HitInfo bottom;
		if (side != CollisionSide.left)
		{
			if (side != CollisionSide.right)
			{
				return false;
			}
			this.rightCache.Update(this.col2d, side, false);
			top = this.rightCache.top;
			mid = this.rightCache.mid;
			bottom = this.rightCache.bottom;
		}
		else
		{
			this.leftCache.Update(this.col2d, side, false);
			top = this.leftCache.top;
			mid = this.leftCache.mid;
			bottom = this.leftCache.bottom;
		}
		if (mid.HasCollider)
		{
			bool flag = !mid.IsSteepSlope;
			if (flag && checkNonSliders && mid.IsNonSlider)
			{
				flag = false;
			}
			if (flag)
			{
				return true;
			}
		}
		if (bottom.HasCollider)
		{
			bool flag2 = !bottom.IsSteepSlope;
			if (flag2 && checkNonSliders && bottom.IsNonSlider)
			{
				flag2 = false;
			}
			if (flag2)
			{
				return true;
			}
		}
		if (checkTop && top.HasCollider)
		{
			bool flag3 = !top.IsSteepSlope;
			if (flag3 && checkNonSliders && top.IsNonSlider)
			{
				flag3 = false;
			}
			if (flag3)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000F21 RID: 3873 RVA: 0x00048070 File Offset: 0x00046270
	private void CheckForBump(CollisionSide side)
	{
		bool flag;
		bool flag2;
		bool flag3;
		this.CheckForBump(side, out flag, out flag2, out flag3);
	}

	// Token: 0x06000F22 RID: 3874 RVA: 0x0004808A File Offset: 0x0004628A
	public void CheckForBump(CollisionSide side, out bool hitBump, out bool hitWall, out bool hitHighWall)
	{
		this.bumpChecker.CheckForBump(side, out hitBump, out hitWall, out hitHighWall);
	}

	// Token: 0x06000F23 RID: 3875 RVA: 0x0004809C File Offset: 0x0004629C
	public bool CheckNearRoof()
	{
		Bounds bounds = this.col2d.bounds;
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Vector3 center = bounds.center;
		Vector3 size = bounds.size;
		Vector2 origin = max;
		Vector2 origin2 = new Vector2(min.x, max.y);
		Vector2 origin3 = new Vector2(center.x + size.x / 4f, max.y);
		Vector2 origin4 = new Vector2(center.x - size.x / 4f, max.y);
		Vector2 direction = new Vector2(-0.5f, 1f);
		Vector2 direction2 = new Vector2(0.5f, 1f);
		Vector2 up = Vector2.up;
		return global::Helper.IsRayHittingNoTriggers(origin2, direction, 2f, 8448) || global::Helper.IsRayHittingNoTriggers(origin, direction2, 2f, 8448) || global::Helper.IsRayHittingNoTriggers(origin3, up, 1f, 8448) || global::Helper.IsRayHittingNoTriggers(origin4, up, 1f, 8448);
	}

	// Token: 0x06000F24 RID: 3876 RVA: 0x000481AC File Offset: 0x000463AC
	public bool CheckClamberLedge(out float y, out Collider2D clamberedCollider)
	{
		y = 0f;
		clamberedCollider = null;
		if (NoClamberRegion.IsClamberBlocked)
		{
			return false;
		}
		if (this.CheckNearRoof())
		{
			return false;
		}
		Vector2 vector = this.transform.position;
		Vector2 direction;
		Vector2 origin;
		Vector2 origin2;
		if (this.cState.facingRight)
		{
			direction = new Vector2(1f, 0f);
			origin = vector + new Vector2(0.77f, 0.67f);
			origin2 = vector + new Vector2(0.37f, 0.67f);
		}
		else
		{
			direction = new Vector2(-1f, 0f);
			origin = vector + new Vector2(-0.77f, 0.67f);
			origin2 = vector + new Vector2(-0.37f, 0.67f);
		}
		if (global::Helper.IsRayHittingNoTriggers(vector + new Vector2(0f, 0.67f), direction, 0.75f, 8448))
		{
			return false;
		}
		Vector2 vector2 = Vector2.zero;
		bool flag = false;
		Vector2 vector3 = Vector2.zero;
		bool flag2 = false;
		Vector2 direction2 = new Vector2(0f, -1f);
		RaycastHit2D raycastHit2D;
		if (global::Helper.IsRayHittingNoTriggers(origin, direction2, 2.26f, 8448, out raycastHit2D))
		{
			vector2 = raycastHit2D.point;
			flag = true;
			clamberedCollider = raycastHit2D.collider;
		}
		RaycastHit2D raycastHit2D2;
		if (global::Helper.IsRayHittingNoTriggers(origin2, direction2, 2.26f, 8448, out raycastHit2D2))
		{
			vector3 = raycastHit2D2.point;
			flag2 = true;
			clamberedCollider = raycastHit2D2.collider;
		}
		vector2.y += 0.1f;
		vector3.y += 0.1f;
		Vector2 direction3 = new Vector2(0f, 1f);
		if (global::Helper.IsRayHittingNoTriggers(vector2, direction3, 2.16f, 8448))
		{
			return false;
		}
		if (global::Helper.IsRayHittingNoTriggers(vector3, direction3, 2.16f, 8448))
		{
			return false;
		}
		if (!flag || !flag2)
		{
			return false;
		}
		if (!vector2.y.IsWithinTolerance(0.1f, vector3.y))
		{
			return false;
		}
		RaycastHit2D raycastHit2D3;
		if (global::Helper.IsRayHittingNoTriggers(new Vector2(vector.x, this.col2d.bounds.min.y + 0.2f), Vector2.down, 2f, 8448, out raycastHit2D3) && vector2.y - raycastHit2D3.point.y < 1.5f)
		{
			return false;
		}
		y = vector2.y;
		return true;
	}

	// Token: 0x06000F25 RID: 3877 RVA: 0x00048402 File Offset: 0x00046602
	public bool CheckTouchingGround()
	{
		return this.CheckTouchingGround(false);
	}

	// Token: 0x06000F26 RID: 3878 RVA: 0x0004840B File Offset: 0x0004660B
	public bool CheckTouchingGround(bool forced)
	{
		this.checkTouchGround.Update(this.col2d, forced);
		return this.checkTouchGround.IsTouchingGround;
	}

	// Token: 0x06000F27 RID: 3879 RVA: 0x0004842C File Offset: 0x0004662C
	private List<CollisionSide> CheckTouching(PhysLayers layer)
	{
		List<CollisionSide> list = new List<CollisionSide>(4);
		Bounds bounds = this.col2d.bounds;
		Vector3 center = bounds.center;
		float distance = bounds.extents.x + 0.16f;
		float distance2 = bounds.extents.y + 0.16f;
		RaycastHit2D raycastHit2D = global::Helper.Raycast2D(center, Vector2.up, distance2, 1 << (int)layer);
		RaycastHit2D raycastHit2D2 = global::Helper.Raycast2D(center, Vector2.right, distance, 1 << (int)layer);
		RaycastHit2D raycastHit2D3 = global::Helper.Raycast2D(center, Vector2.down, distance2, 1 << (int)layer);
		RaycastHit2D raycastHit2D4 = global::Helper.Raycast2D(center, Vector2.left, distance, 1 << (int)layer);
		if (raycastHit2D.collider != null)
		{
			list.Add(CollisionSide.top);
		}
		if (raycastHit2D2.collider != null)
		{
			list.Add(CollisionSide.right);
		}
		if (raycastHit2D3.collider != null)
		{
			list.Add(CollisionSide.bottom);
		}
		if (raycastHit2D4.collider != null)
		{
			list.Add(CollisionSide.left);
		}
		return list;
	}

	// Token: 0x06000F28 RID: 3880 RVA: 0x00048538 File Offset: 0x00046738
	private List<CollisionSide> CheckTouchingAdvanced(PhysLayers layer)
	{
		List<CollisionSide> list = new List<CollisionSide>();
		Bounds bounds = this.col2d.bounds;
		Vector2 origin = new Vector2(bounds.min.x, bounds.max.y);
		Vector2 origin2 = new Vector2(bounds.center.x, bounds.max.y);
		Vector2 origin3 = new Vector2(bounds.max.x, bounds.max.y);
		Vector2 origin4 = new Vector2(bounds.min.x, bounds.center.y);
		Vector2 origin5 = new Vector2(bounds.max.x, bounds.center.y);
		Vector2 origin6 = new Vector2(bounds.min.x, bounds.min.y);
		Vector2 origin7 = new Vector2(bounds.center.x, bounds.min.y);
		Vector2 origin8 = new Vector2(bounds.max.x, bounds.min.y);
		RaycastHit2D raycastHit2D = global::Helper.Raycast2D(origin, Vector2.up, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D2 = global::Helper.Raycast2D(origin2, Vector2.up, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D3 = global::Helper.Raycast2D(origin3, Vector2.up, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D4 = global::Helper.Raycast2D(origin3, Vector2.right, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D5 = global::Helper.Raycast2D(origin5, Vector2.right, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D6 = global::Helper.Raycast2D(origin8, Vector2.right, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D7 = global::Helper.Raycast2D(origin8, Vector2.down, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D8 = global::Helper.Raycast2D(origin7, Vector2.down, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D9 = global::Helper.Raycast2D(origin6, Vector2.down, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D10 = global::Helper.Raycast2D(origin6, Vector2.left, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D11 = global::Helper.Raycast2D(origin4, Vector2.left, 0.16f, 1 << (int)layer);
		RaycastHit2D raycastHit2D12 = global::Helper.Raycast2D(origin, Vector2.left, 0.16f, 1 << (int)layer);
		if (raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null)
		{
			list.Add(CollisionSide.top);
		}
		if (raycastHit2D4.collider != null || raycastHit2D5.collider != null || raycastHit2D6.collider != null)
		{
			list.Add(CollisionSide.right);
		}
		if (raycastHit2D7.collider != null || raycastHit2D8.collider != null || raycastHit2D9.collider != null)
		{
			list.Add(CollisionSide.bottom);
		}
		if (raycastHit2D10.collider != null || raycastHit2D11.collider != null || raycastHit2D12.collider != null)
		{
			list.Add(CollisionSide.left);
		}
		return list;
	}

	// Token: 0x06000F29 RID: 3881 RVA: 0x00048844 File Offset: 0x00046A44
	private CollisionSide FindCollisionDirection(Collision2D collision)
	{
		Vector2 normal = collision.GetSafeContact().Normal;
		float x = normal.x;
		float y = normal.y;
		if (y >= 0.5f)
		{
			return CollisionSide.bottom;
		}
		if (y <= -0.5f)
		{
			return CollisionSide.top;
		}
		if (x < 0f)
		{
			return CollisionSide.right;
		}
		if (x > 0f)
		{
			return CollisionSide.left;
		}
		Debug.LogError(string.Concat(new string[]
		{
			"ERROR: unable to determine direction of collision - contact points at (",
			normal.x.ToString(),
			",",
			normal.y.ToString(),
			")"
		}));
		return CollisionSide.bottom;
	}

	// Token: 0x06000F2A RID: 3882 RVA: 0x000488DC File Offset: 0x00046ADC
	public bool CanJump()
	{
		if (this.IsInputBlocked())
		{
			return false;
		}
		if (this.hero_state == ActorStates.no_input || this.hero_state == ActorStates.hard_landing || this.hero_state == ActorStates.dash_landing || this.cState.wallSliding || this.cState.dashing || this.cState.isSprinting || this.cState.backDashing || this.cState.jumping || this.cState.bouncing || this.cState.shroomBouncing || this.cState.downSpikeRecovery)
		{
			return false;
		}
		if (this.cState.onGround)
		{
			return true;
		}
		if (this.ledgeBufferSteps > 0 && !this.cState.dead && !this.cState.hazardDeath && !this.controlReqlinquished && this.headBumpSteps <= 0 && !this.CheckNearRoof())
		{
			this.ledgeBufferSteps = 0;
			return true;
		}
		return false;
	}

	// Token: 0x06000F2B RID: 3883 RVA: 0x000489E2 File Offset: 0x00046BE2
	public void AllowShuttleCock()
	{
		if (this.sprintBufferSteps < this.ledgeBufferSteps)
		{
			this.sprintBufferSteps = this.ledgeBufferSteps;
		}
		this.syncBufferSteps = true;
	}

	// Token: 0x06000F2C RID: 3884 RVA: 0x00048A05 File Offset: 0x00046C05
	public bool CouldJumpCancel()
	{
		return this.CanDoubleJump(false) || this.CanFloat(false);
	}

	// Token: 0x06000F2D RID: 3885 RVA: 0x00048A1C File Offset: 0x00046C1C
	public bool CanDoubleJump(bool checkControlState = true)
	{
		return (!checkControlState || (this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && !this.controlReqlinquished)) && (this.playerData.hasDoubleJump && !this.doubleJumped && !this.IsDashLocked() && !this.cState.wallSliding && !this.cState.backDashing && !this.IsAttackLocked() && !this.cState.bouncing && !this.cState.shroomBouncing && !this.cState.onGround && !this.cState.doubleJumping && this.Config.CanDoubleJump) && !this.TryQueueWallJumpInterrupt() && !this.IsApproachingSolidGround();
	}

	// Token: 0x06000F2E RID: 3886 RVA: 0x00048AEE File Offset: 0x00046CEE
	private bool CanInfiniteAirJump()
	{
		return this.playerData.infiniteAirJump && this.hero_state != ActorStates.hard_landing && !this.cState.onGround;
	}

	// Token: 0x06000F2F RID: 3887 RVA: 0x00048B18 File Offset: 0x00046D18
	private bool CanSwim()
	{
		return this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && !this.cState.attacking && !this.cState.dashing && !this.cState.jumping && !this.cState.bouncing && !this.cState.shroomBouncing && !this.cState.onGround;
	}

	// Token: 0x06000F30 RID: 3888 RVA: 0x00048B94 File Offset: 0x00046D94
	public bool CanDash()
	{
		return this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.dashCooldownTimer <= 0f && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.preventDash && (this.cState.onGround || !this.airDashed || this.cState.wallSliding) && !this.cState.hazardDeath && this.playerData.hasDash;
	}

	// Token: 0x06000F31 RID: 3889 RVA: 0x00048C5B File Offset: 0x00046E5B
	public bool DashCooldownReady()
	{
		return this.dashCooldownTimer <= 0f;
	}

	// Token: 0x06000F32 RID: 3890 RVA: 0x00048C6D File Offset: 0x00046E6D
	public bool HasHarpoonDash()
	{
		return this.playerData.hasHarpoonDash && this.Config.CanHarpoonDash;
	}

	// Token: 0x06000F33 RID: 3891 RVA: 0x00048C8C File Offset: 0x00046E8C
	public bool CanHarpoonDash()
	{
		return this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.harpoonDashCooldown <= 0f && (!this.cState.dashing || this.dash_timer <= 0f) && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && this.CanDoFSMCancelMove() && this.HasHarpoonDash() && !this.cState.dead && !this.cState.hazardDeath && !this.cState.hazardRespawning;
	}

	// Token: 0x06000F34 RID: 3892 RVA: 0x00048D34 File Offset: 0x00046F34
	public bool CanSprint()
	{
		return this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && !this.cState.dashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.preventDash && !this.controlReqlinquished;
	}

	// Token: 0x06000F35 RID: 3893 RVA: 0x00048DA4 File Offset: 0x00046FA4
	public bool CanSuperJump()
	{
		return !this.gm.isPaused && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.cState.onGround && !this.cState.dashing && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && this.CanDoFSMCancelMove() && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.playerData.hasSuperJump;
	}

	// Token: 0x06000F36 RID: 3894 RVA: 0x00048E7F File Offset: 0x0004707F
	public bool CanAttack()
	{
		return this.attack_cooldown <= 0f && this.CanAttackAction();
	}

	// Token: 0x06000F37 RID: 3895 RVA: 0x00048E96 File Offset: 0x00047096
	public bool ThrowToolCooldownReady()
	{
		return this.throwToolCooldown <= 0f;
	}

	// Token: 0x06000F38 RID: 3896 RVA: 0x00048EA8 File Offset: 0x000470A8
	public bool CanThrowTool()
	{
		return this.CanThrowTool(true);
	}

	// Token: 0x06000F39 RID: 3897 RVA: 0x00048EB1 File Offset: 0x000470B1
	public bool CanThrowTool(bool checkGetWillThrow)
	{
		return this.ThrowToolCooldownReady() && this.CanAttackAction() && (!checkGetWillThrow || this.GetWillThrowTool(true));
	}

	// Token: 0x06000F3A RID: 3898 RVA: 0x00048ED3 File Offset: 0x000470D3
	private bool CanStartWithThrowTool()
	{
		if (this.startWithToolThrow && this.CanThrowTool())
		{
			return true;
		}
		this.startWithToolThrow = false;
		return false;
	}

	// Token: 0x06000F3B RID: 3899 RVA: 0x00048EF0 File Offset: 0x000470F0
	private bool CanAttackAction()
	{
		return !this.cState.attacking && (!this.cState.dashing || this.dashingDown) && !this.cState.dead && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.controlReqlinquished && this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.CanInput() && (!this.inputHandler.inputActions.Down.IsPressed || this.CanDownAttack());
	}

	// Token: 0x06000F3C RID: 3900 RVA: 0x00048F96 File Offset: 0x00047196
	private bool CanDownAttack()
	{
		return this.allowAttackCancellingDownspikeRecovery || (!this.cState.downSpikeBouncing && !this.cState.downSpikeRecovery);
	}

	// Token: 0x06000F3D RID: 3901 RVA: 0x00048FC0 File Offset: 0x000471C0
	private bool TryQueueWallJumpInterrupt()
	{
		if (this.queuedWallJumpInterrupt)
		{
			return true;
		}
		Collider2D collider2D;
		if (!this.playerData.hasWalljump || !this.IsFacingNearWall(this.WALLJUMP_BROLLY_RAY_LENGTH, Color.green, out collider2D))
		{
			return false;
		}
		if (!this.CanChainWallJumps())
		{
			return false;
		}
		if (this.cState.touchingNonSlider)
		{
			return false;
		}
		this.queuedWallJumpInterrupt = true;
		this.touchingWallObj = (collider2D ? collider2D.gameObject : null);
		return true;
	}

	// Token: 0x06000F3E RID: 3902 RVA: 0x00049033 File Offset: 0x00047233
	private bool IsAttackLocked()
	{
		return this.cState.attacking && this.attack_time < this.Config.AttackRecoveryTime;
	}

	// Token: 0x06000F3F RID: 3903 RVA: 0x00049057 File Offset: 0x00047257
	private bool IsDashLocked()
	{
		return this.cState.dashing && (!this.dashingDown || this.dash_time < this.AIR_DASH_TIME);
	}

	// Token: 0x06000F40 RID: 3904 RVA: 0x00049080 File Offset: 0x00047280
	private bool CanFloat(bool checkControlState = true)
	{
		return (!checkControlState || (this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && !this.controlReqlinquished)) && !this.CanInfiniteAirJump() && (this.playerData.hasBrolly && !this.cState.onGround && !this.IsDashLocked() && !this.cState.swimming && this.ledgeBufferSteps <= 0 && !this.cState.wallSliding && !this.IsAttackLocked() && !this.cState.hazardDeath && !this.cState.hazardRespawning && (!this.cState.doubleJumping || this.cState.inUpdraft) && this.CanDoFSMCancelMove() && this.Config.CanBrolly) && !this.TryQueueWallJumpInterrupt() && !this.IsApproachingSolidGround();
	}

	// Token: 0x06000F41 RID: 3905 RVA: 0x00049178 File Offset: 0x00047378
	private bool IsApproachingSolidGround()
	{
		if (this.rb2d.linearVelocity.y > 0f)
		{
			return false;
		}
		Bounds bounds = this.col2d.bounds;
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Vector2 origin = new Vector2(min.x, min.y);
		Vector2 origin2 = new Vector2(max.x, min.y);
		float length = this.JUMP_ABILITY_GROUND_RAY_LENGTH + 0.1f;
		return global::Helper.IsRayHittingNoTriggers(origin, Vector2.down, length, 8448) && global::Helper.IsRayHittingNoTriggers(origin2, Vector2.down, length, 8448);
	}

	// Token: 0x06000F42 RID: 3906 RVA: 0x00049214 File Offset: 0x00047414
	private bool CanNailCharge()
	{
		return !this.cState.dead && !this.cState.attacking && (!this.controlReqlinquished || this.allowNailChargingWhileRelinquished) && !this.cState.recoiling && !this.cState.recoilingLeft && !this.cState.recoilingRight && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.playerData.hasChargeSlash && this.Config.CanNailCharge && !this.IsInputBlocked() && InteractManager.BlockingInteractable == null;
	}

	// Token: 0x06000F43 RID: 3907 RVA: 0x000492C4 File Offset: 0x000474C4
	private bool IsFacingNearWall(float rayLength, Color debugColor)
	{
		Collider2D collider2D;
		return this.IsFacingNearWall(this.cState.facingRight, rayLength, debugColor, out collider2D);
	}

	// Token: 0x06000F44 RID: 3908 RVA: 0x000492E8 File Offset: 0x000474E8
	public bool IsFacingNearWall(bool facingRight, float rayLength, Color debugColor)
	{
		Collider2D collider2D;
		return this.IsFacingNearWall(facingRight, rayLength, debugColor, out collider2D);
	}

	// Token: 0x06000F45 RID: 3909 RVA: 0x00049300 File Offset: 0x00047500
	public bool IsFacingNearWall(float rayLength, Color debugColor, out Collider2D wallCollider)
	{
		return this.IsFacingNearWall(this.cState.facingRight, rayLength, debugColor, out wallCollider);
	}

	// Token: 0x06000F46 RID: 3910 RVA: 0x00049318 File Offset: 0x00047518
	private bool IsFacingNearWall(bool facingRight, float rayLength, Color debugColor, out Collider2D wallCollider)
	{
		wallCollider = null;
		Vector2 vector = this.transform.position;
		Vector2 vector2 = new Vector2(vector.x, vector.y);
		Vector2 direction = facingRight ? Vector2.right : Vector2.left;
		RaycastHit2D raycastHit2D;
		if (!global::Helper.IsRayHittingNoTriggers(vector2, direction, rayLength, 8448, out raycastHit2D))
		{
			return false;
		}
		NonSlider nonSlider;
		if (NonSlider.TryGetNonSlider(raycastHit2D.collider, out nonSlider) && nonSlider.IsActive)
		{
			return false;
		}
		float num = 0f;
		float num2 = 0.1f;
		while (num2 < 1.5f && global::Helper.IsRayHittingNoTriggers(vector2 + new Vector2(0f, num2), direction, rayLength, 8448))
		{
			num += 0.1f;
			num2 += 0.1f;
		}
		num2 = 0.1f;
		while (num2 < 1.5f && global::Helper.IsRayHittingNoTriggers(vector2 - new Vector2(0f, num2), direction, rayLength, 8448))
		{
			num += 0.1f;
			num2 += 0.1f;
		}
		if (num < 1.5f)
		{
			return false;
		}
		wallCollider = raycastHit2D.collider;
		return true;
	}

	// Token: 0x06000F47 RID: 3911 RVA: 0x00049434 File Offset: 0x00047634
	public bool IsFacingNearSlideableWall()
	{
		return this.playerData.hasWalljump && !SlideSurface.IsHeroInside && ((this.cState.wallSliding && this.gm.isPaused) || this.IsFacingNearWall(this.WALLJUMP_RAY_LENGTH, Color.blue));
	}

	// Token: 0x06000F48 RID: 3912 RVA: 0x00049488 File Offset: 0x00047688
	private bool CanStartWithWallSlide()
	{
		bool flag = !this.cState.touchingNonSlider && !this.cState.onGround && !this.cState.recoiling && !this.cState.transitioning && !this.cState.doubleJumping && (this.cState.falling || this.cState.wallSliding || this.controlReqlinquished || this.cState.shuttleCock) && (!this.cState.attacking || this.wallSlashing);
		return (!flag || this.IsFacingNearSlideableWall()) && flag;
	}

	// Token: 0x06000F49 RID: 3913 RVA: 0x0004952D File Offset: 0x0004772D
	private bool CanWallSlide()
	{
		return this.CanInput() && this.CanContinueWallSlide();
	}

	// Token: 0x06000F4A RID: 3914 RVA: 0x0004953F File Offset: 0x0004773F
	private bool CanContinueWallSlide()
	{
		return !this.controlReqlinquished && (this.touchingWallL || this.touchingWallR) && this.CanStartWithWallSlide();
	}

	// Token: 0x06000F4B RID: 3915 RVA: 0x00049564 File Offset: 0x00047764
	public bool CanTakeDamage()
	{
		return this.damageMode != DamageMode.NO_DAMAGE && this.transitionState == HeroTransitionState.WAITING_TO_TRANSITION && !this.cState.transitioning && !this.cState.Invulnerable && !this.cState.recoiling && !this.playerData.isInvincible && this.parryInvulnTimer <= 0f && CheatManager.Invincibility != CheatManager.InvincibilityStates.FullInvincible && !this.cState.dead && !this.cState.hazardDeath && !BossSceneController.IsTransitioning && !HeroInvincibilitySource.IsActive;
	}

	// Token: 0x06000F4C RID: 3916 RVA: 0x000495F8 File Offset: 0x000477F8
	public bool CanTakeDamageIgnoreInvul()
	{
		return this.damageMode != DamageMode.NO_DAMAGE && this.transitionState == HeroTransitionState.WAITING_TO_TRANSITION && !this.playerData.isInvincible && CheatManager.Invincibility != CheatManager.InvincibilityStates.FullInvincible && !this.cState.dead && !this.cState.hazardDeath && !BossSceneController.IsTransitioning && !HeroInvincibilitySource.IsActive;
	}

	// Token: 0x06000F4D RID: 3917 RVA: 0x00049656 File Offset: 0x00047856
	public bool CanBeGrabbed(bool ignoreParryState)
	{
		return (ignoreParryState || !this.IsParrying()) && (this.CanTakeDamage() && this.cState.downspikeInvulnerabilitySteps <= 0);
	}

	// Token: 0x06000F4E RID: 3918 RVA: 0x0004967E File Offset: 0x0004787E
	public bool CanBeGrabbed()
	{
		return this.CanBeGrabbed(false);
	}

	// Token: 0x06000F4F RID: 3919 RVA: 0x00049688 File Offset: 0x00047888
	public bool CanBeBarnacleGrabbed()
	{
		return this.damageMode != DamageMode.NO_DAMAGE && this.transitionState == HeroTransitionState.WAITING_TO_TRANSITION && !this.cState.recoiling && !this.cState.dead && !this.cState.hazardDeath && !BossSceneController.IsTransitioning && !this.WillDoBellBindHit(true);
	}

	// Token: 0x06000F50 RID: 3920 RVA: 0x000496E0 File Offset: 0x000478E0
	private bool CanWallJump(bool mustBeNearWall = true)
	{
		return !SlideSurface.IsHeroInside && this.playerData.hasWalljump && !this.cState.touchingNonSlider && (this.cState.wallSliding || !mustBeNearWall || (this.cState.touchingWall && !this.cState.onGround && this.CanChainWallJumps()));
	}

	// Token: 0x06000F51 RID: 3921 RVA: 0x00049748 File Offset: 0x00047948
	private bool CanChainWallJumps()
	{
		return !this.dashingDown && (this.wallLocked || this.wallJumpChainStepsLeft > 0 || Math.Abs(this.move_input) > Mathf.Epsilon);
	}

	// Token: 0x06000F52 RID: 3922 RVA: 0x0004977C File Offset: 0x0004797C
	private bool CanWallScramble()
	{
		if (!this.cState.wallSliding)
		{
			return false;
		}
		if (!this.playerData.hasWalljump)
		{
			return false;
		}
		if (this.hero_state == ActorStates.no_input || this.hero_state == ActorStates.hard_landing || this.hero_state == ActorStates.dash_landing || this.controlReqlinquished)
		{
			return false;
		}
		if (this.touchingWallL)
		{
			if (!this.inputHandler.inputActions.Left.IsPressed)
			{
				return false;
			}
		}
		else
		{
			if (!this.touchingWallR)
			{
				return false;
			}
			if (!this.inputHandler.inputActions.Right.IsPressed)
			{
				return false;
			}
		}
		return (!this.cState.attacking || this.attack_time >= this.Config.AttackRecoveryTime) && !this.cState.hazardDeath && !this.cState.hazardRespawning;
	}

	// Token: 0x06000F53 RID: 3923 RVA: 0x00049850 File Offset: 0x00047A50
	public bool ShouldHardLand(GameObject obj)
	{
		return !this.cState.hazardDeath && !obj.GetComponent<NoHardLanding>() && (this.cState.willHardLand && this.hero_state != ActorStates.hard_landing) && (!GameManager.instance || GameManager.instance.GameState == GameState.PLAYING);
	}

	// Token: 0x06000F54 RID: 3924 RVA: 0x000498AE File Offset: 0x00047AAE
	private bool IsOnGroundLayer(GameObject obj)
	{
		return (1 << obj.layer & 8448) != 0;
	}

	// Token: 0x06000F55 RID: 3925 RVA: 0x000498C4 File Offset: 0x00047AC4
	private void OnCollisionEnter2D(Collision2D collision)
	{
		bool flag = false;
		GameObject gameObject = collision.gameObject;
		if (this.IsOnGroundLayer(gameObject) || gameObject.CompareTag("HeroWalkable"))
		{
			flag = this.CheckTouchingGround();
			if (flag)
			{
				this.proxyFSM.SendEvent("HeroCtrl-Landed");
				this.umbrellaFSM.SendEvent("LAND");
			}
		}
		this.HandleCollisionTouching(collision);
		if (this.hero_state != ActorStates.no_input)
		{
			CollisionSide collisionSide = this.FindCollisionDirection(collision);
			if (this.IsOnGroundLayer(gameObject) || gameObject.CompareTag("HeroWalkable"))
			{
				this.fallTrailGenerated = false;
				if (collisionSide == CollisionSide.top)
				{
					this.headBumpSteps = this.HEAD_BUMP_STEPS;
					if (this.cState.jumping)
					{
						this.CancelJump();
						this.CancelDoubleJump();
					}
					if (this.cState.bouncing)
					{
						this.CancelBounce();
						this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, 0f);
					}
					if (this.cState.shroomBouncing)
					{
						this.CancelBounce();
						this.rb2d.linearVelocity = new Vector2(this.rb2d.linearVelocity.x, 0f);
					}
				}
				if (collisionSide == CollisionSide.bottom)
				{
					if (this.cState.attacking)
					{
						this.CancelDownAttack();
					}
					if (this.ShouldHardLand(gameObject))
					{
						this.DoHardLanding();
						return;
					}
					if (this.cState.dashing && this.dashingDown)
					{
						this.AffectedByGravity(true);
						this.SetState(ActorStates.dash_landing);
						this.hardLanded = true;
						return;
					}
					if (!SteepSlope.IsSteepSlope(gameObject) && this.hero_state != ActorStates.hard_landing && !this.cState.onGround)
					{
						this.BackOnGround(false);
						return;
					}
				}
			}
		}
		else if (this.hero_state == ActorStates.no_input)
		{
			if (flag)
			{
				this.sprintFSM.SendEvent("HERO TOUCHED GROUND");
			}
			if (this.transitionState == HeroTransitionState.DROPPING_DOWN)
			{
				if (this.gatePosition == GatePosition.bottom || this.gatePosition == GatePosition.top)
				{
					if (this.gatePosition == GatePosition.bottom)
					{
						this.attack_cooldown = 0.1f;
					}
					this.FinishedEnteringScene(true, false);
					return;
				}
			}
			else if (flag && this.cState.isSprinting && !this.cState.willHardLand && !this.cState.onGround)
			{
				this.BackOnGround(false);
			}
		}
	}

	// Token: 0x06000F56 RID: 3926 RVA: 0x00049AF8 File Offset: 0x00047CF8
	private void HandleCollisionTouching(Collision2D collision)
	{
		if (this.cState.downSpiking && this.FindCollisionDirection(collision) == CollisionSide.bottom)
		{
			this.FinishDownspike();
			Vector3 vector = this.downspikeEffectPrefabSpawnPoint ? this.downspikeEffectPrefabSpawnPoint.position : this.transform.position;
			this.nailTerrainImpactEffectPrefabDownSpike.Spawn(vector, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
			NailSlashTerrainThunk.ReportDownspikeHitGround(vector);
		}
		if (this.cState.shuttleCock && this.IsFacingNearWall(this.WALLJUMP_RAY_LENGTH, Color.blue))
		{
			this.TryFsmCancelToWallSlide();
		}
	}

	// Token: 0x06000F57 RID: 3927 RVA: 0x00049BA8 File Offset: 0x00047DA8
	private void OnCollisionStay2D(Collision2D collision)
	{
		GameObject gameObject = collision.gameObject;
		if (gameObject.CompareTag("Geo"))
		{
			return;
		}
		this.HandleCollisionTouching(collision);
		if (this.hero_state != ActorStates.no_input)
		{
			if (this.IsOnGroundLayer(gameObject))
			{
				bool flag = false;
				if (!NonSlider.IsNonSlider(gameObject))
				{
					this.cState.touchingNonSlider = false;
					if (this.CheckStillTouchingWall(CollisionSide.left, false, true))
					{
						this.cState.touchingWall = true;
						this.touchingWallL = true;
						this.touchingWallR = false;
						this.touchingWallObj = gameObject;
					}
					else if (this.CheckStillTouchingWall(CollisionSide.right, false, true))
					{
						this.cState.touchingWall = true;
						this.touchingWallL = false;
						this.touchingWallR = true;
						this.touchingWallObj = gameObject;
					}
					else
					{
						this.cState.touchingWall = false;
						this.touchingWallL = false;
						this.touchingWallR = false;
						this.touchingWallObj = null;
					}
					if (this.CheckTouchingGround())
					{
						flag = true;
						if (this.ShouldHardLand(gameObject))
						{
							this.DoHardLanding();
						}
						else if (this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.cState.falling)
						{
							this.BackOnGround(false);
						}
					}
					else if (this.cState.jumping || this.cState.falling)
					{
						this.LeftGround(true);
					}
				}
				else
				{
					this.cState.touchingNonSlider = true;
					if (this.FindCollisionDirection(collision) == CollisionSide.bottom && this.CheckTouchingGround() && !SteepSlope.IsSteepSlope(collision.gameObject))
					{
						flag = true;
						if (this.ShouldHardLand(gameObject))
						{
							this.DoHardLanding();
						}
						else if (this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.cState.falling)
						{
							this.BackOnGround(false);
						}
					}
				}
				if (!flag && collision.contactCount > 0 && collision.GetContact(0).normal.y >= 1f && (!this.dashingDown || this.rb2d.linearVelocity.y == 0f))
				{
					this.tryShove = true;
					this.onFlatGround = true;
					return;
				}
			}
		}
		else
		{
			FsmBool fsmBool = this.isUmbrellaActive;
			if (fsmBool != null && fsmBool.Value && gameObject.layer == 8)
			{
				Collision2DUtils.Collision2DSafeContact safeContact = collision.GetSafeContact();
				if (!safeContact.IsLegitimate || safeContact.Normal != Vector2.up)
				{
					return;
				}
				this.ShoveOff();
			}
			if (!this.cState.onGround && this.cState.isSprinting)
			{
				bool flag2 = false;
				if (this.IsOnGroundLayer(gameObject) || gameObject.CompareTag("HeroWalkable"))
				{
					flag2 = this.CheckTouchingGround();
					if (flag2)
					{
						this.proxyFSM.SendEvent("HeroCtrl-Landed");
						this.umbrellaFSM.SendEvent("LAND");
					}
				}
				if (flag2 && !this.cState.willHardLand && !this.cState.onGround)
				{
					this.BackOnGround(false);
				}
			}
		}
	}

	// Token: 0x06000F58 RID: 3928 RVA: 0x00049E84 File Offset: 0x00048084
	private void OnCollisionExit2D(Collision2D collision)
	{
		if (this.cState.recoilingLeft || this.cState.recoilingRight)
		{
			this.cState.touchingWall = false;
			this.touchingWallL = false;
			this.touchingWallR = false;
			this.touchingWallObj = null;
			this.cState.touchingNonSlider = false;
		}
		if (this.cState.touchingWall && collision.gameObject == this.touchingWallObj)
		{
			this.cState.touchingWall = false;
			this.touchingWallL = false;
			this.touchingWallR = false;
			this.touchingWallObj = null;
		}
		if (this.hero_state != ActorStates.no_input && !this.cState.recoiling && this.IsOnGroundLayer(collision.gameObject) && !this.CheckTouchingGround())
		{
			this.LeftGround(true);
		}
	}

	// Token: 0x06000F59 RID: 3929 RVA: 0x00049F4C File Offset: 0x0004814C
	private void LeftGround(bool setState)
	{
		if (this.cState.onGround)
		{
			if (!this.cState.jumping && !this.fallTrailGenerated)
			{
				if (this.playerData.environmentType != EnvironmentTypes.Moss && this.playerData.environmentType != EnvironmentTypes.WetMetal && !this.playerData.atBench)
				{
					this.fsm_fallTrail.SendEvent("PLAY");
				}
				this.fallTrailGenerated = true;
			}
			this.SetCanSoftLand();
			this.cState.onGround = false;
			this.proxyFSM.SendEvent("HeroCtrl-LeftGround");
			if (this.cState.wasOnGround)
			{
				this.ledgeBufferSteps = this.LEDGE_BUFFER_STEPS;
				if (this.syncBufferSteps || this.cState.isSprinting || this.cState.dashing)
				{
					this.sprintBufferSteps = this.ledgeBufferSteps;
				}
			}
		}
		if (setState)
		{
			this.SetState(ActorStates.airborne);
		}
	}

	// Token: 0x06000F5A RID: 3930 RVA: 0x0004A034 File Offset: 0x00048234
	private void SetupGameRefs()
	{
		if (this.cState == null)
		{
			this.cState = new HeroControllerStates();
		}
		this.gm = GameManager.instance;
		this.animCtrl = base.GetComponent<HeroAnimationController>();
		this.rb2d = base.GetComponent<Rigidbody2D>();
		this.col2d = base.GetComponent<Collider2D>();
		this.transform = base.GetComponent<Transform>();
		this.renderer = base.GetComponent<MeshRenderer>();
		this.audioCtrl = base.GetComponent<HeroAudioController>();
		this.inputHandler = this.gm.GetComponent<InputHandler>();
		this.proxyFSM = FSMUtility.LocateFSM(base.gameObject, "ProxyFSM");
		this.audioSource = base.GetComponent<AudioSource>();
		this.enviroRegionListener = base.GetComponent<EnviroRegionListener>();
		this.NailImbuement = base.GetComponent<HeroNailImbuement>();
		this.vibrationCtrl = base.GetComponent<HeroVibrationController>();
		this.invPulse = base.GetComponent<InvulnerablePulse>();
		this.spriteFlash = base.GetComponent<SpriteFlash>();
		if (this.runningWaterEffect)
		{
			this.areaEffectTint = this.runningWaterEffect.GetComponent<AreaEffectTint>();
		}
		this.gm.UnloadingLevel += this.OnLevelUnload;
		this.prevGravityScale = this.DEFAULT_GRAVITY;
		this.transition_vel = Vector2.zero;
		this.current_velocity = Vector2.zero;
		this.acceptingInput = true;
		this.positionHistory = new Vector2[2];
	}

	// Token: 0x06000F5B RID: 3931 RVA: 0x0004A180 File Offset: 0x00048380
	private void SetupPools()
	{
	}

	// Token: 0x06000F5C RID: 3932 RVA: 0x0004A184 File Offset: 0x00048384
	private void FilterInput()
	{
		if (this.move_input > 0.3f)
		{
			this.move_input = 1f;
		}
		else if (this.move_input < -0.3f)
		{
			this.move_input = -1f;
		}
		else
		{
			this.move_input = 0f;
		}
		if (this.vertical_input > 0.5f)
		{
			this.vertical_input = 1f;
			return;
		}
		if (this.vertical_input < -0.5f)
		{
			this.vertical_input = -1f;
			return;
		}
		this.vertical_input = 0f;
	}

	// Token: 0x06000F5D RID: 3933 RVA: 0x0004A210 File Offset: 0x00048410
	public Vector3 FindGroundPoint(Vector2 startPoint, bool useExtended = false)
	{
		Vector2 vector;
		return this.TryFindGroundPoint(out vector, startPoint, useExtended) ? vector : startPoint;
	}

	// Token: 0x06000F5E RID: 3934 RVA: 0x0004A234 File Offset: 0x00048434
	private float FindGroundPointY(float x, float y, bool useExtended = false)
	{
		float length = useExtended ? this.FIND_GROUND_POINT_DISTANCE_EXT : this.FIND_GROUND_POINT_DISTANCE;
		RaycastHit2D raycastHit2D;
		global::Helper.IsRayHittingNoTriggers(new Vector2(x, y), Vector2.down, length, 8448, out raycastHit2D);
		return raycastHit2D.point.y + this.col2d.bounds.extents.y - this.col2d.offset.y + Physics2D.defaultContactOffset;
	}

	// Token: 0x06000F5F RID: 3935 RVA: 0x0004A2AC File Offset: 0x000484AC
	private bool TryFindGroundPoint(out Vector2 groundPos, Vector2 startPos, bool useExtended)
	{
		float num = useExtended ? this.FIND_GROUND_POINT_DISTANCE_EXT : this.FIND_GROUND_POINT_DISTANCE;
		RaycastHit2D raycastHit2D;
		if (!global::Helper.IsRayHittingNoTriggers(startPos, Vector2.down, num, 8448, out raycastHit2D))
		{
			Debug.LogErrorFormat("FindGroundPoint: Could not find ground point below {0}, check reference position is not too high (more than {1} tiles).", new object[]
			{
				startPos.ToString(),
				num
			});
			groundPos = startPos;
			return false;
		}
		groundPos = new Vector2(raycastHit2D.point.x, raycastHit2D.point.y + this.col2d.bounds.extents.y - this.col2d.offset.y + 0.01f);
		return true;
	}

	// Token: 0x06000F60 RID: 3936 RVA: 0x0004A366 File Offset: 0x00048566
	public void StartDownspikeInvulnerability()
	{
		this.cState.downspikeInvulnerabilitySteps = this.DOWNSPIKE_INVULNERABILITY_STEPS;
	}

	// Token: 0x06000F61 RID: 3937 RVA: 0x0004A379 File Offset: 0x00048579
	public void StartDownspikeInvulnerabilityLong()
	{
		this.cState.downspikeInvulnerabilitySteps = this.DOWNSPIKE_INVULNERABILITY_STEPS_LONG;
	}

	// Token: 0x06000F62 RID: 3938 RVA: 0x0004A38C File Offset: 0x0004858C
	public void CancelDownspikeInvulnerability()
	{
		this.cState.downspikeInvulnerabilitySteps = 0;
	}

	// Token: 0x06000F63 RID: 3939 RVA: 0x0004A39A File Offset: 0x0004859A
	public void DetachHeroLight()
	{
		if (!this.heroLight)
		{
			return;
		}
		this.heroLight.Detach();
	}

	// Token: 0x06000F64 RID: 3940 RVA: 0x0004A3B5 File Offset: 0x000485B5
	public void ReattachHeroLight()
	{
		if (!this.heroLight)
		{
			return;
		}
		this.heroLight.Reattach();
	}

	// Token: 0x06000F65 RID: 3941 RVA: 0x0004A3D0 File Offset: 0x000485D0
	public void SetAllowNailChargingWhileRelinquished(bool value)
	{
		this.allowNailChargingWhileRelinquished = value;
	}

	// Token: 0x06000F66 RID: 3942 RVA: 0x0004A3D9 File Offset: 0x000485D9
	public void SetAllowRecoilWhileRelinquished(bool value)
	{
		this.allowRecoilWhileRelinquished = value;
	}

	// Token: 0x06000F67 RID: 3943 RVA: 0x0004A3E2 File Offset: 0x000485E2
	public void SetRecoilZeroVelocity(bool value)
	{
		this.recoilZeroVelocity = value;
	}

	// Token: 0x06000F68 RID: 3944 RVA: 0x0004A3EB File Offset: 0x000485EB
	public IEnumerator MoveToPositionX(float targetX, Action onEnd)
	{
		float dir = Mathf.Sign(targetX - this.transform.position.x);
		Func<bool> shouldMove = delegate()
		{
			if (dir < 0f)
			{
				if (this.transform.position.x <= targetX)
				{
					return false;
				}
			}
			else if (this.transform.position.x >= targetX)
			{
				return false;
			}
			return true;
		};
		bool hadControl = !this.controlReqlinquished;
		if (hadControl)
		{
			this.RelinquishControl();
		}
		this.StopAnimationControl();
		tk2dSpriteAnimator component = base.GetComponent<tk2dSpriteAnimator>();
		if (shouldMove())
		{
			float velX = this.GetWalkSpeed() * dir;
			Vector2 linearVelocity = this.rb2d.linearVelocity;
			linearVelocity.x = velX;
			this.rb2d.linearVelocity = linearVelocity;
			if ((this.cState.facingRight && dir < 0f) || (!this.cState.facingRight && dir > 0f))
			{
				if (dir > 0f)
				{
					this.FaceRight();
				}
				else
				{
					this.FaceLeft();
				}
				yield return base.StartCoroutine(component.PlayAnimWait("Turn", null));
			}
			this.animCtrl.PlayClipForced("Walk");
			this.ForceWalkingSound = true;
			float stationaryElapsed = 0f;
			float lastXPos = this.transform.position.x;
			while (stationaryElapsed < 3f && shouldMove())
			{
				yield return null;
				this.CheckForBump((velX > 0f) ? CollisionSide.right : CollisionSide.left);
				linearVelocity = this.rb2d.linearVelocity;
				linearVelocity.x = velX;
				this.rb2d.linearVelocity = linearVelocity;
				float x = this.transform.position.x;
				if (Mathf.Abs(x - lastXPos) < 0.01f)
				{
					stationaryElapsed += Time.deltaTime;
				}
				else
				{
					stationaryElapsed = 0f;
				}
				lastXPos = x;
			}
			linearVelocity = this.rb2d.linearVelocity;
			linearVelocity.x = 0f;
			this.rb2d.linearVelocity = linearVelocity;
			this.transform.SetPositionX(targetX);
			this.animCtrl.PlayClipForced("Idle");
			this.ForceWalkingSound = false;
			yield return null;
			this.StartAnimationControl();
			if (hadControl)
			{
				this.RegainControl();
			}
		}
		if (onEnd != null)
		{
			onEnd();
		}
		yield break;
	}

	// Token: 0x06000F69 RID: 3945 RVA: 0x0004A408 File Offset: 0x00048608
	public void ToggleNoClip()
	{
		if (this.GetIsNoClip())
		{
			this.col2d.enabled = true;
			this.playerData.isInvincible = false;
			this.playerData.infiniteAirJump = false;
			return;
		}
		this.col2d.enabled = false;
		this.playerData.isInvincible = true;
		this.playerData.infiniteAirJump = true;
	}

	// Token: 0x06000F6A RID: 3946 RVA: 0x0004A466 File Offset: 0x00048666
	public bool GetIsNoClip()
	{
		return !this.col2d.enabled;
	}

	// Token: 0x06000F6B RID: 3947 RVA: 0x0004A476 File Offset: 0x00048676
	public float GetRunSpeed()
	{
		if (this.IsUsingQuickening)
		{
			return this.QUICKENING_RUN_SPEED;
		}
		return this.RUN_SPEED;
	}

	// Token: 0x06000F6C RID: 3948 RVA: 0x0004A48D File Offset: 0x0004868D
	public float GetWalkSpeed()
	{
		if (this.IsUsingQuickening)
		{
			return this.QUICKENING_WALK_SPEED;
		}
		return this.WALK_SPEED;
	}

	// Token: 0x06000F6D RID: 3949 RVA: 0x0004A4A4 File Offset: 0x000486A4
	public void ActivateQuickening()
	{
		this.quickeningTimeLeft = this.QUICKENING_DURATION;
		if (this.spawnedQuickeningEffect)
		{
			this.spawnedQuickeningEffect.Recycle<PlayParticleEffects>();
			this.spawnedQuickeningEffect = null;
		}
		bool isEquipped = Gameplay.PoisonPouchTool.IsEquipped;
		PlayParticleEffects playParticleEffects = isEquipped ? this.quickeningPoisonEffectPrefab : this.quickeningEffectPrefab;
		if (playParticleEffects)
		{
			PlayParticleEffects playParticleEffects2 = playParticleEffects.Spawn<PlayParticleEffects>();
			playParticleEffects2.transform.SetParent(this.transform, true);
			playParticleEffects2.transform.SetLocalPosition2D(Vector2.zero);
			this.spawnedQuickeningEffect = playParticleEffects2;
			playParticleEffects2.PlayParticleSystems();
		}
		this.sprintFSMIsQuickening.Value = true;
		this.toolsFSMIsQuickening.Value = true;
		if (!this.spriteFlash.IsFlashing(true, this.quickeningFlash))
		{
			this.quickeningFlash = this.spriteFlash.Flash(isEquipped ? Gameplay.PoisonPouchHeroTintColour : new Color(1f, 0.85f, 0.47f, 1f), 0.7f, 0.2f, 0.01f, 0.22f, 0f, true, 0, 1, true);
		}
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x0004A5B4 File Offset: 0x000487B4
	public void StartRoarLock()
	{
		FSMUtility.SendEventToGameObject(base.gameObject, "ROAR ENTER", false);
	}

	// Token: 0x06000F6F RID: 3951 RVA: 0x0004A5C8 File Offset: 0x000487C8
	public void StartRoarLockNoRecoil()
	{
		PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(base.gameObject, "Roar and Wound States");
		if (playMakerFSM)
		{
			playMakerFSM.FsmVariables.FindFsmFloat("Push Strength").Value = 0f;
		}
		FSMUtility.SendEventToGameObject(base.gameObject, "ROAR ENTER", false);
	}

	// Token: 0x06000F70 RID: 3952 RVA: 0x0004A619 File Offset: 0x00048819
	public void StopRoarLock()
	{
		FSMUtility.SendEventToGameObject(base.gameObject, "ROAR EXIT", false);
	}

	// Token: 0x06000F71 RID: 3953 RVA: 0x0004A62C File Offset: 0x0004882C
	public void CocoonBroken()
	{
		this.CocoonBroken(false, false);
	}

	// Token: 0x06000F72 RID: 3954 RVA: 0x0004A636 File Offset: 0x00048836
	public void CocoonBroken(bool doAirPause)
	{
		this.CocoonBroken(doAirPause, false);
	}

	// Token: 0x06000F73 RID: 3955 RVA: 0x0004A640 File Offset: 0x00048840
	public void CocoonBroken(bool doAirPause, bool forceCanBind)
	{
		int heroCorpseMoneyPool = this.playerData.HeroCorpseMoneyPool;
		this.playerData.HeroCorpseScene = string.Empty;
		this.playerData.HeroCorpseMoneyPool = 0;
		this.playerData.HeroCorpseMarkerGuid = null;
		if (heroCorpseMoneyPool > 0)
		{
			CurrencyManager.AddCurrency(heroCorpseMoneyPool, CurrencyType.Money, true);
		}
		if (this.playerData.silkMax > 9)
		{
			this.playerData.IsSilkSpoolBroken = false;
			EventRegister.SendEvent(EventRegisterEvents.SpoolUnbroken, null);
		}
		this.AddSilk(9, true, SilkSpool.SilkAddSource.Normal, forceCanBind);
		if (!doAirPause || this.hero_state != ActorStates.airborne || this.cState.swimming)
		{
			return;
		}
		if (this.cocoonFloatRoutine != null)
		{
			base.StopCoroutine(this.cocoonFloatRoutine);
		}
		this.cocoonFloatRoutine = base.StartCoroutine(this.CocoonFloatRoutine());
	}

	// Token: 0x06000F74 RID: 3956 RVA: 0x0004A6FE File Offset: 0x000488FE
	private IEnumerator CocoonFloatRoutine()
	{
		bool didRelinquish = !this.playerData.atBench;
		if (didRelinquish)
		{
			this.RelinquishControl();
			this.AffectedByGravity(false);
			this.SetState(ActorStates.no_input);
		}
		int controlVersion = HeroController.ControlVersion;
		yield return new WaitForSeconds(0.25f);
		if (didRelinquish && controlVersion == HeroController.ControlVersion)
		{
			this.RegainControl();
		}
		yield break;
	}

	// Token: 0x06000F75 RID: 3957 RVA: 0x0004A710 File Offset: 0x00048910
	public void RecordLeaveSceneCState()
	{
		if (this.cState.superDashing)
		{
			this.exitedSuperDashing = true;
		}
		if (this.cState.spellQuake)
		{
			this.exitedQuake = true;
		}
		if (this.cState.dashing || this.cState.isSprinting || this.sprintFSM.GetFsmBoolIfExists("Is Sprinting"))
		{
			this.exitedSprinting = true;
		}
	}

	// Token: 0x06000F76 RID: 3958 RVA: 0x0004A778 File Offset: 0x00048978
	public void CleanSpawnedDeliveryEffects()
	{
		if (DeliveryQuestItem.GetActiveItems().Count<DeliveryQuestItem.ActiveItem>() == 0)
		{
			foreach (GameObject gameObject in this.spawnedDeliveryEffects.Values)
			{
				if (gameObject != null)
				{
					Object.Destroy(gameObject);
				}
			}
			this.spawnedDeliveryEffects.Clear();
		}
	}

	// Token: 0x06000F77 RID: 3959 RVA: 0x0004A7F0 File Offset: 0x000489F0
	public void LeavingScene()
	{
		PersistentAudioManager.OnLeaveScene();
		this.ResetFixedUpdateCaches();
		this.cState.ClearInvulnerabilitySources();
		this.CleanSpawnedDeliveryEffects();
		this.RecordLeaveSceneCState();
		this.cState.downSpikeAntic = false;
		if (HeroPerformanceRegion.IsPerforming)
		{
			EventRegister.SendEvent(EventRegisterEvents.FsmCancel, null);
		}
		GameCameras.instance.forceCameraAspect.SetFovOffset(0f, 0f, null);
		Action heroLeavingScene = this.HeroLeavingScene;
		if (heroLeavingScene != null)
		{
			heroLeavingScene();
		}
		this.proxyFSM.SendEvent("HeroCtrl-LeavingScene");
		this.inputBlockers.Clear();
		HeroInvincibilitySource.Clear();
		foreach (DamageEnemies damageEnemies in this.damageEnemiesList)
		{
			damageEnemies.ClearLists();
		}
		this.IsStunned = false;
	}

	// Token: 0x06000F78 RID: 3960 RVA: 0x0004A8D4 File Offset: 0x00048AD4
	public void DoSprintSkid()
	{
		this.sprintFSM.SendEventSafe("DO SPRINT SKID");
	}

	// Token: 0x06000F79 RID: 3961 RVA: 0x0004A8E6 File Offset: 0x00048AE6
	public void AddExtraAirMoveVelocity(HeroController.DecayingVelocity velocity)
	{
		this.extraAirMoveVelocities.Add(velocity);
	}

	// Token: 0x06000F7A RID: 3962 RVA: 0x0004A8F4 File Offset: 0x00048AF4
	public void ClearEffectsInstant()
	{
		EventRegister.SendEvent(EventRegisterEvents.ClearEffectsInstant, null);
		this.ClearEffects();
	}

	// Token: 0x06000F7B RID: 3963 RVA: 0x0004A907 File Offset: 0x00048B07
	public void ClearEffects()
	{
		this.ResetLifebloodState();
		this.ClearEffectsLite();
	}

	// Token: 0x06000F7C RID: 3964 RVA: 0x0004A915 File Offset: 0x00048B15
	public void ClearEffectsLite()
	{
		this.StopQuickening();
		this.SetIsMaggoted(false);
		this.ResetAllCrestState(false);
		this.ResetMaggotCharm();
		this.NailImbuement.SetElement(NailElements.None);
		EventRegister.SendEvent(EventRegisterEvents.ClearEffects, null);
	}

	// Token: 0x06000F7D RID: 3965 RVA: 0x0004A948 File Offset: 0x00048B48
	private void ResetMaggotCharm()
	{
		this.playerData.MaggotCharmHits = 0;
		this.maggotCharmTimer = 0f;
		EventRegister.SendEvent(EventRegisterEvents.MaggotCheck, null);
	}

	// Token: 0x06000F7E RID: 3966 RVA: 0x0004A96C File Offset: 0x00048B6C
	private void StopQuickening()
	{
		if (this.spawnedQuickeningEffect)
		{
			this.spawnedQuickeningEffect.StopParticleSystems();
			this.spawnedQuickeningEffect = null;
		}
		this.spriteFlash.CancelRepeatingFlash(this.quickeningFlash);
		this.sprintFSMIsQuickening.Value = false;
		this.toolsFSMIsQuickening.Value = false;
		this.quickeningTimeLeft = 0f;
	}

	// Token: 0x06000F7F RID: 3967 RVA: 0x0004A9CC File Offset: 0x00048BCC
	public void SilkTaunted()
	{
		this.silkTauntEffectTimeLeft = 6f;
	}

	// Token: 0x06000F80 RID: 3968 RVA: 0x0004A9D9 File Offset: 0x00048BD9
	public bool SilkTauntEffectConsume()
	{
		bool result = this.silkTauntEffectTimeLeft > 0f;
		this.silkTauntEffectTimeLeft = 0f;
		return result;
	}

	// Token: 0x06000F81 RID: 3969 RVA: 0x0004A9F3 File Offset: 0x00048BF3
	public void RingTaunted()
	{
		this.ringTauntEffectTimeLeft = 6f;
	}

	// Token: 0x06000F82 RID: 3970 RVA: 0x0004AA00 File Offset: 0x00048C00
	public bool RingTauntEffectConsume()
	{
		bool result = this.ringTauntEffectTimeLeft > 0f;
		this.ringTauntEffectTimeLeft = 0f;
		return result;
	}

	// Token: 0x06000F83 RID: 3971 RVA: 0x0004AA1A File Offset: 0x00048C1A
	public void ResetTauntEffects()
	{
		this.silkTauntEffectTimeLeft = 0f;
		this.ringTauntEffectTimeLeft = 0f;
	}

	// Token: 0x06000F84 RID: 3972 RVA: 0x0004AA32 File Offset: 0x00048C32
	private void ResetLavaBell()
	{
		this.lavaBellCooldownTimeLeft = 0f;
		this.spawnedLavaBellRechargeEffect.SetActive(false);
		EventRegister.SendEvent(EventRegisterEvents.LavaBellReset, null);
	}

	// Token: 0x06000F85 RID: 3973 RVA: 0x0004AA56 File Offset: 0x00048C56
	public HeroVibrationController GetVibrationCtrl()
	{
		return this.vibrationCtrl;
	}

	// Token: 0x06000F86 RID: 3974 RVA: 0x0004AA60 File Offset: 0x00048C60
	public void ReportPoisonHealthAdded()
	{
		int poisonHealthCount = this.PoisonHealthCount;
		this.PoisonHealthCount = poisonHealthCount + 1;
		if (this.spriteFlash.IsFlashing(true, this.poisonHealthFlash))
		{
			return;
		}
		this.poisonHealthFlash = this.spriteFlash.Flash(Gameplay.PoisonPouchHeroTintColour, 0.7f, 0.2f, 0.01f, 0.22f, 0f, true, 0, 1, true);
	}

	// Token: 0x06000F87 RID: 3975 RVA: 0x0004AAC8 File Offset: 0x00048CC8
	public void ReportPoisonHealthRemoved()
	{
		int poisonHealthCount = this.PoisonHealthCount;
		this.PoisonHealthCount = poisonHealthCount - 1;
		if (this.PoisonHealthCount < 0)
		{
			this.PoisonHealthCount = 0;
		}
		if (this.PoisonHealthCount == 0)
		{
			this.spriteFlash.CancelRepeatingFlash(this.poisonHealthFlash);
		}
	}

	// Token: 0x06000F8A RID: 3978 RVA: 0x0004AC8C File Offset: 0x00048E8C
	Transform ITagDamageTakerOwner.get_transform()
	{
		return base.transform;
	}

	// Token: 0x06000F8E RID: 3982 RVA: 0x0004ACCC File Offset: 0x00048ECC
	[CompilerGenerated]
	private bool <DoMossToolHit>g__DoHitForMossTool|890_0(ToolItem tool, ref int hitTracker, int silk)
	{
		if (!tool || !tool.Status.IsEquipped)
		{
			return false;
		}
		hitTracker++;
		if (hitTracker >= 2)
		{
			hitTracker = 0;
			this.AddSilk(silk, false, SilkSpool.SilkAddSource.Moss);
		}
		else
		{
			GameCameras.instance.silkSpool.SetMossState(hitTracker, silk);
		}
		return true;
	}

	// Token: 0x06000F90 RID: 3984 RVA: 0x0004AD29 File Offset: 0x00048F29
	[CompilerGenerated]
	private void <Respawn>g__SetFacingForSpawnPoint|983_0(ref HeroController.<>c__DisplayClass983_0 A_1)
	{
		if (A_1.respawnMarker)
		{
			if (A_1.respawnMarker.respawnFacingRight)
			{
				this.FaceRight();
				return;
			}
			this.FaceLeft();
		}
	}

	// Token: 0x04000C8E RID: 3214
	public const int GROUND_LAYERS = 8448;

	// Token: 0x04000C8F RID: 3215
	private const float HAZARD_LOOP_COOLDOWN = 0.5f;

	// Token: 0x04000C90 RID: 3216
	private bool verboseMode;

	// Token: 0x04000C91 RID: 3217
	public float RUN_SPEED;

	// Token: 0x04000C92 RID: 3218
	public float WALK_SPEED;

	// Token: 0x04000C93 RID: 3219
	public float JUMP_SPEED;

	// Token: 0x04000C94 RID: 3220
	public float MIN_JUMP_SPEED;

	// Token: 0x04000C95 RID: 3221
	public int JUMP_STEPS;

	// Token: 0x04000C96 RID: 3222
	public int JUMP_STEPS_MIN;

	// Token: 0x04000C97 RID: 3223
	public float AIR_HANG_GRAVITY;

	// Token: 0x04000C98 RID: 3224
	public float AIR_HANG_ACCEL;

	// Token: 0x04000C99 RID: 3225
	public float SHUTTLECOCK_SPEED;

	// Token: 0x04000C9A RID: 3226
	public float FLOAT_SPEED;

	// Token: 0x04000C9B RID: 3227
	public int DOUBLE_JUMP_RISE_STEPS;

	// Token: 0x04000C9C RID: 3228
	public int DOUBLE_JUMP_FALL_STEPS;

	// Token: 0x04000C9D RID: 3229
	public float JUMP_ABILITY_GROUND_RAY_LENGTH;

	// Token: 0x04000C9E RID: 3230
	public float WALLJUMP_RAY_LENGTH;

	// Token: 0x04000C9F RID: 3231
	public float WALLJUMP_BROLLY_RAY_LENGTH;

	// Token: 0x04000CA0 RID: 3232
	public int WJLOCK_STEPS_SHORT;

	// Token: 0x04000CA1 RID: 3233
	public int WJLOCK_STEPS_LONG;

	// Token: 0x04000CA2 RID: 3234
	public int WJLOCK_CHAIN_STEPS;

	// Token: 0x04000CA3 RID: 3235
	public float WJ_KICKOFF_SPEED;

	// Token: 0x04000CA4 RID: 3236
	public int WALL_STICKY_STEPS;

	// Token: 0x04000CA5 RID: 3237
	public float DASH_SPEED;

	// Token: 0x04000CA6 RID: 3238
	public float DASH_TIME;

	// Token: 0x04000CA7 RID: 3239
	public float AIR_DASH_TIME;

	// Token: 0x04000CA8 RID: 3240
	public float DOWN_DASH_TIME;

	// Token: 0x04000CA9 RID: 3241
	public int DASH_QUEUE_STEPS;

	// Token: 0x04000CAA RID: 3242
	public float DASH_COOLDOWN;

	// Token: 0x04000CAB RID: 3243
	public float WALLSLIDE_STICK_TIME;

	// Token: 0x04000CAC RID: 3244
	public float WALLSLIDE_ACCEL;

	// Token: 0x04000CAD RID: 3245
	public float WALLSLIDE_SHUTTLECOCK_VEL;

	// Token: 0x04000CAE RID: 3246
	public float WALLCLING_DECEL;

	// Token: 0x04000CAF RID: 3247
	public float WALLCLING_COOLDOWN;

	// Token: 0x04000CB0 RID: 3248
	public float NAIL_CHARGE_TIME;

	// Token: 0x04000CB1 RID: 3249
	public float NAIL_CHARGE_TIME_QUICK;

	// Token: 0x04000CB2 RID: 3250
	public ToolItem NailChargeTimeQuickTool;

	// Token: 0x04000CB3 RID: 3251
	public float NAIL_CHARGE_BEGIN_TIME;

	// Token: 0x04000CB4 RID: 3252
	public float NAIL_CHARGE_BEGIN_TIME_QUICK;

	// Token: 0x04000CB5 RID: 3253
	public float TIME_TO_ENTER_SCENE_BOT;

	// Token: 0x04000CB6 RID: 3254
	public float SPEED_TO_ENTER_SCENE_HOR;

	// Token: 0x04000CB7 RID: 3255
	public float SPEED_TO_ENTER_SCENE_UP;

	// Token: 0x04000CB8 RID: 3256
	public float SPEED_TO_ENTER_SCENE_DOWN;

	// Token: 0x04000CB9 RID: 3257
	public float DEFAULT_GRAVITY;

	// Token: 0x04000CBA RID: 3258
	public float UNDERWATER_GRAVITY;

	// Token: 0x04000CBB RID: 3259
	public float ALT_ATTACK_RESET;

	// Token: 0x04000CBC RID: 3260
	public int DOWNSPIKE_REBOUND_STEPS;

	// Token: 0x04000CBD RID: 3261
	public float DOWNSPIKE_REBOUND_SPEED;

	// Token: 0x04000CBE RID: 3262
	public float DOWNSPIKE_LAND_RECOVERY_TIME;

	// Token: 0x04000CBF RID: 3263
	public float DOWNSPIKE_ANTIC_DECELERATION;

	// Token: 0x04000CC0 RID: 3264
	public MinMaxFloat DOWNSPIKE_ANTIC_CLAMP_VEL_Y;

	// Token: 0x04000CC1 RID: 3265
	public float JUMP_SPEED_UPDRAFT_EXIT;

	// Token: 0x04000CC2 RID: 3266
	public int DOWNSPIKE_INVULNERABILITY_STEPS = 2;

	// Token: 0x04000CC3 RID: 3267
	public int DOWNSPIKE_INVULNERABILITY_STEPS_LONG = 10;

	// Token: 0x04000CC4 RID: 3268
	public float BOUNCE_TIME;

	// Token: 0x04000CC5 RID: 3269
	public float BOUNCE_VELOCITY;

	// Token: 0x04000CC6 RID: 3270
	public float SHROOM_BOUNCE_VELOCITY;

	// Token: 0x04000CC7 RID: 3271
	public float RECOIL_HOR_VELOCITY;

	// Token: 0x04000CC8 RID: 3272
	public float RECOIL_HOR_VELOCITY_LONG;

	// Token: 0x04000CC9 RID: 3273
	public float RECOIL_HOR_VELOCITY_DRILLDASH;

	// Token: 0x04000CCA RID: 3274
	public int RECOIL_HOR_STEPS;

	// Token: 0x04000CCB RID: 3275
	public float RECOIL_DOWN_VELOCITY;

	// Token: 0x04000CCC RID: 3276
	public float BIG_FALL_TIME;

	// Token: 0x04000CCD RID: 3277
	public float HARD_LANDING_TIME;

	// Token: 0x04000CCE RID: 3278
	public float DOWN_DASH_RECOVER_TIME;

	// Token: 0x04000CCF RID: 3279
	public float MAX_FALL_VELOCITY;

	// Token: 0x04000CD0 RID: 3280
	public float MAX_FALL_VELOCITY_WEIGHTED;

	// Token: 0x04000CD1 RID: 3281
	public float MAX_FALL_VELOCITY_DJUMP;

	// Token: 0x04000CD2 RID: 3282
	public float RECOIL_DURATION;

	// Token: 0x04000CD3 RID: 3283
	public float RECOIL_VELOCITY;

	// Token: 0x04000CD4 RID: 3284
	public float DAMAGE_FREEZE_DOWN;

	// Token: 0x04000CD5 RID: 3285
	public float DAMAGE_FREEZE_WAIT;

	// Token: 0x04000CD6 RID: 3286
	public float DAMAGE_FREEZE_UP;

	// Token: 0x04000CD7 RID: 3287
	public float DAMAGE_FREEZE_SPEED;

	// Token: 0x04000CD8 RID: 3288
	public float INVUL_TIME;

	// Token: 0x04000CD9 RID: 3289
	public float INVUL_TIME_PARRY;

	// Token: 0x04000CDA RID: 3290
	public float INVUL_TIME_QUAKE;

	// Token: 0x04000CDB RID: 3291
	public float INVUL_TIME_CROSS_STITCH;

	// Token: 0x04000CDC RID: 3292
	public float INVUL_TIME_SILKDASH;

	// Token: 0x04000CDD RID: 3293
	public float REVENGE_WINDOW_TIME;

	// Token: 0x04000CDE RID: 3294
	public float DASHCOMBO_WINDOW_TIME;

	// Token: 0x04000CDF RID: 3295
	public float CAST_RECOIL_VELOCITY;

	// Token: 0x04000CE0 RID: 3296
	public float WALLSLIDE_CLIP_DELAY;

	// Token: 0x04000CE1 RID: 3297
	[Space]
	public float QUICKENING_DURATION;

	// Token: 0x04000CE2 RID: 3298
	public float QUICKENING_RUN_SPEED;

	// Token: 0x04000CE3 RID: 3299
	public float QUICKENING_WALK_SPEED;

	// Token: 0x04000CE4 RID: 3300
	[Space]
	public float FIRST_SILK_REGEN_DELAY;

	// Token: 0x04000CE5 RID: 3301
	public float FIRST_SILK_REGEN_DURATION;

	// Token: 0x04000CE6 RID: 3302
	public float SILK_REGEN_DELAY;

	// Token: 0x04000CE7 RID: 3303
	public float SILK_REGEN_DURATION;

	// Token: 0x04000CE8 RID: 3304
	[Space]
	public float CURSED_SILK_EAT_DELAY_FIRST;

	// Token: 0x04000CE9 RID: 3305
	public float CURSED_SILK_EAT_DELAY;

	// Token: 0x04000CEA RID: 3306
	public float CURSED_SILK_EAT_DURATION;

	// Token: 0x04000CEB RID: 3307
	[Space]
	public float MAGGOTED_SILK_EAT_DELAY_FIRST;

	// Token: 0x04000CEC RID: 3308
	public float MAGGOTED_SILK_EAT_DELAY;

	// Token: 0x04000CED RID: 3309
	public float MAGGOTED_SILK_EAT_DURATION;

	// Token: 0x04000CEE RID: 3310
	private int JUMP_QUEUE_STEPS = 2;

	// Token: 0x04000CEF RID: 3311
	private int JUMP_RELEASE_QUEUE_STEPS = 2;

	// Token: 0x04000CF0 RID: 3312
	private int DOUBLE_JUMP_QUEUE_STEPS = 10;

	// Token: 0x04000CF1 RID: 3313
	private int ATTACK_QUEUE_STEPS = 8;

	// Token: 0x04000CF2 RID: 3314
	private int TOOLTHROW_QUEUE_STEPS = 5;

	// Token: 0x04000CF3 RID: 3315
	private int HARPOON_QUEUE_STEPS = 8;

	// Token: 0x04000CF4 RID: 3316
	private float LOOK_DELAY = 0.85f;

	// Token: 0x04000CF5 RID: 3317
	private float LOOK_ANIM_DELAY = 0.25f;

	// Token: 0x04000CF6 RID: 3318
	private float DEATH_WAIT = 4f;

	// Token: 0x04000CF7 RID: 3319
	private const float FROST_DEATH_WAIT = 5.1f;

	// Token: 0x04000CF8 RID: 3320
	private float HAZARD_DEATH_CHECK_TIME = 3f;

	// Token: 0x04000CF9 RID: 3321
	private float FLOATING_CHECK_TIME = 0.18f;

	// Token: 0x04000CFA RID: 3322
	private int LANDING_BUFFER_STEPS = 5;

	// Token: 0x04000CFB RID: 3323
	private int LEDGE_BUFFER_STEPS = 4;

	// Token: 0x04000CFC RID: 3324
	private int HEAD_BUMP_STEPS = 3;

	// Token: 0x04000CFD RID: 3325
	private float FIND_GROUND_POINT_DISTANCE = 10f;

	// Token: 0x04000CFE RID: 3326
	private float FIND_GROUND_POINT_DISTANCE_EXT = 1000f;

	// Token: 0x04000CFF RID: 3327
	private const float MAX_SILK_REGEN_RATE = 0.03f;

	// Token: 0x04000D00 RID: 3328
	private double HARPOON_DASH_TIME;

	// Token: 0x04000D01 RID: 3329
	[Space]
	public ActorStates hero_state;

	// Token: 0x04000D02 RID: 3330
	public ActorStates prev_hero_state;

	// Token: 0x04000D03 RID: 3331
	public HeroTransitionState transitionState;

	// Token: 0x04000D04 RID: 3332
	public DamageMode damageMode;

	// Token: 0x04000D06 RID: 3334
	private HeroLockStates unlockRequests;

	// Token: 0x04000D07 RID: 3335
	public float move_input;

	// Token: 0x04000D08 RID: 3336
	public float vertical_input;

	// Token: 0x04000D09 RID: 3337
	public float controller_deadzone = 0.2f;

	// Token: 0x04000D0A RID: 3338
	public Vector2 current_velocity;

	// Token: 0x04000D0B RID: 3339
	private bool isGameplayScene;

	// Token: 0x04000D0C RID: 3340
	public bool isEnteringFirstLevel;

	// Token: 0x04000D0D RID: 3341
	private bool isDashStabBouncing;

	// Token: 0x04000D0E RID: 3342
	private bool canSoftLand;

	// Token: 0x04000D0F RID: 3343
	private int softLandTime;

	// Token: 0x04000D10 RID: 3344
	private bool blockSteepSlopes;

	// Token: 0x04000D11 RID: 3345
	private bool queuedLifeBloodState;

	// Token: 0x04000D12 RID: 3346
	public Vector2 slashOffset;

	// Token: 0x04000D13 RID: 3347
	public Vector2 upSlashOffset;

	// Token: 0x04000D14 RID: 3348
	public Vector2 downwardSlashOffset;

	// Token: 0x04000D15 RID: 3349
	public Vector2 spell1Offset;

	// Token: 0x04000D16 RID: 3350
	private int jump_steps;

	// Token: 0x04000D17 RID: 3351
	private int jumped_steps;

	// Token: 0x04000D18 RID: 3352
	private int doubleJump_steps;

	// Token: 0x04000D19 RID: 3353
	private float dash_timer;

	// Token: 0x04000D1A RID: 3354
	private float dash_time;

	// Token: 0x04000D1B RID: 3355
	private float attack_time;

	// Token: 0x04000D1C RID: 3356
	private float attack_cooldown;

	// Token: 0x04000D1D RID: 3357
	private float throwToolCooldown;

	// Token: 0x04000D1E RID: 3358
	private int downspike_rebound_steps;

	// Token: 0x04000D1F RID: 3359
	private Vector2 transition_vel;

	// Token: 0x04000D20 RID: 3360
	private float altAttackTime;

	// Token: 0x04000D21 RID: 3361
	private float lookDelayTimer;

	// Token: 0x04000D22 RID: 3362
	private float bounceTimer;

	// Token: 0x04000D24 RID: 3364
	private float hardLandingTimer;

	// Token: 0x04000D25 RID: 3365
	private float dashLandingTimer;

	// Token: 0x04000D26 RID: 3366
	private float recoilTimer;

	// Token: 0x04000D27 RID: 3367
	private int recoilStepsLeft;

	// Token: 0x04000D28 RID: 3368
	private int landingBufferSteps;

	// Token: 0x04000D29 RID: 3369
	private int dashQueueSteps;

	// Token: 0x04000D2A RID: 3370
	private bool dashQueuing;

	// Token: 0x04000D2B RID: 3371
	private float shadowDashTimer;

	// Token: 0x04000D2C RID: 3372
	private float dashCooldownTimer;

	// Token: 0x04000D2D RID: 3373
	private float nailChargeTimer;

	// Token: 0x04000D2E RID: 3374
	private int wallLockSteps;

	// Token: 0x04000D2F RID: 3375
	private int wallJumpChainStepsLeft;

	// Token: 0x04000D30 RID: 3376
	private float wallslideClipTimer;

	// Token: 0x04000D31 RID: 3377
	private float hardLandFailSafeTimer;

	// Token: 0x04000D32 RID: 3378
	private float hazardDeathTimer;

	// Token: 0x04000D33 RID: 3379
	private float floatingBufferTimer;

	// Token: 0x04000D34 RID: 3380
	private float attackDuration;

	// Token: 0x04000D35 RID: 3381
	private AttackDirection prevAttackDir = (AttackDirection)(-1);

	// Token: 0x04000D36 RID: 3382
	public float parryInvulnTimer;

	// Token: 0x04000D37 RID: 3383
	public float revengeWindowTimer;

	// Token: 0x04000D38 RID: 3384
	private float wandererDashComboWindowTimer;

	// Token: 0x04000D39 RID: 3385
	public float downSpikeTimer;

	// Token: 0x04000D3A RID: 3386
	public float downSpikeRecoveryTimer;

	// Token: 0x04000D3B RID: 3387
	private float toolThrowTime;

	// Token: 0x04000D3C RID: 3388
	private float toolThrowDuration;

	// Token: 0x04000D3D RID: 3389
	private float wallStickTimer;

	// Token: 0x04000D3E RID: 3390
	private float wallStickStartVelocity;

	// Token: 0x04000D3F RID: 3391
	private float lavaBellCooldownTimeLeft;

	// Token: 0x04000D40 RID: 3392
	private float shuttlecockTime;

	// Token: 0x04000D41 RID: 3393
	private float shuttlecockTimeResetTimer;

	// Token: 0x04000D42 RID: 3394
	private float wallClingCooldownTimer;

	// Token: 0x04000D43 RID: 3395
	private bool evadingDidClash;

	// Token: 0x04000D44 RID: 3396
	private bool doMaxSilkRegen;

	// Token: 0x04000D45 RID: 3397
	private float maxSilkRegenTimer;

	// Token: 0x04000D46 RID: 3398
	private int shuttleCockJumpSteps;

	// Token: 0x04000D47 RID: 3399
	[Space(6f)]
	[Header("Effect Prefabs")]
	public GameObject nailTerrainImpactEffectPrefab;

	// Token: 0x04000D48 RID: 3400
	public GameObject nailTerrainImpactEffectPrefabDownSpike;

	// Token: 0x04000D49 RID: 3401
	public Transform downspikeEffectPrefabSpawnPoint;

	// Token: 0x04000D4A RID: 3402
	public GameObject takeHitSingleEffectPrefab;

	// Token: 0x04000D4B RID: 3403
	public GameObject takeHitDoubleEffectPrefab;

	// Token: 0x04000D4C RID: 3404
	public GameObject takeHitDoubleBlackThreadEffectPrefab;

	// Token: 0x04000D4D RID: 3405
	public GameObject takeHitBlackHealthNullifyPrefab;

	// Token: 0x04000D4E RID: 3406
	public GameObject takeHitDoubleFlameEffectPrefab;

	// Token: 0x04000D4F RID: 3407
	public GameObject softLandingEffectPrefab;

	// Token: 0x04000D50 RID: 3408
	public GameObject hardLandingEffectPrefab;

	// Token: 0x04000D51 RID: 3409
	public RunEffects runEffectPrefab;

	// Token: 0x04000D52 RID: 3410
	public DashEffect backDashPrefab;

	// Token: 0x04000D53 RID: 3411
	public JumpEffects jumpEffectPrefab;

	// Token: 0x04000D54 RID: 3412
	public GameObject jumpTrailPrefab;

	// Token: 0x04000D55 RID: 3413
	public GameObject fallEffectPrefab;

	// Token: 0x04000D56 RID: 3414
	public ParticleSystem wallslideDustPrefab;

	// Token: 0x04000D57 RID: 3415
	public GameObject artChargeEffect;

	// Token: 0x04000D58 RID: 3416
	public GameObject artChargedEffect;

	// Token: 0x04000D59 RID: 3417
	public tk2dSpriteAnimator artChargedEffectAnim;

	// Token: 0x04000D5A RID: 3418
	public GameObject downspikeBurstPrefab;

	// Token: 0x04000D5B RID: 3419
	public GameObject dashBurstPrefab;

	// Token: 0x04000D5C RID: 3420
	public ParticleSystem dashParticles;

	// Token: 0x04000D5D RID: 3421
	public GameObject wallPuffPrefab;

	// Token: 0x04000D5E RID: 3422
	public GameObject backflipPuffPrefab;

	// Token: 0x04000D5F RID: 3423
	public GameObject airDashEffect;

	// Token: 0x04000D60 RID: 3424
	public GameObject walldashKickoffEffect;

	// Token: 0x04000D61 RID: 3425
	public GameObject umbrellaEffect;

	// Token: 0x04000D62 RID: 3426
	public GameObject doubleJumpEffectPrefab;

	// Token: 0x04000D63 RID: 3427
	public GameObject canBindEffect;

	// Token: 0x04000D64 RID: 3428
	public PlayParticleEffects quickeningEffectPrefab;

	// Token: 0x04000D65 RID: 3429
	public PlayParticleEffects quickeningPoisonEffectPrefab;

	// Token: 0x04000D66 RID: 3430
	public PlayParticleEffects maggotEffectPrefab;

	// Token: 0x04000D67 RID: 3431
	public PlayParticleEffects frostedEffect;

	// Token: 0x04000D68 RID: 3432
	public NoiseMaker SlashNoiseMakerFront;

	// Token: 0x04000D69 RID: 3433
	public NoiseMaker SlashNoiseMakerAbove;

	// Token: 0x04000D6A RID: 3434
	public NoiseMaker SlashNoiseMakerBelow;

	// Token: 0x04000D6B RID: 3435
	public GameObject luckyDiceShieldEffectPrefab;

	// Token: 0x04000D6C RID: 3436
	[SerializeField]
	private GameObject mossDamageEffectPrefab;

	// Token: 0x04000D6D RID: 3437
	[SerializeField]
	private GameObject witchDamageSpawn;

	// Token: 0x04000D6E RID: 3438
	[SerializeField]
	private GameObject silkAcidPrefab;

	// Token: 0x04000D6F RID: 3439
	[SerializeField]
	private GameObject voidAcidPrefab;

	// Token: 0x04000D70 RID: 3440
	[SerializeField]
	private GameObject frostEnterPrefab;

	// Token: 0x04000D71 RID: 3441
	[SerializeField]
	private GameObject heatEnterPrefab;

	// Token: 0x04000D72 RID: 3442
	[SerializeField]
	private GameObject maggotEnterPrefab;

	// Token: 0x04000D73 RID: 3443
	[SerializeField]
	private ParticleSystem runningWaterEffect;

	// Token: 0x04000D74 RID: 3444
	[SerializeField]
	private GameObject wallClingEffect;

	// Token: 0x04000D75 RID: 3445
	[SerializeField]
	private GameObject afterDamageEffectsPrefab;

	// Token: 0x04000D76 RID: 3446
	[Space(6f)]
	[Header("Hero Death")]
	public GameObject spikeDeathPrefab;

	// Token: 0x04000D77 RID: 3447
	public GameObject acidDeathPrefab;

	// Token: 0x04000D78 RID: 3448
	public GameObject lavaDeathPrefab;

	// Token: 0x04000D79 RID: 3449
	public GameObject coalDeathPrefab;

	// Token: 0x04000D7A RID: 3450
	public GameObject zapDeathPrefab;

	// Token: 0x04000D7B RID: 3451
	public GameObject sinkDeathPrefab;

	// Token: 0x04000D7C RID: 3452
	public GameObject steamDeathPrefab;

	// Token: 0x04000D7D RID: 3453
	public GameObject coalSpikeDeathPrefab;

	// Token: 0x04000D7E RID: 3454
	public GameObject heroDeathPrefab;

	// Token: 0x04000D7F RID: 3455
	public GameObject heroDeathCursedPrefab;

	// Token: 0x04000D80 RID: 3456
	public GameObject heroDeathNonLethalPrefab;

	// Token: 0x04000D81 RID: 3457
	public GameObject heroDeathMemoryPrefab;

	// Token: 0x04000D82 RID: 3458
	[SerializeField]
	private GameObject heroDeathFrostPrefab;

	// Token: 0x04000D83 RID: 3459
	[Space(6f)]
	[Header("Hero Other")]
	public GameObject cutscenePrefab;

	// Token: 0x04000D84 RID: 3460
	private GameManager gm;

	// Token: 0x04000D85 RID: 3461
	private Rigidbody2D rb2d;

	// Token: 0x04000D86 RID: 3462
	private Collider2D col2d;

	// Token: 0x04000D87 RID: 3463
	private MeshRenderer renderer;

	// Token: 0x04000D88 RID: 3464
	private new Transform transform;

	// Token: 0x04000D89 RID: 3465
	private HeroAnimationController animCtrl;

	// Token: 0x04000D8A RID: 3466
	public HeroBox heroBox;

	// Token: 0x04000D8B RID: 3467
	public HeroControllerStates cState;

	// Token: 0x04000D8C RID: 3468
	public PlayerData playerData;

	// Token: 0x04000D8D RID: 3469
	private HeroAudioController audioCtrl;

	// Token: 0x04000D8E RID: 3470
	private AudioSource audioSource;

	// Token: 0x04000D8F RID: 3471
	[HideInInspector]
	public UIManager ui;

	// Token: 0x04000D90 RID: 3472
	private InputHandler inputHandler;

	// Token: 0x04000D92 RID: 3474
	public PlayMakerFSM damageEffectFSM;

	// Token: 0x04000D94 RID: 3476
	private HeroVibrationController vibrationCtrl;

	// Token: 0x04000D95 RID: 3477
	public PlayMakerFSM sprintFSM;

	// Token: 0x04000D96 RID: 3478
	public PlayMakerFSM toolsFSM;

	// Token: 0x04000D97 RID: 3479
	private FsmBool sprintFSMIsQuickening;

	// Token: 0x04000D98 RID: 3480
	private FsmBool toolsFSMIsQuickening;

	// Token: 0x04000D99 RID: 3481
	private FsmBool sprintMasterActiveBool;

	// Token: 0x04000D9A RID: 3482
	private FsmFloat sprintSpeedAddFloat;

	// Token: 0x04000D9B RID: 3483
	public PlayMakerFSM mantleFSM;

	// Token: 0x04000D9C RID: 3484
	public PlayMakerFSM umbrellaFSM;

	// Token: 0x04000D9D RID: 3485
	public PlayMakerFSM silkSpecialFSM;

	// Token: 0x04000D9E RID: 3486
	public PlayMakerFSM crestAttacksFSM;

	// Token: 0x04000D9F RID: 3487
	public PlayMakerFSM harpoonDashFSM;

	// Token: 0x04000DA0 RID: 3488
	public PlayMakerFSM superJumpFSM;

	// Token: 0x04000DA1 RID: 3489
	public PlayMakerFSM bellBindFSM;

	// Token: 0x04000DA2 RID: 3490
	public PlayMakerFSM wallScrambleFSM;

	// Token: 0x04000DA3 RID: 3491
	private ParticleSystem dashParticleSystem;

	// Token: 0x04000DA4 RID: 3492
	private InvulnerablePulse invPulse;

	// Token: 0x04000DA5 RID: 3493
	private SpriteFlash spriteFlash;

	// Token: 0x04000DA7 RID: 3495
	private float prevGravityScale;

	// Token: 0x04000DA8 RID: 3496
	private Vector2 recoilVector;

	// Token: 0x04000DA9 RID: 3497
	private Vector2 lastInputState;

	// Token: 0x04000DAA RID: 3498
	private Vector2 velocity_crt;

	// Token: 0x04000DAB RID: 3499
	private Vector2 velocity_prev;

	// Token: 0x04000DAC RID: 3500
	public GatePosition gatePosition;

	// Token: 0x04000DAE RID: 3502
	private bool hardLanded;

	// Token: 0x04000DAF RID: 3503
	private bool fallRumble;

	// Token: 0x04000DB0 RID: 3504
	public bool acceptingInput;

	// Token: 0x04000DB1 RID: 3505
	private bool fallTrailGenerated;

	// Token: 0x04000DB2 RID: 3506
	private float dashBumpCorrection;

	// Token: 0x04000DB3 RID: 3507
	public bool controlReqlinquished;

	// Token: 0x04000DB4 RID: 3508
	public bool enterWithoutInput;

	// Token: 0x04000DB5 RID: 3509
	public bool skipNormalEntry;

	// Token: 0x04000DB6 RID: 3510
	private float downspike_rebound_xspeed;

	// Token: 0x04000DB7 RID: 3511
	private float downSpikeHorizontalSpeed;

	// Token: 0x04000DB8 RID: 3512
	private float shuttlecockSpeed;

	// Token: 0x04000DB9 RID: 3513
	private float currentGravity;

	// Token: 0x04000DBA RID: 3514
	private bool didAirHang;

	// Token: 0x04000DBB RID: 3515
	private int updraftsEntered;

	// Token: 0x04000DBC RID: 3516
	private float harpoonDashCooldown;

	// Token: 0x04000DBD RID: 3517
	private EndBeta endBeta;

	// Token: 0x04000DBE RID: 3518
	private int lastLookDirection;

	// Token: 0x04000DBF RID: 3519
	private int controlRelinquishedFrame;

	// Token: 0x04000DC0 RID: 3520
	private int jumpQueueSteps;

	// Token: 0x04000DC1 RID: 3521
	private bool jumpQueuing;

	// Token: 0x04000DC2 RID: 3522
	private int doubleJumpQueueSteps;

	// Token: 0x04000DC3 RID: 3523
	private bool doubleJumpQueuing;

	// Token: 0x04000DC4 RID: 3524
	private int jumpReleaseQueueSteps;

	// Token: 0x04000DC5 RID: 3525
	private bool jumpReleaseQueuing;

	// Token: 0x04000DC6 RID: 3526
	private int attackQueueSteps;

	// Token: 0x04000DC7 RID: 3527
	private bool attackQueuing;

	// Token: 0x04000DC8 RID: 3528
	private int harpoonQueueSteps;

	// Token: 0x04000DC9 RID: 3529
	private bool harpoonQueuing;

	// Token: 0x04000DCA RID: 3530
	private int toolThrowQueueSteps;

	// Token: 0x04000DCB RID: 3531
	private bool toolThrowQueueing;

	// Token: 0x04000DCC RID: 3532
	public bool touchingWallL;

	// Token: 0x04000DCD RID: 3533
	public bool touchingWallR;

	// Token: 0x04000DCE RID: 3534
	private GameObject touchingWallObj;

	// Token: 0x04000DCF RID: 3535
	private bool wallSlidingL;

	// Token: 0x04000DD0 RID: 3536
	private bool wallSlidingR;

	// Token: 0x04000DD1 RID: 3537
	private bool airDashed;

	// Token: 0x04000DD2 RID: 3538
	public bool dashingDown;

	// Token: 0x04000DD3 RID: 3539
	private bool startWithWallslide;

	// Token: 0x04000DD4 RID: 3540
	private bool startWithJump;

	// Token: 0x04000DD5 RID: 3541
	private bool startWithAnyJump;

	// Token: 0x04000DD6 RID: 3542
	private bool startWithTinyJump;

	// Token: 0x04000DD7 RID: 3543
	private bool startWithShuttlecock;

	// Token: 0x04000DD8 RID: 3544
	private bool startWithFullJump;

	// Token: 0x04000DD9 RID: 3545
	private bool startWithFlipJump;

	// Token: 0x04000DDA RID: 3546
	private bool startWithBackflipJump;

	// Token: 0x04000DDB RID: 3547
	private bool startWithBrolly;

	// Token: 0x04000DDC RID: 3548
	private bool startWithDoubleJump;

	// Token: 0x04000DDD RID: 3549
	private bool startWithWallsprintLaunch;

	// Token: 0x04000DDE RID: 3550
	private bool startWithDash;

	// Token: 0x04000DDF RID: 3551
	private bool dashCurrentFacing;

	// Token: 0x04000DE0 RID: 3552
	private bool startWithDownSpikeBounce;

	// Token: 0x04000DE1 RID: 3553
	private bool startWithDownSpikeBounceSlightlyShort;

	// Token: 0x04000DE2 RID: 3554
	private bool startWithDownSpikeBounceShort;

	// Token: 0x04000DE3 RID: 3555
	private bool startWithDownSpikeEnd;

	// Token: 0x04000DE4 RID: 3556
	private bool startWithHarpoonBounce;

	// Token: 0x04000DE5 RID: 3557
	private bool startWithWitchSprintBounce;

	// Token: 0x04000DE6 RID: 3558
	private bool startWithBalloonBounce;

	// Token: 0x04000DE7 RID: 3559
	private bool startWithUpdraftExit;

	// Token: 0x04000DE8 RID: 3560
	private bool useUpdraftExitJumpSpeed;

	// Token: 0x04000DE9 RID: 3561
	private bool startWithScrambleLeap;

	// Token: 0x04000DEA RID: 3562
	private bool startWithRecoilBack;

	// Token: 0x04000DEB RID: 3563
	private bool startWithRecoilBackLong;

	// Token: 0x04000DEC RID: 3564
	private bool startWithWhipPullRecoil;

	// Token: 0x04000DED RID: 3565
	private bool startWithAttack;

	// Token: 0x04000DEE RID: 3566
	private bool startWithToolThrow;

	// Token: 0x04000DEF RID: 3567
	private bool wallSlashing;

	// Token: 0x04000DF0 RID: 3568
	private bool doubleJumped;

	// Token: 0x04000DF1 RID: 3569
	private bool wallJumpedR;

	// Token: 0x04000DF2 RID: 3570
	private bool wallJumpedL;

	// Token: 0x04000DF3 RID: 3571
	public bool wallLocked;

	// Token: 0x04000DF4 RID: 3572
	private float currentWalljumpSpeed;

	// Token: 0x04000DF5 RID: 3573
	private float walljumpSpeedDecel;

	// Token: 0x04000DF6 RID: 3574
	private int wallUnstickSteps;

	// Token: 0x04000DF7 RID: 3575
	private float recoilVelocity;

	// Token: 0x04000DF8 RID: 3576
	public float conveyorSpeed;

	// Token: 0x04000DF9 RID: 3577
	private bool enteringVertically;

	// Token: 0x04000DFA RID: 3578
	private bool playingWallslideClip;

	// Token: 0x04000DFB RID: 3579
	private bool playedMantisClawClip;

	// Token: 0x04000DFC RID: 3580
	public bool exitedSuperDashing;

	// Token: 0x04000DFD RID: 3581
	public bool exitedQuake;

	// Token: 0x04000DFE RID: 3582
	public bool exitedSprinting;

	// Token: 0x04000DFF RID: 3583
	private bool fallCheckFlagged;

	// Token: 0x04000E00 RID: 3584
	private int ledgeBufferSteps;

	// Token: 0x04000E01 RID: 3585
	private int sprintBufferSteps;

	// Token: 0x04000E02 RID: 3586
	private bool syncBufferSteps;

	// Token: 0x04000E03 RID: 3587
	private double noShuttlecockTime;

	// Token: 0x04000E04 RID: 3588
	private int headBumpSteps;

	// Token: 0x04000E05 RID: 3589
	private bool takeNoDamage;

	// Token: 0x04000E06 RID: 3590
	private bool joniBeam;

	// Token: 0x04000E07 RID: 3591
	public bool fadedSceneIn;

	// Token: 0x04000E08 RID: 3592
	private bool stopWalkingOut;

	// Token: 0x04000E09 RID: 3593
	private bool boundsChecking;

	// Token: 0x04000E0A RID: 3594
	private bool blockerFix;

	// Token: 0x04000E0B RID: 3595
	private bool doFullJump;

	// Token: 0x04000E0C RID: 3596
	private bool startFromMantle;

	// Token: 0x04000E0D RID: 3597
	private bool allowMantle;

	// Token: 0x04000E0E RID: 3598
	private bool allowAttackCancellingDownspikeRecovery;

	// Token: 0x04000E0F RID: 3599
	private bool queuedWallJumpInterrupt;

	// Token: 0x04000E10 RID: 3600
	private bool startWithWallJump;

	// Token: 0x04000E11 RID: 3601
	private bool jumpPressedWhileRelinquished;

	// Token: 0x04000E12 RID: 3602
	private bool regainControlJumpQueued;

	// Token: 0x04000E13 RID: 3603
	private Vector2[] positionHistory;

	// Token: 0x04000E14 RID: 3604
	private bool tilemapTestActive;

	// Token: 0x04000E15 RID: 3605
	private bool allowNailChargingWhileRelinquished;

	// Token: 0x04000E16 RID: 3606
	private bool allowRecoilWhileRelinquished;

	// Token: 0x04000E17 RID: 3607
	private bool recoilZeroVelocity;

	// Token: 0x04000E18 RID: 3608
	private float silkRegenDelayLeft;

	// Token: 0x04000E19 RID: 3609
	private float silkRegenDurationLeft;

	// Token: 0x04000E1A RID: 3610
	private bool isSilkRegenBlocked;

	// Token: 0x04000E1B RID: 3611
	private bool hasSilkSpoolAppeared;

	// Token: 0x04000E1C RID: 3612
	private bool isNextSilkRegenUpgraded;

	// Token: 0x04000E1D RID: 3613
	private bool blockFsmMove;

	// Token: 0x04000E1E RID: 3614
	private HeroController.SilkEatTracker maggotedSilkTracker;

	// Token: 0x04000E1F RID: 3615
	private float maggotCharmTimer;

	// Token: 0x04000E20 RID: 3616
	private int mossCreep1Hits;

	// Token: 0x04000E21 RID: 3617
	private int mossCreep2Hits;

	// Token: 0x04000E22 RID: 3618
	private float frostAmount;

	// Token: 0x04000E23 RID: 3619
	private float frostDamageTimer;

	// Token: 0x04000E24 RID: 3620
	private bool isInFrostRegion;

	// Token: 0x04000E25 RID: 3621
	private SpriteFlash.FlashHandle frostRegionFlash;

	// Token: 0x04000E26 RID: 3622
	private SpriteFlash.FlashHandle frostAnticFlash;

	// Token: 0x04000E29 RID: 3625
	private List<HeroController.DeliveryTimer> currentTimedDeliveries;

	// Token: 0x04000E2A RID: 3626
	private bool doingHazardRespawn;

	// Token: 0x04000E2B RID: 3627
	private double lastHazardRespawnTime;

	// Token: 0x04000E2C RID: 3628
	private int luckyDiceShieldedHits;

	// Token: 0x04000E2D RID: 3629
	private bool forceWalkingSound;

	// Token: 0x04000E2F RID: 3631
	private float silkTauntEffectTimeLeft;

	// Token: 0x04000E30 RID: 3632
	private float ringTauntEffectTimeLeft;

	// Token: 0x04000E31 RID: 3633
	public const float TAUNT_EFFECT_DURATION = 6f;

	// Token: 0x04000E32 RID: 3634
	public const float TAUNT_EFFECT_DAMAGE_MULT = 1.5f;

	// Token: 0x04000E33 RID: 3635
	private int refillSoundSuppressFramesLeft;

	// Token: 0x04000E34 RID: 3636
	private Coroutine recoilRoutine;

	// Token: 0x04000E36 RID: 3638
	private Vector2 groundRayOriginC;

	// Token: 0x04000E37 RID: 3639
	private Vector2 groundRayOriginL;

	// Token: 0x04000E38 RID: 3640
	private Vector2 groundRayOriginR;

	// Token: 0x04000E39 RID: 3641
	private Coroutine takeDamageCoroutine;

	// Token: 0x04000E3A RID: 3642
	private Coroutine tilemapTestCoroutine;

	// Token: 0x04000E3B RID: 3643
	private Coroutine hazardInvulnRoutine;

	// Token: 0x04000E3C RID: 3644
	private Coroutine hazardRespawnRoutine;

	// Token: 0x04000E3D RID: 3645
	[SerializeField]
	[ArrayForEnum(typeof(EnvironmentTypes))]
	private RandomAudioClipTable[] footStepTables;

	// Token: 0x04000E3E RID: 3646
	public AudioClip nailArtChargeComplete;

	// Token: 0x04000E3F RID: 3647
	public AudioClip doubleJumpClip;

	// Token: 0x04000E40 RID: 3648
	public AudioClip mantisClawClip;

	// Token: 0x04000E41 RID: 3649
	public AudioClip downDashCancelClip;

	// Token: 0x04000E42 RID: 3650
	public AudioClip deathImpactClip;

	// Token: 0x04000E43 RID: 3651
	public RandomAudioClipTable attackAudioTable;

	// Token: 0x04000E44 RID: 3652
	public RandomAudioClipTable warriorRageAttackAudioTable;

	// Token: 0x04000E45 RID: 3653
	public RandomAudioClipTable quickSlingAudioTable;

	// Token: 0x04000E46 RID: 3654
	public RandomAudioClipTable woundAudioTable;

	// Token: 0x04000E47 RID: 3655
	public RandomAudioClipTable woundHeavyAudioTable;

	// Token: 0x04000E48 RID: 3656
	public RandomAudioClipTable woundFrostAudioTable;

	// Token: 0x04000E49 RID: 3657
	public RandomAudioClipTable hazardDamageAudioTable;

	// Token: 0x04000E4A RID: 3658
	public RandomAudioClipTable pitFallAudioTable;

	// Token: 0x04000E4B RID: 3659
	public RandomAudioClipTable deathAudioTable;

	// Token: 0x04000E4C RID: 3660
	public RandomAudioClipTable gruntAudioTable;

	// Token: 0x04000E4D RID: 3661
	public AudioSource shuttleCockJumpAudio;

	// Token: 0x04000E4E RID: 3662
	public GameObject shuttleCockJumpEffectPrefab;

	// Token: 0x04000E4F RID: 3663
	[SerializeField]
	private AudioEvent frostedEnterAudio;

	// Token: 0x04000E50 RID: 3664
	[SerializeField]
	private AudioSource frostedAudioLoop;

	// Token: 0x04000E51 RID: 3665
	[SerializeField]
	private float frostedAudioLoopFadeOut;

	// Token: 0x04000E52 RID: 3666
	private Coroutine frostedFadeOutRoutine;

	// Token: 0x04000E53 RID: 3667
	private NailSlash _slashComponent;

	// Token: 0x04000E54 RID: 3668
	private PlayMakerFSM slashFsm;

	// Token: 0x04000E55 RID: 3669
	private DamageEnemies currentSlashDamager;

	// Token: 0x04000E56 RID: 3670
	private Downspike currentDownspike;

	// Token: 0x04000E57 RID: 3671
	private RunEffects runEffect;

	// Token: 0x04000E58 RID: 3672
	private GameObject backDash;

	// Token: 0x04000E59 RID: 3673
	private GameObject fallEffect;

	// Token: 0x04000E5A RID: 3674
	private DashEffect dashEffect;

	// Token: 0x04000E5B RID: 3675
	private GameObject grubberFlyBeam;

	// Token: 0x04000E5C RID: 3676
	private GameObject hazardCorpe;

	// Token: 0x04000E5D RID: 3677
	private GameObject spawnedSilkAcid;

	// Token: 0x04000E5E RID: 3678
	private GameObject spawnedVoidAcid;

	// Token: 0x04000E5F RID: 3679
	private GameObject spawnedFrostEnter;

	// Token: 0x04000E60 RID: 3680
	private GameObject spawnedHeatEnter;

	// Token: 0x04000E61 RID: 3681
	private EnviroRegionListener enviroRegionListener;

	// Token: 0x04000E62 RID: 3682
	private TagDamageTaker tagDamageTaker;

	// Token: 0x04000E63 RID: 3683
	private CharacterBumpCheck bumpChecker;

	// Token: 0x04000E64 RID: 3684
	private PlayParticleEffects spawnedQuickeningEffect;

	// Token: 0x04000E65 RID: 3685
	private SpriteFlash.FlashHandle quickeningFlash;

	// Token: 0x04000E66 RID: 3686
	private PlayParticleEffects spawnedMaggotEffect;

	// Token: 0x04000E67 RID: 3687
	private GameObject spawnedMaggotEnter;

	// Token: 0x04000E68 RID: 3688
	private GameObject spawnedLuckyDiceShieldEffect;

	// Token: 0x04000E69 RID: 3689
	private SpriteFlash.FlashHandle maggotedFlash;

	// Token: 0x04000E6A RID: 3690
	private Coroutine cocoonFloatRoutine;

	// Token: 0x04000E6B RID: 3691
	private MatchXScaleSignOnEnable matchScale;

	// Token: 0x04000E6C RID: 3692
	private SpriteFlash.FlashHandle poisonHealthFlash;

	// Token: 0x04000E6D RID: 3693
	private Dictionary<DeliveryQuestItem, GameObject> spawnedDeliveryEffects = new Dictionary<DeliveryQuestItem, GameObject>();

	// Token: 0x04000E6E RID: 3694
	public PlayMakerFSM vignetteFSM;

	// Token: 0x04000E6F RID: 3695
	public HeroLight heroLight;

	// Token: 0x04000E70 RID: 3696
	public SpriteRenderer vignette;

	// Token: 0x04000E71 RID: 3697
	public PlayMakerFSM dashBurst;

	// Token: 0x04000E72 RID: 3698
	public PlayMakerFSM fsm_thornCounter;

	// Token: 0x04000E73 RID: 3699
	public PlayMakerFSM spellControl;

	// Token: 0x04000E74 RID: 3700
	public PlayMakerFSM fsm_fallTrail;

	// Token: 0x04000E75 RID: 3701
	public PlayMakerFSM fsm_orbitShield;

	// Token: 0x04000E76 RID: 3702
	public PlayMakerFSM fsm_brollyControl;

	// Token: 0x04000E77 RID: 3703
	[SerializeField]
	private Transform toolThrowPoint;

	// Token: 0x04000E78 RID: 3704
	[SerializeField]
	private Transform toolThrowClosePoint;

	// Token: 0x04000E79 RID: 3705
	[SerializeField]
	private Transform toolThrowWallPoint;

	// Token: 0x04000E7A RID: 3706
	[SerializeField]
	private PlayMakerFSM toolEventTarget;

	// Token: 0x04000E7B RID: 3707
	[SerializeField]
	private PlayMakerFSM skillEventTarget;

	// Token: 0x04000E7C RID: 3708
	[SerializeField]
	private DamageTag acidDamageTag;

	// Token: 0x04000E7D RID: 3709
	[SerializeField]
	private DamageTag frostWaterDamageTag;

	// Token: 0x04000E7E RID: 3710
	[SerializeField]
	private GameObject lavaBellEffectPrefab;

	// Token: 0x04000E7F RID: 3711
	[SerializeField]
	private GameObject lavaBellRechargeEffectPrefab;

	// Token: 0x04000E80 RID: 3712
	[SerializeField]
	private GameObject heroPhysicsPusher;

	// Token: 0x04000E81 RID: 3713
	private GameObject spawnedLavaBellRechargeEffect;

	// Token: 0x04000E82 RID: 3714
	[SerializeField]
	private NestedFadeGroupTimedFader rageModeEffectPrefab;

	// Token: 0x04000E83 RID: 3715
	private NestedFadeGroupTimedFader rageModeEffect;

	// Token: 0x04000E84 RID: 3716
	[SerializeField]
	private NestedFadeGroupTimedFader reaperModeEffectPrefab;

	// Token: 0x04000E85 RID: 3717
	private NestedFadeGroupTimedFader reaperModeEffect;

	// Token: 0x04000E86 RID: 3718
	[Space]
	[SerializeField]
	private HeroController.ConfigGroup[] configs;

	// Token: 0x04000E87 RID: 3719
	[SerializeField]
	private HeroController.ConfigGroup[] specialConfigs;

	// Token: 0x04000E8C RID: 3724
	[NonSerialized]
	public bool isHeroInPosition;

	// Token: 0x04000E8D RID: 3725
	private bool isHeroInPrePosition;

	// Token: 0x04000E96 RID: 3734
	private bool jumpReleaseQueueingEnabled;

	// Token: 0x04000E97 RID: 3735
	private ToolItem willThrowTool;

	// Token: 0x04000E98 RID: 3736
	private bool queuedAutoThrowTool;

	// Token: 0x04000E99 RID: 3737
	private const float AUTO_THROW_TOOL_DELAY = 0.15f;

	// Token: 0x04000E9A RID: 3738
	private bool lookDownBlocked;

	// Token: 0x04000E9B RID: 3739
	private HeroController.WarriorCrestStateInfo warriorState;

	// Token: 0x04000E9C RID: 3740
	private readonly Probability.ProbabilityInt[] reaperBundleDrops = new Probability.ProbabilityInt[]
	{
		new Probability.ProbabilityInt
		{
			Value = 1,
			Probability = 0.35f
		},
		new Probability.ProbabilityInt
		{
			Value = 2,
			Probability = 0.5f
		},
		new Probability.ProbabilityInt
		{
			Value = 3,
			Probability = 0.15f
		}
	};

	// Token: 0x04000E9D RID: 3741
	private float silkPartsTimeLeft;

	// Token: 0x04000E9E RID: 3742
	private HeroController.ReaperCrestStateInfo reaperState;

	// Token: 0x04000E9F RID: 3743
	private HeroController.HunterUpgCrestStateInfo hunterUpgState;

	// Token: 0x04000EA1 RID: 3745
	private float quickeningTimeLeft;

	// Token: 0x04000EA2 RID: 3746
	private bool didCheckEdgeAdjust;

	// Token: 0x04000EA5 RID: 3749
	private NailSlash normalSlash;

	// Token: 0x04000EA6 RID: 3750
	private DamageEnemies normalSlashDamager;

	// Token: 0x04000EA7 RID: 3751
	private NailSlash alternateSlash;

	// Token: 0x04000EA8 RID: 3752
	private DamageEnemies alternateSlashDamager;

	// Token: 0x04000EA9 RID: 3753
	private NailSlash upSlash;

	// Token: 0x04000EAA RID: 3754
	private DamageEnemies upSlashDamager;

	// Token: 0x04000EAB RID: 3755
	private NailSlash altUpSlash;

	// Token: 0x04000EAC RID: 3756
	private DamageEnemies altUpSlashDamager;

	// Token: 0x04000EAD RID: 3757
	private NailSlash downSlash;

	// Token: 0x04000EAE RID: 3758
	private Downspike downSpike;

	// Token: 0x04000EAF RID: 3759
	private DamageEnemies downSlashDamager;

	// Token: 0x04000EB0 RID: 3760
	private NailSlash altDownSlash;

	// Token: 0x04000EB1 RID: 3761
	private Downspike altDownSpike;

	// Token: 0x04000EB2 RID: 3762
	private DamageEnemies altDownSlashDamager;

	// Token: 0x04000EB3 RID: 3763
	private NailSlash wallSlash;

	// Token: 0x04000EB4 RID: 3764
	private DamageEnemies wallSlashDamager;

	// Token: 0x04000EB5 RID: 3765
	private AreaEffectTint areaEffectTint;

	// Token: 0x04000EB6 RID: 3766
	private readonly HashSet<object> inputBlockers = new HashSet<object>();

	// Token: 0x04000EB7 RID: 3767
	private readonly List<HeroController.DecayingVelocity> extraAirMoveVelocities = new List<HeroController.DecayingVelocity>();

	// Token: 0x04000EB8 RID: 3768
	private HeroControllerConfig crestConfig;

	// Token: 0x04000EB9 RID: 3769
	private static HeroController _instance;

	// Token: 0x04000EBA RID: 3770
	private static int lastUpdate = -1;

	// Token: 0x04000EBD RID: 3773
	private FsmBool isUmbrellaActive;

	// Token: 0x04000EBE RID: 3774
	private bool tryShove;

	// Token: 0x04000EBF RID: 3775
	private bool onFlatGround;

	// Token: 0x04000EC0 RID: 3776
	private List<DamageEnemies> damageEnemiesList = new List<DamageEnemies>();

	// Token: 0x04000EC1 RID: 3777
	private static RaycastHit2D[] _rayHitStore = new RaycastHit2D[10];

	// Token: 0x04000EC2 RID: 3778
	private double canThrowTime;

	// Token: 0x04000EC3 RID: 3779
	private bool tryCancelDownSlash;

	// Token: 0x04000EC4 RID: 3780
	private int parriedAttack;

	// Token: 0x04000EC5 RID: 3781
	private bool announceNextFixedUpdate;

	// Token: 0x04000EC6 RID: 3782
	private double recoilAllowTime;

	// Token: 0x04000EC8 RID: 3784
	private const float PreventCastByDialogueEndDuration = 0.3f;

	// Token: 0x04000EC9 RID: 3785
	private float preventCastByDialogueEndTimer;

	// Token: 0x04000ECB RID: 3787
	private float preventSoftLandTimer;

	// Token: 0x04000ECC RID: 3788
	private float invulnerableFreezeDuration;

	// Token: 0x04000ECD RID: 3789
	private float invulnerableDuration;

	// Token: 0x04000ECE RID: 3790
	private WallTouchCache leftCache = new WallTouchCache();

	// Token: 0x04000ECF RID: 3791
	private WallTouchCache rightCache = new WallTouchCache();

	// Token: 0x04000ED0 RID: 3792
	private TouchGroundResult checkTouchGround = new TouchGroundResult();

	// Token: 0x020014E4 RID: 5348
	[Serializable]
	public class ConfigGroup
	{
		// Token: 0x17000D11 RID: 3345
		// (get) Token: 0x0600857C RID: 34172 RVA: 0x00270DA0 File Offset: 0x0026EFA0
		// (set) Token: 0x0600857D RID: 34173 RVA: 0x00270DA8 File Offset: 0x0026EFA8
		public NailSlash NormalSlash { get; private set; }

		// Token: 0x17000D12 RID: 3346
		// (get) Token: 0x0600857E RID: 34174 RVA: 0x00270DB1 File Offset: 0x0026EFB1
		// (set) Token: 0x0600857F RID: 34175 RVA: 0x00270DB9 File Offset: 0x0026EFB9
		public DamageEnemies NormalSlashDamager { get; private set; }

		// Token: 0x17000D13 RID: 3347
		// (get) Token: 0x06008580 RID: 34176 RVA: 0x00270DC2 File Offset: 0x0026EFC2
		// (set) Token: 0x06008581 RID: 34177 RVA: 0x00270DCA File Offset: 0x0026EFCA
		public NailSlash AlternateSlash { get; private set; }

		// Token: 0x17000D14 RID: 3348
		// (get) Token: 0x06008582 RID: 34178 RVA: 0x00270DD3 File Offset: 0x0026EFD3
		// (set) Token: 0x06008583 RID: 34179 RVA: 0x00270DDB File Offset: 0x0026EFDB
		public DamageEnemies AlternateSlashDamager { get; private set; }

		// Token: 0x17000D15 RID: 3349
		// (get) Token: 0x06008584 RID: 34180 RVA: 0x00270DE4 File Offset: 0x0026EFE4
		// (set) Token: 0x06008585 RID: 34181 RVA: 0x00270DEC File Offset: 0x0026EFEC
		public NailSlash UpSlash { get; private set; }

		// Token: 0x17000D16 RID: 3350
		// (get) Token: 0x06008586 RID: 34182 RVA: 0x00270DF5 File Offset: 0x0026EFF5
		// (set) Token: 0x06008587 RID: 34183 RVA: 0x00270DFD File Offset: 0x0026EFFD
		public DamageEnemies UpSlashDamager { get; private set; }

		// Token: 0x17000D17 RID: 3351
		// (get) Token: 0x06008588 RID: 34184 RVA: 0x00270E06 File Offset: 0x0026F006
		// (set) Token: 0x06008589 RID: 34185 RVA: 0x00270E0E File Offset: 0x0026F00E
		public NailSlash AltUpSlash { get; private set; }

		// Token: 0x17000D18 RID: 3352
		// (get) Token: 0x0600858A RID: 34186 RVA: 0x00270E17 File Offset: 0x0026F017
		// (set) Token: 0x0600858B RID: 34187 RVA: 0x00270E1F File Offset: 0x0026F01F
		public DamageEnemies AltUpSlashDamager { get; private set; }

		// Token: 0x17000D19 RID: 3353
		// (get) Token: 0x0600858C RID: 34188 RVA: 0x00270E28 File Offset: 0x0026F028
		// (set) Token: 0x0600858D RID: 34189 RVA: 0x00270E30 File Offset: 0x0026F030
		public NailSlash DownSlash { get; private set; }

		// Token: 0x17000D1A RID: 3354
		// (get) Token: 0x0600858E RID: 34190 RVA: 0x00270E39 File Offset: 0x0026F039
		// (set) Token: 0x0600858F RID: 34191 RVA: 0x00270E41 File Offset: 0x0026F041
		public Downspike Downspike { get; private set; }

		// Token: 0x17000D1B RID: 3355
		// (get) Token: 0x06008590 RID: 34192 RVA: 0x00270E4A File Offset: 0x0026F04A
		// (set) Token: 0x06008591 RID: 34193 RVA: 0x00270E52 File Offset: 0x0026F052
		public DamageEnemies DownSlashDamager { get; private set; }

		// Token: 0x17000D1C RID: 3356
		// (get) Token: 0x06008592 RID: 34194 RVA: 0x00270E5B File Offset: 0x0026F05B
		// (set) Token: 0x06008593 RID: 34195 RVA: 0x00270E63 File Offset: 0x0026F063
		public NailSlash AltDownSlash { get; private set; }

		// Token: 0x17000D1D RID: 3357
		// (get) Token: 0x06008594 RID: 34196 RVA: 0x00270E6C File Offset: 0x0026F06C
		// (set) Token: 0x06008595 RID: 34197 RVA: 0x00270E74 File Offset: 0x0026F074
		public Downspike AltDownspike { get; private set; }

		// Token: 0x17000D1E RID: 3358
		// (get) Token: 0x06008596 RID: 34198 RVA: 0x00270E7D File Offset: 0x0026F07D
		// (set) Token: 0x06008597 RID: 34199 RVA: 0x00270E85 File Offset: 0x0026F085
		public DamageEnemies AltDownSlashDamager { get; private set; }

		// Token: 0x17000D1F RID: 3359
		// (get) Token: 0x06008598 RID: 34200 RVA: 0x00270E8E File Offset: 0x0026F08E
		// (set) Token: 0x06008599 RID: 34201 RVA: 0x00270E96 File Offset: 0x0026F096
		public NailSlash WallSlash { get; private set; }

		// Token: 0x17000D20 RID: 3360
		// (get) Token: 0x0600859A RID: 34202 RVA: 0x00270E9F File Offset: 0x0026F09F
		// (set) Token: 0x0600859B RID: 34203 RVA: 0x00270EA7 File Offset: 0x0026F0A7
		public DamageEnemies WallSlashDamager { get; private set; }

		// Token: 0x0600859C RID: 34204 RVA: 0x00270EB0 File Offset: 0x0026F0B0
		public void Setup()
		{
			if (this.ActiveRoot)
			{
				this.ActiveRoot.SetActive(false);
			}
			if (this.NormalSlashObject)
			{
				this.NormalSlash = this.NormalSlashObject.GetComponent<NailSlash>();
				this.NormalSlashDamager = this.NormalSlashObject.GetComponent<DamageEnemies>();
			}
			if (this.AlternateSlashObject)
			{
				this.AlternateSlash = this.AlternateSlashObject.GetComponent<NailSlash>();
				this.AlternateSlashDamager = this.AlternateSlashObject.GetComponent<DamageEnemies>();
			}
			if (this.UpSlashObject)
			{
				this.UpSlash = this.UpSlashObject.GetComponent<NailSlash>();
				this.UpSlashDamager = this.UpSlashObject.GetComponent<DamageEnemies>();
			}
			if (this.AltUpSlashObject)
			{
				this.AltUpSlash = this.AltUpSlashObject.GetComponent<NailSlash>();
				this.AltUpSlashDamager = this.AltUpSlashObject.GetComponent<DamageEnemies>();
			}
			switch (this.Config.DownSlashType)
			{
			case HeroControllerConfig.DownSlashTypes.DownSpike:
				if (this.DownSlashObject)
				{
					this.Downspike = this.DownSlashObject.GetComponent<Downspike>();
				}
				if (this.AltDownSlashObject)
				{
					this.AltDownspike = this.AltDownSlashObject.GetComponent<Downspike>();
				}
				break;
			case HeroControllerConfig.DownSlashTypes.Slash:
				if (this.DownSlashObject)
				{
					this.DownSlash = this.DownSlashObject.GetComponent<NailSlash>();
				}
				if (this.AltDownSlashObject)
				{
					this.AltDownSlash = this.AltDownSlashObject.GetComponent<NailSlash>();
				}
				break;
			case HeroControllerConfig.DownSlashTypes.Custom:
				break;
			default:
				throw new NotImplementedException();
			}
			if (this.DownSlashObject)
			{
				this.DownSlashDamager = this.DownSlashObject.GetComponent<DamageEnemies>();
			}
			if (this.AltDownSlashObject)
			{
				this.AltDownSlashDamager = this.AltDownSlashObject.GetComponent<DamageEnemies>();
			}
			if (this.WallSlashObject)
			{
				this.WallSlash = this.WallSlashObject.GetComponent<NailSlash>();
				this.WallSlashDamager = this.WallSlashObject.GetComponent<DamageEnemies>();
			}
		}

		// Token: 0x0400855C RID: 34140
		public HeroControllerConfig Config;

		// Token: 0x0400855D RID: 34141
		public GameObject ActiveRoot;

		// Token: 0x0400855E RID: 34142
		[Space]
		public GameObject NormalSlashObject;

		// Token: 0x0400855F RID: 34143
		public GameObject AlternateSlashObject;

		// Token: 0x04008560 RID: 34144
		public GameObject UpSlashObject;

		// Token: 0x04008561 RID: 34145
		public GameObject AltUpSlashObject;

		// Token: 0x04008562 RID: 34146
		public GameObject DownSlashObject;

		// Token: 0x04008563 RID: 34147
		public GameObject AltDownSlashObject;

		// Token: 0x04008564 RID: 34148
		public GameObject WallSlashObject;

		// Token: 0x04008565 RID: 34149
		[Space]
		public GameObject DashStab;

		// Token: 0x04008566 RID: 34150
		public GameObject DashStabAlt;

		// Token: 0x04008567 RID: 34151
		public GameObject ChargeSlash;

		// Token: 0x04008568 RID: 34152
		public GameObject TauntSlash;
	}

	// Token: 0x020014E5 RID: 5349
	public struct DecayingVelocity
	{
		// Token: 0x04008579 RID: 34169
		public Vector2 Velocity;

		// Token: 0x0400857A RID: 34170
		public float Decay;

		// Token: 0x0400857B RID: 34171
		public bool CancelOnTurn;

		// Token: 0x0400857C RID: 34172
		public HeroController.DecayingVelocity.SkipBehaviours SkipBehaviour;

		// Token: 0x02001C4D RID: 7245
		public enum SkipBehaviours
		{
			// Token: 0x0400A13B RID: 41275
			None,
			// Token: 0x0400A13C RID: 41276
			WhileMoving,
			// Token: 0x0400A13D RID: 41277
			WhileMovingForward,
			// Token: 0x0400A13E RID: 41278
			WhileMovingBackward
		}
	}

	// Token: 0x020014E6 RID: 5350
	private struct SilkEatTracker
	{
		// Token: 0x0400857D RID: 34173
		public float EatDelayLeft;

		// Token: 0x0400857E RID: 34174
		public float EatDurationLeft;

		// Token: 0x0400857F RID: 34175
		public int EatSilkCount;
	}

	// Token: 0x020014E7 RID: 5351
	public struct DeliveryTimer
	{
		// Token: 0x04008580 RID: 34176
		public DeliveryQuestItem.ActiveItem Item;

		// Token: 0x04008581 RID: 34177
		public float TimeLeft;
	}

	// Token: 0x020014E8 RID: 5352
	// (Invoke) Token: 0x0600859F RID: 34207
	public delegate void HeroSetDelegate(HeroController heroController);

	// Token: 0x020014E9 RID: 5353
	// (Invoke) Token: 0x060085A3 RID: 34211
	public delegate void HeroInPosition(bool forceDirect);

	// Token: 0x020014EA RID: 5354
	// (Invoke) Token: 0x060085A7 RID: 34215
	public delegate void DamageTakenDelegate(HeroController.DamageInfo damageInfo);

	// Token: 0x020014EB RID: 5355
	public struct DamageInfo
	{
		// Token: 0x060085AA RID: 34218 RVA: 0x002710A7 File Offset: 0x0026F2A7
		public DamageInfo(HazardType hazardType)
		{
			this.hazardType = hazardType;
		}

		// Token: 0x04008582 RID: 34178
		public HazardType hazardType;
	}

	// Token: 0x020014EC RID: 5356
	public struct WarriorCrestStateInfo
	{
		// Token: 0x04008583 RID: 34179
		public bool IsInRageMode;

		// Token: 0x04008584 RID: 34180
		public float RageEffectTimeLeft;

		// Token: 0x04008585 RID: 34181
		public float RageHealTimeLeft;

		// Token: 0x04008586 RID: 34182
		public int RageModeHealCount;

		// Token: 0x04008587 RID: 34183
		public int LastHealAttack;
	}

	// Token: 0x020014ED RID: 5357
	public struct ReaperCrestStateInfo
	{
		// Token: 0x04008588 RID: 34184
		public float ReaperModeDurationLeft;

		// Token: 0x04008589 RID: 34185
		public bool IsInReaperMode;
	}

	// Token: 0x020014EE RID: 5358
	public struct HunterUpgCrestStateInfo
	{
		// Token: 0x17000D21 RID: 3361
		// (get) Token: 0x060085AB RID: 34219 RVA: 0x002710B0 File Offset: 0x0026F2B0
		public bool IsComboMeterAboveExtra
		{
			get
			{
				return this.CurrentMeterHits >= Gameplay.HunterCombo2Hits + Gameplay.HunterCombo2ExtraHits;
			}
		}

		// Token: 0x0400858A RID: 34186
		public int CurrentMeterHits;
	}

	// Token: 0x020014EF RID: 5359
	public struct WandererCrestStateInfo
	{
		// Token: 0x0400858B RID: 34187
		public bool QueuedNextHitCritical;

		// Token: 0x0400858C RID: 34188
		public bool CriticalHitsLocked;
	}
}
