import os, platform, json

def getPath():
    return 'G:\\AbandonLibrary'
    
# get abandonPaths
abandonPaths = {}
root = getPath()
if not root or not os.path.exists(root):
# query games
for s in [s for s in os.listdir(root)]:
    abandonPaths[os.path.basepath(s)] = appPath

# print(abandonPaths)
