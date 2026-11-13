# No-leakage / no-memorization / ablation readiness

Strict controls can run now: **no**.

## Blockers

- 1/12 scenarios still lacks an approved source_ref/doc_id candidate in the allowed references: TQ-002-prompt-task-interrupt.
- 12/12 scenarios have license_class UNKNOWN and no source_license_ref.
- 12/12 scenarios have no proven train-vs-held-out partition.
- 54/54 pair rows lack proven license and train-vs-held-out partition for strict source-disjoint controls.
- No nearest-neighbour/source-overlap report exists for the current 54 rows.
- No held-out no-memorization report exists for the current 54 rows.
- No target-disabled or asset-shuffled ablation rerun exists for the current target/eval scope.

## Required before strict QUALITY-03 rerun

- Resolve TQ-002 source_ref/doc_id from an approved local human-interrupt governance reference.
- Add verified source_license_ref/license_class for every source doc used by the 12 scenarios.
- Create a source-disjoint train/held-out partition ledger with no shared doc_id/source_disjoint_group across train and held-out sides.
- Run/source a nearest-neighbour or source-overlap report for all 54 pair rows.
- Run/source held-out no-memorization controls for all 54 pair rows.
- Run/source target-disabled or asset-shuffled ablation controls before strict QUALITY-03 claim.
