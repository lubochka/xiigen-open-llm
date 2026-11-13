# Checks report

Run: `quality-controls-source-license-partition-20260618-073408`

Итог: проверки artifact/state пакета прошли. Strict QUALITY-03 при этом остается NOT READY, потому что license и train/held-out partition не доказаны.

- JSON parse: `PASS` — All *.json artifacts plus execution state parsed.
- JSONL parse: `PASS` — license-resolution-ledger.jsonl = 66 rows; train-heldout-partition-ledger.jsonl = 66 rows.
- 12/54 coverage: `PASS` — 12 scenarios and 54 pair rows covered; source/doc candidate 12/12 and 54/54.
- No forbidden truthy claims: `PASS` — No forbidden readiness/training/token/quality/model/source-code truthy flags found in generated JSON or state.
- UTF-8/no BOM: `PASS` — 19 files checked before this report; all UTF-8 decodable and no BOM.
- git diff --check: `PASS` — git diff --check exited 0.
- No source code changes: `PASS` — Tracked diff contains only docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json; new files are under the phase artifact folder.
- Forbidden cursor-user-data untouched: `PASS` — Known old untracked cursor-user-data path was not staged, moved, deleted or edited by this phase.
