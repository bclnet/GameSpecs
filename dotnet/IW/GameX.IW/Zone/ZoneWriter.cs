using System;
using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone
{
    // https://github.com/RagdollPhysics/zonebuilder
    public unsafe class ZoneWriter
    {
        // must be called before you write anything in your asset!!!
        public static int requireAsset(ZoneInfo info, UnkAssetType type, string name, ZStream buf)
        {
            var a = info.containsAsset(type, name);
            if (a >= 0)
            {
                var off = writeAsset(info, info.assets[a], buf);
                // this will make no sense... just let it we are returning the offset to the pointer in the asset index which isn't generated until load time go figure
                if (ZoneInfo.desiredFFVersion == 276)
                    return (3 << 28) | ((info.index_start + (8 * a) + 4) & 0x0FFFFFFF) + 1;
                //else if (ZoneInfo.desiredFFVersion == 277)
                //    return (int)(off | 0xF0000000);
            }
            else
                Console.WriteLine($"Missing required asset {name} ({getAssetStringForType(type)}). Export may (and probably will) fail!");
            return -1;
        }

        public static int writeAsset(ZoneInfo info, Asset asset, ZStream buf)
        {
            if (asset.written) return asset.offset;
            buf.pushStream(ZSTREAM.TEMP);
            buf.align(ZStream.ALIGN_TO_4); // every asset header is aligned this way
            asset.offset = (((int)ZSTREAM.TEMP & 0x0F) << 28) | ((buf.getStreamOffset(ZSTREAM.TEMP) + 1) & 0x0FFFFFFF);

            var name = getAssetName(asset.type, asset.data);

            // hide the useless assets that we can't change
            if (asset.type != UnkAssetType.TECHSET &&
                asset.type != UnkAssetType.PIXELSHADER &&
                asset.type != UnkAssetType.VERTEXSHADER &&
                asset.type != UnkAssetType.VERTEXDECL)
                Console.WriteLine($"\nWriting asset {name}, of type {getAssetStringForType(asset.type)} at offset 0x{asset.offset:x}");
            else
                Console.WriteLine($"\nWriting asset {name}, of type {getAssetStringForType(asset.type)} at offset 0x{asset.offset:x}");

            switch (asset.type)
            {
                case UnkAssetType.PHYSPRESET: { var box = (PhysPreset)asset.data; PhysPreset.writePhysPreset(info, buf, &box); } break;
                case UnkAssetType.PHYS_COLLMAP: { var box = (PhysGeomList)asset.data; PhysGeomList.writePhysCollmap(info, buf, &box); } break;
                case UnkAssetType.XANIM: { var box = (XAnim)asset.data; XAnim.writeXAnim(info, buf, &box); } break;
                // ASSET_TYPE.XMODELSURFS - handled by xmodel
                case UnkAssetType.XMODEL: { var box = (XModel)asset.data; XModel.writeXModel(info, buf, &box); } break;
                case UnkAssetType.MATERIAL: { var box = (Material)asset.data; Material.writeMaterial(info, buf, &box); } break;
                //case ASSET_TYPE.PIXELSHADER: { var box = (PixelShader)asset.data; PixelShader.writePixelShader(info, buf, &box); } break;
                //case ASSET_TYPE.VERTEXSHADER: { var box = (VertexShader)asset.data; VertexShader.writeVertexShader(info, buf, &box); } break;
                //case ASSET_TYPE.VERTEXDECL: { var box = (VertexDecl)asset.data; VertexDecl.writeVertexDecl(info, buf, &box); } break;
                //case ASSET_TYPE.TECHSET: { var box = (MaterialTechniqueSet)asset.data; MaterialTechniqueSet.writeTechset(info, buf, &box); } break;
                //case ASSET_TYPE.IMAGE: { var box = (GfxImage)asset.data; GfxImage.writeGfxImage(info, buf, &box); } break;
                //case ASSET_TYPE.SOUND: { var box = (SoundAliasList)asset.data; SoundAliasList.writeSoundAlias(info, buf, &box); } break;
                //case ASSET_TYPE.SNDCURVE: { var box = (SndCurve)asset.data; SndCurve.writeSndCurve(info, buf, &box); } break;
                //case ASSET_TYPE.LOADED_SOUND: { var box = (LoadedSound)asset.data; LoadedSound.writeLoadedSound(info, buf, &box); } break;
                //case ASSET_TYPE.COL_MAP_MP: { var box = (ClipMap)asset.data; ClipMap.writeColMap(info, buf, &box); } break;
                //case ASSET_TYPE.COM_MAP: { var box = (ComWorld)asset.data; ComWorld.writeComWorld(info, buf, &box); } break;
                //case ASSET_TYPE.GAME_MAP_MP:
                //case ASSET_TYPE.GAME_MAP_SP: { var box = (GameMap_MP)asset.data; GameMap_MP.writeGameMap(info, buf, &box); } break;
                //case ASSET_TYPE.MAP_ENTS: { var box = (MapEnts)asset.data; MapEnts.writeMapEnts(info, buf, &box); } break;
                //case ASSET_TYPE.FX_MAP:
                //case ASSET_TYPE.GFX_MAP: throw new Exception("How did you get an asset that you can't write?\n");
                //case ASSET_TYPE.LIGHTDEF: { var box = (GfxLightDef)asset.data; GfxLightDef.writeGfxLightDef(info, buf, &box); } break;
                //case ASSET_TYPE.FONT: { var box = (Font)asset.data; Font.writeFont(info, buf, &box); } break;
                //case ASSET_TYPE.MENUFILE:
                //case ASSET_TYPE.MENU: throw new Exception("How did you get an asset that you can't write?\n");
                //case ASSET_TYPE.LOCALIZE: { var box = (Localize)asset.data; Localize.writeLocalize(info, buf, &box); } break;
                //case ASSET_TYPE.WEAPON: { var box = (WeaponVariantDef)asset.data; WeaponVariantDef.writeWeaponVariantDef(info, buf, &box); } break;
                //case ASSET_TYPE.FX: { var box = (FxEffectDef)asset.data; FxEffectDef.writeFxEffectDef(info, buf, &box); } break;
                //case ASSET_TYPE.IMPACTFX: { var box = (FxImpactTable)asset.data; FxImpactTable.writeFxImpactTable(info, buf, &box); } break;
                //case ASSET_TYPE.RAWFILE: { var box = (Rawfile)asset.data; Rawfile.writeRawfile(info, buf, &box); } break;
                //case ASSET_TYPE.STRINGTABLE: { var box = (StringTable)asset.data; StringTable.writeStringTable(info, buf, &box); } break;
                //case ASSET_TYPE.LEADERBOARDDEF: { var box = (LeaderboardDef)asset.data; LeaderboardDef.writeLeaderboardDef(info, buf, &box); } break;
                //case ASSET_TYPE.STRUCTUREDDATADEF: { var box = (StructuredDataDefSet)asset.data; StructuredDataDefSet.writeStructuredDataDefSet(info, buf, &box); } break;
                //case ASSET_TYPE.TRACER: { var box = (Tracer)asset.data; Tracer.writeTracer(info, buf, &box); } break;
                //case ASSET_TYPE.VEHICLE: { var box = (VehicleDef)asset.data; VehicleDef.writeVehicleDef(info, buf, &box); } break;
            }

            buf.popStream(); // TEMP

            asset.written = true;
            return asset.offset;
        }

        public static ZStream writeZone(ZoneInfo info)
        {
            var buf = new ZStream(info.scriptStringCount, info.assetCount);

            buf.pushStream(ZSTREAM.VIRTUAL);

            var pad = ZStream.pad;
            for (var i = 0; i < info.scriptStringCount; i++)
                buf.write(pad, 4, 1);

            for (var i = 0; i < info.scriptStringCount; i++)
                buf.write(info.scriptStrings[i], info.scriptStrings[i].Length + 1, 1);

            buf.align(ZStream.ALIGN_TO_4);

            info.index_start = buf.getStreamOffset(ZSTREAM.VIRTUAL);

            Console.WriteLine($"Index start is at 0x{info.index_start:x}");

            var neg1 = -1;
            for (var i = 0; i < info.assetCount; i++)
            {
                buf.write((int)info.assets[i].type, 4, 1);
                buf.write(neg1, 4, 1);
            }

            for (var i = 0; i < info.assetCount; i++)
                writeAsset(info, info.assets[i], buf);

            buf.resize(-1); // should be maxsize

            // update the stream sizes to be accurate in the written zone
            buf.updateStreamOffsetHeader();

            Console.WriteLine($"\nWrote {info.assetCount} assets, and {info.scriptStringCount} script strings\n");

            return buf;
        }
    }
}