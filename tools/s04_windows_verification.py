#!/usr/bin/env python3
"""Fixed, bounded S03.04 push admission and dual-origin provenance. No dispatch API."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import signal
import stat
import subprocess
import sys
import threading
import uuid

sys.dont_write_bytecode = True
MARKER = ".kiro/xiigen-slices/luna/s04/windows-verification-request.json"
WORKFLOW = ".github/workflows/s03-windows-verification.yml"
HELPER = "tools/s04_windows_verification.py"
REF = "refs/heads/codex/dpo-implementation"
NATIVE_GATE = "S04.04A-NATIVE-RESOURCE"
MARKER_SCHEMA = "s04-windows-verification-request-v1"
RECEIPT_SCHEMA = "s04-windows-push-admission-v1"
MAX_MARKER = 4096
MAX_EVENT = 1024 * 1024
MAX_RECEIPT = 16384
MAX_RUN_METADATA = 1024 * 1024  # Complete fixed-gate filters and their command copies; not a receipt.
MAX_HELPER = 65536
MAX_WORKFLOW = 1024 * 1024
GIT_SECONDS = 10
NORMAL_GATES = frozenset({*("S03.%02d" % n for n in range(1, 13)), "S03.08.PERF", "S04.02A.C01-IDENTITY-SUBSTRATE"})


def focused_policy(gate: object) -> int:
    require(type(gate) is str and gate in NORMAL_GATES, "normal focused gate")
    return 1800 if gate == "S03.04" else 600


def focused_budget(origin: dict) -> dict:
    return {"schema": "s03-focused-budget-v1", "gate": origin.get("gate"),
            "focused_seconds": focused_policy(origin.get("gate")), "origin_sha256": canonical_event_digest(origin)}


def validate_focused_correspondence(origin: dict, metadata: dict, watchdog: dict, selected: dict, observed: dict) -> None:
    expected = focused_budget(origin)
    for receipt in (selected, observed):
        require(type(receipt) is dict and set(receipt) == set(expected), "focused budget receipt fields")
        require(type(receipt.get("focused_seconds")) is int and receipt == expected, "focused budget/origin binding")
    require(type(metadata) is dict and metadata.get("gate") == expected["gate"], "focused metadata gate")
    declared = metadata.get("test_process_watchdog_seconds")
    require(type(declared) is dict and type(declared.get("focused")) is int and declared["focused"] == expected["focused_seconds"], "focused declared policy")
    require(type(watchdog) is dict and watchdog.get("stage") == "focused", "focused watchdog stage")
    seconds = watchdog.get("watchdog_timeout_seconds")
    require(type(seconds) is int and seconds == expected["focused_seconds"], "focused observed policy")
    # Correspondence is not stage acceptance: TIMED_OUT/nonzero/missing TRX remain RED in the existing strict owner.


class Refused(ValueError):
    pass


def require(condition: bool, reason: str) -> None:
    if not condition:
        raise Refused(reason)


def sha(value: object) -> str:
    require(type(value) is str and re.fullmatch(r"[0-9a-fA-F]{40}", value) is not None and value != "0" * 40, "commit identity")
    return value.lower()


def digest(raw: bytes) -> str:
    return hashlib.sha256(raw).hexdigest()


def canonical_event_digest(event: object) -> str:
    """Full strict-decoded event: sorted keys, ASCII JSON, compact separators, no NaN.

    This Python JSON recipe is deterministic for these decoded values, not JCS.
    Per-job webhook raw hashes remain separate because runner formatting can differ.
    """
    try:
        raw = json.dumps(event, sort_keys=True, ensure_ascii=True, separators=(",", ":"), allow_nan=False).encode("ascii")
    except (ValueError, TypeError, RecursionError) as exc:
        raise Refused("full event canonicalization") from exc
    require(0 < len(raw) <= 6 * MAX_EVENT, "full event canonical byte bound")
    return digest(raw)


def strict_json(raw: bytes, maximum: int) -> object:
    require(type(raw) is bytes and 0 < len(raw) <= maximum, "JSON byte bound")
    def pairs(items):
        result = {}
        for key, value in items:
            require(key not in result, "duplicate JSON key")
            result[key] = value
        return result
    try:
        return json.loads(raw.decode("utf-8", errors="strict"), object_pairs_hook=pairs,
                          parse_constant=lambda _: (_ for _ in ()).throw(Refused("nonfinite JSON")))
    except (UnicodeError, json.JSONDecodeError, RecursionError) as exc:
        raise Refused("invalid UTF8/JSON") from exc


def marker(raw: bytes) -> dict:
    data = strict_json(raw, MAX_MARKER)
    require(type(data) is dict and set(data) == {"schema", "gate", "request_id", "candidate_origin"}, "marker keys")
    require(data["schema"] == MARKER_SCHEMA and data["gate"] == "S03.04" and data["candidate_origin"] == "push-event-after", "marker selector")
    value = data["request_id"]
    require(type(value) is str, "marker UUID type")
    try:
        parsed = uuid.UUID(value)
    except (ValueError, AttributeError) as exc:
        raise Refused("marker UUID") from exc
    require(str(parsed) == value and parsed.version == 4 and parsed.variant == uuid.RFC_4122, "canonical fresh UUID")
    return data


def read_file(path: Path, maximum: int) -> bytes:
    before = path.lstat()
    require(stat.S_ISREG(before.st_mode) and not path.is_symlink() and 0 < before.st_size <= maximum, "regular bounded file")
    with path.open("rb") as stream:
        opened = os.fstat(stream.fileno())
        require((opened.st_dev, opened.st_ino, opened.st_size) == (before.st_dev, before.st_ino, before.st_size), "file identity changed")
        raw = stream.read(maximum + 1)
        after = os.fstat(stream.fileno())
    require(0 < len(raw) <= maximum and len(raw) == before.st_size and
            (after.st_dev, after.st_ino, after.st_size, after.st_mtime_ns) ==
            (before.st_dev, before.st_ino, before.st_size, before.st_mtime_ns), "file bytes changed")
    return raw


def git(root: Path, args: list[str], maximum: int = 4096) -> bytes:
    """Only fixed local Git readers; concurrent finite drains, no lazy network fetch."""
    env = dict(os.environ, GIT_TERMINAL_PROMPT="0", GIT_OPTIONAL_LOCKS="0", GIT_NO_LAZY_FETCH="1", GIT_PAGER="")
    process = subprocess.Popen(["git", *args], cwd=root, env=env, stdin=subprocess.DEVNULL,
                               stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                               start_new_session=os.name != "nt",
                               creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0)
    captured = [bytearray(), bytearray()]
    failed = threading.Event()
    def drain(index, source, bound):
        try:
            while chunk := source.read(65536):
                if len(captured[index]) + len(chunk) > bound:
                    failed.set()
                    process.kill()
                    return
                captured[index].extend(chunk)
        except (OSError, ValueError):
            failed.set()
    threads = [threading.Thread(target=drain, args=(0, process.stdout, maximum), daemon=True),
               threading.Thread(target=drain, args=(1, process.stderr, 2048), daemon=True)]
    for thread in threads:
        thread.start()
    try:
        process.wait(timeout=GIT_SECONDS)
    except subprocess.TimeoutExpired as exc:
        if os.name != "nt":
            os.killpg(process.pid, signal.SIGKILL)
        else:
            process.kill()
        process.wait(timeout=2)
        raise Refused("Git deadline") from exc
    finally:
        for thread in threads:
            thread.join(timeout=2)
        process.stdout.close()
        process.stderr.close()
    require(not failed.is_set() and not any(t.is_alive() for t in threads) and process.returncode == 0 and not captured[1], "Git read failed/overbound")
    return bytes(captured[0])


def checkout_head(root: Path, expected: str) -> str:
    require((root / ".git").is_dir() and not (root / ".git").is_symlink(), "Git checkout required; archive fallback refused")
    actual = sha(git(root, ["rev-parse", "--verify", "HEAD"]).decode("ascii").strip())
    require(actual == sha(expected), "checkout HEAD")
    return actual


def blob(root: Path, commit: str, relative: str, maximum: int, *, absent: bool = False, disk: Path | None = None) -> tuple[dict | None, bytes | None]:
    commit = sha(commit)
    tree = git(root, ["ls-tree", "-z", commit, "--", relative])
    target = root / relative if disk is None else disk
    if not tree:
        require(absent and not target.exists() and not target.is_symlink(), "missing/unknown Git file")
        return None, None
    match = re.fullmatch(rb"(100644|100755) blob ([0-9a-f]{40})\t" + re.escape(relative.encode()) + rb"\x00", tree)
    require(match is not None, "regular Git mode/type/path")
    mode, oid = match[1].decode(), match[2].decode()
    size_raw = git(root, ["cat-file", "-s", oid])
    require(re.fullmatch(rb"[1-9][0-9]*\r?\n", size_raw) is not None, "Git blob size")
    size = int(size_raw)
    require(size <= maximum, "Git blob byte bound")
    raw = git(root, ["cat-file", "blob", oid], maximum)
    require(len(raw) == size and hashlib.sha1(b"blob " + str(size).encode() + b"\0" + raw).hexdigest() == oid, "Git blob bytes/OID")
    require(read_file(target, maximum) == raw, "disk/Git blob bytes")
    identity = {"path": relative, "mode": mode, "oid": oid, "byte_length": size, "sha256": digest(raw)}
    return identity, raw


def marker_blob(root: Path, commit: str, *, absent: bool = False) -> dict:
    identity, raw = blob(root, commit, MARKER, MAX_MARKER, absent=absent)
    if identity is None:
        return {"present": False}
    return {"present": True, **identity, "request_id": marker(raw)["request_id"]}


def context(env: dict) -> dict:
    names = {"event_name": "GITHUB_EVENT_NAME", "repository": "GITHUB_REPOSITORY", "repository_id": "GITHUB_REPOSITORY_ID",
             "ref": "GITHUB_REF", "sha": "GITHUB_SHA", "workflow_sha": "GITHUB_WORKFLOW_SHA", "workflow_ref": "GITHUB_WORKFLOW_REF",
             "run_id": "GITHUB_RUN_ID", "run_attempt": "GITHUB_RUN_ATTEMPT", "job": "GITHUB_JOB"}
    result = {k: env.get(v) for k, v in names.items()}
    require(type(result["repository"]) is str and re.fullmatch(r"[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+", result["repository"]) is not None, "repository context")
    for field in ("repository_id", "run_id", "run_attempt"):
        require(type(result[field]) is str and re.fullmatch(r"[1-9][0-9]{0,19}", result[field]) is not None, "numeric run/repository context")
    result["sha"] = sha(result["sha"])
    result["workflow_sha"] = sha(result["workflow_sha"])
    return result


def push_event(event: object, ctx: dict) -> tuple[str, str]:
    require(ctx["event_name"] == "push" and type(event) is dict, "push event required")
    require(event.get("ref") == ctx["ref"] == REF, "push ref")
    repo = event.get("repository")
    require(type(repo) is dict and repo.get("full_name") == ctx["repository"] and type(repo.get("id")) is int and str(repo["id"]) == ctx["repository_id"], "push repository")
    for field in ("created", "deleted", "forced"):
        require(type(event.get(field)) is bool and event[field] is False, "push lifecycle flags")
    before, after = sha(event.get("before")), sha(event.get("after"))
    require(before != after and after == ctx["sha"] == ctx["workflow_sha"], "push before/after/workflow")
    require(ctx["workflow_ref"] == ctx["repository"] + "/" + WORKFLOW + "@" + REF, "push workflow ref")
    head = event.get("head_commit")
    require(type(head) is dict and sha(head.get("id")) == after, "push head commit")
    return before, after


def route(event_name: str, gate: str, result: str, admitted: object, workflow_cancelled: bool = False) -> str | None:
    if workflow_cancelled:
        return None
    if event_name == "workflow_dispatch":
        return "native" if gate == NATIVE_GATE else "normal"
    if event_name == "push" and result == "success" and admitted == "true":
        return "normal"
    return None


def admission(event: dict, ctx: dict, before_root: Path, after_root: Path, event_hash: str) -> dict:
    require(ctx["job"] == "push-admission", "logical admission job")
    before, after = push_event(event, ctx)
    before_head = checkout_head(before_root, before)
    after_head = checkout_head(after_root, after)
    old = marker_blob(before_root, before, absent=True)
    new = marker_blob(after_root, after)
    require(old.get("request_id") != new["request_id"], "unchanged request UUID")
    old_workflow, _ = blob(before_root, before, WORKFLOW, MAX_WORKFLOW)
    new_workflow, _ = blob(after_root, after, WORKFLOW, MAX_WORKFLOW)
    helper, _ = blob(after_root, after, HELPER, MAX_HELPER)
    return {"schema": RECEIPT_SCHEMA, "admitted": True, "repository": ctx["repository"], "repository_id": ctx["repository_id"],
            "ref": REF, "before": before, "after": after, "workflow_sha": ctx["workflow_sha"], "workflow_ref": ctx["workflow_ref"],
            "run_id": ctx["run_id"], "run_attempt": ctx["run_attempt"], "logical_job": "push-admission", "event_sha256": event_hash,
            "event_canonical_sha256": canonical_event_digest(event),
            "actual_before_head": before_head, "actual_after_head": after_head, "before_marker": old, "after_marker": new,
            "before_workflow": old_workflow, "after_workflow": new_workflow, "executed_helper": helper}


def validate_receipt(receipt: object, event: dict, ctx: dict) -> dict:
    keys = {"schema", "admitted", "repository", "repository_id", "ref", "before", "after", "workflow_sha", "workflow_ref", "run_id", "run_attempt", "logical_job", "event_sha256", "event_canonical_sha256", "actual_before_head", "actual_after_head", "before_marker", "after_marker", "before_workflow", "after_workflow", "executed_helper"}
    require(type(receipt) is dict and set(receipt) == keys and receipt["schema"] == RECEIPT_SCHEMA and receipt["admitted"] is True, "receipt schema")
    before, after = push_event(event, ctx)
    expected = {"repository": ctx["repository"], "repository_id": ctx["repository_id"], "ref": REF, "before": before, "after": after,
                "workflow_sha": ctx["workflow_sha"], "workflow_ref": ctx["workflow_ref"], "run_id": ctx["run_id"], "run_attempt": ctx["run_attempt"],
                "logical_job": "push-admission", "actual_before_head": before, "actual_after_head": after}
    require(all(receipt[k] == v and type(receipt[k]) is str for k, v in expected.items()), "receipt event/run/job binding")
    require(type(receipt["event_sha256"]) is str and re.fullmatch(r"[0-9a-f]{64}", receipt["event_sha256"]) is not None, "receipt event hash")
    require(receipt["event_canonical_sha256"] == canonical_event_digest(event), "receipt full canonical event binding")
    def identity(value, path, maximum, is_marker=False, allow_absent=False):
        if allow_absent and type(value) is dict and set(value) == {"present"} and value["present"] is False:
            return
        fields = {"path", "mode", "oid", "byte_length", "sha256"} | ({"present", "request_id"} if is_marker else set())
        require(type(value) is dict and set(value) == fields and value["path"] == path and value["mode"] in ("100644", "100755"), "receipt file identity")
        require(type(value["oid"]) is str and re.fullmatch(r"[0-9a-f]{40}", value["oid"]) is not None and type(value["sha256"]) is str and re.fullmatch(r"[0-9a-f]{64}", value["sha256"]) is not None, "receipt blob hashes")
        require(type(value["byte_length"]) is int and 0 < value["byte_length"] <= maximum, "receipt blob size")
        if is_marker:
            require(value["present"] is True, "receipt marker presence")
            marker(json.dumps({"schema": MARKER_SCHEMA, "gate": "S03.04", "request_id": value["request_id"], "candidate_origin": "push-event-after"}).encode())
    identity(receipt["before_marker"], MARKER, MAX_MARKER, True, True)
    identity(receipt["after_marker"], MARKER, MAX_MARKER, True)
    require(receipt["before_marker"].get("request_id") != receipt["after_marker"]["request_id"], "receipt UUID delta")
    identity(receipt["before_workflow"], WORKFLOW, MAX_WORKFLOW)
    identity(receipt["after_workflow"], WORKFLOW, MAX_WORKFLOW)
    identity(receipt["executed_helper"], HELPER, MAX_HELPER)
    return receipt


def provenance_origin(record: dict, candidate: str, expected_event: str, expected_workflow: str) -> bool:
    """Shared production matrix predicate; caller separately validates raw bytes."""
    try:
        require(type(record) is dict, "origin record")
        for field in ("trigger_kind", "candidate_origin", "raw_dispatch_candidate", "resolved_candidate", "push_admission"):
            require(field in record, "origin field missing")
        require(sha(record["resolved_candidate"]) == sha(candidate), "resolved candidate")
        require(sha(record["event_sha"]) == sha(expected_event) and sha(record["workflow_commit_sha"]) == sha(expected_workflow), "event/workflow origin")
        if record["trigger_kind"] == "workflow_dispatch":
            require(record["candidate_origin"] == "workflow-dispatch-input" and sha(record["raw_dispatch_candidate"]) == sha(candidate) and record["push_admission"] is None, "manual origin")
        elif record["trigger_kind"] == "push":
            require(record["candidate_origin"] == "push-event-after" and record["raw_dispatch_candidate"] is None and sha(candidate) == sha(expected_event) == sha(expected_workflow), "push origin")
            require(record.get("gate") == "S03.04", "push gate")
            ctx = record["push_context"]
            require(type(ctx) is dict and ctx["sha"] == sha(candidate) and ctx["workflow_sha"] == sha(expected_workflow), "push context candidate")
            receipt = validate_receipt(record["push_admission"], record["push_event"], ctx)
            require(record["current_marker"] == receipt["after_marker"], "push current marker")
        else:
            raise Refused("unknown trigger")
        return True
    except (Refused, KeyError, TypeError, AttributeError):
        return False


def windows_origin(event: dict, ctx: dict, root: Path, helper_disk: Path, receipt_raw: bytes | None, env: dict, event_hash: str) -> dict:
    require(ctx["job"] == "windows-verification", "logical Windows job")
    candidate = sha(env.get("CANDIDATE_SHA"))
    actual = checkout_head(root, candidate)
    executed_helper, _ = blob(root, ctx["workflow_sha"], HELPER, MAX_HELPER, disk=helper_disk)
    if ctx["event_name"] == "workflow_dispatch":
        repo = event.get("repository")
        require(type(repo) is dict and repo.get("full_name") == ctx["repository"] and type(repo.get("id")) is int and str(repo["id"]) == ctx["repository_id"], "manual repository")
        require(type(ctx["ref"]) is str and re.fullmatch(r"refs/(heads|tags)/[^\s]+", ctx["ref"]) is not None and ctx["workflow_ref"] == ctx["repository"] + "/" + WORKFLOW + "@" + ctx["ref"], "manual workflow ref")
        inputs = event.get("inputs")
        require(type(inputs) is dict and sha(inputs.get("candidate_sha")) == candidate and inputs.get("gate", "S03.01") == env.get("GATE_NAME"), "raw manual input")
        require(receipt_raw is None, "manual must not import push receipt")
        kind, raw_input, receipt, current = "workflow-dispatch-input", inputs["candidate_sha"], None, None
    else:
        before, after = push_event(event, ctx)
        require(after == actual and env.get("GATE_NAME") == "S03.04" and receipt_raw is not None, "Windows push source/gate")
        receipt = validate_receipt(strict_json(receipt_raw, MAX_RECEIPT), event, ctx)
        current = marker_blob(root, after)
        workflow, _ = blob(root, after, WORKFLOW, MAX_WORKFLOW)
        require(current == receipt["after_marker"] and workflow == receipt["after_workflow"] and executed_helper == receipt["executed_helper"], "Windows after bytes/receipt")
        expected_outputs = {"ADMISSION_ADMITTED": "true", "ADMISSION_AFTER": after, "ADMISSION_BEFORE": before, "ADMISSION_REQUEST_ID": current["request_id"],
                            "ADMISSION_RUN_ID": ctx["run_id"], "ADMISSION_RUN_ATTEMPT": ctx["run_attempt"], "ADMISSION_LOGICAL_JOB": "push-admission",
                            "ADMISSION_WORKFLOW_SHA": ctx["workflow_sha"], "ADMISSION_RECEIPT_SHA256": digest(receipt_raw),
                            "ADMISSION_EVENT_CANONICAL_SHA256": canonical_event_digest(event)}
        require(all(env.get(k) == v for k, v in expected_outputs.items()), "admission outputs/receipt")
        kind, raw_input = "push-event-after", None
    return {"schema": "s04-windows-verification-origin-v1", "trigger_kind": ctx["event_name"], "candidate_origin": kind,
            "raw_dispatch_candidate": raw_input, "resolved_candidate": candidate, "actual_source_sha": actual,
            "event_sha": ctx["sha"], "workflow_sha": ctx["workflow_sha"], "repository": ctx["repository"], "repository_id": ctx["repository_id"],
            "ref": ctx["ref"], "run_id": ctx["run_id"], "run_attempt": ctx["run_attempt"], "gate": env.get("GATE_NAME"),
            "event_sha256": event_hash, "event_canonical_sha256": canonical_event_digest(event), "executed_helper": executed_helper, "push_admission": receipt, "current_marker": current,
            "before_read_by_windows": False}


def origin_matrix() -> list[dict]:
    """Literal synthetic cases through the production origin/routing predicates."""
    import copy
    before, after = "b" * 40, "a" * 40
    ctx = {"event_name": "push", "repository": "example/project", "repository_id": "17", "ref": REF,
           "sha": after, "workflow_sha": after, "workflow_ref": "example/project/" + WORKFLOW + "@" + REF,
           "run_id": "123", "run_attempt": "2", "job": "windows-verification"}
    event = {"ref": REF, "repository": {"full_name": "example/project", "id": 17}, "before": before, "after": after,
             "created": False, "deleted": False, "forced": False, "head_commit": {"id": after}}
    def ident(path, length):
        return {"path": path, "mode": "100644", "oid": "c" * 40, "byte_length": length, "sha256": "d" * 64}
    current = {"present": True, **ident(MARKER, 196), "request_id": "77e0cc98-1df8-43c5-94ca-cb7a5eb9f760"}
    receipt = {"schema": RECEIPT_SCHEMA, "admitted": True, **{k: ctx[k] for k in ("repository", "repository_id", "ref", "workflow_sha", "workflow_ref", "run_id", "run_attempt")},
               "before": before, "after": after, "logical_job": "push-admission", "event_sha256": "e" * 64,
               "event_canonical_sha256": canonical_event_digest(event),
               "actual_before_head": before, "actual_after_head": after, "before_marker": {"present": False}, "after_marker": current,
               "before_workflow": ident(WORKFLOW, 1024), "after_workflow": ident(WORKFLOW, 2048), "executed_helper": ident(HELPER, 4096)}
    record = {"trigger_kind": "push", "candidate_origin": "push-event-after", "raw_dispatch_candidate": None, "resolved_candidate": after,
              "event_sha": after, "workflow_commit_sha": after, "gate": "S03.04", "push_admission": receipt,
              "push_event": event, "push_context": ctx, "current_marker": current}
    cases = []
    def add(name, expected, value):
        cases.append({"name": name, "expected_pass": expected, "actual_pass": value, "passed": expected == value})
    add("positive-push-origin", True, provenance_origin(record, after, after, after))
    changes = [
        ("reject-push-manual-input", ("raw_dispatch_candidate",), after),
        ("reject-push-wrong-origin", ("candidate_origin",), "workflow-dispatch-input"),
        ("reject-push-event-after", ("push_event", "after"), before),
        ("reject-push-head-commit", ("push_event", "head_commit", "id"), before),
        ("reject-push-forced", ("push_event", "forced"), True),
        ("reject-push-created", ("push_event", "created"), True),
        ("reject-push-deleted", ("push_event", "deleted"), True),
        ("reject-push-ref", ("push_event", "ref"), "refs/heads/main"),
        ("reject-push-repository", ("push_event", "repository", "id"), 18),
        ("reject-push-run-attempt", ("push_admission", "run_attempt"), "1"),
        ("reject-push-logical-job", ("push_admission", "logical_job"), "windows-verification"),
        ("reject-push-unchanged-uuid", ("push_admission", "before_marker"), current),
        ("reject-push-missing-after-marker", ("push_admission", "after_marker"), {"present": False}),
        ("reject-push-current-marker", ("current_marker", "oid"), "f" * 40),
        ("reject-push-receipt-workflow", ("push_admission", "workflow_sha"), before),
        ("reject-push-full-event-binding", ("push_admission", "event_canonical_sha256"), "f" * 64),
        ("reject-push-wrong-gate", ("gate",), NATIVE_GATE),
    ]
    for name, path, value in changes:
        changed = copy.deepcopy(record)
        # Receipt and current marker must be independent observations.
        changed["current_marker"] = copy.deepcopy(current)
        target = changed
        for field in path[:-1]:
            target = target[field]
        target[path[-1]] = copy.deepcopy(value)
        add(name, False, provenance_origin(changed, after, after, after))
    add("reject-push-distinct-workflow", False, provenance_origin(record, after, after, before))
    for name, event_name, gate, result, admitted, expected in (
        ("positive-route-manual-normal", "workflow_dispatch", "S03.01", "skipped", "", "normal"),
        ("positive-route-manual-native", "workflow_dispatch", NATIVE_GATE, "skipped", "", "native"),
        ("positive-route-admitted-push", "push", "S03.04", "success", "true", "normal"),
        ("reject-route-skipped-push", "push", "S03.04", "skipped", "true", None),
        ("reject-route-failed-push", "push", "S03.04", "failure", "true", None),
        ("reject-route-unadmitted-push", "push", "S03.04", "success", "", None),
        ("reject-route-unknown-event", "pull_request", "S03.04", "success", "true", None),
    ):
        actual = route(event_name, gate, result, admitted)
        add(name, True, actual == expected)
    add("reject-route-cancelled-manual", False, route("workflow_dispatch", "S03.01", "skipped", "", True) is not None)
    add("reject-route-cancelled-push", False, route("push", "S03.04", "success", "true", True) is not None)
    return cases


def check_records(origin: dict, evidence_root: Path) -> None:
    materialized = strict_json(read_file(evidence_root / "executed-helper-materialization.json", MAX_RECEIPT), MAX_RECEIPT)
    require(materialized == {"workflow_sha": origin["workflow_sha"], "executed_helper": origin["executed_helper"]}, "materialized/executed W helper binding")
    for name in ("initialization.json", "source-sha.json", "run-metadata.json", "workflow-identity.json"):
        maximum = MAX_RUN_METADATA if name == "run-metadata.json" else MAX_RECEIPT
        record = strict_json(read_file(evidence_root / name, maximum), maximum)
        require(type(record) is dict, "producer record")
        for field in ("trigger_kind", "candidate_origin", "raw_dispatch_candidate", "resolved_candidate"):
            require(field in record and record[field] == origin[field], "producer origin binding")
        if name != "initialization.json":
            actual_field = "actual_sha" if name == "source-sha.json" else "actual_source_sha"
            require(sha(record.get(actual_field)) == origin["actual_source_sha"], "producer source binding")
        if name == "workflow-identity.json":
            require(record.get("executed_helper") == origin["executed_helper"] and record.get("verification_origin") == origin, "executed helper/origin binding")


def write_json(path: Path, value: dict) -> bytes:
    raw = (json.dumps(value, ensure_ascii=True, separators=(",", ":"), allow_nan=False) + "\n").encode()
    require(len(raw) <= MAX_RECEIPT, "receipt/output byte bound")
    with path.open("xb") as stream:
        stream.write(raw)
    return raw


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)
    admit = sub.add_parser("admit")
    admit.add_argument("--before-root", type=Path, required=True)
    admit.add_argument("--after-root", type=Path, required=True)
    admit.add_argument("--output", type=Path, required=True)
    win = sub.add_parser("windows")
    win.add_argument("--root", type=Path, required=True)
    win.add_argument("--helper-disk", type=Path, required=True)
    win.add_argument("--receipt", type=Path)
    win.add_argument("--output", type=Path, required=True)
    check = sub.add_parser("check-records")
    check.add_argument("--root", type=Path, required=True)
    check.add_argument("--helper-disk", type=Path, required=True)
    check.add_argument("--receipt", type=Path)
    check.add_argument("--evidence-root", type=Path, required=True)
    check.add_argument("--output", type=Path, required=True)
    budget = sub.add_parser("focused-budget")
    budget.add_argument("--root", type=Path, required=True)
    budget.add_argument("--helper-disk", type=Path, required=True)
    budget.add_argument("--receipt", type=Path)
    budget.add_argument("--evidence-root", type=Path, required=True)
    budget.add_argument("--output", type=Path, required=True)
    matrix = sub.add_parser("matrix")
    matrix.add_argument("--output", type=Path, required=True)
    pred = sub.add_parser("predicate")
    pred.add_argument("--record", type=Path, required=True)
    pred.add_argument("--candidate", required=True)
    pred.add_argument("--event-sha", required=True)
    pred.add_argument("--workflow-sha", required=True)
    args = parser.parse_args()
    try:
        if args.command == "matrix":
            cases = origin_matrix()
            write_json(args.output, {"cases": cases, "case_count": len(cases), "valid": all(c["passed"] for c in cases)})
            require(all(c["passed"] for c in cases), "origin matrix")
            print("validated matrix")
            return 0
        if args.command == "predicate":
            value = provenance_origin(strict_json(read_file(args.record, MAX_RECEIPT), MAX_RECEIPT), args.candidate, args.event_sha, args.workflow_sha)
            print("true" if value else "false")
            return 0
        env = dict(os.environ)
        ctx = context(env)
        event_raw = read_file(Path(env["GITHUB_EVENT_PATH"]), MAX_EVENT)
        event = strict_json(event_raw, MAX_EVENT)
        if args.command == "admit":
            value = admission(event, ctx, args.before_root, args.after_root, digest(event_raw))
            raw = write_json(args.output, value)
            outputs = {"admitted": "true", "after": value["after"], "before": value["before"], "request_id": value["after_marker"]["request_id"],
                       "run_id": value["run_id"], "run_attempt": value["run_attempt"], "logical_job": value["logical_job"], "workflow_sha": value["workflow_sha"], "receipt_sha256": digest(raw),
                       "event_canonical_sha256": value["event_canonical_sha256"]}
            with Path(env["GITHUB_OUTPUT"]).open("a", encoding="utf-8") as stream:
                for key, val in outputs.items():
                    require(re.fullmatch(r"[a-z0-9_-]+", val) is not None, "safe admission output")
                    stream.write(key + "=" + val + "\n")
        else:
            require(args.helper_disk.resolve() == Path(__file__).resolve(), "actual executed helper path")
            receipt_raw = read_file(args.receipt, MAX_RECEIPT) if args.receipt is not None else None
            value = windows_origin(event, ctx, args.root, args.helper_disk, receipt_raw, env, digest(event_raw))
            if args.command in ("check-records", "focused-budget"):
                original = strict_json(read_file(args.evidence_root / "verification-origin.json", MAX_RECEIPT), MAX_RECEIPT)
                require(original == value, "current/original origin bytes")
                if args.command == "check-records":
                    check_records(value, args.evidence_root)
                    metadata = strict_json(read_file(args.evidence_root / "run-metadata.json", MAX_RUN_METADATA), MAX_RUN_METADATA)
                    # The complete watchdog contains bounded process-tree rows, not just a small receipt.
                    watchdog = strict_json(read_file(args.evidence_root / "focused/watchdog.json", MAX_RUN_METADATA), MAX_RUN_METADATA)
                    selected = strict_json(read_file(args.evidence_root / "focused-budget.json", MAX_RECEIPT), MAX_RECEIPT)
                    observed = strict_json(read_file(args.evidence_root / "focused/budget.json", MAX_RECEIPT), MAX_RECEIPT)
                    validate_focused_correspondence(value, metadata, watchdog, selected, observed)
                else:
                    value = focused_budget(value)
            write_json(args.output, value)
        print("validated " + args.command)
        return 0
    except (Refused, OSError, KeyError, TypeError, AttributeError, UnicodeError) as exc:
        print("verification refused: " + (str(exc) if isinstance(exc, Refused) else type(exc).__name__), file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
