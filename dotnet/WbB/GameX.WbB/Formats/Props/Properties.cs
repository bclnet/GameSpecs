using System;
using System.ComponentModel;
using System.Globalization;

namespace GameX.WbB.Formats.Props
{
    /// <summary>
    /// Static selection of client enums that are [AssessmentProperty]<para />
    /// These are properties sent to the client on id.
    /// </summary>
    public class AssessmentProperties : GenericPropertiesId<AssessmentPropertyAttribute> { }

    /// <summary>
    /// These are properties sent to the client on id.
    /// </summary>
    public class AssessmentPropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class CharacterOptions1Attribute : Attribute
    {
        public CharacterOptions1 Option { get; }
        public CharacterOptions1Attribute(CharacterOptions1 option) => Option = option;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CharacterOptions2Attribute : Attribute
    {
        public CharacterOptions2 Option { get; }
        public CharacterOptions2Attribute(CharacterOptions2 option) => Option = option;
    }

    /// <summary>
    /// Static selection of client enums that are NOT [ServerOnly]
    /// </summary>
    public class ClientProperties : GenericPropertiesId<ServerOnlyAttribute> { }

    /// <summary>
    /// These are properties that aren't saved to the shard.
    /// </summary>
    public class EphemeralAttribute : Attribute { }

    /// <summary>
    /// Static selection of client enums that are [Ephemeral]<para />
    /// These are properties that aren't saved to the shard.
    /// </summary>
    public class EphemeralProperties : GenericProperties<EphemeralAttribute> { }

    public enum PositionType : ushort
    {
        // S_CONSTANT: Type:             0x108E, Value: 11, CRASH_AND_TURN_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 17, SAVE_1_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 14, LAST_OUTSIDE_DEATH_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 10, PORTAL_STORM_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 18, SAVE_2_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 19, SAVE_3_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 20, SAVE_4_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 1, LOCATION_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 13, HOUSE_BOOT_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 21, SAVE_5_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 3, INSTANTIATION_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 9, LAST_PORTAL_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 16, LINKED_PORTAL_TWO_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 2, DESTINATION_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 22, SAVE_6_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 7, TARGET_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 23, SAVE_7_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 26, RELATIVE_DESTINATION_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 24, SAVE_8_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 25, SAVE_9_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 0, UNDEF_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 15, LINKED_LIFESTONE_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 27, TELEPORTED_CHARACTER_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 12, PORTAL_SUMMON_LOC_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 6, ACTIVATION_MOVE_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 4, SANCTUARY_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 5, HOME_POSITION
        // S_CONSTANT: Type:             0x108E, Value: 8, LINKED_PORTAL_ONE_POSITION
        /// <summary>
        /// I got nothing for you.
        /// </summary>
        Undef = 0,
        /// <summary>
        /// Current Position
        /// </summary>
        Location = 1,
        /// <summary>
        /// May be used to store where we are headed when we teleport (?)
        /// </summary>
        Destination = 2,
        /// <summary>
        /// Where will we pop into the world (?)
        /// </summary>
        Instantiation = 3,
        /// <summary>
        ///  Last Lifestone Used? (@ls)? | @home | @save | @recall
        /// </summary>
        Sanctuary = 4,
        /// <summary>
        /// This is the home, starting, or base position of an object.
        /// It's usually the position the object first spawned in at.
        /// </summary>
        [Ephemeral] Home = 5,
        /// <summary>
        /// The need to research
        /// </summary>
        ActivationMove = 6,
        /// <summary>
        /// The the position of target.
        /// </summary>
        Target = 7,
        /// <summary>
        /// Primary Portal Recall | Summon Primary Portal | Primary Portal Tie
        /// </summary>
        LinkedPortalOne = 8,
        /// <summary>
        /// Portal Recall (Last Used Portal that can be recalled to)
        /// </summary>
        LastPortal = 9,
        /// <summary>
        /// The portal storm - need research - maybe where you were portaled from or to - to does not seem likely to me.
        /// </summary>
        PortalStorm = 10,
        /// <summary>
        /// The crash and turn - I can't wait to find out.
        /// </summary>
        CrashAndTurn = 11,
        /// <summary>
        /// We are tracking what the portal ties are - could this be the physical location of the portal you summoned?   More research needed.
        /// </summary>
        PortalSummonLoc = 12,
        /// <summary>
        /// That little spot you get sent to just outside the barrier when the slum lord evicts you (??)
        /// </summary>
        HouseBoot = 13,
        /// <summary>
        /// The last outside death. --- boy would I love to extend this to cover deaths in dungeons as well.
        /// </summary>
        LastOutsideDeath = 14, // Location of Corpse
        /// <summary>
        /// The linked lifestone - Lifestone Recall | Lifestone Tie
        /// </summary>
        LinkedLifestone = 15,
        /// <summary>
        /// Secondary Portal Recall | Summon Secondary Portal | Secondary Portal Tie
        /// </summary>
        LinkedPortalTwo = 16,
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save1 = 17, // @save 1 | @home 1 | @recall 1
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save2 = 18, // @save 2 | @home 2 | @recall 2
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save3 = 19, // @save 3 | @home 3 | @recall 3
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save4 = 20, // @save 4 | @home 4 | @recall 4
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save5 = 21, // @save 5 | @home 5 | @recall 5
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save6 = 22, // @save 6 | @home 6 | @recall 6
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save7 = 23, // @save 7 | @home 7 | @recall 7
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save8 = 24, // @save 8 | @home 8 | @recall 8
        /// <summary>
        /// Admin Quick Recall Positions
        /// </summary>
        Save9 = 25, // @save 9 | @home 9 | @recall 9
        /// <summary>
        /// needs research
        /// </summary>
        RelativeDestination = 26,
        TeleportedCharacter = 27,
        [ServerOnly] PCAPRecordedLocation = 8040
    }

    public enum PropertyAttribute : ushort
    {
        Undef = 0,
        Strength = 1,
        Endurance = 2,
        Quickness = 3,
        Coordination = 4,
        Focus = 5,
        Self = 6
    }

    public enum PropertyAttribute2nd : ushort
    {
        Undef = 0,
        MaxHealth = 1,
        Health = 2,
        MaxStamina = 3,
        Stamina = 4,
        MaxMana = 5,
        Mana = 6
    }

    public enum PropertyBool : ushort
    {
        Undef = 0,
        [Ephemeral, ServerOnly] Stuck = 1,
        [Ephemeral] Open = 2,
        Locked = 3,
        RotProof = 4,
        AllegianceUpdateRequest = 5,
        AiUsesMana = 6,
        AiUseHumanMagicAnimations = 7,
        AllowGive = 8,
        CurrentlyAttacking = 9,
        AttackerAi = 10,
        [ServerOnly] IgnoreCollisions = 11,
        [ServerOnly] ReportCollisions = 12,
        [ServerOnly] Ethereal = 13,
        [ServerOnly] GravityStatus = 14,
        [ServerOnly] LightsStatus = 15,
        [ServerOnly] ScriptedCollision = 16,
        [ServerOnly] Inelastic = 17,
        [ServerOnly, Ephemeral] Visibility = 18,
        [ServerOnly] Attackable = 19,
        SafeSpellComponents = 20,
        AdvocateState = 21,
        Inscribable = 22,
        DestroyOnSell = 23,
        UiHidden = 24,
        IgnoreHouseBarriers = 25,
        HiddenAdmin = 26,
        PkWounder = 27,
        PkKiller = 28,
        NoCorpse = 29,
        UnderLifestoneProtection = 30,
        ItemManaUpdatePending = 31,
        [Ephemeral] GeneratorStatus = 32,
        [Ephemeral] ResetMessagePending = 33,
        DefaultOpen = 34,
        DefaultLocked = 35,
        DefaultOn = 36,
        OpenForBusiness = 37,
        IsFrozen = 38,
        DealMagicalItems = 39,
        LogoffImDead = 40,
        ReportCollisionsAsEnvironment = 41,
        AllowEdgeSlide = 42,
        AdvocateQuest = 43,
        [Ephemeral, SendOnLogin] IsAdmin = 44,
        [Ephemeral, SendOnLogin] IsArch = 45,
        [Ephemeral, SendOnLogin] IsSentinel = 46,
        [SendOnLogin] IsAdvocate = 47,
        CurrentlyPoweringUp = 48,
        [Ephemeral] GeneratorEnteredWorld = 49,
        NeverFailCasting = 50,
        VendorService = 51,
        AiImmobile = 52,
        DamagedByCollisions = 53,
        IsDynamic = 54,
        IsHot = 55,
        IsAffecting = 56,
        AffectsAis = 57,
        SpellQueueActive = 58,
        [Ephemeral] GeneratorDisabled = 59,
        IsAcceptingTells = 60,
        LoggingChannel = 61,
        OpensAnyLock = 62,
        UnlimitedUse = 63,
        GeneratedTreasureItem = 64,
        IgnoreMagicResist = 65,
        IgnoreMagicArmor = 66,
        AiAllowTrade = 67,
        [SendOnLogin] SpellComponentsRequired = 68,
        IsSellable = 69,
        IgnoreShieldsBySkill = 70,
        NoDraw = 71,
        ActivationUntargeted = 72,
        HouseHasGottenPriorityBootPos = 73,
        [Ephemeral] GeneratorAutomaticDestruction = 74,
        HouseHooksVisible = 75,
        HouseRequiresMonarch = 76,
        HouseHooksEnabled = 77,
        HouseNotifiedHudOfHookCount = 78,
        AiAcceptEverything = 79,
        IgnorePortalRestrictions = 80,
        RequiresBackpackSlot = 81,
        DontTurnOrMoveWhenGiving = 82,
        [ServerOnly] NpcLooksLikeObject = 83,
        IgnoreCloIcons = 84,
        AppraisalHasAllowedWielder = 85,
        ChestRegenOnClose = 86,
        LogoffInMinigame = 87,
        PortalShowDestination = 88,
        PortalIgnoresPkAttackTimer = 89,
        NpcInteractsSilently = 90,
        Retained = 91,
        IgnoreAuthor = 92,
        Limbo = 93,
        AppraisalHasAllowedActivator = 94,
        ExistedBeforeAllegianceXpChanges = 95,
        IsDeaf = 96,
        [Ephemeral, SendOnLogin] IsPsr = 97,
        Invincible = 98,
        Ivoryable = 99,
        Dyable = 100,
        CanGenerateRare = 101,
        CorpseGeneratedRare = 102,
        NonProjectileMagicImmune = 103,
        [SendOnLogin] ActdReceivedItems = 104,
        Unknown105 = 105,
        [Ephemeral] FirstEnterWorldDone = 106,
        RecallsDisabled = 107,
        RareUsesTimer = 108,
        ActdPreorderReceivedItems = 109,
        [Ephemeral] Afk = 110,
        IsGagged = 111,
        ProcSpellSelfTargeted = 112,
        IsAllegianceGagged = 113,
        EquipmentSetTriggerPiece = 114,
        Uninscribe = 115,
        WieldOnUse = 116,
        ChestClearedWhenClosed = 117,
        NeverAttack = 118,
        SuppressGenerateEffect = 119,
        TreasureCorpse = 120,
        EquipmentSetAddLevel = 121,
        BarberActive = 122,
        TopLayerPriority = 123,
        [SendOnLogin] NoHeldItemShown = 124,
        [SendOnLogin] LoginAtLifestone = 125,
        OlthoiPk = 126,
        [SendOnLogin] Account15Days = 127,
        HadNoVitae = 128,
        [SendOnLogin] NoOlthoiTalk = 129,
        AutowieldLeft = 130,
        // custom
        [ServerOnly] LinkedPortalOneSummon = 9001,
        [ServerOnly] LinkedPortalTwoSummon = 9002,
        [ServerOnly] HouseEvicted = 9003,
        [ServerOnly] UntrainedSkills = 9004,
        [Ephemeral, ServerOnly] IsEnvoy = 9005,
        [ServerOnly] UnspecializedSkills = 9006,
        [ServerOnly] FreeSkillResetRenewed = 9007,
        [ServerOnly] FreeAttributeResetRenewed = 9008,
        [ServerOnly] SkillTemplesTimerReset = 9009,
    }

    public enum PropertyDataId : ushort
    {
        [ServerOnly] Undef = 0,
        [ServerOnly] Setup = 1,
        [SendOnLogin] MotionTable = 2,
        [ServerOnly] SoundTable = 3,
        [SendOnLogin] CombatTable = 4,
        [ServerOnly] QualityFilter = 5,
        [ServerOnly] PaletteBase = 6,
        [ServerOnly] ClothingBase = 7,
        [ServerOnly] Icon = 8,
        [AssessmentProperty] EyesTexture = 9,
        [AssessmentProperty] NoseTexture = 10,
        [AssessmentProperty] MouthTexture = 11,
        [ServerOnly] DefaultEyesTexture = 12,
        [ServerOnly] DefaultNoseTexture = 13,
        [ServerOnly] DefaultMouthTexture = 14,
        [AssessmentProperty] HairPalette = 15,
        [AssessmentProperty] EyesPalette = 16,
        [AssessmentProperty] SkinPalette = 17,
        [ServerOnly] HeadObject = 18,
        [ServerOnly] ActivationAnimation = 19,
        [ServerOnly] InitMotion = 20,
        [ServerOnly] ActivationSound = 21,
        [ServerOnly] PhysicsEffectTable = 22,
        [ServerOnly] UseSound = 23,
        [ServerOnly] UseTargetAnimation = 24,
        [ServerOnly] UseTargetSuccessAnimation = 25,
        [ServerOnly] UseTargetFailureAnimation = 26,
        [ServerOnly] UseUserAnimation = 27,
        [ServerOnly] Spell = 28,
        [ServerOnly] SpellComponent = 29,
        [ServerOnly] PhysicsScript = 30,
        [ServerOnly] LinkedPortalOne = 31,
        [ServerOnly] WieldedTreasureType = 32,
        UnknownGuessedname = 33,
        UnknownGuessedname2 = 34,
        [ServerOnly] DeathTreasureType = 35,
        [ServerOnly] MutateFilter = 36,
        [ServerOnly] ItemSkillLimit = 37,
        [ServerOnly] UseCreateItem = 38,
        [ServerOnly] DeathSpell = 39,
        [ServerOnly] VendorsClassId = 40,
        [ServerOnly] ItemSpecializedOnly = 41,
        [ServerOnly] HouseId = 42,
        [ServerOnly] AccountHouseId = 43,
        [ServerOnly] RestrictionEffect = 44,
        [ServerOnly] CreationMutationFilter = 45,
        [ServerOnly] TsysMutationFilter = 46,
        [ServerOnly] LastPortal = 47,
        [ServerOnly] LinkedPortalTwo = 48,
        [ServerOnly] OriginalPortal = 49,
        [ServerOnly] IconOverlay = 50,
        [ServerOnly] IconOverlaySecondary = 51,
        [ServerOnly] IconUnderlay = 52,
        [ServerOnly] AugmentationMutationFilter = 53,
        [ServerOnly] AugmentationEffect = 54,
        ProcSpell = 55,
        [ServerOnly] AugmentationCreateItem = 56,
        [ServerOnly] AlternateCurrency = 57,
        [ServerOnly] BlueSurgeSpell = 58,
        [ServerOnly] YellowSurgeSpell = 59,
        [ServerOnly] RedSurgeSpell = 60,
        [ServerOnly] OlthoiDeathTreasureType = 61,
        [ServerOnly] PCAPRecordedWeenieHeader = 8001,
        [ServerOnly] PCAPRecordedWeenieHeader2 = 8002,
        [ServerOnly] PCAPRecordedObjectDesc = 8003,
        [ServerOnly] PCAPRecordedPhysicsDesc = 8005,
        [ServerOnly] PCAPRecordedParentLocation = 8009,
        [ServerOnly] PCAPRecordedDefaultScript = 8019,
        [ServerOnly] PCAPRecordedTimestamp0 = 8020,
        [ServerOnly] PCAPRecordedTimestamp1 = 8021,
        [ServerOnly] PCAPRecordedTimestamp2 = 8022,
        [ServerOnly] PCAPRecordedTimestamp3 = 8023,
        [ServerOnly] PCAPRecordedTimestamp4 = 8024,
        [ServerOnly] PCAPRecordedTimestamp5 = 8025,
        [ServerOnly] PCAPRecordedTimestamp6 = 8026,
        [ServerOnly] PCAPRecordedTimestamp7 = 8027,
        [ServerOnly] PCAPRecordedTimestamp8 = 8028,
        [ServerOnly] PCAPRecordedTimestamp9 = 8029,
        [ServerOnly] PCAPRecordedMaxVelocityEstimated = 8030,
        [ServerOnly] PCAPPhysicsDIDDataTemplatedFrom = 8044,
        //[ServerOnly] HairTexture = 9001,
        //[ServerOnly] DefaultHairTexture = 9002,
    }

    partial class PropertyExtensions
    {
        public static string GetValueEnumName(this PropertyDataId property, uint value)
        {
            switch (property)
            {
                case PropertyDataId.ActivationAnimation:
                case PropertyDataId.InitMotion:
                case PropertyDataId.UseTargetAnimation:
                case PropertyDataId.UseTargetFailureAnimation:
                case PropertyDataId.UseTargetSuccessAnimation:
                case PropertyDataId.UseUserAnimation: return Enum.GetName(typeof(MotionCommand), value);
                case PropertyDataId.PhysicsScript:
                case PropertyDataId.RestrictionEffect: return Enum.GetName(typeof(PlayScript), value);
                case PropertyDataId.ActivationSound:
                case PropertyDataId.UseSound: return Enum.GetName(typeof(Sound), value);
                case PropertyDataId.WieldedTreasureType:
                case PropertyDataId.DeathTreasureType: /*todo*/ return null;
                case PropertyDataId.Spell:
                case PropertyDataId.DeathSpell:
                case PropertyDataId.ProcSpell:
                case PropertyDataId.RedSurgeSpell:
                case PropertyDataId.BlueSurgeSpell:
                case PropertyDataId.YellowSurgeSpell: return Enum.GetName(typeof(SpellId), value);
                case PropertyDataId.ItemSkillLimit:
                case PropertyDataId.ItemSpecializedOnly: return System.Enum.GetName(typeof(Skill), value);
                case PropertyDataId.PCAPRecordedParentLocation: return Enum.GetName(typeof(ParentLocation), value);
                case PropertyDataId.PCAPRecordedDefaultScript: return System.Enum.GetName(typeof(MotionCommand), value);
                default: return null;
            }
        }

        public static bool IsHexData(this PropertyDataId property)
        {
            switch (property)
            {
                case PropertyDataId.AccountHouseId:
                case PropertyDataId.AlternateCurrency:
                case PropertyDataId.AugmentationCreateItem:
                case PropertyDataId.AugmentationEffect:
                case PropertyDataId.BlueSurgeSpell:
                case PropertyDataId.DeathSpell:
                case PropertyDataId.DeathTreasureType:
                case PropertyDataId.HouseId:
                case PropertyDataId.ItemSkillLimit:
                case PropertyDataId.ItemSpecializedOnly:
                case PropertyDataId.LastPortal:
                case PropertyDataId.LinkedPortalOne:
                case PropertyDataId.LinkedPortalTwo:
                case PropertyDataId.OlthoiDeathTreasureType:
                case PropertyDataId.OriginalPortal:
                case PropertyDataId.PhysicsScript:
                case PropertyDataId.ProcSpell:
                case PropertyDataId.RedSurgeSpell:
                case PropertyDataId.RestrictionEffect:
                case PropertyDataId.Spell:
                case PropertyDataId.SpellComponent:
                case PropertyDataId.UseCreateItem:
                case PropertyDataId.UseSound:
                case PropertyDataId.VendorsClassId:
                case PropertyDataId.WieldedTreasureType:
                case PropertyDataId.YellowSurgeSpell:
                case PropertyDataId x when x >= PropertyDataId.PCAPRecordedWeenieHeader: return false;
                default: return true;
            }
        }
    }

    public enum PropertyFloat : ushort
    {
        Undef = 0,
        HeartbeatInterval = 1,
        [Ephemeral] HeartbeatTimestamp = 2,
        HealthRate = 3,
        StaminaRate = 4,
        ManaRate = 5,
        HealthUponResurrection = 6,
        StaminaUponResurrection = 7,
        ManaUponResurrection = 8,
        StartTime = 9,
        StopTime = 10,
        ResetInterval = 11,
        Shade = 12,
        ArmorModVsSlash = 13,
        ArmorModVsPierce = 14,
        ArmorModVsBludgeon = 15,
        ArmorModVsCold = 16,
        ArmorModVsFire = 17,
        ArmorModVsAcid = 18,
        ArmorModVsElectric = 19,
        CombatSpeed = 20,
        WeaponLength = 21,
        DamageVariance = 22,
        CurrentPowerMod = 23,
        AccuracyMod = 24,
        StrengthMod = 25,
        MaximumVelocity = 26,
        RotationSpeed = 27,
        MotionTimestamp = 28,
        WeaponDefense = 29,
        WimpyLevel = 30,
        VisualAwarenessRange = 31,
        AuralAwarenessRange = 32,
        PerceptionLevel = 33,
        PowerupTime = 34,
        MaxChargeDistance = 35,
        ChargeSpeed = 36,
        BuyPrice = 37,
        SellPrice = 38,
        DefaultScale = 39,
        LockpickMod = 40,
        RegenerationInterval = 41,
        RegenerationTimestamp = 42,
        GeneratorRadius = 43,
        TimeToRot = 44,
        DeathTimestamp = 45,
        PkTimestamp = 46,
        VictimTimestamp = 47,
        LoginTimestamp = 48,
        CreationTimestamp = 49,
        MinimumTimeSincePk = 50,
        DeprecatedHousekeepingPriority = 51,
        AbuseLoggingTimestamp = 52,
        [Ephemeral] LastPortalTeleportTimestamp = 53,
        UseRadius = 54,
        HomeRadius = 55,
        ReleasedTimestamp = 56,
        MinHomeRadius = 57,
        Facing = 58,
        [Ephemeral] ResetTimestamp = 59,
        LogoffTimestamp = 60,
        EconRecoveryInterval = 61,
        WeaponOffense = 62,
        DamageMod = 63,
        ResistSlash = 64,
        ResistPierce = 65,
        ResistBludgeon = 66,
        ResistFire = 67,
        ResistCold = 68,
        ResistAcid = 69,
        ResistElectric = 70,
        ResistHealthBoost = 71,
        ResistStaminaDrain = 72,
        ResistStaminaBoost = 73,
        ResistManaDrain = 74,
        ResistManaBoost = 75,
        Translucency = 76,
        PhysicsScriptIntensity = 77,
        Friction = 78,
        Elasticity = 79,
        AiUseMagicDelay = 80,
        ItemMinSpellcraftMod = 81,
        ItemMaxSpellcraftMod = 82,
        ItemRankProbability = 83,
        Shade2 = 84,
        Shade3 = 85,
        Shade4 = 86,
        ItemEfficiency = 87,
        ItemManaUpdateTimestamp = 88,
        SpellGestureSpeedMod = 89,
        SpellStanceSpeedMod = 90,
        AllegianceAppraisalTimestamp = 91,
        PowerLevel = 92,
        AccuracyLevel = 93,
        AttackAngle = 94,
        AttackTimestamp = 95,
        CheckpointTimestamp = 96,
        SoldTimestamp = 97,
        UseTimestamp = 98,
        [Ephemeral] UseLockTimestamp = 99,
        HealkitMod = 100,
        FrozenTimestamp = 101,
        HealthRateMod = 102,
        AllegianceSwearTimestamp = 103,
        ObviousRadarRange = 104,
        HotspotCycleTime = 105,
        HotspotCycleTimeVariance = 106,
        SpamTimestamp = 107,
        SpamRate = 108,
        BondWieldedTreasure = 109,
        BulkMod = 110,
        SizeMod = 111,
        GagTimestamp = 112,
        GeneratorUpdateTimestamp = 113,
        DeathSpamTimestamp = 114,
        DeathSpamRate = 115,
        WildAttackProbability = 116,
        FocusedProbability = 117,
        CrashAndTurnProbability = 118,
        CrashAndTurnRadius = 119,
        CrashAndTurnBias = 120,
        GeneratorInitialDelay = 121,
        AiAcquireHealth = 122,
        AiAcquireStamina = 123,
        AiAcquireMana = 124,
        /// <summary>
        /// this had a default of "1" - leaving comment to investigate potential options for defaulting these things (125)
        /// </summary>
        [SendOnLogin] ResistHealthDrain = 125,
        LifestoneProtectionTimestamp = 126,
        AiCounteractEnchantment = 127,
        AiDispelEnchantment = 128,
        TradeTimestamp = 129,
        AiTargetedDetectionRadius = 130,
        EmotePriority = 131,
        [Ephemeral]
        LastTeleportStartTimestamp = 132,
        EventSpamTimestamp = 133,
        EventSpamRate = 134,
        InventoryOffset = 135,
        CriticalMultiplier = 136,
        ManaStoneDestroyChance = 137,
        SlayerDamageBonus = 138,
        AllegianceInfoSpamTimestamp = 139,
        AllegianceInfoSpamRate = 140,
        NextSpellcastTimestamp = 141,
        [Ephemeral] AppraisalRequestedTimestamp = 142,
        AppraisalHeartbeatDueTimestamp = 143,
        ManaConversionMod = 144,
        LastPkAttackTimestamp = 145,
        FellowshipUpdateTimestamp = 146,
        CriticalFrequency = 147,
        LimboStartTimestamp = 148,
        WeaponMissileDefense = 149,
        WeaponMagicDefense = 150,
        IgnoreShield = 151,
        ElementalDamageMod = 152,
        StartMissileAttackTimestamp = 153,
        LastRareUsedTimestamp = 154,
        IgnoreArmor = 155,
        ProcSpellRate = 156,
        ResistanceModifier = 157,
        AllegianceGagTimestamp = 158,
        AbsorbMagicDamage = 159,
        CachedMaxAbsorbMagicDamage = 160,
        GagDuration = 161,
        AllegianceGagDuration = 162,
        [SendOnLogin] GlobalXpMod = 163,
        HealingModifier = 164,
        ArmorModVsNether = 165,
        ResistNether = 166,
        CooldownDuration = 167,
        [SendOnLogin] WeaponAuraOffense = 168,
        [SendOnLogin] WeaponAuraDefense = 169,
        [SendOnLogin] WeaponAuraElemental = 170,
        [SendOnLogin] WeaponAuraManaConv = 171,
        [ServerOnly] PCAPRecordedWorkmanship = 8004,
        [ServerOnly] PCAPRecordedVelocityX = 8010,
        [ServerOnly] PCAPRecordedVelocityY = 8011,
        [ServerOnly] PCAPRecordedVelocityZ = 8012,
        [ServerOnly] PCAPRecordedAccelerationX = 8013,
        [ServerOnly] PCAPRecordedAccelerationY = 8014,
        [ServerOnly] PCAPRecordedAccelerationZ = 8015,
        [ServerOnly] PCAPRecordeOmegaX = 8016,
        [ServerOnly] PCAPRecordeOmegaY = 8017,
        [ServerOnly] PCAPRecordeOmegaZ = 8018
    }

    public enum PropertyInstanceId : ushort
    {
        Undef = 0,
        Owner = 1,
        Container = 2,
        Wielder = 3,
        Freezer = 4,
        [Ephemeral] Viewer = 5,
        [Ephemeral] Generator = 6,
        Scribe = 7,
        [Ephemeral] CurrentCombatTarget = 8,
        [Ephemeral] CurrentEnemy = 9,
        ProjectileLauncher = 10,
        [Ephemeral] CurrentAttacker = 11,
        [Ephemeral] CurrentDamager = 12,
        [Ephemeral] CurrentFollowTarget = 13,
        [Ephemeral] CurrentAppraisalTarget = 14,
        [Ephemeral] CurrentFellowshipAppraisalTarget = 15,
        ActivationTarget = 16,
        Creator = 17,
        Victim = 18,
        Killer = 19,
        Vendor = 20,
        Customer = 21,
        Bonded = 22,
        Wounder = 23,
        [SendOnLogin] Allegiance = 24,
        [SendOnLogin] Patron = 25,
        Monarch = 26,
        [Ephemeral] CombatTarget = 27,
        [Ephemeral] HealthQueryTarget = 28,
        [ServerOnly, Ephemeral] LastUnlocker = 29,
        CrashAndTurnTarget = 30,
        AllowedActivator = 31,
        HouseOwner = 32,
        House = 33,
        Slumlord = 34,
        [Ephemeral] ManaQueryTarget = 35,
        CurrentGame = 36,
        [Ephemeral] RequestedAppraisalTarget = 37,
        AllowedWielder = 38,
        AssignedTarget = 39,
        LimboSource = 40,
        Snooper = 41,
        TeleportedCharacter = 42,
        Pet = 43,
        PetOwner = 44,
        PetDevice = 45,
        [ServerOnly] PCAPRecordedObjectIID = 8000,
        [ServerOnly] PCAPRecordedParentIID = 8008
    }

    public enum PropertyInt : ushort
    {
        Undef = 0,
        [ServerOnly] ItemType = 1,
        CreatureType = 2,
        [ServerOnly] PaletteTemplate = 3,
        ClothingPriority = 4,
        [SendOnLogin] EncumbranceVal = 5, // ENCUMB_VAL_INT,
        [SendOnLogin] ItemsCapacity = 6,
        [SendOnLogin] ContainersCapacity = 7,
        [ServerOnly] Mass = 8,
        [ServerOnly] ValidLocations = 9, // LOCATIONS_INT
        [ServerOnly] CurrentWieldedLocation = 10,
        [ServerOnly] MaxStackSize = 11,
        [ServerOnly] StackSize = 12,
        [ServerOnly] StackUnitEncumbrance = 13,
        [ServerOnly] StackUnitMass = 14,
        [ServerOnly] StackUnitValue = 15,
        [ServerOnly] ItemUseable = 16,
        RareId = 17,
        [ServerOnly] UiEffects = 18,
        Value = 19,
        [Ephemeral, SendOnLogin] CoinValue = 20,
        TotalExperience = 21,
        AvailableCharacter = 22,
        TotalSkillCredits = 23,
        [SendOnLogin] AvailableSkillCredits = 24,
        [SendOnLogin] Level = 25,
        AccountRequirements = 26,
        ArmorType = 27,
        ArmorLevel = 28,
        AllegianceCpPool = 29,
        [SendOnLogin] AllegianceRank = 30,
        ChannelsAllowed = 31,
        ChannelsActive = 32,
        Bonded = 33,
        MonarchsRank = 34,
        AllegianceFollowers = 35,
        ResistMagic = 36,
        ResistItemAppraisal = 37,
        ResistLockpick = 38,
        DeprecatedResistRepair = 39,
        [SendOnLogin] CombatMode = 40,
        CurrentAttackHeight = 41,
        CombatCollisions = 42,
        [SendOnLogin] NumDeaths = 43,
        Damage = 44,
        DamageType = 45,
        [ServerOnly] DefaultCombatStyle = 46,
        [SendOnLogin] AttackType = 47,
        WeaponSkill = 48,
        WeaponTime = 49,
        AmmoType = 50,
        CombatUse = 51,
        [ServerOnly] ParentLocation = 52,
        /// <summary>
        /// TODO: Migrate inventory order away from this and instead use the new InventoryOrder property
        /// TODO: PlacementPosition is used (very sparingly) in cache.bin, so it has (or had) a meaning at one point before we hijacked it
        /// TODO: and used it for our own inventory order
        /// </summary>
        [ServerOnly] PlacementPosition = 53,
        WeaponEncumbrance = 54,
        WeaponMass = 55,
        ShieldValue = 56,
        ShieldEncumbrance = 57,
        MissileInventoryLocation = 58,
        FullDamageType = 59,
        WeaponRange = 60,
        AttackersSkill = 61,
        DefendersSkill = 62,
        AttackersSkillValue = 63,
        AttackersClass = 64,
        [ServerOnly] Placement = 65,
        CheckpointStatus = 66,
        Tolerance = 67,
        TargetingTactic = 68,
        CombatTactic = 69,
        HomesickTargetingTactic = 70,
        NumFollowFailures = 71,
        FriendType = 72,
        FoeType = 73,
        MerchandiseItemTypes = 74,
        MerchandiseMinValue = 75,
        MerchandiseMaxValue = 76,
        NumItemsSold = 77,
        NumItemsBought = 78,
        MoneyIncome = 79,
        MoneyOutflow = 80,
        [Ephemeral] MaxGeneratedObjects = 81,
        [Ephemeral] InitGeneratedObjects = 82,
        ActivationResponse = 83,
        OriginalValue = 84,
        NumMoveFailures = 85,
        MinLevel = 86,
        MaxLevel = 87,
        LockpickMod = 88,
        BoosterEnum = 89,
        BoostValue = 90,
        MaxStructure = 91,
        Structure = 92,
        [ServerOnly] PhysicsState = 93,
        [ServerOnly] TargetType = 94,
        RadarBlipColor = 95,
        EncumbranceCapacity = 96,
        LoginTimestamp = 97,
        [SendOnLogin] CreationTimestamp = 98,
        PkLevelModifier = 99,
        GeneratorType = 100,
        AiAllowedCombatStyle = 101,
        LogoffTimestamp = 102,
        GeneratorDestructionType = 103,
        ActivationCreateClass = 104,
        ItemWorkmanship = 105,
        ItemSpellcraft = 106,
        ItemCurMana = 107,
        ItemMaxMana = 108,
        ItemDifficulty = 109,
        ItemAllegianceRankLimit = 110,
        PortalBitmask = 111,
        AdvocateLevel = 112,
        [SendOnLogin] Gender = 113,
        Attuned = 114,
        ItemSkillLevelLimit = 115,
        GateLogic = 116,
        ItemManaCost = 117,
        Logoff = 118,
        Active = 119,
        AttackHeight = 120,
        NumAttackFailures = 121,
        AiCpThreshold = 122,
        AiAdvancementStrategy = 123,
        Version = 124,
        [SendOnLogin] Age = 125,
        VendorHappyMean = 126,
        VendorHappyVariance = 127,
        CloakStatus = 128,
        [SendOnLogin] VitaeCpPool = 129,
        NumServicesSold = 130,
        MaterialType = 131,
        [SendOnLogin] NumAllegianceBreaks = 132,
        [Ephemeral] ShowableOnRadar = 133,
        [SendOnLogin] PlayerKillerStatus = 134,
        VendorHappyMaxItems = 135,
        ScorePageNum = 136,
        ScoreConfigNum = 137,
        ScoreNumScores = 138,
        [SendOnLogin] DeathLevel = 139,
        [ServerOnly] AiOptions = 140,
        [ServerOnly] OpenToEveryone = 141,
        [ServerOnly] GeneratorTimeType = 142,
        [ServerOnly] GeneratorStartTime = 143,
        [ServerOnly] GeneratorEndTime = 144,
        [ServerOnly] GeneratorEndDestructionType = 145,
        [ServerOnly] XpOverride = 146,
        NumCrashAndTurns = 147,
        ComponentWarningThreshold = 148,
        HouseStatus = 149,
        [ServerOnly] HookPlacement = 150,
        [ServerOnly] HookType = 151,
        [ServerOnly] HookItemType = 152,
        AiPpThreshold = 153,
        GeneratorVersion = 154,
        HouseType = 155,
        PickupEmoteOffset = 156,
        WeenieIteration = 157,
        WieldRequirements = 158,
        WieldSkillType = 159,
        WieldDifficulty = 160,
        [ServerOnly] HouseMaxHooksUsable = 161,
        [ServerOnly, Ephemeral] HouseCurrentHooksUsable = 162,
        AllegianceMinLevel = 163,
        AllegianceMaxLevel = 164,
        HouseRelinkHookCount = 165,
        SlayerCreatureType = 166,
        ConfirmationInProgress = 167,
        ConfirmationTypeInProgress = 168,
        TsysMutationData = 169,
        NumItemsInMaterial = 170,
        NumTimesTinkered = 171,
        AppraisalLongDescDecoration = 172,
        AppraisalLockpickSuccessPercent = 173,
        [Ephemeral] AppraisalPages = 174,
        [Ephemeral] AppraisalMaxPages = 175,
        AppraisalItemSkill = 176,
        GemCount = 177,
        GemType = 178,
        ImbuedEffect = 179,
        AttackersRawSkillValue = 180,
        [SendOnLogin] ChessRank = 181,
        ChessTotalGames = 182,
        ChessGamesWon = 183,
        ChessGamesLost = 184,
        TypeOfAlteration = 185,
        SkillToBeAltered = 186,
        SkillAlterationCount = 187,
        [SendOnLogin] HeritageGroup = 188,
        TransferFromAttribute = 189,
        TransferToAttribute = 190,
        AttributeTransferCount = 191,
        [SendOnLogin] FakeFishingSkill = 192,
        NumKeys = 193,
        DeathTimestamp = 194,
        PkTimestamp = 195,
        VictimTimestamp = 196,
        [ServerOnly] HookGroup = 197,
        AllegianceSwearTimestamp = 198,
        [SendOnLogin] HousePurchaseTimestamp = 199,
        RedirectableEquippedArmorCount = 200,
        MeleeDefenseImbuedEffectTypeCache = 201,
        MissileDefenseImbuedEffectTypeCache = 202,
        MagicDefenseImbuedEffectTypeCache = 203,
        ElementalDamageBonus = 204,
        ImbueAttempts = 205,
        ImbueSuccesses = 206,
        CreatureKills = 207,
        PlayerKillsPk = 208,
        PlayerKillsPkl = 209,
        RaresTierOne = 210,
        RaresTierTwo = 211,
        RaresTierThree = 212,
        RaresTierFour = 213,
        RaresTierFive = 214,
        [SendOnLogin] AugmentationStat = 215,
        [SendOnLogin] AugmentationFamilyStat = 216,
        [SendOnLogin] AugmentationInnateFamily = 217,
        [SendOnLogin] AugmentationInnateStrength = 218,
        [SendOnLogin] AugmentationInnateEndurance = 219,
        [SendOnLogin] AugmentationInnateCoordination = 220,
        [SendOnLogin] AugmentationInnateQuickness = 221,
        [SendOnLogin] AugmentationInnateFocus = 222,
        [SendOnLogin] AugmentationInnateSelf = 223,
        [SendOnLogin] AugmentationSpecializeSalvaging = 224,
        [SendOnLogin] AugmentationSpecializeItemTinkering = 225,
        [SendOnLogin] AugmentationSpecializeArmorTinkering = 226,
        [SendOnLogin] AugmentationSpecializeMagicItemTinkering = 227,
        [SendOnLogin] AugmentationSpecializeWeaponTinkering = 228,
        [SendOnLogin] AugmentationExtraPackSlot = 229,
        [SendOnLogin] AugmentationIncreasedCarryingCapacity = 230,
        [SendOnLogin] AugmentationLessDeathItemLoss = 231,
        [SendOnLogin] AugmentationSpellsRemainPastDeath = 232,
        [SendOnLogin] AugmentationCriticalDefense = 233,
        [SendOnLogin] AugmentationBonusXp = 234,
        [SendOnLogin] AugmentationBonusSalvage = 235,
        [SendOnLogin] AugmentationBonusImbueChance = 236,
        [SendOnLogin] AugmentationFasterRegen = 237,
        [SendOnLogin] AugmentationIncreasedSpellDuration = 238,
        [SendOnLogin] AugmentationResistanceFamily = 239,
        [SendOnLogin] AugmentationResistanceSlash = 240,
        [SendOnLogin] AugmentationResistancePierce = 241,
        [SendOnLogin] AugmentationResistanceBlunt = 242,
        [SendOnLogin] AugmentationResistanceAcid = 243,
        [SendOnLogin] AugmentationResistanceFire = 244,
        [SendOnLogin] AugmentationResistanceFrost = 245,
        [SendOnLogin] AugmentationResistanceLightning = 246,
        RaresTierOneLogin = 247,
        RaresTierTwoLogin = 248,
        RaresTierThreeLogin = 249,
        RaresTierFourLogin = 250,
        RaresTierFiveLogin = 251,
        RaresLoginTimestamp = 252,
        RaresTierSix = 253,
        RaresTierSeven = 254,
        RaresTierSixLogin = 255,
        RaresTierSevenLogin = 256,
        ItemAttributeLimit = 257,
        ItemAttributeLevelLimit = 258,
        ItemAttribute2ndLimit = 259,
        ItemAttribute2ndLevelLimit = 260,
        CharacterTitleId = 261,
        NumCharacterTitles = 262,
        ResistanceModifierType = 263,
        FreeTinkersBitfield = 264,
        EquipmentSetId = 265,
        PetClass = 266,
        Lifespan = 267,
        [Ephemeral]
        RemainingLifespan = 268,
        UseCreateQuantity = 269,
        WieldRequirements2 = 270,
        WieldSkillType2 = 271,
        WieldDifficulty2 = 272,
        WieldRequirements3 = 273,
        WieldSkillType3 = 274,
        WieldDifficulty3 = 275,
        WieldRequirements4 = 276,
        WieldSkillType4 = 277,
        WieldDifficulty4 = 278,
        Unique = 279,
        SharedCooldown = 280,
        [SendOnLogin] Faction1Bits = 281,
        Faction2Bits = 282,
        Faction3Bits = 283,
        Hatred1Bits = 284,
        Hatred2Bits = 285,
        Hatred3Bits = 286,
        [SendOnLogin] SocietyRankCelhan = 287,
        [SendOnLogin] SocietyRankEldweb = 288,
        [SendOnLogin] SocietyRankRadblo = 289,
        HearLocalSignals = 290,
        HearLocalSignalsRadius = 291,
        Cleaving = 292,
        [SendOnLogin] AugmentationSpecializeGearcraft = 293,
        [SendOnLogin] AugmentationInfusedCreatureMagic = 294,
        [SendOnLogin] AugmentationInfusedItemMagic = 295,
        [SendOnLogin] AugmentationInfusedLifeMagic = 296,
        [SendOnLogin] AugmentationInfusedWarMagic = 297,
        [SendOnLogin] AugmentationCriticalExpertise = 298,
        [SendOnLogin] AugmentationCriticalPower = 299,
        [SendOnLogin] AugmentationSkilledMelee = 300,
        [SendOnLogin] AugmentationSkilledMissile = 301,
        [SendOnLogin] AugmentationSkilledMagic = 302,
        ImbuedEffect2 = 303,
        ImbuedEffect3 = 304,
        ImbuedEffect4 = 305,
        ImbuedEffect5 = 306,
        [SendOnLogin] DamageRating = 307,
        [SendOnLogin] DamageResistRating = 308,
        [SendOnLogin] AugmentationDamageBonus = 309,
        [SendOnLogin] AugmentationDamageReduction = 310,
        ImbueStackingBits = 311,
        [SendOnLogin] HealOverTime = 312,
        [SendOnLogin] CritRating = 313,
        [SendOnLogin] CritDamageRating = 314,
        [SendOnLogin] CritResistRating = 315,
        [SendOnLogin] CritDamageResistRating = 316,
        [SendOnLogin] HealingResistRating = 317,
        [SendOnLogin] DamageOverTime = 318,
        ItemMaxLevel = 319,
        ItemXpStyle = 320,
        EquipmentSetExtra = 321,
        [SendOnLogin] AetheriaBitfield = 322,
        [SendOnLogin] HealingBoostRating = 323,
        HeritageSpecificArmor = 324,
        AlternateRacialSkills = 325,
        [SendOnLogin] AugmentationJackOfAllTrades = 326,
        [SendOnLogin] AugmentationResistanceNether = 327,
        [SendOnLogin] AugmentationInfusedVoidMagic = 328,
        [SendOnLogin] WeaknessRating = 329,
        [SendOnLogin] NetherOverTime = 330,
        [SendOnLogin] NetherResistRating = 331,
        LuminanceAward = 332,
        [SendOnLogin] LumAugDamageRating = 333,
        [SendOnLogin] LumAugDamageReductionRating = 334,
        [SendOnLogin] LumAugCritDamageRating = 335,
        [SendOnLogin] LumAugCritReductionRating = 336,
        [SendOnLogin] LumAugSurgeEffectRating = 337,
        [SendOnLogin] LumAugSurgeChanceRating = 338,
        [SendOnLogin] LumAugItemManaUsage = 339,
        [SendOnLogin] LumAugItemManaGain = 340,
        [SendOnLogin] LumAugVitality = 341,
        [SendOnLogin] LumAugHealingRating = 342,
        [SendOnLogin] LumAugSkilledCraft = 343,
        [SendOnLogin] LumAugSkilledSpec = 344,
        [SendOnLogin] LumAugNoDestroyCraft = 345,
        RestrictInteraction = 346,
        OlthoiLootTimestamp = 347,
        OlthoiLootStep = 348,
        UseCreatesContractId = 349,
        [SendOnLogin] DotResistRating = 350,
        [SendOnLogin] LifeResistRating = 351,
        CloakWeaveProc = 352,
        WeaponType = 353,
        [SendOnLogin] MeleeMastery = 354,
        [SendOnLogin] RangedMastery = 355,
        SneakAttackRating = 356,
        RecklessnessRating = 357,
        DeceptionRating = 358,
        CombatPetRange = 359,
        [SendOnLogin] WeaponAuraDamage = 360,
        [SendOnLogin] WeaponAuraSpeed = 361,
        [SendOnLogin] SummoningMastery = 362,
        HeartbeatLifespan = 363,
        UseLevelRequirement = 364,
        [SendOnLogin] LumAugAllSkills = 365,
        UseRequiresSkill = 366,
        UseRequiresSkillLevel = 367,
        UseRequiresSkillSpec = 368,
        UseRequiresLevel = 369,
        [SendOnLogin] GearDamage = 370,
        [SendOnLogin] GearDamageResist = 371,
        [SendOnLogin] GearCrit = 372,
        [SendOnLogin] GearCritResist = 373,
        [SendOnLogin] GearCritDamage = 374,
        [SendOnLogin] GearCritDamageResist = 375,
        [SendOnLogin] GearHealingBoost = 376,
        [SendOnLogin] GearNetherResist = 377,
        [SendOnLogin] GearLifeResist = 378,
        [SendOnLogin] GearMaxHealth = 379,
        Unknown380 = 380,
        [SendOnLogin] PKDamageRating = 381,
        [SendOnLogin] PKDamageResistRating = 382,
        [SendOnLogin] GearPKDamageRating = 383,
        [SendOnLogin] GearPKDamageResistRating = 384,
        Unknown385 = 385,
        /// <summary>
        /// Overpower chance % for endgame creatures.
        /// </summary>
        [SendOnLogin] Overpower = 386,
        [SendOnLogin] OverpowerResist = 387,
        // Client does not display accurately
        [SendOnLogin] GearOverpower = 388,
        // Client does not display accurately
        [SendOnLogin] GearOverpowerResist = 389,
        // Number of times a character has enlightened
        [SendOnLogin] Enlightenment = 390,
        [ServerOnly] PCAPRecordedAutonomousMovement = 8007,
        [ServerOnly] PCAPRecordedMaxVelocityEstimated = 8030,
        [ServerOnly] PCAPRecordedPlacement = 8041,
        [ServerOnly] PCAPRecordedAppraisalPages = 8042,
        [ServerOnly] PCAPRecordedAppraisalMaxPages = 8043,
        //[ServerOnly] TotalLogins = 9001,
        //[ServerOnly] DeletionTimestamp = 9002,
        //[ServerOnly] CharacterOptions1 = 9003,
        //[ServerOnly] CharacterOptions2 = 9004,
        //[ServerOnly] LootTier = 9005,
        //[ServerOnly] GeneratorProbability = 9006,
        //[ServerOnly] WeenieType = 9007, // I don't think this property type is needed anymore. We don't store the weenie type in the property bags, we store it as a separate field in the base objects.
        [ServerOnly] CurrentLoyaltyAtLastLogoff = 9008,
        [ServerOnly] CurrentLeadershipAtLastLogoff = 9009,
        [ServerOnly] AllegianceOfficerRank = 9010,
        [ServerOnly] HouseRentTimestamp = 9011,
        /// <summary>
        ///  Stores the player's selected hairstyle at creation or after a barber use. This is used only for Gear Knights and Olthoi characters who have more than a single part/texture for a "hairstyle" (BodyStyle)
        /// </summary>
        [ServerOnly] Hairstyle = 9012,
        /// <summary>
        /// Used to store the calculated Clothing Priority for use with armor reduced items and items like Over-Robes.
        /// </summary>
        [Ephemeral, ServerOnly] VisualClothingPriority = 9013,
        [ServerOnly] SquelchGlobal = 9014,
        /// <summary>
        /// TODO: This is a place holder for future use. See PlacementPosition
        /// This is the sort order for items in a container
        /// </summary>
        [ServerOnly] InventoryOrder = 9015,
    }

    partial class PropertyExtensions
    {
        public static string GetValueEnumName(this PropertyInt property, int value)
        {
            switch (property)
            {
                case PropertyInt.ActivationResponse: return Enum.GetName(typeof(ActivationResponse), value);
                case PropertyInt.AetheriaBitfield: return Enum.GetName(typeof(AetheriaBitfield), value);
                case PropertyInt.AttackHeight: return Enum.GetName(typeof(AttackHeight), value);
                case PropertyInt.AttackType: return Enum.GetName(typeof(AttackType), value);
                case PropertyInt.Attuned: return Enum.GetName(typeof(AttunedStatus), value);
                case PropertyInt.AmmoType: return Enum.GetName(typeof(AmmoType), value);
                case PropertyInt.Bonded: return Enum.GetName(typeof(BondedStatus), value);
                case PropertyInt.ChannelsActive:
                case PropertyInt.ChannelsAllowed: return Enum.GetName(typeof(Channel), value);
                case PropertyInt.CombatMode: return Enum.GetName(typeof(CombatMode), value);
                case PropertyInt.DefaultCombatStyle:
                case PropertyInt.AiAllowedCombatStyle: return Enum.GetName(typeof(CombatStyle), value);
                case PropertyInt.CombatUse: return Enum.GetName(typeof(CombatUse), value);
                case PropertyInt.ClothingPriority: return Enum.GetName(typeof(CoverageMask), value);
                case PropertyInt.CreatureType:
                case PropertyInt.SlayerCreatureType:
                case PropertyInt.FoeType:
                case PropertyInt.FriendType: return Enum.GetName(typeof(CreatureType), value);
                case PropertyInt.DamageType:
                case PropertyInt.ResistanceModifierType: return Enum.GetName(typeof(DamageType), value);
                case PropertyInt.CurrentWieldedLocation:
                case PropertyInt.ValidLocations: return Enum.GetName(typeof(EquipMask), value);
                case PropertyInt.EquipmentSetId: return Enum.GetName(typeof(EquipmentSet), value);
                case PropertyInt.Gender: return Enum.GetName(typeof(Gender), value);
                case PropertyInt.GeneratorDestructionType:
                case PropertyInt.GeneratorEndDestructionType: return Enum.GetName(typeof(GeneratorDestruct), value);
                case PropertyInt.GeneratorTimeType: return Enum.GetName(typeof(GeneratorTimeType), value);
                case PropertyInt.GeneratorType: return Enum.GetName(typeof(GeneratorType), value);
                case PropertyInt.HeritageGroup:
                case PropertyInt.HeritageSpecificArmor: return Enum.GetName(typeof(HeritageGroup), value);
                case PropertyInt.HookType: return Enum.GetName(typeof(HookType), value);
                case PropertyInt.HouseType: return Enum.GetName(typeof(HouseType), value);
                case PropertyInt.ImbuedEffect:
                case PropertyInt.ImbuedEffect2:
                case PropertyInt.ImbuedEffect3:
                case PropertyInt.ImbuedEffect4:
                case PropertyInt.ImbuedEffect5: return Enum.GetName(typeof(ImbuedEffectType), value);
                case PropertyInt.HookItemType:
                case PropertyInt.ItemType:
                case PropertyInt.MerchandiseItemTypes:
                case PropertyInt.TargetType: return Enum.GetName(typeof(ItemType), value);
                case PropertyInt.ItemXpStyle: return Enum.GetName(typeof(ItemXpStyle), value);
                case PropertyInt.MaterialType: return Enum.GetName(typeof(MaterialType), value);
                case PropertyInt.PaletteTemplate: return Enum.GetName(typeof(PaletteTemplate), value);
                case PropertyInt.PhysicsState: return Enum.GetName(typeof(PhysicsState), value);
                case PropertyInt.HookPlacement:
                case PropertyInt.Placement:
                case PropertyInt.PCAPRecordedPlacement: return Enum.GetName(typeof(Placement), value);
                case PropertyInt.PortalBitmask: return Enum.GetName(typeof(PortalBitmask), value);
                case PropertyInt.PlayerKillerStatus: return Enum.GetName(typeof(PlayerKillerStatus), value);
                case PropertyInt.BoosterEnum: return Enum.GetName(typeof(PropertyAttribute2nd), value);
                case PropertyInt.ShowableOnRadar: return Enum.GetName(typeof(RadarBehavior), value);
                case PropertyInt.RadarBlipColor: return Enum.GetName(typeof(RadarColor), value);
                case PropertyInt.WeaponSkill:
                case PropertyInt.WieldSkillType:
                case PropertyInt.WieldSkillType2:
                case PropertyInt.WieldSkillType3:
                case PropertyInt.WieldSkillType4:
                case PropertyInt.AppraisalItemSkill: return Enum.GetName(typeof(Skill), value);
                case PropertyInt.AccountRequirements: return Enum.GetName(typeof(SubscriptionStatus), value);
                case PropertyInt.SummoningMastery: return Enum.GetName(typeof(SummoningMastery), value);
                case PropertyInt.UiEffects: return Enum.GetName(typeof(UIEffects), value);
                case PropertyInt.ItemUseable: return Enum.GetName(typeof(Usable), value);
                case PropertyInt.WeaponType: return Enum.GetName(typeof(WeaponType), value);
                case PropertyInt.WieldRequirements:
                case PropertyInt.WieldRequirements2:
                case PropertyInt.WieldRequirements3:
                case PropertyInt.WieldRequirements4: return Enum.GetName(typeof(WieldRequirement), value);
                case PropertyInt.GeneratorStartTime:
                case PropertyInt.GeneratorEndTime: return DateTimeOffset.FromUnixTimeSeconds(value).DateTime.ToString(CultureInfo.InvariantCulture);
                case PropertyInt.ArmorType: return Enum.GetName(typeof(ArmorType), value);
                case PropertyInt.ParentLocation: return Enum.GetName(typeof(ParentLocation), value);
                case PropertyInt.PlacementPosition: return Enum.GetName(typeof(Placement), value);
                case PropertyInt.HouseStatus: return Enum.GetName(typeof(HouseStatus), value);
                case PropertyInt.UseCreatesContractId: return Enum.GetName(typeof(ContractId), value);
                case PropertyInt.Faction1Bits:
                case PropertyInt.Faction2Bits:
                case PropertyInt.Faction3Bits:
                case PropertyInt.Hatred1Bits:
                case PropertyInt.Hatred2Bits:
                case PropertyInt.Hatred3Bits: return Enum.GetName(typeof(FactionBits), value);
                case PropertyInt.UseRequiresSkill:
                case PropertyInt.UseRequiresSkillSpec:
                case PropertyInt.SkillToBeAltered: return Enum.GetName(typeof(Skill), value);
                case PropertyInt.HookGroup: return Enum.GetName(typeof(HookGroupType), value);
                //case PropertyInt.TypeOfAlteration: return Enum.GetName(typeof(SkillAlterationType), value);
                default: return null;
            }
        }
    }

    public enum PropertyInt64 : ushort
    {
        Undef = 0,
        [SendOnLogin] TotalExperience = 1,
        [SendOnLogin] AvailableExperience = 2,
        AugmentationCost = 3,
        ItemTotalXp = 4,
        ItemBaseXp = 5,
        [SendOnLogin] AvailableLuminance = 6,
        [SendOnLogin] MaximumLuminance = 7,
        InteractionReqs = 8,
        // custom
        [ServerOnly] AllegianceXPCached = 9000,
        [ServerOnly] AllegianceXPGenerated = 9001,
        [ServerOnly] AllegianceXPReceived = 9002,
        [ServerOnly] VerifyXp = 9003
    }

    public enum PropertyString : ushort
    {
        Undef = 0,
        [SendOnLogin] Name = 1,
        /// <summary>
        /// default "Adventurer"
        /// </summary>
        Title = 2,
        Sex = 3,
        HeritageGroup = 4,
        Template = 5,
        AttackersName = 6,
        Inscription = 7,
        [Description("Scribe Name")] ScribeName = 8,
        VendorsName = 9,
        Fellowship = 10,
        MonarchsName = 11,
        [ServerOnly] LockCode = 12,
        [ServerOnly] KeyCode = 13,
        Use = 14,
        ShortDesc = 15,
        LongDesc = 16,
        ActivationTalk = 17,
        [ServerOnly] UseMessage = 18,
        ItemHeritageGroupRestriction = 19,
        PluralName = 20,
        MonarchsTitle = 21,
        ActivationFailure = 22,
        ScribeAccount = 23,
        TownName = 24,
        CraftsmanName = 25,
        UsePkServerError = 26,
        ScoreCachedText = 27,
        ScoreDefaultEntryFormat = 28,
        ScoreFirstEntryFormat = 29,
        ScoreLastEntryFormat = 30,
        ScoreOnlyEntryFormat = 31,
        ScoreNoEntry = 32,
        [ServerOnly] Quest = 33,
        GeneratorEvent = 34,
        PatronsTitle = 35,
        HouseOwnerName = 36,
        QuestRestriction = 37,
        AppraisalPortalDestination = 38,
        TinkerName = 39,
        ImbuerName = 40,
        HouseOwnerAccount = 41,
        DisplayName = 42,
        DateOfBirth = 43,
        ThirdPartyApi = 44,
        KillQuest = 45,
        [Ephemeral] Afk = 46,
        AllegianceName = 47,
        AugmentationAddQuest = 48,
        KillQuest2 = 49,
        KillQuest3 = 50,
        UseSendsSignal = 51,
        [Description("Gear Plating Name")] GearPlatingName = 52,
        [ServerOnly] PCAPRecordedCurrentMotionState = 8006,
        [ServerOnly] PCAPRecordedServerName = 8031,
        [ServerOnly] PCAPRecordedCharacterName = 8032,
        // custom
        [ServerOnly] AllegianceMotd = 9001,
        [ServerOnly] AllegianceMotdSetBy = 9002,
        [ServerOnly] AllegianceSpeakerTitle = 9003,
        [ServerOnly] AllegianceSeneschalTitle = 9004,
        [ServerOnly] AllegianceCastellanTitle = 9005,
        [ServerOnly] GodState = 9006,
        [ServerOnly] TinkerLog = 9007,
    }

    public enum PropertyType
    {
        PropertyAttribute,
        PropertyAttribute2nd,
        PropertyBook,
        PropertyBool,
        PropertyDataId,
        PropertyDouble,
        PropertyInstanceId,
        PropertyInt,
        PropertyInt64,
        PropertyString,
        PropertyPosition
    }

    /// <summary>
    /// These are properties that are sent in the Player Description Event
    /// </summary>
    public class SendOnLoginAttribute : Attribute { }

    /// <summary>
    /// Static selection of client enums that are [SendOnLogin]<para />
    /// These are properties that are sent in the Player Description Event
    /// </summary>
    public class SendOnLoginProperties : GenericPropertiesId<SendOnLoginAttribute> { }

    /// <summary>
    /// These are properties that are never sent to any client.
    /// </summary>
    public class ServerOnlyAttribute : Attribute { }

    /// <summary>
    /// Static selection of client enums that are [ServerOnly]<para />
    /// These are properties that are never sent to any client.
    /// </summary>
    public class ServerOnlyProperties : GenericPropertiesId<ServerOnlyAttribute> { }
}
