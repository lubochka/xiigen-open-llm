# Orchestrator Teaching Plan — One Row at a Time
# Version: 2.0 — 2026-05-05
# Replaces: ORCHESTRATOR-TEACHING-PLAN-v1.md
# Authority: DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md
#
# v2 corrections (all from code review against actual repo):
# P1-1: LoraConfig.Scaling is read-only; use Alpha = 2f with Rank = 4.
# P1-2: Rejected re-registration does not disable prior Probationary versions;
#        use Candidate sentinel supersession at start of each new row.
# P1-3: LoRA/DPO adapter path confirmed real; adapter tensors are in model graph
#        via RegisterParameter; AdamW skips frozen base weights via Grad == null.
#        SaveCheckpoint includes adapter weights. Clarified in plan text.
# P1-4: Learned-unit selection for compiler/router/node-01..03 requires per-role
#        factories; added prerequisite harness section before the sub-cycle.
# P2-5: Node traces require explicit TracingFlowDecorator wrapping; added Step 5
#        harness sub-step before the verification run.
# P2-6: ModelArtifactBytes split into BaseCheckpointBytes, LoraAdapterBytes,
#        DpoMetricBytes; stopping rule corrected to use LoraAdapterBytes.
# P2-7: CPU percent formula now divides by Environment.ProcessorCount to match
#        StationResourceSampler and stay within 0-100% on multi-core machines.
# P2-8: WorkingSetMb renamed to WorkingSetDeltaMb; noted it is a before/after
#        delta, not a sampled peak.

---

## What this plan is

This plan teaches the adaptive orchestrator one training pair at a time, measuring
the effect of each pair before adding the next. After each pair the system runs a
live prompt through every flow node, captures CPU, memory, disk, and elapsed time,
checks the verdict, and records whether the pair improved the machine or made it
worse. The next pair is only added after the measurement for the current pair is
recorded.

This is not batch training. Batch training teaches many pairs at once and produces
one accuracy number. This plan teaches one pair, tests, measures, decides, then
moves to the next. It is slower but every step is observable and reversible.

---

## Why one row at a time

A learned unit registered in `InMemoryLogicalUnitRegistry` at `Probationary`
trust state is selected by `SecurityVerification34FlowFactory` only when
`UseLearnedUnitWhenAvailable` is true. If the measurement shows the pair made
the unit worse, it is re-registered at `Rejected` trust state and the
deterministic unit retakes the slot. The next pair is a fresh attempt.

This means the machine never silently degrades. Every change is measured before
the next change is applied.

---

## Prerequisites — teaching harness before any row

Before the first row is taught, four wiring steps must be complete. Without them
the trained units cannot become live, the verification run has no per-node traces,
and the measurement fields will be wrong.

**Harness step 1 — learned-unit selector per taught role.**
The only factory that currently selects learned units is `SecurityVerification34FlowFactory`
for the node-08 arbiter role. `PromptRoutingInputCompilerUnit`, `MultiSubPhaseRouterUnit`,
and the generic nine-node flow are constructed directly — they never consult the
unit registry. Before teaching a role, that role needs a factory or selector that
calls `InMemoryLogicalUnitRegistry.ResolveLatest` for that role. Write one thin
factory per taught role following the same pattern as `SecurityVerification34FlowFactory`.
Register it in the flow registry for the sub-phase being taught. Without this,
a registered `Probationary` unit will never be called by the verification run.

**Harness step 2 — tracing wrappers for the verification run.**
`TracingFlowDecorator` and `TracingLogicalUnit` exist but `AdaptiveLogicalOrchestrator.RunAsync`
does not wrap flows or units with them automatically. The `node_traces` field in
`TeachingRowRecord` will be empty unless the verification run uses a wrapped
orchestrator. Before the verification run in Step 5, register
`TracingFlowDecorator`-wrapped flows for the sub-phases being taught and collect
`StationTrace` objects via the `emitTrace` callback into a local list. These become
the `node_traces` array in the record.

**Harness step 3 — LoRA adapter inclusion in the forward path.**
`LoraApplicator.Apply` attaches `LoraLinear` wrappers inside the transformer's
attention blocks and freezes base weights. The model checkpoint includes adapter
weights because `LoraLinear` registers them via `RegisterParameter`. The DPO step
updates only adapter tensors because AdamW skips parameters with `Grad == null`
(frozen base weights). This chain works as long as `LoraApplicator.Apply` is
called on the same `LlmModel` instance used by `DpoTrainer`. Do not create a
separate model copy between these two calls.

**Harness step 4 — checkpoint field split.**
`model.SaveCheckpoint` writes the full model state including adapter weights into
one binary file. The plan's `ModelArtifactBytes` field measures this combined
file. The separate `LoraAdapterBytes` and `DpoMetricBytes` fields should be
measured as follows:

```text
LoraAdapterBytes  — sum of (Rank * InDim + OutDim * Rank) * 4 bytes per LoraLinear adapter
                    (compute from the returned adapter list without a separate file write)
DpoMetricBytes    — sum of File.GetFileSystemEntries(checkpointDir, "dpo_step_*.json")
                    lengths after the DPO step
BaseCheckpointBytes — new FileInfo(checkpointPath).Length
```

Do not substitute `BaseCheckpointBytes` for `LoraAdapterBytes`. The stopping rule
"LoRA adapters must be ≤ 2MB each" applies to the computed `LoraAdapterBytes`
value, not the full checkpoint.

---

## The five components being taught

Each component is an `ILogicalUnit` with a defined input and output type. Teaching
each one is a separate sub-cycle that follows the same pattern.

| Component | Contract | Role in the flow |
|---|---|---|
| Prompt routing input compiler | `ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>` | Layer 0 — converts raw prompt to router input |
| Multi-sub-phase router | `ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>` | Layer 0 — identifies all sub-phases a prompt touches |
| Domain splitter | `ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>` | Node 01 — splits selected transition into domain packets |
| Domain translator | `ILogicalUnit<DomainPacket, TranslatedDomainStatement>` | Node 02 — translates one domain packet into formal vocabulary |
| Scheme planner | `ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>` | Node 03 — plans ordered work branches with dependencies |

The nine-node security flow (sub-phase 3.4) already has deterministic units for
all nine nodes. Teaching starts with the routing layer (compiler and router) because
these gate every other node. If routing is wrong, no downstream teaching matters.

---

## The measurement record

Every pair produces one `TeachingRowRecord` before the next pair begins.

```csharp
public sealed record TeachingRowRecord(
    // Identity
    string PairId,
    string ComponentRole,
    string SubPhaseId,
    string TransitionType,
    int RowIndex,

    // Training resource consumption
    double TrainingElapsedMs,
    double TrainingCpuPercent,           // (cpuDelta / elapsed / ProcessorCount) * 100
    double TrainingWorkingSetDeltaMb,    // before/after delta, not a sampled peak
    double TrainingManagedAllocationMb,
    double TrainingDiskReadBytes,
    double TrainingDiskWriteBytes,
    long   BaseCheckpointBytes,          // full model checkpoint including adapter weights
    long   LoraAdapterBytes,             // computed from adapter list: sum(loraA + loraB) * 4
    long   DpoMetricBytes,               // sum of dpo_step_*.json file sizes

    // Inference resource consumption (one live run after training)
    double InferenceElapsedMs,
    double InferenceCpuPercent,           // (cpuDelta / elapsed / ProcessorCount) * 100
    double InferenceWorkingSetDeltaMb,    // before/after delta, not a sampled peak

    // Result verification
    string PromptUsedForVerification,
    string ExpectedVerdict,
    string ActualVerdict,
    bool   VerdictCorrect,
    double ConfidenceBefore,
    double ConfidenceAfter,
    double MarginImprovement,     // ConfidenceAfter - ConfidenceBefore; negative = regression

    // Decision
    LogicalUnitTrustState TrustStateAssigned,  // Probationary, Trusted, or Rejected
    string Decision,              // "promoted", "rejected", "held"
    string DecisionReason
);
```

All fields are written before moving to the next pair. An incomplete record means
the session did not finish the pair.

---

## Measurement tools used

**Elapsed time:** `System.Diagnostics.Stopwatch` — already used in `ResourceMonitor`
(project 34) and `StationResourceSampler` (LlmInspector.Core). Use `Stopwatch.StartNew()`
before the training call, `stopwatch.Stop()` after. `stopwatch.Elapsed.TotalMilliseconds`.

**CPU:** `Process.GetCurrentProcess().TotalProcessorTime` — same approach as
`StationResourceSampler.Capture()`. Capture before and after, compute delta
divided by elapsed wall time.

**Working set (RAM):** `Process.GetCurrentProcess().WorkingSet64` before and after.
Report the peak delta in MB.

**Managed allocation:** `GC.GetAllocatedBytesForCurrentThread()` before and after,
same as `ResourceMonitor.Start()` / `Stop()`. Reports bytes allocated during the
training call on this thread.

**Disk read/write:** `Process.GetCurrentProcess().TotalProcessorTime` is not disk.
For disk, use `new DriveInfo(checkpointDir[0].ToString())` to read
`AvailableFreeSpace` before and after training. The difference in available space
approximates bytes written. For read, the `DpoDataset.Load()` call reads the pair
file — measure the file size in bytes as `disk_read_bytes`.

**Model artifact size:** `new FileInfo(checkpointPath).Length` after the checkpoint
is written. This is `model_artifact_bytes`.

---

## The sub-cycle for one training pair

This sub-cycle runs once per pair, for every component being taught. The steps
are the same regardless of which component is being taught.

### Step 0 — Baseline snapshot

Before touching anything, run the current deterministic unit on the verification
prompt and record:

```text
baseline_verdict        — actual verdict from the deterministic unit
baseline_confidence     — Confidence field from LogicalUnitResult
baseline_inference_ms   — elapsed time for one inference pass
baseline_working_set_mb — working set during inference
```

### Step 1 — Encode the pair

The A side of the pair becomes the `Prompt` field of a `DpoPair`.
The B side of the pair becomes the `Chosen` field.
A deliberately wrong B (taken from the failure-mode examples in the examination
method) becomes the `Rejected` field.

Tokenize both `Chosen` and `Rejected` using `BpeTokenizer.Encode()`.

### Step 2 — Apply LoRA and freeze base weights

```csharp
// LoraConfig.Scaling is computed: Scaling = Alpha / Rank.
// Set Alpha = 2 with Rank = 4 to achieve Scaling = 0.5.
// Never set Scaling directly — it has no setter.
var adapters = LoraApplicator.Apply(model, new LoraConfig
{
    Rank          = 4,
    Alpha         = 2f,      // Scaling = Alpha / Rank = 2 / 4 = 0.5
    Dropout       = 0.0f,
    TargetModules = ["q_proj", "v_proj"]
});
```

`LoraApplicator.Apply` freezes all base weight tensors by setting
`RequiresGrad = false` on each base `Linear` parameter. The returned adapter
list holds the `LoraLinear` wrappers whose `lora_a` and `lora_b` parameters
have `RequiresGrad = true` by default.

**How DPO updates the adapters:** `DpoTrainer.Step` calls
`_optimizer.Step(_policy.Parameters(), lr)`. `LlmModel.Parameters()` recurses
through all submodules including the `LoraLinear` adapters registered inside
each `TransformerBlock.Attention`. Because `LoraLinear` registers `lora_a` and
`lora_b` via `RegisterParameter` and the base weights have `RequiresGrad = false`
(so `Grad == null`), AdamW skips the frozen base weights and updates only the
adapter tensors. The LoRA update path is real as long as the teaching call goes
through `DpoTrainer.Step` on the same model instance that `LoraApplicator.Apply`
was called on.

**What is not saved by `model.SaveCheckpoint`:** `LlmModel.SaveWeights` calls
`Parameters()` and writes every parameter whose slot is in the model graph. After
`LoraApplicator.Apply` the adapter parameters live inside the model graph via
`RegisterParameter`, so they are included in the checkpoint. The checkpoint
therefore contains both frozen base weights and updated adapter weights in one
file. There is no separate LoRA adapter file at this stage.

### Step 3 — Train on the one pair

Start measurements:

```csharp
var diskBefore   = new DriveInfo(checkpointDir[0].ToString()).AvailableFreeSpace;
var memBefore    = GC.GetAllocatedBytesForCurrentThread();
var wsBefore     = Process.GetCurrentProcess().WorkingSet64;
var cpuBefore    = Process.GetCurrentProcess().TotalProcessorTime;
var sw           = Stopwatch.StartNew();
```

Run one DPO step:

```csharp
var trainer = new DpoTrainer(policyModel, referenceModel, checkpointDir);
var metrics = trainer.Step(batch, step: rowIndex);
```

Stop measurements:

```csharp
sw.Stop();
var diskAfter  = new DriveInfo(checkpointDir[0].ToString()).AvailableFreeSpace;
var memAfter   = GC.GetAllocatedBytesForCurrentThread();
var wsAfter    = Process.GetCurrentProcess().WorkingSet64;
var cpuAfter   = Process.GetCurrentProcess().TotalProcessorTime;

training_elapsed_ms             = sw.Elapsed.TotalMilliseconds
training_cpu_percent            = (cpuAfter - cpuBefore).TotalMilliseconds
                                  / sw.Elapsed.TotalMilliseconds
                                  / Math.Max(1, Environment.ProcessorCount) * 100.0
                                  // matches StationResourceSampler; stays 0-100% on multi-core
training_working_set_delta_mb   = Math.Max(0, wsAfter - wsBefore) / 1_048_576.0
                                  // delta not peak — see P2-8 note
training_managed_allocation_mb  = Math.Max(0, memAfter - memBefore) / 1_048_576.0
training_disk_read_bytes        = pairFileInfo.Length
training_disk_write_bytes       = Math.Max(0, diskBefore - diskAfter)
base_checkpoint_bytes           = new FileInfo(checkpointPath).Length
lora_adapter_bytes              = adapters.Sum(a => (long)(a.LoraA.Value.Data.Length
                                  + a.LoraB.Value.Data.Length) * sizeof(float))
dpo_metric_bytes                = Directory.GetFiles(checkpointDir, "dpo_step_*.json")
                                  .Sum(f => new FileInfo(f).Length)
```

Save checkpoint:

```csharp
model.SaveCheckpoint(checkpointPath);
model_artifact_bytes = new FileInfo(checkpointPath).Length;
```

### Step 4 — Wrap the trained model as an ILogicalUnit

The trained LoRA model produces a unit that replaces the deterministic fallback
in the flow factory. The wrapper implements `ILogicalUnit<TInput, TOutput>` for
the component being taught. Its `ExecuteAsync` method runs:

1. Embed the input using `EmbeddingModel.Embed(input.ToString())`
2. Score candidates using cosine similarity over the HNSW index
3. Return the highest-scoring candidate as the output, with confidence from the score

Register the trained unit in `InMemoryLogicalUnitRegistry` at `Probationary`
trust state:

```csharp
unitRegistry.Register<TInput, TOutput>(
    unitId:      $"unit-{role}-trained-row-{rowIndex:D4}",
    unitRole:    role,
    domainTag:   subPhaseId,
    version:     $"1.0.row{rowIndex}",
    trustState:  LogicalUnitTrustState.Probationary,
    isLearned:   true,
    unit:        trainedUnit);
```

**Slot supersession rule.** `InMemoryLogicalUnitRegistry` is append-only.
`ResolveLatest` finds the newest non-Rejected entry for a role+domain pair.
Registering a bad unit as `Rejected` does **not** disable earlier `Probationary`
entries for the same role — `ResolveLatest` will select the previous probationary
version because it is still the newest non-Rejected entry.

To prevent this, use a dedicated supersession registration at the start of every
new row for the same role:

```csharp
// Step 4a — supersede all prior entries for this role+domain before training.
// Register a sentinel Candidate entry with RegistrationOrder higher than all
// prior Probationary entries. ResolveLatest will find this sentinel first and
// fail because Candidate < Probationary (minimum trust), falling back to
// deterministic. This ensures a bad row does not let a prior row stay active.
unitRegistry.Register<TInput, TOutput>(
    unitId:      $"unit-{role}-slot-superseded-row-{rowIndex:D4}",
    unitRole:    role,
    domainTag:   subPhaseId,
    version:     $"1.0.row{rowIndex}.superseded",
    trustState:  LogicalUnitTrustState.Candidate,   // below Probationary minimum
    isLearned:   false,
    unit:        deterministicFallback);             // safe fallback object required
```

After Step 6 evaluates the new trained unit:

- If decision is "promoted" or "held": register the new trained unit at
  `Probationary`. `ResolveLatest` now finds it as the newest non-Rejected,
  non-Candidate entry.
- If decision is "rejected": do not register at `Rejected`. The supersession
  sentinel above already blocks the previous probationary version. The slot
  stays at deterministic fallback until the next row succeeds.

### Step 5 — Wrap the verification run with tracing decorators

`AdaptiveLogicalOrchestrator.RunAsync` does not automatically wrap flows or units
with `TracingFlowDecorator` or `TracingLogicalUnit`. Without this step the
`node_traces` field will be empty.

Before the verification run, build a trace-collecting harness:

```csharp
// Collect traces from all stations in the verification run.
var collectedTraces = new List<StationTrace>();
Action<StationTrace> emitTrace = trace => collectedTraces.Add(trace);

// Wrap the flow registered for this sub-phase with a tracing decorator.
// Use the same FlowId that the orchestrator will select via ResolveBindingBySubPhase.
var innerBinding = flows.ResolveBindingBySubPhase(subPhaseId, factoryRequest);
if (innerBinding.IsSuccess)
{
    var tracingFlow = new TracingFlowDecorator<string>(
        innerBinding.Value!.Flow,
        $"verify-{subPhaseId}-row-{rowIndex:D4}",
        emitTrace);
    // Re-register the tracing-wrapped flow for the verification run only.
    // Use a versioned id that does not overwrite the production registration.
    flows.Register<string>(
        innerBinding.Value.FlowId + "-tracing",
        innerBinding.Value.ScenarioId,
        "tracing",
        subPhaseId, phaseId, taskType, domainTags,
        tracingFlow);
}
```

After the verification run completes, collect `collectedTraces` into the
`NodeTraces` list for the row record. Then de-register or discard the tracing
registration before the next row begins.

### Step 5b — Run the verification prompt through the full flow

Start inference measurements:

```csharp
var wsInfBefore  = Process.GetCurrentProcess().WorkingSet64;
var cpuInfBefore = Process.GetCurrentProcess().TotalProcessorTime;
var swInf        = Stopwatch.StartNew();
```

Run the adaptive orchestrator with `UseLearnedUnitWhenAvailable: true`:

```csharp
var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
    runId:       $"teach-verify-row-{rowIndex:D4}",
    scenarioId:  AdaptiveLogicalOrchestrator.ScenarioId,
    userRequest: verificationPrompt,
    flags:       new LogicalExecutionFlags { UseLearnedUnitWhenAvailable = true },
    metadata:    new Dictionary<string, object>()));
```

Stop inference measurements:

```csharp
swInf.Stop();
var wsInfAfter  = Process.GetCurrentProcess().WorkingSet64;
var cpuInfAfter = Process.GetCurrentProcess().TotalProcessorTime;

inference_elapsed_ms          = swInf.Elapsed.TotalMilliseconds
inference_cpu_percent         = (cpuInfAfter - cpuInfBefore).TotalMilliseconds
                                / swInf.Elapsed.TotalMilliseconds
                                / Math.Max(1, Environment.ProcessorCount) * 100.0
                                // matches StationResourceSampler
inference_working_set_delta_mb = Math.Max(0, wsInfAfter - wsInfBefore) / 1_048_576.0
                                // delta not peak — see P2-8 note
```

Collect result per branch and per node:

```text
For each branch in result.Value.BranchResults:
  branch_sub_phase_id    = branch.ScenarioId or Metrics["missing_sub_phase"]
  branch_overall_verdict = branch.OverallVerdict
  branch_knowledge_edge  = branch.Metrics["knowledge_edge"] == "True"

For the primary branch (sub-phase being taught):
  actual_verdict         = primary branch OverallVerdict
  actual_confidence      = unit result Confidence from the trained unit
```

### Step 6 — Compute margin improvement and decide trust state

```text
margin_improvement = confidence_after - confidence_before

If actual_verdict == expected_verdict AND margin_improvement >= 0.0:
    decision          = "promoted"
    trust_state       = Probationary   (stays probationary — promotion to Trusted requires N consecutive correct rows)

If actual_verdict == expected_verdict AND margin_improvement < 0.0:
    decision          = "held"
    trust_state       = Probationary   (correct answer but confidence regressed — hold and observe)

If actual_verdict != expected_verdict:
    decision          = "rejected"
    trust_state       = Rejected       (re-register the trained unit as Rejected so the factory falls back to deterministic)
```

Promotion from `Probationary` to `Trusted` happens after five consecutive
`"promoted"` decisions for the same role and sub-phase. Until then the deterministic
unit is always the safe fallback.

### Step 7 — Write the teaching row record

Write all fields of `TeachingRowRecord` to the session ledger file before moving
to the next pair. The ledger is a JSON-lines file:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
  teach-{component-role}-{subphase}-{date}.jsonl
```

One line per pair. Never overwrite a previous line. If the session ends before
writing the record, the pair is treated as incomplete and replayed from Step 0
in the next session.

---

## The order of pairs across all components

Work is ordered so routing is taught first and execution nodes are taught after.
Within each component, pairs are ordered so clean single-sub-phase prompts come
before multi-sub-phase and corruption cases.

### Round 1 — Prompt routing input compiler (Layer 0-A)

Teach `PromptRoutingInputCompilerUnit`. The verification prompt for each row is
the same prompt the pair was derived from. The expected output is the
`SubPhaseRouterSourceDocument` B side from the examination method v7 pair.

Order within round 1:

```text
Row 1:  O1 prompt — multi-predicate (JWT + API + tenant isolation tested)
Row 2:  O1 prompt rephrase 1 — "Add multi-tenant auth to the API with isolation tests"
Row 3:  O1 prompt rephrase 2 — "JWT for tenants, API isolation, run the test suite"
Row 4:  No-predicate prompt — "The 403 is wrong"
Row 5:  No-predicate prompt rephrase — "Admin endpoint returning wrong status"
Row 6:  UI-only prompt — "Redesign the login screen layout"
Row 7:  QA-only prompt — "We need to do a release review before shipping"
Row 8:  Architecture-only prompt — "Define the component boundaries for the payment service"
Row 9:  Multi-predicate mixed — "Architecture design and integration tests for the auth layer"
Row 10: Corruption — wrong predicate assigned (SECURITY for a UI prompt)
```

After row 10: pause, review the ledger. If fewer than 7 of 10 decisions are
"promoted" or "held", add five more pairs from the held-out set before proceeding
to round 2.

### Round 2 — Multi-sub-phase router (Layer 0-B)

Teach `MultiSubPhaseRouterUnit`. Router A sides come from the compiler B outputs
recorded in round 1. Do not invent A shapes.

Order within round 2:

```text
Row 1:  Single sub-phase — SECURITY predicates → 3.4 only
Row 2:  Single sub-phase — TEST predicates → 5.2 only
Row 3:  Two sub-phases — SECURITY + TEST → 3.4 and 5.2
Row 4:  Two sub-phases — ARCHITECTURE + API_CONTRACT → 3.1 and 3.3
Row 5:  Three sub-phases — O1 full → 3.4, 3.1, 5.2
Row 6:  No-predicate — empty CompiledPredicateIds → KnowledgeEdge: true
Row 7:  Unregistered sub-phase — routes to 3.3 which has no flow
Row 8:  Rephrasing of row 5 with different vocabulary
Row 9:  Rephrasing of row 3 with different vocabulary
Row 10: Corruption — both sub-phases same NearestWrongSubPhaseScore as Confidence
```

Pause after row 10, review. Same threshold: 7 of 10 promoted or held.

### Round 3 — Domain splitter (Node 01)

Teach `DomainSplitterUnit`. The A side is the selected A/B/X transition from
the transition orchestrator B side. Rows are ordered by number of domain packets:
one-packet rows first, then multi-packet rows, then boundary and knowledge-edge
packet rows.

### Round 4 — Domain translator (Node 02)

Teach `DomainTranslatorUnit`. One packet at a time. Security packets first
(ground truth from Wave 1 corpus), then architecture, API, testing packets.

### Round 5 — Scheme planner (Node 03)

Teach `SchemePlannerUnit`. Full translated statement sets as A. Work branch
arrays with dependency ordering as B. Single-sub-phase plans first, then
multi-sub-phase plans with explicit `depends_on` fields.

---

## What is measured per flow and per node

After every pair in every round, the verification run passes through the full
adaptive orchestrator. The inspector (`TracingFlowDecorator`, `TracingLogicalUnit`)
captures `StationTrace` records for every node that executes. These are recorded
in addition to the teaching row record.

For each node execution in the verification run, record:

```text
node_role              — "prompt-routing-input-compiler", "multi-sub-phase-router",
                         "domain-splitter", "domain-translator", "scheme-planner",
                         "domain-processor", "applier", "result-translator",
                         "arbiter-selector", "arbiter-unit", "formatter"
node_input_token_count — approximate token count of the input object
node_output_token_count — approximate token count of the output object
node_elapsed_ms        — from StationTrace.StartedAt to StationTrace.CompletedAt
node_cpu_percent       — from StationResourceSnapshot.ProcessCpuPercent
node_memory_mb         — from StationResourceSnapshot.ProcessMemoryMb
node_verdict           — the unit's returned Confidence and OverallVerdict
node_knowledge_edge    — whether this node produced a knowledge-edge result
node_used_learned_unit — whether a learned unit was selected or the deterministic fallback
```

This makes the effect of each pair visible at every level — not just the unit
being trained but all downstream nodes whose inputs changed as a result.

---

## The session ledger format

Each row written to the `.jsonl` file:

```json
{
  "pair_id": "O1-compiler-row-001",
  "component_role": "prompt-routing-input-compiler",
  "sub_phase_id": "whole_development_area",
  "transition_type": "feature_addition",
  "row_index": 1,
  "training": {
    "elapsed_ms": 412.3,
    "cpu_percent": 34.1,             // (cpuDelta / elapsed / ProcessorCount) * 100
    "working_set_delta_mb": 14.2,    // delta not peak
    "managed_allocation_mb": 3.1,
    "disk_read_bytes": 2048,
    "disk_write_bytes": 5242880,
    "base_checkpoint_bytes": 5242880,
    "lora_adapter_bytes": 49152,     // Rank=4: (4*16 + 32*4) * 4 bytes per adapter
    "dpo_metric_bytes": 1024
  },
  "inference": {
    "elapsed_ms": 38.7,
    "cpu_percent": 16.2,             // (cpuDelta / elapsed / ProcessorCount) * 100
    "working_set_delta_mb": 6.8     // delta not peak
  },
  "verification": {
    "prompt": "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested.",
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
    "reason": "Verdict correct. Confidence improved by 0.17."
  },
  "node_traces": [
    {
      "node_role": "prompt-routing-input-compiler",
      "elapsed_ms": 2.1,
      "cpu_percent": 38.0,
      "memory_mb": 1.2,
      "verdict": "0.92",
      "used_learned_unit": true
    },
    {
      "node_role": "multi-sub-phase-router",
      "elapsed_ms": 4.3,
      "cpu_percent": 41.0,
      "memory_mb": 1.8,
      "verdict": "0.91",
      "used_learned_unit": false
    }
  ]
}
```

---

## Stopping rules

**Stop a round early** when three consecutive pairs in that round produce
`"rejected"` decisions. The training data for that component has a systematic
problem — review the pair files before continuing.

**Stop the full session** when:
- The session ledger reaches 50 rows without any component reaching
  `Trusted` trust state. Pause and review whether the model is large
  enough for the vocabulary size.
- Any single pair causes `InferenceElapsedMs` to exceed 10 seconds.
  The model is too large for the current hardware at this batch size.
- `LoraAdapterBytes` exceeds 2MB per adapter (decision D-05). This is computed
  from the returned adapter list as `sum(loraA.Data.Length + loraB.Data.Length) * 4`.
  Reduce `LoraConfig.Rank` from 4 to 2 and restart the round from the last promoted
  row. Do not use `BaseCheckpointBytes` for this check — the checkpoint includes
  frozen base weights and will always be much larger than the adapter budget.

**Promote to Trusted** when five consecutive rows for the same role and
sub-phase all produce `"promoted"` decisions. Register the current checkpoint
as the `Trusted` version in `InMemoryLogicalUnitRegistry`. The flow factory
will now use this unit without the `Probationary` fallback qualifier.

---

## File locations for this plan

Session ledger: `C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\`
Checkpoints: `C:\Projects\llm_mvp_core\checkpoints\teaching\`
Pair files: `C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\`
This plan: `ORCHESTRATOR-TEACHING-PLAN-v1.md`

---

# Last updated: 2026-05-05
