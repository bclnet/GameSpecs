using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;
using Qhandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class RenderWindow : Window
    {
        RenderView refdef;
        IRenderWorld world;
        RenderEntity worldEntity;
        RenderLight rLight;
        IMD5Anim modelAnim;

        Qhandle worldModelDef;
        Qhandle lightDef;
        Qhandle modelDef;
        WinStr modelName;
        WinStr animName;
        string animClass;
        WinVec4 lightOrigin;
        WinVec4 lightColor;
        WinVec4 modelOrigin;
        WinVec4 modelRotate;
        WinVec4 viewOffset;
        WinBool needsRender;
        int animLength;
        int animEndTime;
        bool updateAnimation;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "animClass", StringComparison.OrdinalIgnoreCase)) { ParseString(src, out animClass); return true; }
            return base.ParseInternalVar(name, src);
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            if (string.Equals(name, "model", StringComparison.OrdinalIgnoreCase)) return modelName;
            if (string.Equals(name, "anim", StringComparison.OrdinalIgnoreCase)) return animName;
            if (string.Equals(name, "lightOrigin", StringComparison.OrdinalIgnoreCase)) return lightOrigin;
            if (string.Equals(name, "lightColor", StringComparison.OrdinalIgnoreCase)) return lightColor;
            if (string.Equals(name, "modelOrigin", StringComparison.OrdinalIgnoreCase)) return modelOrigin;
            if (string.Equals(name, "modelRotate", StringComparison.OrdinalIgnoreCase)) return modelRotate;
            if (string.Equals(name, "viewOffset", StringComparison.OrdinalIgnoreCase)) return viewOffset;
            if (string.Equals(name, "needsRender", StringComparison.OrdinalIgnoreCase)) return needsRender;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        new void CommonInit()
        {
            world = renderSystem.AllocRenderWorld();
            needsRender = true;
            lightOrigin = new Vector4(-128f, 0f, 0f, 1f);
            lightColor = new Vector4(1f, 1f, 1f, 1f);
            modelOrigin.Zero();
            viewOffset = new Vector4(-128f, 0f, 0f, 1f);
            modelAnim = null;
            animLength = 0;
            animEndTime = -1;
            modelDef = -1;
            updateAnimation = true;
        }

        public RenderWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public RenderWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }
        public void Dispose()
            => renderSystem.FreeRenderWorld(ref world);

        void BuildAnimation(int time)
        {
            if (!updateAnimation)
                return;

            if (animName.Length != 0 && animClass.Length != 0)
            {
                worldEntity.numJoints = worldEntity.hModel.NumJoints;
                worldEntity.joints = new JointMat[worldEntity.numJoints];
                modelAnim = gameEdit.ANIM_GetAnimFromEntityDef(animClass, animName);
                if (modelAnim != null)
                {
                    animLength = gameEdit.ANIM_GetLength(modelAnim);
                    animEndTime = time + animLength;
                }
            }
            updateAnimation = false;
        }

        void PreRender()
        {
            if (needsRender)
            {
                world.InitFromMap(null);
                var spawnArgs = new Dictionary<string, string> {
                    {"classname", "light"},
                    {"name", "light_1"},
                    {"origin", lightOrigin.ToVec3().ToString()},
                    {"_color", lightColor.ToVec3().ToString()},
                };
                gameEdit.ParseSpawnArgsToRenderLight(spawnArgs, rLight);
                lightDef = world.AddLightDef(rLight);
                if (((string)modelName).Length == 0)
                    common.Warning($"Window '{Name}' in gui '{Gui.SourceFile}': no model set");
                worldEntity.memset();
                spawnArgs.Clear();
                spawnArgs.Add("classname", "func_static");
                spawnArgs.Add("model", modelName);
                spawnArgs.Add("origin", modelOrigin.ToString());
                gameEdit.ParseSpawnArgsToRenderEntity(spawnArgs, worldEntity);
                if (worldEntity.hModel != null)
                {
                    var v = modelRotate.ToVec3();
                    worldEntity.axis = v.ToMat3();
                    worldEntity.shaderParms[0] = 1f;
                    worldEntity.shaderParms[1] = 1f;
                    worldEntity.shaderParms[2] = 1f;
                    worldEntity.shaderParms[3] = 1f;
                    modelDef = world.AddEntityDef(worldEntity);
                }
                needsRender = false;
            }
        }

        void Render(int time)
        {
            rLight.origin = lightOrigin.ToVec3();
            rLight.shaderParms[IRenderWorld.SHADERPARM_RED] = lightColor.x;
            rLight.shaderParms[IRenderWorld.SHADERPARM_GREEN] = lightColor.y;
            rLight.shaderParms[IRenderWorld.SHADERPARM_BLUE] = lightColor.z;
            world.UpdateLightDef(lightDef, rLight);
            if (worldEntity.hModel != null)
            {
                if (updateAnimation) BuildAnimation(time);
                if (modelAnim != null)
                {
                    if (time > animEndTime) animEndTime = time + animLength;
                    gameEdit.ANIM_CreateAnimFrame(worldEntity.hModel, modelAnim, worldEntity.numJoints, worldEntity.joints, animLength - (animEndTime - time), Vector3.origin, false);
                }
                worldEntity.axis = new Angles(modelRotate.x, modelRotate.y, modelRotate.z).ToMat3();
                world.UpdateEntityDef(modelDef, worldEntity);
            }
        }

        public override void Draw(int time, float x_, float y_)
        {
            PreRender();
            Render(time);

            refdef.memset();
            refdef.vieworg = viewOffset.ToVec3(); //: refdef.vieworg.Set(-128f, 0f, 0f);

            refdef.viewaxis.Identity();
            refdef.shaderParms[0] = 1;
            refdef.shaderParms[1] = 1;
            refdef.shaderParms[2] = 1;
            refdef.shaderParms[3] = 1;

            // DG: for scaling menus to 4:3 (like that spinning mars globe in the main menu)
            var x = drawRect.x;
            var y = drawRect.y;
            var w = drawRect.w;
            var h = drawRect.h;
            if (dc.IsMenuScaleFixActive) dc.AdjustCoords(ref x, ref y, ref w, ref h);

            refdef.x = (int)x;
            refdef.y = (int)y;
            refdef.width = (int)w;
            refdef.height = (int)h;
            // DG end
            refdef.fov_x = 90;
            refdef.fov_y = (float)(2 * Math.Atan(drawRect.h / drawRect.w) * MathX.M_RAD2DEG);

            refdef.time = time;
            world.RenderScene(refdef);
        }
    }
}