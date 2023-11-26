import os, json, glob, re

class FileManager:
    def __init__(s, d):
        # applications
        s.applications = applications = {}
        if 'application' in d:
            for id in d['application']:
                applications[id] = Application(id, d['application'][id])
        # directs
        s.directs = directs = {}
        if 'directs' in d:
            for id in d['directs']:
                directs[id] = Direct(id, d['directs'][id])
        # ignores
        s.ignores = ignores = {}
        if 'ignores' in d:
            for id in d['ignores']:
                ignores[id] = Ignore(id, d['ignores'][id])
        # filters
        s.filters = filters = {}
        if 'filters' in d:
            for id in d['filters']:
                filters[id] = Filter(id, d['filters'][id])
    def __repr__(s): return f'''
- applications: {list(s.applications.keys()) if s.applications else None}
- directs: {list(s.directs.keys()) if s.directs else None}
- ignores: {list(s.ignores.keys()) if s.ignores else None}
- filters: {list(s.filters.keys()) if s.filters else None}'''

class Application:
    def __init__(s, id, d):
        s.id = id
        s.dir = (d['dir'] if isinstance(d['dir'], list) else [d['dir']]) if 'dir' in d else None
        s.key = (d['key'] if isinstance(d['key'], list) else [d['key']]) if 'key' in d else None
        s.reg = (d['reg'] if isinstance(d['reg'], list) else [d['reg']]) if 'reg' in d else None
        s.path = (d['path'] if isinstance(d['path'], list) else [d['path']]) if 'path' in d else None
    def __repr__(s): return f'{s.id}'

class Direct:
    def __init__(s, id, d):
        s.id = id
        s.path = (d['path'] if isinstance(d['path'], list) else [d['path']]) if 'path' in d else None
    def __repr__(s): return f'{s.id}'

class Ignore:
    def __init__(s, id, d):
        s.id = id
        s.path = (d['path'] if isinstance(d['path'], list) else [d['path']]) if 'path' in d else None
    def __repr__(s): return f'{s.id}'

class Filter:
    def __init__(s, id, d):
        s.id = id
        s.v = d
    def __repr__(s): return f'{s.id}'
