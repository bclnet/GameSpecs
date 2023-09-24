namespace GameSpec.Unreal.Formats.Core
{
    // Unreal engine 4 versions, declared as enum to be able to see all revisions in a single place
    public enum VERUE4
    {
        // Pre-release UE4 file versions
        ASSET_REGISTRY_TAGS = 112,
        TEXTURE_DERIVED_DATA2 = 124,
        ADD_COOKED_TO_TEXTURE2D = 125,
        REMOVED_STRIP_DATA = 130,
        REMOVE_EXTRA_SKELMESH_VERTEX_INFLUENCES = 134,
        TEXTURE_SOURCE_ART_REFACTOR = 143,
        ADD_SKELMESH_MESHTOIMPORTVERTEXMAP = 152,
        REMOVE_ARCHETYPE_INDEX_FROM_LINKER_TABLES = 163,
        REMOVE_NET_INDEX = 196,
        BULKDATA_AT_LARGE_OFFSETS = 198,
        SUMMARY_HAS_BULKDATA_OFFSET = 212,
        STATIC_MESH_STORE_NAV_COLLISION = 216,
        DEPRECATED_STATIC_MESH_THUMBNAIL_PROPERTIES_REMOVED = 242,
        APEX_CLOTH = 254,
        STATIC_SKELETAL_MESH_SERIALIZATION_FIX = 269,
        SUPPORT_32BIT_STATIC_MESH_INDICES = 277,
        APEX_CLOTH_LOD = 280,
        ARRAY_PROPERTY_INNER_TAGS = 282, // note: here's a typo in UE4 code - "VAR_" instead of "VER_"
        KEEP_SKEL_MESH_INDEX_DATA = 283,
        MOVE_SKELETALMESH_SHADOWCASTING = 302,
        REFERENCE_SKELETON_REFACTOR = 310,
        FIXUP_ROOTBONE_PARENT = 312,
        FIX_ANIMATIONBASEPOSE_SERIALIZATION = 331,
        SUPPORT_8_BONE_INFLUENCES_SKELETAL_MESHES = 332,
        SUPPORT_GPUSKINNING_8_BONE_INFLUENCES = 334,
        ANIM_SUPPORT_NONUNIFORM_SCALE_ANIMATION = 335,
        ENGINE_VERSION_OBJECT = 336,
        SKELETON_GUID_SERIALIZATION = 338,
        // UE4.0 source code was released on GitHub. Note: if we don't have any VERUE4...
        // values between two VERUE4.xx constants, for instance, between VERUE4.X0 and VERUE4.X1,
        // it doesn't matter for this framework which version will be serialized serialized -
        // 4.0 or 4.1, because 4.1 has nothing new for supported object formats compared to 4.0.
        X0 = 342,
        MORPHTARGET_CPU_TANGENTZDELTA_FORMATCHANGE = 348,
        X1 = 352,
        X2 = 363,
        LOAD_FOR_EDITOR_GAME = 365,
        FTEXT_HISTORY = 368,                    // used for UStaticMesh versioning
        STORE_BONE_EXPORT_NAMES = 370,
        X3 = 382,
        ADD_STRING_ASSET_REFERENCES_MAP = 384,
        X4 = 385,
        SKELETON_ADD_SMARTNAMES = 388,
        SOUND_COMPRESSION_TYPE_ADDED = 392,
        RENAME_CROUCHMOVESCHARACTERDOWN = 394,  // used for UStaticMesh versioning
        DEPRECATE_UMG_STYLE_ASSETS = 397,       // used for UStaticMesh versioning
        X5 = 401,
        X6 = 413,
        RENAME_WIDGET_VISIBILITY = 416,         // used for UStaticMesh versioning
        ANIMATION_ADD_TRACKCURVES = 417,
        X7 = 434,
        STRUCT_GUID_IN_PROPERTY_TAG = 441,
        PACKAGE_SUMMARY_HAS_COMPATIBLE_ENGINE_VERSION = 444,
        X8 = 451,
        SERIALIZE_TEXT_IN_PACKAGES = 459,
        X9 = 482,
        X10 = 9,                             // exactly the same file version for 4.9 and 4.10
        COOKED_ASSETS_IN_EDITOR_SUPPORT = 485,
        SOUND_CONCURRENCY_PACKAGE = 489,        // used for UStaticMesh versioning
        X11 = 498,
        INNER_ARRAY_TAG_INFO = 500,
        PROPERTY_GUID_IN_PROPERTY_TAG = 503,
        NAME_HASHES_SERIALIZED = 504,
        X12 = 504,
        X13 = 505,
        PRELOAD_DEPENDENCIES_IN_COOKED_EXPORTS = 507,
        TemplateIndex_IN_COOKED_EXPORTS = 508,
        X14 = 508,
        PROPERTY_TAG_SET_MAP_SUPPORT = 509,
        ADDED_SEARCHABLE_NAMES = 510,
        X15 = 510,
        X64BIT_EXPORTMAP_SERIALSIZES = 511,
        X16 = 513,
        X17 = 513,
        ADDED_SOFT_OBJECT_PATH = 514,
        X18 = 514,
        ADDED_PACKAGE_SUMMARY_LOCALIZATION_ID = 516,
        X19 = 516,
        X20 = 516,
        X21 = 517,
        X22 = 517,
        X23 = 517,
        ADDED_PACKAGE_OWNER = 518,
        X24 = 518,
        X25 = 518,
        SKINWEIGHT_PROFILE_DATA_LAYOUT_CHANGES = 519,
        NON_OUTER_PACKAGE_IMPORT = 520,
        X26 = 522,
        X27 = 522,
        // look for NEW_ENGINE_VERSION over the code to find places where version constants should be inserted.
        // LATEST_SUPPORTED_UE4_VERSION should be updated too.
    }
}