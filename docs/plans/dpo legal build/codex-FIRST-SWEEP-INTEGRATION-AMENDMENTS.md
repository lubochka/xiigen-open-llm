# Codex First-Sweep Integration Amendments

Artifact: `codex-FIRST-SWEEP-INTEGRATION-AMENDMENTS.md`  
Applies to: `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`, `codex-EXECUTION-CONTROL-BOARD.md`, `codex-REQUIREMENTS-COVERAGE-MATRIX.md`, `codex-BENCHMARK-DELTA-REPORT.md`, and referenced `codex-plan-00..14` blocks.  
Basis: verified GPT-5.5/xhigh review packets 01-25 in `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md`.  
Status: first-sweep amendment; must be reviewed by cycles 26-50 before final completion.

## 0. Honesty Lock

```text
verified_subagent_packets_completed = 25
verified_subagent_packets_required = 50
cycles_26_50_status = pending
duplicate_subagent_notifications_count_as_cycles = false
claude_files_edited = false
implementation_authorized = false
git_tests_build_training_run = false
```

Any future report that says "50 cycles done" is false unless `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` contains 50 returned packets.

## 1. Board And Coverage Canon

Canonical baseline execution board:

```text
baseline_phases = 12
baseline_subphases = 26
baseline_substeps = 59
reserve_rows = Ф10.3.1..Ф10.3.6
reserve_rows_in_baseline_count = false
reserve_rows_status = blocked_scope_gated
current_execution_pointer = NONE_PLAN_ONLY
ordered_phase_queue = [Ф0, Ф1, Ф2, Ф3, Ф4, Ф5, Ф6, Ф7, Ф8, Ф9, Ф10, Ф11]
phase.step.status vocabulary = done | in-progress | remaining | review-block | blocked
```

Rules:

- The baseline is 59 after adding `Ф4.3.0`; old `58` references are stale.
- `Ф10.3.*` reserve rows do not change baseline 59 unless Luba chooses Q41 Variant B.
- If Q41 Variant B is chosen, all six R15 domains activate through reserve rows `Ф10.3.1..Ф10.3.6`; these remain outside baseline 59 until Luba chooses Variant B.
- Board status accounting must show baseline rows separately from reserve rows.
- The exact pointer fields above are required for future sub-agent execution.

Coverage canon:

```text
requirements_total = 20
covered = 12
partial = 8
unmapped = 0
partial_set = R1, R6, R9, R14, R15, R16, R17, R20
```

R6 alias lock:

```text
R6_matrix = recon background evidence: 23 codex-recon-mvp files + 40 codex-recon-core reports; passes_unproven=true
R-11_master = thin agent/core boundary; use this name instead of "thin agent R6"
```

No file/report count may be described as `>=20 passes` or `>=40 passes` unless a pass ledger exists.

## 2. Graph Topology Canon

Node canon:

- `codex-plan-01-node.md` owns the node contract.
- `codex-plan-04-dynamicgraph-machine.md` must import or mirror the full `IDynamicGraphNode<TInput,TOutput>` + `DynamicGraphNodeDescriptor` contract, not a reduced `NodeId + UnitId` shape.
- Node feature-weight "matrix" means `features * PreferenceWeights -> top-N + honest NoDecision`; it is not a graph adjacency matrix.

Adjacency decision:

```text
graph_adjacency_matrix_representation = rejected_for_v1
graph_representation = DynamicGraphTopology edge list keyed by NodeId and ToNodeIds
node_index_map_required = false_for_v1
```

If Luba later requires an adjacency matrix, a new `NodeIndexMap`, row/column/cell schema, mutation invalidation rule, and export/import map must be added before execution.

Edge authority canon:

```text
IDynamicGraphEdge minimum fields:
  EdgeId
  FromNodeId
  ToNodeIds
  GraphEdgeKind = FanOut | FanIn | ManyToMany | MultiOutput | ReturnLoop
  ParallelEligible
  AuthorityUnitId
  AuthorityUnitLevel = ILogicalUnit
  DeterministicFinalAuthority = false
  BaselineUnitId
  BaselineRole = baseline | ablation_only
  ExcludedLeakageSignals
  SourceProof
  CutoverProof
  AblationProofRef
```

`AuthorityUnit` input must be `EdgeAuthorityInput`, never `SubPhaseRouterSourceDocument`, because the latter carries `SourcePath` and can leak source identity.

Cutover rule:

- F8 cutover can pass only when the selected learned edge unit already has `PASS_AS_TRAINED_FEATURE` under the full plan-11 two-PASS criteria.
- F9 must re-run leakage/permutation checks after cutover.

## 3. BuildDynamicTopology Canon

Add an explicit topology builder contract:

```csharp
BuildDynamicTopologyResult BuildDynamicTopology(
    DynamicGraphBuildSignal signal,
    TopologyBuildBudget budget);
```

The older `BuildDynamicTopology(current, signal, budget, ct)` shape is superseded for the V1 plan projection; current topology and cancellation are implementation-context inputs, not part of the target planning contract.

Required result fields:

```text
result_status = Changed | NoTopologyChange | NoValidTopology | BudgetExhausted | NeedsTeacher | Error
mutation_set
rollback_ref
created_or_updated_nodes
created_or_updated_edges
selected_edge_kind_reason
budget_used
failure_reason
graph_dpo_events
```

Gate B:

- Fan-out/fan-in/many-to-many/multi-output/return-loop selection must be explained by signal evidence.
- Budget includes max nodes, max edges, max topology mutations, max candidate topologies, timeout, and rollback budget.
- No valid topology emits a non-training result, not a fake trained PASS.
- Topology decisions that become training examples must convert into `GraphDpoPair(level=edge|graph)` through the legal DPO pipeline.

## 4. DPO Pair And Legal Provenance Canon

`legal_gate_passed=true` is necessary but not sufficient for `dpo_train_ready=true`.

Train-ready requires the full triad:

```text
raw_comparison_event complete
legal_provenance_dpo_envelope complete
training_batch projection complete
train_ready_without_triad_count = 0
triad_field_presence_rate = 1.0
```

Mandatory pair/envelope additions:

```text
candidate_order_hash
split_id
split_seed
sampling_params
chosen_authority
rejected_authority
chosen_origin
rejected_origin
near_miss_taxonomy_code
allowed_use_decision
legal_verdict
LegScope = CommonGlobal | PersonalAdaptive | EvalOnly | Quarantine
```

`SourceCategory` is a real enum, not an arbitrary string:

```text
SourceCategory =
  local
  human
  open_weight
  hosted
  deterministic_baseline
  self_judge
  client_unverified
  third_party
```

Row rejection ledger:

```text
RowRejectionReason =
  random_rejected
  copied
  missing_source
  teacher_unknown
  split_leak
  candidate_order_artifact
  closed_proprietary_training
  private_unredacted_hosted
  client_unverified
```

Five-format adapter proof:

Every source format must have a mapping row for: source field, target 14-field mapping, `TeacherProvenance` source/default, chosen authority, rejected authority, rejected taxonomy, triad projection, quarantine condition, and fixture/test.

Closed proprietary teacher output used as a training target has final dataset action:

```text
DATASET_REBUILD_REQUIRED
```

`BLOCK` can remain the local reason category, but the dataset action for train target contamination is rebuild.

Judge-only invariant:

- Closed proprietary models may be eval/judge-only only when their output is not used as `chosen` or `rejected` training target.
- If a judge-selected "better" output becomes `chosen`, `used_for_training=true`; closed judge then forces `DATASET_REBUILD_REQUIRED`.

Privacy/retention gates:

- Hosted raw private text without redaction and retention proof is `BLOCK_FOR_HOSTED`.
- Common/global training with private source, missing redaction, unknown license, unknown terms, unknown leg scope, or missing retention evidence is forbidden.
- `BLOCK_FOR_COMMON` means the row may be personal/adaptive only when all personal/adaptive privacy evidence passes.
- `LegScope` missing means `dpo_train_ready=false`.

Retention evidence must include policy file/version/date/hash, provider no-train or retention snapshot, TTL/deletion rule, redaction method, redaction output hash, and proof that no reconstructable private example remains.

## 5. Lifecycle, Measurement, And No-Hidden-Determinism

Canonical lifecycle manifest wire schema uses snake_case:

```text
manifest_id
schema_version
source_cutoff
late_file_queue
path_id_strategy
rollback_ref
teacher_provenance_ref
leg_scope
checkpoint_hash
loaded_manifest_hash
```

C# records may use PascalCase only with explicit serialization mapping. Import rejects unknown or incompatible `schema_version`.

Fresh-load proof must show a clean process:

```text
process_id
restart_id
trained_metric_before_export
fresh_metric_after_import
tolerance
state_hash_before_export
loaded_manifest_hash
loaded_checkpoint_hash
entrypoint_trace_id
no_replay_cache_proof
```

Lifecycle PASS must invoke `GetOrchestratorAsync()` after import and run the real `ILogicalOrchestrator` entrypoint on held-out/challenge input. `EvaluateAsync` alone is not sufficient.

Continue-training proof:

```text
Load(base) -> Load(lora) -> Apply -> Step
LoraCheckpointIo.Save/Load present
continue_training_delta > 0 after reload
```

Metric vocabulary:

```text
CpuMs
PeakRamMb
ElapsedMs
TrainRows
LiveMarginAccuracy
HeldOutAccuracy
RewardMargin
FreshLoadMetricDelta
ContinueTrainingDelta
```

No metric above may be `UNKNOWN`, placeholder, null, or omitted in a trained PASS.

F5 trained PASS is provisional until e2e/real-entrypoint evidence is present. Final `PASS_AS_TRAINED_FEATURE` requires held-out source-disjoint accuracy, ablation, leakage/permutation, lifecycle, e2e entrypoint, and non-UNKNOWN metrics.

## 6. Auto-DPO Loop Canon

Add:

```text
WeaknessEvent:
  source_input
  candidate_ids
  judge_mode
  weakness_score
  weakness_floor
  reason
  tie_break
  provenance
  no_weakness_verdict
```

```text
StopConfig:
  quality_threshold
  teacher_gap_epsilon
  plateau_window
  min_delta
  max_iterations
  budget_units
```

```text
ShouldStopResult:
  stop
  reason = threshold_met | plateau | budget_exhausted | no_signal | gate_failed | teacher_gap_closed
  measured_delta
  teacher_gap
```

```text
PromotionDecision:
  previous_checkpoint
  candidate_checkpoint
  frozen_reference_id
  heldout_before
  heldout_after
  regression_result
  rollback_ref
  locator_update_order
  append_only_ledger_row
```

F7 channel gate becomes:

```text
accepted_auto_pair_count_after_all_gates >= 1
```

If false: emit `NO_AUTO_DPO_SIGNAL`, do not call `TrainBatchAsync`, do not promote, and do not PASS.

On failed promotion or post-promotion regression: locator remains on the previous checkpoint or rollback restores it, then proof shows `m_prev` restored.

## 7. Leakage And Split Proof

Hard fail conditions:

```text
overlapDetected = true
sourceDisjoint = false
cross_split_duplicate_count > 0
cross_split_near_duplicate_count > 0
candidate_order_non_invariant = true
source_id_only_success = true
```

Any hard fail forbids held-out accuracy and `PASS_AS_TRAINED_FEATURE`.

Report fields:

```text
split_id
split_seed
split_algorithm
train_row_ids
heldout_row_ids
train_source_ids
heldout_source_ids
source_intersection_count
source_disjoint_evidence
train_source_roots
heldout_source_roots
source_root_classification = independent_roots | duplicate_derived | single_source
held_out_roots_or_sources
confidence_effect
normalized_text_hash
content_sha
simhash_or_minhash
near_duplicate_threshold
duplicate_group_id
source_id_only_accuracy
source_id_ablation_delta
offending_source_ids
```

## 8. API, Figma/HTML/CSS, FLOW-38, And Smart Output

Thin boundary:

- Client layer ingests source and may run deterministic baseline/helper rendering.
- Learned authority, DPO conversion, legal gate, training, measurement, and promotion stay in `llm_mvp_core`.
- Any deterministic UI generator is labelled baseline/helper and cannot be claimed as learned behavior.

Add `GeneratedUiOutputFeedbackEvent`:

```text
design_source_id
generated_output_hash
generated_output_kind = html_css | flow_result | smart_result
user_action = selected | corrected | rejected | scored
chosen_ref
rejected_ref
prompt_or_request_id
candidate_order_hash
near_miss_reason
split_id
SourceCategory
TeacherProvenance
redaction_policy_ref
retention_policy_ref
LegScope
dpo_train_ready
```

FLOW-38/Smart output gate:

- Q37 and Q38 are hard blockers for any FLOW-38 `dpo_train_ready=true`.
- Unknown, closed, client-unverified, or third-party better-output defaults to `QUARANTINE` and `semantic_quality_claimed=false`.
- Absence of closed-model markers is not proof of open-weight permissive origin.

## 9. Test Strategy And Source Inventory

P-13 canonical filename:

```text
codex-plan-13-test-strategy.md
```

`codex-plan-13-integration-tests.md` is not the canonical artifact unless created later.

FC-26 remains open until a 14x9 matrix is read back with zero empty cells and each check has assertion and Gate evidence.

Knowledge library manifest schema required before R6 can become covered:

```text
type
why_to_use
when_to_use
document_locations
implementation_locations
evidence_line_range
decision_supported
source_disjoint_evidence
confidence_effect
held_out_roots_or_sources
status
coverage_critic
cutoff_snapshot
file_list_or_index
evidence_depth_score
```

Add these manifest sections:

```text
Reference Inventory
Source-to-Plan Map
Source-to-Review Map
population_proof
non_empty_proof
pass_ledger
```

Until that exists:

```text
Knowledge_Library_schema_complete = false
R6_status = partial
one_file_equals_one_pass = false
```

## 10. Benchmark Delta Status

BDR accepted rows are now integrated by this amendment as design-level canonical repairs:

```text
T2 terms_snapshot_ref + geography/export = integrated_in_target_body_plan08
T3 raw_private_text + redaction/retention = integrated_in_target_body_plan08_plan10
T4 async A/B/C/D readability + fan-in/many-to-many = integrated_in_target_body_plan06
T5 SourceCategory enum + rejected_origin = integrated_in_target_body_plan07_plan08
```

`all_accept_status` after cycle-50 integration = `PARTIAL_T1_REMAINING_READBACK`; 50 returned review packets are complete, but T1 remains implementation-time manifest read-back, not plan-review count.

## 11. Final Questions Only

All unresolved questions stay in the plan's final question section. Current question set to preserve or strengthen:

1. Should Q41 Variant B activate all six R15 domains now, or should those six reserve rows remain blocked outside baseline until a later implementation decision?
2. Should generated HTML/CSS and FLOW-38 feedback default to `PersonalAdaptive` unless legal common proof is present?
3. Where does the personal/adaptive legal manifest live, and does it mirror `legal-provenance-manifest.json` with `LegScope=PersonalAdaptive`?
4. Is `redaction_policy_ref != none` enough for common admission, or must it prove no reconstructable private example remains?
5. Should Q37/Q38 block `dpo_train_ready=true` until answered and encoded in Gate B?
6. What held-out threshold is canonical for graph nodes/edges?
7. Is DynamicGraph e2e strictness 6/6 or N-1 allowed?
8. Should future versions define a true adjacency matrix, or is edge-list topology permanent for v1?
