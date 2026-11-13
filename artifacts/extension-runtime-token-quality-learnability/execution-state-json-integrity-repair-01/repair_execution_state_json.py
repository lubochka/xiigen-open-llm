import datetime as _dt
import hashlib
import json
import os
import shutil
from pathlib import Path


ROOT = Path(r"C:\Projects\llm_mvp_core")
STATE = ROOT / "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
EVIDENCE = ROOT / "artifacts/extension-runtime-token-quality-learnability/execution-state-json-integrity-repair-01"
BACKUP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.before-repair.json"
TEMP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.repaired.tmp.json"
SIDE_CAR = EVIDENCE / "append-only-fragments-after-first-json-root.bin"
SUMMARY = EVIDENCE / "repair-run-summary.json"
PACKET_JSON = EVIDENCE / "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01.packet.json"
PACKET_MD = EVIDENCE / "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01.packet.md"


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest().upper()


def json_parse(path: Path) -> dict:
    try:
        with path.open("r", encoding="utf-8") as f:
            json.load(f)
        return {"ok": True, "method": "python json.load full parse"}
    except json.JSONDecodeError as exc:
        return {
            "ok": False,
            "method": "python json.load full parse",
            "error_type": "JSONDecodeError",
            "message": exc.msg,
            "line": exc.lineno,
            "column": exc.colno,
            "char_position": exc.pos,
        }
    except MemoryError as exc:
        return {"ok": False, "method": "python json.load full parse", "error_type": "MemoryError", "message": str(exc)}
    except Exception as exc:
        return {"ok": False, "method": "python json.load full parse", "error_type": type(exc).__name__, "message": str(exc)}


def scan_first_root(path: Path) -> dict:
    whitespace = {9, 10, 13, 32}
    stack = []
    in_string = False
    escaped = False
    root_start = None
    last_pos = -1
    with path.open("rb") as f:
        while True:
            chunk = f.read(1024 * 1024)
            if not chunk:
                break
            for b in chunk:
                last_pos += 1
                if root_start is None:
                    if b in whitespace:
                        continue
                    if b not in (ord("{"), ord("[")):
                        return {"ok": False, "error": "root is not object_or_array", "root_start": last_pos, "byte": b}
                    root_start = last_pos
                    stack.append(b)
                    continue
                if in_string:
                    if escaped:
                        escaped = False
                    elif b == ord("\\"):
                        escaped = True
                    elif b == ord('"'):
                        in_string = False
                    continue
                if b == ord('"'):
                    in_string = True
                elif b in (ord("{"), ord("[")):
                    stack.append(b)
                elif b in (ord("}"), ord("]")):
                    if not stack:
                        return {"ok": False, "error": "unexpected_closer", "position": last_pos, "byte": b}
                    opener = stack.pop()
                    if (opener == ord("{") and b != ord("}")) or (opener == ord("[") and b != ord("]")):
                        return {"ok": False, "error": "mismatched_closer", "position": last_pos, "opener": opener, "byte": b}
                    if not stack:
                        return {"ok": True, "root_start": root_start, "root_end_exclusive": last_pos + 1, "root_kind": chr(opener)}
    return {"ok": False, "error": "eof_before_root_closed", "root_start": root_start, "last_position": last_pos}


def non_ws_after(path: Path, start: int) -> dict:
    whitespace = b" \t\r\n"
    size = path.stat().st_size
    count = max(0, size - start)
    first = None
    with path.open("rb") as f:
        f.seek(start)
        pos = start
        while True:
            chunk = f.read(1024 * 1024)
            if not chunk:
                break
            for i, b in enumerate(chunk):
                if b not in whitespace:
                    first = pos + i
                    return {"total_bytes_after_root": count, "first_non_ws_after_root": first}
            pos += len(chunk)
    return {"total_bytes_after_root": count, "first_non_ws_after_root": None}


def copy_range(src: Path, dst, start: int, end: int) -> None:
    remaining = end - start
    with src.open("rb") as f:
        f.seek(start)
        while remaining > 0:
            chunk = f.read(min(1024 * 1024, remaining))
            if not chunk:
                raise RuntimeError(f"Unexpected EOF copying {src}")
            dst.write(chunk)
            remaining -= len(chunk)


def trailing_trim_end_before_closer(path: Path, closing_pos: int) -> int:
    pos = closing_pos - 1
    with path.open("rb") as f:
        while pos >= 0:
            f.seek(pos)
            b = f.read(1)
            if b not in (b" ", b"\t", b"\r", b"\n"):
                return pos + 1
            pos -= 1
    return 0


def has_object_content(path: Path, start: int, end: int) -> bool:
    with path.open("rb") as f:
        f.seek(start)
        remaining = end - start
        while remaining > 0:
            chunk = f.read(min(1024 * 1024, remaining))
            if any(b not in b" \t\r\n" for b in chunk):
                return True
            remaining -= len(chunk)
    return False


def write_repaired_state(scan: dict, backup_hash: str, before_size: int, extra_info: dict) -> dict:
    root_end = scan["root_end_exclusive"]
    root_kind = scan["root_kind"]
    source_size = STATE.stat().st_size
    if root_end < source_size:
        with SIDE_CAR.open("wb") as out:
            copy_range(STATE, out, root_end, source_size)
    else:
        SIDE_CAR.write_bytes(b"")

    marker = {
        "worker": "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01",
        "timestamp_utc": _dt.datetime.now(_dt.UTC).isoformat(),
        "repair_type": "append_only_after_first_json_root_preserved_to_sidecar",
        "backup_file": str(BACKUP),
        "backup_sha256": backup_hash,
        "pre_repair_size_bytes": before_size,
        "first_json_root_end_exclusive_byte_offset": root_end,
        "bytes_preserved_to_sidecar": source_size - root_end,
        "sidecar_file": str(SIDE_CAR),
        "non_claims": [
            "does_not_claim_product_readiness",
            "does_not_claim_runtime_or_ui_verification",
            "does_not_claim_extension_repo_changes",
        ],
    }
    marker_bytes = json.dumps(marker, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    with TEMP.open("wb") as out:
        if root_kind == "{":
            closing_pos = root_end - 1
            prefix_end = trailing_trim_end_before_closer(STATE, closing_pos)
            content_exists = has_object_content(STATE, scan["root_start"] + 1, prefix_end)
            copy_range(STATE, out, 0, prefix_end)
            out.write((b",\n  " if content_exists else b"\n  "))
            out.write(json.dumps("_execution_state_json_integrity_repair_01").encode("ascii"))
            out.write(b": ")
            out.write(marker_bytes)
            out.write(b"\n}\n")
            return {"marker_inserted": True, "temp_path": str(TEMP), "inserted_key": "_execution_state_json_integrity_repair_01"}
        copy_range(STATE, out, 0, root_end)
        return {"marker_inserted": False, "temp_path": str(TEMP), "reason": "root_not_object", "root_kind": root_kind}


def main() -> int:
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    before_size = STATE.stat().st_size
    backup_size = BACKUP.stat().st_size if BACKUP.exists() else None
    backup_hash = sha256_file(BACKUP)
    before_hash = sha256_file(STATE)
    before_parse = json_parse(STATE)
    scan = scan_first_root(STATE)
    if not scan.get("ok"):
        summary = {"status": "BLOCKED", "before_size": before_size, "backup_size": backup_size, "before_hash": before_hash, "backup_hash": backup_hash, "parse_error_before": before_parse, "scan": scan}
        SUMMARY.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        return 2
    extra_info = non_ws_after(STATE, scan["root_end_exclusive"])
    repair = write_repaired_state(scan, backup_hash, before_size, extra_info)
    after_temp_parse = json_parse(TEMP)
    if not after_temp_parse.get("ok"):
        summary = {"status": "BLOCKED", "before_size": before_size, "backup_size": backup_size, "before_hash": before_hash, "backup_hash": backup_hash, "parse_error_before": before_parse, "scan": scan, "extra_info": extra_info, "repair": repair, "temp_parse_after": after_temp_parse}
        SUMMARY.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        return 3
    os.replace(TEMP, STATE)
    after_size = STATE.stat().st_size
    after_hash = sha256_file(STATE)
    after_parse = json_parse(STATE)
    sidecar_hash = sha256_file(SIDE_CAR)
    packet = {
        "subagent_id_or_nickname": "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01",
        "changed_files": [str(STATE), str(BACKUP), str(SIDE_CAR), str(SUMMARY), str(PACKET_JSON), str(PACKET_MD), str(Path(__file__))],
        "exact_files_read": [str(STATE), str(BACKUP), str(ROOT / "AGENTS.md")],
        "exact_files_written": [str(STATE), str(BACKUP), str(SIDE_CAR), str(SUMMARY), str(PACKET_JSON), str(PACKET_MD), str(Path(__file__))],
        "backup_path_and_hash_or_size": {"path": str(BACKUP), "sha256": backup_hash, "size_bytes": backup_size},
        "parse_error_before": before_parse,
        "repair_summary": {
            "root_scan": scan,
            "extra_info": extra_info,
            "backup_matches_source_before_repair": backup_hash == before_hash and backup_size == before_size,
            "before_size_bytes": before_size,
            "before_sha256": before_hash,
            "after_size_bytes": after_size,
            "after_sha256": after_hash,
            "sidecar_path": str(SIDE_CAR),
            "sidecar_size_bytes": SIDE_CAR.stat().st_size,
            "sidecar_sha256": sidecar_hash,
            "state_marker": repair,
        },
        "parse_verification_after": after_parse,
        "state_integrity_status": "PASS" if after_parse.get("ok") else "PARTIAL",
        "next_bounded_worker_scope": "Parent review may inspect this packet and, if accepted, continue the plan-derived extension work through bounded workers; no product readiness is claimed here.",
        "evidence_supports_only": "The execution-state file is backed up, append-only bytes after the first JSON root are preserved in a sidecar, and the repaired state file parses as JSON via Python json.load.",
        "blockers": [] if after_parse.get("ok") else ["full parse failed after repair"],
    }
    SUMMARY.write_text(json.dumps({**packet, "script_status": "completed"}, ensure_ascii=False, indent=2), encoding="utf-8")
    PACKET_JSON.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
    md = [
        "# EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01 Packet",
        "",
        f"- subagent_id_or_nickname: {packet['subagent_id_or_nickname']}",
        f"- state_integrity_status: {packet['state_integrity_status']}",
        f"- backup: {BACKUP}",
        f"- backup_sha256: {backup_hash}",
        f"- before_parse: {json.dumps(before_parse, ensure_ascii=False)}",
        f"- after_parse: {json.dumps(after_parse, ensure_ascii=False)}",
        f"- sidecar: {SIDE_CAR}",
        f"- evidence_supports_only: {packet['evidence_supports_only']}",
        f"- blockers: {json.dumps(packet['blockers'], ensure_ascii=False)}",
    ]
    PACKET_MD.write_text("\n".join(md) + "\n", encoding="utf-8")
    print(json.dumps(packet, ensure_ascii=False, indent=2))
    return 0 if after_parse.get("ok") else 4


if __name__ == "__main__":
    raise SystemExit(main())
