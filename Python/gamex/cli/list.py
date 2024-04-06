from __future__ import annotations
from typing import TYPE_CHECKING, Any, Optional, cast
from argparse import ArgumentParser, _SubParsersAction
from pydantic import BaseModel
from .. import families, getFamily

def register(subparser: _SubParsersAction[ArgumentParser]) -> None:
    sub = subparser.add_parser("list", help="list files contents")
    # optional
    sub.add_argument("-f", "--family", type=str, help="Family")
    sub.add_argument("-u", "--uri", type=str, help="Pak file to be list")
    sub.set_defaults(func=list, args_model=CLIListArgs)

class CLIListArgs(BaseModel):
    family: Optional[str] = None
    uri: Optional[str] = None

@staticmethod
def list(args: CLIListArgs) -> None:
    # list families
    if not args.family:
        print('Families installed:')
        for id,val in families.items():
            print(f'  {id} - {val.name}')
        return 1
    
    # get family
    family = getFamily(args.family, False)
    if not family: print(f'No family found named "{args.family}".'); return 0
    
    # list found paths in family
    if not args.uri:
        print(f'{family.name}\nDescription: {family.description}\nStudio: {family.studio}')
        print('\nGames:')
        for game in family.games.values():
            # print(f'{game.name}{f" -> [','.join(game.paks)]" if game.found else ""}')
            print(f'{game.name}{f" -> found" if game.found else ""}')
        paths = family.fileManager.paths
        print('\nPaths:')
        if not paths:
            print(f'No paths found for family "{args.family}".')
            return
        for id,val in paths.items():
            print(f'{family.getGame(id)} - {', '.join(val)}')
        return
    
    # list files in pack for family
    else:
        print(f'{family.name} - {args.uri}')
        with family.openPakFile(args.uri) as f:
            # if not len(f.pakFiles):
            #     print('No paks found.')
            #     return
            print(f)