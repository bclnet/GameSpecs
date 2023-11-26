import os, platform

class StandardFileSystem:
    def __init__(s, root):
        s.root = root

class HostFileSystem:
    def __init__(s, uri):
        s.uri = uri
