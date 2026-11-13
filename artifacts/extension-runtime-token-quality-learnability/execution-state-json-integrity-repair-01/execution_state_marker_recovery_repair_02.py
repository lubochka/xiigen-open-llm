import base64
import datetime as dt
import hashlib
import json
import os
import re
import shutil
from pathlib import Path


ROOT = Path(r"C:\Projects\llm_mvp_core")
STATE = ROOT / "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
EVIDENCE = ROOT / "artifacts/extension-runtime-token-quality-learnability/execution-state-json-integrity-repair-01"
AGENTS = ROOT / "AGENTS.md"
SINGER_BACKUP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.before-repair.json"
CURRENT_PRE_REPAIR_BACKUP = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.before-repair-02-current.json"
CANDIDATE = EVIDENCE / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.repair-02-candidate.tmp.json"
RECOVERY_INDEX = EVIDENCE / "recovered-top-level-marker-index-02.json"
UNRECOVERED_JSONL = EVIDENCE / "unrecovered-fragments-02.jsonl"
UNRECOVERED_MANIFEST = EVIDENCE / "unrecovered-fragments-02.manifest.json"
SUMMARY = EVIDENCE / "repair-02-run-summary.json"
PACKET_JSON = EVIDENCE / "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02.packet.json"
PACKET_MD = EVIDENCE / "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02.packet.md"
RUN_LOG = EVIDENCE / "execution_state_marker_recovery_repair_02.run.log"

EXPECTED_BACKUP_SIZE = 466_985_374
EXPECTED_BACKUP_SHA256 = "4C491AB1AE0B107F67123D7DB115CAE08BEA9DDD00E0C4EA0BAF6002CA9D9924"

PRIOR_OUTPUTS = [
    EVIDENCE / "EXECUTION-STATE-JSON-INTEGRITY-REPAIR-01.packet.json",
    EVIDENCE / "repair-run-summary.json",
    EVIDENCE / "repair_leading_comma_member_stream.run.log",
    EVIDENCE / "repair_execution_state_json.run1.log",
    EVIDENCE / "recover_complete_markers_to_valid_state.run.log",
    EVIDENCE / "recovered-top-level-marker-index.json",
]


def now_utc() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest().upper()


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def json_parse(path: Path, include_root_summary: bool = False) -> dict:
    try:
        with path.open("r", encoding="utf-8") as f:
            data = json.load(f)
        result = {"ok": True, "method": "python json.load full parse"}
        if include_root_summary:
            result["root_type"] = type(data).__name__
            if isinstance(data, dict):
                keys = list(data.keys())
                result["top_level_key_count"] = len(keys)
                result["top_level_keys_sample"] = keys[:20]
                result["repair_02_marker_present"] = "_recovered_state_integrity_repair_02" in data
        return result
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
        return {
            "ok": False,
            "method": "python json.load full parse",
            "error_type": "MemoryError",
            "message": str(exc),
        }
    except Exception as exc:
        return {
            "ok": False,
            "method": "python json.load full parse",
            "error_type": type(exc).__name__,
            "message": str(exc),
        }


def small_file_record(path: Path) -> dict:
    record = {
        "path": str(path),
        "exists": path.exists(),
    }
    if not path.exists():
        return record
    record["size_bytes"] = path.stat().st_size
    record["sha256"] = sha256_file(path)
    if path.suffix.lower() == ".json" and record["size_bytes"] < 2_000_000:
        try:
            with path.open("r", encoding="utf-8") as f:
                parsed = json.load(f)
            record["json_ok"] = True
            if isinstance(parsed, dict):
                record["json_keys_sample"] = list(parsed.keys())[:20]
        except Exception as exc:
            record["json_ok"] = False
            record["json_error"] = f"{type(exc).__name__}: {exc}"
    return record


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


def decode_json_key(raw_key: bytes) -> str:
    return json.loads(b'"' + raw_key + b'"')


def write_unrecovered_sidecar(source: bytes, covered_ranges: list[tuple[int, int]]) -> dict:
    gaps = []
    cursor = 0
    for start, end in sorted(covered_ranges):
        if start > cursor:
            gaps.append((cursor, start))
        cursor = max(cursor, end)
    if cursor < len(source):
        gaps.append((cursor, len(source)))

    combined = hashlib.sha256()
    total_bytes = 0
    delimiter_only_count = 0
    with UNRECOVERED_JSONL.open("w", encoding="utf-8", newline="\n") as f:
        for idx, (start, end) in enumerate(gaps, start=1):
            chunk = source[start:end]
            total_bytes += len(chunk)
            combined.update(chunk)
            delimiter_only = all(b in b" \t\r\n,:" for b in chunk)
            if delimiter_only:
                delimiter_only_count += 1
            record = {
                "gap_index": idx,
                "byte_start": start,
                "byte_end_exclusive": end,
                "size_bytes": len(chunk),
                "sha256": sha256_bytes(chunk),
                "delimiter_or_whitespace_only": delimiter_only,
                "utf8_preview_replace": chunk[:240].decode("utf-8", "replace"),
                "base64": base64.b64encode(chunk).decode("ascii"),
            }
            f.write(json.dumps(record, ensure_ascii=False, separators=(",", ":")) + "\n")

    sidecar_parse = verify_unrecovered_sidecar(total_bytes, combined.hexdigest().upper())
    manifest = {
        "path": str(UNRECOVERED_JSONL),
        "gap_count": len(gaps),
        "delimiter_or_whitespace_only_gap_count": delimiter_only_count,
        "total_unrecovered_gap_bytes": total_bytes,
        "combined_gap_sha256": combined.hexdigest().upper(),
        "sidecar_file_sha256": sha256_file(UNRECOVERED_JSONL),
        "verification": sidecar_parse,
        "note": "Gaps are source bytes not copied as recovered top-level JSON member payloads; full source bytes are also preserved in the verified Singer backup.",
    }
    UNRECOVERED_MANIFEST.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    return manifest


def verify_unrecovered_sidecar(expected_size: int, expected_combined_sha256: str) -> dict:
    try:
        total = 0
        combined = hashlib.sha256()
        with UNRECOVERED_JSONL.open("r", encoding="utf-8") as f:
            for line_number, line in enumerate(f, start=1):
                record = json.loads(line)
                data = base64.b64decode(record["base64"].encode("ascii"))
                if sha256_bytes(data) != record["sha256"]:
                    return {"ok": False, "line": line_number, "reason": "record_sha256_mismatch"}
                total += len(data)
                combined.update(data)
        actual = combined.hexdigest().upper()
        return {
            "ok": total == expected_size and actual == expected_combined_sha256,
            "total_bytes": total,
            "combined_gap_sha256": actual,
        }
    except Exception as exc:
        return {"ok": False, "error_type": type(exc).__name__, "message": str(exc)}


def write_packet_md(packet: dict) -> None:
    lines = [
        "# EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02 Packet",
        "",
        f"- subagent_id_or_nickname: {packet['subagent_id_or_nickname']}",
        f"- state_integrity_status: {packet['state_integrity_status']}",
        f"- singer_backup: {packet['backup_path_and_hash_or_size']['singer_backup']['path']}",
        f"- singer_backup_sha256: {packet['backup_path_and_hash_or_size']['singer_backup']['sha256']}",
        f"- current_pre_repair_backup: {packet['backup_path_and_hash_or_size']['current_pre_repair_02_backup']['path']}",
        f"- current_pre_repair_backup_sha256: {packet['backup_path_and_hash_or_size']['current_pre_repair_02_backup']['sha256']}",
        f"- parse_error_before: {json.dumps(packet['parse_error_before'], ensure_ascii=False)}",
        f"- parse_verification_after: {json.dumps(packet['parse_verification_after'], ensure_ascii=False)}",
        f"- recovered_top_level_marker_count: {packet['repair_summary']['recovered_top_level_marker_count']}",
        f"- unrecovered_gap_count: {packet['repair_summary']['unrecovered_sidecar']['gap_count']}",
        f"- total_unrecovered_gap_bytes: {packet['repair_summary']['unrecovered_sidecar']['total_unrecovered_gap_bytes']}",
        f"- evidence_supports_only: {packet['evidence_supports_only']}",
        f"- blockers: {json.dumps(packet['blockers'], ensure_ascii=False)}",
    ]
    PACKET_MD.write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> int:
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    log_lines = []

    def log(message: str) -> None:
        line = f"{now_utc()} {message}"
        log_lines.append(line)
        print(line)

    log("start EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02")

    agents_hash = sha256_file(AGENTS)
    current_before_size = STATE.stat().st_size
    current_before_hash = sha256_file(STATE)
    singer_backup_size = SINGER_BACKUP.stat().st_size
    singer_backup_hash = sha256_file(SINGER_BACKUP)
    current_before_parse = json_parse(STATE, include_root_summary=True)
    singer_backup_parse = json_parse(SINGER_BACKUP)
    prior_outputs = [small_file_record(path) for path in PRIOR_OUTPUTS]

    log(f"current_before_hash={current_before_hash} size={current_before_size}")
    log(f"singer_backup_hash={singer_backup_hash} size={singer_backup_size}")

    if singer_backup_size != EXPECTED_BACKUP_SIZE or singer_backup_hash != EXPECTED_BACKUP_SHA256:
        packet = {
            "subagent_id_or_nickname": "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02",
            "changed_files": [str(SUMMARY), str(PACKET_JSON), str(PACKET_MD), str(RUN_LOG), str(Path(__file__))],
            "exact_files_read": [str(AGENTS), str(STATE), str(SINGER_BACKUP), *[str(p) for p in PRIOR_OUTPUTS]],
            "exact_files_written": [str(SUMMARY), str(PACKET_JSON), str(PACKET_MD), str(RUN_LOG), str(Path(__file__))],
            "backup_path_and_hash_or_size": {
                "singer_backup": {"path": str(SINGER_BACKUP), "sha256": singer_backup_hash, "size_bytes": singer_backup_size}
            },
            "parse_error_before": {
                "current_state_before_repair_02": current_before_parse,
                "singer_backup_original_malformed": singer_backup_parse,
            },
            "repair_summary": {"status": "BLOCKED", "reason": "Singer backup did not match expected size/hash."},
            "parse_verification_after": json_parse(STATE, include_root_summary=True),
            "state_integrity_status": "BLOCKED",
            "next_bounded_worker_scope": "Verify the backup source before any further state replacement attempt.",
            "evidence_supports_only": "The expected Singer backup was not verified; no state replacement was performed.",
            "blockers": ["singer_backup_hash_or_size_mismatch"],
        }
        SUMMARY.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
        PACKET_JSON.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
        write_packet_md(packet)
        RUN_LOG.write_text("\n".join(log_lines) + "\n", encoding="utf-8")
        return 2

    current_backup_record = None
    if current_before_hash != singer_backup_hash or current_before_size != singer_backup_size:
        shutil.copy2(STATE, CURRENT_PRE_REPAIR_BACKUP)
        current_backup_record = {
            "path": str(CURRENT_PRE_REPAIR_BACKUP),
            "sha256": sha256_file(CURRENT_PRE_REPAIR_BACKUP),
            "size_bytes": CURRENT_PRE_REPAIR_BACKUP.stat().st_size,
            "created_because": "current governed state differed from verified Singer backup",
        }
        log(f"current pre-repair backup written: {current_backup_record['path']}")
    else:
        current_backup_record = {
            "path": None,
            "sha256": None,
            "size_bytes": None,
            "created_because": "not created because current governed state matched Singer backup",
        }

    source = SINGER_BACKUP.read_bytes()
    pattern = re.compile(br'(?m)^  "((?:[^"\\]|\\.)+)"\s*:\s*\{')
    recovered = []
    rejected = []
    covered_ranges: list[tuple[int, int]] = []
    seen: dict[str, int] = {}

    with CANDIDATE.open("wb") as out:
        marker = {
            "worker": "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02",
            "timestamp_utc": now_utc(),
            "repair_type": "verified_backup_recovery_with_current_state_backup_and_unrecovered_gap_sidecars",
            "active_luba_quote": "Не понимаю почему снова стоп??? Найди и эту лазейку пожалуйста. Занеси это и действия внизу в план работы- сделай план работы полным по нашим стандартам. Сделай 20 проходов и убедись что план доведёт наконец необходимые extension до полной готовности. И приступайк действию до полной готовности всех 3 extensions. NO STOPS approved!!! All through agents!!!",
            "singer_backup": {"path": str(SINGER_BACKUP), "sha256": singer_backup_hash, "size_bytes": singer_backup_size},
            "current_state_before_repair_02": {"path": str(STATE), "sha256": current_before_hash, "size_bytes": current_before_size, "parse": current_before_parse},
            "agents_sha256": agents_hash,
            "non_claims": [
                "does_not_claim_product_readiness",
                "does_not_claim_runtime_or_ui_verification",
                "does_not_claim_extension_repo_changes",
                "does_not_claim_full_semantic_preservation_beyond_recovered_parseable_members_and_preserved_source_bytes",
            ],
        }
        out.write(b'{\n  "_recovered_state_integrity_repair_02": ')
        out.write(json.dumps(marker, ensure_ascii=False, separators=(",", ":")).encode("utf-8"))
        out.write(b',\n  "recovered_top_level_state_markers": {\n')
        first = True
        for match in pattern.finditer(source):
            value_start = source.find(b"{", match.end() - 1, match.end() + 16)
            if value_start < 0:
                rejected.append({"byte_offset": match.start(), "reason": "object_start_not_found"})
                continue
            value_end = find_matching_object_end(source, value_start)
            if value_end is None:
                rejected.append({"byte_offset": match.start(), "reason": "object_end_not_found"})
                continue
            raw_key = match.group(1)
            try:
                key = decode_json_key(raw_key)
                json.loads(b'{"x":' + source[value_start:value_end] + b'}')
            except Exception as exc:
                rejected.append({
                    "byte_offset": match.start(),
                    "raw_key": raw_key[:160].decode("utf-8", "replace"),
                    "reason": type(exc).__name__,
                    "detail": str(exc)[:300],
                })
                continue
            count = seen.get(key, 0) + 1
            seen[key] = count
            output_key = key if count == 1 else f"{key}__duplicate_{count}"
            if not first:
                out.write(b",\n")
            first = False
            out.write(b"    ")
            out.write(json.dumps(output_key, ensure_ascii=False).encode("utf-8"))
            out.write(b": ")
            out.write(source[value_start:value_end])
            recovered.append({
                "output_key": output_key,
                "source_key": key,
                "source_member_byte_start": match.start(),
                "source_value_byte_start": value_start,
                "source_member_byte_end_exclusive": value_end,
                "source_member_size_bytes": value_end - match.start(),
            })
            covered_ranges.append((match.start(), value_end))
        out.write(b'\n  },\n  "malformed_original_preserved_in_backup": ')
        out.write(json.dumps({"path": str(SINGER_BACKUP), "sha256": singer_backup_hash, "size_bytes": singer_backup_size}, ensure_ascii=False, separators=(",", ":")).encode("utf-8"))
        out.write(b',\n  "current_pre_repair_02_preserved": ')
        out.write(json.dumps(current_backup_record, ensure_ascii=False, separators=(",", ":")).encode("utf-8"))
        out.write(b'\n}\n')

    unrecovered_manifest = write_unrecovered_sidecar(source, covered_ranges)
    source_size = len(source)
    recovered_member_bytes = sum(item["source_member_size_bytes"] for item in recovered)
    del source

    candidate_parse = json_parse(CANDIDATE, include_root_summary=True)
    if not candidate_parse.get("ok"):
        packet = {
            "subagent_id_or_nickname": "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02",
            "changed_files": [
                str(CURRENT_PRE_REPAIR_BACKUP),
                str(CANDIDATE),
                str(RECOVERY_INDEX),
                str(UNRECOVERED_JSONL),
                str(UNRECOVERED_MANIFEST),
                str(SUMMARY),
                str(PACKET_JSON),
                str(PACKET_MD),
                str(RUN_LOG),
                str(Path(__file__)),
            ],
            "exact_files_read": [str(AGENTS), str(STATE), str(SINGER_BACKUP), *[str(p) for p in PRIOR_OUTPUTS]],
            "exact_files_written": [
                str(CURRENT_PRE_REPAIR_BACKUP),
                str(CANDIDATE),
                str(UNRECOVERED_JSONL),
                str(UNRECOVERED_MANIFEST),
                str(SUMMARY),
                str(PACKET_JSON),
                str(PACKET_MD),
                str(RUN_LOG),
                str(Path(__file__)),
            ],
            "backup_path_and_hash_or_size": {
                "singer_backup": {"path": str(SINGER_BACKUP), "sha256": singer_backup_hash, "size_bytes": singer_backup_size},
                "current_pre_repair_02_backup": current_backup_record,
            },
            "parse_error_before": {
                "current_state_before_repair_02": current_before_parse,
                "singer_backup_original_malformed": singer_backup_parse,
            },
            "repair_summary": {
                "status": "BLOCKED",
                "reason": "candidate_failed_parse_before_replacement",
                "candidate_parse": candidate_parse,
                "recovered_top_level_marker_count": len(recovered),
                "rejected_candidate_count": len(rejected),
                "unrecovered_sidecar": unrecovered_manifest,
            },
            "parse_verification_after": json_parse(STATE, include_root_summary=True),
            "state_integrity_status": "BLOCKED",
            "next_bounded_worker_scope": "Inspect candidate parse error and repair the recovery writer; governed state was not replaced.",
            "evidence_supports_only": "A current backup and unrecovered sidecar were written, but no governed state replacement occurred because candidate parse failed.",
            "blockers": ["candidate_parse_failed_before_replacement"],
        }
        SUMMARY.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
        PACKET_JSON.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
        write_packet_md(packet)
        RUN_LOG.write_text("\n".join(log_lines) + "\n", encoding="utf-8")
        return 3

    candidate_hash = sha256_file(CANDIDATE)
    os.replace(CANDIDATE, STATE)
    after_parse = json_parse(STATE, include_root_summary=True)
    after_hash = sha256_file(STATE)
    after_size = STATE.stat().st_size

    recovery_index = {
        "worker": "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02",
        "singer_backup": {"path": str(SINGER_BACKUP), "sha256": singer_backup_hash, "size_bytes": singer_backup_size},
        "source_size_bytes": source_size,
        "recovered_top_level_marker_count": len(recovered),
        "recovered_member_bytes": recovered_member_bytes,
        "unrecovered_gap_bytes": unrecovered_manifest["total_unrecovered_gap_bytes"],
        "unrecovered_fragments_manifest": str(UNRECOVERED_MANIFEST),
        "rejected_candidate_count": len(rejected),
        "recovered": recovered,
        "rejected": rejected,
    }
    RECOVERY_INDEX.write_text(json.dumps(recovery_index, ensure_ascii=False, indent=2), encoding="utf-8")

    packet = {
        "subagent_id_or_nickname": "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02",
        "changed_files": [
            str(STATE),
            str(CURRENT_PRE_REPAIR_BACKUP),
            str(RECOVERY_INDEX),
            str(UNRECOVERED_JSONL),
            str(UNRECOVERED_MANIFEST),
            str(SUMMARY),
            str(PACKET_JSON),
            str(PACKET_MD),
            str(RUN_LOG),
            str(Path(__file__)),
        ],
        "exact_files_read": [
            str(AGENTS),
            str(STATE),
            str(SINGER_BACKUP),
            *[str(p) for p in PRIOR_OUTPUTS],
        ],
        "exact_files_written": [
            str(STATE),
            str(CURRENT_PRE_REPAIR_BACKUP),
            str(RECOVERY_INDEX),
            str(UNRECOVERED_JSONL),
            str(UNRECOVERED_MANIFEST),
            str(SUMMARY),
            str(PACKET_JSON),
            str(PACKET_MD),
            str(RUN_LOG),
            str(Path(__file__)),
        ],
        "backup_path_and_hash_or_size": {
            "singer_backup": {"path": str(SINGER_BACKUP), "sha256": singer_backup_hash, "size_bytes": singer_backup_size},
            "current_pre_repair_02_backup": current_backup_record,
        },
        "parse_error_before": {
            "current_state_before_repair_02": current_before_parse,
            "singer_backup_original_malformed": singer_backup_parse,
        },
        "repair_summary": {
            "diagnosis": "Current governed state was parseable from repair-01; Singer backup remained malformed with a leading comma and extra-data fragments. Repair-02 rebuilt a valid state from the verified Singer backup, added the required _recovered_state_integrity_repair_02 marker, preserved current pre-repair state, and wrote sidecars for unrecovered gap bytes.",
            "candidate_verified_before_replacement": candidate_parse,
            "candidate_sha256_before_replacement": candidate_hash,
            "after_sha256": after_hash,
            "after_size_bytes": after_size,
            "recovered_top_level_marker_count": len(recovered),
            "recovered_member_bytes": recovered_member_bytes,
            "source_size_bytes": source_size,
            "rejected_candidate_count": len(rejected),
            "unrecovered_sidecar": unrecovered_manifest,
            "prior_interrupted_outputs": prior_outputs,
        },
        "parse_verification_after": after_parse,
        "state_integrity_status": "PASS" if after_parse.get("ok") and after_parse.get("repair_02_marker_present") else "PARTIAL",
        "next_bounded_worker_scope": "Parent review may inspect repair-02 packet, recovery index, and sidecars, then continue plan-derived extension work through bounded workers. This repair did not touch extension repos, run product/runtime/UI/browser/external AI commands, or use git.",
        "evidence_supports_only": "Execution-state JSON parseability after repair-02, presence of the _recovered_state_integrity_repair_02 marker, recovery of independently parseable top-level marker objects from the verified Singer backup, preservation of current pre-repair state, and sidecar preservation of unrecovered gap bytes. This does not claim product readiness.",
        "blockers": [] if after_parse.get("ok") and after_parse.get("repair_02_marker_present") else ["state parse ok but repair-02 marker was not confirmed"],
    }
    SUMMARY.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
    PACKET_JSON.write_text(json.dumps(packet, ensure_ascii=False, indent=2), encoding="utf-8")
    write_packet_md(packet)
    log(f"after_hash={after_hash} size={after_size} status={packet['state_integrity_status']}")
    RUN_LOG.write_text("\n".join(log_lines) + "\n", encoding="utf-8")
    print(json.dumps(packet, ensure_ascii=False, indent=2))
    return 0 if packet["state_integrity_status"] == "PASS" else 4


if __name__ == "__main__":
    raise SystemExit(main())
