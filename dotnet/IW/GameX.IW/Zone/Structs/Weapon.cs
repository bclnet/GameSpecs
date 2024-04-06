using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    public enum weapFireType_t : int
    {
        WEAPON_FIRETYPE_FULLAUTO = 0x0,
        WEAPON_FIRETYPE_SINGLESHOT = 0x1,
        WEAPON_FIRETYPE_BURSTFIRE2 = 0x2,
        WEAPON_FIRETYPE_BURSTFIRE3 = 0x3,
        WEAPON_FIRETYPE_BURSTFIRE4 = 0x4,
        WEAPON_FIRETYPE_DOUBLE_BARREL = 0x5,
        WEAPON_FIRETYPE_MAX
    }

    public enum weapInventoryType_t : int
    {
        WEAPINVENTORY_PRIMARY = 0,
        WEAPINVENTORY_OFFHAND = 1,
        WEAPINVENTORY_ITEM = 2,
        WEAPINVENTORY_ALTMODE = 3,
        WEAPINVENTORY_EXCLUSIVE = 4,
        WEAPINVENTORY_SCAVENGER = 5,
        WEAPINVENTORY_MAX
    }

    public enum PenetrateType
    {
        PENETRATE_TYPE_NONE = 0x0,
        PENETRATE_TYPE_SMALL = 0x1,
        PENETRATE_TYPE_MEDIUM = 0x2,
        PENETRATE_TYPE_LARGE = 0x3,
        PENETRATE_TYPE_COUNT = 0x4
    }

    public enum activeReticleType_t : int
    {
        VEH_ACTIVE_RETICLE_NONE = 0,
        VEH_ACTIVE_RETICLE_PIP_ON_A_STICK = 1,
        VEH_ACTIVE_RETICLE_BOUNCING_DIAMOND = 2,
        VEH_ACTIVE_RETICLE_MAX
    }

    public enum weapType_t : int
    {
        WEAPTYPE_BULLET = 0,
        WEAPTYPE_GRENADE = 1,
        WEAPTYPE_PROJECTILE = 2,
        WEAPTYPE_RIOTSHIELD = 3,
        WEAPTYPE_MAX
    }

    public enum weapClass_t : int
    {
        WEAPCLASS_RIFLE = 0,
        WEAPCLASS_SNIPER = 1,
        WEAPCLASS_MG = 2,
        WEAPCLASS_SMG = 3,
        WEAPCLASS_SPREAD = 4,
        WEAPCLASS_PISTOL = 5,
        WEAPCLASS_GRENADE = 6,
        WEAPCLASS_ROCKETLAUNCHER = 7,
        WEAPCLASS_TURRET = 8,
        WEAPCLASS_THROWINGKNIFE = 9,
        WEAPCLASS_NON_PLAYER = 10,
        WEAPCLASS_ITEM = 11,
        WEAPCLASS_MAX
    }

    public enum OffhandClass : int
    {
        OFFHAND_CLASS_NONE = 0,
        OFFHAND_CLASS_FRAG_GRENADE = 1,
        OFFHAND_CLASS_SMOKE_GRENADE = 2,
        OFFHAND_CLASS_FLASH_GRENADE = 3,
        OFFHAND_CLASS_MAX
    }

    public enum playerAnimType_t : int
    {
        PLAER_ANIM_TYPE_NONE = 0x0,
        PLAER_ANIM_TYPE_OTHER = 0x1,
        PLAER_ANIM_TYPE_PISTOL = 0x2,
        PLAER_ANIM_TYPE_SMG = 0x3,
        PLAER_ANIM_TYPE_AUTORIFLE = 0x4,
        PLAER_ANIM_TYPE_MG = 0x5,
        PLAER_ANIM_TYPE_SNIPER = 0x6,
        PLAER_ANIM_TYPE_ROCKETLAUNCHER = 0x7,
        PLAER_ANIM_TYPE_EXPLOSIVE = 0x8,
        PLAER_ANIM_TYPE_GRENADE = 0x9,
        PLAER_ANIM_TYPE_TURRET = 0xA,
        PLAER_ANIM_TYPE_C4 = 0xB,
        PLAER_ANIM_TYPE_M203 = 0xC,
        PLAER_ANIM_TYPE_HOLD = 0xD,
        PLAER_ANIM_TYPE_BRIEFCASE = 0xE,
        PLAER_ANIM_TYPE_RIOTSHIELD = 0xF,
        PLAER_ANIM_TYPE_LAPTOP = 0x10,
        PLAER_ANIM_TYPE_THROWINGKNIFE = 0x11
    }

    public enum weapProjExplosion_t
    {
        WEAPPROJEXP_GRENADE = 0x0,
        WEAPPROJEXP_ROCKET = 0x1,
        WEAPPROJEXP_FLASHBANG = 0x2,
        WEAPPROJEXP_NONE = 0x3,
        WEAPPROJEXP_DUD = 0x4,
        WEAPPROJEXP_SMOKE = 0x5,
        WEAPPROJEXP_HEAVY = 0x6,
        WEAPPROJEXP_NUM = 0x7
    }

    public enum WeapStickinessType
    {
        WEAPSTICKINESS_NONE = 0x0,
        WEAPSTICKINESS_ALL = 0x1,
        WEAPSTICKINESS_ALL_ORIENT = 0x2,
        WEAPSTICKINESS_GROUND = 0x3,
        WEAPSTICKINESS_GROUND_WITH_YAW = 0x4,
        WEAPSTICKINESS_KNIFE = 0x5,
        WEAPSTICKINESS_COUNT = 0x6
    }

    public enum weaponIconRatioType_t
    {
        WEAPON_ICON_RATIO_1TO1 = 0x0,
        WEAPON_ICON_RATIO_2TO1 = 0x1,
        WEAPON_ICON_RATIO_4TO1 = 0x2,
        WEAPON_ICON_RATIO_COUNT = 0x3
    }

    public enum ammoCounterClipType_t
    {
        AMMO_COUNTER_CLIP_NONE = 0x0,
        AMMO_COUNTER_CLIP_MAGAZINE = 0x1,
        AMMO_COUNTER_CLIP_SHORTMAGAZINE = 0x2,
        AMMO_COUNTER_CLIP_SHOTGUN = 0x3,
        AMMO_COUNTER_CLIP_ROCKET = 0x4,
        AMMO_COUNTER_CLIP_BELTFED = 0x5,
        AMMO_COUNTER_CLIP_ALTWEAPON = 0x6,
        AMMO_COUNTER_CLIP_COUNT = 0x7
    }

    public enum weapOverlayReticle_t
    {
        WEAPOVERLAYRETICLE_NONE = 0x0,
        WEAPOVERLAYRETICLE_CROSSHAIR = 0x1,
        WEAPOVERLAYRETICLE_NUM = 0x2
    }

    public enum weapOverlayInterface_t
    {
        WEAPOVERLAYINTERFACE_NONE = 0x0,
        WEAPOVERLAYINTERFACE_JAVELIN = 0x1,
        WEAPOVERLAYINTERFACE_TURRETSCOPE = 0x2,
        WEAPOVERLAYINTERFACECOUNT = 0x3
    }

    public enum weapStance_t
    {
        WEAPSTANCE_STAND = 0x0,
        WEAPSTANCE_DUCK = 0x1,
        WEAPSTANCE_PRONE = 0x2,
        WEAPSTANCE_NUM = 0x3
    }

    public enum ImpactType
    {
        IMPACT_TYPE_NONE = 0,
        IMPACT_TYPE_BULLET_SMALL = 1,
        IMPACT_TYPE_BULLET_LARGE = 2,
        IMPACT_TYPE_BULLET_AP = 3,
        IMPACT_TYPE_SHOTGUN_FMJ = 4,
        IMPACT_TYPE_SHOTGUN = 5,
        IMPACT_TYPE_GRENADE_BOUNCE = 7,
        IMPACT_TYPE_GRENADE_EXPLODE = 8,
        IMPACT_TYPE_ROCKET_EXPLODE = 9,
        IMPACT_TYPE_PROJECTILE_DUD = 10,
        IMPACT_TYPE_MAX
    }

    public enum guidedMissileType_t
    {
        MISSILE_GUIDANCE_NONE = 0x0,
        MISSILE_GUIDANCE_SIDEWINDER = 0x1,
        MISSILE_GUIDANCE_HELLFIRE = 0x2,
        MISSILE_GUIDANCE_JAVELIN = 0x3,
        MISSILE_GUIDANCE_MAX
    }

    public enum surfaceNames_t
    {
        bark,
        brick,
        carpet,
        cloth,
        concrete,
        dirt,
        flesh,
        foliage,
        glass,
        grass,
        gravel,
        ice,
        metal,
        mud,
        paper,
        plaster,
        rock,
        sand,
        snow,
        water,
        wood,
        asphalt,
        ceramic,
        plastic,
        rubber,
        cushion,
        fruit,
        paintedmetal,
        riotshield,
        slush,
        opaqueglass
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct snd_alias_list_name //: union
    {
        [FieldOffset(0)] public char* name;
        [FieldOffset(0)] public SoundAliasList* asset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct WeaponDef
    {
        public char* szInternalName;
        public XModel** gunXModel;           //Count = 16
        public XModel* handXModel;
        public char** szXAnimsR;        //Count = 37
        public char** szXAnimsL;        //Count = 37
        public char* szModeName;
        public short* noteTrackSoundMap0; public short* noteTrackSoundMap1;// Count = 16
        public short* noteTrackRumbleMap0; public short* noteTrackRumbleMap1;// Count = 16
        public playerAnimType_t playerAnimType;
        public weapType_t weaponType;
        public weapClass_t weaponClass;
        public PenetrateType penetrateType;
        public weapInventoryType_t inventoryType;
        public weapFireType_t fireType;
        public OffhandClass offhandClass;
        public weapStance_t stance;
        public FxEffectDef* viewFlashEffect;
        public FxEffectDef* worldFlashEffect;
        public snd_alias_list_name sounds_00; public snd_alias_list_name sounds_01; public snd_alias_list_name sounds_02; public snd_alias_list_name sounds_03; public snd_alias_list_name sounds_04;
        public snd_alias_list_name sounds_05; public snd_alias_list_name sounds_06; public snd_alias_list_name sounds_07; public snd_alias_list_name sounds_08; public snd_alias_list_name sounds_09;
        public snd_alias_list_name sounds_10; public snd_alias_list_name sounds_11; public snd_alias_list_name sounds_12; public snd_alias_list_name sounds_13; public snd_alias_list_name sounds_14;
        public snd_alias_list_name sounds_15; public snd_alias_list_name sounds_16; public snd_alias_list_name sounds_17; public snd_alias_list_name sounds_18; public snd_alias_list_name sounds_19;
        public snd_alias_list_name sounds_20; public snd_alias_list_name sounds_21; public snd_alias_list_name sounds_22; public snd_alias_list_name sounds_23; public snd_alias_list_name sounds_24;
        public snd_alias_list_name sounds_25; public snd_alias_list_name sounds_26; public snd_alias_list_name sounds_27; public snd_alias_list_name sounds_28; public snd_alias_list_name sounds_29;
        public snd_alias_list_name sounds_30; public snd_alias_list_name sounds_31; public snd_alias_list_name sounds_32; public snd_alias_list_name sounds_33; public snd_alias_list_name sounds_34;
        public snd_alias_list_name sounds_35; public snd_alias_list_name sounds_36; public snd_alias_list_name sounds_37; public snd_alias_list_name sounds_38; public snd_alias_list_name sounds_39;
        public snd_alias_list_name sounds_40; public snd_alias_list_name sounds_41; public snd_alias_list_name sounds_42; public snd_alias_list_name sounds_43; public snd_alias_list_name sounds_44;
        public snd_alias_list_name sounds_45; public snd_alias_list_name sounds_46; //47
        public snd_alias_list_name* bounceSound;
        public FxEffectDef* viewShellEjectEffect;
        public FxEffectDef* worldShellEjectEffect;
        public FxEffectDef* viewLastShotEjectEffect;
        public FxEffectDef* worldLastShotEjectEffect;
        public Material* reticleCenter;
        public Material* reticleSide;
        public int iReticleCenterSize;
        public int iReticleSideSize;
        public int iReticleMinOfs;
        public activeReticleType_t activeReticleType;
        public fixed float vStandMove[3];
        public fixed float vStandRot[3];
        public fixed float vStrafeMove[3];
        public fixed float vStrafeRot[3];
        public fixed float vDuckedOfs[3];
        public fixed float vDuckedMove[3];
        public fixed float vDuckedRot[3];
        public fixed float vProneOfs[3];
        public fixed float vProneMove[3];
        public fixed float vProneRot[3];
        public float fPosMoveRate;
        public float fPosProneMoveRate;
        public float fStandMoveMinSpeed;
        public float fDuckedMoveMinSpeed;
        public float fProneMoveMinSpeed;
        public float fPosRotRate;
        public float fPosProneRotRate;
        public float fStandRotMinSpeed;
        public float fDuckedRotMinSpeed;
        public float fProneRotMinSpeed;
        public XModel** worldModel;
        public XModel* worldClipModel;
        public XModel* rocketModel;
        public XModel* knifeModel;
        public XModel* worldKnifeModel;
        public Material* hudIcon;
        public weaponIconRatioType_t hudIconRatio;
        public Material* pickupIcon;
        public weaponIconRatioType_t pickupIconRatio;
        public Material* ammoCounterIcon;
        public weaponIconRatioType_t ammoCounterIconRatio;
        public ammoCounterClipType_t ammoCounterClip;
        public int iStartAmmo;
        public char* szAmmoName;
        public int iAmmoIndex;
        public char* szClipName;
        public int iClipIndex;
        public int iMaxAmmo;
        public int shotCount;
        public char* szSharedAmmoCapName;
        public int iSharedAmmoCapIndex;
        public int iSharedAmmoCap;
        public int damage;
        public int playerDamage;
        public int iMeleeDamage;
        public int iDamageType;
        public int iFireDelay;
        public int iMeleeDelay;
        public int meleeChargeDelay;
        public int iDetonateDelay;
        public int iRechamberTime;
        public int iRechamberOneHanded;
        public int iRechamberBoltTime;
        public int iHoldFireTime;
        public int iDetonateTime;
        public int iMeleeTime;
        public int meleeChargeTime;
        public int iReloadTime;
        public int reloadShowRocketTime;
        public int iReloadEmptyTime;
        public int iReloadAddTime;
        public int iReloadStartTime;
        public int iReloadStartAddTime;
        public int iReloadEndTime;
        public int iDropTime;
        public int iRaiseTime;
        public int iAltDropTime;
        public int quickDropTime;
        public int quickRaiseTime;
        public int iFirstRaiseTime;
        public int iEmptyRaiseTime;
        public int iEmptyDropTime;
        public int sprintInTime;
        public int sprintLoopTime;
        public int sprintOutTime;
        public int stunnedTimeBegin;
        public int stunnedTimeLoop;
        public int stunnedTimeEnd;
        public int nightVisionWearTime;
        public int nightVisionWearTimeFadeOutEnd;
        public int nightVisionWearTimePowerUp;
        public int nightVisionRemoveTime;
        public int nightVisionRemoveTimePowerDown;
        public int nightVisionRemoveTimeFadeInStart;
        public int fuseTime;
        public int aifuseTime;
        public float autoAimRange;
        public float aimAssistRange;
        public float aimAssistRangeAds;
        public float aimPadding;
        public float enemyCrosshairRange;
        public float moveSpeedScale;
        public float adsMoveSpeedScale;
        public float sprintDurationScale;
        public float adsZoomInFrac;
        public float adsZoomOutFrac;
        public Material* AdsOverlayShader;
        public Material* AdsOverlayShaderLowRes;
        public Material* AdsOverlayShaderEMP;
        public Material* AdsOverlayShaderEMPLowRes;
        public weapOverlayReticle_t adsOverlayReticle;
        public weapOverlayInterface_t adsOverlayInterface;
        public float adsOverlayWidth;
        public float adsOverlayHeight;
        public float adsOverlayWidthSplitscreen;
        public float adsOverlayHeightSplitscreen;
        public float fAdsBobFactor;
        public float fAdsViewBobMult;
        public float fHipSpreadStandMin;
        public float fHipSpreadDuckedMin;
        public float fHipSpreadProneMin;
        public float hipSpreadStandMax;
        public float hipSpreadDuckedMax;
        public float hipSpreadProneMax;
        public float fHipSpreadDecayRate;
        public float fHipSpreadFireAdd;
        public float fHipSpreadTurnAdd;
        public float fHipSpreadMoveAdd;
        public float fHipSpreadDuckedDecay;
        public float fHipSpreadProneDecay;
        public float fHipReticleSidePos;
        public float fAdsIdleAmount;
        public float fHipIdleAmount;
        public float adsIdleSpeed;
        public float hipIdleSpeed;
        public float fIdleCrouchFactor;
        public float fIdleProneFactor;
        public float fGunMaxPitch;
        public float fGunMaxYaw;
        public float swayMaxAngle;
        public float swayLerpSpeed;
        public float swayPitchScale;
        public float swayYawScale;
        public float swayHorizScale;
        public float swayVertScale;
        public float swayShellShockScale;
        public float adsSwayMaxAngle;
        public float adsSwayLerpSpeed;
        public float adsSwayPitchScale;
        public float adsSwayYawScale;
        public float adsSwayHorizScale;
        public float adsSwayVertScale;
        public float adsViewErrorMin;
        public float adsViewErrorMax;
        public PhysGeomList* collisions;
        public float dualWieldViewModelOffset;
        public weaponIconRatioType_t killIconRatio;
        public int iReloadAmmoAdd;
        public int iReloadStartAdd;
        public int iDropAmmoMin;
        public int ammoDropClipPercentMin;
        public int ammoDropClipPercentMax;
        public int iExplosionRadius;
        public int iExplosionRadiusMin;
        public int iExplosionInnerDamage;
        public int iExplosionOuterDamage;
        public float damageConeAngle;
        public float bulletExplDmgMult;
        public float bulletExplRadiusMult;
        public int iProjectileSpeed;
        public int iProjectileSpeedUp;
        public int iProjectileSpeedForward;
        public int iProjectileActivateDist;
        public float projLifetime;
        public float timeToAccelerate;
        public float projectileCurvature;
        public XModel* projectileModel;
        public weapProjExplosion_t projExplosiveType;
        public FxEffectDef* projExplosionEffect;
        public FxEffectDef* projDudEffect;
        public snd_alias_list_name projExplosionSound;
        public snd_alias_list_name projDudSound;
        public WeapStickinessType stickiness;
        public float lowAmmoWarningThreshold;
        public float ricochetChance;
        public float* parallelBounce;            //Refer to surfaceNames_t
        public float* perpendicularBounce;         //Refer to surfaceNames_t
        public FxEffectDef* projTrailEffect;
        public FxEffectDef* projBeaconEffect;
        public fixed float vProjectileColor[3];
        public guidedMissileType_t guidedMissileType;
        public float maxSteeringAccel;
        public float projIgnitionDelay;
        public FxEffectDef* projIgnitionEffect;
        public snd_alias_list_name projIgnitionSound;
        public float fAdsAimPitch;
        public float fAdsCrosshairInFrac;
        public float fAdsCrosshairOutFrac;
        public int adsGunKickReducedKickBullets;
        public float adsGunKickReducedKickPercent;
        public float fAdsGunKickPitchMin;
        public float fAdsGunKickPitchMax;
        public float fAdsGunKickYawMin;
        public float fAdsGunKickYawMax;
        public float fAdsGunKickAccel;
        public float fAdsGunKickSpeedMax;
        public float fAdsGunKickSpeedDecay;
        public float fAdsGunKickStaticDecay;
        public float fAdsViewKickPitchMin;
        public float fAdsViewKickPitchMax;
        public float fAdsViewKickYawMin;
        public float fAdsViewKickYawMax;
        public float fAdsViewScatterMin;
        public float fAdsViewScatterMax;
        public float fAdsSpread;
        public int hipGunKickReducedKickBullets;
        public float hipGunKickReducedKickPercent;
        public float fHipGunKickPitchMin;
        public float fHipGunKickPitchMax;
        public float fHipGunKickYawMin;
        public float fHipGunKickYawMax;
        public float fHipGunKickAccel;
        public float fHipGunKickSpeedMax;
        public float fHipGunKickSpeedDecay;
        public float fHipGunKickStaticDecay;
        public float fHipViewKickPitchMin;
        public float fHipViewKickPitchMax;
        public float fHipViewKickYawMin;
        public float fHipViewKickYawMax;
        public float fHipViewScatterMin;
        public float fHipViewScatterMax;
        public float fightDist;
        public float maxDist;
        public char* accuracyGraphName0; public char* accuracyGraphName1;
        public vec2* accuracyGraphKnots;
        public vec2* originalAccuracyGraphKnots;
        public short accuracyGraphKnotCount;
        public short originalAccuracyGraphKnotCount;
        public int iPositionReloadTransTime;
        public float leftArc;
        public float rightArc;
        public float topArc;
        public float bottomArc;
        public float accuracy;
        public float aiSpread;
        public float playerSpread;
        public float minVertTurnSpeed;
        public float minHorTurnSpeed;
        public float maxVertTurnSpeed;
        public float maxHorTurnSpeed;
        public float pitchConvergenceTime;
        public float yawConvergenceTime;
        public float suppressTime;
        public float maxRange;
        public float fAnimHorRotateInc;
        public float fPlayerPositionDist;
        public char* szUseHintString;
        public char* dropHintString;
        public int iUseHintStringIndex;
        public int dropHintStringIndex;
        public float horizViewJitter;
        public float vertViewJitter;
        public float scanSpeed;
        public float scanAccel;
        public int scanPauseTime;
        public char* szScript;
        public fixed float fOOPosAnimLength[2];
        public int minDamage;
        public int minPlayerDamage;
        public float maxDamageRange;
        public float minDamageRange;
        public float destabilizationRateTime;
        public float destabilizationCurvatureMax;
        public int destabilizeDistance;
        public float* locationDamageMultipliers;   //1Refer to bodyParts_t
        public char* fireRumble;
        public char* meleeImpactRumble;
        public Tracer* tracer;
        public int turretScopeZoomRate;
        public int turretScopeZoomMin;
        public int turretScopeZoomMax;
        public int turretOverheatUpRate;
        public int turretOverheatDownRate;
        public int turretOverheatPenalty;
        public snd_alias_list_name turretOverheatSound;
        public FxEffectDef* turretOverheatEffect;
        public char* turretBarrelSpinRumble;
        public int turretBarrelSpinUpTime;
        public int turretBarrelSpinDownTime;
        public int turretBarrelSpinSpeed;
        public snd_alias_list_name turretBarrelSpinMaxSnd;
        public snd_alias_list_name turretBarrelSpinUpSnds0; public snd_alias_list_name turretBarrelSpinUpSnds1; public snd_alias_list_name turretBarrelSpinUpSnds2; public snd_alias_list_name turretBarrelSpinUpSnds3;
        public snd_alias_list_name turretBarrelSpinDownSnds0; public snd_alias_list_name turretBarrelSpinDownSnds1; public snd_alias_list_name turretBarrelSpinDownSnds2; public snd_alias_list_name turretBarrelSpinDownSnds3;
        public snd_alias_list_name missileConeSoundAlias;
        public snd_alias_list_name missileConeSoundAliasAtBase;
        public float missileConeSoundRadiusAtTop;
        public float missileConeSoundRadiusAtBase;
        public float missileConeSoundHeight;
        public float missileConeSoundOriginOffset;
        public float missileConeSoundVolumescaleAtCore;
        public float missileConeSoundVolumescaleAtEdge;
        public float missileConeSoundVolumescaleCoreSize;
        public float missileConeSoundPitchAtTop;
        public float missileConeSoundPitchAtBottom;
        public float missileConeSoundPitchTopSize;
        public float missileConeSoundPitchBottomSize;
        public float missileConeSoundCrossfadeTopSize;
        public float missileConeSoundCrossfadeBottomSize;
        public bool shareAmmo;
        public bool lockonSupported;
        public bool requireLockonToFire;
        public bool bigExplosion;
        public bool noAdsWhenMagEmpty;
        public bool avoidDropCleanup;
        public bool inheritsPerks;
        public bool crosshairColorChange;
        public bool rifleBullet;
        public bool armorPiercing;
        public bool boltAction;
        public bool aimDownSight;
        public bool rechamberWhileAds;
        public bool bBulletExplosiveDamage;
        public bool cookOffHold;
        public bool clipOnly;
        public bool noAmmoPickup;
        public bool adsFire;
        public bool cancelAutoHolsterWhenEmpty;
        public bool disableSwitchToWhenEmpty;
        public bool suppressAmmoReserveDisplay;
        public bool laserSightDuringNightvision;
        public bool markableViewmodel;
        public bool noDualWield;
        public bool flipKillIcon;
        public bool noPartialReload;
        public bool segmentedReload;
        public bool blocksProne;
        public bool silenced;
        public bool isRollingGrenade;
        public bool projExplosionEffectForceNormalUp;
        public bool projImpactExplode;
        public bool stickToPlayers;
        public bool hasDetonator;
        public bool disableFiring;
        public bool timedDetonation;
        public bool rotate;
        public bool holdButtonToThrow;
        public bool freezeMovementWhenFiring;
        public bool thermalScope;
        public bool altModeSameWeapon;
        public bool turretBarrelSpinEnabled;
        public bool missileConeSoundEnabled;
        public bool missileConeSoundPitchshiftEnabled;
        public bool missileConeSoundCrossfadeEnabled;
        public bool offhandHoldIsCancelable;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WeaponVariantDef
    {
        public char* name;
        public WeaponDef* WeaponDef;
        public char* displayName;
        public short* hideTags; // Count = 32
        public char** szXAnims;        //Count = 37
        public float fAdsZoomFov;
        public int iAdsTransInTime;
        public int iAdsTransOutTime;
        public int iClipSize;
        public ImpactType impactType;
        public int iFireTime;
        public weaponIconRatioType_t dpadIconRatio;
        public float fPenetrateMultiplier;
        public float fAdsViewKickCenterSpeed;
        public float fHipViewKickCenterSpeed;
        public char* altWeaponName;
        public uint altWeaponIndex;
        public int iAltRaiseTime;
        public Material* killIcon;
        public Material* dpadIcon;
        public int unknown8;
        public int iFirstRaiseTime;
        public int iDropAmmoMax;
        public float adsDofStart;
        public float adsDofEnd;
        public short accuracyGraphKnotCount;
        public short originalAccuracyGraphKnotCount;
        public vec2* accuracyGraphKnots;
        public vec2* originalAccuracyGraphKnots;
        public bool motionTracker;
        public bool enhanced;
        public bool dpadIconShowsAmmo;
    }
}