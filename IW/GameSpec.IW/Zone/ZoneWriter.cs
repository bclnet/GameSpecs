using System;
using static GameSpec.IW.Zone.Asset;

namespace GameSpec.IW.Zone
{
    // https://github.com/RagdollPhysics/zonebuilder
    public unsafe class ZoneWriter
    {
        // must be called before you write anything in your asset!!!
        int requireAsset(ZoneInfo info, ASSET_TYPE type, string name, ZStream buf)
        {
            var a = info.containsAsset(type, name);
            if (a >= 0)
            {
                var off = writeAsset(info, info.assets[a], buf);
                // this will make no sense... just let it we are returning the offset to the pointer in the asset index which isn't generated until load time go figure
                if (desiredFFVersion == 276)
                    return (3 << 28) | ((info.index_start + (8 * a) + 4) & 0x0FFFFFFF) + 1;
                else if (desiredFFVersion == 277)
                    return (int)(off | 0xF0000000);
            }
            else
                Console.WriteLine($"Missing required asset {name} ({getAssetStringForType(type)}). Export may (and probably will) fail!");
            return -1;
        }

        int writeAsset(ZoneInfo info, Asset asset, ZStream buf)
        {
            if (asset.written) return asset.offset;
            buf.pushStream(ZSTREAM.TEMP);
            buf.align(ZStream.ALIGN_TO_4); // every asset header is aligned this way
            asset.offset = (((int)ZSTREAM.TEMP & 0x0F) << 28) | ((buf.getStreamOffset(ZSTREAM.TEMP) + 1) & 0x0FFFFFFF);

            var name = getAssetName(asset.type, asset.data);

            // hide the useless assets that we can't change
            if (asset.type != ASSET_TYPE.TECHSET &&
                asset.type != ASSET_TYPE.PIXELSHADER &&
                asset.type != ASSET_TYPE.VERTEXSHADER &&
                asset.type != ASSET_TYPE.VERTEXDECL)
            {
                Console.WriteLine($"\nWriting asset {name}, of type {getAssetStringForType(asset.type)} at offset 0x{asset.offset:x}");
            }
            else
            {
                Console.WriteLine($"\nWriting asset {name}, of type {getAssetStringForType(asset.type)} at offset 0x{asset.offset:x}");
            }

            switch (asset.type)
            {
                case ASSET_TYPE.PHYSPRESET:
                    {
                        var box = (PhysPreset)asset.data;
                        PhysPreset.writePhysPreset(info, buf, &box);
                        break;
                    }
                case ASSET_TYPE.PHYS_COLLMAP:
                    {
                        var box = (PhysGeomList)asset.data;
                        PhysGeomList.writePhysCollmap(info, buf, &box);
                        break;
                    }
                    //case ASSET_TYPE.XANIM:
                    //    XAnim.writeXAnim(info, buf, (XAnim)asset.data);
                    //    break;
                    //// ASSET_TYPE_XMODELSURFS - handled by xmodel
                    //case ASSET_TYPE.XMODEL:
                    //    XModel.writeXModel(info, buf, (XModel)asset.data);
                    //    break;
                    //case ASSET_TYPE.MATERIAL:
                    //    Material.writeMaterial(info, buf, (Material)asset.data);
                    //    break;
                    //case ASSET_TYPE.PIXELSHADER:
                    //    PixelShader.writePixelShader(info, buf, (PixelShader)asset.data);
                    //    break;
                    //case ASSET_TYPE.VERTEXSHADER:
                    //    VertexShader.writeVertexShader(info, buf, (VertexShader)asset.data);
                    //    break;
                    //case ASSET_TYPE.VERTEXDECL:
                    //    VertexDecl.writeVertexDecl(info, buf, (VertexDecl)asset.data);
                    //    break;
                    //case ASSET_TYPE.TECHSET:
                    //    MaterialTechniqueSet.writeTechset(info, buf, (MaterialTechniqueSet)asset.data);
                    //    break;
                    //case ASSET_TYPE.IMAGE:
                    //    GfxImage.writeGfxImage(info, buf, (GfxImage)asset.data);
                    //    break;
                    //case ASSET_TYPE.SOUND:
                    //    SoundAliasList.writeSoundAlias(info, buf, (SoundAliasList)asset.data);
                    //    break;
                    //case ASSET_TYPE.SNDCURVE:
                    //    SndCurve.writeSndCurve(info, buf, (SndCurve)asset.data);
                    //    break;
                    //case ASSET_TYPE.LOADED_SOUND:
                    //    LoadedSound.writeLoadedSound(info, buf, (LoadedSound)asset.data);
                    //    break;
                    //case ASSET_TYPE.COL_MAP_MP:
                    //    ClipMap.writeColMap(info, buf, (ClipMap)asset.data);
                    //    break;
                    //case ASSET_TYPE.COM_MAP:
                    //    ComWorld.writeComWorld(info, buf, (ComWorld)asset.data);
                    //    break;
                    //case ASSET_TYPE.GAME_MAP_MP:
                    //case ASSET_TYPE.GAME_MAP_SP:
                    //    GameMap_MP.writeGameMap(info, buf, (GameMap_MP)asset.data);
                    //    break;
                    //case ASSET_TYPE.MAP_ENTS:
                    //    MapEnts.writeMapEnts(info, buf, (MapEnts)asset.data);
                    //    break;
                    //case ASSET_TYPE.FX_MAP:
                    //case ASSET_TYPE.GFX_MAP:
                    //    throw new Exception("How did you get an asset that you can't write?\n");
                    //    break;
                    //case ASSET_TYPE.LIGHTDEF:
                    //    GfxLightDef.writeGfxLightDef(info, buf, (GfxLightDef)asset.data);
                    //    break;
                    //case ASSET_TYPE.FONT:
                    //    Font.writeFont(info, buf, (Font)asset.data);
                    //    break;
                    //case ASSET_TYPE.MENUFILE:
                    //case ASSET_TYPE.MENU:
                    //    throw new Exception("How did you get an asset that you can't write?\n");
                    //case ASSET_TYPE.LOCALIZE:
                    //    Localize.writeLocalize(info, buf, (Localize)asset.data);
                    //    break;
                    //case ASSET_TYPE.WEAPON:
                    //    WeaponVariantDef.writeWeaponVariantDef(info, buf, (WeaponVariantDef)asset.data);
                    //    break;
                    //case ASSET_TYPE.FX:
                    //    FxEffectDef.writeFxEffectDef(info, buf, (FxEffectDef)asset.data);
                    //    break;
                    //case ASSET_TYPE.IMPACTFX:
                    //    FxImpactTable.writeFxImpactTable(info, buf, (FxImpactTable)asset.data);
                    //    break;
                    //case ASSET_TYPE.RAWFILE:
                    //    Rawfile.writeRawfile(info, buf, (Rawfile)asset.data);
                    //    break;
                    //case ASSET_TYPE.STRINGTABLE:
                    //    StringTable.writeStringTable(info, buf, (StringTable)asset.data);
                    //    break;
                    //case ASSET_TYPE.LEADERBOARDDEF:
                    //    LeaderboardDef.writeLeaderboardDef(info, buf, (LeaderboardDef)asset.data);
                    //    break;
                    //case ASSET_TYPE.STRUCTUREDDATADEF:
                    //    StructuredDataDefSet.writeStructuredDataDefSet(info, buf, (StructuredDataDefSet)asset.data);
                    //    break;
                    //case ASSET_TYPE.TRACER:
                    //    Tracer.writeTracer(info, buf, (Tracer)asset.data);
                    //    break;
                    //case ASSET_TYPE.VEHICLE:
                    //    VehicleDef.writeVehicleDef(info, buf, (VehicleDef)asset.data);
                    //    break;
            }

            buf.popStream(); // TEMP

            asset.written = true;
            return asset.offset;
        }

        ZStream writeZone(ZoneInfo info)
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