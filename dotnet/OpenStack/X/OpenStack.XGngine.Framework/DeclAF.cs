using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX.OpenStack.Gngine.Render;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public enum DECLAF_CONSTRAINT
    {
        INVALID,
        FIXED,
        BALLANDSOCKETJOINT,
        UNIVERSALJOINT,
        HINGE,
        SLIDER,
        SPRING
    }

    public enum DECLAF_JOINTMOD
    {
        AXIS,
        ORIGIN,
        BOTH
    }

    public delegate bool GetJointTransform(object model, JointMat frame, string jointName, out Vector3 origin, out Matrix3x3 axis);

    public class AFVector
    {
        public enum VEC
        {
            COORDS = 0,
            JOINT,
            BONECENTER,
            BONEDIR
        }

        public VEC type;
        public string joint1;
        public string joint2;
        Vector3 vec;
        bool negate;

        public AFVector()
        {
            type = VEC.COORDS;
            vec.Zero();
            negate = false;
        }

        public bool Parse(Lexer src)
        {
            if (!src.ReadToken(out var token)) return false;

            if (token == "-") { negate = true; if (!src.ReadToken(out token)) return false; }
            else negate = false;

            if (token == "(")
            {
                type = VEC.COORDS;
                vec.x = src.ParseFloat(); src.ExpectTokenString(",");
                vec.y = src.ParseFloat(); src.ExpectTokenString(",");
                vec.z = src.ParseFloat(); src.ExpectTokenString(")");
            }
            else if (token == "joint")
            {
                type = VEC.JOINT;
                src.ExpectTokenString("(");
                src.ReadToken(out token); joint1 = token;
                src.ExpectTokenString(")");
            }
            else if (token == "bonecenter")
            {
                type = VEC.BONECENTER;
                src.ExpectTokenString("(");
                src.ReadToken(out token); joint1 = token; src.ExpectTokenString(",");
                src.ReadToken(out token); joint2 = token;
                src.ExpectTokenString(")");
            }
            else if (token == "bonedir")
            {
                type = VEC.BONEDIR;
                src.ExpectTokenString("(");
                src.ReadToken(out token); joint1 = token; src.ExpectTokenString(",");
                src.ReadToken(out token); joint2 = token;
                src.ExpectTokenString(")");
            }
            else { src.Error($"unknown token {token} in vector"); return false; }

            return true;
        }

        public bool Finish(string fileName, GetJointTransform GetJointTransform, JointMat frame, object model)
        {
            Vector3 start, end;
            switch (type)
            {
                case VEC.COORDS:
                    break;
                case VEC.JOINT:
                    if (!GetJointTransform(model, frame, joint1, out vec, out _)) { common.Warning($"invalid joint {joint1} in joint() in '{fileName}'"); vec.Zero(); }
                    break;
                case VEC.BONECENTER:
                    if (!GetJointTransform(model, frame, joint1, out start, out _)) { common.Warning($"invalid joint {joint1} in bonecenter() in '{fileName}'"); start.Zero(); }
                    if (!GetJointTransform(model, frame, joint2, out end, out _)) { common.Warning($"invalid joint {joint2} in bonecenter() in '{fileName}'"); end.Zero(); }
                    vec = (start + end) * 0.5f;
                    break;
                case VEC.BONEDIR:
                    if (!GetJointTransform(model, frame, joint1, out start, out _)) { common.Warning($"invalid joint {joint1} in bonedir() in '{fileName}'"); start.Zero(); }
                    if (!GetJointTransform(model, frame, joint2, out end, out _)) { common.Warning($"invalid joint {joint2} in bonedir() in '{fileName}'"); end.Zero(); }
                    vec = end - start;
                    break;
                default:
                    vec.Zero();
                    break;
            }

            if (negate) vec = -vec;
            return true;
        }

        public bool Write(VFile f)
        {
            if (negate) f.WriteFloatString("-");
            switch (type)
            {
                case VEC.COORDS: f.WriteFloatString($"( {vec.x}, {vec.y}, {vec.z} )"); break;
                case VEC.JOINT: f.WriteFloatString($"joint( \"{joint1}\" )"); break;
                case VEC.BONECENTER: f.WriteFloatString($"bonecenter( \"{joint1}\", \"{joint2}\" )"); break;
                case VEC.BONEDIR: f.WriteFloatString($"bonedir( \"{joint1}\", \"{joint2}\" )"); break;
                default: break;
            }
            return true;
        }

        public string ToString(int precision = 8)
        {
            var str = type switch
            {
                VEC.COORDS => string.Format($"( {{0:f{precision}}}, {{1:f{precision}}}, {{2:f{precision}}} )", vec.x, vec.y, vec.z),
                VEC.JOINT => $"joint( \"{joint1}\" )",
                VEC.BONECENTER => $"bonecenter( \"{joint1}\", \"{joint2}\" )",
                VEC.BONEDIR => $"bonedir( \"{joint1}\", \"{joint2}\" )",
                _ => string.Empty,
            };
            if (negate) str = "-" + str;
            return str;
        }

        public ref Vector3 ToVec3
            => ref vec;
    }

    public class DeclAF_Body
    {
        public string name;
        public string jointName;
        public DECLAF_JOINTMOD jointMod;
        public TRM modelType;
        public AFVector v1, v2;
        public int numSides;
        public float width;
        public float density;
        public AFVector origin;
        public Angles angles;
        public CONTENTS contents;
        public CONTENTS clipMask;
        public bool selfCollision;
        public Matrix3x3 inertiaScale;
        public float linearFriction;
        public float angularFriction;
        public float contactFriction;
        public string containedJoints;
        public AFVector frictionDirection;
        public AFVector contactMotorDirection;

        public void SetDefault(DeclAF file)
        {
            name = "noname";
            modelType = TRM.BOX;
            v1.type = AFVector.VEC.COORDS; v1.ToVec3.x = v1.ToVec3.y = v1.ToVec3.z = -10f;
            v2.type = AFVector.VEC.COORDS; v2.ToVec3.x = v2.ToVec3.y = v2.ToVec3.z = 10f;
            numSides = 3;
            origin.ToVec3.Zero();
            angles.Zero();
            density = 0.2f;
            inertiaScale.Identity();
            linearFriction = file.defaultLinearFriction;
            angularFriction = file.defaultAngularFriction;
            contactFriction = file.defaultContactFriction;
            contents = file.contents;
            clipMask = file.clipMask;
            selfCollision = file.selfCollision;
            frictionDirection.ToVec3.Zero();
            contactMotorDirection.ToVec3.Zero();
            jointName = "origin";
            jointMod = DECLAF_JOINTMOD.AXIS;
            containedJoints = "*origin";
        }
    }

    public class DeclAF_Constraint
    {
        public string name;
        public string body1;
        public string body2;
        public DECLAF_CONSTRAINT type;
        public float friction;
        public float stretch;
        public float compress;
        public float damping;
        public float restLength;
        public float minLength;
        public float maxLength;
        public AFVector anchor;
        public AFVector anchor2;
        public AFVector[] shaft = new AFVector[2];
        public AFVector axis;
        public enum LIMIT
        {
            NONE = -1,
            CONE,
            PYRAMID
        }
        public LIMIT limit;
        public AFVector limitAxis;
        internal float[] limitAngles = new float[3];

        public void SetDefault(DeclAF file)
        {
            name = "noname";
            type = DECLAF_CONSTRAINT.UNIVERSALJOINT;
            body1 = file.bodies.Count != 0 ? file.bodies[0].name : "world";
            body2 = "world";
            friction = file.defaultConstraintFriction;
            anchor.ToVec3.Zero();
            anchor2.ToVec3.Zero();
            axis.ToVec3.Set(1f, 0f, 0f);
            shaft[0].ToVec3.Set(0f, 0f, -1f);
            shaft[1].ToVec3.Set(0f, 0f, 1f);
            limit = LIMIT.NONE;
            limitAngles[0] = limitAngles[1] = limitAngles[2] = 0f;
            limitAxis.ToVec3.Set(0f, 0f, -1f);
        }
    }

    public class DeclAF : Decl
    {
        public DeclAF()
            => FreeData();

        public override int Size
            => 0;

        public override string DefaultDefinition =>
@"{
	settings {
		model """"
		skin """"
		friction 0.01, 0.01, 0.8, 0.5
		suspendSpeed 20, 30, 40, 60
		noMoveTime 1
		noMoveTranslation 10
		noMoveRotation 10
		minMoveTime -1
		maxMoveTime -1
		totalMass -1
		contents corpse
		clipMask solid, corpse
		selfCollision 1
	}
	body ""body"" {
		joint ""origin""
		mod orientation
		model box( ( -10, -10, -10 ), ( 10, 10, 10 ) )
		origin ( 0, 0, 0 )
		density 0.2
		friction 0.01, 0.01, 0.8
		contents corpse
		clipMask solid, corpse
		selfCollision 1
		containedJoints ""*origin""
	}
}";

        public override void FreeData()
        {
            modified = false;
            defaultLinearFriction = 0.01f;
            defaultAngularFriction = 0.01f;
            defaultContactFriction = 0.8f;
            defaultConstraintFriction = 0.5f;
            totalMass = -1;
            suspendVelocity.Set(20f, 30f);
            suspendAcceleration.Set(40f, 60f);
            noMoveTime = 1f;
            noMoveTranslation = 10f;
            noMoveRotation = 10f;
            minMoveTime = -1f;
            maxMoveTime = -1f;
            selfCollision = true;
            contents = CONTENTS.CORPSE;
            clipMask = CONTENTS.SOLID | CONTENTS.CORPSE;
            bodies.Clear();
            constraints.Clear();
        }

        public virtual void Finish(GetJointTransform GetJointTransform, JointMat frame, object model)
        {
            var name = Name;
            for (var i = 0; i < bodies.Count; i++)
            {
                var body = bodies[i];
                body.v1.Finish(name, GetJointTransform, frame, model);
                body.v2.Finish(name, GetJointTransform, frame, model);
                body.origin.Finish(name, GetJointTransform, frame, model);
                body.frictionDirection.Finish(name, GetJointTransform, frame, model);
                body.contactMotorDirection.Finish(name, GetJointTransform, frame, model);
            }
            for (var i = 0; i < constraints.Count; i++)
            {
                var constraint = constraints[i];
                constraint.anchor.Finish(name, GetJointTransform, frame, model);
                constraint.anchor2.Finish(name, GetJointTransform, frame, model);
                constraint.shaft[0].Finish(name, GetJointTransform, frame, model);
                constraint.shaft[1].Finish(name, GetJointTransform, frame, model);
                constraint.axis.Finish(name, GetJointTransform, frame, model);
                constraint.limitAxis.Finish(name, GetJointTransform, frame, model);
            }
        }

        public bool Save()
        {
            RebuildTextSource();
            ReplaceSourceFileText();
            modified = false;
            return true;
        }

        #region Body

        public void NewBody(string name)
        {
            var body = new DeclAF_Body { name = name };
            body.SetDefault(this);
            bodies.Add(body);
        }

        // rename the body with the given name and rename all constraint body references
        public void RenameBody(string oldName, string newName)
        {
            for (var i = 0; i < bodies.Count; i++)
                if (string.Equals(bodies[i].name, oldName, StringComparison.OrdinalIgnoreCase)) { bodies[i].name = newName; break; }
            for (var i = 0; i < constraints.Count; i++)
                if (string.Equals(constraints[i].body1, oldName, StringComparison.OrdinalIgnoreCase)) constraints[i].body1 = newName;
                else if (string.Equals(constraints[i].body2, oldName, StringComparison.OrdinalIgnoreCase)) constraints[i].body2 = newName;
        }

        // delete the body with the given name and delete all constraints that reference the body
        public void DeleteBody(string name)
        {
            for (var i = 0; i < bodies.Count; i++)
                if (string.Equals(bodies[i].name, name, StringComparison.OrdinalIgnoreCase)) { bodies.RemoveAt(i); break; }
            for (var i = 0; i < constraints.Count; i++)
                if (string.Equals(constraints[i].body1, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(constraints[i].body2, name, StringComparison.OrdinalIgnoreCase)) { constraints.RemoveAt(i); i--; }
        }

        #endregion

        #region Constraint

        public void NewConstraint(string name)
        {
            var constraint = new DeclAF_Constraint { name = name };
            constraint.SetDefault(this);
            constraints.Add(constraint);
        }

        public void RenameConstraint(string oldName, string newName)
        {
            for (var i = 0; i < constraints.Count; i++)
                if (string.Equals(constraints[i].name, oldName, StringComparison.OrdinalIgnoreCase)) { constraints[i].name = newName; return; }
        }
        public void DeleteConstraint(string name)
        {
            for (var i = 0; i < constraints.Count; i++)
                if (string.Equals(constraints[i].name, name, StringComparison.OrdinalIgnoreCase)) { constraints.RemoveAt(i); return; }
        }

        #endregion

        #region Transform

        public static CONTENTS ContentsFromString(string str)
        {
            CONTENTS c = 0;
            Lexer src = new(str, str.Length, "DeclAF::ContentsFromString");
            while (src.ReadToken(out var token))
            {
                if (string.Equals(token, "none", StringComparison.OrdinalIgnoreCase)) c = 0;
                else if (string.Equals(token, "solid", StringComparison.OrdinalIgnoreCase)) c |= CONTENTS.SOLID;
                else if (string.Equals(token, "body", StringComparison.OrdinalIgnoreCase)) c |= CONTENTS.BODY;
                else if (string.Equals(token, "corpse", StringComparison.OrdinalIgnoreCase)) c |= CONTENTS.CORPSE;
                else if (string.Equals(token, "playerclip", StringComparison.OrdinalIgnoreCase)) c |= CONTENTS.PLAYERCLIP;
                else if (string.Equals(token, "monsterclip", StringComparison.OrdinalIgnoreCase)) c |= CONTENTS.MONSTERCLIP;
                else if (token == ",") continue;
                else return c;
            }
            return c;
        }

        public static string ContentsToString(CONTENTS contents)
        {
            var str = string.Empty;
            if ((contents & CONTENTS.SOLID) != 0) { if (str.Length != 0) str += ", "; str += "solid"; }
            if ((contents & CONTENTS.BODY) != 0) { if (str.Length != 0) str += ", "; str += "body"; }
            if ((contents & CONTENTS.CORPSE) != 0) { if (str.Length != 0) str += ", "; str += "corpse"; }
            if ((contents & CONTENTS.PLAYERCLIP) != 0) { if (str.Length != 0) str += ", "; str += "playerclip"; }
            if ((contents & CONTENTS.MONSTERCLIP) != 0) { if (str.Length != 0) str += ", "; str += "monsterclip"; }
            if (str.Length == 0) str = "none";
            return str;
        }

        public static DECLAF_JOINTMOD JointModFromString(string str)
        {
            if (string.Equals(str, "orientation", StringComparison.OrdinalIgnoreCase)) return DECLAF_JOINTMOD.AXIS;
            if (string.Equals(str, "position", StringComparison.OrdinalIgnoreCase)) return DECLAF_JOINTMOD.ORIGIN;
            if (string.Equals(str, "both", StringComparison.OrdinalIgnoreCase)) return DECLAF_JOINTMOD.BOTH;
            return DECLAF_JOINTMOD.AXIS;
        }

        public static string JointModToString(DECLAF_JOINTMOD jointMod)
            => jointMod switch
            {
                DECLAF_JOINTMOD.AXIS => "orientation",
                DECLAF_JOINTMOD.ORIGIN => "position",
                DECLAF_JOINTMOD.BOTH => "both",
                _ => "orientation",
            };

        #endregion

        public bool modified;
        public string model;
        public string skin;
        public float defaultLinearFriction;
        public float defaultAngularFriction;
        public float defaultContactFriction;
        public float defaultConstraintFriction;
        public float totalMass;
        public Vector2 suspendVelocity;
        public Vector2 suspendAcceleration;
        public float noMoveTime;
        public float noMoveTranslation;
        public float noMoveRotation;
        public float minMoveTime;
        public float maxMoveTime;
        public CONTENTS contents;
        public CONTENTS clipMask;
        public bool selfCollision;
        public List<DeclAF_Body> bodies = new();
        public List<DeclAF_Constraint> constraints = new();

        #region Parse

        static bool ParseContents(Lexer src, out CONTENTS c)
        {
            var str = string.Empty;
            while (src.ReadToken(out var token))
            {
                str += token;
                if (!src.CheckTokenString(",")) break;
                str += ",";
            }
            c = ContentsFromString(str);
            return true;
        }

        unsafe bool ParseBody(Lexer src)
        {
            var hasJoint = false;
            AFVector angles = new();
            DeclAF_Body body = new();

            body.SetDefault(this);
            bodies.Add(body);

            if (!src.ExpectTokenType(TT.STRING, 0, out var token) || !src.ExpectTokenString("{"))
                return false;

            body.name = token;
            if (string.Equals(body.name, "origin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(body.name, "world", StringComparison.OrdinalIgnoreCase))
            {
                src.Error("a body may not be named \"origin\" or \"world\"");
                return false;
            }

            while (src.ReadToken(out token))
            {
                if (string.Equals(token, "model", StringComparison.OrdinalIgnoreCase))
                {
                    if (!src.ExpectTokenType(TT.NAME, 0, out token)) return false;
                    if (string.Equals(token, "box", StringComparison.OrdinalIgnoreCase))
                    {
                        body.modelType = TRM.BOX;
                        if (!src.ExpectTokenString("(") || !body.v1.Parse(src) || !src.ExpectTokenString(",") || !body.v2.Parse(src) || !src.ExpectTokenString(")")) return false;
                    }
                    else if (string.Equals(token, "octahedron", StringComparison.OrdinalIgnoreCase))
                    {
                        body.modelType = TRM.OCTAHEDRON;
                        if (!src.ExpectTokenString("(") || !body.v1.Parse(src) || !src.ExpectTokenString(",") || !body.v2.Parse(src) || !src.ExpectTokenString(")")) return false;
                    }
                    else if (string.Equals(token, "dodecahedron", StringComparison.OrdinalIgnoreCase))
                    {
                        body.modelType = TRM.DODECAHEDRON;
                        if (!src.ExpectTokenString("(") || !body.v1.Parse(src) || !src.ExpectTokenString(",") || !body.v2.Parse(src) || !src.ExpectTokenString(")")) return false;
                    }
                    else if (string.Equals(token, "cylinder", StringComparison.OrdinalIgnoreCase))
                    {
                        body.modelType = TRM.CYLINDER;
                        if (!src.ExpectTokenString("(") || !body.v1.Parse(src) || !src.ExpectTokenString(",") || !body.v2.Parse(src) || !src.ExpectTokenString(",")) return false;
                        body.numSides = src.ParseInt(); if (!src.ExpectTokenString(")")) return false;
                    }
                    else if (string.Equals(token, "cone", StringComparison.OrdinalIgnoreCase))
                    {
                        body.modelType = TRM.CONE;
                        if (!src.ExpectTokenString("(") || !body.v1.Parse(src) || !src.ExpectTokenString(",") || !body.v2.Parse(src) || !src.ExpectTokenString(",")) return false;
                        body.numSides = src.ParseInt(); if (!src.ExpectTokenString(")")) return false;
                    }
                    else if (string.Equals(token, "bone", StringComparison.OrdinalIgnoreCase))
                    {
                        body.modelType = TRM.BONE;
                        if (!src.ExpectTokenString("(") || !body.v1.Parse(src) || !src.ExpectTokenString(",") || !body.v2.Parse(src) || !src.ExpectTokenString(",")) return false;
                        body.width = src.ParseFloat(); if (!src.ExpectTokenString(")")) return false;
                    }
                    else if (string.Equals(token, "custom", StringComparison.OrdinalIgnoreCase)) { src.Error("custom models not yet implemented"); return false; }
                    else { src.Error($"unknown model type {token}"); return false; }
                }
                else if (string.Equals(token, "origin", StringComparison.OrdinalIgnoreCase)) { if (!body.origin.Parse(src)) return false; }
                else if (string.Equals(token, "angles", StringComparison.OrdinalIgnoreCase)) { if (!angles.Parse(src)) return false; body.angles = new Angles(angles.ToVec3.x, angles.ToVec3.y, angles.ToVec3.z); }
                else if (string.Equals(token, "joint", StringComparison.OrdinalIgnoreCase)) { if (!src.ExpectTokenType(TT.STRING, 0, out token)) return false; body.jointName = token; hasJoint = true; }
                else if (string.Equals(token, "mod", StringComparison.OrdinalIgnoreCase)) { if (!src.ExpectAnyToken(out token)) return false; body.jointMod = JointModFromString(token); }
                else if (string.Equals(token, "density", StringComparison.OrdinalIgnoreCase)) body.density = src.ParseFloat();
                else if (string.Equals(token, "inertiaScale", StringComparison.OrdinalIgnoreCase)) fixed (float* _ = &body.inertiaScale[0].x) src.Parse1DMatrix(9, _);
                else if (string.Equals(token, "friction", StringComparison.OrdinalIgnoreCase))
                {
                    body.linearFriction = src.ParseFloat(); src.ExpectTokenString(",");
                    body.angularFriction = src.ParseFloat(); src.ExpectTokenString(",");
                    body.contactFriction = src.ParseFloat();
                }
                else if (string.Equals(token, "contents", StringComparison.OrdinalIgnoreCase)) ParseContents(src, out body.contents);
                else if (string.Equals(token, "clipMask", StringComparison.OrdinalIgnoreCase)) ParseContents(src, out body.clipMask);
                else if (string.Equals(token, "selfCollision", StringComparison.OrdinalIgnoreCase)) body.selfCollision = src.ParseBool();
                else if (string.Equals(token, "containedjoints", StringComparison.OrdinalIgnoreCase)) { if (!src.ExpectTokenType(TT.STRING, 0, out token)) return false; body.containedJoints = token; }
                else if (string.Equals(token, "frictionDirection", StringComparison.OrdinalIgnoreCase)) { if (!body.frictionDirection.Parse(src)) return false; }
                else if (string.Equals(token, "contactMotorDirection", StringComparison.OrdinalIgnoreCase)) { if (!body.contactMotorDirection.Parse(src)) return false; }
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in body"); return false; }
            }

            if (body.modelType == TRM.INVALID) { src.Error("no model set for body"); return false; }
            if (!hasJoint) { src.Error("no joint set for body"); return false; }

            body.clipMask |= CONTENTS.MOVEABLECLIP;

            return true;
        }

        bool ParseFixed(Lexer src)
        {
            DeclAF_Constraint constraint = new();

            constraint.SetDefault(this);
            constraints.Add(constraint);

            if (!src.ExpectTokenType(TT.STRING, 0, out var token) || !src.ExpectTokenString("{"))
                return false;

            constraint.type = DECLAF_CONSTRAINT.FIXED;
            constraint.name = token;

            while (src.ReadToken(out token))
            {
                if (string.Equals(token, "body1", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body1 = token; }
                else if (string.Equals(token, "body2", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body2 = token; }
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in ball and socket joint"); return false; }
            }

            return true;
        }

        bool ParseBallAndSocketJoint(Lexer src)
        {
            DeclAF_Constraint constraint = new();

            constraint.SetDefault(this);
            constraints.Add(constraint);

            if (!src.ExpectTokenType(TT.STRING, 0, out var token) || !src.ExpectTokenString("{"))
                return false;

            constraint.type = DECLAF_CONSTRAINT.BALLANDSOCKETJOINT;
            constraint.limit = DeclAF_Constraint.LIMIT.NONE;
            constraint.name = token;
            constraint.friction = 0.5f;
            constraint.anchor.ToVec3.Zero();
            constraint.shaft[0].ToVec3.Zero();

            while (src.ReadToken(out token))
            {
                if (string.Equals(token, "body1", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body1 = token; }
                else if (string.Equals(token, "body2", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body2 = token; }
                else if (string.Equals(token, "anchor", StringComparison.OrdinalIgnoreCase)) { if (!constraint.anchor.Parse(src)) return false; }
                else if (string.Equals(token, "conelimit", StringComparison.OrdinalIgnoreCase))
                {
                    if (!constraint.limitAxis.Parse(src) || !src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[0] = src.ParseFloat(); if (!src.ExpectTokenString(",") || !constraint.shaft[0].Parse(src)) return false;
                    constraint.limit = DeclAF_Constraint.LIMIT.CONE;
                }
                else if (string.Equals(token, "pyramidlimit", StringComparison.OrdinalIgnoreCase))
                {
                    if (!constraint.limitAxis.Parse(src) || !src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[0] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[1] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[2] = src.ParseFloat(); if (!src.ExpectTokenString(",") || !constraint.shaft[0].Parse(src)) return false;
                    constraint.limit = DeclAF_Constraint.LIMIT.PYRAMID;
                }
                else if (string.Equals(token, "friction", StringComparison.OrdinalIgnoreCase)) constraint.friction = src.ParseFloat();
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in ball and socket joint"); return false; }
            }

            return true;
        }

        bool ParseUniversalJoint(Lexer src)
        {
            DeclAF_Constraint constraint = new();

            constraint.SetDefault(this);
            constraints.Add(constraint);

            if (!src.ExpectTokenType(TT.STRING, 0, out var token) || !src.ExpectTokenString("{"))
                return false;

            constraint.type = DECLAF_CONSTRAINT.UNIVERSALJOINT;
            constraint.limit = DeclAF_Constraint.LIMIT.NONE;
            constraint.name = token;
            constraint.friction = 0.5f;
            constraint.anchor.ToVec3.Zero();
            constraint.shaft[0].ToVec3.Zero();
            constraint.shaft[1].ToVec3.Zero();

            while (src.ReadToken(out token))
            {
                if (string.Equals(token, "body1", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body1 = token; }
                else if (string.Equals(token, "body2", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body2 = token; }
                else if (string.Equals(token, "anchor", StringComparison.OrdinalIgnoreCase)) { if (!constraint.anchor.Parse(src)) return false; }
                else if (string.Equals(token, "shafts", StringComparison.OrdinalIgnoreCase)) { if (!constraint.shaft[0].Parse(src) || !src.ExpectTokenString(",") || !constraint.shaft[1].Parse(src)) return false; }
                else if (string.Equals(token, "conelimit", StringComparison.OrdinalIgnoreCase))
                {
                    if (!constraint.limitAxis.Parse(src) || !src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[0] = src.ParseFloat();
                    constraint.limit = DeclAF_Constraint.LIMIT.CONE;
                }
                else if (string.Equals(token, "pyramidlimit", StringComparison.OrdinalIgnoreCase))
                {
                    if (!constraint.limitAxis.Parse(src) || !src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[0] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[1] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[2] = src.ParseFloat();
                    constraint.limit = DeclAF_Constraint.LIMIT.PYRAMID;
                }
                else if (string.Equals(token, "friction", StringComparison.OrdinalIgnoreCase)) constraint.friction = src.ParseFloat();
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in universal joint"); return false; }
            }

            return true;
        }

        bool ParseHinge(Lexer src)
        {
            DeclAF_Constraint constraint = new();

            constraint.SetDefault(this);
            constraints.Add(constraint);

            if (!src.ExpectTokenType(TT.STRING, 0, out var token) || !src.ExpectTokenString("{"))
                return false;

            constraint.type = DECLAF_CONSTRAINT.HINGE;
            constraint.limit = DeclAF_Constraint.LIMIT.NONE;
            constraint.name = token;
            constraint.friction = 0.5f;
            constraint.anchor.ToVec3.Zero();
            constraint.axis.ToVec3.Zero();

            while (src.ReadToken(out token))
            {
                if (string.Equals(token, "body1", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body1 = token; }
                else if (string.Equals(token, "body2", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body2 = token; }
                else if (string.Equals(token, "anchor", StringComparison.OrdinalIgnoreCase)) { if (!constraint.anchor.Parse(src)) return false; }
                else if (string.Equals(token, "axis", StringComparison.OrdinalIgnoreCase)) { if (!constraint.axis.Parse(src)) return false; }
                else if (string.Equals(token, "limit", StringComparison.OrdinalIgnoreCase))
                {
                    constraint.limitAngles[0] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[1] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    constraint.limitAngles[2] = src.ParseFloat();
                    constraint.limit = DeclAF_Constraint.LIMIT.CONE;
                }
                else if (string.Equals(token, "friction", StringComparison.OrdinalIgnoreCase)) constraint.friction = src.ParseFloat();
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in hinge"); return false; }
            }

            return true;
        }

        bool ParseSlider(Lexer src)
        {
            DeclAF_Constraint constraint = new();

            constraint.SetDefault(this);
            constraints.Add(constraint);

            if (!src.ExpectTokenType(TT.STRING, 0, out var token) || !src.ExpectTokenString("{"))
                return false;

            constraint.type = DECLAF_CONSTRAINT.SLIDER;
            constraint.limit = DeclAF_Constraint.LIMIT.NONE;
            constraint.name = token;
            constraint.friction = 0.5f;

            while (src.ReadToken(out token))
            {
                if (string.Equals(token, "body1", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body1 = token; }
                else if (string.Equals(token, "body2", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body2 = token; }
                else if (string.Equals(token, "axis", StringComparison.OrdinalIgnoreCase)) { if (!constraint.axis.Parse(src)) return false; }
                else if (string.Equals(token, "friction", StringComparison.OrdinalIgnoreCase)) constraint.friction = src.ParseFloat();
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in slider"); return false; }
            }

            return true;
        }

        bool ParseSpring(Lexer src)
        {
            DeclAF_Constraint constraint = new();

            constraint.SetDefault(this);
            constraints.Add(constraint);

            if (!src.ExpectTokenType(TT.STRING, 0, out var token) || !src.ExpectTokenString("{"))
                return false;

            constraint.type = DECLAF_CONSTRAINT.SPRING;
            constraint.limit = DeclAF_Constraint.LIMIT.NONE;
            constraint.name = token;
            constraint.friction = 0.5f;

            while (src.ReadToken(out token))
            {
                if (string.Equals(token, "body1", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body1 = token; }
                else if (string.Equals(token, "body2", StringComparison.OrdinalIgnoreCase)) { src.ExpectTokenType(TT.STRING, 0, out token); constraint.body2 = token; }
                else if (string.Equals(token, "anchor1", StringComparison.OrdinalIgnoreCase)) { if (!constraint.anchor.Parse(src)) return false; }
                else if (string.Equals(token, "anchor2", StringComparison.OrdinalIgnoreCase)) { if (!constraint.anchor2.Parse(src)) return false; }
                else if (string.Equals(token, "friction", StringComparison.OrdinalIgnoreCase)) constraint.friction = src.ParseFloat();
                else if (string.Equals(token, "stretch", StringComparison.OrdinalIgnoreCase)) constraint.stretch = src.ParseFloat();
                else if (string.Equals(token, "compress", StringComparison.OrdinalIgnoreCase)) constraint.compress = src.ParseFloat();
                else if (string.Equals(token, "damping", StringComparison.OrdinalIgnoreCase)) constraint.damping = src.ParseFloat();
                else if (string.Equals(token, "restLength", StringComparison.OrdinalIgnoreCase)) constraint.restLength = src.ParseFloat();
                else if (string.Equals(token, "minLength", StringComparison.OrdinalIgnoreCase)) constraint.minLength = src.ParseFloat();
                else if (string.Equals(token, "maxLength", StringComparison.OrdinalIgnoreCase)) constraint.maxLength = src.ParseFloat();
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in spring"); return false; }
            }

            return true;
        }

        bool ParseSettings(Lexer src)
        {
            if (!src.ExpectTokenString("{"))
                return false;

            while (src.ReadToken(out var token))
            {
                if (string.Equals(token, "mesh", StringComparison.OrdinalIgnoreCase)) { if (!src.ExpectTokenType(TT.STRING, 0, out token)) return false; }
                else if (string.Equals(token, "anim", StringComparison.OrdinalIgnoreCase)) { if (!src.ExpectTokenType(TT.STRING, 0, out token)) return false; }
                else if (string.Equals(token, "model", StringComparison.OrdinalIgnoreCase)) { if (!src.ExpectTokenType(TT.STRING, 0, out token)) return false; model = token; }
                else if (string.Equals(token, "skin", StringComparison.OrdinalIgnoreCase)) { if (!src.ExpectTokenType(TT.STRING, 0, out token)) return false; skin = token; }
                else if (string.Equals(token, "friction", StringComparison.OrdinalIgnoreCase))
                {
                    defaultLinearFriction = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    defaultAngularFriction = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    defaultContactFriction = src.ParseFloat(); if (src.CheckTokenString(",")) defaultConstraintFriction = src.ParseFloat();
                }
                else if (string.Equals(token, "totalMass", StringComparison.OrdinalIgnoreCase)) totalMass = src.ParseFloat();
                else if (string.Equals(token, "suspendSpeed", StringComparison.OrdinalIgnoreCase))
                {
                    suspendVelocity[0] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    suspendVelocity[1] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    suspendAcceleration[0] = src.ParseFloat(); if (!src.ExpectTokenString(",")) return false;
                    suspendAcceleration[1] = src.ParseFloat();
                }
                else if (string.Equals(token, "noMoveTime", StringComparison.OrdinalIgnoreCase)) noMoveTime = src.ParseFloat();
                else if (string.Equals(token, "noMoveTranslation", StringComparison.OrdinalIgnoreCase)) noMoveTranslation = src.ParseFloat();
                else if (string.Equals(token, "noMoveRotation", StringComparison.OrdinalIgnoreCase)) noMoveRotation = src.ParseFloat();
                else if (string.Equals(token, "minMoveTime", StringComparison.OrdinalIgnoreCase)) minMoveTime = src.ParseFloat();
                else if (string.Equals(token, "maxMoveTime", StringComparison.OrdinalIgnoreCase)) maxMoveTime = src.ParseFloat();
                else if (string.Equals(token, "contents", StringComparison.OrdinalIgnoreCase)) ParseContents(src, out contents);
                else if (string.Equals(token, "clipMask", StringComparison.OrdinalIgnoreCase)) ParseContents(src, out clipMask);
                else if (string.Equals(token, "selfCollision", StringComparison.OrdinalIgnoreCase)) selfCollision = src.ParseBool();
                else if (token == "}") break;
                else { src.Error($"unknown token {token} in settings"); return false; }
            }

            return true;
        }

        public override bool Parse(string text)
        {
            int i, j;

            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");

            while (src.ReadToken(out var token))
            {
                if (string.Equals(token, "settings", StringComparison.OrdinalIgnoreCase)) { if (!ParseSettings(src)) return false; }
                else if (string.Equals(token, "body", StringComparison.OrdinalIgnoreCase)) { if (!ParseBody(src)) return false; }
                else if (string.Equals(token, "fixed", StringComparison.OrdinalIgnoreCase)) { if (!ParseFixed(src)) return false; }
                else if (string.Equals(token, "ballAndSocketJoint", StringComparison.OrdinalIgnoreCase)) { if (!ParseBallAndSocketJoint(src)) return false; }
                else if (string.Equals(token, "universalJoint", StringComparison.OrdinalIgnoreCase)) { if (!ParseUniversalJoint(src)) return false; }
                else if (string.Equals(token, "hinge", StringComparison.OrdinalIgnoreCase)) { if (!ParseHinge(src)) return false; }
                else if (string.Equals(token, "slider", StringComparison.OrdinalIgnoreCase)) { if (!ParseSlider(src)) return false; }
                else if (string.Equals(token, "spring", StringComparison.OrdinalIgnoreCase)) { if (!ParseSpring(src)) return false; }
                else if (token == "}") break;
                else { src.Error($"unknown keyword {token}"); return false; }
            }

            for (i = 0; i < bodies.Count; i++)
                // check for multiple bodies with the same name
                for (j = i + 1; j < bodies.Count; j++)
                    if (bodies[i].name == bodies[j].name)
                        src.Error($"two bodies with the same name \"{bodies[i].name}\"");

            for (i = 0; i < constraints.Count; i++)
            {
                // check for multiple constraints with the same name
                for (j = i + 1; j < constraints.Count; j++)
                    if (constraints[i].name == constraints[j].name)
                        src.Error($"two constraints with the same name \"{constraints[i].name}\"");
                // check if there are two valid bodies set
                if (constraints[i].body1 == "")
                    src.Error($"no valid body1 specified for constraint '{constraints[i].name}'");
                if (constraints[i].body2 == "")
                    src.Error($"no valid body2 specified for constraint '{constraints[i].name}'");
            }

            // make sure the body which modifies the origin comes first
            for (i = 0; i < bodies.Count; i++)
                if (bodies[i].jointName == "origin")
                {
                    if (i != 0) { var b = bodies[0]; bodies[0] = bodies[i]; bodies[i] = b; }
                    break;
                }

            return true;
        }

        #endregion

        #region Write

        bool WriteBody(VFile f, DeclAF_Body body)
        {
            f.WriteFloatString($"\nbody \"{body.name}\" {{\n");
            f.WriteFloatString($"\tjoint \"{body.jointName}\"\n");
            f.WriteFloatString($"\tmod {JointModToString(body.jointMod)}\n");
            switch (body.modelType)
            {
                case TRM.BOX:
                    {
                        f.WriteFloatString("\tmodel box( ");
                        body.v1.Write(f); f.WriteFloatString(", ");
                        body.v2.Write(f);
                        f.WriteFloatString(" )\n");
                        break;
                    }
                case TRM.OCTAHEDRON:
                    {
                        f.WriteFloatString("\tmodel octahedron( ");
                        body.v1.Write(f); f.WriteFloatString(", ");
                        body.v2.Write(f);
                        f.WriteFloatString(" )\n");
                        break;
                    }
                case TRM.DODECAHEDRON:
                    {
                        f.WriteFloatString("\tmodel dodecahedron( ");
                        body.v1.Write(f); f.WriteFloatString(", ");
                        body.v2.Write(f);
                        f.WriteFloatString(" )\n");
                        break;
                    }
                case TRM.CYLINDER:
                    {
                        f.WriteFloatString("\tmodel cylinder( ");
                        body.v1.Write(f); f.WriteFloatString(", ");
                        body.v2.Write(f);
                        f.WriteFloatString($", {body.numSides} )\n");
                        break;
                    }
                case TRM.CONE:
                    {
                        f.WriteFloatString("\tmodel cone( ");
                        body.v1.Write(f); f.WriteFloatString(", ");
                        body.v2.Write(f);
                        f.WriteFloatString($", {body.numSides} )\n");
                        break;
                    }
                case TRM.BONE:
                    {
                        f.WriteFloatString("\tmodel bone( ");
                        body.v1.Write(f); f.WriteFloatString(", ");
                        body.v2.Write(f);
                        f.WriteFloatString($", {body.width} )\n");
                        break;
                    }
                default:
                    Debug.Assert(false);
                    break;
            }
            f.WriteFloatString("\torigin ");
            body.origin.Write(f);
            f.WriteFloatString("\n");
            if (body.angles != Angles.zero)
                f.WriteFloatString($"\tangles ( {body.angles.pitch}, {body.angles.yaw}, {body.angles.roll} )\n");
            f.WriteFloatString($"\tdensity {body.density}\n");
            if (body.inertiaScale != Matrix3x3.identity)
            {
                var ic = body.inertiaScale;
                f.WriteFloatString($"\tinertiaScale ({ic[0].x} {ic[0].y} {ic[0].z} {ic[1].x} {ic[1].y} {ic[1].z} {ic[2].x} {ic[2].y} {ic[2].z})\n");
            }
            if (body.linearFriction != -1)
                f.WriteFloatString($"\tfriction {body.linearFriction}, {body.angularFriction}, {body.contactFriction}\n");
            f.WriteFloatString($"\tcontents {ContentsToString(body.contents)}\n");
            f.WriteFloatString($"\tclipMask {ContentsToString(body.clipMask)}\n");
            f.WriteFloatString($"\tselfCollision {body.selfCollision}\n");
            if (body.frictionDirection.ToVec3 != Vector3.origin)
            {
                f.WriteFloatString("\tfrictionDirection "); body.frictionDirection.Write(f); f.WriteFloatString("\n");
            }
            if (body.contactMotorDirection.ToVec3 != Vector3.origin)
            {
                f.WriteFloatString("\tcontactMotorDirection "); body.contactMotorDirection.Write(f); f.WriteFloatString("\n");
            }
            f.WriteFloatString($"\tcontainedJoints \"{body.containedJoints}\"\n");
            f.WriteFloatString("}\n");
            return true;
        }

        bool WriteFixed(VFile f, DeclAF_Constraint c)
        {
            f.WriteFloatString($"\nfixed \"{c.name}\" {{\n");
            f.WriteFloatString($"\tbody1 \"{c.body1}\"\n");
            f.WriteFloatString($"\tbody2 \"{c.body2}\"\n");
            f.WriteFloatString("}\n");
            return true;
        }

        bool WriteBallAndSocketJoint(VFile f, DeclAF_Constraint c)
        {
            f.WriteFloatString($"\nballAndSocketJoint \"{c.name}\" {{\n");
            f.WriteFloatString($"\tbody1 \"{c.body1}\"\n");
            f.WriteFloatString($"\tbody2 \"{c.body2}\"\n");
            f.WriteFloatString("\tanchor ");
            c.anchor.Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString($"\tfriction {c.friction}\n");
            if (c.limit == DeclAF_Constraint.LIMIT.CONE)
            {
                f.WriteFloatString("\tconeLimit ");
                c.limitAxis.Write(f);
                f.WriteFloatString($", {c.limitAngles[0]}, ");
                c.shaft[0].Write(f);
                f.WriteFloatString("\n");
            }
            else if (c.limit == DeclAF_Constraint.LIMIT.PYRAMID)
            {
                f.WriteFloatString("\tpyramidLimit ");
                c.limitAxis.Write(f);
                f.WriteFloatString($", {c.limitAngles[0]}, {c.limitAngles[1]}, {c.limitAngles[2]}, ");
                c.shaft[0].Write(f);
                f.WriteFloatString("\n");
            }
            f.WriteFloatString("}\n");
            return true;
        }

        bool WriteUniversalJoint(VFile f, DeclAF_Constraint c)
        {
            f.WriteFloatString($"\nuniversalJoint \"{c.name}\" {{\n");
            f.WriteFloatString($"\tbody1 \"{c.body1}\"\n");
            f.WriteFloatString($"\tbody2 \"{c.body2}\"\n");
            f.WriteFloatString("\tanchor ");
            c.anchor.Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString("\tshafts ");
            c.shaft[0].Write(f);
            f.WriteFloatString(", ");
            c.shaft[1].Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString($"\tfriction {c.friction}\n");
            if (c.limit == DeclAF_Constraint.LIMIT.CONE)
            {
                f.WriteFloatString("\tconeLimit ");
                c.limitAxis.Write(f);
                f.WriteFloatString($", {c.limitAngles[0]}\n");
            }
            else if (c.limit == DeclAF_Constraint.LIMIT.PYRAMID)
            {
                f.WriteFloatString("\tpyramidLimit ");
                c.limitAxis.Write(f);
                f.WriteFloatString($", {c.limitAngles[0]}, {c.limitAngles[1]}, {c.limitAngles[2]}\n");
            }
            f.WriteFloatString("}\n");
            return true;
        }

        bool WriteHinge(VFile f, DeclAF_Constraint c)
        {
            f.WriteFloatString($"\nhinge \"{c.name}\" {{\n");
            f.WriteFloatString($"\tbody1 \"{c.body1}\"\n");
            f.WriteFloatString($"\tbody2 \"{c.body2}\"\n");
            f.WriteFloatString("\tanchor ");
            c.anchor.Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString("\taxis ");
            c.axis.Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString($"\tfriction {c.friction}\n");
            if (c.limit == DeclAF_Constraint.LIMIT.CONE)
            {
                f.WriteFloatString("\tlimit ");
                f.WriteFloatString($"{c.limitAngles[0]}, {c.limitAngles[1]}, {c.limitAngles[2]}");
                f.WriteFloatString("\n");
            }
            f.WriteFloatString("}\n");
            return true;
        }

        bool WriteSlider(VFile f, DeclAF_Constraint c)
        {
            f.WriteFloatString($"\nslider \"{c.name}\" {{\n");
            f.WriteFloatString($"\tbody1 \"{c.body1}\"\n");
            f.WriteFloatString($"\tbody2 \"{c.body2}\"\n");
            f.WriteFloatString("\taxis ");
            c.axis.Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString($"\tfriction {c.friction}\n");
            f.WriteFloatString("}\n");
            return true;
        }

        bool WriteSpring(VFile f, DeclAF_Constraint c)
        {
            f.WriteFloatString($"\nspring \"{c.name}\" {{\n");
            f.WriteFloatString($"\tbody1 \"{c.body1}\"\n");
            f.WriteFloatString($"\tbody2 \"{c.body2}\"\n");
            f.WriteFloatString("\tanchor1 ");
            c.anchor.Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString("\tanchor2 ");
            c.anchor2.Write(f);
            f.WriteFloatString("\n");
            f.WriteFloatString($"\tfriction {c.friction}\n");
            f.WriteFloatString($"\tstretch {c.stretch}\n");
            f.WriteFloatString($"\tcompress {c.compress}\n");
            f.WriteFloatString($"\tdamping {c.damping}\n");
            f.WriteFloatString($"\trestLength {c.restLength}\n");
            f.WriteFloatString($"\tminLength {c.minLength}\n");
            f.WriteFloatString($"\tmaxLength {c.maxLength}\n");
            f.WriteFloatString("}\n");
            return true;
        }

        bool WriteConstraint(VFile f, DeclAF_Constraint c)
            => c.type switch
            {
                DECLAF_CONSTRAINT.FIXED => WriteFixed(f, c),
                DECLAF_CONSTRAINT.BALLANDSOCKETJOINT => WriteBallAndSocketJoint(f, c),
                DECLAF_CONSTRAINT.UNIVERSALJOINT => WriteUniversalJoint(f, c),
                DECLAF_CONSTRAINT.HINGE => WriteHinge(f, c),
                DECLAF_CONSTRAINT.SLIDER => WriteSlider(f, c),
                DECLAF_CONSTRAINT.SPRING => WriteSpring(f, c),
                _ => false,
            };

        bool WriteSettings(VFile f)
        {
            f.WriteFloatString("\nsettings {\n");
            f.WriteFloatString($"\tmodel \"{model}\"\n");
            f.WriteFloatString($"\tskin \"{skin}\"\n");
            f.WriteFloatString($"\tfriction {defaultLinearFriction}, {defaultAngularFriction}, {defaultContactFriction}, {defaultConstraintFriction}\n");
            f.WriteFloatString($"\tsuspendSpeed {suspendVelocity[0]}, {suspendVelocity[1]}, {suspendAcceleration[0]}, {suspendAcceleration[1]}\n");
            f.WriteFloatString($"\tnoMoveTime {noMoveTime}\n");
            f.WriteFloatString($"\tnoMoveTranslation {noMoveTranslation}\n");
            f.WriteFloatString($"\tnoMoveRotation {noMoveRotation}\n");
            f.WriteFloatString($"\tminMoveTime %f\n", minMoveTime);
            f.WriteFloatString($"\tmaxMoveTime %f\n", maxMoveTime);
            f.WriteFloatString($"\ttotalMass %f\n", totalMass);
            f.WriteFloatString($"\tcontents %s\n", ContentsToString(contents));
            f.WriteFloatString($"\tclipMask %s\n", ContentsToString(clipMask));
            f.WriteFloatString($"\tselfCollision %d\n", selfCollision);
            f.WriteFloatString("}\n");
            return true;
        }

        #endregion

        bool RebuildTextSource()
        {
            int i;
            VFile_Memory f = new();

            f.WriteFloatString(
@"
/*
Generated by the Articulated Figure Editor.
Do not edit directly but launch the game and type 'editAFs' on the console.
*/
");

            f.WriteFloatString($"\narticulatedFigure {Name} {{\n");

            if (!WriteSettings(f))
                return false;

            for (i = 0; i < bodies.Count; i++)
                if (!WriteBody(f, bodies[i]))
                    return false;

            for (i = 0; i < constraints.Count; i++)
                if (!WriteConstraint(f, constraints[i]))
                    return false;

            f.WriteFloatString("\n}");

            Text = Encoding.ASCII.GetString(f.DataPtr);

            return true;
        }
    }
}
