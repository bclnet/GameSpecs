using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using System.Text;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public class ParticleParm
    {
        public DeclTable table;
        public float from;
        public float to;

        public float Eval(float frac, RandomX rand)
            => table != null ? table.TableLookup(frac) : from + frac * (to - from);

        public float Integrate(float frac, RandomX rand)
        {
            if (table != null) { common.Printf("ParticleParm::Integrate: can't integrate tables\n"); return 0; }
            return (from + frac * (to - from) * 0.5f) * frac;
        }
    }

    public struct ParticleGen
    {
        public RenderEntity renderEnt;          // for shaderParms, etc
        public RenderView renderView;
        public int index;               // particle number in the system
        public float frac;              // 0.0 to 1.0
        public RandomX random;
        public Vector3 origin;              // dynamic smoke particles can have individual origins and axis
        public Matrix3x3 axis;

        public float age;               // in seconds, calculated as fraction * stage.particleLife
        public RandomX originalRandom;     // needed so aimed particles can reset the random for another origin calculation
        public float animationFrameFrac;    // set by ParticleTexCoords, used to make the cross faded version
    }

    // single particle stage
    public class ParticleStage
    {
        public enum PDIST
        {
            RECT,             // ( sizeX sizeY sizeZ )
            CYLINDER,         // ( sizeX sizeY sizeZ )
            SPHERE            // ( sizeX sizeY sizeZ ringFraction ) a ringFraction of zero allows the entire sphere, 0.9 would only allow the outer 10% of the sphere
        }

        public enum PDIR
        {
            CONE,              // parm0 is the solid cone angle
            OUTWARD            // direction is relative to offset from origin, parm0 is an upward bias
        }

        public enum PPATH
        {
            STANDARD,
            HELIX,            // ( sizeX sizeY sizeZ radialSpeed climbSpeed )
            FLIES,
            ORBIT,
            DRIP
        }

        public enum POR
        {
            VIEW,
            AIMED,              // angle and aspect are disregarded
            X,
            Y,
            Z
        }

        public ParticleStage(ParticleStage src)
        {
            material = src.material;
            totalParticles = src.totalParticles;
            cycles = src.cycles;
            cycleMsec = src.cycleMsec;
            spawnBunching = src.spawnBunching;
            particleLife = src.particleLife;
            timeOffset = src.timeOffset;
            deadTime = src.deadTime;
            distributionType = src.distributionType;
            distributionParms[0] = src.distributionParms[0];
            distributionParms[1] = src.distributionParms[1];
            distributionParms[2] = src.distributionParms[2];
            distributionParms[3] = src.distributionParms[3];
            directionType = src.directionType;
            directionParms[0] = src.directionParms[0];
            directionParms[1] = src.directionParms[1];
            directionParms[2] = src.directionParms[2];
            directionParms[3] = src.directionParms[3];
            speed = src.speed;
            gravity = src.gravity;
            worldGravity = src.worldGravity;
            randomDistribution = src.randomDistribution;
            entityColor = src.entityColor;
            customPathType = src.customPathType;
            customPathParms[0] = src.customPathParms[0];
            customPathParms[1] = src.customPathParms[1];
            customPathParms[2] = src.customPathParms[2];
            customPathParms[3] = src.customPathParms[3];
            customPathParms[4] = src.customPathParms[4];
            customPathParms[5] = src.customPathParms[5];
            customPathParms[6] = src.customPathParms[6];
            customPathParms[7] = src.customPathParms[7];
            offset = src.offset;
            animationFrames = src.animationFrames;
            animationRate = src.animationRate;
            initialAngle = src.initialAngle;
            rotationSpeed = src.rotationSpeed;
            orientation = src.orientation;
            orientationParms[0] = src.orientationParms[0];
            orientationParms[1] = src.orientationParms[1];
            orientationParms[2] = src.orientationParms[2];
            orientationParms[3] = src.orientationParms[3];
            size = src.size;
            aspect = src.aspect;
            color = src.color;
            fadeColor = src.fadeColor;
            fadeInFraction = src.fadeInFraction;
            fadeOutFraction = src.fadeOutFraction;
            fadeIndexFraction = src.fadeIndexFraction;
            hidden = src.hidden;
            boundsExpansion = src.boundsExpansion;
            bounds = src.bounds;
        }

        public ParticleStage()
        {
            material = null;
            totalParticles = 0;
            cycles = 0f;
            cycleMsec = 0;
            spawnBunching = 0f;
            particleLife = 0f;
            timeOffset = 0f;
            deadTime = 0f;
            distributionType = PDIST.RECT;
            distributionParms[0] = distributionParms[1] = distributionParms[2] = distributionParms[3] = 0f;
            directionType = PDIR.CONE;
            directionParms[0] = directionParms[1] = directionParms[2] = directionParms[3] = 0f;
            // idParticleParm		speed;
            gravity = 0f;
            worldGravity = false;
            customPathType = PPATH.STANDARD;
            customPathParms[0] = customPathParms[1] = customPathParms[2] = customPathParms[3] = 0f;
            customPathParms[4] = customPathParms[5] = customPathParms[6] = customPathParms[7] = 0f;
            offset.Zero();
            animationFrames = 0;
            animationRate = 0f;
            randomDistribution = true;
            entityColor = false;
            initialAngle = 0f;
            // idParticleParm		rotationSpeed;
            orientation = POR.VIEW;
            orientationParms[0] = orientationParms[1] = orientationParms[2] = orientationParms[3] = 0f;
            // idParticleParm		size
            // idParticleParm		aspect
            color.Zero();
            fadeColor.Zero();
            fadeInFraction = 0f;
            fadeOutFraction = 0f;
            fadeIndexFraction = 0f;
            hidden = false;
            boundsExpansion = 0f;
            bounds.Clear();
        }

        // Sets the stage to a default state
        public void Default()
        {
            material = declManager.FindMaterial("_default");
            totalParticles = 100;
            spawnBunching = 1f;
            particleLife = 1.5f;
            timeOffset = 0f;
            deadTime = 0f;
            distributionType = PDIST.RECT;
            distributionParms[0] = 8f;
            distributionParms[1] = 8f;
            distributionParms[2] = 8f;
            distributionParms[3] = 0f;
            directionType = PDIR.CONE;
            directionParms[0] = 90f;
            directionParms[1] = 0f;
            directionParms[2] = 0f;
            directionParms[3] = 0f;
            orientation = POR.VIEW;
            orientationParms[0] = 0f;
            orientationParms[1] = 0f;
            orientationParms[2] = 0f;
            orientationParms[3] = 0f;
            speed.from = 150f;
            speed.to = 150f;
            speed.table = null;
            gravity = 1f;
            worldGravity = false;
            customPathType = PPATH.STANDARD;
            customPathParms[0] = 0f;
            customPathParms[1] = 0f;
            customPathParms[2] = 0f;
            customPathParms[3] = 0f;
            customPathParms[4] = 0f;
            customPathParms[5] = 0f;
            customPathParms[6] = 0f;
            customPathParms[7] = 0f;
            offset.Zero();
            animationFrames = 0;
            animationRate = 0f;
            initialAngle = 0f;
            rotationSpeed.from = 0f;
            rotationSpeed.to = 0f;
            rotationSpeed.table = null;
            size.from = 4f;
            size.to = 4f;
            size.table = null;
            aspect.from = 1f;
            aspect.to = 1f;
            aspect.table = null;
            color.x = 1f;
            color.y = 1f;
            color.z = 1f;
            color.w = 1f;
            fadeColor.x = 0f;
            fadeColor.y = 0f;
            fadeColor.z = 0f;
            fadeColor.w = 0f;
            fadeInFraction = 0.1f;
            fadeOutFraction = 0.25f;
            fadeIndexFraction = 0f;
            boundsExpansion = 0f;
            randomDistribution = true;
            entityColor = false;
            cycleMsec = (int)(particleLife + deadTime) * 1000;
        }

        // includes trails and cross faded animations
        // returns the number of verts created, which will range from 0 to 4*NumQuadsPerParticle()
        public virtual int NumQuadsPerParticle()
        {
            var count = 1;
            if (orientation == POR.AIMED)
            {
                var trails = MathX.Ftoi(orientationParms[0]);
                // each trail stage will add an extra quad
                count *= 1 + trails;
            }

            // if we are doing strip-animation, we need to double the number and cross fade them
            if (animationFrames > 1) count *= 2;
            return count;
        }

        // Returns 0 if no particle is created because it is completely faded out
        // Returns 4 if a normal quad is created
        // Returns 8 if two cross faded quads are created
        // 
        // Vertex order is:
        // 
        // 0 1
        // 2 3
        public virtual int CreateParticle(ParticleGen g, DrawVert[] verts)
        {
            verts[0].Clear();
            verts[1].Clear();
            verts[2].Clear();
            verts[3].Clear();

            ParticleColors(g, verts);

            // if we are completely faded out, kill the particle
            if (verts[0].color0 == 0 && verts[0].color1 == 0 && verts[0].color2 == 0 && verts[0].color3 == 0)
                return 0;

            ParticleOrigin(g, out var origin);

            ParticleTexCoords(g, verts);

            var numVerts = ParticleVerts(g, origin, verts);

            if (animationFrames <= 1)
                return numVerts;

            // if we are doing strip-animation, we need to double the quad and cross fade it
            var width = 1f / animationFrames;
            var frac = g.animationFrameFrac;
            var iFrac = 1f - frac;
            for (var i = 0; i < numVerts; i++)
            {
                verts[numVerts + i] = verts[i];

                verts[numVerts + i].st.x += width;

                verts[numVerts + i].color0 = (byte)(verts[numVerts + i].color0 * frac);
                verts[numVerts + i].color1 = (byte)(verts[numVerts + i].color1 * frac);
                verts[numVerts + i].color2 = (byte)(verts[numVerts + i].color2 * frac);
                verts[numVerts + i].color3 = (byte)(verts[numVerts + i].color3 * frac);

                verts[i].color0 = (byte)(verts[i].color0 * iFrac);
                verts[i].color1 = (byte)(verts[i].color1 * iFrac);
                verts[i].color2 = (byte)(verts[i].color2 * iFrac);
                verts[i].color3 = (byte)(verts[i].color3 * iFrac);
            }

            return numVerts * 2;
        }

        public void ParticleOrigin(ParticleGen g, out Vector3 origin)
        {
            if (customPathType == PPATH.STANDARD)
            {
                // find intial origin distribution
                float radiusSqr, angle1, angle2;

                switch (distributionType)
                {
                    case PDIST.RECT:
                        {   // ( sizeX sizeY sizeZ )
                            origin.x = (randomDistribution ? g.random.CRandomFloat() : 1f) * distributionParms[0];
                            origin.y = (randomDistribution ? g.random.CRandomFloat() : 1f) * distributionParms[1];
                            origin.z = (randomDistribution ? g.random.CRandomFloat() : 1f) * distributionParms[2];
                            break;
                        }
                    case PDIST.CYLINDER:
                        {   // ( sizeX sizeY sizeZ ringFraction )
                            angle1 = (randomDistribution ? g.random.CRandomFloat() : 1f) * MathX.TWO_PI;

                            MathX.SinCos16(angle1, out origin.x, out origin.y);
                            origin.z = randomDistribution ? g.random.CRandomFloat() : 1f;

                            // reproject points that are inside the ringFraction to the outer band
                            if (distributionParms[3] > 0f)
                            {
                                radiusSqr = origin[0] * origin[0] + origin[1] * origin[1];
                                if (radiusSqr < distributionParms[3] * distributionParms[3])
                                {
                                    // if we are inside the inner reject zone, rescale to put it out into the good zone
                                    var f = (float)Math.Sqrt(radiusSqr) / distributionParms[3];
                                    var invf = 1f / f;
                                    var newRadius = distributionParms[3] + f * (1f - distributionParms[3]);
                                    var rescale = invf * newRadius;

                                    origin.x *= rescale;
                                    origin.y *= rescale;
                                }
                            }
                            origin.x *= distributionParms[0];
                            origin.y *= distributionParms[1];
                            origin.z *= distributionParms[2];
                            break;
                        }
                    case PDIST.SPHERE:
                        {   // ( sizeX sizeY sizeZ ringFraction )
                            // iterating with rejection is the only way to get an even distribution over a sphere
                            if (randomDistribution)
                            {
                                do
                                {
                                    origin.x = g.random.CRandomFloat();
                                    origin.y = g.random.CRandomFloat();
                                    origin.z = g.random.CRandomFloat();
                                    radiusSqr = origin.x * origin.x + origin.y * origin.y + origin.z * origin.z;
                                } while (radiusSqr > 1f);
                            }
                            else
                            {
                                origin = new(1f, 1f, 1f);
                                radiusSqr = 3f;
                            }

                            if (distributionParms[3] > 0f)
                            {
                                // we could iterate until we got something that also satisfied ringFraction,
                                // but for narrow rings that could be a lot of work, so reproject inside points instead
                                if (radiusSqr < distributionParms[3] * distributionParms[3])
                                {
                                    // if we are inside the inner reject zone, rescale to put it out into the good zone
                                    var f = (float)Math.Sqrt(radiusSqr) / distributionParms[3];
                                    var invf = 1f / f;
                                    var newRadius = distributionParms[3] + f * (1f - distributionParms[3]);
                                    var rescale = invf * newRadius;

                                    origin.x *= rescale;
                                    origin.y *= rescale;
                                    origin.z *= rescale;
                                }
                            }
                            origin.x *= distributionParms[0];
                            origin.y *= distributionParms[1];
                            origin.z *= distributionParms[2];
                            break;
                        }
                    default:
                        {
                            origin = new();
                            break;
                        }
                }

                // offset will effect all particle origin types. add this before the velocity and gravity additions
                origin += offset;

                // add the velocity over time
                Vector3 dir;

                switch (directionType)
                {
                    case PDIR.CONE:
                        // angle is the full angle, so 360 degrees is any spherical direction
                        angle1 = g.random.CRandomFloat() * directionParms[0] * MathX.M_DEG2RAD;
                        angle2 = g.random.CRandomFloat() * MathX.PI;

                        MathX.SinCos16(angle1, out var s1, out var c1);
                        MathX.SinCos16(angle2, out var s2, out var c2);

                        dir.x = s1 * c2;
                        dir.y = s1 * s2;
                        dir.z = c1;
                        break;
                    case PDIR.OUTWARD:
                        dir = origin;
                        dir.Normalize();
                        dir[2] += directionParms[0];
                        break;
                    default: common.Error("ParticleStage::ParticleOrigin: bad direction"); return;
                }

                // add speed
                var iSpeed = speed.Integrate(g.frac, g.random);
                origin += dir * iSpeed * particleLife;
            }
            else
            {
                // custom paths completely override both the origin and velocity calculations, but still use the standard gravity
                float angle1, angle2, speed1, speed2;
                switch (customPathType)
                {
                    case PPATH.HELIX:
                        {   // ( sizeX sizeY sizeZ radialSpeed axialSpeed )
                            speed1 = g.random.CRandomFloat();
                            speed2 = g.random.CRandomFloat();
                            angle1 = g.random.RandomFloat() * MathX.TWO_PI + customPathParms[3] * speed1 * g.age;

                            MathX.SinCos16(angle1, out var s1, out var c1);

                            origin.x = c1 * customPathParms[0];
                            origin.y = s1 * customPathParms[1];
                            origin.z = g.random.RandomFloat() * customPathParms[2] + customPathParms[4] * speed2 * g.age;
                            break;
                        }
                    case PPATH.FLIES:
                        {   // ( radialSpeed axialSpeed size )
                            speed1 = MathX.ClampFloat(0.4f, 1f, g.random.CRandomFloat());
                            speed2 = MathX.ClampFloat(0.4f, 1f, g.random.CRandomFloat());
                            angle1 = g.random.RandomFloat() * MathX.PI * 2 + customPathParms[0] * speed1 * g.age;
                            angle2 = g.random.RandomFloat() * MathX.PI * 2 + customPathParms[1] * speed1 * g.age;

                            MathX.SinCos16(angle1, out var s1, out var c1);
                            MathX.SinCos16(angle2, out var s2, out var c2);

                            origin.x = c1 * c2;
                            origin.y = s1 * c2;
                            origin.z = -s2;
                            origin *= customPathParms[2];
                            break;
                        }
                    case PPATH.ORBIT:
                        {   // ( radius speed axis )
                            angle1 = g.random.RandomFloat() * MathX.TWO_PI + customPathParms[1] * g.age;

                            MathX.SinCos16(angle1, out var s1, out var c1);

                            origin.x = c1 * customPathParms[0];
                            origin.y = s1 * customPathParms[0];
                            origin.z = 0f;
                            origin.ProjectSelfOntoSphere(customPathParms[0]);
                            break;
                        }
                    case PPATH.DRIP:
                        {   // ( speed )
                            origin.x = 0f;
                            origin.y = 0f;
                            origin.z = -(g.age * customPathParms[0]);
                            break;
                        }
                    default:
                        {
                            origin = new();
                            common.Error("ParticleStage::ParticleOrigin: bad customPathType");
                            break;
                        }
                }

                origin += offset;
            }

            // adjust for the per-particle smoke offset
            origin *= g.axis;
            origin += g.origin;

            // add gravity after adjusting for axis
            if (worldGravity)
            {
                var gra = new Vector3(0, 0, -gravity);
                gra *= g.renderEnt.axis.Transpose();
                origin += gra * g.age * g.age;
            }
            else origin.z -= gravity * g.age * g.age;
        }

        public int ParticleVerts(ParticleGen g, Vector3 origin, DrawVert[] verts)
        {
            var psize = size.Eval(g.frac, g.random);
            var paspect = aspect.Eval(g.frac, g.random);

            var width = psize;
            var height = psize * paspect;

            Vector3 left, up;

            if (orientation == POR.AIMED)
            {
                // reset the values to an earlier time to get a previous origin
                var currentRandom = g.random;
                var currentAge = g.age;
                var currentFrac = g.frac;
                var verts_p = 0;
                var stepOrigin = origin;
                var stepLeft = new Vector3();
                var numTrails = MathX.Ftoi(orientationParms[0]);

                stepLeft.Zero();
                var trailTime = orientationParms[1];
                if (trailTime == 0) trailTime = 0.5f;

                var height2 = 1f / (1 + numTrails);
                var t = 0f;
                for (var i = 0; i <= numTrails; i++)
                {
                    g.random = g.originalRandom;
                    g.age = currentAge - (i + 1) * trailTime / (numTrails + 1);    // time to back up
                    g.frac = g.age / particleLife;

                    ParticleOrigin(g, out var oldOrigin);
                    up = stepOrigin - oldOrigin;    // along the direction of travel
                    g.renderEnt.axis.ProjectVector(g.renderView.viewaxis[0], out var forwardDir);
                    up -= up * forwardDir * forwardDir;
                    up.Normalize();

                    left = up.Cross(forwardDir);
                    left *= psize;

                    verts[verts_p + 0] = verts[0];
                    verts[verts_p + 1] = verts[1];
                    verts[verts_p + 2] = verts[2];
                    verts[verts_p + 3] = verts[3];

                    if (i == 0)
                    {
                        verts[verts_p + 0].xyz = stepOrigin - left;
                        verts[verts_p + 1].xyz = stepOrigin + left;
                    }
                    else
                    {
                        verts[verts_p + 0].xyz = stepOrigin - stepLeft;
                        verts[verts_p + 1].xyz = stepOrigin + stepLeft;
                    }
                    verts[verts_p + 2].xyz = oldOrigin - left;
                    verts[verts_p + 3].xyz = oldOrigin + left;

                    // modify texcoords
                    verts[verts_p + 0].st.x = verts[0].st.x; verts[verts_p + 0].st.y = t;
                    verts[verts_p + 1].st.x = verts[1].st.x; verts[verts_p + 1].st.y = t;
                    verts[verts_p + 2].st.x = verts[2].st.x; verts[verts_p + 2].st.y = t + height2;
                    verts[verts_p + 3].st.x = verts[3].st.x; verts[verts_p + 3].st.y = t + height2;

                    t += height2;

                    verts_p += 4;

                    stepOrigin = oldOrigin;
                    stepLeft = left;
                }

                g.random = currentRandom;
                g.age = currentAge;
                g.frac = currentFrac;

                return 4 * (numTrails + 1);
            }

            // constant rotation
            var angle = initialAngle != 0f ? initialAngle : 360 * g.random.RandomFloat();

            var angleMove = rotationSpeed.Integrate(g.frac, g.random) * particleLife;
            // have hald the particles rotate each way
            if ((g.index & 1) != 0) angle += angleMove;
            else angle -= angleMove;

            angle = angle / 180 * MathX.PI;
            var c = MathX.Cos16(angle);
            var s = MathX.Sin16(angle);

            if (orientation == POR.Z)
            {
                // oriented in entity space
                left.x = s; left.y = c; left.z = 0f;
                up.x = c; up.y = -s; up.z = 0f;
            }
            else if (orientation == POR.X)
            {
                // oriented in entity space
                left.x = 0f; left.y = c; left.z = s;
                up.x = 0f; up.y = -s; up.z = c;
            }
            else if (orientation == POR.Y)
            {
                // oriented in entity space
                left.x = c; left.y = 0f; left.z = s;
                up.x = -s; up.y = 0f; up.z = c;
            }
            else
            {
                // oriented in viewer space
                g.renderEnt.axis.ProjectVector(g.renderView.viewaxis[1], out var entityLeft);
                g.renderEnt.axis.ProjectVector(g.renderView.viewaxis[2], out var entityUp);

                left = entityLeft * c + entityUp * s;
                up = entityUp * c - entityLeft * s;
            }

            left *= width;
            up *= height;

            verts[0].xyz = origin - left + up;
            verts[1].xyz = origin + left + up;
            verts[2].xyz = origin - left - up;
            verts[3].xyz = origin + left - up;

            return 4;
        }

        public void ParticleTexCoords(ParticleGen g, DrawVert[] verts)
        {
            float s, width;
            float t, height;

            if (animationFrames > 1)
            {
                width = 1f / animationFrames;
                var floatFrame = animationRate != 0f
                    // explicit, cycling animation
                    ? g.age * animationRate
                    // single animation cycle over the life of the particle
                    : g.frac * animationFrames;
                var intFrame = (int)floatFrame;
                g.animationFrameFrac = floatFrame - intFrame;
                s = width * intFrame;
            }
            else
            {
                s = 0f;
                width = 1f;
            }

            t = 0f;
            height = 1f;

            verts[0].st.x = s; verts[0].st.y = t;
            verts[1].st.x = s + width; verts[1].st.y = t;
            verts[2].st.x = s; verts[2].st.y = t + height;
            verts[3].st.x = s + width; verts[3].st.y = t + height;
        }

        public void ParticleColors(ParticleGen g, DrawVert[] verts)
        {
            var fadeFraction = 1f;

            // most particles fade in at the beginning and fade out at the end
            if (g.frac < fadeInFraction)
                fadeFraction *= (g.frac / fadeInFraction);
            if (1f - g.frac < fadeOutFraction)
                fadeFraction *= ((1f - g.frac) / fadeOutFraction);

            // individual gun smoke particles get more and more faded as the cycle goes on (note that totalParticles won't be correct for a surface-particle deform)
            if (fadeIndexFraction != 0f)
            {
                var indexFrac = (totalParticles - g.index) / (float)totalParticles;
                if (indexFrac < fadeIndexFraction)
                    fadeFraction *= indexFrac / fadeIndexFraction;
            }

            for (var i = 0; i < 4; i++)
            {
                var fcolor = (entityColor ? g.renderEnt.shaderParms[i] : color[i]) * fadeFraction + fadeColor[i] * (1f - fadeFraction);
                var icolor = (byte)MathX.FtoiFast(fcolor * 255f);
                if (icolor < 0) icolor = 0;
                else if (icolor > 255) icolor = 255;

                if (i == 0) verts[0].color0 = verts[1].color0 = verts[2].color0 = verts[3].color0 = icolor;
                else if (i == 1) verts[0].color1 = verts[1].color1 = verts[2].color1 = verts[3].color1 = icolor;
                else if (i == 2) verts[0].color2 = verts[1].color2 = verts[2].color2 = verts[3].color2 = icolor;
                else if (i == 3) verts[0].color3 = verts[1].color3 = verts[2].color3 = verts[3].color3 = icolor;
            }
        }

        public string CustomPathName
            => DeclParticle.ParticleCustomDesc[(int)customPathType < DeclParticle.CustomParticleCount ? (int)customPathType : 0].name;

        public string CustomPathDesc
            => DeclParticle.ParticleCustomDesc[(int)customPathType < DeclParticle.CustomParticleCount ? (int)customPathType : 0].desc;

        public int NumCustomPathParms
            => DeclParticle.ParticleCustomDesc[(int)customPathType < DeclParticle.CustomParticleCount ? (int)customPathType : 0].count;

        public void SetCustomPathType(string p)
        {
            customPathType = PPATH.STANDARD;
            for (var i = 0; i < DeclParticle.CustomParticleCount; i++)
                if (string.Equals(p, DeclParticle.ParticleCustomDesc[i].name, StringComparison.OrdinalIgnoreCase))
                {
                    customPathType = (PPATH)i;
                    break;
                }
        }


        public Material material;

        public int totalParticles;     // total number of particles, although some may be invisible at a given time
        public float cycles;               // allows things to oneShot ( 1 cycle ) or run for a set number of cycles on a per stage basis

        public int cycleMsec;          // ( particleLife + deadTime ) in msec

        public float spawnBunching;        // 0.0 = all come out at first instant, 1.0 = evenly spaced over cycle time
        public float particleLife;     // total seconds of life for each particle
        public float timeOffset;           // time offset from system start for the first particle to spawn
        public float deadTime;         // time after particleLife before respawning

        //-------------------------------	// standard path parms

        public PDIST distributionType;
        public float[] distributionParms = new float[4];

        public PDIR directionType;
        public float[] directionParms = new float[4];

        public ParticleParm speed;
        public float gravity;              // can be negative to float up
        public bool worldGravity;          // apply gravity in world space
        public bool randomDistribution;        // randomly orient the quad on emission ( defaults to true )
        public bool entityColor;           // force color from render entity ( fadeColor is still valid )

        //------------------------------	// custom path will completely replace the standard path calculations

        public PPATH customPathType;      // use custom C code routines for determining the origin
        public float[] customPathParms = new float[8];

        //--------------------------------

        public Vector3 offset;              // offset from origin to spawn all particles, also applies to customPath

        public int animationFrames;    // if > 1, subdivide the texture S axis into frames and crossfade
        public float animationRate;        // frames per second

        public float initialAngle;     // in degrees, random angle is used if zero ( default )
        public ParticleParm rotationSpeed;       // half the particles will have negative rotation speeds

        public POR orientation;   // view, aimed, or axis fixed
        public float[] orientationParms = new float[4];

        public ParticleParm size;
        public ParticleParm aspect;              // greater than 1 makes the T axis longer

        public Vector4 color;
        public Vector4 fadeColor;           // either 0 0 0 0 for additive, or 1 1 1 0 for blended materials
        public float fadeInFraction;       // in 0.0 to 1.0 range
        public float fadeOutFraction;  // in 0.0 to 1.0 range
        public float fadeIndexFraction;    // in 0.0 to 1.0 range, causes later index smokes to be more faded

        public bool hidden;                // for editor use

        //-----------------------------------

        public float boundsExpansion;  // user tweak to fix poorly calculated bounds

        public Bounds bounds;                // derived
    }

    // group of particle stages
    public class DeclParticle : Decl
    {
        internal struct ParticleParmDesc
        {
            public ParticleParmDesc(string name, int count, string desc)
            {
                this.name = name;
                this.count = count;
                this.desc = desc;
            }
            public string name;
            public int count;
            public string desc;
        }

        internal static readonly ParticleParmDesc[] ParticleDistributionDesc = {
            new ("rect", 3, string.Empty),
            new ("cylinder", 4, string.Empty),
            new ("sphere", 3, string.Empty)
        };

        internal static readonly ParticleParmDesc[] ParticleDirectionDesc = {
            new ("cone", 1, string.Empty),
            new ("outward", 1, string.Empty)
        };

        internal static readonly ParticleParmDesc[] ParticleOrientationDesc = {
            new ("view", 0, string.Empty),
            new ("aimed", 2, string.Empty),
            new ("x", 0, string.Empty),
            new ("y", 0, string.Empty),
            new ("z", 0, string.Empty)
        };

        internal static readonly ParticleParmDesc[] ParticleCustomDesc = {
            new ("standard", 0, "Standard"),
            new ("helix", 5, "sizeX Y Z radialSpeed axialSpeed"),
            new ("flies", 3, "radialSpeed axialSpeed size"),
            new ("orbit", 2, "radius speed"),
            new ("drip", 2, "something something")
        };

        internal static readonly int CustomParticleCount = ParticleCustomDesc.Length;

        public List<ParticleStage> stages = new();
        public Bounds bounds;
        public float depthHack;

        public override int Size => 0;
        public override string DefaultDefinition =>
@"{
	{
		material _default
		count 20
		time  1.0
	}
}";
        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");

            depthHack = 0f;

            while (true)
            {
                if (!src.ReadToken(out var token)) break;
                if (token == "}") break;
                if (token == "{")
                {
                    var stage = ParseParticleStage(src);
                    if (stage == null)
                    {
                        src.Warning("Particle stage parse failed");
                        MakeDefault();
                        return false;
                    }
                    stages.Add(stage);
                    continue;
                }
                if (string.Equals(token, "depthHack", StringComparison.OrdinalIgnoreCase)) { depthHack = src.ParseFloat(); continue; }

                src.Warning($"bad token {token}"); MakeDefault(); return false;
            }

            // calculate the bounds
            bounds.Clear();
            for (var i = 0; i < stages.Count; i++)
            {
                GetStageBounds(stages[i]);
                bounds.AddBounds(stages[i].bounds);
            }

            if (bounds.Volume <= 0.1f)
                bounds = new Bounds(Vector3.origin).Expand(8f);

            return true;
        }
        public override void FreeData() => stages.Clear();

        public bool Save(string fileName = null)
        {
            RebuildTextSource();
            if (fileName != null)
                declManager.CreateNewDecl(DECL.PARTICLE, Name, fileName);
            ReplaceSourceFileText();
            return true;
        }

        bool RebuildTextSource()
        {
            VFile_Memory f = new();

            f.WriteFloatString(
@"

    /*
    Generated by the Particle Editor.
    To use the particle editor, launch the game and type 'editParticles' on the console.
    */
");
            f.WriteFloatString($"particle {Name} {{\n");

            if (depthHack != 0f)
                f.WriteFloatString($"\tdepthHack\t{depthHack}\n");

            for (var i = 0; i < stages.Count; i++)
                WriteStage(f, stages[i]);

            f.WriteFloatString("}");

            Text = Encoding.ASCII.GetString(f.DataPtr);

            return true;
        }

        void GetStageBounds(ParticleStage stage)
        {
            stage.bounds.Clear();

            // this isn't absolutely guaranteed, but it should be close
            var g = new ParticleGen
            {
                renderEnt = new RenderEntity { axis = Matrix3x3.identity },
                renderView = new RenderView { viewaxis = Matrix3x3.identity },
                axis = Matrix3x3.identity,
            };
            g.origin.Zero();

            var steppingRandom = new RandomX(0);

            // just step through a lot of possible particles as a representative sampling
            for (var i = 0; i < 1000; i++)
            {
                g.random = g.originalRandom = steppingRandom;

                var maxMsec = (int)(stage.particleLife * 1000);
                for (var inCycleTime = 0; inCycleTime < maxMsec; inCycleTime += 16)
                {
                    // make sure we get the very last tic, which may make up an extreme edge
                    if (inCycleTime + 16 > maxMsec) inCycleTime = maxMsec - 1;

                    g.frac = (float)inCycleTime / (stage.particleLife * 1000);
                    g.age = inCycleTime * 0.001f;

                    // if the particle doesn't get drawn because it is faded out or beyond a kill region, don't increment the verts
                    stage.ParticleOrigin(g, out var origin);
                    stage.bounds.AddPoint(origin);
                }
            }

            // find the max size
            var maxSize = 0f;

            for (var f = 0f; f <= 1f; f += 1f / 64)
            {
                var size = stage.size.Eval(f, steppingRandom);
                var aspect = stage.aspect.Eval(f, steppingRandom);
                if (aspect > 1) size *= aspect;
                if (size > maxSize) maxSize = size;
            }

            maxSize += 8;   // just for good measure. users can specify a per-stage bounds expansion to handle odd cases
            stage.bounds.ExpandSelf(maxSize + stage.boundsExpansion);
        }

        ParticleStage ParseParticleStage(Lexer src)
        {
            var stage = new ParticleStage();
            stage.Default();

            while (true)
            {
                if (src.HadError) break;
                if (!src.ReadToken(out var token)) break;
                if (token == "}") break;
                if (string.Equals(token, "material", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); stage.material = declManager.FindMaterial(token); continue; }
                if (string.Equals(token, "count", StringComparison.OrdinalIgnoreCase)) { stage.totalParticles = src.ParseInt(); continue; }
                if (string.Equals(token, "time", StringComparison.OrdinalIgnoreCase)) { stage.particleLife = src.ParseFloat(); continue; }
                if (string.Equals(token, "cycles", StringComparison.OrdinalIgnoreCase)) { stage.cycles = src.ParseFloat(); continue; }
                if (string.Equals(token, "timeOffset", StringComparison.OrdinalIgnoreCase)) { stage.timeOffset = src.ParseFloat(); continue; }
                if (string.Equals(token, "deadTime", StringComparison.OrdinalIgnoreCase)) { stage.deadTime = src.ParseFloat(); continue; }
                if (string.Equals(token, "randomDistribution", StringComparison.OrdinalIgnoreCase)) { stage.randomDistribution = src.ParseBool(); continue; }
                if (string.Equals(token, "bunching", StringComparison.OrdinalIgnoreCase)) { stage.spawnBunching = src.ParseFloat(); continue; }
                if (string.Equals(token, "distribution", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    if (string.Equals(token, "rect", StringComparison.OrdinalIgnoreCase)) stage.distributionType = ParticleStage.PDIST.RECT;
                    else if (string.Equals(token, "cylinder", StringComparison.OrdinalIgnoreCase)) stage.distributionType = ParticleStage.PDIST.CYLINDER;
                    else if (string.Equals(token, "sphere", StringComparison.OrdinalIgnoreCase)) stage.distributionType = ParticleStage.PDIST.SPHERE;
                    else src.Error($"bad distribution type: {token}\n");
                    ParseParms(src, stage.distributionParms);
                    continue;
                }
                if (string.Equals(token, "direction", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    if (string.Equals(token, "cone", StringComparison.OrdinalIgnoreCase)) stage.directionType = ParticleStage.PDIR.CONE;
                    else if (string.Equals(token, "outward", StringComparison.OrdinalIgnoreCase)) stage.directionType = ParticleStage.PDIR.OUTWARD;
                    else src.Error($"bad direction type: {token}\n");
                    ParseParms(src, stage.directionParms);
                    continue;
                }
                if (string.Equals(token, "orientation", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    if (string.Equals(token, "view", StringComparison.OrdinalIgnoreCase)) stage.orientation = ParticleStage.POR.VIEW;
                    else if (string.Equals(token, "aimed", StringComparison.OrdinalIgnoreCase)) stage.orientation = ParticleStage.POR.AIMED;
                    else if (string.Equals(token, "x", StringComparison.OrdinalIgnoreCase)) stage.orientation = ParticleStage.POR.X;
                    else if (string.Equals(token, "y", StringComparison.OrdinalIgnoreCase)) stage.orientation = ParticleStage.POR.Y;
                    else if (string.Equals(token, "z", StringComparison.OrdinalIgnoreCase)) stage.orientation = ParticleStage.POR.Z;
                    else src.Error($"bad orientation type: {token}\n");
                    ParseParms(src, stage.orientationParms);
                    continue;
                }
                if (string.Equals(token, "customPath", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    if (string.Equals(token, "standard", StringComparison.OrdinalIgnoreCase)) stage.customPathType = ParticleStage.PPATH.STANDARD;
                    else if (string.Equals(token, "helix", StringComparison.OrdinalIgnoreCase)) stage.customPathType = ParticleStage.PPATH.HELIX;
                    else if (string.Equals(token, "flies", StringComparison.OrdinalIgnoreCase)) stage.customPathType = ParticleStage.PPATH.FLIES;
                    else if (string.Equals(token, "spherical", StringComparison.OrdinalIgnoreCase)) stage.customPathType = ParticleStage.PPATH.ORBIT;
                    else src.Error($"bad path type: {token}\n");
                    ParseParms(src, stage.customPathParms);
                    continue;
                }
                if (string.Equals(token, "speed", StringComparison.OrdinalIgnoreCase)) { ParseParametric(src, out stage.speed); continue; }
                if (string.Equals(token, "rotation", StringComparison.OrdinalIgnoreCase)) { ParseParametric(src, out stage.rotationSpeed); continue; }
                if (string.Equals(token, "angle", StringComparison.OrdinalIgnoreCase)) { stage.initialAngle = src.ParseFloat(); continue; }
                if (string.Equals(token, "entityColor", StringComparison.OrdinalIgnoreCase)) { stage.entityColor = src.ParseBool(); continue; }
                if (string.Equals(token, "size", StringComparison.OrdinalIgnoreCase)) { ParseParametric(src, out stage.size); continue; }
                if (string.Equals(token, "aspect", StringComparison.OrdinalIgnoreCase)) { ParseParametric(src, out stage.aspect); continue; }
                if (string.Equals(token, "fadeIn", StringComparison.OrdinalIgnoreCase)) { stage.fadeInFraction = src.ParseFloat(); continue; }
                if (string.Equals(token, "fadeOut", StringComparison.OrdinalIgnoreCase)) { stage.fadeOutFraction = src.ParseFloat(); continue; }
                if (string.Equals(token, "fadeIndex", StringComparison.OrdinalIgnoreCase)) { stage.fadeIndexFraction = src.ParseFloat(); continue; }
                if (string.Equals(token, "color", StringComparison.OrdinalIgnoreCase)) { stage.color.x = src.ParseFloat(); stage.color.y = src.ParseFloat(); stage.color.z = src.ParseFloat(); stage.color.w = src.ParseFloat(); continue; }
                if (string.Equals(token, "fadeColor", StringComparison.OrdinalIgnoreCase)) { stage.fadeColor.x = src.ParseFloat(); stage.fadeColor.y = src.ParseFloat(); stage.fadeColor.z = src.ParseFloat(); stage.fadeColor.w = src.ParseFloat(); continue; }
                if (string.Equals(token, "offset", StringComparison.OrdinalIgnoreCase)) { stage.offset.x = src.ParseFloat(); stage.offset.y = src.ParseFloat(); stage.offset.z = src.ParseFloat(); continue; }
                if (string.Equals(token, "animationFrames", StringComparison.OrdinalIgnoreCase)) { stage.animationFrames = src.ParseInt(); continue; }
                if (string.Equals(token, "animationRate", StringComparison.OrdinalIgnoreCase)) { stage.animationRate = src.ParseFloat(); continue; }
                if (string.Equals(token, "boundsExpansion", StringComparison.OrdinalIgnoreCase)) { stage.boundsExpansion = src.ParseFloat(); continue; }
                if (string.Equals(token, "gravity", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    if (string.Equals(token, "world", StringComparison.OrdinalIgnoreCase)) stage.worldGravity = true;
                    else src.UnreadToken(token);
                    stage.gravity = src.ParseFloat();
                    continue;
                }
                src.Error($"unknown token {token}\n");
            }

            // derive values
            stage.cycleMsec = (int)(stage.particleLife + stage.deadTime) * 1000;

            return stage;
        }

        // Parses a variable length list of parms on one line
        void ParseParms(Lexer src, float[] parms)
        {
            var maxParms = parms.Length;
            Array.Clear(parms, 0, maxParms);
            var count = 0;
            while (true)
            {
                if (!src.ReadTokenOnLine(out var token)) return;
                if (count == maxParms) { src.Error("too many parms on line"); return; }
                parms[count] = float.Parse(stringX.StripQuotes(token));
                count++;
            }
        }

        void ParseParametric(Lexer src, out ParticleParm parm)
        {
            parm = new();

            if (!src.ReadToken(out var token)) { src.Error("not enough parameters"); return; }
            if (stringX.IsNumeric(token))
            {
                // can have a to + 2nd parm
                parm.from = parm.to = float.Parse(token);
                if (src.ReadToken(out token))
                    if (string.Equals(token, "to", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!src.ReadToken(out token)) { src.Error("missing second parameter"); return; }
                        parm.to = float.Parse(token);
                    }
                    else src.UnreadToken(token);
            }
            // table
            else parm.table = (DeclTable)declManager.FindType(DECL.TABLE, token, false);
        }

        void WriteStage(VFile f, ParticleStage stage)
        {
            int i;

            f.WriteFloatString("\t{\n");
            f.WriteFloatString($"\t\tcount\t\t\t\t{stage.totalParticles}\n");
            f.WriteFloatString($"\t\tmaterial\t\t\t{stage.material.Name}\n");
            if (stage.animationFrames != 0) f.WriteFloatString($"\t\tanimationFrames \t{stage.animationFrames}\n");
            if (stage.animationRate != 0f) f.WriteFloatString($"\t\tanimationRate \t\t{stage.animationRate:3}\n");
            f.WriteFloatString($"\t\ttime\t\t\t\t{stage.particleLife:3}\n");
            f.WriteFloatString($"\t\tcycles\t\t\t\t{stage.cycles:3}\n");
            if (stage.timeOffset != 0f) f.WriteFloatString($"\t\ttimeOffset\t\t\t{stage.timeOffset:3}\n");
            if (stage.deadTime != 0f) f.WriteFloatString($"\t\tdeadTime\t\t\t{stage.deadTime:3}\n");
            f.WriteFloatString($"\t\tbunching\t\t\t{stage.spawnBunching:3}\n");

            f.WriteFloatString($"\t\tdistribution\t\t{ParticleDistributionDesc[(int)stage.distributionType].name} ");
            for (i = 0; i < ParticleDistributionDesc[(int)stage.distributionType].count; i++) f.WriteFloatString($"{stage.distributionParms[i]:3} ");
            f.WriteFloatString("\n");

            f.WriteFloatString("\t\tdirection\t\t\t%s ", ParticleDirectionDesc[(int)stage.directionType].name);
            for (i = 0; i < ParticleDirectionDesc[(int)stage.directionType].count; i++) f.WriteFloatString($"\"{stage.directionParms[i]:3}\" ");
            f.WriteFloatString("\n");

            f.WriteFloatString($"\t\torientation\t\t\t{ParticleOrientationDesc[(int)stage.orientation].name} ");
            for (i = 0; i < ParticleOrientationDesc[(int)stage.orientation].count; i++) f.WriteFloatString($"{stage.orientationParms[i]:3} ");
            f.WriteFloatString("\n");

            if (stage.customPathType != ParticleStage.PPATH.STANDARD)
            {
                f.WriteFloatString($"\t\tcustomPath {ParticleCustomDesc[(int)stage.customPathType].name} ");
                for (i = 0; i < ParticleCustomDesc[(int)stage.customPathType].count; i++) f.WriteFloatString($"{stage.customPathParms[i]:3} ");
                f.WriteFloatString("\n");
            }

            if (stage.entityColor) f.WriteFloatString("\t\tentityColor\t\t\t1\n");

            WriteParticleParm(f, stage.speed, "speed");
            WriteParticleParm(f, stage.size, "size");
            WriteParticleParm(f, stage.aspect, "aspect");

            if (stage.rotationSpeed.from != 0f) WriteParticleParm(f, stage.rotationSpeed, "rotation");
            if (stage.initialAngle != 0f) f.WriteFloatString($"\t\tangle\t\t\t\t{stage.initialAngle}\n");

            f.WriteFloatString($"\t\trandomDistribution\t\t\t\t{(stage.randomDistribution ? 1 : 0)}\n");
            f.WriteFloatString($"\t\tboundsExpansion\t\t\t\t{stage.boundsExpansion:3}\n");

            f.WriteFloatString($"\t\tfadeIn\t\t\t\t{stage.fadeInFraction:3}\n");
            f.WriteFloatString($"\t\tfadeOut\t\t\t\t{stage.fadeOutFraction:3}\n");
            f.WriteFloatString($"\t\tfadeIndex\t\t\t\t{stage.fadeIndexFraction:3}\n");

            f.WriteFloatString($"\t\tcolor \t\t\t\t{stage.color.x:3} {stage.color.y:3} {stage.color.z:3} {stage.color.w:3}\n");
            f.WriteFloatString($"\t\tfadeColor \t\t\t{stage.fadeColor.x:3} {stage.fadeColor.y:3} {stage.fadeColor.z:3} {stage.fadeColor.w:3}\n");

            f.WriteFloatString($"\t\toffset \t\t\t\t{stage.offset.x:3} {stage.offset.y:3} {stage.offset.z:3}\n");
            f.WriteFloatString("\t\tgravity \t\t\t");
            if (stage.worldGravity) f.WriteFloatString("world ");
            f.WriteFloatString("{stage.gravity:3}\n");
            f.WriteFloatString("\t}\n");
        }

        void WriteParticleParm(VFile f, ParticleParm parm, string name)
        {
            f.WriteFloatString($"\t\t{name}\t\t\t\t ");
            if (parm.table != null) f.WriteFloatString($"{parm.table.Name}\n");
            else
            {
                f.WriteFloatString($"\"{parm.from:3}\" ");
                f.WriteFloatString(parm.from == parm.to ? "\n" : $" to \"{parm.to:3}\"\n");
            }
        }
    }
}
