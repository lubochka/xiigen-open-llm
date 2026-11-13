# Plan: Compiled Prompt as the Training Text Foundation
# Version: 1
# Date: 2026-05-07
# Floor before: 1265
# Floor after:  1271

---

## The gap

The orchestrator already compiles the raw prompt into a structured form via
`PromptRoutingInputCompilerUnit` before routing. That form contains everything
the KNN matrix needs: normalized roots, development predicate ids, pipeline
predicates, domain, stack language. It is exactly what routing decisions are based on.

When a knowledge edge is detected and `RunWithCapabilityGenerationAsync` hands
the prompt to the trainer, it passes `request.OriginalRequest.UserRequest` — the
raw string — and the compiled form is discarded. `BuildTrainingText` then tries to
re-derive signal from the raw text using `LogicalTeachingTextCompressor`, which
calls the Roslyn C# AST parser (`SemanticTokenizerPipeline.Level3Tokens`) on an
English natural language sentence. That parser fails, the fallback tokenizer splits
by whitespace and drops short words, and the result fed to the KNN matrix is a
sorted bag of surface words — no predicates, no stack, no structure.

The training text and the routing text must be the same form. They are not.

---

## What changes

### Principle

The compiled text that routing uses is the text that training must use. The
compiled form is already available in the orchestrator at the point where the
training request is created — it just isn't passed through. This plan threads it
through.

### The compiled form that matters

`PromptRoutingInputCompilerUnit.BuildCompiledText` produces:

```
{raw text} {normalized roots} {PIPELINE_PREDICATE_1} {PIPELINE_PREDICATE_2}
semantic_domain:BUG semantic_stack:typescript
```

Example for `"fix the rxjs pipe in the Angular interceptor"`:
```
fix the rxjs pipe in the Angular interceptor fix rxjs pipe angular intercept
CALL_OP AWAIT_OP semantic_domain:BUG semantic_stack:typescript
```

This is what the router scores against sub-phase predicates. This is what the
KNN matrix must store and retrieve against.

---

## Phase 1 — Add CompiledText to LogicalCapabilityTrainingRequest

**File:** `21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs`

Add one optional field to the training request. Null means not available — the
trainer falls back to compiling from `UserRequest` on demand.

```csharp
public sealed record LogicalCapabilityTrainingRequest(
    string RunId,
    LogicalOrchestratorRequest OriginalRequest,
    LogicalFlowResult[] KnowledgeEdgeBranches,
    ILogicalTopologyRegistry Topology,
    InMemoryLogicalFlowRegistry Flows,
    InMemoryLogicalUnitRegistry Units)
{
    public Domain Domain { get; init; } = Domain.CODE_Q;

    // The compiled text produced by PromptRoutingInputCompilerUnit for this request.
    // When present, BuildTrainingText uses this instead of re-deriving from UserRequest.
    // Format: "{normalized roots} {pipeline predicates} semantic_domain:{d} semantic_stack:{s}"
    public string? CompiledText { get; init; }
}
```

This field is additive with a null default. All existing callers continue to work.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 2 — Pass compiled text from orchestrator to trainer

**File:** `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`

`RunWithCapabilityGenerationAsync` calls `RunAsync` first, which internally calls
`CompilePromptAsync`. The compiled result is consumed by routing but not kept on
the method's stack. Change `RunAsync` to return the compiled text alongside the
orchestrator result, or capture it in a field scoped to the request.

The simplest correct approach: extract the compiled text from `RunAsync` by
running `DevelopmentPromptCompiler.Compile` directly at the call site where the
training request is built. The compiler is stateless and fast (keyword scan).

```csharp
// In RunWithCapabilityGenerationAsync, before creating the training request:
var compiler = new SemanticCompiler29.DevelopmentPromptCompiler();
var compiledForTraining = compiler.Compile(request.UserRequest);
var compiledText = compiledForTraining.IsSuccess
    ? PromptRoutingInputCompilerUnit.BuildCompiledTextPublic(compiledForTraining.Value!)
    : null;

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
        CompiledText = compiledText   // ← NEW
    }, cancellationToken);
```

This requires making `BuildCompiledText` public (or internal + InternalsVisibleTo)
on `PromptRoutingInputCompilerUnit` so the orchestrator can call the same method
that routing uses. Rename to `BuildCompiledTextPublic` or just add `public static`.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 3 — Replace BuildTrainingText body

**File:** `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`

Replace the current `BuildTrainingText` signature and body:

**Current:**
```csharp
private static string BuildTrainingText(
    string userRequest,
    LogicalFlowResult branch,
    LogicalSubPhase subPhase)
```

**New:**
```csharp
private static string BuildTrainingText(
    string? compiledText,
    string userRequest,
    LogicalFlowResult branch,
    LogicalSubPhase subPhase)
```

**New body:**

```csharp
// Use the compiled form when available — this is what routing used and what
// the KNN matrix must store. Fall back to on-demand compilation, then to
// the raw request as a last resort.
string promptSide;
if (!string.IsNullOrWhiteSpace(compiledText))
{
    promptSide = compiledText;
}
else
{
    var compiler = new SemanticCompiler29.DevelopmentPromptCompiler();
    var onDemand = compiler.Compile(userRequest ?? string.Empty);
    promptSide = onDemand.IsSuccess
        ? PromptRoutingInputCompilerUnit.BuildCompiledTextPublic(onDemand.Value!)
        : userRequest?.Trim() ?? string.Empty;
}

var raw = string.Join(" ",
    promptSide,
    branch.FinalHumanResult,
    subPhase.SubPhaseId,
    subPhase.Name,
    subPhase.Purpose,
    string.Join(" ", subPhase.PredicateIds));

var vector = TrainingVectorBuilder.Build(promptSide);
var vectorText = string.Join(",",
    vector.Select(v => v.ToString("0.###",
        System.Globalization.CultureInfo.InvariantCulture)));

return $"{raw} teaching_vector_60: {vectorText}".Trim();
```

Note: `LogicalTeachingTextCompressor.CompressForTeaching` is removed from the
return value. The compiled text already is the compressed form — running the
Roslyn parser again on top of it is redundant and actively harmful (it parses
English text as C# source).

Update the call site to pass `request.CompiledText`:

```csharp
var trainingText = BuildTrainingText(
    request.CompiledText,      // ← NEW: compiled form from orchestrator
    request.OriginalRequest.UserRequest,
    branch,
    subPhase.Value!.SubPhase);
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 4 — Fix BuildTokenizer

**File:** `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`

`BuildTokenizer` currently trains on `trainingText` — the full concatenation
including `branch.FinalHumanResult`, sub-phase name, purpose, etc. The tokenizer
vocabulary should be seeded from the prompt-side signal only, so the same predicate
tokens (`IMPLEMENT`, `CALL_OP`, `semantic_stack:typescript`) always map to the same
token ids across capability chains in the same domain.

Add the development predicate lexicon as a fixed seed vocabulary so the token ids
for canonical predicates are stable regardless of which developer's surface words
happened to appear first:

```csharp
private static BpeTokenizer BuildTokenizer(string compiledText, string trainingText)
{
    // Seed vocabulary from compiled text + canonical predicate terms.
    // This ensures IMPLEMENT, CALL_OP, semantic_stack:typescript occupy the
    // same token ids across all capability chains in the same domain.
    var seedVocab = string.Join(" ",
        compiledText,
        trainingText,
        "SECURITY AUTHORIZATION API_CONTRACT ARCHITECTURE IMPLEMENT TEST QA UI DOCUMENT LEARNING",
        "NULL_CHECK RANGE_CHECK ASSIGN_OP RETURN_OP LOOP_OP BRANCH_OP CALL_OP AWAIT_OP",
        "THROW_OP CATCH_OP VALIDATE_OP CREATE_OP UPDATE_OP DELETE_OP GET_OP SEND_OP",
        "semantic_domain:BUG semantic_domain:PLAN semantic_domain:ARCH semantic_domain:CODE_Q",
        "semantic_stack:csharp semantic_stack:typescript semantic_stack:python semantic_stack:unknown");

    var tokenizer = new BpeTokenizer();
    tokenizer.Train(seedVocab, 512);
    return tokenizer;
}
```

Update call site to pass `compiledText` alongside `trainingText`.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 5 — Tests

**New file:** `Tests/CompiledPromptTrainingTests.cs`

### Test 1 — BuildTrainingText uses compiled form, not raw text

```
Given: compiledText = "fix rxjs pipe angular CALL_OP AWAIT_OP semantic_domain:BUG semantic_stack:typescript"
       userRequest = "fix the rxjs pipe in the Angular interceptor service"
       a branch and sub-phase (4.1 Backend Services)
When:  BuildTrainingText called
Then:  result contains "CALL_OP"
       result contains "semantic_stack:typescript"
       result does NOT start with "fix the rxjs pipe in the Angular interceptor service"
         (raw surface text is not the primary signal)
```

### Test 2 — On-demand fallback compiles when CompiledText is null

```
Given: CompiledText = null
       UserRequest = "fix the authorization check in the tenant service"
When:  BuildTrainingText called
Then:  result contains "semantic_domain"
       result contains "AUTH_OP" or "semantic_stack" or a predicate token
       result does not throw
```

### Test 3 — Token ids for canonical predicates are stable across two calls

```
Given: compiledText_1 = "fix rxjs pipe CALL_OP semantic_stack:typescript"
       compiledText_2 = "add auth guard AUTH_OP semantic_stack:typescript"
When:  BuildTokenizer called twice (once per compiled text)
Then:  tokenizer_1.Encode("CALL_OP") produces a non-zero token id
       tokenizer_2.Encode("AUTH_OP") produces a non-zero token id
       both tokenizers can encode "semantic_stack:typescript" without unknown tokens
```

### Test 4 — Two prompts with different stacks produce different training texts

```
Given: prompt_ts = "fix the rxjs observable in the Angular service"
       prompt_cs = "fix the null ref in the C# async task"
When:  both compiled and passed to BuildTrainingText
Then:  ts_training != cs_training
       ts_training contains "semantic_stack:typescript"
       cs_training contains "semantic_stack:csharp"
```

### Test 5 — CompiledText field on training request survives the contract

```
Given: LogicalCapabilityTrainingRequest with CompiledText = "IMPLEMENT TEST semantic_stack:csharp"
When:  request is constructed and the field is read back
Then:  request.CompiledText == "IMPLEMENT TEST semantic_stack:csharp"
       other fields (RunId, Domain, etc.) are unaffected
```

### Test 6 — Orchestrator passes compiled text to training request

```
Given: AdaptiveLogicalOrchestrator with a trainer that records the training request
       request with UserRequest = "fix the null ref in the service"
       a flow registry with no registered flow (forces knowledge edge)
When:  RunWithCapabilityGenerationAsync called
Then:  the captured training request has CompiledText != null
       the CompiledText contains "semantic_domain" or a predicate token
       the CompiledText does NOT equal "fix the null ref in the service"
         (raw prompt is not passed as compiled text)
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1271, zero failures.
(+6 new tests)

---

## What this fixes

| Before | After |
|---|---|
| `BuildTrainingText` receives raw `"fix the rxjs pipe in the Angular interceptor"` | Receives `"fix rxjs pipe angular CALL_OP AWAIT_OP semantic_domain:BUG semantic_stack:typescript"` |
| `CompressForTeaching` runs Roslyn C# AST parser on English sentence | Removed — compiled text is already the compressed form |
| Tokenizer vocabulary seeded from surface words of one developer's prompt | Seeded from canonical predicate lexicon — stable across all chains |
| `TrainingVectorBuilder.Build(userRequest)` — raw surface words → 60-slot vector | `TrainingVectorBuilder.Build(compiledText)` — predicate tokens → 60-slot vector |
| KNN matrix embeds surface vocabulary | KNN matrix embeds predicate + stack vocabulary |
| Two TypeScript prompts on different sub-phases may embed closer to C# prompts | TypeScript embeddings cluster by stack; routing and training share the same space |

---

## Relation to PLAN-STACK-COMPILATION-PATTERNS-v1

This plan creates the correct foundation. PLAN-STACK-COMPILATION-PATTERNS-v1
extends `BuildCompiledText` in `PromptRoutingInputCompilerUnit` with
`StackCompilationPatternProvider` — adding `rxjs_pipe observable_stream injectable`
to the compiled form for TypeScript prompts. Once this plan is in place,
those enriched tokens automatically flow into training because training uses
the same compiled form. The two plans are sequential: this plan first,
then stack patterns on top.

---

## Floor summary

| After | Floor |
|---|---:|
| Start | 1265 |
| Phases 1–4 (no new tests) | 1265 |
| Phase 5 | 1271 |
