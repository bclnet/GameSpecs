from __future__ import annotations
from argparse import ArgumentParser

from . import list, get, test

def register_commands(parser: ArgumentParser) -> None:
    subparsers = parser.add_subparsers(help="All cmd subcommands")
    list.register(subparsers)
    get.register(subparsers)
    test.register(subparsers)