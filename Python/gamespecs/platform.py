import os
from enum import Enum

class Stats:
    pass

class Type(Enum):
    Unknown = 0
    OpenGL = 1
    Unity = 2
    Vulken = 3
    Test = 4
    Other = 5

class OS(Enum):
    Windows = 1
    OSX = 1
    Linux = 1
    Android = 1

PlatformType:Type = None
PlatformTag:str = None
PlatformOS:OS = OS.Windows
GraphicFactory:object = None
Startups:list[object] = []
InTestHost:bool = False
LogFunc:object = None

class TestGraphic:
    def __init__(self, source):
        self._source = source

class TestPlatform:
    def startup() -> bool:
        PlatformType = Type.Test
        GraphicFactory = lambda source: TestGraphic(source)
        LogFunc = lambda a: print(a)
        return True

# startup
def startup():
    if InTestHost and len(Startups) == 0: Startups.append(TestPlatform.startup)
    for startup in Startups:
        if startup(): return
    PlatformType = Type.Unknown
    GraphicFactory = lambda source: None
    LogFunc = lambda a: print(a)