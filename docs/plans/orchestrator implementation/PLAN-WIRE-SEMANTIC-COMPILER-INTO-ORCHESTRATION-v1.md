# Plan — Wire SemanticCompilerFacade into the Prompt Compiler and Domain Classifier
# Version: 1.0 — 2026-05-06
# Session type: EXECUTOR
# Test floor entering this plan: 1224 (run live before touching any file)
# Latest commit: 0c8d349

---

## What this plan does and does not change

This plan wires the existing `SemanticCompilerFacade` (project 12) into the two places
in project 21 where its output is currently absent:

1. `PromptRoutingInputCompilerUnit` — currently calls its own raw keyword lexicon on the
   raw prompt. This plan adds a call to `SemanticCompilerFacade.Compile(prompt)` first,
   passes the normalised text to the existing predicate extractor, and propagates the
   domain label from the compilation result into the outgoing `SubPhaseRouterSourceDocument`.

2. `PromptDomainClassifier` — currently is a self-contained static class with its own
   keyword profiles. This plan upgrades it to delegate to
   `RouteClassifier.ClassifyByKeywords` from project 12, which is the canonical
   implementation confirmed by experiment E18 at 95% accuracy.

This plan does **not**:
- Change `MultiSubPhaseRouterUnit` or any routing logic
- Change `AdaptiveLogicalOrchestrator`
- Change `DefaultLogicalCapabilityTrainer`
- Add new interfaces or new projects
- Change the `ILogicalUnit` contract for the compiler unit
- Alter the `SubPhaseRoutingDecision` shape

The `SemanticCompilerFacade` already exists and is fully functional. Project 21 already
references project 12 in its `.csproj`. No new dependencies are introduced.

---

## Baseline verification — Gate A (run before touching any file)

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: 1224 passing, 0 failing. If different, stop and report before proceeding.

---

## Phase 1 — Add domain label to SubPhaseRouterSourceDocument

**File:** `21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs`

**Current shape:**

```csharp
public sealed record SubPhaseRouterSourceDocument(
    string DocumentId,
    string SourcePath,
    string SourceTier,
    string LicenseOrAuthority,
    string DocumentType,
    string DocumentTitle,
    string CompiledText,
    string[] CompiledPredicateIds,
    string SourceStatus,
    string KnownFailureMode);
```

**Target shape — add one field at the end:**

```csharp
public sealed record SubPhaseRouterSourceDocument(
    string DocumentId,
    string SourcePath,
    string SourceTier,
    string LicenseOrAuthority,
    string DocumentType,
    string DocumentTitle,
    string CompiledText,
    string[] CompiledPredicateIds,
    string SourceStatus,
    string KnownFailureMode,
    string DomainLabel);   // ← NEW: D-04 domain from PromptDomainClassifier
```

**Rationale:** The domain label is produced in the compiler step and must travel with
the source document so the orchestrator and trainer do not need to re-classify. The
field is a plain string to match the existing metadata dictionary pattern and avoid
a circular dependency on the `DomainLabel` enum across namespaces.

**Constructor call sites to update (grep before editing):**

```bash
grep -rn "new SubPhaseRouterSourceDocument(" C:\Projects\llm_mvp_core\ --include="*.cs"
```

Every call site must pass the domain label string as the final argument. For any site
that does not have a domain label available, pass `string.Empty`.

Gate B command for Phase 1:

```bash
dotnet build 21.AgentInfrastructure/AgentInfrastructure.csproj 2>&1 | grep -E "error|warning" | head -20
```

Expected: zero errors. Fix all errors before proceeding.

---

## Phase 2 — Upgrade PromptDomainClassifier to delegate to RouteClassifier

**File:** `21.AgentInfrastructure/Logical/PromptDomainClassifier.cs`

**Current behaviour:** `Classify(string prompt)` uses a self-contained `DomainProfile[]`
array with its own keyword sets. This duplicates the canonical keyword logic already
in `12.SemanticCompiler/Routing/RouteClassifier.cs`.

**Target behaviour:** `Classify(string prompt)` delegates to
`RouteClassifier.ClassifyByKeywords(prompt)` from project 12, then maps the result back
to a `PromptDomainClassification` record. The confidence and evidence terms are derived
using the same scoring logic as today, but the domain decision comes from the canonical
classifier.

**Exact change — replace the body of the `Classify` method:**

```csharp
public static PromptDomainClassification Classify(string prompt)
{
    // Delegate domain label to the canonical RouteClassifier (E18, 95% accuracy).
    var domainLabel = RouteClassifier.ClassifyByKeywords(prompt ?? string.Empty);

    // Derive evidence terms from the winning domain's keyword list for audit trail.
    var normalized = (prompt ?? string.Empty).ToLowerInvariant();
    var profile = Profiles.FirstOrDefault(p => p.Domain == domainLabel);
    var evidence = profile?.Terms
        .Where(term => normalized.Contains(term, StringComparison.Ordinal))
        .Distinct(StringComparer.Ordinal)
        .ToArray() ?? [];

    // Confidence is proportion of matched terms to total matched terms across all profiles.
    var total = Math.Max(1, Profiles.Sum(p =>
        p.Terms.Count(term => normalized.Contains(term, StringComparison.Ordinal))));
    var confidence = evidence.Length == 0
        ? 0.2
        : Math.Min(0.99, Math.Max(0.2, (double)evidence.Length / total));

    return new PromptDomainClassification(domainLabel, confidence, evidence);
}
```

The `Profiles` array stays in place — it is still used to derive evidence terms for the
audit trail. Only the domain selection decision moves to `RouteClassifier`.

**Using directive to add at the top of the file:**

```csharp
using SemanticCompiler;
```

Gate B command for Phase 2:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~PromptRoutingInputCompilerUnit" 2>&1 | tail -10
```

Expected: all 7 existing compiler tests pass.

---

## Phase 3 — Wire SemanticCompilerFacade into PromptRoutingInputCompilerUnit

**File:** `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs`

**Current behaviour:** `ExecuteAsync` calls `ExtractPredicates(prompt)` directly on the
raw prompt text using the hardcoded `PredicateLexicon`. The `SemanticCompilerFacade`
translation pipeline (E25 pattern classification, E26 contextual disambiguation, E27
word reordering) is never called.

**Target behaviour:** `ExecuteAsync` calls `SemanticCompilerFacade.Compile(prompt)` first.
If compilation succeeds, it passes the normalised text (from `CompilationResult.NormalizedWords`
joined as a space-separated string) to `ExtractPredicates`. The domain label from
`CompilationResult.Domain` is forwarded into `PromptDomainClassifier.Classify` to
produce the `PromptDomainClassification` used to set the `DomainLabel` field on the
outgoing `SubPhaseRouterSourceDocument`.

If `SemanticCompilerFacade.Compile` fails (empty source, etc.), fall back to the
existing raw prompt path — this preserves the current behaviour as a safe fallback and
ensures no regression for any existing test input.

**Constructor change — inject SemanticCompilerFacade:**

```csharp
private readonly SemanticCompilerFacade _semanticCompiler;

public PromptRoutingInputCompilerUnit(SemanticCompilerFacade? semanticCompiler = null)
{
    // Default construction with no arguments keeps all existing test call sites valid.
    _semanticCompiler = semanticCompiler ?? new SemanticCompilerFacade();
}
```

**ExecuteAsync change — add facade call before predicate extraction:**

```csharp
// Attempt semantic compilation to get normalised text and domain label.
var compilationResult = _semanticCompiler.Compile(request.Input.UserRequest);
var textForPredicates = compilationResult.IsSuccess
    ? string.Join(" ", compilationResult.Value!.NormalizedWords)
    : request.Input.UserRequest;

// Determine domain label: prefer compiled result, fall back to direct classification.
var domainClassification = compilationResult.IsSuccess
    ? new PromptDomainClassification(compilationResult.Value!.Domain, 0.95, [])
    : PromptDomainClassifier.Classify(request.Input.UserRequest);

// Extract predicate evidence from the normalised text.
var predicateIds = ExtractPredicates(textForPredicates);
```

**SubPhaseRouterSourceDocument construction change — add domain label:**

```csharp
var sourceDocument = new SubPhaseRouterSourceDocument(
    documentId,
    "logical-orchestrator://user-prompt",
    "runtime_prompt",
    "user_request",
    "user_prompt",
    "Runtime user prompt",
    request.Input.UserRequest.Trim(),
    predicateIds,
    "ready",
    predicateIds.Length == 0 ? "knowledge_edge: no high-signal development predicate found" : "runtime_prompt_compiled",
    domainClassification.Domain.ToString());  // ← NEW
```

**Using directive to add:**

```csharp
using SemanticCompiler;
```

Gate B command for Phase 3:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~PromptRoutingInputCompilerUnit" 2>&1 | tail -10
```

Expected: all 7 existing compiler tests pass. The normalised text may change predicate
coverage for some tests — check carefully. If any test now fails because normalisation
changes the token set, update the test to reflect the corrected behaviour, documenting
why in the test comment. Do not silence failures.

---

## Phase 4 — Add new tests for the wired path

**File:** `Tests/PromptRoutingInputCompilerUnitTests.cs`

Add the following four tests. Each is a new `[Fact]`. Do not modify existing tests.

**Test 1 — Domain label flows through to SubPhaseRouterSourceDocument:**

```csharp
[Fact]
public async Task PromptRoutingInputCompilerUnit_BugPrompt_DomainLabelIsBug()
{
    // The domain label on the compiled document must match the D-04 classification.
    var request = BuildRequest("domain-label-bug-test",
        "Fix the null reference exception in the Figma export flow.");
    var result = await new PromptRoutingInputCompilerUnit().ExecuteAsync(
        new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));
    Assert.True(result.IsSuccess, result.ErrorMessage);
    Assert.Equal("BUG", result.Value!.Output.DomainLabel);
}
```

**Test 2 — Normalised text from semantic compiler reaches predicate extraction:**

```csharp
[Fact]
public async Task PromptRoutingInputCompilerUnit_NormalisedTextReachesPredicateExtraction()
{
    // After semantic compilation, predicate extraction runs on normalised tokens.
    // A prompt with architecture vocabulary must still emit the ARCHITECTURE predicate.
    var request = BuildRequest("normalised-predicate-test",
        "Design the domain adapter topology and registry structure.");
    var result = await new PromptRoutingInputCompilerUnit().ExecuteAsync(
        new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));
    Assert.True(result.IsSuccess, result.ErrorMessage);
    Assert.Contains("ARCHITECTURE", result.Value!.Output.CompiledPredicateIds);
}
```

**Test 3 — SemanticCompiler failure falls back to raw path without throwing:**

```csharp
[Fact]
public async Task PromptRoutingInputCompilerUnit_EmptySemanticCompilerResult_FallsBackToRawPath()
{
    // If semantic compilation returns failure (e.g. empty input guard triggered
    // on a very short prompt), the unit must still succeed using the raw path.
    var request = BuildRequest("fallback-raw-path-test", "bug");
    var result = await new PromptRoutingInputCompilerUnit().ExecuteAsync(
        new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));
    // Unit must succeed — the fallback path must not surface a failure result.
    Assert.True(result.IsSuccess, result.ErrorMessage);
}
```

**Test 4 — PromptDomainClassifier delegates to canonical keyword priority:**

```csharp
[Fact]
public void PromptDomainClassifier_BugBeforeRefactorPriority()
{
    // A prompt containing both bug and refactor signals must resolve to BUG
    // because BUG has higher priority than REFACTOR in the D-04 canonical order.
    var result = PromptDomainClassifier.Classify(
        "Fix the broken refactoring flow — it crashes on null input.");
    Assert.Equal(DomainLabel.BUG, result.Domain);
}
```

Gate B command for Phase 4:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~PromptRoutingInputCompilerUnit" 2>&1 | tail -10
```

Expected: 11 tests pass (7 existing + 4 new).

---

## Phase 5 — Full suite verification and commit

Gate B — full suite:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: 1228 passing (1224 prior floor + 4 new tests), 0 failing.

If any test regresses below 1224: stop, identify the failing test, do not commit, and
report the failure and file name.

**Commit message:**

```
wire SemanticCompilerFacade into PromptRoutingInputCompilerUnit and PromptDomainClassifier

- SubPhaseRouterSourceDocument gains DomainLabel field (plain string)
- PromptDomainClassifier.Classify delegates to RouteClassifier.ClassifyByKeywords (E18)
- PromptRoutingInputCompilerUnit calls SemanticCompilerFacade.Compile first;
  normalised words feed predicate extraction; domain label forwarded to source document
- Fallback path preserved: if facade compile fails, unit uses raw prompt
- 4 new tests; floor raises from 1224 to 1228
```

---

## Execution order and stop conditions

Execute phases in order: 1, 2, 3, 4, 5. Do not skip phases or reorder.

Stop and report (do not continue) if:
- Gate A shows a count below 1224 before any change
- Any Gate B build step produces errors
- Any phase causes a test that was passing before the phase to fail
- The full suite after Phase 5 is below 1228

---

## What changes and what does not

| Component | Change |
|---|---|
| `SubPhaseRouterSourceDocument` | New `DomainLabel` string field added at end |
| `PromptDomainClassifier` | `Classify` delegates domain decision to `RouteClassifier.ClassifyByKeywords` |
| `PromptRoutingInputCompilerUnit` | Constructor accepts injected facade; `ExecuteAsync` calls facade before predicates; domain label propagated to output document |
| `MultiSubPhaseRouterUnit` | No change |
| `AdaptiveLogicalOrchestrator` | No change |
| `DefaultLogicalCapabilityTrainer` | No change |
| Test file | 4 new tests added; no existing tests modified |
| Project references | No change — project 21 already references project 12 |

---

# Plan version: 1.0
# Author: ARCHITECT session 2026-05-06
# Gate counts: Gate A (1 run), Gate B per phase (5 runs), total 6 gate commands
