# Source-overlap / nearest-neighbor report

Run: `quality-controls-source-license-partition-20260618-073408`

Это metadata-only triage, без model/checkpoint/runtime mutation. Он НЕ доказывает no-memorization PASS.

## Exact source_ref groups

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`: 10 scenarios / 48 pair rows.
- `C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/skills-inventory-universal-vs-project-specific.md`: 1 scenarios / 3 pair rows.
- `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\problem with authority.txt`: 1 scenarios / 3 pair rows.

## Exact doc_id groups with overlap

- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#deterministic-shortcut-no-overclaim`: 1 scenarios / 9 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#final-readiness-matrix`: 1 scenarios / 9 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#no-overclaim-evidence-separation`: 1 scenarios / 9 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#documentation-no-overclaim-evidence-boundary`: 1 scenarios / 3 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#dpo-source-quality-and-rollback-safety`: 1 scenarios / 3 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#evidence-separation-redaction`: 1 scenarios / 3 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#learnability-dpo-lifecycle`: 1 scenarios / 3 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-debug-visibility`: 1 scenarios / 3 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-proof-depth-gate`: 1 scenarios / 3 pair rows.
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#token-before-after-pairing`: 1 scenarios / 3 pair rows.
- `problem-with-authority.txt#human-interrupt-hard-pause-lines-43279-43328`: 1 scenarios / 3 pair rows.
- `skills-inventory-universal-vs-project-specific.md#universal-core-vs-project-adapter-skill-registry`: 1 scenarios / 3 pair rows.

## Вывод

Strict source-disjoint split не готов: license неизвестны, train/held-out не назначены, а 48/54 pair rows завязаны на один active-plan source_ref.
