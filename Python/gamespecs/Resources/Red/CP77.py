import os, pathlib
from zipfile import ZipFile
from io import BytesIO
from importlib import resources

file = resources.files().joinpath('Resources', 'Red/C977.zip').open('rb')
pak: ZipFile = ZipFile(file, 'r')
hashEntries: dict[str, object] = { x.filename:x for x in pak.infolist() }

hashLookup: dict[str, dict[int, str]] = {}
@staticmethod
def getHashLookup(path: str) -> dict[int, str]:
    pass
