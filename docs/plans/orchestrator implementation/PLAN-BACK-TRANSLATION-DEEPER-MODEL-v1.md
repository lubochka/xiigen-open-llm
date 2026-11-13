# Plan: Result Back-Translation and Deeper Model Config
# Version: 1
# Date: 2026-05-07
# Addresses: KNN output is a machine routing decision, not a developer-facing answer;
#            model depth insufficient for composing predicate patterns
# Depends on: PLAN-DPC-INTEGRATION-v1 completed first
# Floor before: 1257  (after DPC integration plan)
# Floor after:  1265

---

## The two problems this plan fixes

### Problem 1 — the KNN output is never translated back to human language

After `GeneratedCapabilityFlow` calls the KNN unit, `unitResult.Value.Output` is the
`ChosenText` string stored in the KNN example. That string looks like:

```
Generated capability for sub-phase 3.4: Security and Authorization Model.
Execute the requested work through task type security-and-authorization-model.
```

This is a machine routing decision. It tells the orchestrator which sub-phase was
selected, but it tells the developer nothing useful about what the system will actually
do for their request. The nine-node flow produces `"formatted human result for
{TaskType}"` — also weak, but at least generic. The generated capability flow produces
even less: a routing label with no connection to the developer's original question.

The correct result should combine three pieces:
the developer's original prompt, the selected sub-phase's purpose text from the
topology, and the sub-phase's predicate ids. Together these produce a response like:

```
For your request: "fix the auth check in the tenant service"
→ Sub-phase 3.4 (Security and Authorization Model)
  Scope: Owns security model, authentication, authorization, token, OAuth,
         OIDC, ASVS, session, and tenant-isolation routing.
  Focus areas: SECURITY, AUTHORIZATION
```

This is what back-translation means: take the machine output (sub-phase id + KNN
chosen text) and render it as a developer-readable response using the topology
metadata that is already in memory.

The `TemplateBeautifier` in project 29 maps predicate sequences to code snippets. It
is not the right tool here — it handles structured predicate chains like
`VALIDATE_OP(X) → SUCCESS`, not capability routing decisions. The back-translation
needed here is a new `CapabilityResultTranslator` that takes the sub-phase metadata
and the original prompt and composes a structured human response.

### Problem 2 — the model has one layer and hidden dimension 16

The current tiny config produces 18,736 total parameters with 256 LoRA-trainable
parameters. One transformer layer cannot compose patterns — it applies a single
attention pass and one feed-forward step. The DPO accuracy plateau at 0.23–0.49
and loss stuck at 0.694 (the initialization value) reflects a model that does not
have enough capacity to learn the predicate-to-sub-phase mapping reliably in one
optimizer step.

Moving to 4 layers and hidden dimension 32 gives 69,920 total parameters with 2,048
LoRA-trainable parameters. The model can now compose patterns across four rounds of
attention — enough to learn that IMPLEMENT + BUG + stack:typescript maps to sub-phase
4.1 even when the developer's surface vocabulary varies. The model stays small enough
to run on a developer machine (273 KB on disk per checkpoint).

The timing cost is real. Four layers at hidden 32 is roughly 4–6x slower per DPO
step than one layer at hidden 16, based on the compute ratio of the attention and
feed-forward operations. The full 200-row batch-20 run would take 4–6 hours instead
of 1 hour on the same hardware. This is acceptable because the calibration runs are
not latency-sensitive — they run as background jobs.

---

## Phase 1 — New CapabilityResultTranslator in project 21

### File created

```
21.AgentInfrastructure/Logical/CapabilityResultTranslator.cs
```

### What it does

`CapabilityResultTranslator` is a static class with one public method that takes the
KNN output string, the sub-phase metadata from the topology, and the original
developer prompt, and returns a developer-readable string.

```csharp
// This translator converts a machine capability routing decision into a developer-
// readable response that names what the system will do and why.
public static class CapabilityResultTranslator
{
    // Translate a KNN capability output into a human-readable developer response.
    // knnOutput:    the ChosenText string returned by the KNN unit
    // subPhase:     the resolved LogicalSubPhase from the topology registry
    // originalPrompt: the raw developer prompt from the original request
    public static string Translate(
        string knnOutput,
        LogicalSubPhase subPhase,
        string originalPrompt)
    {
        // Build a structured response from topology metadata and the original prompt.
        // The knnOutput is used as a fallback signal if sub-phase metadata is absent.
        var promptLine = string.IsNullOrWhiteSpace(originalPrompt)
            ? string.Empty
            : $"For your request: \"{originalPrompt.Trim()}\"";

        var subPhaseLine = $"→ Sub-phase {subPhase.SubPhaseId} ({subPhase.Name})";
        var scopeLine = string.IsNullOrWhiteSpace(subPhase.Purpose)
            ? string.Empty
            : $"  Scope: {subPhase.Purpose}";
        var focusLine = subPhase.PredicateIds.Length == 0
            ? string.Empty
            : $"  Focus areas: {string.Join(", ", subPhase.PredicateIds)}";

        return string.Join(Environment.NewLine,
            new[] { promptLine, subPhaseLine, scopeLine, focusLine }
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    // Translate when sub-phase metadata is unavailable — fall back to parsing the
    // KNN output string for a sub-phase id and name.
    public static string TranslateFromKnnOutput(
        string knnOutput,
        string originalPrompt)
    {
        if (string.IsNullOrWhiteSpace(knnOutput))
            return string.IsNullOrWhiteSpace(originalPrompt)
                ? "Capability identified."
                : $"Capability identified for: \"{originalPrompt.Trim()}\"";

        return string.IsNullOrWhiteSpace(originalPrompt)
            ? knnOutput
            : $"For your request: \"{originalPrompt.Trim()}\"" +
              Environment.NewLine + knnOutput;
    }
}
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1257, zero failures.

---

## Phase 2 — Wire CapabilityResultTranslator into GeneratedCapabilityFlow

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

`GeneratedCapabilityFlow` currently has no reference to the topology registry and
cannot resolve sub-phase metadata. Add the topology registry and original prompt as
constructor parameters so the flow can perform back-translation.

Add two new fields and update the constructor:

```csharp
private readonly ILogicalTopologyRegistry? _topology;
private readonly string _originalPrompt;
```

Update `GeneratedCapabilityFlowFactory.Create` to pass both when constructing
`GeneratedCapabilityFlow`. The factory already receives a `LogicalFlowFactoryRequest`
which carries the topology registry and metadata. The original prompt is in the
metadata under the existing `"source.user_request"` key or falls back to empty string.

In `ExecuteAsync`, after the KNN unit returns its result, replace the direct use of
`unitResult.Value.Output` as `FinalHumanResult` with the translated form:

```csharp
// Resolve sub-phase metadata for back-translation.
string humanResult;
if (_topology is not null)
{
    var subPhaseEntry = _topology.ResolveSubPhase(_subPhaseId);
    humanResult = subPhaseEntry.IsSuccess
        ? CapabilityResultTranslator.Translate(
            unitResult.Value.Output,
            subPhaseEntry.Value!.SubPhase,
            _originalPrompt)
        : CapabilityResultTranslator.TranslateFromKnnOutput(
            unitResult.Value.Output,
            _originalPrompt);
}
else
{
    humanResult = CapabilityResultTranslator.TranslateFromKnnOutput(
        unitResult.Value.Output,
        _originalPrompt);
}

var result = new LogicalFlowResult(
    request.FlowId,
    request.ScenarioId,
    "PASS",
    humanResult,          // translated, not raw KNN output
    summary,
    metrics,
    evidenceIds,
    unitResult.Value.Warnings);
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1257, zero failures.

---

## Phase 3 — Deeper model config: 4 layers, hidden dimension 32

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

Replace `BuildTinyConfig` with the deeper configuration:

```csharp
private static ModelConfig BuildTinyConfig()
{
    // 4 layers at hidden dim 32 gives the model enough depth to compose
    // predicate patterns across multiple attention rounds.
    // Total parameters: ~69,920 (273 KB per checkpoint, float32).
    // LoRA-trainable parameters: 2,048 across 4 layers.
    // DPO cost: approximately 4-6x slower per step than the 1-layer config.
    return new ModelConfig
    {
        VocabSize    = 512,
        ContextLen   = 64,
        HiddenDim    = 32,
        NumLayers    = 4,
        NumHeads     = 4,
        NumKvHeads   = 2,
        FfnHiddenDim = 64
    };
}
```

The vocabulary size and context length are unchanged. The existing tokenizer training
and DPO pipeline consume the same input shapes — only the model's internal computation
deepens.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1257, zero failures.
No new test needed here — the model config change is covered by the integration test
in Phase 5 which runs a full train-infer round trip and verifies the output shape.

---

## Phase 4 — Pass original prompt through to GeneratedCapabilityFlow

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

`GeneratedCapabilityFlowFactory` needs the original developer prompt to pass to
`GeneratedCapabilityFlow` at construction time. The prompt is available at training
time in `request.OriginalRequest.UserRequest`. Store it in the manifest and retrieve
it at flow construction time.

Add `OriginalPrompt` to `GeneratedCapabilityCheckpointManifest`:

```csharp
public string OriginalPrompt { get; init; } = string.Empty;
```

Populate it in `BuildGeneratedCapabilityManifest`:

```csharp
OriginalPrompt: request.OriginalRequest.UserRequest?.Trim() ?? string.Empty
```

In `GeneratedCapabilityFlowFactory.Create`, extract the original prompt from the
manifest when constructing the flow:

```csharp
return OperationResult<ILogicalFlow<string>>.Ok(new GeneratedCapabilityFlow(
    _phaseId, _subPhaseId, _taskType, _domainTags, _predicateIds,
    _unitDomainTag, learnedUnit.Value!,
    request.Topology,          // topology for sub-phase metadata lookup
    _originalPrompt));         // stored at training time
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1257, zero failures.

---

## Phase 5 — Tests: back-translation correctness and deeper model round-trip

### File

```
Tests/CapabilityResultTranslatorAndDeeperModelTests.cs  (new file)
```

### Test 1 — Translate produces sub-phase name and purpose in output

```
Given: subPhase with SubPhaseId="3.4", Name="Security and Authorization Model",
         Purpose="Owns security model, authentication, authorization...",
         PredicateIds=["SECURITY","AUTHORIZATION"]
       originalPrompt = "fix the auth check in the tenant service"
       knnOutput = "Generated capability for sub-phase 3.4: ..."
When:  CapabilityResultTranslator.Translate is called
Then:  result contains "3.4"
       result contains "Security and Authorization Model"
       result contains "SECURITY"
       result contains "AUTHORIZATION"
       result contains "fix the auth check in the tenant service"
       result does NOT contain "Generated capability"
         (the machine routing label is replaced, not echoed)
```

### Test 2 — Translate fallback when topology unavailable

```
Given: knnOutput = "Generated capability for sub-phase 4.1: Backend Services..."
       originalPrompt = "add the user service endpoint"
When:  CapabilityResultTranslator.TranslateFromKnnOutput is called
Then:  result contains "add the user service endpoint"
       result is not empty
```

### Test 3 — GeneratedCapabilityFlow returns translated result, not raw KNN output

```
Given: a GeneratedCapabilityFlow constructed with a topology registry
         that contains sub-phase 4.1 with known Purpose and PredicateIds
       originalPrompt = "implement the backend service"
When:  ExecuteAsync is called
Then:  LogicalFlowResult.FinalHumanResult contains "Backend Services"
       LogicalFlowResult.FinalHumanResult contains "implement the backend service"
       LogicalFlowResult.FinalHumanResult does NOT start with "Generated capability"
       LogicalFlowResult.OverallVerdict is "PASS"
```

### Test 4 — Deeper model config has correct shape

```
Given: BuildTinyConfig() is called
Then:  config.NumLayers = 4
       config.HiddenDim = 32
       config.NumHeads = 4
       config.NumKvHeads = 2
       config.FfnHiddenDim = 64
       config.VocabSize = 512
       config.ContextLen = 64
```

### Test 5 — Deeper model can complete a full DPO step without error

```
Given: a DpoTrainer constructed with two LlmModel instances using BuildTinyConfig
       a DpoBatch with ChosenIds and RejectedIds of length 64 tokens
When:  DpoTrainer.Step is called
Then:  the returned DpoMetrics has Step = 1
       Loss is a finite float (not NaN, not infinite)
       Accuracy is 0.0 or 1.0
       the policy checkpoint file is written to the checkpoint directory
```

### Test 6 — LoRA adapter count is 8 at 4 layers (2 adapters per layer × 4 layers)

```
Given: a LlmModel using BuildTinyConfig (4 layers)
       LoraConfig with Rank=2, Alpha=4, TargetModules=["q_proj","v_proj"]
When:  LoraApplicator.Apply is called
Then:  the returned adapter list has exactly 8 entries (2 per layer × 4 layers)
       all base model parameters have RequiresGrad = false
       adapter parameters have finite float values (no NaN initialization)
```

### Test 7 — OriginalPrompt is stored in manifest and retrieved at flow construction

```
Given: a TrainAndRegisterAsync call with OriginalRequest.UserRequest = "add auth guard"
When:  the manifest is written to disk and reloaded
Then:  manifest.OriginalPrompt = "add auth guard"
```

### Test 8 — Full round-trip with deeper model and back-translation

```
Given: a raw prompt "implement the authorization guard for the tenant controller"
       compiled form has DevelopmentPredicateIds = ["SECURITY","AUTHORIZATION","IMPLEMENT"]
       sub-phase 3.4 in the topology with full Purpose text
When:  TrainAndRegisterAsync completes (deeper model, compiled input)
       then GeneratedCapabilityFlow.ExecuteAsync is called with the same prompt
Then:  FinalHumanResult contains "Security and Authorization Model"
       FinalHumanResult contains "implement the authorization guard"
       FinalHumanResult does NOT contain "Generated capability for sub-phase"
       OverallVerdict is "PASS"
       metrics["sub_phase_id"] = "3.4"
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.
(+8 new fact methods)

---

## Execution order

Phases 1 through 4 in sequence, each gated on the suite staying green at 1257.
Phase 5 adds the eight tests and raises the floor to 1265.

This plan must execute after PLAN-DPC-INTEGRATION-v1 is complete. The back-translation
and deeper model work assumes the compiled prompt form is already flowing through the
training pipeline correctly.

---

## What the checkpoint directory looks like after both plans are complete

Every checkpoint written after both plans are implemented will have:

- `policy.bin` — weights from a 4-layer, hidden-32 model
- `tokenizer.json` — vocabulary seeded from canonical development predicate ids
- `generated_capability_manifest.json` — with `OriginalPrompt` field populated

Manifests from the prior batch-20 run (1-layer, raw-prompt vocabulary) remain on disk
but must not be used as continued-learning parents for the corrected pipeline. The
next run must start from a fresh checkpoint root.

---

## Parameter comparison

| Config | Total params | LoRA-trainable | Disk per checkpoint | Layers | Hidden |
|---|---:|---:|---|---|---|
| Before (1L H=16) | 18,736 | 256 | 73 KB | 1 | 16 |
| After (4L H=32) | 69,920 | 2,048 | 273 KB | 4 | 32 |

---

## Floor summary

| After | Floor |
|---|---:|
| Before Phase 1 | 1257 |
| After Phase 1 | 1257 |
| After Phase 2 | 1257 |
| After Phase 3 | 1257 |
| After Phase 4 | 1257 |
| After Phase 5 | 1265 |
