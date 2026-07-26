#!/usr/bin/env python3
"""Validate Phase 02 dual-path evidence contract samples.

This is intentionally small and dependency-free. It is not a general JSON Schema
implementation; it enforces the Phase 02 contract rules that the local JSON
Schema artifact documents and covers negative cases required by the work order.
"""

from __future__ import annotations

import copy
import json
import sys
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
EVIDENCE_DIR = ROOT / "docs" / "plans" / "evidence" / "extension-full-readiness-dual-path"

SCHEMA_PATH = EVIDENCE_DIR / "phase-02-dual-path-evidence-schema.json"
AUTOMATED_SAMPLE = EVIDENCE_DIR / "phase-02-sample-automated-boundary-packet.json"
LOCAL_SAMPLE = EVIDENCE_DIR / "phase-02-sample-local-desktop-packet.json"
JOINED_SAMPLE = EVIDENCE_DIR / "phase-02-sample-joined-evidence.json"

IDENTITY_FIELDS = ["scenario_id", "host", "claim_id", "marker"]
TOP_LEVEL_REQUIRED = [
    "packet_kind",
    "schema_version",
    "phase",
    "scenario_id",
    "host",
    "claim_id",
    "marker",
    "evidence",
    "remaining",
    "downgraded_claim",
    "non_claims",
    "blocker_or_NONE",
    "redaction_status",
    "provenance",
    "typed_payloads",
]
PATH_REQUIRED = ["status", "evidence", "remaining", "blocker_or_NONE", "downgraded_claim", "provenance"]
LOCAL_PATH_REQUIRED = ["activation", *PATH_REQUIRED]
EVIDENCE_ITEM_REQUIRED = [
    "evidence_type",
    "status",
    "artifact_ref_or_NONE",
    "summary",
    "redaction_status",
    "provenance",
]
PROVENANCE_REQUIRED = [
    "collected_by",
    "collected_at_utc_or_NONE",
    "source",
    "command_or_NONE",
    "input_refs",
    "notes",
]

AUTOMATED_PASS = "PASS_AUTOMATED_BOUNDARY"
LOCAL_PASS = "PASS_LOCAL_DESKTOP"
JOINED_PASS = "PASS_JOINED"
NON_PASS_STATUSES = {"NOT_RUN", "NEEDS_VERIFICATION", "NOT_DISCOVERED_YET", "NOT_CAPTURED"}
PASS_EVIDENCE_STATUSES = {"COLLECTED", "ACCEPTED"}
PLACEHOLDER_VALUES = {
    "NONE",
    "NOT_RUN",
    "NOT_CAPTURED",
    "NOT_DISCOVERED_YET",
    "NEEDS_VERIFICATION",
    "NONE_COLLECTED_IN_PHASE_01_01",
    "NONE_COLLECTED_IN_PHASE_02_SAMPLE",
}
PASS_CONTRADICTION_MARKERS = {
    "NEEDS_VERIFICATION",
    "NOT_RUN",
    "NOT_CAPTURED",
    "NOT_DISCOVERED_YET",
    "NONE_COLLECTED_IN_PHASE_01_01",
    "NONE_COLLECTED_IN_PHASE_02_SAMPLE",
}
SUBSTITUTE_EVIDENCE_TYPES = {"documentation", "dashboard", "ledger", "checklist"}

TYPED_PAYLOAD_REQUIREMENTS: dict[str, tuple[str, list[str]]] = {
    "token-savings": (
        "token",
        [
            "workload_id",
            "tokenizer",
            "tokenizer_version",
            "baseline_tokens",
            "candidate_tokens",
            "repeat_count",
            "savings_calculation",
            "quality_guard",
            "evidence",
            "remaining",
            "downgraded_claim",
        ],
    ),
    "semantic-quality": (
        "semantic_quality",
        [
            "held_out_case_id",
            "rubric",
            "baseline_score",
            "candidate_score",
            "leakage_checks",
            "answer_evidence",
            "remaining",
            "downgraded_claim",
        ],
    ),
    "learnability-training": (
        "learnability_dpo",
        [
            "dataset_provenance",
            "allowed_use_status",
            "train_eval_split",
            "leakage_gates",
            "before_after_behavior",
            "checkpoint_diff",
            "rollback_evidence",
            "dpo_pair_evidence",
            "remaining",
            "downgraded_claim",
        ],
    ),
    "dpo-quality": (
        "learnability_dpo",
        [
            "dataset_provenance",
            "allowed_use_status",
            "train_eval_split",
            "leakage_gates",
            "before_after_behavior",
            "checkpoint_diff",
            "rollback_evidence",
            "dpo_pair_evidence",
            "remaining",
            "downgraded_claim",
        ],
    ),
    "asset-lifecycle": (
        "asset_lifecycle",
        [
            "backup_export_import",
            "share",
            "continue_training",
            "rollback",
            "package_manifest",
            "readback_evidence",
            "remaining",
            "downgraded_claim",
        ],
    ),
    "checkpoint-promotion": (
        "checkpoint",
        [
            "candidate_hash",
            "compatibility_load_result",
            "quality_gate",
            "promotion_owner",
            "rollback_evidence",
            "host_reload_evidence",
            "remaining",
            "downgraded_claim",
        ],
    ),
    "locator-update": (
        "locator",
        [
            "locator_source_of_truth",
            "locator_diff",
            "fresh_load_result",
            "rollback_locator",
            "per_host_reload_evidence",
            "remaining",
            "downgraded_claim",
        ],
    ),
    "codex-chat-ui": (
        "codex_chat_ui",
        [
            "activation",
            "visible_marker",
            "screenshot_video_log_evidence",
            "helper_vs_real_ui_separation",
            "exact_blocker_if_unavailable",
            "remaining",
            "downgraded_claim",
        ],
    ),
}


class ContractError(Exception):
    pass


def load_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def require(condition: bool, message: str) -> None:
    if not condition:
        raise ContractError(message)


def require_object(value: Any, path: str) -> dict[str, Any]:
    require(isinstance(value, dict), f"{path} must be an object")
    return value


def require_list(value: Any, path: str) -> list[Any]:
    require(isinstance(value, list), f"{path} must be a list")
    require(len(value) > 0, f"{path} must not be empty")
    return value


def require_string(value: Any, path: str) -> None:
    require(isinstance(value, str) and len(value) > 0, f"{path} must be a non-empty string")


def require_fields(obj: dict[str, Any], fields: list[str], path: str) -> None:
    for field in fields:
        require(field in obj, f"{path}.{field} is missing")


def validate_no_schema_defaults(schema: Any, path: str = "$") -> None:
    if isinstance(schema, dict):
        require("default" not in schema, f"{path}.default is forbidden")
        for key, value in schema.items():
            validate_no_schema_defaults(value, f"{path}.{key}")
    elif isinstance(schema, list):
        for index, value in enumerate(schema):
            validate_no_schema_defaults(value, f"{path}[{index}]")


def validate_provenance(value: Any, path: str) -> None:
    obj = require_object(value, path)
    require_fields(obj, PROVENANCE_REQUIRED, path)
    for field in ["collected_by", "collected_at_utc_or_NONE", "source", "command_or_NONE"]:
        require_string(obj[field], f"{path}.{field}")
    require_list(obj["input_refs"], f"{path}.input_refs")


def is_placeholder(value: Any) -> bool:
    if not isinstance(value, str):
        return False
    normalized = value.strip().upper()
    return normalized in PLACEHOLDER_VALUES or normalized.startswith("NONE_COLLECTED")


def contains_pass_contradiction(value: Any) -> bool:
    if not isinstance(value, str):
        return False
    normalized = value.upper()
    return any(marker in normalized for marker in PASS_CONTRADICTION_MARKERS)


def validate_no_pass_contradiction(value: Any, path: str) -> None:
    require(
        not contains_pass_contradiction(value),
        f"{path} contradicts PASS with placeholder/non-claim text",
    )


def validate_evidence_list(value: Any, path: str) -> list[Any]:
    items = require_list(value, path)
    for index, item in enumerate(items):
        item_path = f"{path}[{index}]"
        obj = require_object(item, item_path)
        require_fields(obj, EVIDENCE_ITEM_REQUIRED, item_path)
        for field in ["evidence_type", "status", "artifact_ref_or_NONE", "summary"]:
            require_string(obj[field], f"{item_path}.{field}")
        validate_provenance(obj["provenance"], f"{item_path}.provenance")
        if obj["evidence_type"] in SUBSTITUTE_EVIDENCE_TYPES:
            require(
                obj.get("claims_substitute_for_path") is not True,
                f"{item_path} attempts to substitute {obj['evidence_type']} for a required path",
            )
    return items


def validate_pass_evidence_list(value: Any, path: str) -> None:
    items = validate_evidence_list(value, path)
    for index, item in enumerate(items):
        item_path = f"{path}[{index}]"
        obj = require_object(item, item_path)
        require(
            obj["status"] in PASS_EVIDENCE_STATUSES,
            f"{item_path}.status must be COLLECTED or ACCEPTED for PASS",
        )
        require(
            not is_placeholder(obj["artifact_ref_or_NONE"]),
            f"{item_path}.artifact_ref_or_NONE must reference a real collected artifact for PASS",
        )
        require(
            "placeholder" not in obj["evidence_type"].lower(),
            f"{item_path}.evidence_type must not be placeholder evidence for PASS",
        )
        validate_no_pass_contradiction(obj["summary"], f"{item_path}.summary")
        provenance = require_object(obj["provenance"], f"{item_path}.provenance")
        require(
            not is_placeholder(provenance["collected_at_utc_or_NONE"]),
            f"{item_path}.provenance.collected_at_utc_or_NONE must be a real collection timestamp for PASS",
        )
        for input_index, input_ref in enumerate(provenance["input_refs"]):
            require(
                not is_placeholder(input_ref),
                f"{item_path}.provenance.input_refs[{input_index}] must reference real collected evidence for PASS",
            )


def validate_pass_path_block(obj: dict[str, Any], path: str, pass_status: str) -> None:
    require(obj["status"] == pass_status, f"{path}.status must be {pass_status} for PASS validation")
    validate_pass_evidence_list(obj["evidence"], f"{path}.evidence")
    require(obj["blocker_or_NONE"] == "NONE", f"{path}.blocker_or_NONE must be NONE for PASS")
    validate_no_pass_contradiction(obj["downgraded_claim"], f"{path}.downgraded_claim")
    if "activation" in obj:
        require(
            not is_placeholder(obj["activation"]),
            f"{path}.activation must be real Local desktop activation evidence for PASS",
        )


def validate_common_packet(packet: dict[str, Any]) -> None:
    require_fields(packet, TOP_LEVEL_REQUIRED, "$")
    for field in IDENTITY_FIELDS:
        require_string(packet[field], f"$.{field}")
    validate_evidence_list(packet["evidence"], "$.evidence")
    require_list(packet["remaining"], "$.remaining")
    require_string(packet["downgraded_claim"], "$.downgraded_claim")
    require_list(packet["non_claims"], "$.non_claims")
    require_string(packet["blocker_or_NONE"], "$.blocker_or_NONE")
    validate_provenance(packet["provenance"], "$.provenance")
    validate_typed_payloads(packet)


def validate_path_block(value: Any, path: str, kind: str) -> None:
    obj = require_object(value, path)
    required = LOCAL_PATH_REQUIRED if kind == "local" else PATH_REQUIRED
    require_fields(obj, required, path)
    require_string(obj["status"], f"{path}.status")
    validate_evidence_list(obj["evidence"], f"{path}.evidence")
    require_list(obj["remaining"], f"{path}.remaining")
    require_string(obj["blocker_or_NONE"], f"{path}.blocker_or_NONE")
    require_string(obj["downgraded_claim"], f"{path}.downgraded_claim")
    validate_provenance(obj["provenance"], f"{path}.provenance")
    if kind == "local":
        require_string(obj["activation"], f"{path}.activation")
    if obj["status"] == AUTOMATED_PASS:
        require(kind == "automated", f"{path}.status cannot be {AUTOMATED_PASS} for {kind}")
        validate_pass_path_block(obj, path, AUTOMATED_PASS)
    if obj["status"] == LOCAL_PASS:
        require(kind == "local", f"{path}.status cannot be {LOCAL_PASS} for {kind}")
        validate_pass_path_block(obj, path, LOCAL_PASS)


def validate_typed_payloads(packet: dict[str, Any]) -> None:
    payloads = require_object(packet["typed_payloads"], "$.typed_payloads")
    requirement = TYPED_PAYLOAD_REQUIREMENTS.get(packet["claim_id"])
    if requirement is None:
        return
    payload_name, fields = requirement
    require(payload_name in payloads, f"$.typed_payloads.{payload_name} is required for {packet['claim_id']}")
    payload = require_object(payloads[payload_name], f"$.typed_payloads.{payload_name}")
    require_fields(payload, fields, f"$.typed_payloads.{payload_name}")
    if "remaining" in payload:
        require_list(payload["remaining"], f"$.typed_payloads.{payload_name}.remaining")
    if "evidence" in payload:
        require_list(payload["evidence"], f"$.typed_payloads.{payload_name}.evidence")
    require_string(payload["downgraded_claim"], f"$.typed_payloads.{payload_name}.downgraded_claim")


def validate_automated_packet(packet: dict[str, Any]) -> None:
    require(packet.get("packet_kind") == "automated_boundary_packet", "$.packet_kind must be automated_boundary_packet")
    validate_common_packet(packet)
    require("automated_boundary" in packet, "$.automated_boundary is missing")
    validate_path_block(packet["automated_boundary"], "$.automated_boundary", "automated")


def validate_local_packet(packet: dict[str, Any]) -> None:
    require(packet.get("packet_kind") == "local_desktop_packet", "$.packet_kind must be local_desktop_packet")
    validate_common_packet(packet)
    require("local_desktop" in packet, "$.local_desktop is missing")
    validate_path_block(packet["local_desktop"], "$.local_desktop", "local")


def validate_joined_packet(packet: dict[str, Any]) -> None:
    require(packet.get("packet_kind") == "joined_evidence_packet", "$.packet_kind must be joined_evidence_packet")
    validate_common_packet(packet)
    require_fields(
        packet,
        [
            "join_keys",
            "automated_boundary_packet_ref",
            "local_desktop_packet_ref",
            "automated_boundary",
            "local_desktop",
            "join_status",
            "joined_claim_proven",
            "downgrade_record",
        ],
        "$",
    )
    for ref_field in ["automated_boundary_packet_ref", "local_desktop_packet_ref"]:
        require_string(packet[ref_field], f"$.{ref_field}")
        require(packet[ref_field] != "NONE", f"$.{ref_field} must reference a packet")
    join_keys = require_object(packet["join_keys"], "$.join_keys")
    require_fields(join_keys, IDENTITY_FIELDS, "$.join_keys")
    for field in IDENTITY_FIELDS:
        require(join_keys[field] == packet[field], f"$.join_keys.{field} must match $.{field}")

    validate_path_block(packet["automated_boundary"], "$.automated_boundary", "automated")
    validate_path_block(packet["local_desktop"], "$.local_desktop", "local")
    require(isinstance(packet["joined_claim_proven"], bool), "$.joined_claim_proven must be boolean")

    auto_status = packet["automated_boundary"]["status"]
    local_status = packet["local_desktop"]["status"]
    if packet["join_status"] == JOINED_PASS:
        require(auto_status == AUTOMATED_PASS, "PASS_JOINED requires PASS_AUTOMATED_BOUNDARY")
        require(local_status == LOCAL_PASS, "PASS_JOINED requires PASS_LOCAL_DESKTOP")
        require(packet["joined_claim_proven"] is True, "PASS_JOINED requires joined_claim_proven true")
        validate_pass_evidence_list(packet["evidence"], "$.evidence")
        require(packet["blocker_or_NONE"] == "NONE", "$.blocker_or_NONE must be NONE for PASS_JOINED")
        validate_no_pass_contradiction(packet["downgraded_claim"], "$.downgraded_claim")
    else:
        require(packet["joined_claim_proven"] is False, "Non-PASS joined packet must not mark joined_claim_proven true")

    if auto_status == AUTOMATED_PASS and local_status != LOCAL_PASS:
        require(packet["join_status"] != JOINED_PASS, "One Automated boundary path cannot prove a joined claim")
    if local_status == LOCAL_PASS and auto_status != AUTOMATED_PASS:
        require(packet["join_status"] != JOINED_PASS, "One Local desktop path cannot prove a joined claim")

    downgrade = require_object(packet["downgrade_record"], "$.downgrade_record")
    require_fields(
        downgrade,
        [
            "applies_to_host",
            "scenario_id",
            "claim_id",
            "marker",
            "reason",
            "downgraded_claim",
            "remaining",
            "one_path_cannot_prove_joined_claim",
            "documentation_dashboard_ledger_substitute_allowed",
        ],
        "$.downgrade_record",
    )
    require(downgrade["applies_to_host"] == packet["host"], "$.downgrade_record.applies_to_host must match host")
    for field in ["scenario_id", "claim_id", "marker"]:
        require(downgrade[field] == packet[field], f"$.downgrade_record.{field} must match $.{field}")
    require(downgrade["one_path_cannot_prove_joined_claim"] is True, "downgrade must preserve one-path rejection")
    require(
        downgrade["documentation_dashboard_ledger_substitute_allowed"] is False,
        "documentation/dashboard/ledger substitution must be false",
    )
    require_list(downgrade["remaining"], "$.downgrade_record.remaining")
    require_string(downgrade["downgraded_claim"], "$.downgrade_record.downgraded_claim")
    if packet["join_status"] == JOINED_PASS:
        validate_no_pass_contradiction(downgrade["reason"], "$.downgrade_record.reason")
        validate_no_pass_contradiction(downgrade["downgraded_claim"], "$.downgrade_record.downgraded_claim")

    if local_status.startswith("BLOCKED"):
        require(packet["local_desktop"]["blocker_or_NONE"] != "NONE", "Blocked local path requires exact blocker")
        require("NEEDS_VERIFICATION" in downgrade["downgraded_claim"] or "downgrade" in downgrade["downgraded_claim"].lower(), "Blocked local path must downgrade only this host/scenario claim")


def validate_packet(packet: dict[str, Any]) -> None:
    kind = packet.get("packet_kind")
    if kind == "automated_boundary_packet":
        validate_automated_packet(packet)
    elif kind == "local_desktop_packet":
        validate_local_packet(packet)
    elif kind == "joined_evidence_packet":
        validate_joined_packet(packet)
    else:
        raise ContractError(f"Unsupported packet_kind: {kind!r}")


def validate_identity_match(automated: dict[str, Any], local: dict[str, Any], joined: dict[str, Any]) -> None:
    for field in IDENTITY_FIELDS:
        require(automated[field] == local[field], f"Automated/local {field} mismatch")
        require(automated[field] == joined[field], f"Automated/joined {field} mismatch")
    require(joined["automated_boundary_packet_ref"] != "NONE", "Joined packet missing automated ref")
    require(joined["local_desktop_packet_ref"] != "NONE", "Joined packet missing local ref")


def validate_no_duplicate_scenario_markers(rows: list[dict[str, Any]]) -> None:
    seen: set[tuple[str, str]] = set()
    for index, row in enumerate(rows):
        for field in ["scenario_id", "marker"]:
            require(field in row, f"rows[{index}].{field} is missing")
        key = (row["scenario_id"], row["marker"])
        require(key not in seen, f"Duplicate scenario_id + marker pair: {key[0]} / {key[1]}")
        seen.add(key)


def readiness_pass(packet: dict[str, Any]) -> bool:
    return (
        packet.get("packet_kind") == "joined_evidence_packet"
        and packet.get("join_status") == JOINED_PASS
        and packet.get("joined_claim_proven") is True
        and packet.get("automated_boundary", {}).get("status") == AUTOMATED_PASS
        and packet.get("local_desktop", {}).get("status") == LOCAL_PASS
    )


def expect_failure(label: str, func) -> str:
    try:
        func()
    except ContractError as exc:
        return f"PASS expected failure: {label}: {exc}"
    raise ContractError(f"Negative test did not fail: {label}")


def run_negative_tests(automated: dict[str, Any], local: dict[str, Any], joined: dict[str, Any]) -> list[str]:
    results = []

    missing_identity = copy.deepcopy(automated)
    del missing_identity["scenario_id"]
    results.append(expect_failure("reject missing scenario_id", lambda: validate_packet(missing_identity)))

    duplicate = copy.deepcopy(automated)
    results.append(expect_failure("reject duplicate scenario_id + marker", lambda: validate_no_duplicate_scenario_markers([automated, duplicate])))

    missing_status = copy.deepcopy(automated)
    del missing_status["automated_boundary"]["status"]
    results.append(expect_failure("reject missing path status", lambda: validate_packet(missing_status)))

    missing_blocker = copy.deepcopy(local)
    del missing_blocker["local_desktop"]["blocker_or_NONE"]
    results.append(expect_failure("reject missing path blocker", lambda: validate_packet(missing_blocker)))

    missing_downgrade = copy.deepcopy(automated)
    del missing_downgrade["automated_boundary"]["downgraded_claim"]
    results.append(expect_failure("reject missing path downgraded_claim", lambda: validate_packet(missing_downgrade)))

    joined_missing_ref = copy.deepcopy(joined)
    joined_missing_ref["local_desktop_packet_ref"] = "NONE"
    results.append(expect_failure("reject joined evidence without both path refs", lambda: validate_packet(joined_missing_ref)))

    mismatched_local = copy.deepcopy(local)
    mismatched_local["host"] = "Claude"
    results.append(expect_failure("reject mismatched host", lambda: validate_identity_match(automated, mismatched_local, joined)))

    mismatched_claim = copy.deepcopy(local)
    mismatched_claim["claim_id"] = "semantic-quality"
    results.append(expect_failure("reject mismatched claim_id", lambda: validate_identity_match(automated, mismatched_claim, joined)))

    mismatched_marker = copy.deepcopy(local)
    mismatched_marker["marker"] = "FRDP-P01-MISMATCHED-MARKER"
    results.append(expect_failure("reject mismatched marker", lambda: validate_identity_match(automated, mismatched_marker, joined)))

    missing_payload = copy.deepcopy(automated)
    del missing_payload["typed_payloads"]["token"]
    results.append(expect_failure("reject missing token typed payload", lambda: validate_packet(missing_payload)))

    bad_join_pass = copy.deepcopy(joined)
    bad_join_pass["join_status"] = JOINED_PASS
    bad_join_pass["joined_claim_proven"] = True
    results.append(expect_failure("reject PASS_JOINED while both paths are NOT_RUN", lambda: validate_packet(bad_join_pass)))

    placeholder_pass_probe = copy.deepcopy(joined)
    placeholder_pass_probe["join_status"] = JOINED_PASS
    placeholder_pass_probe["joined_claim_proven"] = True
    placeholder_pass_probe["automated_boundary"]["status"] = AUTOMATED_PASS
    placeholder_pass_probe["local_desktop"]["status"] = LOCAL_PASS
    placeholder_pass_probe["automated_boundary"]["evidence"][0]["status"] = "NOT_RUN"
    placeholder_pass_probe["automated_boundary"]["evidence"][0]["artifact_ref_or_NONE"] = "NONE"
    placeholder_pass_probe["local_desktop"]["evidence"][0]["status"] = "NOT_CAPTURED"
    placeholder_pass_probe["local_desktop"]["evidence"][0]["artifact_ref_or_NONE"] = "NONE"
    placeholder_pass_probe["evidence"][0]["status"] = "NOT_RUN"
    placeholder_pass_probe["evidence"][0]["artifact_ref_or_NONE"] = "NONE"
    results.append(
        expect_failure(
            "reject placeholder PASS overclaim probe with NOT_RUN/NOT_CAPTURED/NONE evidence",
            lambda: validate_packet(placeholder_pass_probe),
        )
    )

    doc_substitute = copy.deepcopy(joined)
    doc_substitute["evidence"][0]["evidence_type"] = "ledger"
    doc_substitute["evidence"][0]["claims_substitute_for_path"] = True
    results.append(expect_failure("reject documentation/dashboard/ledger substitution", lambda: validate_packet(doc_substitute)))

    require(readiness_pass(joined) is False, "NOT_RUN joined sample must not be treated as readiness pass")
    results.append("PASS expected non-pass: NOT_RUN/NEEDS_VERIFICATION did not become PASS")

    return results


def main() -> int:
    schema = load_json(SCHEMA_PATH)
    automated = load_json(AUTOMATED_SAMPLE)
    local = load_json(LOCAL_SAMPLE)
    joined = load_json(JOINED_SAMPLE)

    require(schema.get("$schema") == "https://json-schema.org/draft/2020-12/schema", "schema draft marker missing")
    require("$defs" in schema, "schema $defs missing")
    validate_no_schema_defaults(schema)

    validate_packet(automated)
    validate_packet(local)
    validate_packet(joined)
    validate_identity_match(automated, local, joined)
    validate_no_duplicate_scenario_markers([automated])
    validate_no_duplicate_scenario_markers([local])
    validate_no_duplicate_scenario_markers([joined])

    negative_results = run_negative_tests(automated, local, joined)

    print("VALIDATION PASSED: Phase 02 dual-path evidence contract")
    print(f"schema: {SCHEMA_PATH.relative_to(ROOT)}")
    print(f"sample automated boundary packet: {AUTOMATED_SAMPLE.relative_to(ROOT)}")
    print(f"sample local desktop packet: {LOCAL_SAMPLE.relative_to(ROOT)}")
    print(f"sample joined evidence packet: {JOINED_SAMPLE.relative_to(ROOT)}")
    print("positive samples: 3")
    print(f"negative tests: {len(negative_results)}")
    for result in negative_results:
        print(result)
    print("non_claim: samples are NOT_RUN contract samples only, not readiness evidence")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except ContractError as exc:
        print(f"VALIDATION FAILED: {exc}", file=sys.stderr)
        raise SystemExit(1)
