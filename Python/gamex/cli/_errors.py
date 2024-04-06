from __future__ import annotations
import sys, pydantic
from ._utils import Colors

class CLIError(BaseException):
    ...

class SilentCLIError(CLIError):
    ...

def display_error(err: CLIError | pydantic.ValidationError) -> None:
    if isinstance(err, SilentCLIError): return
    sys.stderr.write("{}Error:{} {}\n".format(Colors.FAIL, Colors.ENDC, err))