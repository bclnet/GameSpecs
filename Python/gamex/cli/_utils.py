from __future__ import annotations

class Colors:
    HEADER = "\033[95m"
    OKBLUE = "\033[94m"
    OKGREEN = "\033[92m"
    WARNING = "\033[93m"
    FAIL = "\033[91m"
    ENDC = "\033[0m"
    BOLD = "\033[1m"
    UNDERLINE = "\033[4m"

def can_use_http2() -> bool:
    try:
        import h2  # type: ignore  # noqa
    except ImportError: return False
    return True