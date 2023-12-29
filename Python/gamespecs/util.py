import os
from typing import Any

def _value(elem: dict[str, Any], key: str, default: Any = None) -> Any:
    return elem[key] if key in elem else default

def _list(elem: dict[str, Any], key: str, default: Any = None) -> Any:
    return (elem[key] if isinstance(elem[key], list) else [elem[key]]) if key in elem else default

def _method(elem: dict[str, Any], key: str, method: Any, default: Any = None) -> Any:
    return method(elem[key]) if key in elem else default

def _related(elem: dict[str, Any], key: str, method: Any, default: Any = None) -> Any:
    return { k:method(k, v) for k,v in elem[key].items() } if key in elem else {}
def _relatedTrim(elem: dict[str, Any], key: str, method: Any, default: Any = None) -> Any:
    return { k:v for k,v in { k:method(k, v) for k,v in elem[key].items() }.items() if v } if key in elem else {}

def _guessExtension(buf):
    if len(buf) < 4: return ''
    extensionInt = int.from_bytes(buf, 'little', signed=False)
    extension = f'.{buf[0:3].decode('ascii', 'ignore')}' if extensionInt != 0x75B22630 else '.asf'
    return extension.lower()

def grammerSize(i):
    t, c=['','K','M','G','T'], 0
    while i > 1024: i /= 1024; c += 1
    return f'{round(i, 2)}{t[c]}B'