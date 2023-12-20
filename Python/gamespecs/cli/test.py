from __future__ import annotations
from typing import TYPE_CHECKING, Any, Optional, cast
from argparse import ArgumentParser, _SubParsersAction
from pydantic import BaseModel

def register(subparser: _SubParsersAction[ArgumentParser]) -> None:
    sub = subparser.add_parser("test", help="commands")
    # required
    sub.add_argument("-f", "--family", type=str, default="Bethesda", help="Family")
    sub.add_argument("-u", "--uri", type=str, default="game:/Morrowind.bsa#Morrowind", help="Pak file to be extracted")
    sub.set_defaults(func=test, args_model=CLITestArgs)

class CLITestArgs(BaseModel):
    family: str
    uri: str

@staticmethod
def test(args: CLITestArgs) -> None:
    print(args)