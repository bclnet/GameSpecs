# finds a type
@staticmethod
def findType(klass):
    from importlib import import_module
    klass, modulePath = klass.rsplit(',', 1)
    try:
        _, className = klass.rsplit('.', 1)
        module = import_module(moduleName := f'game_specs.{modulePath.strip().replace('.', '_')}')
        return getattr(module, className)
    except (ImportError, AttributeError) as e:
        raise ImportError(klass)
        #moduleName, className 
