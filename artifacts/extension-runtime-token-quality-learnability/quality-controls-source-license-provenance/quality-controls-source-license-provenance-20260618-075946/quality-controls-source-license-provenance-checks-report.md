# Checks report

Run: `quality-controls-source-license-provenance-20260618-075946`

- JSON parse: `PASS`
- JSONL parse: `PASS` — source-license-provenance-ledger.jsonl = 66, source-disjoint-train-heldout-split.jsonl = 66.
- 12/54 coverage: `PASS`
- No forbidden truthy claims: `PASS`
- UTF-8/no BOM: `PASS` — state plus new run files have no BOM, CRLF, or trailing whitespace.
- git diff --check: `PASS` — exited 0 after LF-only rewrite.
- source-code changes none: `PASS` — only state and new provenance artifacts changed; pre-existing cursor-user-data remains untracked and untouched.
