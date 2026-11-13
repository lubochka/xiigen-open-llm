# Checks Report

- Run: `quality-controls-source-disjoint-ablation-20260618-070000`
- Status: `PASS`

| Check | Status | Evidence |
|---|---|---|
| JSON/JSONL parse | `PASS` | json_files=9 jsonl_files=2 |
| 12 scenarios and 54 pair refs accounted for | `PASS` | scenarios=12 pairs=54 |
| forbidden truthy claims grep | `PASS` | No forbidden truthy claim flags found |
| UTF-8/no-BOM and Russian report readability | `PASS` | bom_failures=0 cyrillic_chars=1028 missing_terms= |
| no source-like hidden deterministic changes | `PASS` | Only .json/.jsonl/.md artifacts plus execution state were written; forbidden cursor-user-data path untouched. |
| self-review | `PASS` | No overclaim; metadata not invented; next action requires approved source metadata before QUALITY-03 strict rerun. |
| git diff --check | `PASS` | Exit code 0; no whitespace errors. |
