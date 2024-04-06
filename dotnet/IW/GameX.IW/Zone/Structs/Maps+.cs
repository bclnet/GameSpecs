using System;
using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone
{
    public unsafe partial struct cBrushSide
    {
        public static void writecbrushside_t(ZStream buf, cBrushSide* data, int num)
        {
            fixed (byte* _ = buf.at)
            {
                //: WRITE_ASSET_NUM(data, cBrushSide, num);
                var dest = (cBrushSide*)_;
                buf.write((byte*)data, sizeof(cBrushSide), num);

                for (var i = 0; i < num; i++)
                    //: HAS_FIELD((&data[i]), side)
                    if ((&data[i])->side != null) // OffsetToPoiner
                    {
                        buf.align(ZStream.ALIGN_TO_4);
                        buf.write((char*)data[i].side, sizeof(cPlane), 1);
                        dest[i].side = (cPlane*)-1;
                    }
            }
        }
    }

    public unsafe partial struct BrushWrapper
    {
        public static void writeBrush(ZStream buf, BrushWrapper* data, BrushWrapper* dest)
        {
            fixed (byte* _ = buf.at)
            {
                cBrushSide.writecbrushside_t(buf, data->brush.brushSide, data->brush.count);
                data->brush.brushSide = (cBrushSide*)-1;

                //: WRITE_FIELD(data, brush.brushEdge, char, totalEdgeCount);
                if (data->brush.brushEdge != null)
                {
                    buf.write((byte*)data->planes, sizeof(char), data->totalEdgeCount);
                    dest->brush.brushEdge = (char*)-1;
                }
            }
        }

        public static void writeBrushWrapper(ZStream buf, BrushWrapper* data)
        {
            fixed (byte* _ = buf.at)
            {
                //: WRITE_ASSET(data, BrushWrapper);
                var dest = (BrushWrapper*)_;
                buf.write((byte*)data, sizeof(BrushWrapper), 1);

                writeBrush(buf, data, dest);

                //: WRITE_FIELD_ALIGNED(data, planes, cPlane, brush.count, ALIGN_TO_4); // OffsetToPoiner
                if (data->planes != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    buf.write((byte*)data->planes, sizeof(cPlane), data->brush.count);
                    dest->planes = (cPlane*)-1;
                }
            }
        }
    }

    public unsafe partial struct PhysGeomInfo
    {
        public static void writePhysGeomInfo(ZStream buf, PhysGeomInfo* data, int num)
        {
            fixed (byte* _ = buf.at)
            {
                //: WRITE_ASSET_NUM(data, PhysGeomInfo, num);
                var dest = (PhysGeomInfo*)_;
                buf.write((byte*)data, sizeof(PhysGeomInfo), num);

                for (var i = 0; i < num; i++)
                    if (dest[i].brush != null)
                    {
                        buf.align(ZStream.ALIGN_TO_4);
                        BrushWrapper.writeBrushWrapper(buf, dest[i].brush);
                    }
            }
        }
    }

    public unsafe partial struct PhysGeomList
    {
        public static void writePhysCollmap(ZoneInfo info, ZStream buf, PhysGeomList* data)
        {
            fixed (byte* _ = buf.at)
            {
                //: WRITE_ASSET(data, PhysGeomList);
                var dest = (PhysGeomList*)_;
                buf.write((byte*)data, sizeof(PhysGeomList), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);

                //: WRITE_NAME(data);
                buf.write(data->name, strlen(data->name) + 1, 1);
                dest->name = (char*)-1;

                if (data->geoms != null)
                {
                    buf.align(ZStream.ALIGN_TO_4);
                    PhysGeomInfo.writePhysGeomInfo(buf, data->geoms, (int)data->count);
                }

                buf.popStream();
            }
        }

        public static object addPhysCollmap(ZoneInfo info, string name, char* data, int dataLen)
        {
            if (dataLen < 0) return null;
            Console.Write("Can't add new PhysCollmap assets!");
            return null;
        }
    }
}