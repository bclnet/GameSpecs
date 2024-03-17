import os, time
from openai import OpenAI

# open ai client
seed = 0xdeadbeef
client = OpenAI(api_key=os.environ['OPENAI_API_KEY'])

# yeild paths recursivly and in order
def yieldPaths(folder, extension):
    files = {os.path.basename(f.path):f for f in os.scandir(folder) if f.path.endswith(extension)}
    _intro = files.pop('_intro' + extension, None)
    _summary = files.pop('_summary' + extension, None)

    # introduction
    if _intro: yield _intro.path

    # files
    for f in files.values(): yield f.path

    # directories
    dirs = {os.path.basename(f.path):f for f in os.scandir(folder) if f.is_dir()}
    for f in dirs.values():
        for x in yieldPaths(f.path, extension): yield x

    # summary
    if _summary: yield _summary.path

# openai chat
messages = []
# model = 'gpt-4'
model = 'gpt-3.5-turbo-0125'
def chat(content, reply, callout):
    content = content.strip()

    # get user or system content
    role = 'user'
    if content.startswith('SYSTEM\n'): 
        content = content[7:]; role = 'system'; callout = False
    messages.append({'role': role, 'content': content})
    # print(f'{role}: {content}')

    # get assistant content
    role = 'assistant'
    if callout:
        completion = client.chat.completions.create(seed=seed, model=model, messages=messages)
        reply = completion.choices[0].message.content
        # print(reply)
    messages.append({'role': role, 'content': reply})
    # print(f'{role}: {reply}')

    return reply

# read tags
with open('GPT.txt', 'r') as f: tags = f.read().split('\n')
allTag = len(tags) > 0 and tags[0] == 'all'

# process .gpt files
count = 0; maxcount = 100
for path in yieldPaths('book', '.gpt'):
    count += 1
    if count > maxcount: break
    ascPath = f'{path[:-4]}.asc'
    callout = True if allTag else path in tags

    # print
    print(f'{count:02}:{"X" if callout else "O"} - {path}')

    # process files
    reply = ''
    with open(ascPath, 'r') as f: reply = f.read()
    with open(path, 'r') as f: reply = chat(f.read(), reply, callout)
    with open(ascPath, 'w') as f: f.write(reply)
