import os
from typing import Any
from .platform import Startups, Type, PlatformType, GraphicFactory, LogFunc, startup

class OpenGLGraphic:
    def __init__(self, source):
        self._source = source

class OpenGLObjectBuilder(ObjectBuilderBase):
    pass

class OpenGLPlatform:
    def startup() -> bool:
        PlatformType = Type.OpenGL
        GraphicFactory = lambda source: OpenGLGraphic(source)
        LogFunc = lambda a: print(a)
        return True

Startups.append(OpenGLPlatform.startup)
startup()