# Ablation readiness: target-disabled / asset-shuffled

Run: `quality-controls-source-license-partition-20260618-073408`

Статус: `NOT_READY_TARGET_DISABLED_ASSET_SHUFFLED_BLOCKED_BY_NO_TARGET_AND_LICENSE_PARTITION_GAPS`.

Эта фаза может только описать readiness gaps. Target-disabled и asset-shuffled ablations не запускались: нет accepted licensed train/held-out assets, нет target checkpoint/run, model mutation запрещена.

## Контроли

- `target_disabled`: not available — no target/adaptive checkpoint mutation or runtime/model run authorized in this phase.
- `asset_shuffled`: not available — no accepted licensed train/heldout assets exist to shuffle.
- `source_id_removed`: not available — only metadata/artifact phase; no runner/model evaluation authorized.
- `nearest_neighbor_removed`: not available — no train rows and no nearest training source accepted.
