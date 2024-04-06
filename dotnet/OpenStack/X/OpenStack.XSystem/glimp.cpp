
bool GLimp_Init(glimpParms_t parms) {
	common->Printf("Initializing OpenGL subsystem\n");

	int colorbits = 32;
	int depthbits = 24;
	int stencilbits = 8;

	Doom3Quest_GetScreenRes(&glConfig.vidWidth, &glConfig.vidHeight);
	glConfig.isFullscreen = true;


	common->Printf("Using %d color bits, %d depth, %d stencil display\n", colorbits, depthbits, stencilbits);

	glConfig.colorBits = colorbits;
	glConfig.depthBits = depthbits;
	glConfig.stencilBits = stencilbits;

	glConfig.displayFrequency = 0;



	GLimp_WindowActive(true);

	return true;
}

bool GLimp_SetScreenParms(glimpParms_t parms) {
	return true;
}

void GLimp_Shutdown() {
}

void GLimp_SetupFrame(int buffer /*unused*/) {

	Doom3Quest_processMessageQueue();

	Doom3Quest_prepareEyeBuffer();
}


void GLimp_SwapBuffers() {
	Doom3Quest_finishEyeBuffer();

	//We can now submit the stereo frame
	Doom3Quest_submitFrame();
}

void GLimp_SetGamma(unsigned short red[256], unsigned short green[256], unsigned short blue[256]) {

}

extern "C" void ActivateContext();
void GLimp_ActivateContext() {
	ActivateContext();
}

extern "C" void DeactivateContext();
void GLimp_DeactivateContext() {
    DeactivateContext();
}

/*
===================
GLimp_ExtensionPointer
===================
*/
#ifdef __ANDROID__
#include <dlfcn.h>
#endif
GLExtension_t GLimp_ExtensionPointer(const char *name) {
#ifdef __ANDROID__
	static void *glesLib = NULL;

	if( !glesLib )
	{
	    int flags = RTLD_LOCAL | RTLD_NOW;
		//glesLib = dlopen("libGLESv2_CM.so", flags);
		glesLib = dlopen("libGLESv3.so", flags);
		if( !glesLib )
		{
			glesLib = dlopen("libGLESv2.so", flags);
		}
	}

	GLExtension_t ret =  (GLExtension_t)dlsym(glesLib, name);
	//common->Printf("GLimp_ExtensionPointer %s  %p\n",name,ret);
	return ret;
#endif
}

void GLimp_WindowActive(bool active)
{
	LOGI( "GLimp_WindowActive %d", active );

	tr.windowActive = active;

	if(!active)
	{
		tr.BackendThreadShutdown();
	}
}

void GLimp_GrabInput(int flags) {
}
