using WaveEngine.Bindings.OpenGLES;
using static WaveEngine.Bindings.OpenGLES.GL;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    //(PixelType) VertexAttribPointerType.UnsignedByte
    //BlitFramebufferFilter.Nearest
    //BlitFramebufferFilter.Linear

    public unsafe static class QGL
    {
        public const int GL_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FE;

        public const uint VBOEmpty = unchecked((uint)-1);
        public static void qglActiveTexture(TextureUnit texture) => glActiveTexture(texture);
        public static void qglAttachShader(uint program, uint shader) => glAttachShader(program, shader);
        public static void qglBindAttribLocation(uint program, uint index, string name) { fixed (char* nameC = name) glBindAttribLocation(program, index, nameC); }
        public static void qglBindBuffer(BufferTargetARB target, uint buffer) => glBindBuffer(target, buffer);
        public static void qglBindFramebuffer(FramebufferTarget target, uint framebuffer) => glBindFramebuffer(target, framebuffer);
        public static void qglBindRenderbuffer(RenderbufferTarget target, uint renderbuffer) => glBindRenderbuffer(target, renderbuffer);
        public static void qglBindTexture(TextureTarget target, uint texture) => glBindTexture(target, texture);
        public static void qglBlendColor(float red, float green, float blue, float alpha) => glBlendColor(red, green, blue, alpha);
        public static void qglBlendEquation(BlendEquationModeEXT mode) => glBlendEquation(mode);
        public static void qglBlendEquationSeparate(BlendEquationModeEXT modeRGB, BlendEquationModeEXT modeAlpha) => glBlendEquationSeparate(modeRGB, modeAlpha);
        public static void qglBlendFunc(BlendingFactor sfactor, BlendingFactor dfactor) => glBlendFunc(sfactor, dfactor);
        public static void qglBlendFuncSeparate(BlendingFactor sfactorRGB, BlendingFactor dfactorRGB, BlendingFactor sfactorAlpha, BlendingFactor dfactorAlpha) => glBlendFuncSeparate(sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha);
        public static void qglBufferData(BufferTargetARB target, int size, void* data, BufferUsageARB usage) => glBufferData(target, size, data, usage);
        public static void qglBufferSubData(BufferTargetARB target, nint offset, int size, void* data) => glBufferSubData(target, offset, size, data);
        public static FramebufferStatus qglCheckFramebufferStatus(FramebufferTarget target) => glCheckFramebufferStatus(target);
        public static void qglClear(uint mask) => glClear(mask);
        public static void qglClearColor(float red, float green, float blue, float alpha) => glClearColor(red, green, blue, alpha);
        public static void qglClearDepthf(float d) => glClearDepthf(d);
        public static void qglClearStencil(int s) => glClearStencil(s);
        public static void qglColorMask(bool red, bool green, bool blue, bool alpha) => glColorMask(red, green, blue, alpha);
        public static void qglCompileShader(uint shader) => glCompileShader(shader);
        public static void qglCompressedTexImage2D(TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, int imageSize, void* data) => glCompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, data);
        public static void qglCompressedTexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, int imageSize, void* data) => glCompressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, data);
        public static void qglCopyTexImage2D(TextureTarget target, int level, InternalFormat internalformat, int x, int y, int width, int height, int border) => glCopyTexImage2D(target, level, internalformat, x, y, width, height, border);
        public static void qglCopyTexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int x, int y, int width, int height) => glCopyTexSubImage2D(target, level, xoffset, yoffset, x, y, width, height);
        public static uint qglCreateProgram() => glCreateProgram();
        public static uint qglCreateShader(ShaderType type) => glCreateShader(type);
        public static void qglCullFace(CullFaceMode mode) => glCullFace(mode);
        public static void qglDeleteBuffers(int n, uint* buffers) => glDeleteBuffers(n, buffers);
        public static void qglDeleteFramebuffers(int n, uint* framebuffers) => glDeleteFramebuffers(n, framebuffers);
        public static void qglDeleteProgram(uint program) => glDeleteProgram(program);
        public static void qglDeleteRenderbuffers(int n, uint* renderbuffers) => glDeleteRenderbuffers(n, renderbuffers);
        public static void qglDeleteShader(uint shader) => glDeleteShader(shader);
        public static void qglDeleteTextures(int n, uint* textures) => glDeleteTextures(n, textures);
        public static void qglDepthFunc(DepthFunction func) => glDepthFunc(func);
        public static void qglDepthMask(bool flag) => glDepthMask(flag);
        public static void qglDepthRangef(float n, float f) => glDepthRangef(n, f);
        public static void qglDetachShader(uint program, uint shader) => glDetachShader(program, shader);
        public static void qglDisable(EnableCap cap) => glDisable(cap);
        public static void qglDisableVertexAttribArray(uint index) => glDisableVertexAttribArray(index);
        public static void qglDrawArrays(PrimitiveType mode, int first, int count) => glDrawArrays(mode, first, count);
        public static void qglDrawElements(PrimitiveType mode, int count, DrawElementsType type, void* indices) => glDrawElements(mode, count, type, indices);
        public static void qglEnable(EnableCap cap) => glEnable(cap);
        public static void qglEnableVertexAttribArray(uint index) => glEnableVertexAttribArray(index);
        public static void qglFinish() => glFinish();
        public static void qglFlush() => glFlush();
        public static void qglFramebufferRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, uint renderbuffer) => glFramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
        public static void qglFramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level) => glFramebufferTexture2D(target, attachment, textarget, texture, level);
        public static void qglFrontFace(FrontFaceDirection mode) => glFrontFace(mode);
        public static void qglGenBuffers(int n, out uint buffers) { uint buffers_; glGenBuffers(n, &buffers_); buffers = buffers_; }
        public static void qglGenerateMipmap(TextureTarget target) => glGenerateMipmap(target);
        public static void qglGenFramebuffers(int n, uint* framebuffers) => glGenFramebuffers(n, framebuffers);
        public static void qglGenRenderbuffers(int n, uint* renderbuffers) => glGenRenderbuffers(n, renderbuffers);
        public static void qglGenTextures(int n, uint* textures) => glGenTextures(n, textures);
        public static void qglGetActiveAttrib(uint program, uint index, int bufSize, int* length, int* size, uint* type, char* name) => glGetActiveAttrib(program, index, bufSize, length, size, type, name);
        public static void qglGetActiveUniform(uint program, uint index, int bufSize, int* length, int* size, uint* type, char* name) => glGetActiveUniform(program, index, bufSize, length, size, type, name);
        public static void qglGetAttachedShaders(uint program, int maxCount, int* count, uint* shaders) => glGetAttachedShaders(program, maxCount, count, shaders);
        public static int qglGetAttribLocation(uint program, string name) { fixed (char* nameC = name) return glGetAttribLocation(program, nameC); }
        public static void qglGetBooleanv(GetPName pname, bool* data) => glGetBooleanv(pname, data);
        public static void qglGetBufferParameteriv(BufferTargetARB target, uint pname, int* @params) => glGetBufferParameteriv(target, pname, @params);
        public static ErrorCode qglGetError() => glGetError();
        public static void qglGetFloatv(GetPName pname, out float data) { float _; glGetFloatv(pname, &_); data = _; }
        public static void qglGetFramebufferAttachmentParameteriv(FramebufferTarget target, FramebufferAttachment attachment, FramebufferAttachmentParameterName pname, int* @params) => glGetFramebufferAttachmentParameteriv(target, attachment, pname, @params);
        public static void qglGetIntegerv(GetPName pname, out int data) { int _; glGetIntegerv(pname, &_); data = _; }
        public static void qglGetProgramiv(uint program, ProgramPropertyARB pname, int* @params) => glGetProgramiv(program, pname, @params);
        public static void qglGetProgramInfoLog(uint program, int bufSize, int* length, char* infoLog) => glGetProgramInfoLog(program, bufSize, length, infoLog);
        public static void qglGetRenderbufferParameteriv(RenderbufferTarget target, RenderbufferParameterName pname, int* @params) => glGetRenderbufferParameteriv(target, pname, @params);
        public static void qglGetShaderiv(uint shader, ShaderParameterName pname, int* @params) => glGetShaderiv(shader, pname, @params);
        public static void qglGetShaderInfoLog(uint shader, int bufSize, int* length, char* infoLog) => glGetShaderInfoLog(shader, bufSize, length, infoLog);
        public static void qglGetShaderPrecisionFormat(ShaderType shadertype, PrecisionType precisiontype, int* range, int* precision) => glGetShaderPrecisionFormat(shadertype, precisiontype, range, precision);
        public static void qglGetShaderSource(uint shader, int bufSize, int* length, char* source) => glGetShaderSource(shader, bufSize, length, source);
        public static string qglGetString(StringName name) => new((char*)glGetString(name));
        public static void qglGetTexParameterfv(TextureTarget target, GetTextureParameter pname, float* @params) => glGetTexParameterfv(target, pname, @params);
        public static void qglGetTexParameteriv(TextureTarget target, GetTextureParameter pname, int* @params) => glGetTexParameteriv(target, pname, @params);
        public static void qglGetUniformfv(uint program, int location, float* @params) => glGetUniformfv(program, location, @params);
        public static void qglGetUniformiv(uint program, int location, int* @params) => glGetUniformiv(program, location, @params);
        public static int qglGetUniformLocation(uint program, string name) { fixed (char* nameC = name) return glGetUniformLocation(program, nameC); }
        public static void qglGetVertexAttribfv(uint index, uint pname, float* @params) => glGetVertexAttribfv(index, pname, @params);
        public static void qglGetVertexAttribiv(uint index, uint pname, int* @params) => glGetVertexAttribiv(index, pname, @params);
        public static void qglGetVertexAttribPointerv(uint index, uint pname, void** pointer) => glGetVertexAttribPointerv(index, pname, pointer);
        public static void qglHint(HintTarget target, HintMode mode) => glHint(target, mode);
        public static bool qglIsBuffer(uint buffer) => glIsBuffer(buffer);
        public static bool qglIsEnabled(EnableCap cap) => glIsEnabled(cap);
        public static bool qglIsFramebuffer(uint framebuffer) => glIsFramebuffer(framebuffer);
        public static bool qglIsProgram(uint program) => glIsProgram(program);
        public static bool qglIsRenderbuffer(uint renderbuffer) => glIsRenderbuffer(renderbuffer);
        public static bool qglIsShader(uint shader) => glIsShader(shader);
        public static bool qglIsTexture(uint texture) => glIsTexture(texture);
        public static void qglLineWidth(float width) => glLineWidth(width);
        public static void qglLinkProgram(uint program) => glLinkProgram(program);
        public static void qglPixelStorei(PixelStoreParameter pname, int param) => glPixelStorei(pname, param);
        public static void qglPolygonOffset(float factor, float units) => glPolygonOffset(factor, units);
        public static void qglReadPixels(int x, int y, int width, int height, PixelFormat format, VertexAttribPointerType type, void* pixels) => glReadPixels(x, y, width, height, format, (PixelType)type, pixels);
        public static void qglReleaseShaderCompiler() => glReleaseShaderCompiler();
        public static void qglRenderbufferStorage(RenderbufferTarget target, InternalFormat internalformat, int width, int height) => glRenderbufferStorage(target, internalformat, width, height);
        public static void qglSampleCoverage(float value, bool invert) => glSampleCoverage(value, invert);
        public static void qglScissor(int x, int y, int width, int height) => glScissor(x, y, width, height);
        public static void qglShaderBinary(int count, uint* shaders, uint binaryformat, void* binary, int length) => glShaderBinary(count, shaders, binaryformat, binary, length);
        public static void qglShaderSource(uint shader, int count, string @string) { fixed (char* stringC = @string) glShaderSource(shader, count, (IntPtr)stringC, null); }
        public static void qglStencilFunc(StencilFunction func, int @ref, uint mask) => glStencilFunc(func, @ref, mask);
        public static void qglStencilFuncSeparate(StencilFaceDirection face, StencilFunction func, int @ref, uint mask) => glStencilFuncSeparate(face, func, @ref, mask);
        public static void qglStencilMask(uint mask) => glStencilMask(mask);
        public static void qglStencilMaskSeparate(StencilFaceDirection face, uint mask) => glStencilMaskSeparate(face, mask);
        public static void qglStencilOp(StencilOp fail, StencilOp zfail, StencilOp zpass) => glStencilOp(fail, zfail, zpass);
        public static void qglStencilOpSeparate(StencilFaceDirection face, StencilOp sfail, StencilOp dpfail, StencilOp dppass) => glStencilOpSeparate(face, sfail, dpfail, dppass);
        public static void qglTexImage2D(TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, PixelFormat format, VertexAttribPointerType type, void* pixels) => glTexImage2D(target, level, (int)internalformat, width, height, border, format, (PixelType)type, pixels);
        public static void qglTexParameterf(TextureTarget target, GetTextureParameter pname, float param) => glTexParameterf(target, (TextureParameterName)pname, param);
        public static void qglTexParameterfv(TextureTarget target, GetTextureParameter pname, float* @params) => glTexParameterfv(target, (TextureParameterName)pname, @params);
        public static void qglTexParameteri(TextureTarget target, GetTextureParameter pname, int param) => glTexParameteri(target, (TextureParameterName)pname, param);
        public static void qglTexParameteriv(TextureTarget target, GetTextureParameter pname, int* @params) => glTexParameteriv(target, (TextureParameterName)pname, @params);
        public static void qglTexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, VertexAttribPointerType type, void* pixels) => glTexSubImage2D(target, level, xoffset, yoffset, width, height, format, (PixelType)type, pixels);
        public static void qglUniform1f(int location, float v0) => glUniform1f(location, v0);
        public static void qglUniform1fv(int location, int count, float* value) => glUniform1fv(location, count, value);
        public static void qglUniform1i(int location, int v0) => glUniform1i(location, v0);
        public static void qglUniform1iv(int location, int count, int* value) => glUniform1iv(location, count, value);
        public static void qglUniform2f(int location, float v0, float v1) => glUniform2f(location, v0, v1);
        public static void qglUniform2fv(int location, int count, float* value) => glUniform2fv(location, count, value);
        public static void qglUniform2i(int location, int v0, int v1) => glUniform2i(location, v0, v1);
        public static void qglUniform2iv(int location, int count, int* value) => glUniform2iv(location, count, value);
        public static void qglUniform3f(int location, float v0, float v1, float v2) => glUniform3f(location, v0, v1, v2);
        public static void qglUniform3fv(int location, int count, float* value) => glUniform3fv(location, count, value);
        public static void qglUniform3i(int location, int v0, int v1, int v2) => glUniform3i(location, v0, v1, v2);
        public static void qglUniform3iv(int location, int count, int* value) => glUniform3iv(location, count, value);
        public static void qglUniform4f(int location, float v0, float v1, float v2, float v3) => glUniform4f(location, v0, v1, v2, v3);
        public static void qglUniform4fv(int location, int count, float* value) => glUniform4fv(location, count, value);
        public static void qglUniform4i(int location, int v0, int v1, int v2, int v3) => glUniform4i(location, v0, v1, v2, v3);
        public static void qglUniform4iv(int location, int count, int* value) => glUniform4iv(location, count, value);
        public static void qglUniformMatrix2fv(int location, int count, bool transpose, float* value) => glUniformMatrix2fv(location, count, transpose, value);
        public static void qglUniformMatrix3fv(int location, int count, bool transpose, float* value) => glUniformMatrix3fv(location, count, transpose, value);
        public static void qglUniformMatrix4fv(int location, int count, bool transpose, float* value) => glUniformMatrix4fv(location, count, transpose, value);
        public static void qglUseProgram(uint program) => glUseProgram(program);
        public static void qglValidateProgram(uint program) => glValidateProgram(program);
        public static void qglVertexAttrib1f(uint index, float x) => glVertexAttrib1f(index, x);
        public static void qglVertexAttrib1fv(uint index, float* v) => glVertexAttrib1fv(index, v);
        public static void qglVertexAttrib2f(uint index, float x, float y) => glVertexAttrib2f(index, x, y);
        public static void qglVertexAttrib2fv(uint index, float* v) => glVertexAttrib2fv(index, v);
        public static void qglVertexAttrib3f(uint index, float x, float y, float z) => glVertexAttrib3f(index, x, y, z);
        public static void qglVertexAttrib3fv(uint index, float* v) => glVertexAttrib3fv(index, v);
        public static void qglVertexAttrib4f(uint index, float x, float y, float z, float w) => glVertexAttrib4f(index, x, y, z, w);
        public static void qglVertexAttrib4fv(uint index, float* v) => glVertexAttrib4fv(index, v);
        public static void qglVertexAttribPointer(uint index, int size, VertexAttribPointerType type, bool normalized, int stride, void* pointer) => glVertexAttribPointer(index, size, type, normalized, stride, pointer);
        public static void qglViewport(int x, int y, int width, int height) => glViewport(x, y, width, height);

        //: gl
        //public static void qglBegin(PrimitiveType type) { }
        //public static void qglEnd() { }
        //public static void qglVertex3f(float x, float y, float z) { }
        //public static void qglColor3f(float x, float y, float z) { }
        //public static void qglColor3fv(float* values) { }
        //public static void qglPushAttrib(int attrib) { }
        //public static void qglPushMatrix() => glPushMatrix();
        //public static void qglPopAttrib() { }
        //public static void qglPopMatrix() => glPopMatrix();
        //public static void qglLoadIdentity() => glLoadIdentity();
        //public static void qglLoadMatrixf(float* m) => glLoadMatrixf(m);

        //: added
        public static uint qglGetUniformBlockIndex(uint program, string uniformBlockName) { fixed (char* uniformBlockNameC = uniformBlockName) return glGetUniformBlockIndex(program, uniformBlockNameC); }
    }
}