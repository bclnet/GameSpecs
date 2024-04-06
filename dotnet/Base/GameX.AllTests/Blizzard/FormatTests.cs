using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX.Blizzard
{
    [TestClass]
    public class FormatsTests
    {
        static readonly Family family = FamilyManager.GetFamily("Blizzard");

        [DataTestMethod]
        [DataRow(true, "game:/code_post_gfx.ff#COD4")]          // 2007 - Call of Duty 4: Modern Warfare
        [DataRow(true, "game:/code_post_gfx.ff#WaW")]           // 2008 - Call of Duty: World at War
        [DataRow(true, "game:/code_post_gfx.ff#MW2")]           // 2009 - Call of Duty: Modern Warfare 2
        [DataRow(true, "game:/code_post_gfx.ff#BO")]            // 2010 - Call of Duty: Black Ops
        [DataRow(true, "game:/code_post_gfx.ff#MW3")]           // 2011 - Call of Duty: Modern Warfare 3
        [DataRow(true, "game:/code_post_gfx.ff#BO2")]           // 2012 - Call of Duty: Black Ops II
        [DataRow(true, "game:/code_post_gfx.ff#Ghosts")]        // 2013 - Call of Duty: Ghosts
        [DataRow(true, "game:/code_post_gfx.ff#AW")]            // 2014 - Call of Duty: Advanced Warfare
        [DataRow(true, "game:/core_post_gfx.ff#BO3")]           // 2015 - Call of Duty: Black Ops III
        [DataRow(true, "game:/code_post_gfx.ff#Blizzard")]            // 2016 - Call of Duty: Infinite Warfare
        [DataRow(true, "game:/code_post_gfx.ff#WWII")]          // 2017 - Call of Duty: WWII
        [DataRow(false, "game:/code_post_gfx.ff#BO4")]          // 2018 - Call of Duty: Black Ops 4
        [DataRow(true, "game:/code_post_gfx.ff#MW")]            // 2019 - Call of Duty: Modern Warfare
        [DataRow(false, "game:/code_post_gfx.ff#BOCW")]         // 2020 - Call of Duty: Black Ops Cold War
        [DataRow(false, "game:/code_post_gfx.ff#Vanguard")]     // 2021 - Call of Duty: Vanguard
        [DataRow(true, "game:/code_post_gfx.ff#COD:MW2")]       // 2022 - Call of Duty: Modern Warfare II
        public void DLG(bool installed, string sampleFile)
        {
            if (!installed) return;
            var dat = family.OpenPakFile(new Uri(sampleFile));
        }

        [DataTestMethod]
        [DataRow(true, "game:/mp_backlot_load.ff#COD4")]        // 2007 - Call of Duty 4: Modern Warfare
        [DataRow(true, "game:/mp_makin_day_load.ff#WaW")]       // 2008 - Call of Duty: World at War
        [DataRow(true, "game:/mp_underpass_load.ff#MW2")]       // 2009 - Call of Duty: Modern Warfare 2
        [DataRow(true, "game:/ui_mp.ff#BO")]                    // 2010 - Call of Duty: Black Ops
        [DataRow(true, "game:/mp_alpha_load.ff#MW3")]           // 2011 - Call of Duty: Modern Warfare 3
        [DataRow(true, "game:/yemen_gump_outro.ff#BO2")]        // 2012 - Call of Duty: Black Ops II
        [DataRow(true, "game:/ui_install.ff#Ghosts")]           // 2013 - Call of Duty: Ghosts
        [DataRow(true, "game:/patch_sanfran.ff#AW")]            // 2014 - Call of Duty: Advanced Warfare
        [DataRow(true, "game:/core_post_gfx.ff#BO3")]           // 2015 - Call of Duty: Black Ops III
        [DataRow(true, "game:/code_pre_gfx.ff#Blizzard")]             // 2016 - Call of Duty: Infinite Warfare
        [DataRow(true, "game:/code_post_gfx.ff#WWII")]          // 2017 - Call of Duty: WWII
        //[DataRow(true, "game:/karma_gump_checkin.ff#BO4")]    // 2018 - Call of Duty: Black Ops 4
        //[DataRow(true, "game:/karma_gump_checkin.ff#MW")]     // 2019 - Call of Duty: Modern Warfare
        //[DataRow(false, "game:/karma_gump_checkin.ff#BOCW")]   // 2020 - Call of Duty: Black Ops Cold War
        //[DataRow(false, "game:/karma_gump_checkin.ff#Vanguard")]   // 2021 - Call of Duty: Vanguard
        //[DataRow(true, "game:/karma_gump_checkin.ff#COD:MW2")]    // 2022 - Call of Duty: Modern Warfare II
        public void DLG2(bool installed, string sampleFile)
        {
            if (!installed) return;
            var dat = family.OpenPakFile(new Uri(sampleFile));
        }
    }
}

// https://github.com/mauserzjeh/iwi
// https://github.com/mauserzjeh/iwi2dds
// https://github.com/mauserzjeh/iwi2dds/blob/master/main.go
// https://github.com/mauserzjeh/cod-asset-importer
// https://wiki.zeroy.com/index.php?title=Call_of_Duty_4:_d3dbsp
// https://gist.github.com/Scobalula/a0fd08197497336f67b7ff551b2db404
// https://wiki.zeroy.com/images/b/b4/Unpack.png