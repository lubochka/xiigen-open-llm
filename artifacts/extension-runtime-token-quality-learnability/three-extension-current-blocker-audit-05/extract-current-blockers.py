import json
from pathlib import Path

root = Path(r"C:\Projects\llm_mvp_core")
state_path = root / "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
review_path = root / "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl"
artifact_root = root / "artifacts/extension-runtime-token-quality-learnability"

raw = state_path.read_bytes()
with state_path.open("r", encoding="utf-8-sig") as f:
    state = json.load(f)
markers = state.get("recovered_top_level_state_markers", {})

review_ids = []
with review_path.open("r", encoding="utf-8-sig") as f:
    for line in f:
        if line.strip():
            row = json.loads(line)
            review_ids.append(row.get("current_pass_id") or f"pass-{row.get('pass')}")

interesting = []
for p in artifact_root.iterdir():
    if p.is_dir() and any(s in p.name.lower() for s in ["claude", "cursor", "codex", "row23", "plan-review", "current-blocker", "04c", "path-b"]):
        interesting.append({"name": p.name, "mtime": p.stat().st_mtime, "path": str(p)})
interesting.sort(key=lambda x: x["mtime"], reverse=True)

out = {
    "state_utf8_bom": raw.startswith(b"\xef\xbb\xbf"),
    "state_top_keys": list(state.keys()),
    "marker_names": list(markers.keys()),
    "claude_marker": markers.get("claude_live_host_route_proof_or_blocker_01_20260628"),
    "cursor_marker": markers.get("cursor_ext_readiness_01_20260628"),
    "row20_codex_marker": markers.get("row20_status_board_sync_20260626"),
    "row21_codex_marker": markers.get("row21_status_board_sync_20260626"),
    "row22_codex_marker": markers.get("row22_status_board_sync_20260626"),
    "row23_marker": markers.get("row23_input_path_resolution_20260628"),
    "claude_current_marker": markers.get("claude_live_host_blocker_packet_02_20260628"),
    "codex_current_marker": markers.get("codex_host_ui_access_recovery_01_20260628_124503"),
    "cursor_current_marker": markers.get("cursor_visible_xiigen_row23_local_proof_04c_20260628"),
    "three_extension_consolidation": markers.get("three_extension_final_blocker_consolidation_02_20260628_122133"),
    "plan_repair_marker": markers.get("plan_repair_stop_loophole_20260628"),
    "review_nonblank_lines": len(review_ids),
    "review_parsed_ids": review_ids,
    "relevant_artifact_dirs": interesting[:40],
}
print(json.dumps(out, indent=2, ensure_ascii=True))
