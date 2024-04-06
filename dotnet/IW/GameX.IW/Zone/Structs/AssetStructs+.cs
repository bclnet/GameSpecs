using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone
{
    public unsafe partial struct PhysPreset
    {
        #region PhysPresetData

        // easy
        public static void writePhysPreset(ZoneInfo info, ZStream buf, PhysPreset* data)
        {
            fixed (byte* _ = buf.at)
            {
                //: WRITE_ASSET(data, PhysPreset);
                var dest = (PhysPreset*)_;
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

        public static object addPhysPreset(ZoneInfo info, string name, char* data, int dataLen)
        {
            if (dataLen < 0) { return null; }

            var ret = new PhysPreset();
            //ret.name = name;

            //if (strlen(Info_ValueForKey(data, "sndAliasPrefix")) > 0) ret.sndAliasPrefix = Info_ValueForKey(data, "sndAliasPrefix");
            //if (strlen(Info_ValueForKey(data, "mass")) > 0) ret.mass = stof(Info_ValueForKey(data, "mass"));
            //if (strlen(Info_ValueForKey(data, "friction")) > 0) ret.friction = stof(Info_ValueForKey(data, "friction"));
            //if (strlen(Info_ValueForKey(data, "bounce")) > 0) ret.bounce = stof(Info_ValueForKey(data, "bounce"));
            //if (strlen(Info_ValueForKey(data, "bulletForceScale")) > 0) ret.bulletForceScale = stof(Info_ValueForKey(data, "bulletForceScale"));
            //if (strlen(Info_ValueForKey(data, "piecesSpreadFraction")) > 0) ret.piecesSpreadFraction = stof(Info_ValueForKey(data, "piecesSpreadFraction"));
            //if (strlen(Info_ValueForKey(data, "piecesUpwardVelocity")) > 0) ret.piecesUpwardVelocity = stof(Info_ValueForKey(data, "piecesUpwardVelocity"));

            return ret;
        }

        #endregion

        #region RawfileData

        public static void writeRawfile(ZoneInfo info, ZStream buf, Rawfile* data)
        {
            fixed (byte* _ = buf.at)
            {
                var dest = (Rawfile*)_;
                buf.write((byte*)data, sizeof(Rawfile), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);

                var writeLen = data->sizeCompressed;
                if (writeLen == 0) writeLen = data->sizeUnCompressed + 1;

                buf.write(data->name, strlen(data->name) + 1, 1);
                dest->name = (char*)-1;

                if (data->compressedData != null)
                {
                    //buf->align(ALIGN_TO_0);
                    buf.write((byte*)data->compressedData, writeLen, 1);
                    dest->compressedData = (char*)-1;
                }

                buf.popStream();
            }
        }

        public static object addRawfile(ZoneInfo info, char* name, char* data, int dataLen)
        {
            if (dataLen < 0) return null; // no fixups needed here

            //z_stream strm;
            //memset(&strm, 0, sizeof(z_stream));
            //char* dest = new char[dataLen * 2];

            //strm.next_out = (Bytef*)dest;
            //strm.next_in = (Bytef*)data;
            //strm.avail_out = dataLen * 2;
            //strm.avail_in = dataLen;

            //if (deflateInit(&strm, Z_BEST_COMPRESSION) != Z_OK) { Com_Error(false, "Failed to compress zlib buffer!"); return NULL; }
            //if (deflate(&strm, Z_FINISH) != Z_STREAM_END) { Com_Error(false, "Failed to compress zlib buffer!"); return NULL; }
            //if (deflateEnd(&strm) != Z_OK) { Com_Error(false, "Failed to compress zlib buffer!"); return NULL; }

            var ret = new Rawfile();
            //ret->name = strdup(name);
            //ret->sizeCompressed = strm.total_out;
            //ret->sizeUnCompressed = dataLen;
            //ret->compressedData = dest;

            return ret;
        }

        #endregion
    }
}