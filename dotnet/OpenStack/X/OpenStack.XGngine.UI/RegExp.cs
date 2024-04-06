using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class Register
    {
        public Register() { }
        public Register(string p, REGTYPE t)
        {
            name = p;
            type = t;
            Debug.Assert(t >= 0 && t < REGTYPE.NUMTYPES);
            regCount = REGCOUNT[(int)t];
            enabled = type == REGTYPE.STRING ? false : true;
            var = null;
        }

        public enum REGTYPE : short { VEC4 = 0, FLOAT, BOOL, INT, STRING, VEC2, VEC3, RECTANGLE, NUMTYPES };
        public static int[] REGCOUNT = { 4, 1, 1, 1, 0, 2, 3, 4 };

        public bool enabled;
        public REGTYPE type;
        public string name;
        public int regCount;
        public ushort[] regs = new ushort[4];
        public WinVar var;

        public void SetToRegs(float[] registers)
        {
            Vector4 v = new(); Vector2 v2; Vector3 v3; Rectangle rect;

            if (!enabled || var == null || (var != null && (var.Dict != null || !var.Eval))) return;

            switch (type)
            {
                case REGTYPE.VEC4: v = (WinVec4)var; break;
                case REGTYPE.RECTANGLE: rect = (WinRectangle)var; v = rect.ToVec4(); break;
                case REGTYPE.VEC2: v2 = (WinVec2)var; v.x = v2.x; v.y = v2.y; break;
                case REGTYPE.VEC3: v3 = (WinVec3)var; v.x = v3.x; v.y = v3.y; v.z = v3.z; break;
                case REGTYPE.FLOAT: v.x = (WinFloat)var; break;
                case REGTYPE.INT: v.x = (WinInt)var; break;
                case REGTYPE.BOOL: v.x = (WinBool)var ? 1f : 0f; break;
                default: common.FatalError("Register::SetToRegs: bad reg type"); break;
            }
            for (var i = 0; i < regCount; i++) registers[regs[i]] = v[i];
        }

        public void GetFromRegs(float[] registers)
        {
            Vector4 v = new(); Rectangle rect = new();

            if (!enabled || var == null || (var != null && (var.Dict != null || !var.Eval))) return;

            for (var i = 0; i < regCount; i++) v[i] = registers[regs[i]];

            switch (type)
            {
                case REGTYPE.VEC4: var = (WinVec4)v; break;
                case REGTYPE.RECTANGLE: rect.x = v.x; rect.y = v.y; rect.w = v.z; rect.h = v.w; var = (WinRectangle)rect; break;
                case REGTYPE.VEC2: var = (WinVec2)v.ToVec2(); break;
                case REGTYPE.VEC3: var = (WinVec3)v.ToVec3(); break;
                case REGTYPE.FLOAT: var = (WinFloat)v[0]; break;
                case REGTYPE.INT: var = (WinInt)v[0]; break;
                case REGTYPE.BOOL: var = (WinBool)(v[0] != 0.0f); break;
                default: common.FatalError("Register::GetFromRegs: bad reg type"); break;
            }
        }

        public void CopyRegs(Register src)
        {
            regs[0] = src.regs[0];
            regs[1] = src.regs[1];
            regs[2] = src.regs[2];
            regs[3] = src.regs[3];
        }

        public void Enable(bool b) => enabled = b;

        public void ReadFromDemoFile(VFileDemo f)
        {
            f.ReadBool(out enabled);
            f.ReadShort(out short type); this.type = (REGTYPE)type;
            f.ReadInt(out regCount);
            for (var i = 0; i < 4; i++) f.ReadUnsignedShort(out regs[i]);
            name = f.ReadHashString();
        }

        public void WriteToDemoFile(VFileDemo f)
        {
            f.WriteBool(enabled);
            f.WriteShort((short)type);
            f.WriteInt(regCount);
            for (var i = 0; i < 4; i++) f.WriteUnsignedShort(regs[i]);
            f.WriteHashString(name);
        }

        public void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(enabled);
            savefile.Write(type);
            savefile.Write(regCount);
            savefile.WriteTMany(regs);

            var len = name.Length;
            savefile.Write(len); savefile.Write(Encoding.ASCII.GetBytes(name), len);

            var.WriteToSaveGame(savefile);
        }

        public void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(out enabled);
            savefile.Read(out type);
            savefile.Read(out regCount);
            savefile.ReadTMany(out regs, regs.Length);

            savefile.Read(out int len); savefile.ReadASCII(out name, len);

            var.ReadFromSaveGame(savefile);
        }
    }

    public class RegisterList
    {
        Dictionary<string, Register> regs = new(StringComparer.OrdinalIgnoreCase);

        public void AddReg(string name, Register.REGTYPE type, Parser src, Window win, WinVar var)
        {
            var reg = FindReg(name);
            if (reg == null)
            {
                Debug.Assert(type >= 0 && type < Register.REGTYPE.NUMTYPES);
                var numRegs = Register.REGCOUNT[(int)type];
                reg = new Register(name, type) { var = var };
                if (type == Register.REGTYPE.STRING)
                {
                    if (src.ReadToken(out var tok))
                    {
                        tok = common.LanguageDictGetString(tok);
                        var.Init(tok, win);
                    }
                }
                else
                    for (var i = 0; i < numRegs; i++)
                    {
                        reg.regs[i] = (ushort)win.ParseExpression(src, null);
                        if (i < numRegs - 1) src.ExpectTokenString(",");
                    }
                regs.Add(name, reg);
            }
            else
            {
                var numRegs = Register.REGCOUNT[(int)type];
                reg.var = var;
                if (type == Register.REGTYPE.STRING)
                {
                    if (src.ReadToken(out var tok)) var.Init(tok, win);
                }
                else
                    for (var i = 0; i < numRegs; i++)
                    {
                        reg.regs[i] = (ushort)win.ParseExpression(src, null);
                        if (i < numRegs - 1) src.ExpectTokenString(",");
                    }
            }
        }

        public void AddReg(string name, Register.REGTYPE type, Vector4 data, Window win, WinVar var)
        {
            if (FindReg(name) == null)
            {
                Debug.Assert(type >= 0 && type < Register.REGTYPE.NUMTYPES);
                var numRegs = Register.REGCOUNT[(int)type];
                var reg = new Register(name, type) { var = var };
                for (var i = 0; i < numRegs; i++) reg.regs[i] = (ushort)win.ExpressionConstant(data[i]);
                regs.Add(name, reg);
            }
        }

        public Register FindReg(string name)
            => regs.TryGetValue(name, out var z) ? z : null;

        public void SetToRegs(float[] registers)
        {
            foreach (var reg in regs.Values)
                reg.SetToRegs(registers);
        }

        public void GetFromRegs(float[] registers)
        {
            foreach (var reg in regs.Values)
                reg.GetFromRegs(registers);
        }

        public void Reset()
            => regs.Clear();

        public void ReadFromDemoFile(VFileDemo f)
        {
            regs.Clear();
            f.ReadInt(out var c);
            for (var i = 0; i < c; i++)
            {
                var reg = new Register();
                reg.ReadFromDemoFile(f);
                regs.Add(reg.name, reg);
            }
        }

        public void WriteToDemoFile(VFileDemo f)
        {
            f.WriteInt(regs.Count);
            foreach (var reg in regs.Values) reg.WriteToDemoFile(f);
        }

        public void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(regs.Count);
            foreach (var reg in regs.Values) reg.WriteToSaveGame(savefile);
        }

        public void ReadFromSaveGame(VFile savefile)
        {
            regs.Clear();
            savefile.Read(out int num);
            for (var i = 0; i < num; i++)
            {
                var reg = new Register();
                reg.ReadFromSaveGame(savefile);
                regs.Add(reg.name, reg);
            }
        }
    }
}