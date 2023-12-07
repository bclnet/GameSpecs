# finds a type
@staticmethod
def findType(klass):
    from importlib import import_module
    klass, modulePath = klass.rsplit(',', 1)
    try:
        _, className = klass.rsplit('.', 1)
        moduleName = f'game_specs.{modulePath.strip().replace('.', '_')}'
        module = import_module(moduleName)
        return getattr(module, className)
    except (ImportError, AttributeError) as e: raise ImportError(moduleName, className, klass)
