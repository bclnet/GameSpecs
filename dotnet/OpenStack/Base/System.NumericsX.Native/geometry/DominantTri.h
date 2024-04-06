#ifndef __DOMINANTTRI_H__
#define __DOMINANTTRI_H__

typedef int GlIndex;

struct DominantTri {
	GlIndex v2, v3;
	float normalizationScale[3];
};

#endif
