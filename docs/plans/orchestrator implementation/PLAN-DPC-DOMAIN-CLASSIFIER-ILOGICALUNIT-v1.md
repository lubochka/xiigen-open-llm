# Plan: DPC Domain Classifier and ILogicalUnit Promotion
# Version: 1
# Date: 2026-05-07
# Addresses: A61 — ClassifyDomainAsync uses raw keyword scan, not compiled predicates;
#            RouteClassifier.Train() never called in production;
#            DomainClassifierUnit and AttributedSegmentRouter are plain classes, not ILogicalUnits;
#            every prompt pipeline step must be extensible through the registry
# Depends on: PLAN-DPC-INTEGRATION-v1 complete (A55 fixed)
# Floor before: 1265
# Floor after:  1273

---

## The problems

### Problem 1 — Compile runs after classify; the compiler result is discarded for classification

The orchestrator's `RunAsync` calls `ClassifyDomainAsync` first, then `CompilePromptAsync`.
The compiled `DevelopmentPromptCompilation` — which already contains typed predicate ids,
normalised text, domain classification, and stack language — is produced and used for
routing, but the domain classifier never sees it. `ClassifyDomainAsync` has already run
its own independent keyword scan on the raw prompt string and moved on.

The result is that the system computes the same semantic content twice from the same
raw text using two different keyword vocabularies, with no shared state. The router
uses the richer compiled form; the classifier uses the weaker raw scan.

### Problem 2 — RouteClassifier.Train() is never called in production

`DomainClassifierUnit` holds a `SemanticCompiler.RouteClassifier` instance whose
`Train()` method loads a KNN from labeled `(int[] predicates, Domain)` examples.
`DomainClassifierUnit.ClassifyBaseDomain` takes the KNN path only when `_trained`
is true and the request carries non-empty `Predicates`. Neither condition is ever
true in production:

- `_trained` is false because nothing calls `DomainClassifierUnit.Train()`.
- `request.Predicates` is null because `ClassifyDomainAsync` passes only the raw
  prompt string, never the compiled predicate ids.

Every classification in production falls through to `ClassifyByKeywords` — the static
keyword scan. The E18 KNN classifier, which was proven to achieve +15.2% domain
accuracy, has never executed in production.

### Problem 3 — DomainClassifierUnit and AttributedSegmentRouter are plain classes

`DomainClassifierUnit` in project 23 is a plain service class. It has constants `Id`,
`Role`, and `DomainTag`, but it does not implement `ILogicalUnit`. The unit registry
cannot hold it; a trained replacement cannot be registered; the DPO loop cannot
improve it; it cannot be extended by domain.

`AttributedSegmentRouter` in project 23 is the same — plain class with no registry
presence. It is used by `PromptRoutingInputCompilerUnit` to split multi-segment prompts,
but a learned router replacement cannot be substituted.

### Problem 4 — InternalLlmGateway holds a plain DomainClassifierUnit

`InternalLlmGateway` constructs a `DomainClassifierUnit` directly as a field. It is
not resolved from the registry, never trained, and permanently frozen at keyword scan.

### Problem 5 — Training domain fallback uses raw classification

When the orchestrator cannot read the domain from the first run result, it falls back
to `PromptDomainClassifier.Classify(request.UserRequest).Domain` — a raw keyword scan
on the prompt string. At this point in the execution the compiled form is available
in the request metadata. It should be read from there instead.

---

## What the correct flow must look like after this plan

```
RunAsync(request)
  │
  ▼
Phase 0 — Compile ONCE
  PromptRoutingInputCompilerUnit (ILogicalUnit, registry-resolvable)
  → DevelopmentPromptCompilation
    { DevelopmentPredicateIds, PipelinePredicates, NormalizedText, Domain, StackLanguage }
  │
  ▼
Phase 1 — Classify from compiled form
  CompiledDomainClassifierUnit (NEW ILogicalUnit, registry-resolvable)
  input:  DevelopmentPromptCompilation
  output: PromptDomainClassification
  uses:   DevelopmentPredicateIds → bridge to int[] → RouteClassifier KNN
  fallback: ClassifyByKeywords on NormalizedText (not raw prompt)
  │
  ▼
Phase 2 — Route from compiled form (unchanged)
  MultiSubPhaseRouterUnit (already receives compiled SubPhaseRouterSourceDocument)
  │
  ▼
Phase 3 — Attribute segments from compiled form
  CompiledAttributedSegmentUnit (NEW ILogicalUnit, registry-resolvable)
  input:  DevelopmentPromptCompilation
  output: AttributedSegmentDocument[]
  │
  ▼
Phase 4 — DPO teaching loop
  After each teaching step: collect (compiled predicate ids → domain) pair
  Feed to RouteClassifier.Train() so classifier improves from feedback
```

---

## Phase 1 — Swap orchestrator order: compile first, classify from compiled

### File changed

```
21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs
```

### What changes

Move `CompilePromptAsync` before `ClassifyDomainAsync`. Pass the compiled result to
the classification step so the classifier receives the predicate form, not the raw string.

```csharp
// OLD order
var domainClassification = await ClassifyDomainAsync(request, flags, ct);
// ...
var compiled = await CompilePromptAsync(request, flags, ct);

// NEW order
var compiled = await CompilePromptAsync(request, flags, ct);
if (!compiled.IsSuccess)
    return OperationResult<LogicalOrchestratorResult>.Fail(...);

var domainClassification = await ClassifyDomainAsync(request, flags, compiled.Value!.Output, ct);
```

`ClassifyDomainAsync` gains an optional `SubPhaseRouterSourceDocument? compiledDoc`
parameter. When present it passes the compiled predicate ids and normalised text into
`DomainClassifierRequest` instead of the raw prompt.

Also fix the training domain fallback (line 173): read the domain label already stored
in request metadata by `AddToMetadata` rather than calling raw `Classify` again.

```csharp
// OLD
: PromptDomainClassifier.Classify(request.UserRequest).Domain;

// NEW
: TryReadDomainFromMetadata(request.Metadata, out var metaDomain)
    ? metaDomain
    : PromptDomainClassifier.Classify(request.UserRequest).Domain;
```

Also fix the transition classification in `FollowTransitionSignalsAsync` — it calls
`ClassifyDomainAsync` with no compiled doc. Compile the transition prompt first.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 2 — Promote DomainClassifierUnit to ILogicalUnit; wire KNN from compiled predicates

### Files changed

```
23.SemanticCompilerAdapter/DomainClassifierUnit.cs
21.AgentInfrastructure/Logical/PromptDomainClassifier.cs
```

### What changes in DomainClassifierUnit

`DomainClassifierUnit` becomes `ILogicalUnit<DomainClassifierRequest, DomainClassification>`.
This makes it directly registry-resolvable — a trained replacement can be registered
at role `"domain_classifier"` without the `PromptDomainClassifierUnit` wrapper.

```csharp
// Before: plain class
public sealed class DomainClassifierUnit

// After: ILogicalUnit
public sealed class DomainClassifierUnit
    : ILogicalUnit<DomainClassifierRequest, DomainClassification>
```

Add `ExecuteAsync` that wraps the existing `Classify` call.

Add a string predicate bridge method:

```csharp
// Bridge: DPC PipelinePredicates (string[]) → int[] for RouteClassifier KNN
// Uses the 22-slot canonical vocab from project 29's LogicExtractor.PredicateKeys
public static int[] BridgePredicates(string[] pipelinePredicates)
{
    var result = new List<int>();
    foreach (var name in pipelinePredicates)
    {
        var idx = Array.IndexOf(PredicateKeys, name);
        if (idx >= 0) result.Add(idx);
    }
    return result.ToArray();
}
```

`PredicateKeys` is the 22-element array from project 29's `LogicExtractor` — the
same names in the same order, so the int indices are stable.

`ClassifyBaseDomain` now uses the bridge:

```csharp
private Domain ClassifyBaseDomain(DomainClassifierRequest request)
{
    // Compiled predicate ids take priority over raw keyword scan
    if (request.Predicates is { Length: > 0 } intPredicates)
    {
        if (_trained)
        {
            var knn = _classifier.Classify(intPredicates);
            if (knn.IsSuccess) return knn.Value;
        }
        // Untrained but predicates present — use predicate-domain affinity map
        return PredicateAffinityDomain(intPredicates);
    }

    // String predicates from DPC PipelinePredicates — bridge then classify
    if (request.PipelinePredicates is { Length: > 0 } strPredicates)
    {
        var bridged = BridgePredicates(strPredicates);
        if (bridged.Length > 0)
            return PredicateAffinityDomain(bridged);
    }

    return SemanticCompiler.RouteClassifier.ClassifyByKeywords(
        string.IsNullOrWhiteSpace(request.NormalizedText)
            ? request.Prompt
            : request.NormalizedText);
}
```

Add `NormalizedText` and `PipelinePredicates` fields to `DomainClassifierRequest`:

```csharp
public sealed record DomainClassifierRequest(
    string Prompt,
    int[]? Predicates = null,
    string[]? PipelinePredicates = null,    // NEW — from DPC PipelinePredicates
    string? NormalizedText = null,           // NEW — from DPC NormalizedText
    object? PriorContext = null,
    IReadOnlyDictionary<string, object>? Metadata = null);
```

### What changes in PromptDomainClassifier

`PromptDomainClassifierUnit` is kept for backward compatibility but is now just a
thin adapter — it constructs a `DomainClassifierRequest` from the raw string input
and delegates to `DomainClassifierUnit.ExecuteAsync`.

The orchestrator's `ClassifyDomainAsync` resolves `DomainClassifierUnit` directly
from the registry at role `"domain_classifier"` when a compiled doc is available.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 3 — Promote AttributedSegmentRouter to ILogicalUnit

### Files changed

```
23.SemanticCompilerAdapter/AttributedSegmentRouter.cs
21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs
```

### What changes

`AttributedSegmentRouter` becomes
`ILogicalUnit<DevelopmentPromptCompilation, AttributedSegmentDocument[]>`.

Input is the full `DevelopmentPromptCompilation` already produced in Phase 1. The
unit extracts segments from `compiled.Segments`, routes each through `DomainClassifierUnit`,
and returns `AttributedSegmentDocument[]`.

```csharp
public sealed class AttributedSegmentRouter
    : ILogicalUnit<DevelopmentPromptCompilation, AttributedSegmentDocument[]>
{
    public const string Id    = "unit-attributed-segment-router-v1";
    public const string Role  = "attributed-segment-router";
    public const string DomainTag = "whole_development_area";
    // ...
}
```

`PromptRoutingInputCompilerUnit` resolves `AttributedSegmentRouter` from the registry
instead of constructing it directly. A learned replacement can be registered at role
`"attributed-segment-router"`.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 4 — Wire RouteClassifier.Train() into the DPO teaching loop

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

After each successful `TrainAndRegisterAsync` call produces a new capability manifest,
collect a `(int[] predicates, Domain)` pair from the manifest's compiled predicate ids
and the domain tag. Accumulate these pairs across the session and call
`DomainClassifierUnit.Train()` on the current session's unit instance.

```csharp
// After dpoResult succeeds, per manifest:
var bridgedPredicates = DomainClassifierUnit.BridgePredicates(
    manifest.PredicateIds);         // string[] from DPC PipelinePredicates in manifest
var domain = Enum.Parse<Domain>(manifest.DomainTag, ignoreCase: true);
_domainTrainingExamples.Add((bridgedPredicates, domain));

// After all branches trained, update the classifier:
if (_domainTrainingExamples.Count > 0)
    _domainClassifierUnit.Train(_domainTrainingExamples);
```

`_domainClassifierUnit` is the same `DomainClassifierUnit` instance that the registry
resolves in `ClassifyDomainAsync` — they must be the same object. The orchestrator
passes its registry into the trainer request (already done via `request.Units`); the
trainer resolves the active domain classifier unit and trains it.

This means: every time the DPO loop teaches a domain group, the domain classifier
gets richer training data. After 200 rows, it has seen real `(predicates, domain)`
pairs from actual developer prompts and no longer relies solely on keyword scan.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 5 — Fix InternalLlmGateway

### File changed

```
21.AgentInfrastructure/Loop/InternalLlmGateway.cs
```

### What changes

Replace the directly constructed `DomainClassifierUnit` field with resolution from
the unit registry. `InternalLlmGateway` receives the `InMemoryLogicalUnitRegistry`
and resolves at role `"domain_classifier"`. Where no registry is available the
existing fallback construction is kept as a last resort.

Compile the prompt through `DevelopmentPromptCompiler` before classifying, same as
the orchestrator now does after Phase 1.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 6 — New tests

### File

```
Tests/DpcDomainClassifierILogicalUnitTests.cs  (new file)
```

### Test 1 — Compile runs before classify; classifier receives compiled predicates

```
Given: an orchestrator request with prompt "fix the auth check in the tenant service"
When:  RunAsync executes
Then:  the compiled doc is produced before ClassifyDomainAsync is called
       DomainClassifierRequest.PipelinePredicates is non-empty
       DomainClassifierRequest.NormalizedText is the compiled normalised form
       DomainClassifierRequest.Prompt is NOT the raw developer string
```

### Test 2 — BridgePredicates maps string names to correct int indices

```
Given: string[] { "NULL_CHECK", "AUTH_OP", "CREATE_OP" }
When:  DomainClassifierUnit.BridgePredicates is called
Then:  result contains 0  (NULL_CHECK index in PredicateKeys)
       result contains 19 (AUTH_OP index)
       result contains 12 (CREATE_OP index)
       result has exactly 3 elements
```

### Test 3 — DomainClassifierUnit is ILogicalUnit and resolves from registry

```
Given: an InMemoryLogicalUnitRegistry with DomainClassifierUnit registered
When:  registry.ResolveLatest<DomainClassifierRequest, DomainClassification>(
           "domain_classifier", "whole_development_area", Probationary) is called
Then:  the resolved unit is a DomainClassifierUnit
       ExecuteAsync returns a valid DomainClassification
```

### Test 4 — Trained KNN path is used when training data is present

```
Given: a DomainClassifierUnit trained with 10 (int[], Domain) examples
       a DomainClassifierRequest with Predicates = [0, 19] (NULL_CHECK, AUTH_OP)
When:  Classify is called
Then:  ClassifyBaseDomain uses the KNN path (_trained = true, predicates present)
       result domain is determined by KNN vote, not ClassifyByKeywords
       evidence contains "knn_trained" not "semantic_domain_keyword_fallback"
```

### Test 5 — AttributedSegmentRouter is ILogicalUnit and resolves from registry

```
Given: an InMemoryLogicalUnitRegistry with AttributedSegmentRouter registered
       a DevelopmentPromptCompilation with 2 segments
When:  ExecuteAsync is called
Then:  returns AttributedSegmentDocument[] with 2 entries
       each entry has a non-UNKNOWN domain
       the unit resolves from registry at role "attributed-segment-router"
```

### Test 6 — RouteClassifier.Train is called after DPO teaching step

```
Given: a DefaultLogicalCapabilityTrainer with a DomainClassifierUnit reference
       a teaching run that produces 3 manifests across BUG and PLAN domains
When:  TrainAndRegisterAsync completes
Then:  DomainClassifierUnit._trained is true
       DomainClassifierUnit._classifier contains examples for BUG and PLAN
       subsequent Classify calls use the KNN path, not ClassifyByKeywords
```

### Test 7 — Training domain fallback reads from metadata, not raw Classify

```
Given: an orchestrator request where TryReadDomainFromResult returns false
       domain label already present in request metadata as "BUG"
When:  the capability trainer's domain is determined
Then:  domain is BUG (read from metadata)
       PromptDomainClassifier.Classify(UserRequest) is NOT called
```

### Test 8 — InternalLlmGateway resolves DomainClassifierUnit from registry

```
Given: an InternalLlmGateway constructed with a registry containing a trained unit
When:  ClassifyWithCanonicalEvidence is called with a prompt
Then:  the trained unit is used, not a fresh DomainClassifierUnit()
       the compiled predicate form is passed, not the raw string
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1273, zero failures.
(+8 new fact methods)

---

## Execution order

Phases 1 through 5 in sequence, each gated on the suite staying green at 1265.
Phase 6 adds the eight tests and raises the floor to 1273.

---

## What is NOT in this plan

E18 and E51A experiments: their `TextToPredicates` local methods remain in the
experiment files. They are diagnostic history — their results are internally
consistent with what was in production at the time they were written. They do not
need to be re-run; the production path is now corrected. If new experiments need
domain classification, they must call `DomainClassifierUnit.ExecuteAsync` with a
compiled request, not a local keyword scan.

E24 experiment: the `FullPrompt` path in the experiment is diagnostic history.
The production `AttributedSegmentRouter` now takes `DevelopmentPromptCompilation`
directly. No change to the experiment file is needed.

---

## Floor summary

| After | Floor |
|---|---:|
| Before Phase 1 | 1265 |
| After Phase 1 | 1265 |
| After Phase 2 | 1265 |
| After Phase 3 | 1265 |
| After Phase 4 | 1265 |
| After Phase 5 | 1265 |
| After Phase 6 | 1273 |
