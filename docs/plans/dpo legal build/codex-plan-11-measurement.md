# Codex Plan 11 - Measurement

Artifact: `codex-plan-11-measurement.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault.

## Authority Anchor

```text
CURRENT TASK: Preserve measurement planning and finish 50 real review cycles.
NOT THE TASK: No implementation, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark and old packets.
```

## Canon

Measurement must distinguish structural readiness, real trained improvement, and evidence quality. A trained PASS requires a real entrypoint, fresh load, held-out evaluation, and before/after comparison.

Required metrics: `PassesGate`, `RewardMargin`, `After`, `Before`, `TrainedNotCounterBaseline`, `triad_field_presence_rate`, `train_ready_without_triad_count`, `self_judge_train_ready_without_legal_gate`, `accepted_auto_pair_count_after_all_gates`, `cross_split_near_duplicate_count`.

Split proof fields: `train_source_roots`, `eval_source_roots`, `source_hashes`, `cross_split_near_duplicate_count`, `simhash_or_minhash`, `near_duplicate_threshold`.

## MeasurementReport Canon

Plan-11 owns the typed `MeasurementReport` contract. Guide-03 may explain how to
read it, but this plan is the canonical owner.

```text
record MeasurementReport {
  string report_id;
  string subject_ref;                 // node, edge, graph, pair batch, or run id
  string measurement_verdict;          // PASS_AS_HONEST_UNTRAINED_SCAFFOLD | PASS_AS_TRAINED_FEATURE | FAIL
  SpeedAxis speed;
  AccuracyAxis accuracy;
  QualityAxis quality;
  string[] evidence_refs;
}

record SpeedAxis {
  double before_latency_ms;
  double after_latency_ms;
  double throughput_delta;
}

record AccuracyAxis {
  double live_accuracy;
  double held_out_accuracy;
  double before_accuracy;
  double after_accuracy;
  double reward_margin;
  int cross_split_near_duplicate_count;
}

record QualityAxis {
  double exact_prompt_leakage_rate;
  bool candidate_order_invariant;
  bool trained_not_counter_baseline;
  bool no_unknown_metric_values;
}
```

`live_accuracy` and `held_out_accuracy` are separate fields. A trained feature
cannot pass on live/train accuracy alone.

## Gate B

- `UNKNOWN` is not a passing metric value.
- Near duplicate count above zero blocks accuracy claims until adjudicated.
- Edge authority reports include authority id, baseline role, leakage exclusions, source proof, cutover proof, and ablation proof.
- No-signal auto-DPO path has accepted count 0, no train, no promotion, previous checkpoint retained.
- Current comparison-run packet records: 50/50; C49-C50 returned and integrated.

## Final Questions Only

None.
