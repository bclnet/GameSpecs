using static GameSpec.IW.Zone.Asset;

namespace GameSpec.IW.Zone
{
    public unsafe partial struct PhysPreset
    {
        // easy
        public static void writePhysPreset(ZoneInfo info, ZStream buf, PhysPreset* data)
        {
            fixed (byte* _ = buf.at)
            {
                //: WRITE_ASSET(data, PhysPreset);
                PhysPreset* dest = (PhysPreset*)_;
                buf.write((byte*)data, sizeof(PhysPreset), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);

                //: WRITE_NAME(data);
                buf.write(data->name, strlen(data->name) + 1, 1);
                dest->name = (char*)-1;

                //: WRITE_STRING(data, sndAliasPrefix);
                buf.write(data->sndAliasPrefix, strlen(data->sndAliasPrefix) + 1, 1);
                dest->sndAliasPrefix = (char*)-1;

                buf.popStream();
            }
        }

        //void* addPhysPreset(ZoneInfo info, string name, char* data, int dataLen)
        //{
        //    if (dataLen < 0) { return data; }

        //    PhysPreset* ret = new PhysPreset;
        //    memset(ret, 0, sizeof(PhysPreset));

        //    ret->name = strdup(name);

        //    if (strlen(Info_ValueForKey(data, "sndAliasPrefix")) > 0) ret->sndAliasPrefix = Info_ValueForKey(data, "sndAliasPrefix");
        //    if (strlen(Info_ValueForKey(data, "mass")) > 0) ret->mass = stof(Info_ValueForKey(data, "mass"));
        //    if (strlen(Info_ValueForKey(data, "friction")) > 0) ret->friction = stof(Info_ValueForKey(data, "friction"));
        //    if (strlen(Info_ValueForKey(data, "bounce")) > 0) ret->bounce = stof(Info_ValueForKey(data, "bounce"));
        //    if (strlen(Info_ValueForKey(data, "bulletForceScale")) > 0) ret->bulletForceScale = stof(Info_ValueForKey(data, "bulletForceScale"));
        //    if (strlen(Info_ValueForKey(data, "piecesSpreadFraction")) > 0) ret->piecesSpreadFraction = stof(Info_ValueForKey(data, "piecesSpreadFraction"));
        //    if (strlen(Info_ValueForKey(data, "piecesUpwardVelocity")) > 0) ret->piecesUpwardVelocity = stof(Info_ValueForKey(data, "piecesUpwardVelocity"));

        //    return data;
        //}
    }
}