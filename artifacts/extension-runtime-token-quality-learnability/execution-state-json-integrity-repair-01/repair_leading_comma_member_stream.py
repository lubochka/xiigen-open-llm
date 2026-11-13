import datetime as _dt
import hashlib
import json
import os
from pathlib import Path


ROOT = Path(r"C:\Projects\llm_mvp_core")
STATE = ROOT / "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
EVIDENCE = ROOT / "artifacts/extension-runtime-token-quality-learnability/execution-state-json-integrity-repair-01"
BACKUP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.before-repair.json"
BASE_TEMP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.leading-comma-base.tmp.json"
FINAL_TEMP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.leading-comma-final.tmp.json"
REMOVED_PREFIX = EVIDENCE / "leading-comma-removed-prefix.bin"
SUMMARY = EVIDENCE / "repair-run-summary.json"
PACKET_JSON = EVIDENCE / "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01.packet.json"
PACKET_MD = EVIDENCE / "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01.packet.md"
RUN_LOG_NOTE = EVIDENCE / "repair-leading-comma-run-note.txt"


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
        return {"ok": False, "method": "python json.load full parse", "error_type": "JSONDecodeError", "message": exc.msg, "line": exc.lineno, "column": exc.colno, "char_position": exc.pos}
    except MemoryError as exc:
        return {"ok": False, "method": "python json.load full parse", "error_type": "MemoryError", "message": str(exc)}
    except Exception as exc:
        return {"ok": False, "method": "python json.load full parse", "error_type": type(exc).__name__, "message": str(exc)}


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


def first_non_ws(path: Path) -> dict:
    whitespace = b" \t\r\n"
    with path.open("rb") as f:
        pos = 0
        while True:
            chunk = f.read(1024 * 1024)
            if not chunk:
                return {"position": None, "byte": None}
            for i, b in enumerate(chunk):
                if b not in whitespace:
                    return {"position": pos + i, "byte": b}
            pos += len(chunk)


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


def write_base_candidate(first: dict) -> dict:
    pos = first["position"]
    with STATE.open("rb") as src, BASE_TEMP.open("wb") as dst:
        src.seek(0)
        prefix = src.read(pos + 1)
        REMOVED_PREFIX.write_bytes(prefix)
        dst.write(b"{")
        copy_range(STATE, dst, pos + 1, STATE.stat().st_size)
    return {"base_temp": str(BASE_TEMP), "removed_prefix_path": str(REMOVED_PREFIX), "removed_prefix_size_bytes": REMOVED_PREFIX.stat().st_size}


def write_final_with_marker(base_scan: dict, backup_hash: str, backup_size: int, before_hash: str, before_size: int, base_parse: dict) -> dict:
    marker = {
        "worker": "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01",
        "timestamp_utc": _dt.datetime.now(_dt.UTC).isoformat(),
        "repair_type": "leading_comma_member_stream_wrapped_as_json_object",
        "backup_file": str(BACKUP),
        "backup_sha256": backup_hash,
        "backup_size_bytes": backup_size,
        "pre_repair_sha256": before_hash,
        "pre_repair_size_bytes": before_size,
        "candidate_parse_before_marker": base_parse,
        "removed_prefix_sidecar": str(REMOVED_PREFIX),
        "non_claims": [
            "does_not_claim_product_readiness",
            "does_not_claim_runtime_or_ui_verification",
            "does_not_claim_extension_repo_changes",
        ],
    }
    marker_bytes = json.dumps(marker, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    closing_pos = base_scan["root_end_exclusive"] - 1
    prefix_end = trailing_trim_end_before_closer(BASE_TEMP, closing_pos)
    with FINAL_TEMP.open("wb") as out:
        copy_range(BASE_TEMP, out, 0, prefix_end)
        out.write(b",\n  ")
        out.write(json.dumps("_execution_state_json_integrity_repair_01").encode("ascii"))
        out.write(b": ")
        out.write(marker_bytes)
        out.write(b"\n}\n")
    return {"final_temp": str(FINAL_TEMP), "inserted_key": "_execution_state_json_integrity_repair_01", "marker": marker}


def main() -> int:
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    before_size = STATE.stat().st_size
    backup_size = BACKUP.stat().st_size
    before_hash = sha256_file(STATE)
    backup_hash = sha256_file(BACKUP)
    before_parse = json_parse(STATE)
    first = first_non_ws(STATE)
    if first["byte"] != ord(","):
        summary = {"status": "BLOCKED", "reason": "first_non_ws_byte_not_comma", "first_non_ws": first, "parse_error_before": before_parse}
        SUMMARY.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        print(json.dumps(summary, ensure_ascii=False, indent=2))
        return 2
    base = write_base_candidate(first)
    base_parse = json_parse(BASE_TEMP)
    if not base_parse.get("ok"):
        summary = {"status": "BLOCKED", "reason": "base_candidate_failed_full_parse", "first_non_ws": first, "base": base, "parse_error_before": before_parse, "base_parse": base_parse}
        SUMMARY.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        print(json.dumps(summary, ensure_ascii=False, indent=2))
        return 3
    base_scan = scan_first_root(BASE_TEMP)
    if not base_scan.get("ok"):
        summary = {"status": "BLOCKED", "reason": "base_candidate_scan_failed", "first_non_ws": first, "base": base, "parse_error_before": before_parse, "base_parse": base_parse, "base_scan": base_scan}
        SUMMARY.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        print(json.dumps(summary, ensure_ascii=False, indent=2))
        return 4
    final = write_final_with_marker(base_scan, backup_hash, backup_size, before_hash, before_size, base_parse)
    final_parse = json_parse(FINAL_TEMP)
    if not final_parse.get("ok"):
        summary = {"status": "BLOCKED", "reason": "final_candidate_failed_full_parse", "parse_error_before": before_parse, "base": base, "base_parse": base_parse, "base_scan": base_scan, "final": final, "final_parse": final_parse}
        SUMMARY.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        print(json.dumps(summary, ensure_ascii=False, indent=2))
        return 5
    os.replace(FINAL_TEMP, STATE)
    after_parse = json_parse(STATE)
    after_hash = sha256_file(STATE)
    packet = {
        "subagent_id_or_nickname": "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01",
        "changed_files": [str(STATE), str(BACKUP), str(REMOVED_PREFIX), str(SUMMARY), str(PACKET_JSON), str(PACKET_MD), str(Path(__file__)), str(EVIDENCE / "repair_execution_state_json.py"), str(EVIDENCE / "repair_execution_state_json.run1.log")],
        "exact_files_read": [str(ROOT / "AGENTS.md"), str(STATE), str(BACKUP)],
        "exact_files_written": [str(STATE), str(BACKUP), str(REMOVED_PREFIX), str(SUMMARY), str(PACKET_JSON), str(PACKET_MD), str(Path(__file__)), str(EVIDENCE / "repair_execution_state_json.py"), str(EVIDENCE / "repair_execution_state_json.run1.log")],
        "backup_path_and_hash_or_size": {"path": str(BACKUP), "sha256": backup_hash, "size_bytes": backup_size},
        "parse_error_before": before_parse,
        "repair_summary": {
            "diagnosis": "Current state began with a comma, so it was an object-member stream rather than a JSON document.",
            "minimal_transformation": "Preserved byte-for-byte backup, stored the removed leading prefix in a sidecar, prepended an opening object brace, copied the original bytes after the leading comma, and inserted a compact repair marker before the final root close after the base candidate fully parsed.",
            "first_non_ws_before": first,
            "removed_prefix_path": str(REMOVED_PREFIX),
            "removed_prefix_sha256": sha256_file(REMOVED_PREFIX),
            "removed_prefix_size_bytes": REMOVED_PREFIX.stat().st_size,
            "backup_matches_source_before_repair": backup_hash == before_hash and backup_size == before_size,
            "base_candidate_parse": base_parse,
            "base_candidate_scan": base_scan,
            "after_size_bytes": STATE.stat().st_size,
            "after_sha256": after_hash,
            "state_marker": {"inserted_key": "_execution_state_json_integrity_repair_01", "repair_type": final["marker"]["repair_type"]},
        },
        "parse_verification_after": after_parse,
        "state_integrity_status": "PASS" if after_parse.get("ok") else "PARTIAL",
        "next_bounded_worker_scope": "Parent review may inspect this packet and continue the plan-derived extension work through bounded workers; this worker did not touch extension repos, run product/runtime/UI/browser/external AI commands, or use git.",
        "evidence_supports_only": "The execution-state file had a byte-for-byte backup; the malformed leading-comma object-member stream was minimally wrapped into a valid JSON object; the removed prefix was preserved; the repaired state fully parses with Python json.load.",
        "blockers": [] if after_parse.get("ok") else ["full parse failed after repair"],
    }
    SUMMARY.write_text(json.dumps({**packet, "script_status": "completed"}, ensure_ascii=False, indent=2), encoding="utf-8")
    PACKET_JSON.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
    PACKET_MD.write_text(
        "\n".join([
            "# EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01 Packet",
            "",
            f"- subagent_id_or_nickname: {packet['subagent_id_or_nickname']}",
            f"- state_integrity_status: {packet['state_integrity_status']}",
            f"- backup: {BACKUP}",
            f"- backup_sha256: {backup_hash}",
            f"- parse_error_before: {json.dumps(before_parse, ensure_ascii=False)}",
            f"- parse_verification_after: {json.dumps(after_parse, ensure_ascii=False)}",
            f"- evidence_supports_only: {packet['evidence_supports_only']}",
            f"- blockers: {json.dumps(packet['blockers'], ensure_ascii=False)}",
        ]) + "\n",
        encoding="utf-8",
    )
    RUN_LOG_NOTE.write_text("Primary leading-comma repair runner completed. See packet JSON and summary JSON.\n", encoding="utf-8")
    print(json.dumps(packet, ensure_ascii=False, indent=2))
    return 0 if after_parse.get("ok") else 6


if __name__ == "__main__":
    raise SystemExit(main())
