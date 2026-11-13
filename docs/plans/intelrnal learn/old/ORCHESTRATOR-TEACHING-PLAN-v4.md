# Orchestrator Teaching Plan — One Row at a Time
# Version: 4.0 — 2026-05-08
# Supersedes: ORCHESTRATOR-TEACHING-PLAN-v3.md
# Authority: DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3.md
#             EXAMINATION-METHOD-ORCHESTRATION-v9.md
#             XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md
#
# v4 additions over v3:
# Q1: Pair quality gate added to Step 1 — token overlap test before encoding.
#     A pair that fails the test at Step 1 is discarded before any LoRA work.
# Q2: Pair selection priority from internal source registry added to round ordering.
#     Correction events from Source 2 export are highest-value pairs for live LoRA.
#     ARCH Type A from Source 1 rows 1-520 are the second priority.
# Q3: Verification confidence threshold added to Step 6 decision logic.
#     A pair that produces confidence_after < 0.50 on the verification prompt
#     is marked "held" regardless of verdict_correct — confidence too low to trust.
# Q4: Round-level margin tracking added. A round where fewer than 6 of 10 rows
#     achieve margin_improvement >= 0 triggers a pair quality audit before
#     continuing. This catches systematic pair quality problems early.
# Q5: Pair rejection diagnosis added to Step 6. When a row is rejected, the
#     record now includes overlap_ratio and annotation_quality for post-session
#     analysis.
# Q6: Loss threshold added to stopping rules. Loss staying at 0.693 across
#     five consecutive rows means the model is at random-guess level — stop
#     and audit pair quality before continuing.
# All v3 corrections (P1-1 through P2-8) preserved.

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
pair files — or pairs directly from the internal source registry — to teach the
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

## Pair quality gate — applies before any LoRA work

This gate runs at Step 1 before encoding. A pair that fails is discarded. No
LoRA training runs on a pair that fails the quality gate.

### Token overlap test

```
shared = tokens in both chosen and rejected that are NOT also in the prompt
total  = tokens in chosen + tokens in rejected − tokens in prompt
overlap_ratio = shared / total
```

**overlap_ratio > 0.60 → discard the pair. Do not proceed to Step 2.**

Why this gate is placed at Step 1, not at pair file creation: pairs may arrive
from multiple sources (batch DPO file, correction event, held-out set). The gate
ensures no pair reaches the LoRA training step with insufficient distributional
separation, regardless of where it originated.

Expected overlap ratios by pair type:

| Pair type | Expected overlap | Expected LoRA outcome |
|---|---|---|
| Correction event from Source 2 export | 0.10 – 0.25 | Positive margin_improvement |
| ARCH Type A (boundary vs implementation) | 0.10 – 0.20 | Positive margin_improvement |
| PLAN Type B with strong annotation | 0.35 – 0.50 | Positive margin_improvement |
| Type C (wrong domain) | 0.15 – 0.35 | Positive margin_improvement |
| Negation template | 0.65 – 0.80 | Negative margin_improvement |

### Annotation quality check

If `overlap_ratio` is between 0.40 and 0.60, the annotation must be strong
enough to carry the distributional signal. Check:

```
Annotation is strong if it:
  — names the specific sub-phase the rejected incorrectly addresses
  — explains why that sub-phase does not own this responsibility
  — states what the prompt actually asked for that rejected missed

Annotation is weak if it:
  — says only "this is incorrect" or "wrong approach"
  — names the correct sub-phase but does not explain the confusion
  — is absent entirely
```

A pair with `overlap_ratio` between 0.40 and 0.60 AND a weak annotation is
discarded. These pairs are borderline on overlap and rely on the annotation
to carry the training signal. Without a strong annotation, reward margin
will be near zero.

### Pair selection priority from internal source registry

When selecting which pairs to teach in a round, prefer in this order:

```
Priority 1 — Correction events from Source 2 export (overlap 0.10–0.25)
  Bundle order: legacy-docs-skills-from-old-code → figma-to-code → flow-01-station-testing

Priority 2 — ARCH Type A from Source 1 rows 1–520 (overlap 0.10–0.20)
  Rows contain ARCHITECTURE predicate vocabulary confirmed in calibration run.
  Reclassify as ARCH domain before teaching.

Priority 3 — PLAN Type B from Source 1 rows 521–727 (overlap 0.15–0.35)
  Confirmed positive margin in calibration batches 16 and 17.

Priority 4 — All other eligible pairs
```

Do not teach pairs from Source 1 rows 1235–1753 as PLAN or ARCH A sides.
These are the meta-commentary rows confirmed negative margin in batch 30
(acc=0.306). Correction events from this range are acceptable.

---

## Prerequisites — teaching harness before any row

Before the first row is taught, four wiring steps must be complete.

**Harness step 1 — learned-unit selector per taught role.**

The only factory that currently selects learned units is
`SecurityVerification34FlowFactory` for the node-08 arbiter role.
`PromptRoutingInputCompilerUnit`, `MultiSubPhaseRouterUnit`, and the generic
nine-node flow are constructed directly — they never consult the unit registry.
Before teaching a role, write one thin factory per taught role following the
same pattern as `SecurityVerification34FlowFactory`. Register it in the flow
registry for the sub-phase being taught.

**Harness step 2 — tracing wrappers for the verification run.**

`TracingFlowDecorator` and `TracingLogicalUnit` exist but
`AdaptiveLogicalOrchestrator.RunAsync` does not wrap them automatically.
Before the verification run in Step 5, register `TracingFlowDecorator`-wrapped
flows for the sub-phases being taught and collect `StationTrace` objects via
the `emitTrace` callback.

**Harness step 3 — LoRA adapter inclusion in the forward path.**

`LoraApplicator.Apply` must be called on the same `LlmModel` instance used by
`DpoTrainer`. Do not create a separate model copy between these two calls.
`LoraConfig.Scaling` is computed (Scaling = Alpha / Rank). Use Alpha = 2f with
Rank = 4 to achieve Scaling = 0.5. Never set Scaling directly — it has no setter.

**Harness step 4 — checkpoint field split.**

```
LoraAdapterBytes    — sum of (Rank * InDim + OutDim * Rank) * 4 bytes per adapter
                      computed from the adapter list, not from the checkpoint file
DpoMetricBytes      — sum of dpo_step_*.json file sizes in checkpointDir
BaseCheckpointBytes — new FileInfo(checkpointPath).Length
```

The stopping rule "LoRA adapters must be ≤ 2MB each" applies to `LoraAdapterBytes`,
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

Teaching starts with routing (compiler → domain classifier → router) because
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
    string DomainLabel,         // CODE_Q, BUG, PLAN, ARCH, REFACTOR — from D-04
    string TransitionType,
    int    RowIndex,
    string SourceRegistryId,    // which entry in XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md
    string SourceSessionRows,   // e.g. "521-727" for Source 1 positive zone

    // Pair quality fields — recorded before training
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

    // DPO step metrics — from dpo_step_*.json
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
    string RejectionDiagnosis   // populated only when Decision = "rejected"
);
```

All fields are written before moving to the next pair.

---

## The sub-cycle for one training pair

### Step 0 — Baseline snapshot

Before touching anything, run the current deterministic unit on the verification
prompt and record:

```
baseline_verdict        — actual verdict from the deterministic unit
baseline_confidence     — Confidence from LogicalUnitResult
baseline_inference_ms   — elapsed time for one inference pass
baseline_working_set_mb — working set during inference
```

The verification prompt must be an eligible prompt — not meta-commentary, not
a session-loading command. Prefer a prompt from the internal source priority list.

### Step 1 — Pair quality gate

Apply the token overlap test and annotation quality check (see Pair Quality Gate
section above). Record `overlap_ratio` and `annotation_quality` in the row record
whether the pair passes or fails.

**If the pair fails the gate:**
- Record the failure in the ledger with `decision: "discarded_quality_gate"`
- Log `overlap_ratio` and the specific failure reason
- Move to the next pair. Do not proceed to Step 2.

**If the pair passes:**
- Proceed to Step 2.

### Step 2 — Encode the pair

The A side becomes `Prompt` in `DpoPair`.
The B side becomes `Chosen`.
The rejected B (from the pair file, Type A / B / C) becomes `Rejected`.

Tokenize both `Chosen` and `Rejected` using `BpeTokenizer.Encode()`.

After tokenisation, verify the encoded token counts:
```
If len(chosen_tokens) < 5 OR len(rejected_tokens) < 5:
  discard pair — too short for meaningful DPO signal
```

### Step 3 — Apply LoRA and train

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
These are the DPO-step-level metrics — separate from the inference-level
`margin_improvement` computed in Step 6.

**If `dpo_reward_margin < 0` at Step 3:**
The pair produced negative reward margin in the DPO step itself. This means
the model already preferred rejected over chosen before the step, and the step
reinforced the wrong direction. Record this and proceed to Step 5b — the
verification run will confirm whether the unit degraded.

### Step 4 — Wrap trained model as ILogicalUnit

Register the trained unit at `Probationary` trust state. Apply slot supersession
before registration:

```csharp
// Step 4a — supersede prior entries to prevent stale Probationary units from
// re-activating after a rejected decision.
unitRegistry.Register<TInput, TOutput>(
    unitId:     $"unit-{role}-slot-superseded-row-{rowIndex:D4}",
    unitRole:   role,
    domainTag:  subPhaseId,
    version:    $"1.0.row{rowIndex}.superseded",
    trustState: LogicalUnitTrustState.Candidate,  // below Probationary minimum
    isLearned:  false,
    unit:       deterministicFallback);

// Step 4b — register the trained unit.
unitRegistry.Register<TInput, TOutput>(
    unitId:     $"unit-{role}-trained-row-{rowIndex:D4}",
    unitRole:   role,
    domainTag:  subPhaseId,
    version:    $"1.0.row{rowIndex}",
    trustState: LogicalUnitTrustState.Probationary,
    isLearned:  true,
    unit:       trainedUnit);
```

### Step 5 — Wrap verification run with tracing decorators

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

### Step 5b — Run the verification prompt

Run with `UseLearnedUnitWhenAvailable: true`. Measure inference resources.
Collect per-branch and per-node results.

The verification prompt must match the pair's A side — not a different prompt.
The expected verdict is the B side outcome the pair was teaching.

### Step 6 — Compute margin improvement and decide trust state

```
margin_improvement = confidence_after - confidence_before
dpo_reward_margin  = from Step 3 metrics (chosen_reward - rejected_reward)
```

**Decision logic:**

```
If actual_verdict == expected_verdict
   AND margin_improvement >= 0.0
   AND confidence_after >= 0.50:     → "promoted", Probationary

If actual_verdict == expected_verdict
   AND margin_improvement < 0.0:     → "held", Probationary
                                       (correct verdict but confidence regressed)

If actual_verdict == expected_verdict
   AND confidence_after < 0.50:      → "held", Probationary
                                       (correct verdict but confidence too low to trust)

If actual_verdict != expected_verdict:  → "rejected"
```

**When decision is "rejected" — record RejectionDiagnosis:**

```
Check dpo_reward_margin from Step 3:
  If dpo_reward_margin < 0:
    RejectionDiagnosis = "negative_dpo_margin: model preferred rejected over chosen
                          at training time — pair overlap_ratio was {overlap_ratio}"

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

### Step 7 — Write the teaching row record

Write all fields to the session ledger before moving to the next pair. The ledger
is a JSON-lines file — one record per line, never overwrite.

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
`annotation_quality`, `source_registry_id` are new required fields in v4.

---

## Round ordering

Work is ordered so routing is taught first. Within each round, pairs are
ordered by the internal source priority: correction events first, then ARCH
zone rows, then PLAN positive zone rows.

### Round 1 — Prompt routing input compiler (Layer 0-A)

Row selection order (priority from internal registry):
```
Rows 1–5:   Correction events from Source 2 export
             (legacy-docs-skills bundle, ARCH/CODE_Q corrections)
Row 6:      ARCH zone prompt — "Define the component boundaries for the payment service"
Row 7:      PLAN positive zone — "continue from where you stopped — Phase 1,
             Factory Interfaces and Engine Processing"
Row 8:      UI-only prompt — "Redesign the login screen layout"
Row 9:      Multi-predicate — "Architecture design and integration tests for the auth layer"
Row 10:     Stage 9 corruption — wrong predicate assigned
```

After row 10: pause and review. If fewer than 7 of 10 decisions are "promoted"
or "held" — or if `dpo_loss` has not moved below 0.680 across rows 1–10 —
run the round-level margin audit before proceeding.

### Round 1b — Domain classifier (Layer 0-B)

One domain classifier row per prompt from Round 1. A side: compiled prompt +
predicates. B side: domain label (CODE_Q / BUG / PLAN / ARCH / REFACTOR).

Rejected (Type C): wrong domain label for the prompt's intent verb. Use the
intent verb → domain table from the examination method.

### Round 2 — Multi-sub-phase router (Layer 0-C)

Router A sides come from compiler B outputs recorded in Round 1. Do not invent
A shapes. Row ordering follows the same source priority — correction events first.

Rows 1–5: single and dual sub-phase routing from correction event prompts.
Rows 6–9: architecture-only, PLAN multi-phase, no-predicate, unregistered.
Row 10: Stage 9 ambiguous-predicate-case corruption.

### Round 3 — Domain splitter (Node 01)

A side: selected A/B/X transition from domain classifier B side.
Row ordering: one-packet rows first, then multi-packet, then knowledge-edge.

### Round 4 — Domain translator (Node 02)

One packet at a time. Correction-event-derived security packets first (highest
overlap separation), then architecture, API, testing.

### Round 5 — Scheme planner (Node 03)

Primary PLAN training signal. A side: full set of translated statements.
B side: ordered work branches with `depends_on` fields.

Every multi-task pair must be Type B rejected (wrong dependency order). Use
the dependency ordering table:

| Correct | Rejected | Why wrong order fails |
|---|---|---|
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mocks | Mock tokens bypass validation — cannot test real isolation |
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, 3.1 later | Implementing without decided boundaries creates coupling to undo |
| 3.2 Data Model → 3.3 API Contract | 3.3 first, model from contract | Contract shape depends on model shape — leaks implementation into public API |

For Round 5, the confidence threshold in Step 6 applies strictly:
`confidence_after < 0.50` → "held", not "promoted", even with correct verdict.
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
     → Pair vocabulary too similar. Replace rejected with more distant sub-phase.

   If rejected rows have overlap_ratio < 0.50 but annotation_quality = "weak":
     → Annotation not providing enough distributional signal for Type B pairs.
       Strengthen annotations before continuing.

   If overlap and annotation are fine but verdict_correct = false:
     → Chosen content inaccurate. Review the B side of failing pairs.

   If dpo_reward_margin < 0 across the round but overlap_ratio < 0.40:
     → Model size may be insufficient for the vocabulary size.
       Consider reducing LoraConfig.Rank from 4 to 2 and restarting the round.
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
  This means the model is at random-guess level — the pairs are not providing
  learning signal. Stop and run the round-level margin audit.

**Promote to Trusted** when five consecutive "promoted" decisions for the same
role and sub-phase. Register the current checkpoint as Trusted.

---

## File locations

```
Session ledger:  C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
Checkpoints:     C:\Projects\llm_mvp_core\checkpoints\teaching\
Pair files:      C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\
Internal registry: XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md
```

---

## Last updated: 2026-05-08
