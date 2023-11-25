from StoreManager

def parse(elem):
    system = platform.system()
    if system == 'Windows': addApplicationByRegistry(elem)
    elif system == 'Linux' or system == 'Darwin': addApplicationByStore(elem)
    addApplicationByDirectory(elem)
    addDirect(elem)

def addApplicationByDirectory(elem):
    print(elem)

def addApplicationByStore(elem):
    print(elem)

def addAddApplicationByRegistry(elem):
    print(elem)

def addDirect(elem):
    print(elem)

def addPath(elem):
    print(elem)
