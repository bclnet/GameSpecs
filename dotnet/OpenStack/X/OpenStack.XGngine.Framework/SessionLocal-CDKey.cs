using System.NumericsX.OpenStack.Gngine.Framework.Async;
using System.NumericsX.OpenStack.System;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.Gngine.Framework.Framework;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    unsafe partial class SessionLocal
    {
        static readonly string CDKEY_FILEPATH = $"../{Config.BASE_GAMEDIR}/{Config.CDKEY_FILE}";
        static readonly string XPKEY_FILEPATH = $"../{Config.BASE_GAMEDIR}/{Config.XPKEY_FILE}";

        // digits to letters table
        static readonly string CDKEY_DIGITS = "TWSBJCGD7PA23RLH";

        enum CDKEY
        {
            UNKNOWN,  // need to perform checks on the key
            INVALID,  // that key is wrong
            OK,       // valid
            CHECKING, // sent a check request ( gameAuth only )
            NA        // does not apply, xp key when xp is not present
        }

        const int CDKEY_BUF_LEN = 17;
        const int CDKEY_AUTH_TIMEOUT = 5000;

        string cdkey = string.Empty;
        CDKEY cdkey_state;
        string xpkey = string.Empty;
        CDKEY xpkey_state;
        int authEmitTimeout;
        bool authWaitBox;
        string authMsg;

        public override void ReadCDKey()
        {
            var buffer = stackalloc char[32];
            cdkey_state = CDKEY.UNKNOWN;

            var filename = CDKEY_FILEPATH;
            var f = fileSystem.OpenExplicitFileRead(fileSystem.RelativePathToOSPath(filename, "fs_configpath"));
            // try the install path, which is where the cd installer and steam put it
            if (f == null) f = fileSystem.OpenExplicitFileRead(fileSystem.RelativePathToOSPath(filename, "fs_basepath"));

            if (f == null) { common.Printf($"Couldn't read {filename}.\n"); cdkey = string.Empty; }
            else
            {
                Unsafe.InitBlock(buffer, 0, 32);
                f.Read((byte*)buffer, CDKEY_BUF_LEN - 1);
                fileSystem.CloseFile(f);
                cdkey = new string(buffer, 0, CDKEY_BUF_LEN);
            }

            xpkey_state = CDKEY.UNKNOWN;

            filename = XPKEY_FILEPATH;
            f = fileSystem.OpenExplicitFileRead(fileSystem.RelativePathToOSPath(filename, "fs_configpath"));
            // try the install path, which is where the cd installer and steam put it
            if (f == null) f = fileSystem.OpenExplicitFileRead(fileSystem.RelativePathToOSPath(filename, "fs_basepath"));

            if (f == null) { common.Printf($"Couldn't read {filename}.\n"); xpkey = string.Empty; }
            else
            {
                Unsafe.InitBlock(buffer, 0, 32);
                f.Read((byte*)buffer, CDKEY_BUF_LEN - 1);
                fileSystem.CloseFile(f);
                xpkey = new string(buffer, 0, CDKEY_BUF_LEN);
            }
        }

        public override void WriteCDKey()
        {
            var filename = CDKEY_FILEPATH;
            // OpenFileWrite advertises creating directories to the path if needed, but that won't work with a '..' in the path occasionally on windows, but mostly on Linux and OSX, the fs_configpath/base may not exist in full
            var OSPath = fileSystem.BuildOSPath(cvarSystem.GetCVarString("fs_configpath"), Config.BASE_GAMEDIR, Config.CDKEY_FILE);
            fileSystem.CreateOSPath(OSPath);
            var f = fileSystem.OpenFileWrite(filename, "fs_configpath");
            if (f == null) { common.Printf($"Couldn't write {filename}.\n"); return; }
            f.Printf($"{cdkey}{Config.CDKEY_TEXT}");
            fileSystem.CloseFile(f);

            filename = XPKEY_FILEPATH;
            f = fileSystem.OpenFileWrite(filename, "fs_configpath");
            if (f == null) { common.Printf($"Couldn't write {filename}.\n"); return; }
            f.Printf($"{xpkey}{Config.CDKEY_TEXT}");
            fileSystem.CloseFile(f);
        }

        public override string GetCDKey(bool xp)
            => !xp ? cdkey
            : xpkey_state == CDKEY.OK || xpkey_state == CDKEY.CHECKING ? xpkey
            : null;

        /// <summary>
        /// the function will only modify keys to _OK or _CHECKING if the offline checks are passed
        /// if the function returns false, the offline checks failed, and offline_valid holds which keys are bad
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="netConnect">if set to <c>true</c> [net connect].</param>
        /// <param name="offline_valid">The offline valid.</param>
        /// <returns></returns>
        public override bool CheckKey(string key, bool netConnect, bool[] offline_valid)
        {
            throw new NotImplementedException();
            //var lkey = new char[2][CDKEY_BUF_LEN];
            //var l_chk = new char[2][3];
            //var s_chk = stackalloc char[3];
            //int imax, i_key;
            //uint checksum, chk8;
            //var edited_key = stackalloc bool[2];

            //// make sure have a right input string
            //Debug.Assert(key.Length == (CDKEY_BUF_LEN - 1) * 2 + 4 + 3 + 4);

            //edited_key[0] = key[0] == '1';
            //idStr.Copynz(lkey[0], key + 2, CDKEY_BUF_LEN);
            //idStr.ToUpper(lkey[0]);
            //idStr.Copynz(l_chk[0], key + CDKEY_BUF_LEN + 2, 3);
            //idStr.ToUpper(l_chk[0]);
            //edited_key[1] = (key[CDKEY_BUF_LEN + 2 + 3] == '1');
            //idStr.Copynz(lkey[1], key + CDKEY_BUF_LEN + 7, CDKEY_BUF_LEN);
            //idStr.ToUpper(lkey[1]);
            //idStr.Copynz(l_chk[1], key + CDKEY_BUF_LEN * 2 + 7, 3);
            //idStr.ToUpper(l_chk[1]);

            //imax = fileSystem.HasD3XP ? 2 : 1;
            //offline_valid[0] = offline_valid[1] = true;
            //for (i_key = 0; i_key < imax; i_key++)
            //{
            //    // check that the characters are from the valid set
            //    int i;
            //    for (i = 0; i < CDKEY_BUF_LEN - 1; i++)
            //    {
            //        if (!strchr(CDKEY_DIGITS, lkey[i_key][i]))
            //        {
            //            offline_valid[i_key] = false;
            //            continue;
            //        }
            //    }

            //    if (edited_key[i_key])
            //    {
            //        // verify the checksum for edited keys only
            //        checksum = CRC32_BlockChecksum(lkey[i_key], CDKEY_BUF_LEN - 1);
            //        chk8 = (checksum & 0xff) ^ (((checksum & 0xff00) >> 8) ^ (((checksum & 0xff0000) >> 16) ^ ((checksum & 0xff000000) >> 24)));
            //        idStr.snPrintf(s_chk, 3, "%02X", chk8);
            //        if (idStr.Icmp(l_chk[i_key], s_chk) != 0)
            //        {
            //            offline_valid[i_key] = false;
            //            continue;
            //        }
            //    }
            //}

            //if (!offline_valid[0] || !offline_valid[1])
            //    return false;

            //// offline checks passed, we'll return true and optionally emit key check requests
            //// the function should only modify the key states if the offline checks passed successfully

            //// set the keys, don't send a game auth if we are net connecting
            //idStr.Copynz(cdkey, lkey[0], CDKEY_BUF_LEN);
            //netConnect? cdkey_state = CDKEY_OK : cdkey_state = CDKEY_CHECKING;
            //if (fileSystem.HasD3XP())
            //{
            //    idStr.Copynz(xpkey, lkey[1], CDKEY_BUF_LEN);
            //    netConnect? xpkey_state = CDKEY_OK : xpkey_state = CDKEY_CHECKING;
            //}
            //else xpkey_state = CDKEY.NA;
            //if (!netConnect) EmitGameAuth();
            //SetCDKeyGuiVars();

            //return true;
        }

        /// <summary>
        /// checking that the key is present and uses only valid characters if d3xp is installed, check for a valid xpkey as well
        /// emit an auth packet to the master if possible and needed
        /// </summary>
        /// <param name="strict">if set to <c>true</c> [strict].</param>
        /// <returns></returns>
        public override bool CDKeysAreValid(bool strict)
        {
            int i; var emitAuth = false;

            if (cdkey_state == CDKEY.UNKNOWN)
            {
                if (cdkey.Length != CDKEY_BUF_LEN - 1) cdkey_state = CDKEY.INVALID;
                else for (i = 0; i < CDKEY_BUF_LEN - 1; i++) if (!CDKEY_DIGITS.Contains(cdkey[i])) { cdkey_state = CDKEY.INVALID; break; }
                if (cdkey_state == CDKEY.UNKNOWN) { cdkey_state = CDKEY.CHECKING; emitAuth = true; }
            }
            if (xpkey_state == CDKEY.UNKNOWN)
            {
                if (fileSystem.HasD3XP)
                {
                    if (xpkey.Length != CDKEY_BUF_LEN - 1) xpkey_state = CDKEY.INVALID;
                    else for (i = 0; i < CDKEY_BUF_LEN - 1; i++) if (!CDKEY_DIGITS.Contains(xpkey[i])) xpkey_state = CDKEY.INVALID;
                    if (xpkey_state == CDKEY.UNKNOWN) { xpkey_state = CDKEY.CHECKING; emitAuth = true; }
                }
                else xpkey_state = CDKEY.NA;
            }
            if (emitAuth) EmitGameAuth();
            // make sure to keep the mainmenu gui up to date in case we made state changes
            SetCDKeyGuiVars();
            if (strict) return cdkey_state == CDKEY.OK && (xpkey_state == CDKEY.OK || xpkey_state == CDKEY.NA);
            else return (cdkey_state == CDKEY.OK || cdkey_state == CDKEY.CHECKING) && (xpkey_state == CDKEY.OK || xpkey_state == CDKEY.CHECKING || xpkey_state == CDKEY.NA);
        }

        public override void ClearCDKey(bool[] valid)
        {
            if (!valid[0]) { cdkey = ""; cdkey_state = CDKEY.UNKNOWN; }
            // if a key was in checking and not explicitely asked for clearing, put it back to ok
            else if (cdkey_state == CDKEY.CHECKING) cdkey_state = CDKEY.OK;

            if (!valid[1]) { xpkey = ""; xpkey_state = CDKEY.UNKNOWN; }
            else if (xpkey_state == CDKEY.CHECKING) xpkey_state = CDKEY.OK;
            WriteCDKey();
        }

        public override bool WaitingForGameAuth
            => authEmitTimeout != 0;

        public override void CDKeysAuthReply(bool valid, string auth_msg)
        {
            // close the wait box
            if (authWaitBox) { StopBox(); authWaitBox = false; }
            if (!valid)
            {
                common.DPrintf("auth key is invalid\n");
                authMsg = auth_msg;
                if (cdkey_state == CDKEY.CHECKING) cdkey_state = CDKEY.INVALID;
                if (xpkey_state == CDKEY.CHECKING) xpkey_state = CDKEY.INVALID;
            }
            else
            {
                common.DPrintf("client is authed in\n");
                if (cdkey_state == CDKEY.CHECKING) cdkey_state = CDKEY.OK;
                if (xpkey_state == CDKEY.CHECKING) xpkey_state = CDKEY.OK;
            }
            authEmitTimeout = 0;
            SetCDKeyGuiVars();
        }

        // return true if we actually waiting on an auth reply
        public bool MaybeWaitOnCDKey()
        {
            if (authEmitTimeout > 0)
            {
                authWaitBox = true;
                sessLocal.MessageBox(MSG.WAIT, common.LanguageDictGetString("#str_07191"), null, true, null, null, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// we toggled some key state to CDKEY_CHECKING. send a standalone auth packet to validate
        /// </summary>
        void EmitGameAuth()
        {
            // make sure the auth reply is empty, we use it to indicate an auth reply
            authMsg = string.Empty;
            if (AsyncNetwork.client.SendAuthCheck(cdkey_state == CDKEY.CHECKING ? cdkey : null, xpkey_state == CDKEY.CHECKING ? xpkey : null))
            {
                authEmitTimeout = SysW.Milliseconds + CDKEY_AUTH_TIMEOUT;
                common.DPrintf("authing with the master..\n");
            }
            // net is not available
            else
            {
                common.DPrintf("sendAuthCheck failed\n");
                if (cdkey_state == CDKEY.CHECKING) cdkey_state = CDKEY.OK;
                if (xpkey_state == CDKEY.CHECKING) xpkey_state = CDKEY.OK;
            }
        }

        public string AuthMsg
            => authMsg;
    }
}

