#!/usr/bin/env python3
"""Optional retrospective inventory; never a CREATE_ONLY generation gate.

Read candidate files without changing them. Distinguish Git LFS pointers from
actual JSONL, bind known historical audit hashes only when explicitly requested,
and inspect saved prompt metadata separately from generated row content.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import re

BASE = Path(__file__).resolve().parents[1]
POINTER = re.compile(r"\Aversion https://git-lfs.github.com/spec/v1\r?\noid sha256:([0-9a-f]{64})\r?\nsize ([0-9]+)\s*\Z")


def load(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def exact_snapshot_bytes(path, tree_entry):
    data = path.read_bytes()
    if not tree_entry:
        return data, "local_file_without_remote_blob_comparison"
    # The connector snapshot writer may add terminal newlines. Only accept a
    # removed newline when the resulting exact Git blob equals the pinned tree.
    for extra in range(4):
        candidate = data[:-extra] if extra else data
        if extra and not data.endswith(b"\n" * extra):
            continue
        digest = hashlib.sha1(b"blob " + str(len(candidate)).encode() + b"\0" + candidate).hexdigest()
        if digest == tree_entry.get("sha"):
            return candidate, "pinned_git_blob_matches" + ("_after_removing_snapshot_newline" if extra else "")
    return data, "snapshot_bytes_do_not_match_pinned_git_blob"


def known_audits():
    result = {}
    for filename in ("24-GENERATOR-OUTPUT-AUDIT-2026-10-25.md", "25-GENERATOR-FOLLOWUP-AUDIT-2026-10-25.md"):
        path = BASE / filename
        if not path.is_file():
            continue
        for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
            match = re.search(r"\|\s*([^|]+?draft\.jsonl)[^|]*\|.*?`([0-9a-f]{64})`", line)
            if match:
                relative, digest = match.groups()
                result.setdefault(relative.strip(), []).append({"audit": filename, "audit_line": line_number,
                                                                 "historical_body_sha256": digest})
    return result


def saved_prompt(path, tree_entry):
    if not path.is_file():
        return None
    data, integrity = exact_snapshot_bytes(path, tree_entry)
    text = data.decode("utf-8-sig")
    assignments = []
    for body in re.findall(r"```(?:json)?\s*\n(.*?)\n```", text, re.S):
        try:
            value = json.loads(body)
        except ValueError:
            continue
        if isinstance(value, dict) and value.get("schema_version") in {"xiigen.batch-request.v1", "xiigen.domain-campaign-request.v1"}:
            fields = ("schema_version", "count", "domain_id", "topic_id", "profile_id", "objective_id", "split")
            item = {key: value.get(key) for key in fields}
            item["saved_learning_sets_root"] = value.get("paths", {}).get("learning_sets_root")
            item["saved_request_path"] = value.get("paths", {}).get("request")
            item["legacy_output_directory"] = value.get("output_directory")
            item["evidence_kind"] = "assignment_embedded_in_saved_prompt_not_observed_output"
            assignments.append(item)
    return {"integrity": integrity, "assignments": assignments,
            "assigned_count_is_not_actual_row_count": True}


def inspect(root, registry, tree=None, bind_audits=False, source_head=None):
    entries = {entry["path"]: entry for entry in (tree or {}).get("tree", []) if entry["type"] == "blob"}
    streams = {allocation["stream_id"]: allocation for allocation in registry.get("allocations", [])}
    audits = known_audits() if bind_audits else {}
    files = []
    pair_locations = {}
    for path in sorted(root.rglob("draft.jsonl")):
        relative = path.relative_to(root).as_posix()
        parts = Path(relative).parts
        stream = "/".join(parts[:2])
        assignment = streams.get(stream)
        entry = entries.get(relative)
        data, integrity = exact_snapshot_bytes(path, entry)
        text = data.decode("utf-8-sig")
        pointer = POINTER.fullmatch(text)
        record = {"path": relative, "git_blob_sha": entry.get("sha") if entry else None,
                  "snapshot_integrity": integrity, "stream_id_from_path": stream,
                  "expected_domain_id": assignment.get("domain_id") if assignment else None,
                  "expected_topic_id": assignment.get("topic_id") if assignment else None,
                  "expected_objective_id": assignment.get("objective_id") if assignment else None,
                  "expected_profile_id": assignment.get("profile_id") if assignment else None,
                  "domain_mapping_basis": "preparation_registry_not_unread_candidate_metadata",
                  "actual_row_count": None, "fresh_semantic_review": False, "structural_findings": []}
        if pointer:
            record.update(storage_kind="git_lfs_pointer", body_loaded=False,
                          lfs_object_sha256=pointer.group(1), lfs_object_size=int(pointer.group(2)),
                          body_access_status="NOT_LOADED", quality_status="needs_assessment")
            body_digest = pointer.group(1)
        else:
            record.update(storage_kind="jsonl_payload", body_loaded=True, quality_status="needs_assessment")
            body_digest = hashlib.sha256(data).hexdigest() if bind_audits else None
            count = 0
            for line_number, line in enumerate(text.splitlines(), 1):
                if not line.strip():
                    continue
                try:
                    row = json.loads(line)
                except ValueError as error:
                    record["structural_findings"].append({"line": line_number, "issue": "invalid_json", "detail": str(error)})
                    continue
                count += 1
                if not isinstance(row, dict):
                    record["structural_findings"].append({"line": line_number, "issue": "row_is_not_object"})
                    continue
                for field in ("prompt", "chosen", "rejected"):
                    if not isinstance(row.get(field), str) or not row[field].strip():
                        record["structural_findings"].append({"line": line_number, "issue": "missing_nonempty_" + field})
                if isinstance(row.get("chosen"), str) and row.get("chosen") == row.get("rejected"):
                    record["structural_findings"].append({"line": line_number, "issue": "equal_chosen_rejected"})
                if assignment:
                    for field in ("domain_id", "topic_id", "profile_id"):
                        if row.get(field) != assignment[field]:
                            record["structural_findings"].append({"line": line_number, "issue": field + "_differs_from_current_registry", "observed": row.get(field), "expected": assignment[field], "interpretation": "check_historical_assignment_before_calling_a_legacy_row_invalid"})
                if row.get("pair_id"):
                    pair_locations.setdefault(str(row["pair_id"]), []).append({"path": relative, "line": line_number})
            record["actual_row_count"] = count
        bindings = []
        for audit in audits.get(relative, []):
            bindings.append(dict(audit, matches_current_object=body_digest == audit["historical_body_sha256"],
                                 evidence_kind="historical_audit_bound_to_current_object_not_fresh_semantic_read"))
        record["historical_audit_bindings"] = bindings
        prompt_relative = str(Path(relative).with_name("prompt.md")).replace("\\", "/")
        record["saved_prompt"] = saved_prompt(path.with_name("prompt.md"), entries.get(prompt_relative))
        files.append(record)
    expected = {entry["path"] for entry in (tree or {}).get("tree", []) if entry["type"] == "blob" and entry["path"].endswith("/draft.jsonl")}
    observed = {record["path"] for record in files}
    duplicates = {key: value for key, value in pair_locations.items() if len(value) > 1}
    return {"schema_version": "xiigen.committed-corpus-inventory.v1",
            "recorded_at": datetime.now(timezone.utc).isoformat(timespec="seconds"),
            "source_commit": source_head, "source_repository": "lubochka/llm_mvp_core",
            "runtime_tree_sha": (tree or {}).get("sha"),
            "scope": "Read-only retrospective inventory; optional, not a generation gate.",
            "summary": {"draft_paths": len(files), "lfs_pointers": sum(not r["body_loaded"] for r in files),
                        "payloads_loaded": sum(r["body_loaded"] for r in files),
                        "actual_rows_read": sum(r["actual_row_count"] or 0 for r in files),
                        "current_corpus_row_count_established": bool(files) and all(
                            r["body_loaded"] and not any(f["issue"] in {"invalid_json", "row_is_not_object"}
                                                         for f in r["structural_findings"])
                            for r in files),
                        "files_bound_to_historical_audit": sum(any(a["matches_current_object"] for a in r["historical_audit_bindings"]) for r in files),
                        "saved_prompts_read": sum(r["saved_prompt"] is not None for r in files),
                        "snapshot_missing_draft_paths": sorted(expected - observed),
                        "snapshot_extra_draft_paths": sorted(observed - expected) if expected else []},
            "files": files, "duplicate_pair_id_locations_in_loaded_payloads": duplicates,
            "limitations": ["A Git LFS pointer is an object reference, not malformed training JSONL.",
                            "Assignments, declared counts and creation reports cannot establish unread row semantics.",
                            "Historical matching hashes bind prior evidence; they do not turn it into a fresh semantic review.",
                            "No source data, saved request, status, checkpoint or model is modified.",
                            "No generation, acceptance, native import, training or learning metrics are performed."]}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--registry", type=Path, default=BASE / "campaign" / "registry.json")
    parser.add_argument("--tree", type=Path, help="Optional pinned Git tree metadata for a downloaded snapshot")
    parser.add_argument("--source-head")
    parser.add_argument("--bind-known-audits", action="store_true", help="Optionally compare object identity with audit 24/25; never a mandatory generation checksum")
    parser.add_argument("--output", type=Path, required=True, help="New report path; existing files are not overwritten")
    args = parser.parse_args()
    if not args.root.is_dir():
        parser.error("The exact --root directory does not exist; no search or download was attempted")
    result = inspect(args.root.resolve(), load(args.registry), load(args.tree) if args.tree else None,
                     args.bind_known_audits, args.source_head)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as handle:
        json.dump(result, handle, ensure_ascii=False, indent=2)
        handle.write("\n")
    print(json.dumps(result["summary"], ensure_ascii=False))


if __name__ == "__main__":
    main()
