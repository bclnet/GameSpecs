using System;
using System.Collections.Generic;

namespace GameEstate
{
    public static class TestHelper
    {
        static readonly Estate estateAC = EstateManager.GetEstate("AC");
        static readonly Estate estateArkane = EstateManager.GetEstate("Arkane");
        static readonly Estate estateAurora = EstateManager.GetEstate("Aurora");
        static readonly Estate estateCry = EstateManager.GetEstate("Cry");
        static readonly Estate estateCyanide = EstateManager.GetEstate("Cyanide");
        static readonly Estate estateOrigin = EstateManager.GetEstate("Origin");
        static readonly Estate estateRed = EstateManager.GetEstate("Red");
        static readonly Estate estateRsi = EstateManager.GetEstate("Rsi");
        static readonly Estate estateTes = EstateManager.GetEstate("Tes");
        static readonly Estate estateValve = EstateManager.GetEstate("Valve");

        public static readonly Dictionary<string, Lazy<EstatePakFile>> Paks = new()
        {
            { "AC:AC", new Lazy<EstatePakFile>(() => estateAC.OpenPakFile(new Uri("game:/*.dat#AC"))) },
            { "AC:AC:1", new Lazy<EstatePakFile>(() => estateAC.OpenPakFile(new Uri("game:/client_highres.dat#AC"))) },
            { "Arkane:Dishonored2", new Lazy<EstatePakFile>(() => estateArkane.OpenPakFile(new Uri("game:/*.index#Dishonored2"))) },
            { "Arkane:Dishonored2:1", new Lazy<EstatePakFile>(() => estateArkane.OpenPakFile(new Uri("game:/game1.index#Dishonored2"))) },
            { "Cry:MWO", new Lazy<EstatePakFile>(() => estateCry.OpenPakFile(new Uri("game:/*.pak#MWO"))) },
            { "Cry:MWO:1", new Lazy<EstatePakFile>(() => estateCry.OpenPakFile(new Uri("game:/GameData.pak#MWO"))) },
            { "Cyanide:TheCouncil", new Lazy<EstatePakFile>(() => estateCyanide.OpenPakFile(new Uri("game:/*.cpk#TheCouncil"))) },
            { "Cyanide:TheCouncil:1", new Lazy<EstatePakFile>(() => estateCyanide.OpenPakFile(new Uri("game:/Engine_Main_0.cpk#TheCouncil"))) },
            { "Origin:UltimaOnline", new Lazy<EstatePakFile>(() => estateOrigin.OpenPakFile(new Uri("game:/*.idx#UltimaOnline"))) },
            { "Origin:UltimaOnline:1", new Lazy<EstatePakFile>(() => estateOrigin.OpenPakFile(new Uri("game:/anim.idx#UltimaOnline"))) },
            { "Origin:UltimaIX", new Lazy<EstatePakFile>(() => estateOrigin.OpenPakFile(new Uri("game:/static/*.flx#UltimaIX"))) },
            { "Origin:UltimaIX:1", new Lazy<EstatePakFile>(() => estateOrigin.OpenPakFile(new Uri("game:/static/activity.flx#UltimaIX"))) },
            { "Red:Witcher", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/*.key#Witcher"))) },
            { "Red:Witcher:1", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/main.key#Witcher"))) },
            { "Red:Witcher2", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/*#Witcher2"))) },
            { "Red:Witcher2:1", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/base_scripts.dzip#Witcher2"))) },
            { "Red:Witcher2:2", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/krbr.dzip#Witcher2"))) },
            { "Red:Witcher3", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/content0/*#Witcher3"))) },
            { "Red:Witcher3:1", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/content0/bundles/xml.bundle#Witcher3"))) },
            { "Red:Witcher3:2", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/content0/collision.cache#Witcher3"))) },
            { "Red:Witcher3:3", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/content0/dep.cache#Witcher3"))) },
            { "Red:Witcher3:4", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/content0/texture.cache#Witcher3"))) },
            { "Red:CP77", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/*.archive#CP77"))) },
            { "Red:CP77:1", new Lazy<EstatePakFile>(() => estateRed.OpenPakFile(new Uri("game:/basegame_2_mainmenu.archive#CP77"))) },
            { "Rsi:StarCitizen", new Lazy<EstatePakFile>(() => estateRsi.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"))) },
            { "Tes:Morrowind", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Morrowind.bsa#Morrowind"))) },
            { "Tes:Oblivion", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Oblivion*.bsa#Oblivion"))) },
            { "Tes:Oblivion:1", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Oblivion - Meshes.bsa#Oblivion"))) },
            { "Tes:Oblivion:2", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Oblivion - Textures - Compressed.bsa#Oblivion"))) },
            { "Tes:Skyrim", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Skyrim*.bsa#Skyrim"))) },
            { "Tes:Skyrim:1", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Skyrim - Meshes0.bsa#Skyrim"))) },
            { "Tes:Skyrim:2", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Skyrim - Textures.bsa#Skyrim"))) },
            { "Tes:SkyrimSE", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Skyrim*.bsa#SkyrimSE"))) },
            { "Tes:SkyrimSE:1", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Skyrim - Meshes0.bsa#SkyrimSE"))) },
            { "Tes:SkyrimSE:2", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Skyrim - Textures0.bsa#SkyrimSE"))) },
            { "Tes:Fallout2", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/*.dat#Fallout2"))) },
            { "Tes:Fallout3", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Fallout*.bsa#Fallout3"))) },
            { "Tes:FalloutNV", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Fallout*.bsa#FalloutNV"))) },
            { "Tes:Fallout4", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Fallout4*.ba2#Fallout4"))) },
            { "Tes:Fallout4:1", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Fallout4 - Meshes.ba2#Fallout4"))) },
            { "Tes:Fallout4VR", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Fallout4*.ba2#FalloutVR"))) },
            { "Tes:Fallout4VR:1", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Fallout4 - Startup.ba2#Fallout4VR"))) },
            { "Tes:Fallout4VR:2", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/Fallout4 - Textures8.ba2#Fallout4VR"))) },
            { "Tes:Fallout76", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/SeventySix*.ba2#Fallout76"))) },
            { "Tes:Fallout76:1", new Lazy<EstatePakFile>(() => estateTes.OpenPakFile(new Uri("game:/SeventySix - 00UpdateMain.ba2#Fallout76"))) },
            { "Valve:Dota2", new Lazy<EstatePakFile>(() => estateValve.OpenPakFile(new Uri("game:/(core:dota)/*_dir.vpk#Dota2"))) },
            { "Valve:Dota2:1", new Lazy<EstatePakFile>(() => estateValve.OpenPakFile(new Uri("game:/dota/pak01_dir.vpk#Dota2"))) },
        };
    }
}
