# Quality controls source metadata fill checks

Run: quality-controls-source-metadata-fill-20260618-065930

| Check | Status |
|---|---|
| json_jsonl_parse | PASS |
| coverage_12_scenarios_54_pair_rows | PASS |
| no_forbidden_truthy_claims | PASS |
| utf8_no_bom | PASS |
| git_diff_check | PASS |
| no_source_like_hidden_changes | PASS |

Strict controls can proceed: **no**.

## Exact Missing Inputs

- Approved source_ref/doc_id for TQ-002-prompt-task-interrupt.
- Verified source_license_ref/license_class for all 12 scenarios.
- Source-disjoint train/held-out partition ledger for all 12 scenarios and 54 pair rows.
- Nearest-neighbour/source-overlap report for 54 pair rows.
- Held-out no-memorization report for 54 pair rows.
- Target-disabled or asset-shuffled ablation rerun for the current scope.
