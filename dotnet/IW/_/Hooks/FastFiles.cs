using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.IW.Formats
{
    public unsafe class FastFiles
    {
        public struct XZoneInfo
        {
            public XZoneInfo(string name, int allocFlags, int freeFlags)
            {
                this.name = name;
                this.allocFlags = allocFlags;
                this.freeFlags = freeFlags;
            }
            public string name;
            public int allocFlags;
            public int freeFlags;
        }

        static object CurrentKey;
        static object CurrentCTR;
        static List<string> ZonePaths = new List<string>();

        static bool IsIW4xZone;
        static bool StreamRead;
        static byte LastByteRead;
        static uint CurrentZone;
        static uint MaxZones;

        // This has to be called only once, when the game starts
        static void LoadInitialZones(List<XZoneInfo> zoneInfo, int sync)
        {
            var data = new List<XZoneInfo>(zoneInfo);
            if (Exists("patch_mp"))
                data.Add(new XZoneInfo("patch_mp", 1, 0));
            if (File.Exists($"{Config.fs_game}/mod.ff"))
                data.Add(new XZoneInfo("mod", 1, 0));

            LoadDLCUIZones(data, sync);
        }

        // This has to be called every time the cgame is reinitialized
        static void LoadDLCUIZones(List<XZoneInfo> zoneInfo, int sync)
        {
            var data = new List<XZoneInfo>(zoneInfo);
            var info = new XZoneInfo(null, 2, 0);

            // Custom ui stuff
            if (Exists("ui_mp"))
            {
                info.name = "ui_mp";
                data.Add(info);
            }
            else // Fallback
            {
                info.name = "dlc1_ui_mp";
                data.Add(info);
                info.name = "dlc2_ui_mp";
                data.Add(info);
            }

            LoadLocalizeZones(data, sync);
        }

        static void LoadGfxZones(List<XZoneInfo> zoneInfo, int sync)
        {
            var data = new List<XZoneInfo>(zoneInfo);
            if (Exists("code_post_gfx_mp"))
                data.Add(new XZoneInfo("code_post_gfx_mp", zoneInfo[0].allocFlags, zoneInfo[0].freeFlags));

            Game.DB_LoadXAssets(data, sync);
        }

        // This has to be called every time fastfiles are loaded :D
        static void LoadLocalizeZones(List<XZoneInfo> zoneInfo, int sync)
        {
            var data = new List<XZoneInfo>(zoneInfo);
            var info = new XZoneInfo(null, 4, 0);

            // Not sure how they should be loaded :S
            var langZone = $"localized_{Config.Language}";

            if (Exists(langZone))
                info.name = langZone;
            else if (Exists("localized_english")) // Fallback
                info.name = "localized_english";

            data.Add(info);

            Game.DB_LoadXAssets(data, sync);
        }

        // Name is a bit weird, due to FasFileS and ExistS :P
        static bool Exists(string file)
        {
            var path = GetZoneLocation(file);
            path += file;

            if (!path.EndsWith(".ff"))
                path += ".ff";

            return File.Exists(path);
        }

        static bool Ready()
        {
            return true; //(Game::Sys_IsDatabaseReady() && Game::Sys_IsDatabaseReady2());
        }

        static string GetZoneLocation(string file)
        {
            var dir = Config.fs_basepath;

            var paths = new List<string>(ZonePaths);
            var modDir = Config.fs_game;
            if ((file == "mod" || file == "mod.ff") && !string.IsNullOrEmpty(modDir))
                paths.Add($"modDir\\");

            if (file.StartsWith("mp_"))
            {
                var zone = file;
                if (zone.EndsWith(".ff"))
                    zone = zone.Replace(".ff", "");

                var filename = zone;
                if (zone.EndsWith("_load"))
                    zone = zone.Replace("_load", "");

                if (File.Exists($"usermaps\\{zone}\\{filename}.ff"))
                    return $"usermaps\\{zone}\\";
            }

            foreach (var path in paths)
            {
                var absoluteFile = $"{dir}\\{path}{file}";

                // No ".ff" appended, append it manually
                if (!absoluteFile.EndsWith(".ff"))
                    absoluteFile += ".ff";

                // Check if FastFile exists
                if (File.Exists(absoluteFile))
                    return path;
            }

            return $"zone\\{Config.Language}\\";
        }

        static void AddZonePath(string path)
            => ZonePaths.Add(path);

        static IntPtr Current()
        {
            var file = Hook.Get<IntPtr>(0x112A680) + 4;
            return file == new IntPtr(4)
                ? default //""
                : file;
        }

        static void ReadXFileHeader(byte[] buffer, int size)
        {
            if (IsIW4xZone)
            {
                char pad;
                Game.DB_ReadXFile(&pad, 1);
            }

            Game.DB_ReadXFile(buffer, size);
        }

        static void ReadVersionStub(ref uint version, int size)
        {
            CurrentZone++;
            Game.DB_ReadXFileUncompressed(version, size);

            Zones.SetVersion(version);

            // Allow loading of codo versions
            if ((version >= VERSION_ALPHA2 && version <= 360) || (version >= 423 && version <= 461))
                version = XFILE_VERSION;

            if (version != XFILE_VERSION)
                Console.WriteLine($"Zone version {Zones.Version()} is not supported!");
        }

        static void ReadHeaderStub(uint header, int size)
        {
            IsIW4xZone = false;
            LastByteRead = 0;
            Game.DB_ReadXFileUncompressed(header, size);

            if (header[0] == XFILE_HEADER_IW4X)
            {
                IsIW4xZone = true;

                if (header[1] < XFILE_VERSION_IW4X)
                    Console.WriteLine($"The fastfile you are trying to load is outdated ({header[1]}, expected {XFILE_VERSION_IW4X})");
                else if (header[1] > XFILE_VERSION_IW4X)
                    Console.WriteLine($"You are loading a fastfile that is too new ({header[1]}, expected {XFILE_VERSION_IW4X}), update your game or rebuild the fastfile");

                reinterpret_cast<ulong[]>(header) = XFILE_MAGIC_UNSIGNED;
            }
        }

        //static void AuthLoadInitCrypto()
        //{
        //    if (Zones::Version() >= 319)
        //    {
        //        register_hash(&sha256_desc);
        //        register_cipher(&aes_desc);

        //        rsa_key key;
        //        unsigned char encKey[256];
        //        int hash = find_hash("sha256"), aes = find_cipher("aes"), stat;

        //        Game::DB_ReadXFileUncompressed(encKey, 256);

        //        unsigned long outLen = sizeof(CurrentKey);
        //        rsa_import(ZoneKey, sizeof(ZoneKey), &key);
        //        rsa_decrypt_key_ex(encKey, 256, CurrentKey.data, &outLen, null, NULL, hash, Zones::Version() >= 359 ? 1 : 2, &stat, &key);
        //        rsa_free(&key);

        //        ctr_start(aes, CurrentKey.iv, CurrentKey.key, sizeof(CurrentKey.key), 0, 0, &CurrentCTR);
        //    }

        //    Hook.Call < void() > (0x46FAE0)();
        //}

        //static int AuthLoadInflateCompare(unsigned char* buffer, int length, unsigned char* ivValue)
        //{
        //    if (Zones::Version() >= 319)
        //    {
        //        ctr_setiv(ivValue, 16, &CurrentCTR);
        //        ctr_decrypt(buffer, buffer, length, &CurrentCTR);
        //    }

        //    return Hook.Call < int(unsigned char *, int, unsigned char *) > (0x5BA240)(buffer, length, ivValue);
        //}

        //static int InflateInitDecrypt(z_streamp strm, byte[] version, int stream_size)
        //{
        //    if (Zones::Version() >= 319)
        //    {
        //        ctr_decrypt(strm->next_in, const_cast < unsigned char *> (strm->next_in), strm->avail_in, &CurrentCTR);
        //    }

        //    return Hook.Call < int(z_streamp, const char*, int)> (0x4D8090)(strm, version, stream_size);
        //    //return inflateInit_(strm, version, stream_size);
        //}

        //static void AuthLoadInflateDecryptBaseFunc(byte[] buffer)
        //{
        //    if (Zones::Version() >= 319)
        //    {
        //        ctr_setiv(CurrentKey.iv, sizeof(CurrentKey.iv), &CurrentCTR);
        //        ctr_decrypt(buffer, buffer, 8192, &CurrentCTR);
        //    }
        //}

        //static void AuthLoadInflateDecryptBase()
        //{
        //    //__asm
        //    //    {
        //    //    pushad
        //    //    push ebx
        //    //    call AuthLoadInflateDecryptBaseFunc
        //    //    pop ebx
        //    //    popad
        //    //        push 5B96F0h
        //    //        retn
        //    //    }
        //}

        static float GetFullLoadedFraction()
        {
            var singleProgress = 1f / MaxZones;
            var partialProgress = singleProgress * (CurrentZone - 1);
            var currentProgress = Math.Max(Math.Min(Game.DB_GetLoadedFraction(), 1f), 0f);
            return Math.Min(partialProgress + (currentProgress * singleProgress), 1f);
        }

        static void LoadZonesStub(XZoneInfo zoneInfo, uint zoneCount)
        {
            //    CurrentZone = 0;
            //    MaxZones = zoneCount;

            //    Hook.Call < void(Game::XZoneInfo *, unsigned int) > (0x5BBAC0)(zoneInfo, zoneCount);
        }

        //static void ReadXFile(void* /*buffer*/, int /*size*/)
        //{
        //    __asm
        //{
        //        mov eax, [esp + 4]

        //    mov ecx, [esp + 8]


        //    push 445468h
        //    retn

        //}
        //}

        static void ReadXFileStub(char* buffer, int size)
        {
            //    ReadXFile(buffer, size);

            //    if (IsIW4xZone)
            //    {
            //        for (int i = 0; i < size; ++i)
            //        {
            //            buffer[i] ^= LastByteRead;
            //            Utils::RotLeft(buffer[i], 4);
            //            buffer[i] ^= -1;
            //            Utils::RotRight(buffer[i], 6);

            //            LastByteRead = buffer[i];
            //        }
            //    }
        }

#if DEBUG
        static void LogStreamRead(int len)
        {
            //    *Game::g_streamPos += len;

            //    if (StreamRead)
            //    {
            //        std::string data = Utils::String::VA("%d\n", len);
            //        if (*Game::g_streamPosIndex == 2) data = Utils::String::VA("(%d)\n", len);
            //        Utils::IO::WriteFile("userraw/logs/iw4_reads.log", data, true);
            //    }
        }
#endif

        static void Load_XSurfaceArray(int atStreamStart, int count)
        {
            //    // read the actual count from the varXModelSurfs ptr
            //    auto surface = *reinterpret_cast<Game::XModelSurfs**>(0x0112A95C);

            //    // call original read function with the correct count
            //    Hook.Call < void(int, int) > (0x004925B0)(atStreamStart, surface->numsurfs);
        }

        static FastFiles()
        {
            //Dvar::Register<bool>("ui_zoneDebug", false, Game::DVAR_ARCHIVE, "Display current loaded zone.");

            // Fix XSurface assets
            new Hook(0x0048E8A5, Load_XSurfaceArray, HOOK_CALL).install()->quick();

            // Redirect zone paths
            new Hook(0x44DA90, GetZoneLocation, HOOK_JUMP).install()->quick();

            // Allow loading 'newer' zones
            new Hook(0x4158E7, ReadVersionStub, HOOK_CALL).install()->quick();

            // Allow loading IW4x zones
            new Hook(0x4157B8, ReadHeaderStub, HOOK_CALL).install()->quick();

            // Obfuscate zone data
            new Hook(Game.DB_ReadXFile, ReadXFileStub, HOOK_JUMP).install()->quick();

            // Allow custom zone loading
            if (!ZoneBuilder::IsEnabled())
            {
                new Hook(0x506BC7, LoadInitialZones, HOOK_CALL).install()->quick();
                new Hook(0x60B4AC, LoadDLCUIZones, HOOK_CALL).install()->quick();
                new Hook(0x506B25, LoadGfxZones, HOOK_CALL).install()->quick();
            }

            // basic checks (hash jumps, both normal and playlist)
            Hook.Nop(0x5B97A3, 2);
            Hook.Nop(0x5BA493, 2);

            Hook.Nop(0x5B991C, 2);
            Hook.Nop(0x5BA60C, 2);

            Hook.Nop(0x5B97B4, 2);
            Hook.Nop(0x5BA4A4, 2);

            // allow loading of IWffu (unsigned) files
            Hook.Set<BYTE>(0x4158D9, 0xEB); // main function
            Hook.Nop(0x4A1D97, 2); // DB_AuthLoad_InflateInit

            // some other, unknown, check
            // this is replaced by hooks below
            Hook.Set<BYTE>(0x5B9912, 0xB8);
            Hook.Set<DWORD>(0x5B9913, 1);

            Hook.Set<BYTE>(0x5BA602, 0xB8);
            Hook.Set<DWORD>(0x5BA603, 1);

            // Initialize crypto
            new Hook(0x4D02F0, AuthLoadInitCrypto, HOOK_CALL).install()->quick();

            // Initial stage decryption
            new Hook(0x4D0306, InflateInitDecrypt, HOOK_CALL).install()->quick();

            // Hash bit decryption
            new Hook(0x5B9958, AuthLoadInflateCompare, HOOK_CALL).install()->quick();
            new Hook(0x5B9912, AuthLoadInflateCompare, HOOK_CALL).install()->quick();

            // General read
            new Hook(0x5B98E4, AuthLoadInflateDecryptBase, HOOK_CALL).install()->quick();

            // Fix fastfile progress
            new Hook(0x4E5DE3, LoadZonesStub, HOOK_CALL).install()->quick();
            new Hook(0x407761, GetFullLoadedFraction, HOOK_CALL).install()->quick();
            new Hook(0x49FA1E, GetFullLoadedFraction, HOOK_CALL).install()->quick();
            new Hook(0x589090, GetFullLoadedFraction, HOOK_CALL).install()->quick();
            new Hook(0x629FC0, GetFullLoadedFraction, HOOK_JUMP).install()->quick();

            // XFile header loading
            new Hook(0x4159E2, ReadXFileHeader, HOOK_CALL).install()->quick();

            // Add custom zone paths
            AddZonePath("zone\\patch\\");
            AddZonePath("zone\\dlc\\");

            //if (!Dedicated::IsEnabled() && !ZoneBuilder::IsEnabled())
            //{
            //    Scheduler::Loop([]

            //{
            //        if (Current().empty() || !Dvar::Var("ui_zoneDebug").get<bool>()) return;

            //        auto * const font = Game::R_RegisterFont("fonts/consoleFont", 0);
            //        float color[4] = { 1.0f, 1.0f, 1.0f, (Game::CL_IsCgameInitialized() ? 0.3f : 1.0f) };

            //        auto FFTotalSize = *reinterpret_cast<std::uint32_t*>(0x10AA5D8);
            //        auto FFCurrentOffset = *reinterpret_cast<std::uint32_t*>(0x10AA608);

            //        float fastfileLoadProgress = (float(FFCurrentOffset) / float(FFTotalSize)) * 100.0f;
            //        if (std::isinf(fastfileLoadProgress))
            //        {
            //            fastfileLoadProgress = 100.0f;
            //        }
            //        else if (std::isnan(fastfileLoadProgress))
            //        {
            //            fastfileLoadProgress = 0.0f;
            //        }

            //        Game::R_AddCmdDrawText(Utils::String::VA("Loading FastFile: %s [%0.1f%%]", Current().data(), fastfileLoadProgress), std::numeric_limits<int>::max(), font, 5.0f, static_cast<float>(Renderer::Height() - 5), 1.0f, 1.0f, 0.0f, color, Game::ITEM_TEXTSTYLE_NORMAL);
            //    }, Scheduler::Pipeline::RENDERER);
            //}

            //    Command::Add("loadzone", [](Command::Params * params)
            //{
            //        if (params->size() < 2) return;

            //        Game::XZoneInfo info;
            //        info.name = params->get(1);
            //        info.allocFlags = 1;//0x01000000;
            //        info.freeFlags = 0;

            //        Game::DB_LoadXAssets(&info, 1, true);
            //    });

            //    Command::Add("awaitDatabase", [](Command::Params *)

            //{
            //        Logger::Print("Waiting for database...\n");
            //        while (!Game::Sys_IsDatabaseReady()) std::this_thread::sleep_for(100ms);
            //    });

#if DEBUG
            // ZoneBuilder debugging
            //Utils::IO::WriteFile("userraw/logs/iw4_reads.log", "", false);
            //Hook(0x4A8FA0, LogStreamRead, HOOK_JUMP).install()->quick();
            //Hook(0x4BCB62, () =>
            //{
            //    StreamRead = true;
            //    Hook.Call < void(bool) > (0x4B8DB0)(true); // currently set to Load_GfxWorld
            //    StreamRead = false;
            //}, HOOK_CALL).install()/*->quick()*/;
#endif
        }
    }
}