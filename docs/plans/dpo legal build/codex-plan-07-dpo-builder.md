# Codex Plan 07 - DPO Builder

Artifact: `codex-plan-07-dpo-builder.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault.

## Authority Anchor

```text
CURRENT TASK: Preserve DPO-builder planning and finish 50 real review cycles.
NOT THE TASK: No implementation, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark and old packets.
```

## Canon

`GraphDpoBuilder.BuildPairAsync` produces a `GraphDpoPair` only when the legal gate, split gate, provenance gate, and authority gate pass. Empty batches are invalid for training.

Required pair fields follow the canonical `GraphDpoPair` record owned by
`codex-plan-09-auto-dpo-loop.md` §5.1. This builder may use legacy adapter aliases
only at the boundary: `chosen_output -> chosen`, `rejected_output -> rejected`,
`chosen_authority_unit_id -> chosen_authority`, and
`rejected_authority_unit_id -> rejected_authority`. The stored train-ready pair
must use one vocabulary, not both.

Required pair fields: `pair_id`, `level`, `node_or_edge_id`, `input_context`,
`chosen`, `rejected`, `wrong_markers`, `reason`, `teacher_provenance`,
`chosen_authority`, `rejected_authority`, `chosen_origin`, `rejected_origin`,
`source_category`, `leg_scope`, `legal_verdict`, `overlap_score`,
`teacher_provenance_ref`, `split_proof_ref`, `row_rejection_reason`, `source_ids`,
`held_out`, `candidate_order_invariant`, `excluded_leakage_signals`,
`dpo_train_ready`.

`SourceCategory` values are: local, human, open_weight, hosted, deterministic_baseline, self_judge, client_unverified, third_party.

`rejected_origin` must be explicit. Valid rejected-origin classes include random-rejected, copied, missing-source, teacher-unknown, split-leak, order-artifact, and baseline-loser.

## Gate B

- `dpo_train_ready=true` requires the full triad: legal accept, split accept, authority/provenance accept.
- `train_ready_without_triad_count=0`.
- Five input formats are adapted by explicit adapters; no adapter bypasses legal provenance.
- FLOW-38 and UI feedback enter only through `GeneratedUiOutputFeedbackEvent`.
- `NO_AUTO_DPO_SIGNAL=true` path creates no pair and no promotion.
- Current comparison-run packet records: 50/50; C49-C50 returned and integrated.

## Final Questions Only

None.
