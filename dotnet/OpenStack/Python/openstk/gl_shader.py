import re, math, numpy as np
from importlib import resources
from OpenGL.GL import *
from openstk.gfx_render import Shader

ShaderSeed = 0x13141516
RenderMode = 'renderMode_'; RenderModeLength = len(RenderMode)

# ShaderLoader
class ShaderLoader:
    _cachedShaders: dict[int, Shader] = {}
    _shaderDefines: dict[str, list[str]] = {}
    
    def _calculateShaderCacheHash(self, shaderFileName: str, args: dict[str, bool]) -> int:
        # b = [shaderFileName]
        # parameters = self._shaderDefines[shaderFileName].intersect(args.keys)
        # foreach (var key in parameters)
        # {
        #     b.AppendLine(key);
        #     b.AppendLine(args[key] ? "t" : "f");
        # }
        # return MurmurHash2.Hash(b.ToString(), ShaderSeed)
        return 0

    def getShaderFileByName(self, shaderName: str) -> str: pass

    def getShaderSource(self, shaderName: str) -> str: pass

    def loadShader(self, shaderName: str, args: dict[str, bool]) -> Shader:
        shaderFileName = self.getShaderFileByName(shaderName)
        
        # cache
        # if shaderFileName in self._shaderDefines:
        #     shaderCacheHash = self._calculateShaderCacheHash(shaderFileName, args)
        #     if shaderCacheHash in self._cachedShaders: return self._cachedShaders[shaderCacheHash]

        # build
        defines = []

        # vertex shader
        vertexShader = glCreateShader(GL_VERTEX_SHADER)
        if True:
            shaderSource = self.getShaderSource(f'{shaderFileName}.vert')
            glShaderSource(vertexShader, self.preprocessVertexShader(shaderSource, args))
            # find defines supported from source
            defines += self.findDefines(shaderSource)
        glCompileShader(vertexShader)
        shaderStatus = glGetShaderiv(vertexShader, GL_COMPILE_STATUS)
        if shaderStatus != 1:
            vsInfo = glGetShaderInfoLog(vertexShader)
            raise Exception(f'Error setting up Vertex Shader "{shaderName}": {vsInfo}')

        # fragment shader
        fragmentShader = glCreateShader(GL_FRAGMENT_SHADER)
        if True:
            shaderSource = self.getShaderSource(f'{shaderFileName}.frag')
            glShaderSource(fragmentShader, self.updateDefines(shaderSource, args))
            # find render modes supported from source, take union to avoid duplicates
            defines += self.findDefines(shaderSource)
        glCompileShader(fragmentShader)
        shaderStatus = glGetShaderiv(fragmentShader, GL_COMPILE_STATUS)
        if shaderStatus != 1:
            fsInfo = glGetShaderInfoLog(fragmentShader)
            raise Exception(f'Error setting up Fragment Shader "{shaderName}": {fsInfo}')

        renderModes = [k[RenderModeLength] for k in defines if k.startswith(RenderMode)]

        shader = Shader(glGetUniformLocation,
            name = shaderName,
            parameters = args,
            program = glCreateProgram(),
            renderModes = renderModes
            )
        glAttachShader(shader.program, vertexShader)
        glAttachShader(shader.program, fragmentShader)
        glLinkProgram(shader.program)
        glValidateProgram(shader.program)
        linkStatus = glGetProgramiv(shader.program, GL_LINK_STATUS)
        if linkStatus != 1:
            linkInfo = glGetProgramInfoLog(shader.program)
            raise Exception(f'Error linking shaders: {linkInfo} (link status = {linkStatus})')

        glDetachShader(shader.program, vertexShader)
        glDeleteShader(vertexShader)
        glDetachShader(shader.program, fragmentShader)
        glDeleteShader(fragmentShader)

        self._shaderDefines[shaderFileName] = defines
        newShaderCacheHash = self._calculateShaderCacheHash(shaderFileName, args)
        self._cachedShaders[newShaderCacheHash] = shader
        print(f'Shader {newShaderCacheHash} ({shaderName}) ({', '.join(args.Keys)}) compiled and linked succesfully')
        return shader

    def loadPlaneShader(self, shaderName: str, args: dict[str, bool]) -> Shader:
        shaderFileName = self.getShaderFileByName(shaderName)

        # vertex shader
        vertexShader = glCreateShader(GL_VERTEX_SHADER)
        if True:
            shaderSource = self.getShaderSource('plane.vert')
            glShaderSource(vertexShader, shaderSource)
        glCompileShader(vertexShader)
        shaderStatus = glGetShaderiv(vertexShader, GL_COMPILE_STATUS)
        if shaderStatus != 1:
            vsInfo = glGetShaderInfoLog(vertexShader)
            raise Exception(f'Error setting up Vertex Shader "{shaderName}": {vsInfo}')

        # fragment shader
        fragmentShader = glCreateShader(GL_FRAGMENT_SHADER)
        if True:
            shaderSource = self.getShaderSource(f'{shaderFileName}.frag')
            glShaderSource(fragmentShader, self.updateDefines(shaderSource, args))
        glCompileShader(fragmentShader)
        shaderStatus = glGetShaderiv(fragmentShader, GL_COMPILE_STATUS)
        if shaderStatus != 1:
            fsInfo = glGetShaderInfoLog(fragmentShader)
            raise Exception(f'Error setting up Fragment Shader "{shaderName}": {fsInfo}')

        shader = Shader(glGetUniformLocation,
            name = shaderName,
            program = glCreateProgram()
            )

        glAttachShader(shader.program, vertexShader)
        glAttachShader(shader.program, fragmentShader)
        glLinkProgram(shader.program)
        glValidateProgram(shader.program)
        linkStatus = glGetProgramiv(shader.program, GL_LINK_STATUS)
        if linkStatus != 1:
            linkInfo = glGetProgramInfoLog(shader.program)
            raise Exception(f'Error linking shaders: {linkInfo} (link status = {linkStatus})')

        glDetachShader(shader.program, vertexShader)
        glDeleteShader(vertexShader)
        glDetachShader(shader.program, fragmentShader)
        glDeleteShader(fragmentShader)
        return shader

    # Preprocess a vertex shader's source to include the #version plus #defines for parameters
    def preprocessVertexShader(self, source: str, args: dict[str, bool]) -> str:
        # Update parameter defines
        paramSource = self.updateDefines(source, args)
        # Inject code into shader based on #includes
        includedSource = self.resolveIncludes(paramSource)
        return includedSource

    # Update default defines with possible overrides from the model
    @staticmethod
    def updateDefines(source: str, args: dict[str, bool]) -> str:
        # Find all #define param_(paramName) (paramValue) using regex
        defines = re.compile('#define param_(\\S*?) (\\S*?)\\s*?\\n').finditer(source)
        for define in defines:
            # Check if this parameter is in the arguments
            if (key := define[1]) in args:
                # Overwrite default value
                start, end = define.span(2)
                source = source[:start] + ('1' if args[key] else '0') + source[end:]
        return source

    # Remove any #includes from the shader and replace with the included code
    def resolveIncludes(self, source: str) -> str:
        includes = re.compile('#include "([^"]*?)";?\\s*\\n').finditer(source)
        for define in includes:
            # Read included code
            includedCode = self.getShaderSource(define[1])
            # Recursively resolve includes in the included code. (Watch out for cyclic dependencies!)
            includedCode = self.resolveIncludes(includedCode)
            if not includedCode.endswith('\n'): includedCode += '\n'
            # Replace the include with the code
            start, end = define.span(0)
            source = source.replace(source[start:end], includedCode)
        return source

    @staticmethod
    def findDefines(source: str) -> list[str]:
        defines = re.compile('#define param_(\\S+)').finditer(source)
        return [x[1] for x in defines]

# ShaderDebugLoader
class ShaderDebugLoader(ShaderLoader):
    def getShaderFileByName(self, shaderName: str) -> str:
        match shaderName:
            case 'plane': return 'plane'
            case 'vrf.error': return 'error'
            case 'vrf.grid': return 'debug_grid'
            case 'vrf.picking': return 'picking'
            case 'vrf.particle.sprite': return 'particle_sprite'
            case 'vrf.particle.trail': return 'particle_trail'
            case 'tools_sprite.vfx': return 'sprite'
            case 'vr_unlit.vfx': return 'vr_unlit'
            case 'vr_black_unlit.vfx': return 'vr_black_unlit'
            case 'water_dota.vfx': return 'water'
            case 'hero.vfx', 'hero_underlords.vfx': return 'dota_hero'
            case 'multiblend.vfx': return 'multiblend'
            case _:
                if shaderName.startsWith('vr_'): return 'vr_standard'
                return 'simple'

    def getShaderSource(self, shaderFileName: str) -> str:
        return resources.files().joinpath('shaders', shaderFileName).read_text(encoding='utf-8')

# print(ShaderDebugLoader().loadShader('water_dota.vfx', {'fulltangent': 1}))