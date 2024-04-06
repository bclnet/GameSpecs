from __future__ import annotations
from typing import TYPE_CHECKING, Any, Optional, cast
from argparse import ArgumentParser, _SubParsersAction
from pydantic import BaseModel
from .. import getFamily

def register(subparser: _SubParsersAction[ArgumentParser]) -> None:
    sub = subparser.add_parser("get", help="get files contents")
    # required
    sub.add_argument("-f", "--family", type=str, required=True, help="Family")
    sub.add_argument("-u", "--uri", type=str, required=True, help="Pak file to be extracted")
    # optional
    sub.add_argument("--path", type=str, default="./out", help="Output folder")
    sub.add_argument("--option", type=str, help="Data option")
    sub.set_defaults(func=get, args_model=CLIExportArgs)

class CLIExportArgs(BaseModel):
    family: str
    uri: str
    path: Optional[str] = None
    option: Optional[str] = None

@staticmethod
def get(args: CLIExportArgs) -> None:
    # from_ = ProgramState.Load(data => Convert.ToInt32(data), 0)
    
    # get family
    family = getFamily(args.family)
    if not family: print(f'No family found named "{args.family}".'); return 0

    # print(args)