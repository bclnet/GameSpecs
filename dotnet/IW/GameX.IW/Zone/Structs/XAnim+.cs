using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone
{
    public unsafe partial struct XAnim
    {
        public static void writeXAnimDeltaParts(ZoneInfo info, ZStream buf, XAnim* parts)
        {
            fixed (byte* _ = buf.at)
            {
                XAnimDeltaPart* data = parts->delta;
                XAnimDeltaPart* dest = (XAnimDeltaPart*)_;
                buf.write((byte*)data, sizeof(XAnimDeltaPart), 1);

                if (data->trans != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->trans, 4, 1); // not full struct
                    if (data->trans->size != 0)
                    {
                        buf.write((byte*)&data->trans->u, 0x1C, 1); // not full struct
                        if (parts->framecount > 0x100)
                            buf.write((byte*)&data->trans->u.frames.indices, sizeof(short), data->trans->size + 1);
                        else
                            buf.write((byte*)&data->trans->u.frames.indices, sizeof(char), data->trans->size + 1);

                        if (data->trans->u.frames.frames._1 != null)
                        {
                            if (data->trans->smallTrans != 0)
                            {
                                buf.align(ZStream.ALIGN_TO_1);
                                buf.write((byte*)data->trans->u.frames.frames._1, sizeof(char) * 3, data->trans->size + 1);
                            }
                            else
                            {
                                buf.align(ZStream.ALIGN_TO_4);
                                buf.write((byte*)data->trans->u.frames.frames._2, sizeof(short) * 3, data->trans->size + 1);
                            }
                            //dest->trans->u.frames.frames = (char*)-1;
                        }
                    }
                    else
                        buf.write((byte*)data->trans->u.frame0, sizeof(float), 3);
                    dest->trans = (XAnimPartTrans*)-1;
                }

                if (data->quat2 != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->quat2, 4, 1); // not full struct
                    if (data->quat2->size != 0)
                    {
                        buf.write((byte*)&data->quat2->u, 0x4, 1); // not full struct
                        if (parts->framecount > 0x100)
                            buf.write((byte*)&data->quat2->u.frames.indices, sizeof(short), data->quat2->size + 1);
                        else
                            buf.write((byte*)&data->quat2->u.frames.indices, sizeof(char), data->quat2->size + 1);

                        if (data->quat2->u.frames.frames != null)
                        {
                            buf.align(ZStream.ALIGN_TO_4);
                            buf.write((byte*)data->quat2->u.frames.frames, sizeof(short) * 2, data->quat2->size + 1);
                            //dest->quat2->u.frames.frames = (short*)-1;
                        }
                    }
                    else
                        buf.write((byte*)data->quat2->u.frame0, sizeof(short) * 2, 1);
                    dest->quat2 = (XAnimDeltaPartQuat2*)-1;
                }

                if (data->quat != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->quat, 4, 1);
                    if (data->quat->size != 0)
                    {
                        buf.write((byte*)&data->quat->u, 0x4, 1); // not full struct
                        if (parts->framecount > 0x100)
                            buf.write((byte*)&data->quat->u.frames.indices, sizeof(short), data->quat->size + 1);
                        else
                            buf.write((byte*)&data->quat->u.frames.indices, sizeof(char), data->quat->size + 1);

                        if (data->quat->u.frames.frames != null)
                        {
                            buf.align(ZStream.ALIGN_TO_4);
                            buf.write((byte*)data->quat->u.frames.frames, sizeof(short) * 4, data->quat->size + 1);
                            //dest->quat->u.frames.frames = (short*)-1;
                        }
                    }
                    else
                        buf.write((byte*)data->quat->u.frame0, sizeof(short) * 4, 1);
                    dest->quat = (XAnimDeltaPartQuat*)-1;
                }
            }
        }

        public static void writeXAnim(ZoneInfo info, ZStream buf, XAnim* data)
        {
            fixed (byte* _ = buf.at)
            {
                //: WRITE_ASSET(data, XAnim);
                var dest = (XAnim*)_;
                buf.write((byte*)data, sizeof(XAnim), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);
                //: WRITE_NAME(data);
                buf.write(data->name, strlen(data->name) + 1, 1);
                dest->name = (char*)-1;

                //: WRITE_FIELD_ALIGNED(data, tagnames, short, boneCount[PART_TYPE_ALL], ALIGN_TO_2);
                if (data->tagnames != null)
                {
                    buf.align(ZStream.ALIGN_TO_2);
                    buf.write((byte*)data->tagnames, sizeof(short), data->boneCount[(int)XAnimPartType.PART_TYPE_ALL]);
                    dest->tagnames = (short*)-1;
                }
                //: WRITE_FIELD_ALIGNED(data, notetracks, XAnimNotifyInfo, notetrackCount, ALIGN_TO_4);
                if (data->notetracks != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->notetracks, sizeof(XAnimNotifyInfo), data->notetrackCount);
                    dest->notetracks = (XAnimNotifyInfo*)-1;
                }
                if (data->delta != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    writeXAnimDeltaParts(info, buf, data);
                    dest->delta = (XAnimDeltaPart*)-1;
                }
                //: WRITE_FIELD_ALIGNED(data, dataByte, char, dataByteCount, ALIGN_TO_1);
                if (data->dataByte != null)
                {
                    buf.align(ZStream.ALIGN_TO_1);
                    buf.write((byte*)data->dataByte, sizeof(char), data->dataByteCount);
                    dest->dataByte = (char*)-1;
                }
                //: WRITE_FIELD_ALIGNED(data, dataShort, short, dataShortCount, ALIGN_TO_2);
                if (data->dataShort != null)
                {
                    buf.align(ZStream.ALIGN_TO_2);
                    buf.write((byte*)data->dataShort, sizeof(short), data->dataShortCount);
                    dest->dataShort = (short*)-1;
                }
                //: WRITE_FIELD_ALIGNED(data, dataInt, int, dataIntCount, ALIGN_TO_4);
                if (data->dataInt != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->dataInt, sizeof(int), data->dataIntCount);
                    dest->dataInt = (int*)-1;
                }
                //: WRITE_FIELD_ALIGNED(data, randomDataShort, short, randomDataShortCount, ALIGN_TO_2);
                if (data->randomDataShort != null)
                {
                    buf.align(ZStream.ALIGN_TO_2);
                    buf.write((byte*)data->randomDataShort, sizeof(short), data->randomDataShortCount);
                    dest->randomDataShort = (short*)-1;
                }
                //: WRITE_FIELD_ALIGNED(data, randomDataByte, char, randomDataByteCount, ALIGN_TO_1);
                if (data->randomDataByte != null)
                {
                    buf.align(ZStream.ALIGN_TO_1);
                    buf.write((byte*)data->randomDataByte, sizeof(char), data->randomDataByteCount);
                    dest->randomDataByte = (char*)-1;
                }
                //: WRITE_FIELD_ALIGNED(data, randomDataInt, int, randomDataIntCount, ALIGN_TO_4);
                if (data->randomDataInt != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->randomDataInt, sizeof(int), data->randomDataIntCount);
                    dest->randomDataInt = (int*)-1;
                }
                if (data->indices.data != null)
                {
                    if (data->framecount > 255)
                    {
                        buf.align(ZStream.ALIGN_TO_2);
                        buf.write((byte*)data->indices.data, data->indexcount * 2, 1);
                    }
                    else
                    {
                        buf.align(ZStream.ALIGN_TO_1);
                        buf.write((byte*)data->indices.data, data->indexcount, 1);
                    }
                    dest->indices.data = (void*)-1;
                }

                buf.popStream();
            }
        }

        public static object addXAnim(ZoneInfo info, string name, char* data, int dataLen)
        {
            //    if (dataLen < 0)
            //    {
            //        XAnim* a = (XAnim*)data;
            //        // fix these if we are dumping a pre loaded anim
            //        for (int i = 0; i < a->boneCount[PART_TYPE_ALL]; i++)
            //            a->tagnames[i] = addScriptString(info, SL_ConvertToString(a->tagnames[i]));

            //        for (int i = 0; i < a->notetrackCount; i++)
            //            a->notetracks[i].name = addScriptString(info, SL_ConvertToString(a->notetracks[i].name));

            //        return a;
            //    }

            //    BUFFER* buf = new BUFFER(data, dataLen);

            var anim = new XAnim();
            //    int numTags;
            //    int numNotetracks;
            //    buf.read(&numTags, 4, 1);
            //    buf.read(&numNotetracks, 4, 1);

            //    // read in the script strings
            //    char str[128];
            //    short* tagnames = new short[numTags];

            //    for (int i = 0; i < numTags; i++)
            //    {
            //        buf.readstr(str, 128);
            //        tagnames[i] = addScriptString(info, str);
            //    }

            //    // read in the notetracks
            //    XAnimNotifyInfo* notetracks = new XAnimNotifyInfo[numNotetracks];

            //    for (int i = 0; i < numNotetracks; i++)
            //    {
            //        buf.readstr(str, 128);
            //        notetracks[i].name = addScriptString(info, str);
            //    }

            //    // copy to the new struct
            //    memcpy(anim, buf.at(), sizeof(XAnim));
            //    anim->tagnames = tagnames;
            //    anim->notetracks = notetracks;

            //    buf.seek(sizeof(XAnim), SEEK_CUR);
            //    buf.seek(strlen(buf.at()) + 1, SEEK_CUR);
            //    buf.seek(numTags * 2, SEEK_CUR); // skip the data to maintain compatibility

            //    XAnimNotifyInfo* tracks = (XAnimNotifyInfo*)buf.at();

            //    for (int i = 0; i < numNotetracks; i++)
            //        anim->notetracks[i].time = tracks[i].time;

            //    buf.seek(numNotetracks * 8, SEEK_CUR); // skip the data to maintain compatibility

            //    // load data into the struct
            //    anim->name = strdup(name);

            //    if (anim->delta)
            //    {
            //        anim->delta = new XAnimDeltaPart;
            //        memcpy(anim->delta, buf.at(), sizeof(XAnimDeltaPart));
            //        buf.seek(sizeof(XAnimDeltaPart), SEEK_CUR);
            //        if (anim->delta->trans)
            //        {
            //            // allocate here as big as it could possibly be
            //            XAnimPartTrans* tmpTrans = (XAnimPartTrans*)buf.at();
            //            anim->delta->trans = (XAnimPartTrans*)new char[sizeof(XAnimPartTrans) + ((tmpTrans->size + 1) * 2)];
            //            memcpy(anim->delta->trans, buf.at(), sizeof(XAnimPartTrans));
            //            buf.seek(sizeof(XAnimPartTrans), SEEK_CUR);
            //            if (anim->delta->trans->size)
            //            {
            //                if (anim->framecount < 256)
            //                {
            //                    memcpy(anim->delta->trans->u.frames.indices._1, buf.at(), (anim->delta->trans->size + 1));
            //                    buf.seek((anim->delta->trans->size + 1), SEEK_CUR);
            //                }
            //                else
            //                {
            //                    memcpy(anim->delta->trans->u.frames.indices._2, buf.at(), 2 * (anim->delta->trans->size + 1));
            //                    buf.seek(2 * (anim->delta->trans->size + 1), SEEK_CUR);
            //                }

            //                if (anim->delta->trans->u.frames.frames._1)
            //                {
            //                    // make it as big as it could possibly be
            //                    anim->delta->trans->u.frames.frames._2 = (unsigned short(*)[3])new char[6 * (anim->delta->trans->size + 1)];
            //                    if (anim->delta->trans->smallTrans)
            //                    {
            //                        memcpy(anim->delta->trans->u.frames.frames._1, buf.at(), 3 * (anim->delta->trans->size + 1));
            //                        buf.seek(3 * (anim->delta->trans->size + 1), SEEK_CUR);
            //                    }
            //                    else
            //                    {
            //                        memcpy(anim->delta->trans->u.frames.frames._2, buf.at(), 6 * (anim->delta->trans->size + 1));
            //                        buf.seek(6 * (anim->delta->trans->size + 1), SEEK_CUR);
            //                    }
            //                }
            //            }
            //            if (anim->delta->quat)
            //            {
            //                // allocate here as big as it could possibly be
            //                anim->delta->quat = (XAnimDeltaPartQuat*)new char[sizeof(XAnimDeltaPartQuat) + ((anim->delta->trans->size + 1) * 2)];
            //                memcpy(anim->delta->quat, buf.at(), sizeof(XAnimDeltaPartQuat));
            //                buf.seek(sizeof(XAnimDeltaPartQuat), SEEK_CUR);
            //                if (anim->delta->quat->size)
            //                {
            //                    if (anim->framecount < 256)
            //                    {
            //                        memcpy(anim->delta->quat->u.frames.indices, buf.at(), (anim->delta->quat->size + 1));
            //                        buf.seek((anim->delta->quat->size + 1), SEEK_CUR);
            //                    }
            //                    else
            //                    {
            //                        memcpy(anim->delta->quat->u.frames.indices, buf.at(), 2 * (anim->delta->quat->size + 1));
            //                        buf.seek(2 * (anim->delta->quat->size + 1), SEEK_CUR);
            //                    }

            //                    if (anim->delta->quat->u.frames.frames)
            //                    {
            //                        anim->delta->quat->u.frames.frames = new short[4 * (anim->delta->quat->size + 1)];
            //                        memcpy(anim->delta->quat->u.frames.frames, buf.at(), 8 * (anim->delta->quat->size + 1));
            //                        buf.seek(8 * (anim->delta->quat->size + 1), SEEK_CUR);
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    if (anim->dataByte)
            //    {
            //        anim->dataByte = new char[anim->dataByteCount];
            //        memcpy(anim->dataByte, buf.at(), anim->dataByteCount);
            //        buf.seek(anim->dataByteCount, SEEK_CUR);
            //    }

            //    if (anim->dataShort)
            //    {
            //        anim->dataShort = new short[anim->dataShortCount];
            //        memcpy(anim->dataShort, buf.at(), anim->dataShortCount * 2);
            //        buf.seek(anim->dataShortCount * 2, SEEK_CUR);
            //    }

            //    if (anim->dataInt)
            //    {
            //        anim->dataInt = new int[anim->dataIntCount];
            //        memcpy(anim->dataInt, buf.at(), anim->dataIntCount * 4);
            //        buf.seek(anim->dataIntCount * 4, SEEK_CUR);
            //    }

            //    if (anim->randomDataShort)
            //    {
            //        anim->randomDataShort = new short[anim->randomDataShortCount];
            //        memcpy(anim->randomDataShort, buf.at(), anim->randomDataShortCount * 2);
            //        buf.seek(anim->randomDataShortCount * 2, SEEK_CUR);
            //    }

            //    if (anim->randomDataByte)
            //    {
            //        anim->randomDataByte = new char[anim->randomDataByteCount];
            //        memcpy(anim->randomDataByte, buf.at(), anim->randomDataByteCount);
            //        buf.seek(anim->randomDataByteCount, SEEK_CUR);
            //    }

            //    if (anim->randomDataInt)
            //    {
            //        anim->randomDataInt = new int[anim->randomDataIntCount];
            //        memcpy(anim->randomDataInt, buf.at(), anim->randomDataIntCount * 4);
            //        buf.seek(anim->randomDataIntCount * 4, SEEK_CUR);
            //    }

            //    if (anim->indices.data)
            //    {
            //        if (anim->framecount < 256)
            //        {
            //            anim->indices.data = new char[anim->indexcount];
            //            memcpy(anim->indices.data, buf.at(), anim->indexcount);
            //        }
            //        else
            //        {
            //            anim->indices.data = new char[anim->indexcount * 2];
            //            memcpy(anim->indices.data, buf.at(), anim->indexcount * 2);
            //        }
            //    }

            return anim;
        }
    }
}