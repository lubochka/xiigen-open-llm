# Orchestrator Teaching Plan â€” One Row at a Time
# Version: 5.0 â€” 2026-05-08
# Supersedes: ORCHESTRATOR-TEACHING-PLAN-v4.md
# Authority: DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3.md
#             EXAMINATION-METHOD-ORCHESTRATION-v13.md
#             XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md
#
# v5 additions over v4:
# R1: Registry reference updated to v11. Source priority table and pair type
#     list now cover all 34 types and all A/B-series families from the
#     extended corpus scan. Teaching pairs from Types 23â€“34 and B-series
#     families have explicit priority assignments.
# R2: Examination method reference updated to v13. Pair kind allowed list
#     extended with topology_graph, example_catalog, and naming_translation
#     from v13. B-series failure modes (EM-37â€“EM-40) added.
# R3: Batch composition minimums aligned with DPO-PAIR-AUTHORING-GUIDANCE-
#     ARCH-PLAN-v6 subdivision rules. v4 listed flat percentages; v5 maps
#     them to v6 subtypes (Type A subtype breakdown within the 40% quota).
# R4: Source pair selection priority extended with Types Gâ€“J from v6
#     guidance and pre-made pair sources from registry Types 23â€“34:
#     validation reports (Type 32), conflict analysis (Type 33),
#     naming tables (Type 31), positive/negative catalogs (Type 21/A21).
# R5: Canonicality gate added as explicit pre-Step-0 check. mirror/stale/
#     archive_container/secure_operational â†’ reject before any other check.
# R6: Per-source diagnosis table extended with B-series families.
# R7: Round dependency table extended with planning pairs from
#     Phase implementation plans (Type 29).
# All v4 additions (Q1â€“Q6) and v3 corrections (P1-1 through P2-8) preserved.

---
## 2026-05-11 Internal Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

This guide must be read with the current run evidence in:
`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json`

Current protected baseline: RUN-416 / V246, 672 rows, weighted accuracy 0.7068,
weighted reward margin 0.016892, domains orchestration 257 / plans 341 / skills 74.
Future additions must protect orchestration, plans, and skills first.

Hard fixes from the live run:

1. Do not use deterministic filename/path code to decide semantic authority.
   Scripts may inventory, validate, split, hash, and train. They may not decide
   that a user request, `basic_prompt.txt`, session-start prompt, genesis prompt,
   or flow initiation document is low-authority without model-led registry
   adjudication.
2. Root prompt and user-intent documents are potential prompt authority. They
   belong in model-led authority adjudication, not generic review-required, when
   the script cannot type them.
3. Train-ready pairs must satisfy: `chosen != rejected`, rejected contains a
   specific `[WRONG: ...]`, no mojibake in prompt/a_side/chosen/rejected, source
   span recorded, canonicality/version state explicit.
4. Ledger, status, audit, inventory, and progress rows are evidence surfaces.
   They may justify pairs but must not be treated as DPO rows themselves.
5. New or repaired sets must run as 16-64 row standalone ablations before combo
   promotion. Promote only if reward margin is positive and weighted accuracy
   does not damage the protected baseline.
6. External code-documentation rows must not be mixed directly into internal
   orchestration/plans/skills training; they require their own code-doc gates.
7. If a document is a new type vs a new version, decide by structure, consumer
   action, A-side question, and lifecycle/canonicality. Do not decide by name
   alone.

Script references updated for this rule:

- `prepare_internal_learning_sets.py` now writes `model-led-authority-candidates.jsonl`
  for root prompt/user-intent candidates and `training-pairs-repair-required.jsonl`
  for rows blocked by hard DPO gates.
- `augment_internal_learning_sets_tier3.py` now routes B162/B165 prompt-authority
  candidates to model-led authority adjudication instead of treating them as
  ordinary low-confidence review rows.

---

## What this plan is

This plan teaches the adaptive orchestrator one training pair at a time.
After each pair the system runs a live prompt through every flow node, captures
CPU, memory, disk, and elapsed time, checks the verdict, and records whether
the pair improved the model or made it worse. The next pair is added only after
the measurement for the current pair is recorded.

This is not batch training. Batch training teaches many pairs at once and
produces one accuracy number. This plan teaches one pair, tests, measures,
decides, then moves to the next. Every step is observable and reversible.

**Relationship to the batch DPO plan:** XIIGEN-DPO-INTERNAL-TEACHING-PLAN-v2.md
produces pair files for the batch calibration runner. This plan uses those same
pair files â€” or pairs directly from the internal source registry â€” to teach the
live adaptive orchestrator one row at a time. The batch plan confirms a pair
produces positive reward margin in bulk training; this plan teaches the live
model from the confirmed pairs.

---

## Why one row at a time

A learned unit registered at `Probationary` trust state is selected by the flow
factory only when `UseLearnedUnitWhenAvailable` is true. If the measurement shows
a pair made the unit worse, the unit is superseded and the deterministic fallback
retakes the slot. The next pair is a fresh attempt.

The machine never silently degrades. Every change is measured before the next
change is applied.

---

## Pair quality gate â€” applies before any LoRA work

This gate runs at Step 1 before encoding. A pair that fails is discarded. No
LoRA training runs on a pair that fails the quality gate.

### Token overlap test

```
shared = tokens in both chosen and rejected that are NOT also in the prompt
total  = tokens in chosen + tokens in rejected âˆ’ tokens in prompt
overlap_ratio = shared / total
```

**overlap_ratio > 0.60 â†’ discard the pair. Do not proceed to Step 2.**

Why this gate is placed at Step 1, not at pair file creation: pairs may arrive
from multiple sources (batch DPO file, correction event, held-out set). The gate
ensures no pair reaches the LoRA training step with insufficient distributional
separation, regardless of where it originated.

Expected overlap ratios by pair type:

| Pair type | Expected overlap | Expected LoRA outcome |
|---|---|---|
| Correction event from Source 2 export (Type 3) | 0.10â€“0.25 | Positive margin_improvement |
| Positive/negative catalog (A21 / Type 21) | < 0.30 | Positive margin_improvement |
| Gap remediation wrong-fix pair (Type 23 / U14) | 0.15â€“0.30 | Positive margin_improvement |
| Gap remediation false-pass ArbiterUnit (Type 23) | 0.10â€“0.20 | Positive margin_improvement |
| Validation report verdict (Type 32) | 0.15â€“0.25 | Positive margin_improvement |
| Conflict analysis unchosen alternative (Type 33) | 0.10â€“0.25 | Positive margin_improvement |
| Naming/mapping translation table (Type 31) | 0.05â€“0.15 | Positive margin_improvement |
| Config schema violation (Type 25) | 0.10â€“0.30 | Positive margin_improvement |
| Source code contract field (Type 26) | 0.10â€“0.25 | Positive margin_improvement |
| Diagram topology direction (Type 27 / A13) | 0.10â€“0.20 | Positive margin_improvement |
| Test suite assertion (Type 28 / A15) | 0.10â€“0.25 | Positive margin_improvement |
| ARCH Type A (boundary vs implementation) | 0.10â€“0.20 | Positive margin_improvement |
| PLAN Type B with strong annotation | 0.35â€“0.50 | Positive margin_improvement |
| Phase plan ordering pair (Type 29) | 0.30â€“0.45 | Positive margin_improvement |
| Type C (wrong domain) | 0.15â€“0.35 | Positive margin_improvement |
| Generated code debug pair (Type 24 / U23) | 0.25â€“0.50 | Positive with strong annotation |
| Versioned document evolution (Type 16) | 0.05â€“0.15 | Positive margin_improvement |
| Negation template (forbidden) | 0.65â€“0.80 | Negative margin_improvement |

### Annotation quality check

If `overlap_ratio` is between 0.40 and 0.60, the annotation must be strong
enough to carry the distributional signal. Check:

```
Annotation is strong if it:
  â€” names the specific sub-phase the rejected incorrectly addresses
  â€” explains why that sub-phase does not own this responsibility
  â€” states what the prompt actually asked for that rejected missed

Annotation is weak if it:
  â€” says only "this is incorrect" or "wrong approach"
  â€” names the correct sub-phase but does not explain the confusion
  â€” is absent entirely
```

A pair with `overlap_ratio` between 0.40 and 0.60 AND a weak annotation is
discarded. These pairs are borderline on overlap and rely on the annotation
to carry the training signal. Without a strong annotation, reward margin
will be near zero.

### Pair selection priority from internal source registry

When selecting which pairs to teach in a round, prefer in this order:

```
Priority 1 â€” Pre-made pairs requiring no rejected construction (lowest effort,
             highest trust when provenance is confirmed)
  1a. Correction events from Source 2 export (Type 3; overlap 0.10â€“0.25)
      Bundle order: legacy-docs-skills-from-old-code â†’ figma-to-code â†’ flow-01-station-testing
  1b. Positive/negative example catalogs (Type 21 / A21; overlap < 0.30)
      Gate 1 label verification mandatory before use.
  1c. Gap remediation pairs from U14 / Type 23 (overlap 0.10â€“0.30)
      Three-part structure required: wrong-fix â†’ correct-fix with evidence.
  1d. Validation report pairs (Type 32; overlap 0.15â€“0.25)
      Real gate verdict as chosen; fabricated concern as rejected.
  1e. Conflict and comparison analysis pairs (Type 33; overlap 0.10â€“0.25)
      Unchosen alternative is pre-made rejected from the document.

Priority 2 â€” Pairs from high-precision registry types
  2a. ARCH Type A from Source 1 rows 1â€“520 (Type 2 / ARCH zone; overlap 0.10â€“0.20)
      Reclassify as ARCH domain before teaching.
  2b. Source code contract pairs (Type 26; overlap 0.10â€“0.25)
      Exact interface/DTO field pairs; stronger than prose for boundary pairs.
  2c. Configuration schema pairs (Type 25; overlap 0.10â€“0.30)
      Schema enum violations; binary pass/fail; strongest for ArbiterUnit pairs.
  2d. Diagram topology pairs (Type 27 / A13; overlap 0.10â€“0.20)
      Parse nodes and edges; never flatten to prose first.
  2e. Test suite assertion pairs (Type 28 / A15; overlap 0.10â€“0.25)
      Passing test assertion as chosen; assertion-failing variant as rejected.

Priority 3 â€” PLAN Type B positive zone
  Source 1 rows 521â€“727 (overlap 0.15â€“0.35)
  Confirmed positive margin in calibration batches 16 and 17.
  Phase implementation plan pairs (Type 29; overlap 0.30â€“0.45) alongside.

Priority 4 â€” All other eligible pairs from Types 1â€“34
```

Do not teach pairs from Source 1 rows 1235â€“1753 as PLAN or ARCH A sides.
These are the meta-commentary rows confirmed negative margin in batch 30
(acc=0.306). Correction events from this range are acceptable.

Do not teach pairs from B11 (README/summary documents) as B-side authority.
These are A-side prompt sources only.

Do not teach pairs from A18 (vendored codebases) or U22 (external training sets)
in the internal teaching pipeline. These require the external registry and a
separate batch file with mixed_with_internal: false.

---

## Canonicality gate â€” applies before Step 0

Every source document must be assigned a canonicality class before a pair from
it enters the teaching pipeline. This gate runs before the baseline snapshot.
A pair whose source fails the canonicality check is rejected immediately â€” the
pair quality gate and overlap test do not run.

```
canonical          â†’ permitted; proceed to Step 0
extracted_copy     â†’ permitted if extraction is dated and archive path recorded
mirror             â†’ rejected; use canonical copy instead
stale              â†’ rejected unless pair is a Type 16 superseded-vocabulary pair
archive_container  â†’ rejected; unpack and reclassify source first
secure_operational â†’ rejected; do not log path in ledger
external           â†’ separate external-only batch file required;
                     mixed_with_internal: false must be set
```

Record `source_canonicality` and `canonical_confirmed_via` in the TeachingRowRecord
before any other field. A row without these fields is not accepted as evidence
regardless of verdict.

---

## Prerequisites â€” teaching harness before any row

Before the first row is taught, four wiring steps must be complete.

**Harness step 1 â€” learned-unit selector per taught role.**

The only factory that currently selects learned units is
`SecurityVerification34FlowFactory` for the node-08 arbiter role.
`PromptRoutingInputCompilerUnit`, `MultiSubPhaseRouterUnit`, and the generic
nine-node flow are constructed directly â€” they never consult the unit registry.
Before teaching a role, write one thin factory per taught role following the
same pattern as `SecurityVerification34FlowFactory`. Register it in the flow
registry for the sub-phase being taught.

**Harness step 2 â€” tracing wrappers for the verification run.**

`TracingFlowDecorator` and `TracingLogicalUnit` exist but
`AdaptiveLogicalOrchestrator.RunAsync` does not wrap them automatically.
Before the verification run in Step 5, register `TracingFlowDecorator`-wrapped
flows for the sub-phases being taught and collect `StationTrace` objects via
the `emitTrace` callback.

**Harness step 3 â€” LoRA adapter inclusion in the forward path.**

`LoraApplicator.Apply` must be called on the same `LlmModel` instance used by
`DpoTrainer`. Do not create a separate model copy between these two calls.
`LoraConfig.Scaling` is computed (Scaling = Alpha / Rank). Use Alpha = 2f with
Rank = 4 to achieve Scaling = 0.5. Never set Scaling directly â€” it has no setter.

**Harness step 4 â€” checkpoint field split.**

```
LoraAdapterBytes    â€” sum of (Rank * InDim + OutDim * Rank) * 4 bytes per adapter
                      computed from the adapter list, not from the checkpoint file
DpoMetricBytes      â€” sum of dpo_step_*.json file sizes in checkpointDir
BaseCheckpointBytes â€” new FileInfo(checkpointPath).Length
```

The stopping rule "LoRA adapters must be â‰¤ 2MB each" applies to `LoraAdapterBytes`,
not `BaseCheckpointBytes`.

---

## Components being taught

| Component | Contract | Role |
|---|---|---|
| Prompt routing input compiler | `ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>` | Layer 0-A |
| Domain classifier | `ILogicalUnit<DomainClassifierRequest, DomainClassification>` | Layer 0-B |
| Multi-sub-phase router | `ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>` | Layer 0-C |
| Domain splitter | `ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>` | Node 01 |
| Domain translator | `ILogicalUnit<DomainPacket, TranslatedDomainStatement>` | Node 02 |
| Scheme planner | `ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>` | Node 03 |

Teaching starts with routing (compiler â†’ domain classifier â†’ router) because
these gate every downstream node. If routing is wrong, no downstream teaching
matters.

---

## The measurement record

Every pair produces one `TeachingRowRecord` before the next pair begins.

```csharp
public sealed record TeachingRowRecord(
    // Identity
    string PairId,
    string ComponentRole,
    string SubPhaseId,
    string DomainLabel,         // CODE_Q, BUG, PLAN, ARCH, REFACTOR â€” from D-04
    string TransitionType,
    int    RowIndex,
    string SourceRegistryId,    // which entry in XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md
    string SourceSessionRows,   // e.g. "521-727" for Source 1 positive zone

    // Pair quality fields â€” recorded before training
    double OverlapRatio,        // shared_tokens / total_tokens; > 0.60 = should not have reached here
    string AnnotationQuality,   // "strong", "weak", "absent"

    // Training resource consumption
    double TrainingElapsedMs,
    double TrainingCpuPercent,          // (cpuDelta / elapsed / ProcessorCount) * 100
    double TrainingWorkingSetDeltaMb,   // before/after delta, not a sampled peak
    double TrainingManagedAllocationMb,
    double TrainingDiskReadBytes,
    double TrainingDiskWriteBytes,
    long   BaseCheckpointBytes,
    long   LoraAdapterBytes,            // from adapter list: sum(loraA + loraB) * 4
    long   DpoMetricBytes,

    // DPO step metrics â€” from dpo_step_*.json
    double DpoLoss,             // should move below 0.693 for learning to be occurring
    double DpoRewardMargin,     // chosen_reward - rejected_reward from DPO step
    double DpoAccuracy,         // fraction of pairs where chosen_reward > rejected_reward

    // Inference resource consumption
    double InferenceElapsedMs,
    double InferenceCpuPercent,
    double InferenceWorkingSetDeltaMb,

    // Result verification
    string PromptUsedForVerification,
    string ExpectedVerdict,
    string ActualVerdict,
    bool   VerdictCorrect,
    double ConfidenceBefore,
    double ConfidenceAfter,
    double MarginImprovement,   // ConfidenceAfter - ConfidenceBefore; negative = regression

    // Decision
    LogicalUnitTrustState TrustStateAssigned,
    string Decision,            // "promoted", "rejected", "held"
    string DecisionReason,
    string RejectionDiagnosis,  // populated only when Decision = "rejected"

    // Canonicality fields â€” required before Step 0
    string SourceCanonicality,      // "canonical", "extracted_copy" only at submission
    string CanonicalConfirmedVia,   // document state register or "What changed" header
    string PairKind                 // from allowed list in EXAMINATION-METHOD-ORCHESTRATION-v13
);
```

All fields are written before moving to the next pair.

---

## The sub-cycle for one training pair

### Step 0 â€” Baseline snapshot

Before touching anything, run the current deterministic unit on the verification
prompt and record:

```
baseline_verdict        â€” actual verdict from the deterministic unit
baseline_confidence     â€” Confidence from LogicalUnitResult
baseline_inference_ms   â€” elapsed time for one inference pass
baseline_working_set_mb â€” working set during inference
```

The verification prompt must be an eligible prompt â€” not meta-commentary, not
a session-loading command. Prefer a prompt from the internal source priority list.

### Step 1 â€” Pair quality gate

Apply the token overlap test and annotation quality check (see Pair Quality Gate
section above). Record `overlap_ratio` and `annotation_quality` in the row record
whether the pair passes or fails.

**If the pair fails the gate:**
- Record the failure in the ledger with `decision: "discarded_quality_gate"`
- Log `overlap_ratio` and the specific failure reason
- Move to the next pair. Do not proceed to Step 2.

**If the pair passes:**
- Proceed to Step 2.

### Step 2 â€” Encode the pair

The A side becomes `Prompt` in `DpoPair`.
The B side becomes `Chosen`.
The rejected B (from the pair file, Type A / B / C) becomes `Rejected`.

Tokenize both `Chosen` and `Rejected` using `BpeTokenizer.Encode()`.

After tokenisation, verify the encoded token counts:
```
If len(chosen_tokens) < 5 OR len(rejected_tokens) < 5:
  discard pair â€” too short for meaningful DPO signal
```

### Step 3 â€” Apply LoRA and train

```csharp
var adapters = LoraApplicator.Apply(model, new LoraConfig
{
    Rank          = 4,
    Alpha         = 2f,       // Scaling = Alpha / Rank = 0.5; never set Scaling directly
    Dropout       = 0.0f,
    TargetModules = ["q_proj", "v_proj"]
});
```

Start measurements, run one DPO step, stop measurements:

```csharp
var diskBefore  = new DriveInfo(checkpointDir[0].ToString()).AvailableFreeSpace;
var memBefore   = GC.GetAllocatedBytesForCurrentThread();
var wsBefore    = Process.GetCurrentProcess().WorkingSet64;
var cpuBefore   = Process.GetCurrentProcess().TotalProcessorTime;
var sw          = Stopwatch.StartNew();

var trainer = new DpoTrainer(policyModel, referenceModel, checkpointDir);
var metrics = trainer.Step(batch, step: rowIndex);

sw.Stop();

training_elapsed_ms            = sw.Elapsed.TotalMilliseconds
training_cpu_percent           = (cpuAfter - cpuBefore).TotalMilliseconds
                                 / sw.Elapsed.TotalMilliseconds
                                 / Math.Max(1, Environment.ProcessorCount) * 100.0
training_working_set_delta_mb  = Math.Max(0, wsAfter - wsBefore) / 1_048_576.0
lora_adapter_bytes             = adapters.Sum(a =>
                                 (long)(a.LoraA.Value.Data.Length
                                 + a.LoraB.Value.Data.Length) * sizeof(float))
dpo_loss                       = metrics.Loss          // read from dpo_step_*.json
dpo_reward_margin              = metrics.RewardMargin  // chosen_reward - rejected_reward
dpo_accuracy                   = metrics.Accuracy
```

Record `dpo_loss`, `dpo_reward_margin`, `dpo_accuracy` in the row record.
These are the DPO-step-level metrics â€” separate from the inference-level
`margin_improvement` computed in Step 6.

**If `dpo_reward_margin < 0` at Step 3:**
The pair produced negative reward margin in the DPO step itself. This means
the model already preferred rejected over chosen before the step, and the step
reinforced the wrong direction. Record this and proceed to Step 5b â€” the
verification run will confirm whether the unit degraded.

### Step 4 â€” Wrap trained model as ILogicalUnit

Register the trained unit at `Probationary` trust state. Apply slot supersession
before registration:

```csharp
// Step 4a â€” supersede prior entries to prevent stale Probationary units from
// re-activating after a rejected decision.
unitRegistry.Register<TInput, TOutput>(
    unitId:     $"unit-{role}-slot-superseded-row-{rowIndex:D4}",
    unitRole:   role,
    domainTag:  subPhaseId,
    version:    $"1.0.row{rowIndex}.superseded",
    trustState: LogicalUnitTrustState.Candidate,  // below Probationary minimum
    isLearned:  false,
    unit:       deterministicFallback);

// Step 4b â€” register the trained unit.
unitRegistry.Register<TInput, TOutput>(
    unitId:     $"unit-{role}-trained-row-{rowIndex:D4}",
    unitRole:   role,
    domainTag:  subPhaseId,
    version:    $"1.0.row{rowIndex}",
    trustState: LogicalUnitTrustState.Probationary,
    isLearned:  true,
    unit:       trainedUnit);
```

### Step 5 â€” Wrap verification run with tracing decorators

`AdaptiveLogicalOrchestrator.RunAsync` does not wrap units automatically.
Build the trace-collecting harness before the verification run:

```csharp
var collectedTraces = new List<StationTrace>();
Action<StationTrace> emitTrace = trace => collectedTraces.Add(trace);

var innerBinding = flows.ResolveBindingBySubPhase(subPhaseId, factoryRequest);
if (innerBinding.IsSuccess)
{
    var tracingFlow = new TracingFlowDecorator<string>(
        innerBinding.Value!.Flow,
        $"verify-{subPhaseId}-row-{rowIndex:D4}",
        emitTrace);
    flows.Register<string>(
        innerBinding.Value.FlowId + "-tracing",
        innerBinding.Value.ScenarioId,
        "tracing",
        subPhaseId, phaseId, taskType, domainTags,
        tracingFlow);
}
```

### Step 5b â€” Run the verification prompt

Run with `UseLearnedUnitWhenAvailable: true`. Measure inference resources.
Collect per-branch and per-node results.

The verification prompt must match the pair's A side â€” not a different prompt.
The expected verdict is the B side outcome the pair was teaching.

### Step 6 â€” Compute margin improvement and decide trust state

```
margin_improvement = confidence_after - confidence_before
dpo_reward_margin  = from Step 3 metrics (chosen_reward - rejected_reward)
```

**Decision logic:**

```
If actual_verdict == expected_verdict
   AND margin_improvement >= 0.0
   AND confidence_after >= 0.50:     â†’ "promoted", Probationary

If actual_verdict == expected_verdict
   AND margin_improvement < 0.0:     â†’ "held", Probationary
                                       (correct verdict but confidence regressed)

If actual_verdict == expected_verdict
   AND confidence_after < 0.50:      â†’ "held", Probationary
                                       (correct verdict but confidence too low to trust)

If actual_verdict != expected_verdict:  â†’ "rejected"
```

**When decision is "rejected" â€” record RejectionDiagnosis:**

```
Check dpo_reward_margin from Step 3:
  If dpo_reward_margin < 0:
    RejectionDiagnosis = "negative_dpo_margin: model preferred rejected over chosen
                          at training time â€” pair overlap_ratio was {overlap_ratio}"

Check overlap_ratio from Step 1:
  If overlap_ratio > 0.50:
    RejectionDiagnosis += " | high_overlap: chosen and rejected too similar"

Check annotation_quality from Step 1:
  If annotation_quality = "weak" or "absent":
    RejectionDiagnosis += " | weak_annotation: distributional signal insufficient
                            for Type B pair"

If none of the above: RejectionDiagnosis = "verdict_mismatch: model output did not
                        match expected; check chosen content accuracy"
```

**Promotion from Probationary to Trusted:** five consecutive "promoted" decisions
for the same role and sub-phase. Until then the deterministic unit is always the
safe fallback.

### Step 7 â€” Write the teaching row record

Write all fields to the session ledger before moving to the next pair. The ledger
is a JSON-lines file â€” one record per line, never overwrite.

---

## Session ledger format

```json
{
  "pair_id": "O1-compiler-row-001",
  "component_role": "prompt-routing-input-compiler",
  "sub_phase_id": "whole_development_area",
  "domain_label": "ARCH",
  "row_index": 1,
  "source_registry_id": "source-1-arch-zone",
  "source_session_rows": "1-520",
  "pair_quality": {
    "overlap_ratio": 0.18,
    "annotation_quality": "strong",
    "gate_passed": true
  },
  "training": {
    "elapsed_ms": 412.3,
    "cpu_percent": 34.1,
    "working_set_delta_mb": 14.2,
    "managed_allocation_mb": 3.1,
    "disk_read_bytes": 2048,
    "disk_write_bytes": 5242880,
    "base_checkpoint_bytes": 5242880,
    "lora_adapter_bytes": 49152,
    "dpo_metric_bytes": 1024,
    "dpo_loss": 0.631,
    "dpo_reward_margin": 0.0023,
    "dpo_accuracy": 0.72
  },
  "inference": {
    "elapsed_ms": 38.7,
    "cpu_percent": 16.2,
    "working_set_delta_mb": 6.8
  },
  "verification": {
    "prompt": "Define the component boundaries for the notification service.",
    "expected_verdict": "PASS",
    "actual_verdict": "PASS",
    "verdict_correct": true,
    "confidence_before": 0.62,
    "confidence_after": 0.79,
    "margin_improvement": 0.17
  },
  "decision": {
    "trust_state": "Probationary",
    "outcome": "promoted",
    "reason": "Verdict correct. Confidence improved by 0.17. DPO margin positive.",
    "rejection_diagnosis": null
  },
  "node_traces": [
    {
      "node_role": "prompt-routing-input-compiler",
      "elapsed_ms": 2.1,
      "cpu_percent": 38.0,
      "memory_mb": 1.2,
      "verdict": "0.92",
      "used_learned_unit": true
    }
  ]
}
```

`dpo_loss`, `dpo_reward_margin`, `dpo_accuracy`, `overlap_ratio`,
`annotation_quality`, `source_registry_id` required from v4.
`source_canonicality`, `canonical_confirmed_via`, `pair_kind` added in v5.
All fields required before moving to the next pair.

---

## Round ordering

Work is ordered so routing is taught first. Within each round, pairs are
ordered by the internal source priority: correction events first, then ARCH
zone rows, then PLAN positive zone rows.

### Round 1 â€” Prompt routing input compiler (Layer 0-A)

Row selection order (priority from internal registry):
```
Rows 1â€“5:   Correction events from Source 2 export
             (legacy-docs-skills bundle, ARCH/CODE_Q corrections)
Row 6:      ARCH zone prompt â€” "Define the component boundaries for the payment service"
Row 7:      PLAN positive zone â€” "continue from where you stopped â€” Phase 1,
             Factory Interfaces and Engine Processing"
Row 8:      UI-only prompt â€” "Redesign the login screen layout"
Row 9:      Multi-predicate â€” "Architecture design and integration tests for the auth layer"
Row 10:     Stage 9 corruption â€” wrong predicate assigned
```

After row 10: pause and review. If fewer than 7 of 10 decisions are "promoted"
or "held" â€” or if `dpo_loss` has not moved below 0.680 across rows 1â€“10 â€”
run the round-level margin audit before proceeding.

### Round 1b â€” Domain classifier (Layer 0-B)

One domain classifier row per prompt from Round 1. A side: compiled prompt +
predicates. B side: domain label (CODE_Q / BUG / PLAN / ARCH / REFACTOR).

Rejected (Type C): wrong domain label for the prompt's intent verb. Use the
intent verb â†’ domain table from the examination method.

### Round 2 â€” Multi-sub-phase router (Layer 0-C)

Router A sides come from compiler B outputs recorded in Round 1. Do not invent
A shapes. Row ordering follows the same source priority â€” correction events first.

Rows 1â€“5: single and dual sub-phase routing from correction event prompts.
Rows 6â€“9: architecture-only, PLAN multi-phase, no-predicate, unregistered.
Row 10: Stage 9 ambiguous-predicate-case corruption.

### Round 3 â€” Domain splitter (Node 01)

A side: selected A/B/X transition from domain classifier B side.
Row ordering: one-packet rows first, then multi-packet, then knowledge-edge.

### Round 4 â€” Domain translator (Node 02)

One packet at a time. Correction-event-derived security packets first (highest
overlap separation), then architecture, API, testing.

### Round 5 â€” Scheme planner (Node 03)

Primary PLAN training signal. A side: full set of translated statements.
B side: ordered work branches with `depends_on` fields.

Every multi-task pair must be Type B rejected (wrong dependency order). Use
the dependency ordering table:

| Correct | Rejected | Why wrong order fails |
|---|---|---|
| 3.4 Security â†’ 5.2 Integration Tests | 5.2 first with mocks | Mock tokens bypass validation â€” cannot test real isolation |
| 3.1 Architecture â†’ 4.1 Backend Services | 4.1 first, 3.1 later | Implementing without decided boundaries creates coupling to undo |
| 3.2 Data Model â†’ 3.3 API Contract | 3.3 first, model from contract | Contract shape depends on model shape â€” leaks implementation into public API |
| 3.5 Observability Design â†’ 7.3 Monitoring | 7.3 first, design retroactively | Cannot configure collection of signals that have not been defined |
| 3.3 API Contract â†’ 4.5 Integration Connectors | 4.5 first, contract from implementation | Implementation detail leaks into public contract |
| Phase N step A â†’ Phase N step B (Type 29) | Step B first, step A after | Phase implementation plans encode explicit prerequisites; skipping them creates rework |

For Round 5, the confidence threshold in Step 6 applies strictly:
`confidence_after < 0.50` â†’ "held", not "promoted", even with correct verdict.
PLAN scheme planner pairs are the most complex; low confidence means the model
has not learned the dependency ordering reliably.

---

## Round-level margin audit

Triggered when: fewer than 6 of 10 rows in a round are "promoted" or "held",
OR `dpo_loss` has not moved below 0.680 across the round.

The loss floor of 0.680 is derived from the calibration run: loss at 0.693
= ln(2) = random-guess level. Movement to 0.680 indicates the model is
beginning to distinguish chosen from rejected. A round that does not show
this movement has a systematic pair quality problem.

**Audit procedure:**

```
1. List all rows in the round with dpo_reward_margin < 0.
2. For each: check overlap_ratio and annotation_quality from the row record.
3. Identify pattern:

   If all rejected rows have overlap_ratio > 0.50:
     â†’ Pair vocabulary too similar. Replace rejected with more distant sub-phase.

   If rejected rows have overlap_ratio < 0.50 but annotation_quality = "weak":
     â†’ Annotation not providing enough distributional signal for Type B pairs.
       Strengthen annotations before continuing.

   If all rejected rows have overlap_ratio > 0.50:
     â†’ Pair vocabulary too similar. Replace rejected with more distant sub-phase.

   If rejected rows have overlap_ratio < 0.50 but annotation_quality = "weak":
     â†’ Annotation not providing enough distributional signal for Type B pairs.
       Strengthen annotations before continuing.

   If overlap and annotation are fine but verdict_correct = false:
     â†’ Chosen content inaccurate. Review the B side of failing pairs.

   If dpo_reward_margin < 0 across the round but overlap_ratio < 0.40:
     â†’ Model size may be insufficient for the vocabulary size.
       Consider reducing LoraConfig.Rank from 4 to 2 and restarting the round.

   NEW in v5 â€” B-series failure modes:
   If source_registry_id points to B02 serialized parts (PHASExA_DNA1_*.md etc.):
     â†’ Check whether a merged version exists. Parts files may be mirrors.
       If merged version present: discard parts pairs; use merged source.

   If source_registry_id points to B07 conflict analysis and margin is negative:
     â†’ Confirm unchosen alternative in the document has not been superseded.
       Apply three-check rule: stale unchosen alternative = Type 16 rejected only.

   If source_registry_id points to B11 README/summary:
     â†’ B-side authority from README is not permitted. B-side must use
       Type 2/5/26 source. Discard pair and rebuild with correct B-side authority.
```

Do not continue the round until the identified pattern is resolved.

---

## Stopping rules

**Stop a round early** when three consecutive pairs produce "rejected" decisions.
Review pair files before continuing.

**Stop the full session** when any of:
- Session ledger reaches 50 rows without any component reaching Trusted trust state.
  Review model size vs vocabulary size.
- Any single pair causes `InferenceElapsedMs` > 10 seconds.
  Model too large for current hardware at this batch size.
- `LoraAdapterBytes` > 2MB per adapter (decision D-05).
  Reduce `LoraConfig.Rank` from 4 to 2. Restart round from last promoted row.
  Do not use `BaseCheckpointBytes` for this check.
- `dpo_loss` stays at or above 0.693 across five consecutive rows.
  This means the model is at random-guess level â€” the pairs are not providing
  learning signal. Stop and run the round-level margin audit.

**Promote to Trusted** when five consecutive "promoted" decisions for the same
role and sub-phase. Register the current checkpoint as Trusted.

---

## File locations

```
Session ledger:  C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
Checkpoints:     C:\Projects\llm_mvp_core\checkpoints\teaching\
Pair files:      C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\
Internal registry: XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md
```

---

## Last updated: 2026-05-08. Version 5.0.

