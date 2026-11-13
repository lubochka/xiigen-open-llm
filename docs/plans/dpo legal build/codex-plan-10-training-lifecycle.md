# Codex Plan 10 - Training Lifecycle

Artifact: `codex-plan-10-training-lifecycle.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault.

## Authority Anchor

```text
CURRENT TASK: Preserve training lifecycle planning and finish 50 real review cycles.
NOT THE TASK: No implementation, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark and old packets.
```

## Canon

Training lifecycle must prove a real load/train/evaluate path, not a cached or synthetic pass. The LoRA/DPO sequence is `Load(base) -> Load(lora) -> Apply -> Step -> Evaluate -> PromoteOrRollback`.

Naive `SetValue` mutation is not accepted as proof of trainable behavior.

Required manifest fields: `manifest_id`, `schema_version`, `graph_id`, `node_ids`, `edge_ids`, `edge_authority`, `policy_version`, `source_cutoff`, `late_file_queue`, `path_id_strategy`, `checkpoint_hash`, `base_checkpoint_hash`, `lora_checkpoint_hash`, `rollback_ref`, `teacher_provenance_ref`, `split_proof_ref`.

Lifecycle proof fields: `process_id`, `restart_id`, `entrypoint_trace_id`, `no_replay_cache_proof`, `loaded_checkpoint_hash`, `loaded_manifest_hash`, `manifest_schema_version`, `source_cutoff`, `path_id_strategy`, `rollback_ref`.

## Gate B

- Fresh clean process load is required before claiming trained PASS.
- Rollback loads the previous checkpoint and reproduces the previous metric.
- Manifest schema version is present and compatible.
- Teacher provenance uses the 18-field canon from plan-08.
- Current comparison-run packet records: 50/50; C49-C50 returned and integrated
  in the current-run ledger.

## Final Questions Only

None.
