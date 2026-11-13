"""A short cross-process transaction lock for the local authoring campaign.

The OS owns the lock; process exit releases it. There is deliberately no age-based
takeover of a live authoring claim. Generation never runs inside this lock.
"""
from contextlib import contextmanager
import errno
import os
from pathlib import Path
import time


@contextmanager
def transaction(work_root, campaign_id, timeout=20.0):
    directory = Path(work_root) / "campaigns" / ".coordination-locks"
    directory.mkdir(parents=True, exist_ok=True)
    path = directory / (campaign_id + ".lock")
    handle = path.open("a+b")
    # msvcrt locks a real byte; two initializers may append, which is harmless.
    if handle.seek(0, os.SEEK_END) == 0:
        handle.write(b"0")
        handle.flush()
    acquired = False
    deadline = time.monotonic() + timeout
    try:
        while True:
            handle.seek(0)
            try:
                if os.name == "nt":
                    import msvcrt
                    msvcrt.locking(handle.fileno(), msvcrt.LK_NBLCK, 1)
                else:
                    import fcntl
                    fcntl.flock(handle.fileno(), fcntl.LOCK_EX | fcntl.LOCK_NB)
                acquired = True
                break
            except OSError as error:
                if error.errno not in {errno.EACCES, errno.EAGAIN, errno.EDEADLK, errno.EBUSY}:
                    raise
                if time.monotonic() >= deadline:
                    raise ValueError("Another campaign command is updating shared state. No work was claimed or overwritten. Retry this exact command after that command finishes.") from error
                time.sleep(0.1)
        yield
    finally:
        if acquired:
            handle.seek(0)
            if os.name == "nt":
                import msvcrt
                msvcrt.locking(handle.fileno(), msvcrt.LK_UNLCK, 1)
            else:
                import fcntl
                fcntl.flock(handle.fileno(), fcntl.LOCK_UN)
        handle.close()
