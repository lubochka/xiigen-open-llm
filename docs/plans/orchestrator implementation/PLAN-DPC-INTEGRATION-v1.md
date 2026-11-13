# Plan: DevelopmentPromptCompiler Integration into the Teaching Pipeline
# Version: 1
# Date: 2026-05-07
# Addresses: raw prompt fed to tokenizer and training text instead of compiled predicate form
# Floor before: 1251
# Floor after:  1257

---

## The problem — two separate failures stacked on the same root cause

The developer prompt travels through the system in two distinct phases. During
routing, the orchestrator compiles it through `DevelopmentPromptCompiler` via
`PromptRoutingInputCompilerUnit`, which produces a `DevelopmentPromptCompilation`
containing typed development predicate ids (SECURITY, IMPLEMENT, API_CONTRACT, etc.),
normalized text, pipeline predicates, domain classification, and stack language. That
compiled form is what the sub-phase router uses to make its decision.

When the knowledge-edge branches arrive at `DefaultLogicalCapabilityTrainer`, none of
that compiled form is passed in. `TrainAndRegisterAsync` receives
`request.OriginalRequest.UserRequest` — the raw string typed by the developer — and
`BuildTrainingText` concatenates that raw string with branch metadata. The tokenizer
is then trained on that concatenation.

This creates two failures:

**Failure 1 — the tokenizer vocabulary is wrong.** `BuildTokenizer` is called with
`trainingText`, which is a string built from surface words of the developer's message.
The BPE vocabulary learned from this string is different for every capability chain
because every developer uses different words. The model cannot learn stable cross-domain
patterns because the same predicate concept — for example, "authorization" — maps to a
different token id in each training session depending on the vocabulary of the specific
prompt that happened to seed it.

**Failure 2 — the training text carries surface noise instead of semantic signal.** The
chosen side of the preference pair is a fixed template string about sub-phase execution.
The prompt side is raw natural language. The DPO step is trying to teach the model
to associate "fix this Angular null ref in the service" with sub-phase 4.1, rather
than associating "IMPLEMENT BUG stack:typescript sub-phase:4.1" with sub-phase 4.1.
The router uses the latter form. The trained model was never shown it.

**Failure 3 — the runner's originalRequest UserRequest is not even the real prompt.**
The calibration runner constructs a synthetic `originalRequest` with a UserRequest
string like `"XIIGen documentation teaching; batch_size=20; batch=1; domain=BUG;
rows=7; branches=43."`. The individual row prompts from the training set are in the
`branches` (the `KnowledgeEdgeBranches` array), not in `UserRequest`. The trainer
reads `UserRequest` to build training text, so the training text for every domain
group in every batch has been built from this synthetic batch-description string, not
from the actual developer prompts.

All three failures must be fixed together in a single plan because they interact: the
right fix for Failure 3 changes what is passed to `BuildTrainingText`, and the right
fix for Failures 1 and 2 changes what `BuildTrainingText` does with that input.

---

## What the correct flow must look like

```
Developer prompt (raw)
        │
        ▼
DevelopmentPromptCompiler.Compile(prompt)
        │
        ├── DevelopmentPredicateIds  ← stable canonical vocabulary
        ├── NormalizedText           ← collapsed, stemmed form
        ├── PipelinePredicates       ← structural predicate ids from project 29
        ├── Domain                   ← classified domain
        └── StackLanguage            ← detected technology stack
        │
        ▼
BuildTrainingText receives the COMPILED form, not the raw string
        │
        ├── Prompt side of DPO pair: predicate ids + normalized text + domain + stack
        └── Teaching vector: built from compiled predicates, not raw surface words
        │
        ▼
BuildTokenizer receives the COMPILED vocabulary
        │
        └── Vocabulary seeded from canonical predicate ids and normalized roots —
            stable across sessions for the same domain
        │
        ▼
DpoTrainer: model learns to associate compiled predicate form with sub-phase
        │
        ▼
At inference time: incoming prompt is compiled the same way before KNN lookup
        └── Training form and inference form now match
```

---

## Phase 1 — Add compiled prompt to LogicalCapabilityTrainingRequest

### Files changed

```
21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs
21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs
```

### What changes in LogicalCapabilityTrainingContracts.cs

Add one optional field to `LogicalCapabilityTrainingRequest` that carries the already-
compiled form of the original request when the orchestrator has it available:

```csharp
// CompiledPrompt carries the DevelopmentPromptCompiler output for the original request.
// When present, BuildTrainingText uses this instead of re-compiling from UserRequest.
// When null, BuildTrainingText compiles on demand.
public SemanticCompiler29.DevelopmentPromptCompilation? CompiledPrompt { get; init; }
```

This field is optional with a null default so the contract change is backward
compatible. All existing callers continue to work without modification.

### What changes in AdaptiveLogicalOrchestrator.cs

The orchestrator already has the compiled document from `CompilePromptAsync`. Pass it
through to the training request so the trainer does not need to re-compile:

```csharp
// compiled is the LogicalUnitResult<SubPhaseRouterSourceDocument> from CompilePromptAsync
var training = await _capabilityTrainer.TrainAndRegisterAsync(
    new LogicalCapabilityTrainingRequest(
        request.RunId,
        request,
        knowledgeEdges,
        _topology!,
        _flows!,
        _units!)
    {
        Domain = trainingDomain,
        CompiledPrompt = compiledPromptCompilation  // NEW
    }, cancellationToken);
```

The orchestrator already holds the `DevelopmentPromptCompilation` from when it called
`PromptRoutingInputCompilerUnit`. Extract it from the result and pass it here.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1251, zero failures.
No new test needed — this is additive field addition with null default.

---

## Phase 2 — Fix BuildTrainingText to use the compiled form

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

`BuildTrainingText` currently takes `string userRequest`. Change it to take
`DevelopmentPromptCompilation? compiled` and `string userRequest` (fallback).

When `compiled` is not null, build the training text from the compiled form:

```csharp
private static string BuildTrainingText(
    SemanticCompiler29.DevelopmentPromptCompilation? compiled,
    string userRequest,
    LogicalFlowResult branch,
    LogicalSubPhase subPhase)
{
    // Use compiled predicate form when available. Fall back to raw prompt when not.
    // The compiled form is what the router uses — training and routing must share
    // the same input representation so the trained model sees the same vocabulary
    // at inference time as it saw during training.
    string promptSide;
    if (compiled is not null)
    {
        promptSide = string.Join(" ",
            string.Join(" ", compiled.DevelopmentPredicateIds),
            compiled.NormalizedText,
            string.Join(" ", compiled.PipelinePredicates),
            $"semantic_domain:{compiled.Domain}",
            $"semantic_stack:{compiled.StackLanguage}");
    }
    else
    {
        // Compile on demand when the caller did not pre-compile.
        var onDemand = new SemanticCompiler29.DevelopmentPromptCompiler().Compile(userRequest);
        promptSide = onDemand.IsSuccess
            ? string.Join(" ",
                string.Join(" ", onDemand.Value!.DevelopmentPredicateIds),
                onDemand.Value.NormalizedText,
                string.Join(" ", onDemand.Value.PipelinePredicates),
                $"semantic_domain:{onDemand.Value.Domain}",
                $"semantic_stack:{onDemand.Value.StackLanguage}")
            : userRequest?.Trim() ?? string.Empty;
    }

    var raw = string.Join(" ",
        promptSide,
        branch.FinalHumanResult,
        subPhase.SubPhaseId,
        subPhase.Name,
        subPhase.Purpose,
        string.Join(" ", subPhase.PredicateIds));

    // Teaching vector is built from the compiled prompt side, not raw surface text.
    var vectorSource = compiled is not null
        ? string.Join(" ", compiled.DevelopmentPredicateIds, compiled.NormalizedText)
        : userRequest ?? string.Empty;
    var vector = TrainingVectorBuilder.Build(vectorSource);
    var vectorText = string.Join(",",
        vector.Select(v => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)));

    // Keep LogicalTeachingTextCompressor on the raw prompt side for backward compatibility
    // in the manifest TrainingText field (used for replay lookup).
    var compressed = LogicalTeachingTextCompressor.CompressForTeaching(
        string.Join(" ", userRequest?.Trim() ?? string.Empty, promptSide));

    return $"{raw} semantic_compressed: {compressed} teaching_vector_60: {vectorText}".Trim();
}
```

Update the single call site inside `TrainAndRegisterAsync` to pass
`request.CompiledPrompt`:

```csharp
var trainingText = BuildTrainingText(
    request.CompiledPrompt,
    request.OriginalRequest.UserRequest,
    branch,
    subPhase.Value!.SubPhase);
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1251, zero failures.

---

## Phase 3 — Fix BuildTokenizer to use compiled vocabulary

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

The tokenizer must be trained on stable canonical vocabulary, not on raw surface words.
The canonical vocabulary for the development domain is the set of predicate ids plus
normalized roots from the compiled form, combined with the fixed seed words.

Change `BuildTokenizer` to accept the compilation result:

```csharp
private static BpeTokenizer BuildTokenizer(
    SemanticCompiler29.DevelopmentPromptCompilation? compiled,
    string trainingText)
{
    // Build vocabulary from canonical predicate ids and normalized text when available.
    // This makes the token id for "IMPLEMENT" stable across all capability chains in
    // the same domain, rather than varying with each developer's surface vocabulary.
    var vocabText = compiled is not null
        ? string.Join(" ",
            string.Join(" ", compiled.DevelopmentPredicateIds),
            compiled.NormalizedText,
            string.Join(" ", compiled.PipelinePredicates),
            trainingText,
            "capability generated learned flow selected rejected " +
            "SECURITY AUTHORIZATION API_CONTRACT ARCHITECTURE IMPLEMENT TEST QA UI DOCUMENT LEARNING")
        : $"{trainingText} capability generated learned flow selected rejected";

    var tokenizer = new BpeTokenizer();
    tokenizer.Train(vocabText, 300);
    return tokenizer;
}
```

The seed vocabulary now always includes the full development predicate lexicon,
guaranteeing that the predicate ids occupy stable positions in the vocabulary
regardless of which developer prompt happened to be the first training example in a
capability chain.

Update the call site in `BuildTeachingStack` (the fresh-tokenizer path):

```csharp
BuildTokenizer(request.CompiledPrompt, trainingText)
```

This requires passing `request.CompiledPrompt` down through `BuildTeachingStack`. Add
it as an optional parameter with null default.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1251, zero failures.

---

## Phase 4 — Fix the runner: pass actual row prompts as UserRequest

### File changed

```
experiments-v2/XIIGEN-DOCS-BATCH-COMPARISON/runner/XiigenDocsBatchComparisonRunner/Program.cs
```

### What changes

The runner currently builds a synthetic `originalRequest` with a batch-description
string as `UserRequest`. The trainer reads `UserRequest` to build training text. The
fix is to build `originalRequest` from the actual concatenated prompts of the rows
in the domain group, so the trainer sees real developer language as fallback even
before the compiled form is injected via Phase 1.

```csharp
// OLD — synthetic batch description
var originalRequest = new LogicalOrchestratorRequest(
    groupId,
    AdaptiveLogicalOrchestrator.ScenarioId,
    $"XIIGen documentation teaching; batch_size={batchSize}; batch={batchNumber}; ...",
    flags,
    metadata);

// NEW — actual prompts from the domain group rows, joined as a representative prompt
var representativePrompt = string.Join(" | ",
    groupRows
        .Select(row => row.UserRequest?.Trim() ?? string.Empty)
        .Where(p => !string.IsNullOrWhiteSpace(p))
        .Distinct(StringComparer.Ordinal)
        .Take(5));  // cap at 5 to keep context length bounded

var originalRequest = new LogicalOrchestratorRequest(
    groupId,
    AdaptiveLogicalOrchestrator.ScenarioId,
    string.IsNullOrWhiteSpace(representativePrompt)
        ? $"domain:{domain} batch:{batchNumber}"
        : representativePrompt,
    flags,
    metadata);
```

Separately, compile the representative prompt through `DevelopmentPromptCompiler`
and attach it to the training request as `CompiledPrompt`:

```csharp
var compiler = new SemanticCompiler29.DevelopmentPromptCompiler();
var compiledPrompt = compiler.Compile(originalRequest.UserRequest);

var training = await trainer.TrainAndRegisterAsync(
    new LogicalCapabilityTrainingRequest(groupId, originalRequest, branches, topology, flows, units)
    {
        Domain = domain,
        CompiledPrompt = compiledPrompt.IsSuccess ? compiledPrompt.Value : null
    });
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1251, zero failures.

---

## Phase 5 — Fix the inference side: compile the incoming prompt before KNN lookup

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

`GeneratedCapabilityFlow.ExecuteAsync` feeds `request.Input` — which is the raw
developer prompt string — directly into `_unit.ExecuteAsync`. The KNN unit then embeds
that raw string to find nearest neighbors. But the KNN index was built from compiled
vocabulary (after Phase 2 and 3). The training form and the query form must match.

Add a compilation step before the KNN lookup:

```csharp
public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
    LogicalFlowRequest<string> request,
    CancellationToken cancellationToken = default)
{
    // ...existing validation...

    // Compile the incoming prompt to the same canonical form used during training.
    // The KNN index was built from compiled predicate vocabulary, so the query must
    // use the same representation for distances to be meaningful.
    var compiler = new SemanticCompiler29.DevelopmentPromptCompiler();
    var compiled = compiler.Compile(request.Input);
    var queryText = compiled.IsSuccess
        ? string.Join(" ",
            string.Join(" ", compiled.Value!.DevelopmentPredicateIds),
            compiled.Value.NormalizedText,
            string.Join(" ", compiled.Value.PipelinePredicates),
            $"semantic_domain:{compiled.Value.Domain}",
            $"semantic_stack:{compiled.Value.StackLanguage}")
        : request.Input;

    var unitResult = await _unit.ExecuteAsync(new LogicalUnitRequest<string>(
        request.RunId,
        DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
        DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
        _unitDomainTag,
        queryText,  // compiled form, not raw
        request.Flags,
        request.Metadata), cancellationToken);

    // ...rest of method unchanged...
}
```

`DevelopmentPromptCompiler` is stateless — constructing it per call is safe until
Phase 6 caches it. The compile call is fast (keyword scan + normalizer).

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1251, zero failures.

---

## Phase 6 — New tests: compilation contract and round-trip consistency

### File

```
Tests/DevelopmentPromptCompilerTeachingIntegrationTests.cs  (new file)
```

### Test 1 — BuildTrainingText uses compiled predicates, not raw surface words

```
Given: a raw prompt "fix the null ref in the Angular service"
       compiled form has DevelopmentPredicateIds = ["IMPLEMENT", "TEST"]
       NormalizedText = "null ref angular service"
When:  BuildTrainingText is called with the compiled form
Then:  the returned training text contains "IMPLEMENT" and "TEST"
       the returned training text does NOT contain "null" as a standalone surface word
         without a predicate label (i.e. the predicate form dominates the surface form)
       the teaching vector is built from the predicate ids, not the raw prompt
```

### Test 2 — BuildTokenizer vocabulary contains canonical predicate ids

```
Given: a compiled form with DevelopmentPredicateIds = ["IMPLEMENT", "TEST"]
When:  BuildTokenizer is called with the compiled form
Then:  the tokenizer can encode the string "IMPLEMENT" to a non-unknown token id
       the tokenizer can encode the string "TEST" to a non-unknown token id
       both ids are stable when BuildTokenizer is called again with a different
         compiled form that also contains ["IMPLEMENT", "TEST"]
```

### Test 3 — Training form and inference form are consistent

```
Given: a raw prompt "fix the null ref in the Angular service"
       a GeneratedCapabilityFlow trained on the compiled form of this prompt
When:  ExecuteAsync is called with the same raw prompt at inference time
Then:  the query text passed to the KNN unit contains "IMPLEMENT"
       the query text is NOT the raw string unmodified
```

### Test 4 — On-demand compilation fallback works when CompiledPrompt is null

```
Given: a LogicalCapabilityTrainingRequest with CompiledPrompt = null
       UserRequest = "add authentication to the controller"
When:  TrainAndRegisterAsync is called
Then:  the training text contains "SECURITY" or "AUTHORIZATION"
         (compiled on demand from UserRequest)
       the training completes without error
       a manifest is written to the checkpoint directory
```

### Test 5 — Runner representative prompt uses real row text

```
Given: a domain group of 3 rows with UserRequest values
         "fix the null ref", "add auth guard", "update the schema"
When:  the runner builds originalRequest for the group
Then:  originalRequest.UserRequest contains text from at least one of the three rows
       originalRequest.UserRequest does NOT contain "XIIGen documentation teaching"
```

### Test 6 — Full round-trip: compile → train → infer → output makes sense

```
Given: a raw prompt "fix the authorization check in the tenant service"
       compiled form has DevelopmentPredicateIds = ["SECURITY", "AUTHORIZATION", "IMPLEMENT"]
       a sub-phase topology with sub-phase 3.4 (Security and Authorization Model)
When:  one full TrainAndRegisterAsync call completes for this prompt
       then GeneratedCapabilityFlow.ExecuteAsync is called with the same prompt
Then:  the flow result OverallVerdict is "PASS"
       the flow result metrics contain sub_phase_id = "3.4"
       the FinalHumanResult contains "3.4" or "Security" or "Authorization"
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1257, zero failures.
(+6 new fact methods, each verifying one specific guarantee)

---

## Execution order

Phases 1 through 5 in sequence, each gated on the suite staying green at 1251.
Phase 6 adds the six tests. All phases complete before the next calibration run.

The existing 317 checkpoint manifests from the batch-20 run were trained without
this fix. They are not usable as a continued-learning baseline for the corrected
training path because their tokenizer vocabularies were built from raw surface text.
The next calibration run starts fresh in a new checkpoint directory.

---

## Floor summary

| After | Floor |
|---|---:|
| Before Phase 1 | 1251 |
| After Phase 1 | 1251 |
| After Phase 2 | 1251 |
| After Phase 3 | 1251 |
| After Phase 4 | 1251 |
| After Phase 5 | 1251 |
| After Phase 6 | 1257 |
