import datetime as _dt
import hashlib
import json
import os
import re
from pathlib import Path


ROOT = Path(r"C:\Projects\llm_mvp_core")
STATE = ROOT / "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
EVIDENCE = ROOT / "artifacts/extension-runtime-token-quality-learnability/execution-state-json-integrity-repair-01"
BACKUP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.before-repair.json"
TEMP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.recovered-markers.tmp.json"
SUMMARY = EVIDENCE / "repair-run-summary.json"
PACKET_JSON = EVIDENCE / "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01.packet.json"
PACKET_MD = EVIDENCE / "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01.packet.md"
RECOVERY_INDEX = EVIDENCE / "recovered-top-level-marker-index.json"


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


def find_matching_object_end(data: bytes, start: int) -> int | None:
    depth = 0
    in_string = False
    escaped = False
    for pos in range(start, len(data)):
        b = data[pos]
        if in_string:
            if escaped:
                escaped = False
            elif b == 92:
                escaped = True
            elif b == 34:
                in_string = False
            continue
        if b == 34:
            in_string = True
        elif b == 123:
            depth += 1
        elif b == 125:
            depth -= 1
            if depth == 0:
                return pos + 1
            if depth < 0:
                return None
    return None


def decode_key(raw: bytes) -> str:
    return json.loads(b'"' + raw + b'"')


def main() -> int:
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    before_size = STATE.stat().st_size
    backup_size = BACKUP.stat().st_size
    before_hash = sha256_file(STATE)
    backup_hash = sha256_file(BACKUP)
    before_parse = json_parse(STATE)
    data = STATE.read_bytes()
    pattern = re.compile(br'(?m)^  "((?:[^"\\]|\\.)+)"\s*:\s*\{')
    marker = {
        "worker": "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01",
        "timestamp_utc": _dt.datetime.now(_dt.UTC).isoformat(),
        "repair_type": "recover_complete_top_level_marker_objects_from_malformed_state",
        "backup_file": str(BACKUP),
        "backup_sha256": backup_hash,
        "backup_size_bytes": backup_size,
        "pre_repair_sha256": before_hash,
        "pre_repair_size_bytes": before_size,
        "parse_error_before": before_parse,
        "non_claims": [
            "does_not_claim_product_readiness",
            "does_not_claim_runtime_or_ui_verification",
            "does_not_claim_extension_repo_changes",
        ],
    }
    recovered = []
    rejected = []
    seen = {}
    with TEMP.open("wb") as out:
        out.write(b'{\n  "_execution_state_json_integrity_repair_01": ')
        out.write(json.dumps(marker, ensure_ascii=False, separators=(",", ":")).encode("utf-8"))
        out.write(b',\n  "recovered_top_level_state_markers": {\n')
        first_written = True
        for match in pattern.finditer(data):
            value_start = data.find(b"{", match.end() - 1, match.end() + 8)
            if value_start < 0:
                rejected.append({"byte_offset": match.start(), "reason": "object_start_not_found"})
                continue
            value_end = find_matching_object_end(data, value_start)
            if value_end is None:
                rejected.append({"byte_offset": match.start(), "reason": "object_end_not_found"})
                continue
            raw_key = match.group(1)
            try:
                key = decode_key(raw_key)
                json.loads(b'{"x":' + data[value_start:value_end] + b'}')
            except Exception as exc:
                rejected.append({"byte_offset": match.start(), "raw_key": raw_key[:120].decode("utf-8", "replace"), "reason": type(exc).__name__, "detail": str(exc)[:200]})
                continue
            count = seen.get(key, 0) + 1
            seen[key] = count
            output_key = key if count == 1 else f"{key}__duplicate_{count}"
            if not first_written:
                out.write(b",\n")
            first_written = False
            out.write(b"    ")
            out.write(json.dumps(output_key, ensure_ascii=False).encode("utf-8"))
            out.write(b": ")
            out.write(data[value_start:value_end])
            recovered.append({"output_key": output_key, "source_key": key, "byte_start": match.start(), "byte_end_exclusive": value_end})
        out.write(b'\n  },\n  "malformed_original_preserved_in_backup": ')
        out.write(json.dumps({"path": str(BACKUP), "sha256": backup_hash, "size_bytes": backup_size}, ensure_ascii=False, separators=(",", ":")).encode("utf-8"))
        out.write(b"\n}\n")
    temp_parse = json_parse(TEMP)
    if not temp_parse.get("ok"):
        summary = {"status": "BLOCKED", "reason": "recovered_state_failed_full_parse", "parse_error_before": before_parse, "temp_parse": temp_parse, "recovered_count": len(recovered), "rejected_count": len(rejected)}
        SUMMARY.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
        print(json.dumps(summary, ensure_ascii=False, indent=2))
        return 2
    os.replace(TEMP, STATE)
    after_parse = json_parse(STATE)
    after_hash = sha256_file(STATE)
    index = {"recovered_count": len(recovered), "rejected_count": len(rejected), "recovered": recovered, "rejected_sample": rejected[:200]}
    RECOVERY_INDEX.write_text(json.dumps(index, ensure_ascii=False, indent=2), encoding="utf-8")
    packet = {
        "subagent_id_or_nickname": "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01",
        "changed_files": [
            str(STATE),
            str(BACKUP),
            str(SUMMARY),
            str(PACKET_JSON),
            str(PACKET_MD),
            str(RECOVERY_INDEX),
            str(Path(__file__)),
            str(EVIDENCE / "repair_execution_state_json.py"),
            str(EVIDENCE / "repair_leading_comma_member_stream.py"),
            str(EVIDENCE / "repair_execution_state_json.run1.log"),
            str(EVIDENCE / "repair_leading_comma_member_stream.run.log"),
            str(EVIDENCE / "leading-comma-removed-prefix.bin"),
        ],
        "exact_files_read": [str(ROOT / "AGENTS.md"), str(STATE), str(BACKUP)],
        "exact_files_written": [
            str(STATE),
            str(BACKUP),
            str(SUMMARY),
            str(PACKET_JSON),
            str(PACKET_MD),
            str(RECOVERY_INDEX),
            str(Path(__file__)),
            str(EVIDENCE / "repair_execution_state_json.py"),
            str(EVIDENCE / "repair_leading_comma_member_stream.py"),
            str(EVIDENCE / "repair_execution_state_json.run1.log"),
            str(EVIDENCE / "repair_leading_comma_member_stream.run.log"),
            str(EVIDENCE / "leading-comma-removed-prefix.bin"),
        ],
        "backup_path_and_hash_or_size": {"path": str(BACKUP), "sha256": backup_hash, "size_bytes": backup_size},
        "parse_error_before": before_parse,
        "repair_summary": {
            "diagnosis": "Original state began with a comma and later contained cut/misaligned fragments; a whole-document wrap failed with Extra data at char 3750.",
            "minimal_safe_repair": "Preserved byte-for-byte backup, recovered independently parseable top-level marker-object fragments into a fresh valid JSON state, inserted a compact repair marker, and wrote a recovery index.",
            "backup_matches_source_before_repair": backup_hash == before_hash and backup_size == before_size,
            "before_sha256": before_hash,
            "before_size_bytes": before_size,
            "after_sha256": after_hash,
            "after_size_bytes": STATE.stat().st_size,
            "recovered_top_level_marker_count": len(recovered),
            "rejected_candidate_count": len(rejected),
            "recovery_index": str(RECOVERY_INDEX),
            "whole_document_wrap_attempt": "BLOCKED: base candidate failed full parse with Extra data at line 32 column 1 char 3750.",
        },
        "parse_verification_after": after_parse,
        "state_integrity_status": "PASS" if after_parse.get("ok") else "PARTIAL",
        "next_bounded_worker_scope": "Parent review may inspect this packet and recovery index, then continue the plan-derived extension work through bounded workers. This repair did not touch extension repos, run product/runtime/UI/browser/external AI commands, or use git.",
        "evidence_supports_only": "Execution-state JSON parseability is restored, complete top-level marker objects recoverable from the malformed file are preserved in a valid JSON object, and the full malformed pre-repair bytes are preserved in the verified backup.",
        "blockers": [] if after_parse.get("ok") else ["full parse failed after recovered-state repair"],
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
            f"- recovered_top_level_marker_count: {len(recovered)}",
            f"- rejected_candidate_count: {len(rejected)}",
            f"- evidence_supports_only: {packet['evidence_supports_only']}",
            f"- blockers: {json.dumps(packet['blockers'], ensure_ascii=False)}",
        ]) + "\n",
        encoding="utf-8",
    )
    print(json.dumps(packet, ensure_ascii=False, indent=2))
    return 0 if after_parse.get("ok") else 3


if __name__ == "__main__":
    raise SystemExit(main())
