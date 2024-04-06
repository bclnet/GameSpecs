using static GameX.IW.Zone.Asset;

namespace GameX.IW.Zone
{
    public unsafe partial struct VehicleDef
    {
        public static void writeVehicleDef(ZoneInfo info, ZStream buf, VehicleDef* data)
        {
            fixed (byte* _ = buf.at)
            {
                int physPreset = 0, turretWeapon = 0, compassFriendlyIcon = 0, compassEnemyIcon = 0;
                if (data->vehiclePhysics.physPreset != null)
                    physPreset = ZoneWriter.requireAsset(info, UnkAssetType.PHYSPRESET, new string(data->vehiclePhysics.physPreset->name), buf);
                if (data->turretWeapon != null)
                    turretWeapon = ZoneWriter.requireAsset(info, UnkAssetType.WEAPON, new string(data->turretWeapon->name), buf);
                if (data->compassFriendlyIcon != null)
                    compassFriendlyIcon = ZoneWriter.requireAsset(info, UnkAssetType.MATERIAL, new string(data->compassFriendlyIcon->name), buf);
                if (data->compassEnemyIcon != null)
                    compassEnemyIcon = ZoneWriter.requireAsset(info, UnkAssetType.MATERIAL, new string(data->compassEnemyIcon->name), buf);

                //: WRITE_ASSET(data, VehicleDef);
                var dest = (VehicleDef*)_;
                buf.write((byte*)data, sizeof(VehicleDef), 1);
                buf.pushStream(ZSTREAM.VIRTUAL);

                //: WRITE_NAME(data);
                buf.write(data->name, strlen(data->name) + 1, 1);
                dest->name = (char*)-1;

                //: HAS_FIELD(data, useHintString)
                if (data->useHintString != null)
                {
                    //: WRITE_STRING(data, useHintString);
                    buf.write(data->useHintString, strlen(data->useHintString) + 1, 1);
                    dest->useHintString = (char*)-1;
                }

                if (data->vehiclePhysics.physPresetName != null)
                {
                    //:WRITE_STRING(data, vehiclePhysics.physPresetName);
                    buf.write(data->vehiclePhysics.physPresetName, strlen(data->vehiclePhysics.physPresetName) + 1, 1);
                    dest->vehiclePhysics.physPresetName = (char*)-1;
                }

                if (data->vehiclePhysics.physPreset != null)
                    dest->vehiclePhysics.physPreset = (PhysPreset*)physPreset;

                if (data->vehiclePhysics.accelGraphName != null)
                {
                    //: WRITE_STRING(data, vehiclePhysics.accelGraphName);
                    buf.write(data->vehiclePhysics.accelGraphName, strlen(data->vehiclePhysics.accelGraphName) + 1, 1);
                    dest->vehiclePhysics.accelGraphName = (char*)-1;
                }

                if (data->turretWeaponName != null)
                {
                    //: WRITE_STRING(data, turretWeaponName);
                    buf.write(data->turretWeaponName, strlen(data->turretWeaponName) + 1, 1);
                    dest->turretWeaponName = (char*)-1;
                }

                if (data->turretWeapon != null)
                    dest->turretWeapon = (WeaponVariantDef*)turretWeapon;

                if (data->turretSpinSnd.name != null)
                {
                    //: WRITE_STRING(data, turretSpinSnd.name);
                    buf.write(data->turretSpinSnd.name, strlen(data->turretSpinSnd.name) + 1, 1);
                    dest->turretSpinSnd.name = (char*)-1;
                }

                if (data->turretStopSnd.name != null)
                {
                    //: WRITE_STRING(data, turretStopSnd.name);
                    buf.write(data->turretStopSnd.name, strlen(data->turretStopSnd.name) + 1, 1);
                    dest->turretStopSnd.name = (char*)-1;
                }

                if (data->compassFriendlyIcon != null)
                    dest->compassFriendlyIcon = (Material*)compassFriendlyIcon;

                if (data->compassEnemyIcon != null)
                    dest->compassEnemyIcon = (Material*)compassEnemyIcon;

                if (data->lowIdleSound.name != null)
                {
                    //: WRITE_STRING(data, lowIdleSound.name);
                    buf.write(data->lowIdleSound.name, strlen(data->lowIdleSound.name) + 1, 1);
                    dest->lowIdleSound.name = (char*)-1;
                }

                if (data->highIdleSound.name != null)
                {
                    //:WRITE_STRING(data, highIdleSound.name);
                    buf.write(data->highIdleSound.name, strlen(data->highIdleSound.name) + 1, 1);
                    dest->highIdleSound.name = (char*)-1;
                }

                if (data->lowEngineSound.name != null)
                {
                    //:WRITE_STRING(data, lowEngineSound.name);
                    buf.write(data->lowEngineSound.name, strlen(data->lowEngineSound.name) + 1, 1);
                    dest->lowEngineSound.name = (char*)-1;
                }

                if (data->highEngineSound.name != null)
                {
                    //:WRITE_STRING(data, highEngineSound.name);
                    buf.write(data->highEngineSound.name, strlen(data->highEngineSound.name) + 1, 1);
                    dest->highEngineSound.name = (char*)-1;
                }

                if (data->engineStartUpSnd.name != null)
                {
                    //:WRITE_STRING(data, engineStartUpSnd.name);
                    buf.write(data->engineStartUpSnd.name, strlen(data->engineStartUpSnd.name) + 1, 1);
                    dest->engineStartUpSnd.name = (char*)-1;
                }

                if (data->engineShutdownSnd.name != null)
                {
                    //:WRITE_STRING(data, engineShutdownSnd.name);
                    buf.write(data->engineShutdownSnd.name, strlen(data->engineShutdownSnd.name) + 1, 1);
                    dest->engineShutdownSnd.name = (char*)-1;
                }

                if (data->engineIdleSnd.name != null)
                {
                    //:WRITE_STRING(data, engineIdleSnd.name);
                    buf.write(data->engineIdleSnd.name, strlen(data->engineIdleSnd.name) + 1, 1);
                    dest->engineIdleSnd.name = (char*)-1;
                }

                if (data->engineSustainSnd.name != null)
                {
                    //:WRITE_STRING(data, engineSustainSnd.name);
                    buf.write(data->engineSustainSnd.name, strlen(data->engineSustainSnd.name) + 1, 1);
                    dest->engineSustainSnd.name = (char*)-1;
                }

                if (data->engineRampUpSnd.name != null)
                {
                    //:WRITE_STRING(data, engineRampUpSnd.name);
                    buf.write(data->engineRampUpSnd.name, strlen(data->engineRampUpSnd.name) + 1, 1);
                    dest->engineRampUpSnd.name = (char*)-1;
                }

                if (data->engineRampDownSnd.name != null)
                {
                    //:WRITE_STRING(data, engineRampDownSnd.name);
                    buf.write(data->engineRampDownSnd.name, strlen(data->engineRampDownSnd.name) + 1, 1);
                    dest->engineRampDownSnd.name = (char*)-1;
                }

                if (data->suspensionSoftSnd.name != null)
                {
                    //:WRITE_STRING(data, suspensionSoftSnd.name);
                    buf.write(data->suspensionSoftSnd.name, strlen(data->suspensionSoftSnd.name) + 1, 1);
                    dest->suspensionSoftSnd.name = (char*)-1;
                }

                if (data->suspensionHardSnd.name != null)
                {
                    //:WRITE_STRING(data, suspensionHardSnd.name);
                    buf.write(data->suspensionHardSnd.name, strlen(data->suspensionHardSnd.name) + 1, 1);
                    dest->suspensionHardSnd.name = (char*)-1;
                }

                if (data->collisionSnd.name != null)
                {
                    //:WRITE_STRING(data, collisionSnd.name);
                    buf.write(data->collisionSnd.name, strlen(data->collisionSnd.name) + 1, 1);
                    dest->collisionSnd.name = (char*)-1;
                }

                if (data->speedSnd.name != null)
                {
                    //:WRITE_STRING(data, speedSnd.name);
                    buf.write(data->speedSnd.name, strlen(data->speedSnd.name) + 1, 1);
                    dest->speedSnd.name = (char*)-1;
                }

                if (data->surfaceSndPrefix != null)
                {
                    //:WRITE_STRING(data, surfaceSndPrefix);
                    buf.write(data->surfaceSndPrefix, strlen(data->surfaceSndPrefix) + 1, 1);
                    dest->surfaceSndPrefix = (char*)-1;
                }

                for (var i = 0; i < 31; i++)
                {
                    if (data->surfaceSounds(i)->name == null) continue;

                    buf.write((byte*)data->surfaceSounds(i)->name, strlen(data->surfaceSounds(i)->name) + 1, 1);
                    dest->surfaceSounds(i)->name = (char*)-1;
                }

                buf.popStream();
            }
        }

        //void* addVehicleDef(ZoneInfo info, char* name, char* data, int dataLen)
        //{
        //    if (dataLen > 0) { throw new Exception("No custom Vehicles yet."); return null; }
        //    return data;
        //}
    }
}