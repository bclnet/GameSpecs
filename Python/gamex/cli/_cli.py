from __future__ import annotations

import sys, logging, argparse, httpx
from typing import Any, List, Type, Optional
from typing_extensions import ClassVar
from pydantic import BaseModel, ValidationError
from .. import __version__
from ._utils import can_use_http2
from ._errors import CLIError, display_error
from ._main import register_commands

logger = logging.getLogger()
formatter = logging.Formatter("[%(asctime)s] %(message)s")
handler = logging.StreamHandler(sys.stderr)
handler.setFormatter(formatter)
logger.addHandler(handler)

class Arguments(BaseModel):
    verbosity: int
    version: Optional[str] = None
    proxy: Optional[List[str]]
    # internal, set by subparsers to parse their specific args
    args_model: Optional[Type[BaseModel]] = None
    # internal, used so that subparsers can forward unknown arguments
    unknown_args: List[str] = []
    allow_unknown_args: bool = False

def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=None, prog="gamex")
    parser.add_argument("-v", "--verbose", action="count", dest="verbosity", default=0, help="set verbosity")
    parser.add_argument("-V", "--version", action="version", version="%(prog)s " + __version__)
    parser.add_argument("-p", "--proxy", nargs="+", help="set proxy to use")
    def help() -> None: parser.print_help()
    parser.set_defaults(func=help)
    register_commands(parser)
    return parser

def main() -> int:
    try:
        _main()
    except (CLIError, ValidationError) as err:
        display_error(err)
        return 1
    except KeyboardInterrupt:
        sys.stderr.write("\n")
        return 1
    return 0

def _parse_args(parser: argparse.ArgumentParser) -> tuple[argparse.Namespace, Arguments, list[str]]:
    # argparse by default will strip out the `--` but we want to keep it for unknown arguments
    if "--" in sys.argv:
        idx = sys.argv.index("--")
        known_args = sys.argv[1:idx]
        unknown_args = sys.argv[idx:]
    else:
        known_args = sys.argv[1:]
        unknown_args = []

    parsed, remaining_unknown = parser.parse_known_args(known_args)

    # append any remaining unknown arguments from the initial parsing
    remaining_unknown.extend(unknown_args)

    args = Arguments.model_validate(vars(parsed))
    if not args.allow_unknown_args:
        # we have to parse twice to ensure any unknown arguments
        # result in an error if that behaviour is desired
        parser.parse_args()

    return parsed, args, remaining_unknown

def _main() -> None:
    parser = _build_parser()
    parsed, args, unknown = _parse_args(parser)

    if args.verbosity != 0:
        sys.stderr.write("Warning: --verbosity isn't supported yet\n")

    proxies = {}
    if args.proxy is not None:
        for proxy in args.proxy:
            key = "https://" if proxy.startswith("https") else "http://"
            if key in proxies:
                raise CLIError(f"Multiple {key} proxies given - only the last one would be used")
            proxies[key] = proxy

    http_client = httpx.Client(
        proxies=proxies or None,
        http2=can_use_http2(),
    )
    # openai.http_client = http_client

    try:
        if args.args_model:
            parsed.func(
                args.args_model.model_validate({
                    **{
                        # we omit None values so that they can be defaulted to `NotGiven`
                        # and we'll strip it from the API request
                        key: value
                        for key, value in vars(parsed).items()
                        if value is not None
                    },
                    "unknown_args": unknown,
                })
            )
        else:
            parsed.func()
    finally:
        try:
            http_client.close()
        except Exception:
            pass

if __name__ == "__main__":
    sys.exit(main())