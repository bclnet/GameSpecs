using System;
using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    public enum VehicleType : int
    {
        VEH_WHEELS_4 = 0x0,
        VEH_TANK = 0x1,
        VEH_PLANE = 0x2,
        VEH_BOAT = 0x3,
        VEH_ARTILLERY = 0x4,
        VEH_HELICOPTER = 0x5,
        VEH_SNOWMOBILE = 0x6,
        VEH_TYPE_COUNT = 0x7,
    }

    public enum VehicleAxleType
    {
        VEH_AXLE_FRONT = 0x0,
        VEH_AXLE_REAR = 0x1,
        VEH_AXLE_ALL = 0x2,
        VEH_AXLE_COUNT = 0x3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct VehiclePhysDef
    {
        public int physicsEnabled;
        public char* physPresetName;
        public PhysPreset* physPreset;
        public char* accelGraphName;
        public VehicleAxleType steeringAxle;
        public VehicleAxleType powerAxle;
        public VehicleAxleType brakingAxle;
        public float topSpeed;
        public float reverseSpeed;
        public float maxVelocity;
        public float maxPitch;
        public float maxRoll;
        public float suspensionTravelFront;
        public float suspensionTravelRear;
        public float suspensionStrengthFront;
        public float suspensionDampingFront;
        public float suspensionStrengthRear;
        public float suspensionDampingRear;
        public float frictionBraking;
        public float frictionCoasting;
        public float frictionTopSpeed;
        public float frictionSide;
        public float frictionSideRear;
        public float velocityDependentSlip;
        public float rollStability;
        public float rollResistance;
        public float pitchResistance;
        public float yawResistance;
        public float uprightStrengthPitch;
        public float uprightStrengthRoll;
        public float targetAirPitch;
        public float airYawTorque;
        public float airPitchTorque;
        public float minimumMomentumForCollision;
        public float collisionLaunchForceScale;
        public float wreckedMassScale;
        public float wreckedBodyFriction;
        public float minimumJoltForNotify;
        public float slipThreshholdFront;
        public float slipThreshholdRear;
        public float slipFricScaleFront;
        public float slipFricScaleRear;
        public float slipFricRateFront;
        public float slipFricRateRear;
        public float slipYawTorque;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct VehicleDef
    {
        public char* name;
        public VehicleType type;
        public char* useHintString;
        public int health;
        public int quadBarrel;
        public float texScrollScale;
        public float maxSpeed;
        public float accel;
        public float rotRate;
        public float rotAccel;
        public float maxBodyPitch;
        public float maxBodyRoll;
        public float fakeBodyAccelPitch;
        public float fakeBodyAccelRoll;
        public float fakeBodyVelPitch;
        public float fakeBodyVelRoll;
        public float fakeBodySideVelPitch;
        public float fakeBodySidePitchStrength;
        public float fakeBodyRollStrength;
        public float fakeBodyPitchDampening;
        public float fakeBodyRollDampening;
        public float fakeBodyBoatRockingAmplitude;
        public float fakeBodyBoatRockingPeriod;
        public float fakeBodyBoatRockingRotationPeriod;
        public float fakeBodyBoatRockingFadeoutSpeed;
        public float boatBouncingMinForce;
        public float boatBouncingMaxForce;
        public float boatBouncingRate;
        public float boatBouncingFadeinSpeed;
        public float boatBouncingFadeoutSteeringAngle;
        public float collisionDamage;
        public float collisionSpeed;
        public float killcamZDist;
        public float killcamBackDist;
        public float killcamUpDist;
        public int playerProtected;
        public int bulletDamage;
        public int armorPiercingDamage;
        public int grenadeDamage;
        public int projectileDamage;
        public int projectileSplashDamage;
        public int heavyExplosiveDamage;
        public VehiclePhysDef vehiclePhysics;
        public float boostDuration;
        public float boostRechargeTime;
        public float boostAcceleration;
        public float supensionTravel;
        public float maxSteeringAngle;
        public float steeringLerp;
        public float minSteeringScale;
        public float minSteeringSpeed;
        public int camLookEnabled;
        public float camLerp;
        public float camPitchInfluence;
        public float camRollInfluence;
        public float camFovIncrease;
        public float camFovOffset;
        public float camFovSpeed;
        public char* turretWeaponName;
        public WeaponVariantDef* turretWeapon;
        public float turretHorizSpanLeft;
        public float turretHorizSpanRight;
        public float turretVertSpanUp;
        public float turretVertSpanDown;
        public float turretRotRate;
        public snd_alias_list_name turretSpinSnd;
        public snd_alias_list_name turretStopSnd;
        public int trophyEnabled;
        public float trophyRadius;
        public float trophyInactiveRadius;
        public int trophyAmmoCount;
        public float trophyReloadTime;
        public fixed short trophyTags[4];
        public Material* compassFriendlyIcon;
        public Material* compassEnemyIcon;
        public int compassIconWidth;
        public int compassIconHeight;
        public snd_alias_list_name lowIdleSound;
        public snd_alias_list_name highIdleSound;
        public snd_alias_list_name lowEngineSound;
        public snd_alias_list_name highEngineSound;
        public float engineSndSpeed;
        public snd_alias_list_name engineStartUpSnd;
        public int engineStartUpLength;
        public snd_alias_list_name engineShutdownSnd;
        public snd_alias_list_name engineIdleSnd;
        public snd_alias_list_name engineSustainSnd;
        public snd_alias_list_name engineRampUpSnd;
        public int engineRampUpLength;
        public snd_alias_list_name engineRampDownSnd;
        public int engineRampDownLength;
        public snd_alias_list_name suspensionSoftSnd;
        public float suspensionSoftCompression;
        public snd_alias_list_name suspensionHardSnd;
        public float suspensionHardConpression;
        public snd_alias_list_name collisionSnd;
        public float collisionBlendSpeed;
        public snd_alias_list_name speedSnd;
        public float speedSndBlendSpeed;
        public char* surfaceSndPrefix;
        public snd_alias_list_name* surfaceSounds(int idx) => throw new NotImplementedException();
        public snd_alias_list_name surfaceSounds_00; public snd_alias_list_name surfaceSounds_01; public snd_alias_list_name surfaceSounds_02; public snd_alias_list_name surfaceSounds_03; public snd_alias_list_name surfaceSounds_04;
        public snd_alias_list_name surfaceSounds_05; public snd_alias_list_name surfaceSounds_06; public snd_alias_list_name surfaceSounds_07; public snd_alias_list_name surfaceSounds_08; public snd_alias_list_name surfaceSounds_09;
        public snd_alias_list_name surfaceSounds_10; public snd_alias_list_name surfaceSounds_11; public snd_alias_list_name surfaceSounds_12; public snd_alias_list_name surfaceSounds_13; public snd_alias_list_name surfaceSounds_14;
        public snd_alias_list_name surfaceSounds_15; public snd_alias_list_name surfaceSounds_16; public snd_alias_list_name surfaceSounds_17; public snd_alias_list_name surfaceSounds_18; public snd_alias_list_name surfaceSounds_19;
        public snd_alias_list_name surfaceSounds_20; public snd_alias_list_name surfaceSounds_21; public snd_alias_list_name surfaceSounds_22; public snd_alias_list_name surfaceSounds_23; public snd_alias_list_name surfaceSounds_24;
        public snd_alias_list_name surfaceSounds_25; public snd_alias_list_name surfaceSounds_26; public snd_alias_list_name surfaceSounds_27; public snd_alias_list_name surfaceSounds_28; public snd_alias_list_name surfaceSounds_29;
        public snd_alias_list_name surfaceSounds_30; //: 0x1F
        public float surfaceSndBlendSpeed;
        public float slideVolume;
        public float slideBlendSpeed;
        public float inAirPitch;
    }
}