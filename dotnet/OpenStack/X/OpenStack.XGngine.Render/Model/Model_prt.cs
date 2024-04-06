using Gengine.Framework;
using System.NumericsX;
using System.NumericsX.OpenStack;
using static Gengine.Lib;

namespace Gengine.Render
{
    public class RenderModelPrt : RenderModelStatic
    {
        const string parametricParticle_SnapshotName = "_ParametricParticle_Snapshot_";

        DeclParticle particleSystem;

        public override void InitFromFile(string fileName)
        {
            name = fileName;
            particleSystem = (DeclParticle)declManager.FindType(DECL.PARTICLE, fileName);
        }

        public override void TouchData()
            // Ensure our particle system is added to the list of referenced decls
            => particleSystem = (DeclParticle)declManager.FindType(DECL.PARTICLE, name);

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
			idRenderModelStatic* staticModel;

			if (cachedModel && !r_useCachedDynamicModels.GetBool())
			{
				delete cachedModel;
				cachedModel = NULL;
			}

			// this may be triggered by a model trace or other non-view related source, to which we should look like an empty model
			if (renderEntity == NULL || viewDef == NULL)
			{
				delete cachedModel;
				return NULL;
			}

			if (r_skipParticles.GetBool())
			{
				delete cachedModel;
				return NULL;
			}

			/*
			// if the entire system has faded out
			if ( renderEntity->shaderParms[SHADERPARM_PARTICLE_STOPTIME] && viewDef->renderView.time * 0.001f >= renderEntity->shaderParms[SHADERPARM_PARTICLE_STOPTIME] ) {
				delete cachedModel;
				return NULL;
			}
			*/

			if (cachedModel != NULL)
			{

				assert(dynamic_cast<idRenderModelStatic*>(cachedModel) != NULL);
				assert(idStr::Icmp(cachedModel->Name(), parametricParticle_SnapshotName) == 0);

				staticModel = static_cast<idRenderModelStatic*>(cachedModel);

			}
			else
			{

				staticModel = new idRenderModelStatic;
				staticModel->InitEmpty(parametricParticle_SnapshotName);
			}

			particleGen_t g;

			g.renderEnt = renderEntity;
			g.renderView = &viewDef->renderView;
			g.origin.Zero();
			g.axis.Identity();

			for (int stageNum = 0; stageNum < particleSystem->stages.Num(); stageNum++)
			{
				idParticleStage* stage = particleSystem->stages[stageNum];

				if (!stage->material)
				{
					continue;
				}
				if (!stage->cycleMsec)
				{
					continue;
				}
				if (stage->hidden)
				{       // just for gui particle editor use
					staticModel->DeleteSurfaceWithId(stageNum);
					continue;
				}

				idRandom steppingRandom, steppingRandom2;

				int stageAge = g.renderView->time + renderEntity->shaderParms[SHADERPARM_TIMEOFFSET] * 1000 - stage->timeOffset * 1000;
				int stageCycle = stageAge / stage->cycleMsec;

				// some particles will be in this cycle, some will be in the previous cycle
				steppingRandom.SetSeed(((stageCycle << 10) & idRandom::MAX_RAND) ^ (int)(renderEntity->shaderParms[SHADERPARM_DIVERSITY] * idRandom::MAX_RAND));
				steppingRandom2.SetSeed((((stageCycle - 1) << 10) & idRandom::MAX_RAND) ^ (int)(renderEntity->shaderParms[SHADERPARM_DIVERSITY] * idRandom::MAX_RAND));

				int count = stage->totalParticles * stage->NumQuadsPerParticle();

				int surfaceNum;
				modelSurface_t* surf;

				if (staticModel->FindSurfaceWithId(stageNum, surfaceNum))
				{
					surf = &staticModel->surfaces[surfaceNum];
					R_FreeStaticTriSurfVertexCaches(surf->geometry);
				}
				else
				{
					surf = &staticModel->surfaces.Alloc();
					surf->id = stageNum;
					surf->shader = stage->material;
					surf->geometry = R_AllocStaticTriSurf();
					R_AllocStaticTriSurfVerts(surf->geometry, 4 * count);
					R_AllocStaticTriSurfIndexes(surf->geometry, 6 * count);
					R_AllocStaticTriSurfPlanes(surf->geometry, 6 * count);
				}

				int numVerts = 0;
				idDrawVert* verts = surf->geometry->verts;

				for (int index = 0; index < stage->totalParticles; index++)
				{
					g.index = index;

					// bump the random
					steppingRandom.RandomInt();
					steppingRandom2.RandomInt();

					// calculate local age for this index
					int bunchOffset = stage->particleLife * 1000 * stage->spawnBunching * index / stage->totalParticles;

					int particleAge = stageAge - bunchOffset;
					int particleCycle = particleAge / stage->cycleMsec;
					if (particleCycle < 0)
					{
						// before the particleSystem spawned
						continue;
					}
					if (stage->cycles && particleCycle >= stage->cycles)
					{
						// cycled systems will only run cycle times
						continue;
					}

					if (particleCycle == stageCycle)
					{
						g.random = steppingRandom;
					}
					else
					{
						g.random = steppingRandom2;
					}

					int inCycleTime = particleAge - particleCycle * stage->cycleMsec;

					if (renderEntity->shaderParms[SHADERPARM_PARTICLE_STOPTIME] &&
							g.renderView->time - inCycleTime >= renderEntity->shaderParms[SHADERPARM_PARTICLE_STOPTIME] * 1000)
					{
						// don't fire any more particles
						continue;
					}

					// supress particles before or after the age clamp
					g.frac = (float)inCycleTime / (stage->particleLife * 1000);
					if (g.frac < 0.0f)
					{
						// yet to be spawned
						continue;
					}
					if (g.frac > 1.0f)
					{
						// this particle is in the deadTime band
						continue;
					}

					// this is needed so aimed particles can calculate origins at different times
					g.originalRandom = g.random;

					g.age = g.frac * stage->particleLife;

					// if the particle doesn't get drawn because it is faded out or beyond a kill region, don't increment the verts
					numVerts += stage->CreateParticle(&g, verts + numVerts);
				}

				// numVerts must be a multiple of 4
				assert((numVerts & 3) == 0 && numVerts <= 4 * count);

				// build the indexes
				int numIndexes = 0;
				glIndex_t* indexes = surf->geometry->indexes;
				for (int i = 0; i < numVerts; i += 4)
				{
					indexes[numIndexes + 0] = i;
					indexes[numIndexes + 1] = i + 2;
					indexes[numIndexes + 2] = i + 3;
					indexes[numIndexes + 3] = i;
					indexes[numIndexes + 4] = i + 3;
					indexes[numIndexes + 5] = i + 1;
					numIndexes += 6;
				}

				surf->geometry->tangentsCalculated = false;
				surf->geometry->facePlanesCalculated = false;
				surf->geometry->numVerts = numVerts;
				surf->geometry->numIndexes = numIndexes;
				surf->geometry->bounds = stage->bounds;     // just always draw the particles
			}

			return staticModel;
		}

		public override DynamicModel IsDynamicModel
			=> DynamicModel.DM_CONTINUOUS;

		public override Bounds Bounds(RenderEntity ent)
			=> particleSystem.bounds;

        public override float DepthHack
			=> particleSystem.depthHack;

		public override int Memory
			=> 0;
    }
}