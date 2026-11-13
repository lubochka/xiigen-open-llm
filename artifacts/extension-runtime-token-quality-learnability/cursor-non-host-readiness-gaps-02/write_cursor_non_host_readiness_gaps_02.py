import json
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(r"C:\Projects\llm_mvp_core")
ARTIFACT_ROOT = ROOT / "artifacts" / "extension-runtime-token-quality-learnability"
OUT_DIR = ARTIFACT_ROOT / "cursor-non-host-readiness-gaps-02"
STATE_PATH = ROOT / "docs" / "plans" / "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
STATE_MARKER_KEY = "_cursor_non_host_readiness_gaps_02"

PACKET_PATHS = {
    "cursor_ext_readiness_01": ARTIFACT_ROOT / "cursor-ext-readiness-01" / "cursor-ext-readiness-01-packet.json",
    "cursor_ext_path_b_runtime_01": ARTIFACT_ROOT / "cursor-ext-path-b-runtime-01" / "cursor-ext-path-b-runtime-01-packet.json",
    "cursor_ext_path_b_ui_screenshot_01": ARTIFACT_ROOT / "cursor-ext-path-b-ui-screenshot-01" / "cursor-ext-path-b-ui-screenshot-01-packet.json",
    "cursor_same_row23_visible_ui_blocker_packet_02": ARTIFACT_ROOT / "cursor-same-row23-visible-ui-blocker-packet-02" / "cursor-same-row23-visible-ui-blocker-packet-02-packet.json",
    "cursor_nonhost_readiness_gaps_01": ARTIFACT_ROOT / "cursor-nonhost-readiness-gaps-01" / "cursor-nonhost-readiness-gaps-01-packet.json",
    "cursor_lifecycle_rollback_nonhost_validation_01": ARTIFACT_ROOT / "cursor-lifecycle-rollback-nonhost-validation-01" / "cursor-lifecycle-rollback-nonhost-validation-01-packet.json",
    "cursor_docs_final_coverage_or_token_quality_nonhost_01": ARTIFACT_ROOT / "cursor-docs-final-coverage-or-token-quality-nonhost-01" / "cursor-docs-final-coverage-or-token-quality-nonhost-01-packet.json",
    "cursor_token_quality_same_pair_nonhost_01": ARTIFACT_ROOT / "cursor-token-quality-same-pair-nonhost-01" / "cursor-token-quality-same-pair-nonhost-01-packet.json",
    "three_extension_final_blocker_consolidation_02": ARTIFACT_ROOT / "three-extension-final-blocker-consolidation-02" / "three-extension-final-blocker-consolidation-02-packet.json",
    "execution_state_marker_recovery_repair_02": ARTIFACT_ROOT / "execution-state-json-integrity-repair-01" / "EXECUTION-STATE-MARKER-RECOVERY-REPAIR-02.packet.json",
    "execution_state_marker_recovery_repair_02_summary": ARTIFACT_ROOT / "execution-state-json-integrity-repair-01" / "repair-02-run-summary.json",
}

WRITTEN_PACKET = OUT_DIR / "cursor-non-host-readiness-gaps-02-packet.json"
WRITTEN_MD = OUT_DIR / "cursor-non-host-readiness-gaps-02-packet.md"
STATE_APPEND_BLOCK = OUT_DIR / "state-append-block.json"
STATE_VERIFY = OUT_DIR / "state-update-verification.json"
RUNNER_SUMMARY = OUT_DIR / "runner-summary.json"


def utc_now():
    return datetime.now(timezone.utc).isoformat()


def read_json(path):
    text = path.read_text(encoding="utf-8-sig")
    return json.loads(text)


def write_json(path, data):
    path.write_text(json.dumps(data, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def coalesce_status(packet):
    for key in ("status", "cursor_readiness_status", "result", "overall_status"):
        value = packet.get(key)
        if value:
            return value
    return None


def ref(packet_id, field):
    return f"{PACKET_PATHS[packet_id]}#{field}"


def contains_bytes(path, needle):
    overlap = max(len(needle) - 1, 0)
    previous = b""
    with path.open("rb") as handle:
        while True:
            chunk = handle.read(1024 * 1024)
            if not chunk:
                return False
            haystack = previous + chunk
            if needle in haystack:
                return True
            previous = haystack[-overlap:] if overlap else b""


def last_non_ws_byte(path):
    whitespace = set(b" \r\n\t")
    with path.open("rb") as handle:
        handle.seek(0, 2)
        size = handle.tell()
        pos = size
        while pos > 0:
            read_size = min(1024 * 1024, pos)
            pos -= read_size
            handle.seek(pos)
            chunk = handle.read(read_size)
            for idx in range(len(chunk) - 1, -1, -1):
                if chunk[idx] not in whitespace:
                    return pos + idx, chunk[idx], size
    return None, None, size


def append_state_marker(marker):
    key_bytes = json.dumps(STATE_MARKER_KEY).encode("utf-8")
    with STATE_PATH.open("rb") as state_handle:
        starts_with_bom = state_handle.read(3) == b"\xef\xbb\xbf"
    state_size_before = STATE_PATH.stat().st_size

    if contains_bytes(STATE_PATH, key_bytes):
        return {
            "state_transition_made_or_not": "not_made_marker_already_present",
            "marker_key": STATE_MARKER_KEY,
            "state_path": str(STATE_PATH),
            "starts_with_bom": starts_with_bom,
            "state_size_before": state_size_before,
            "state_size_after": STATE_PATH.stat().st_size,
            "targeted_marker_readback": "present",
        }

    pos, byte_value, original_size = last_non_ws_byte(STATE_PATH)
    if pos is None or byte_value != ord("}"):
        return {
            "state_transition_made_or_not": "not_made_final_top_level_closing_brace_not_found",
            "marker_key": STATE_MARKER_KEY,
            "state_path": str(STATE_PATH),
            "starts_with_bom": starts_with_bom,
            "state_size_before": state_size_before,
            "state_size_after": STATE_PATH.stat().st_size,
            "targeted_marker_readback": "absent",
            "last_non_ws_byte": byte_value,
        }

    marker_text = json.dumps(marker, indent=2, sort_keys=True)
    marker_member = (
        ",\n  "
        + json.dumps(STATE_MARKER_KEY)
        + ": "
        + marker_text.replace("\n", "\n  ")
        + "\n}\n"
    )

    with STATE_PATH.open("r+b") as handle:
        handle.truncate(pos)
        handle.seek(pos)
        handle.write(marker_member.encode("utf-8"))

    present_after = contains_bytes(STATE_PATH, key_bytes)
    return {
        "state_transition_made_or_not": "made_appended_top_level_marker" if present_after else "attempted_but_marker_not_found",
        "marker_key": STATE_MARKER_KEY,
        "state_path": str(STATE_PATH),
        "starts_with_bom": starts_with_bom,
        "state_size_before": state_size_before,
        "state_size_after": STATE_PATH.stat().st_size,
        "original_last_non_ws_offset": pos,
        "original_size": original_size,
        "targeted_marker_readback": "present" if present_after else "absent",
    }


def main():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    generated_at_utc = utc_now()

    packets = {}
    for packet_id, path in PACKET_PATHS.items():
        packets[packet_id] = read_json(path)

    exact_files_read = [
        str(ROOT / "AGENTS.md"),
        str(STATE_PATH),
        *[str(path) for path in PACKET_PATHS.values()],
    ]

    packet_extracts = {
        packet_id: {
            "path": str(PACKET_PATHS[packet_id]),
            "status": coalesce_status(packet),
            "evidence_supports_only": packet.get("evidence_supports_only"),
            "blockers": packet.get("blockers"),
        }
        for packet_id, packet in packets.items()
    }

    consolidation_cursor = packets["three_extension_final_blocker_consolidation_02"].get("statuses_by_extension", {}).get("Cursor", {})
    cursor_next_action = consolidation_cursor.get("safe_next_worker_after_user_action") or packets["three_extension_final_blocker_consolidation_02"].get("safe_next_worker_after_user_action", {}).get("Cursor")

    cursor_non_host_gap_matrix = [
        {
            "gap": "token boundary/savings",
            "status": "PARTIAL",
            "evidence_refs": [
                ref("cursor_token_quality_same_pair_nonhost_01", "token_same_pair_status"),
                ref("cursor_token_quality_same_pair_nonhost_01", "metrics_ref"),
                ref("cursor_docs_final_coverage_or_token_quality_nonhost_01", "token_quality_nonhost_status"),
                ref("cursor_nonhost_readiness_gaps_01", "blockers"),
            ],
            "evidence_summary": "Existing evidence supports local same-pair helper/core boundary counts only. It does not include an accepted tokenizer baseline or before/after reduction proof.",
            "non_claims": [
                "No token savings claim.",
                "No tokenizer-accepted token reduction claim.",
            ],
        },
        {
            "gap": "quality non-regression/improvement",
            "status": "PARTIAL",
            "evidence_refs": [
                ref("cursor_token_quality_same_pair_nonhost_01", "quality_same_pair_status"),
                ref("cursor_token_quality_same_pair_nonhost_01", "evidence_supports_only"),
                ref("cursor_docs_final_coverage_or_token_quality_nonhost_01", "blockers"),
            ],
            "evidence_summary": "Existing evidence supports structural non-regression boundaries only: no external AI, no training, no UI invocation, redacted trace, and no readiness/quality claims. It does not include semantic improvement or held-out quality evaluation.",
            "non_claims": [
                "No semantic quality improvement claim.",
                "No held-out quality pass claim.",
            ],
        },
        {
            "gap": "learnability/training/checkpoint",
            "status": "NOT_CLAIMED",
            "evidence_refs": [
                ref("cursor_lifecycle_rollback_nonhost_validation_01", "blockers"),
                ref("cursor_token_quality_same_pair_nonhost_01", "non_claims"),
                ref("three_extension_final_blocker_consolidation_02", "statuses_by_extension.Cursor.gates_not_proven"),
            ],
            "evidence_summary": "Existing Cursor packets explicitly preserve no-claim boundaries for training, checkpoint mutation, checkpoint promotion, and full learnability readiness.",
            "non_claims": [
                "No training claim.",
                "No checkpoint mutation claim.",
                "No checkpoint promotion claim.",
            ],
        },
        {
            "gap": "lifecycle/export/import/share/continue-training",
            "status": "PARTIAL",
            "evidence_refs": [
                ref("cursor_lifecycle_rollback_nonhost_validation_01", "lifecycle_rollback_nonhost_status"),
                ref("cursor_lifecycle_rollback_nonhost_validation_01", "local_validation_ref"),
                ref("cursor_lifecycle_rollback_nonhost_validation_01", "evidence_supports_only"),
            ],
            "evidence_summary": "Existing evidence supports local non-host file/hash/report boundary validation and revalidation of prior lifecycle reports. It does not rerun training/export/import/fresh-load/share/continue-training commands.",
            "non_claims": [
                "No fresh export/import/fresh-load run by this worker.",
                "No complete lifecycle readiness claim.",
            ],
        },
        {
            "gap": "rollback/target safety",
            "status": "PARTIAL",
            "evidence_refs": [
                ref("cursor_lifecycle_rollback_nonhost_validation_01", "lifecycle_rollback_nonhost_gates"),
                ref("cursor_nonhost_readiness_gaps_01", "static_boundary_evidence"),
                ref("cursor_nonhost_readiness_gaps_01", "blockers"),
            ],
            "evidence_summary": "Existing evidence supports local target/rollback locator and hash boundaries. It does not prove production rollback safety or target promotion.",
            "non_claims": [
                "No production rollback safety claim.",
                "No target promotion claim.",
            ],
        },
        {
            "gap": "docs/final coverage",
            "status": "PARTIAL",
            "evidence_refs": [
                ref("cursor_docs_final_coverage_or_token_quality_nonhost_01", "docs_final_coverage_status"),
                ref("cursor_docs_final_coverage_or_token_quality_nonhost_01", "verifier_status"),
                ref("three_extension_final_blocker_consolidation_02", "statuses_by_extension.Cursor"),
            ],
            "evidence_summary": "Existing evidence supports docs checklist/non-claim boundary packaging. It does not prove final same-scenario joined Path A plus Path B coverage, full Cursor UI/chat readiness, or all-three readiness.",
            "non_claims": [
                "No final dual-path coverage claim.",
                "No full Cursor readiness claim.",
                "No all-three extension readiness claim.",
            ],
        },
    ]

    commands_or_tools_used = [
        {
            "tool_or_command": "Get-Content AGENTS.md -Raw",
            "result": "exit 0; full raw read command executed, but tool display was truncated, so this packet also relies on targeted AGENTS rule extraction output.",
        },
        {
            "tool_or_command": "BOM/state targeted header read",
            "result": "exit 0; state starts with UTF-8 BOM; strict UTF-8 byte decoding succeeded for the targeted header read; no full strict JSON parse claimed by this worker.",
        },
        {
            "tool_or_command": "Named artifact directory and file discovery",
            "result": "exit 0 for directory discovery and corrected file inventory; one earlier foreach pipe inventory command failed and produced no usable evidence.",
        },
        {
            "tool_or_command": "Targeted packet JSON summaries",
            "result": "exit 0 after corrected commands; summaries read the eight requested Cursor packet JSON files plus final consolidation and repair-02 packets.",
        },
        {
            "tool_or_command": "State broad Select-String attempt",
            "result": "timed out after 20 seconds; not used as evidence except as a failed broad-search attempt.",
        },
        {
            "tool_or_command": "write_cursor_non_host_readiness_gaps_02.py",
            "result": "running now; writes packet, markdown report, state append block, and targeted marker verification.",
        },
    ]

    safe_non_host_work_remaining = "no"
    safe_non_host_work_remaining_explanation = (
        "No additional filesystem-only read/parse Cursor non-host closure is identified from the inspected packets. "
        "Remaining Cursor proof is host-action gated for the same-Row23 visible local-only UI route. "
        "Token savings, semantic improvement, and training/checkpoint claims would require separate explicit eval/training authorization and evidence; they are not claimable from existing packets."
    )

    blockers = [
        "Same-Row23 visible UI submission remains host-action gated: Cursor Agent/chat must be closed or unfocusable and a local-only XIIGen panel route must be exposed before typing/submission.",
        "Token savings are not proven: existing evidence has boundary counts only and no accepted tokenizer baseline or before/after reduction.",
        "Semantic quality improvement is not proven: existing evidence has structural non-regression boundaries only and no held-out semantic quality pass.",
        "Learnability/training/checkpoint mutation or promotion is not proven and is explicitly not claimed by existing packets.",
        "Lifecycle/export/import/share/continue-training is partial only: existing evidence revalidates non-host boundaries and prior reports but does not rerun fresh export/import/fresh-load/share/continue-training.",
        "Rollback/target safety is partial only: local locator/hash boundaries exist, but production rollback safety and target promotion are not proven.",
        "Docs/final coverage is partial only: docs checklist/non-claim packaging exists, but final same-scenario joined Path A plus Path B coverage is not proven.",
    ]

    packet = {
        "schema": "cursor-non-host-readiness-gaps-02.v1",
        "subagent_id_or_nickname": "CURSOR-NON-HOST-READINESS-GAPS-02",
        "generated_at_utc": generated_at_utc,
        "requested_artifact_type": "Cursor non-host gap closure/downgrade packet + narrow execution-state marker",
        "claim_scope": "Cursor non-host packet/readback downgrade only; no runtime/UI/browser/external AI/network/git/product probe.",
        "evidence_scope": "Exact packet JSON files listed in exact_files_read plus targeted state header/marker handling.",
        "count_claimed": 11,
        "independent_evidence_count": len(PACKET_PATHS),
        "subagent_id_or_NONE": "CURSOR-NON-HOST-READINESS-GAPS-02",
        "downgrade_needed": True,
        "changed_files": [
            str(Path(__file__).resolve()),
            str(WRITTEN_PACKET),
            str(WRITTEN_MD),
            str(STATE_APPEND_BLOCK),
            str(STATE_VERIFY),
            str(RUNNER_SUMMARY),
            str(STATE_PATH),
        ],
        "exact_files_read": exact_files_read,
        "exact_files_written": [
            str(Path(__file__).resolve()),
            str(WRITTEN_PACKET),
            str(WRITTEN_MD),
            str(STATE_APPEND_BLOCK),
            str(STATE_VERIFY),
            str(RUNNER_SUMMARY),
            str(STATE_PATH),
        ],
        "commands_or_tools_used": commands_or_tools_used,
        "packet_extracts": packet_extracts,
        "cursor_non_host_gap_matrix": cursor_non_host_gap_matrix,
        "cursor_readiness_status": "PARTIAL",
        "cursor_readiness_status_reason": "Existing Cursor evidence includes local helper/core, Extension Host command path, screenshot retention, and partial non-host boundary packets, but same-Row23 visible UI route, token savings, semantic improvement, training/checkpoint, full lifecycle, production rollback, final coverage, and all-three readiness are not proven.",
        "state_transition_made_or_not": "pending_before_state_marker_append",
        "safe_non_host_work_remaining": safe_non_host_work_remaining,
        "safe_non_host_work_remaining_explanation": safe_non_host_work_remaining_explanation,
        "next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION": cursor_next_action or "NEEDS_USER_HOST_ACTION: provide a safe local-only Cursor visible route before same-Row23 visible UI proof.",
        "evidence_supports_only": (
            "Cursor non-host downgrade matrix from existing packet evidence. It supports PARTIAL/NOT_CLAIMED classifications only. "
            "It does not prove full Cursor readiness, token savings, semantic quality improvement, learnability/training, checkpoint mutation/promotion, production rollback safety, final dual-path coverage, or all-three extension readiness."
        ),
        "blockers": blockers,
        "non_claims": [
            "No UI/browser/screenshot action by this worker.",
            "No external AI call.",
            "No network.",
            "No product/runtime probe.",
            "No git action.",
            "No source edit.",
            "No token savings claim.",
            "No semantic quality improvement claim.",
            "No training or checkpoint mutation/promotion claim.",
            "No full Cursor readiness claim.",
            "No all-three extension readiness claim.",
        ],
        "parent_will_perform_commit_push_after_review": True,
        "must_stop_after_report": True,
    }

    write_json(WRITTEN_PACKET, packet)

    state_marker = {
        "subagent_id_or_nickname": packet["subagent_id_or_nickname"],
        "generated_at_utc": generated_at_utc,
        "packet_path": str(WRITTEN_PACKET),
        "cursor_readiness_status": packet["cursor_readiness_status"],
        "safe_non_host_work_remaining": packet["safe_non_host_work_remaining"],
        "next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION": packet["next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION"],
        "evidence_supports_only": packet["evidence_supports_only"],
        "gap_statuses": {row["gap"]: row["status"] for row in cursor_non_host_gap_matrix},
        "non_claims": packet["non_claims"],
    }
    write_json(STATE_APPEND_BLOCK, {STATE_MARKER_KEY: state_marker})
    state_result = append_state_marker(state_marker)
    write_json(STATE_VERIFY, state_result)

    packet["state_transition_made_or_not"] = state_result["state_transition_made_or_not"]
    packet["state_update_verification_ref"] = str(STATE_VERIFY)
    packet["state_marker_key"] = STATE_MARKER_KEY
    packet["commands_or_tools_used"][-1]["result"] = "exit 0; packet files written and state marker verification emitted."
    write_json(WRITTEN_PACKET, packet)

    md_lines = [
        "# CURSOR-NON-HOST-READINESS-GAPS-02",
        "",
        "## Claim Guard",
        f"- claim_scope: {packet['claim_scope']}",
        f"- evidence_scope: {packet['evidence_scope']}",
        f"- count_claimed: {packet['count_claimed']}",
        f"- independent_evidence_count: {packet['independent_evidence_count']}",
        f"- subagent_id_or_NONE: {packet['subagent_id_or_NONE']}",
        f"- downgrade_needed: {str(packet['downgrade_needed']).lower()}",
        "",
        "## Status",
        f"- cursor_readiness_status: {packet['cursor_readiness_status']}",
        f"- state_transition_made_or_not: {packet['state_transition_made_or_not']}",
        f"- safe_non_host_work_remaining: {packet['safe_non_host_work_remaining']}",
        f"- next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: {packet['next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION']}",
        "",
        "## Gap Matrix",
    ]
    for row in cursor_non_host_gap_matrix:
        md_lines.extend(
            [
                f"- {row['gap']}: {row['status']}",
                f"  - evidence: {'; '.join(row['evidence_refs'])}",
                f"  - summary: {row['evidence_summary']}",
            ]
        )
    md_lines.extend(
        [
            "",
            "## Evidence Supports Only",
            packet["evidence_supports_only"],
            "",
            "## Blockers",
            *[f"- {blocker}" for blocker in blockers],
        ]
    )
    WRITTEN_MD.write_text("\n".join(md_lines) + "\n", encoding="utf-8")

    runner_summary = {
        "subagent_id_or_nickname": packet["subagent_id_or_nickname"],
        "generated_at_utc": utc_now(),
        "written_packet": str(WRITTEN_PACKET),
        "written_md": str(WRITTEN_MD),
        "state_transition_made_or_not": packet["state_transition_made_or_not"],
        "cursor_readiness_status": packet["cursor_readiness_status"],
        "safe_non_host_work_remaining": packet["safe_non_host_work_remaining"],
        "blocker_count": len(blockers),
    }
    write_json(RUNNER_SUMMARY, runner_summary)

    print(json.dumps(runner_summary, indent=2, sort_keys=True))


if __name__ == "__main__":
    main()
